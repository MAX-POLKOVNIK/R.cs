using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using R.cs.Core.ProjectItemsProcessors;

namespace R.cs.Core
{
    internal sealed class ControllerGenerator : ISourceCodeGenerator
    {
        private static readonly string[] SupportedViewControllersTypes =
        {
            "viewController",
            "tableViewController",
            "glkViewController",
            "avPlayerViewController",
            "pageViewController",
            "pageViewController",
            "splitViewController",
            "tabBarController",
            "collectionViewController"
        };

        private readonly string[] _storyboardsPaths;
        private readonly string _projectPath;

        public ControllerGenerator(string projectPath, StoryboardsProcessor storyboardsProcessor)
        {
            _projectPath = projectPath ?? throw new ArgumentNullException(nameof(projectPath));

            _storyboardsPaths = storyboardsProcessor.StoryboardPaths
                .Select(x => Path.Combine(Directory.GetParent(projectPath).ToString(), x))
                .ToArray();
        }

        public string GenerateSourceCode()
        {
            var registeredTouchClasses = GetRegisteredTouchClasses().Result;
            var mappedViewControllers = GetMappedViewControllers(registeredTouchClasses);

            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("public static class ViewController");
            stringBuilder.AppendLine("{");

            foreach (var (storyboardIdentifier, viewControllerCsharpClass, storyboard) in mappedViewControllers)
            {
                stringBuilder.AppendLine($"public static {viewControllerCsharpClass} {storyboardIdentifier}(Foundation.NSBundle bundle = null) " +
                                         $"=> ({viewControllerCsharpClass}) UIKit.UIStoryboard.FromName(\"{storyboard}\", bundle).InstantiateViewController(\"{storyboardIdentifier}\");");
            }

            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }

        private (string storyboardIdentifier, string viewControllerCsharpClass, string storyboard)[] GetMappedViewControllers(IDictionary<string, string> registeredTouchClasses)
        {
            var list = new List<(string referencedIdentifier, string viewControllerCsharpClass, string storyboard)>();

            foreach (var storyboardsPath in _storyboardsPaths)
            {
                var xdoc = XDocument.Load(storyboardsPath);

                var scenesElement = xdoc.Descendants()
                    .First(x => x.Name == "document")
                    .Descendants()
                    .FirstOrDefault(x => x.Name == "scenes");

                if (scenesElement == null)
                    continue;

                var l = scenesElement.Descendants()
                    .Where(x => x.Name == "scene")
                    .Descendants()
                    .Where(x => x.Name == "objects")
                    .Descendants()
                    .ToArray();
                    
                var viewControllers = l.Where(x => SupportedViewControllersTypes.Any(y => y == x.Name.ToString()));

                foreach (var viewController in viewControllers)
                {
                    var referencedIdentifier = viewController.Attributes().FirstOrDefault(x => x.Name == "storyboardIdentifier");
                    if (referencedIdentifier == null)
                        continue;

                    var customClass = viewController.Attributes().FirstOrDefault(x => x.Name == "customClass");
                    if (customClass == null)
                        continue;

                    list.Add((referencedIdentifier.Value, registeredTouchClasses[customClass.Value], Path.GetFileNameWithoutExtension(storyboardsPath)));
                }
            }

            return list.ToArray();
        }

        private async Task<IDictionary<string, string>> GetRegisteredTouchClasses()
        {
            var msWorkspace = MSBuildWorkspace.Create();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var project = await msWorkspace.OpenProjectAsync(_projectPath);

            var registeredTouchClasses = new Dictionary<string, string>();

            foreach (var document in project.Documents)
            {
                var classes = (await document.GetSyntaxRootAsync())
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Where(x => x.AttributeLists.Count > 0)
                    .Where(x => x.AttributeLists.Any(z => z.Attributes.Any(y => y.Name.ToString() == "Register")))
                    .Where(x => x.Parent is NamespaceDeclarationSyntax)
                    .ToArray();

                foreach (var classDeclarationSyntax in classes)
                {
                    var className = classDeclarationSyntax.Identifier.Text;
                    var namespaceName = ((NamespaceDeclarationSyntax)classDeclarationSyntax.Parent).Name;
                    var registeredName = GetRegisterAttributeValue(classDeclarationSyntax.AttributeLists);

                    registeredTouchClasses.Add(registeredName.Trim('"'), $"{namespaceName}.{className}");
                }
            }

            stopwatch.Stop();

            string GetRegisterAttributeValue(SyntaxList<AttributeListSyntax> clAttributeLists)
            {
                foreach (var attributeListSyntax in clAttributeLists)
                {
                    foreach (var attributeSyntax in attributeListSyntax.Attributes)
                    {
                        if (attributeSyntax.Name.ToString() == "Register")
                        {
                            return attributeSyntax.ArgumentList.Arguments.First().ToString();
                        }
                    }
                }

                throw new Exception("Can't retreive [Register] attribute value");
            }

            return registeredTouchClasses;
        }
    }
}
