using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Build.Evaluation;

namespace R.cs.Core.ProjectItemsProcessors
{
    internal sealed class XibsProcessor : IProjectItemProcessor
    {
        private const string SupportedProjectItemType = "InterfaceDefinition";
        private const string StoryboardFileExtension = ".xib";

        private readonly IDictionary<string, string> _consts;

        public XibsProcessor()
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

            stringBuilder.AppendLine("static class Xib");
            stringBuilder.AppendLine("{");

            foreach (var @const in _consts)
            {
                stringBuilder.AppendLine($"public static T {@const.Key}<T>(Foundation.NSBundle bundle = null, Foundation.NSObject owner = null, Foundation.NSDictionary options = null) where T : Foundation.NSObject" +
                                         $" => ObjCRuntime.Runtime.GetNSObject<T>((bundle ??  Foundation.NSBundle.MainBundle).LoadNib(\"{@const.Value}\", owner, options).ValueAt(0));");
            }

            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }
    }
}
