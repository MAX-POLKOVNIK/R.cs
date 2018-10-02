using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Build.Evaluation;

namespace R.cs.Core.ProjectItemsProcessors
{
    internal sealed class ColorsProcessor : IProjectItemProcessor
    {
        private const string SupportedProjectItemType = "ImageAsset";
        private const string ContentsFileName = "Contents.json";
        private const string AssetSetExtension = ".colorset";

        private readonly IDictionary<string, string> _consts;

        public ColorsProcessor()
        {
            _consts = new Dictionary<string, string>();
        }

        public bool Process(ProjectItem projectItem)
        {
            if (projectItem.ItemType != SupportedProjectItemType)
                return false;

            var filePath = projectItem.EvaluatedInclude.Replace('\\', Path.DirectorySeparatorChar);
            var filename = Path.GetFileName(filePath);

            if (filename != ContentsFileName)
                return false;

            var parent = Directory.GetParent(filePath).ToString();

            if (!parent.Contains(AssetSetExtension))
                return false;

            var assetName = Path.GetFileNameWithoutExtension(parent.Replace(AssetSetExtension, string.Empty));
            
            var constName = ValidNamesProvider.GetCorrectConstName(assetName);
            var constValue = ValidNamesProvider.GetCorrectResourceBundleName(assetName);

            _consts.Add(constName, constValue);

            return true;
        }

        public string GenerateSourceCode()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("public static class Color");
            stringBuilder.AppendLine("{");

            foreach (var @const in _consts)
            {
                stringBuilder.AppendLine($"public static UIKit.UIColor {@const.Key}() => UIKit.UIColor.FromName(\"{@const.Value}\");");
            }

            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }
    }
}
