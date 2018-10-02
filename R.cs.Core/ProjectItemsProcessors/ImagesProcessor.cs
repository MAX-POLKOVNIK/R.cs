using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Evaluation;

namespace R.cs.Core.ProjectItemsProcessors
{
    internal sealed class ImagesProcessor : IProjectItemProcessor
    {
        internal class BundleDirectory
        {
            public string Name { get; set; }
            public IDictionary<string, string> BundleResources { get; set; } = new Dictionary<string, string>();
            public List<BundleDirectory> BundleSubDirectories { get; set; } = new List<BundleDirectory>();
        }

        // https://developer.apple.com/library/archive/documentation/2DDrawing/Conceptual/DrawingPrintingiOS/LoadingImages/LoadingImages.html#//apple_ref/doc/uid/TP40010156-CH17-SW7
        private static readonly string[] SupportedImagesFormats = {".png", ".tiff", ".tif", ".jpeg", ".jpg", ".gif", ".bmp", ".BMPf", ".ico", ".cur", ".xbm"};

        private const string SupportedBundleResourceProjectItemType = "BundleResource";
        private const string SupportedAssetResourceProjectItemType = "ImageAsset";

        private const string ContentsFileName = "Contents.json";
        private const string AssetSetExtension = ".imageset";

        private const string ResourcesDirectoryName = "Resources";

        private readonly BundleDirectory _rootBundleDirectory = new BundleDirectory
        {
            Name = ResourcesDirectoryName
        };

        public bool Accept(ProjectItem projectItem)
        {
            switch (projectItem.ItemType)
            {
                case SupportedAssetResourceProjectItemType:
                    return AcceptAssetResourceProjectItem(projectItem);
                case SupportedBundleResourceProjectItemType:
                    return AcceptBundleResourceProjectItem(projectItem);
                default:
                    return false;
            }
        }

        private bool AcceptBundleResourceProjectItem(ProjectItem projectItem)
        {
            var extension = Path.GetExtension(projectItem.EvaluatedInclude);

            if (SupportedImagesFormats.All(x => x != extension))
                return false;

            var splittedPath = projectItem.EvaluatedInclude.Split(Path.DirectorySeparatorChar);
            var currentBundleDirectoryCollection = new List<BundleDirectory> {_rootBundleDirectory};
            var destinationDirectory = _rootBundleDirectory;

            if (splittedPath.Length > 3)
            {
                Console.WriteLine();
            }

            while (splittedPath.Length > 1)
            {
                var directory = currentBundleDirectoryCollection.FirstOrDefault(x => x.Name == splittedPath[0]);

                if (directory == null)
                {
                    directory = new BundleDirectory
                    {
                        Name = splittedPath[0]
                    };

                    currentBundleDirectoryCollection.Add(directory);
                }

                destinationDirectory = directory;
                splittedPath = splittedPath.Skip(1).ToArray();
                currentBundleDirectoryCollection = destinationDirectory.BundleSubDirectories;
            }
            
            var key = ValidNamesProvider.GetCorrectConstName(ValidNamesProvider.GetCorrectResourceBundleName(splittedPath[0]));
            var value = ValidNamesProvider.GetCorrectResourceBundleName(projectItem.EvaluatedInclude);

            if (destinationDirectory.BundleResources.Values.Any(x => x == value))
                return true;

            destinationDirectory.BundleResources.Add(key, value);

            return true;
        }

        private bool AcceptAssetResourceProjectItem(ProjectItem projectItem)
        {
            var filename = Path.GetFileName(projectItem.EvaluatedInclude);

            if (filename != ContentsFileName)
                return false;

            var parent = Directory.GetParent(projectItem.EvaluatedInclude).ToString();

            if (!parent.Contains(AssetSetExtension))
                return false;

            var assetName = Path.GetFileNameWithoutExtension(parent.Replace(AssetSetExtension, string.Empty));

            var constName = ValidNamesProvider.GetCorrectConstName(assetName);
            var constValue = ValidNamesProvider.GetCorrectResourceBundleName(assetName);

            _rootBundleDirectory.BundleResources.Add(constName, constValue);

            return true;
        }

        public string GenerateSourceCode()
        {
            var stringBuilder = new StringBuilder();

            AddClassForBundleDirectory(_rootBundleDirectory);

            void AddClassForBundleDirectory(BundleDirectory bd)
            {
                stringBuilder.AppendLine($"public static class {bd.Name}");
                stringBuilder.AppendLine("{");

                foreach (var resource in bd.BundleResources)
                {
                    stringBuilder.AppendLine($"public static UIKit.UIImage {resource.Key}() => UIKit.UIImage.FromBundle(@\"{resource.Value}\");");
                }

                foreach (var bundleSubDirectory in bd.BundleSubDirectories)
                {
                    AddClassForBundleDirectory(bundleSubDirectory);
                }

                stringBuilder.AppendLine("}");
            }

            return stringBuilder.ToString();
        }
    }
}
