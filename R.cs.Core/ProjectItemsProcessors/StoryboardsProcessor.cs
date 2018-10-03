using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Build.Evaluation;

namespace R.cs.Core.ProjectItemsProcessors
{
    internal sealed class StoryboardsProcessor : IProjectItemProcessor
    {
        private const string SupportedProjectItemType = "InterfaceDefinition";
        private const string StoryboardFileExtension = ".storyboard";

        private readonly IDictionary<string, string> _consts;

        public StoryboardsProcessor()
        {
            _consts = new Dictionary<string, string>();
        }

        public bool Process(ProjectItem projectItem)
        {
            if (projectItem.ItemType != SupportedProjectItemType)
                return false;

            var filename = Path.GetFileName(projectItem.EvaluatedInclude);
            var extension = Path.GetExtension(filename);

            if (extension != StoryboardFileExtension)
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

            stringBuilder.AppendLine("public static class Storyboard");
            stringBuilder.AppendLine("{");

            foreach (var @const in _consts)
            {
                stringBuilder.AppendLine($"public static UIKit.UIStoryboard {@const.Key}(Foundation.NSBundle bundle = null) => UIKit.UIStoryboard.FromName(\"{@const.Value}\", bundle);");
            }

            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }
    }
}
