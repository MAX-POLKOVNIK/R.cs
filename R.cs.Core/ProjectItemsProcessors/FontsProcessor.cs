using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Evaluation;

namespace R.cs.Core.ProjectItemsProcessors
{
    internal sealed class FontsProcessor : IProjectItemProcessor
    {
        private const string SupportedProjectItemType = "BundleResource";
        private static readonly string[] FontFileExtensions = {".otf", ".ttf"};

        private readonly IDictionary<string, string> _consts;

        public FontsProcessor()
        {
            _consts = new Dictionary<string, string>();
        }

        public bool Process(ProjectItem projectItem)
        {
            if (projectItem.ItemType != SupportedProjectItemType)
                return false;

            var filename = Path.GetFileName(projectItem.EvaluatedInclude);
            var extension = Path.GetExtension(filename);

            if (FontFileExtensions.All(x => extension != x))
                return false;

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

            var constName = ValidNamesProvider.GetCorrectConstName(fileNameWithoutExtension);
            var constValue = ValidNamesProvider.GetCorrectResourceBundleName(fileNameWithoutExtension);

            _consts.Add(constName, constValue);

            return true;
        }

        public string GenerateSourceCode()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("public static class Font");
            stringBuilder.AppendLine("{");

            foreach (var @const in _consts)
            {
                stringBuilder.AppendLine($"public static UIKit.UIFont {@const.Key}(System.nfloat size) => UIKit.UIFont.FromName(\"{@const.Value}\", size);");
            }

            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }
    }
}
