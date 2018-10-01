using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Evaluation;

namespace R.cs.Core
{
    public class Generator
    {
        private static readonly string GeneratedFileDescription = $@"// This file generated with R.cs v.{typeof(Generator).Assembly.GetName().Version}";

        private static readonly string PathToRcs = Path.Combine("Resources", "R.cs");

        private readonly char[] _symbolsToRemove = {'-', '.'};

        private static readonly char Separator = IsRunningOnMono() ? '/' : '\\';

        private const string RootBundleDirectoryName = "R.cs_root";

        public class BundleDirectory
        {
            public string Name { get; set; }
            public IDictionary<string, string> BundleResources { get; set; }
            public BundleDirectory[] BundleSubDirectories { get; set; }
        }
        
        public string Do(string path, string rootNamespace)
        {
            var project = ProjectCollection.GlobalProjectCollection.GetLoadedProjects(path).FirstOrDefault() 
                ?? new Project(path);

            IDictionary<string, string> images;
            IDictionary<string, string> colors;
            IDictionary<string, string> storyboards;
            IDictionary<string, string> xibs;

            try
            {
                images = project.AllEvaluatedItems
                    .Where(x => x.ItemType == "ImageAsset")
                    .Select(x => x.EvaluatedInclude.Split(Separator))
                    .Where(x => x.Any(y => y.Contains(".imageset")))
                    .GroupBy(x => x.First(y => y.Contains(".imageset")))
                    .Select(x => Path.GetFileNameWithoutExtension(x.Key))
                    .ToDictionary(GetCorrectConstName, x => x);
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating assets", ex);
            }

            try
            {
                colors = project.AllEvaluatedItems
                    .Where(x => x.ItemType == "ImageAsset")
                    .Select(x => x.EvaluatedInclude.Split(Separator))
                    .Where(x => x.Any(y => y.Contains(".colorset")))
                    .GroupBy(x => x.First(y => y.Contains(".colorset")))
                    .Select(x => Path.GetFileNameWithoutExtension(x.Key))
                    .ToDictionary(GetCorrectConstName, x => x);
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating colors", ex);
            }

            try
            {
                storyboards = project.AllEvaluatedItems
                    .Where(x => x.ItemType == "InterfaceDefinition")
                    .Select(x => x.EvaluatedInclude.Split(Separator))
                    .Select(x => x.Last())
                    .Where(x => Path.GetExtension(x) == ".storyboard")
                    .Select(Path.GetFileNameWithoutExtension)
                    .ToDictionary(GetCorrectConstName, x => x);
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating storyboards", ex);
            }

            try
            {
                xibs = project.AllEvaluatedItems
                    .Where(x => x.ItemType == "InterfaceDefinition")
                    .Select(x => x.EvaluatedInclude.Split(Separator))
                    .Select(x => x.Last())
                    .Where(x => Path.GetExtension(x) == ".xib")
                    .Select(Path.GetFileNameWithoutExtension)
                    .ToDictionary(GetCorrectConstName, x => x);
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating xibs", ex);
            }

            var rootBundleDirectories = project.AllEvaluatedItems
                .Where(x => x.ItemType == "BundleResource")
                .Select(x => new KeyValuePair<string[], string>(x.EvaluatedInclude.Split(Separator), x.EvaluatedInclude))
                .GroupBy(x => x.Key.FirstOrDefault())
                .Select(ToBundleDirectory)
                .ToArray();

            var rootBundle = rootBundleDirectories.FirstOrDefault(x => x.Name == "Resources") 
                ?? new BundleDirectory
            {
                Name = "Resources",
                BundleResources = new Dictionary<string, string>(),
                BundleSubDirectories = new BundleDirectory[0]
            };

            BundleDirectory ToBundleDirectory(IGrouping<string, KeyValuePair<string[], string>> directory)
            {
                var name = directory.Key;

                var items = directory.Select(x => new KeyValuePair<string[], string>(x.Key.Skip(1).ToArray(), x.Value)).ToArray();

                IDictionary<string, string> resources;

                try
                {
                    resources = items.Where(x => x.Key.Length == 1)
                        .Select(x => new KeyValuePair<string, string>(Path.GetFileNameWithoutExtension(x.Key.First()), x.Value))
                        .Select(x =>
                        {
                            var indexOfAt = x.Key.IndexOf("@", StringComparison.InvariantCulture);
                            return indexOfAt > 0
                                ? new KeyValuePair<string, string>(x.Key.Substring(0, indexOfAt), GetCorrectResourceBundleName(x.Value))
                                : new KeyValuePair<string, string>(x.Key, GetCorrectResourceBundleName(x.Value));
                        })
                        .DistinctBy(x => x.Key)
                        .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                        .ToDictionary(x => GetCorrectConstName(x.Key), x => x.Value);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error generating bundle resources", ex);
                }

                var subDirectories = items.Where(x => x.Key.Length > 1)
                    .GroupBy(x => x.Key.FirstOrDefault())
                    .Select(ToBundleDirectory)
                    .ToArray();

                var bundleDirectory = new BundleDirectory
                {
                    Name = GetCorrectConstName(name),
                    BundleResources = resources,
                    BundleSubDirectories = subDirectories
                };

                return bundleDirectory;
            }

            var fileContent = GenerateRcsContent($"{rootNamespace}.Resources", images, colors, rootBundle, storyboards, xibs);

            var resourceClassItem = project.AllEvaluatedItems.FirstOrDefault(x => x.ItemType == "Compile" && x.EvaluatedInclude == PathToRcs);

            if (resourceClassItem == null)
            {
                project.AddItem("Compile", PathToRcs);
                project.Save();
            }

            var fullRcsFilePath = Path.Combine(Directory.GetParent(path).ToString(), PathToRcs);

            File.WriteAllText(fullRcsFilePath, fileContent);

            ProjectCollection.GlobalProjectCollection.UnloadProject(project);

            return fileContent;
        }

        public string GetCorrectConstName(string original)
        {
            if (Regex.IsMatch(original, @"^\d"))
            {
                original = $"_{original}";
            }

            return _symbolsToRemove.Aggregate(original, (current, c) => current.Replace(c, '_'));
        }

        public string GetCorrectResourceBundleName(string original)
        {
            if (original.StartsWith($"Resources{Separator}"))
            {
                original = original.Replace($"Resources{Separator}", "");
            }

            original = new[] { "@1x", "@2x", "@3x" }.Aggregate(original, (current, s1) => current.Replace(s1, ""));

            original = original.Replace(@"\", "/");

            return Path.ChangeExtension(original, null);
        }

        public string GenerateRcsContent(string @namespace, IDictionary<string, string> images, IDictionary<string, string> colors,
            BundleDirectory bundleDirectory, IDictionary<string, string> storyboards, IDictionary<string, string> xibs)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(GeneratedFileDescription);
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine($"namespace {@namespace}");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine("    public static class R");
            stringBuilder.AppendLine("    {");
            AddClass("Image", images);
            AddClass("Color", colors);
            AddClass("Storyboard", storyboards);
            AddClass("Xibs", xibs);
            AddClassForBundleDirectory(bundleDirectory, 8);
            stringBuilder.AppendLine("    }");
            stringBuilder.AppendLine("}");

            void AddClassForBundleDirectory(BundleDirectory bd, int spacesCount)
            {
                stringBuilder.AppendLine($"{new string(' ', spacesCount)}public static class {(bd.Name == RootBundleDirectoryName ? "Bundle" : bd.Name)}");
                stringBuilder.AppendLine($"{new string(' ', spacesCount)}{{");

                foreach (var resource in bd.BundleResources)
                {
                    stringBuilder.AppendLine($"{new string(' ', spacesCount + 4)}public const string {resource.Key} = @\"{resource.Value}\";");
                }

                foreach (var bundleSubDirectory in bd.BundleSubDirectories)
                {
                    AddClassForBundleDirectory(bundleSubDirectory, spacesCount + 4);
                }

                stringBuilder.AppendLine($"{new string(' ', spacesCount)}}}");
            }

            void AddClass(string className, IDictionary<string, string> consts)
            {
                stringBuilder.AppendLine($"        public static class {className}");
                stringBuilder.AppendLine("        {");

                foreach (var @const in consts)
                {
                    stringBuilder.AppendLine($"            public const string {@const.Key} = \"{@const.Value}\";");
                }

                stringBuilder.AppendLine("        }");
            }

            return stringBuilder.ToString();
        }

        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }
    }
}
