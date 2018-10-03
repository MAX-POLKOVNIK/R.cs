using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace R.cs.Core.ProjectItemsProcessors
{
    internal sealed class ControllersProcessor : IProjectItemProcessor
    {
        private const string InterfaceDefinitionItemType = "InterfaceDefinition";
        private const string CompileItemType = "Compile";
        private const string StoryboardFileExtension = ".storyboard";

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

        private readonly string _projectPath;
        private readonly List<(string storyboardIdentifier, string customClass, string storyboard)> _viewControllerList;
        private readonly IDictionary<string, string> _csNativeClassToCsClassDictionary;

        public ControllersProcessor(string projectPath)
        {
            _projectPath = projectPath ?? throw new ArgumentNullException(nameof(projectPath));

            _viewControllerList = new List<(string storyboardIdentifier, string customClass, string storyboard)>();
            _csNativeClassToCsClassDictionary = new Dictionary<string, string>();
        }

        public string GenerateSourceCode()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("public static class ViewController");
            stringBuilder.AppendLine("{");

            foreach (var (storyboardIdentifier, nativeClass, storyboard) in _viewControllerList)
            {
                stringBuilder.AppendLine($"public static {_csNativeClassToCsClassDictionary[nativeClass]} {storyboardIdentifier}(Foundation.NSBundle bundle = null) " +
                                         $"=> ({_csNativeClassToCsClassDictionary[nativeClass]}) UIKit.UIStoryboard.FromName(\"{storyboard}\", bundle).InstantiateViewController(\"{storyboardIdentifier}\");");
            }

            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }

        public bool Process(ProjectItem projectItem)
        {
            switch (projectItem.ItemType)
            {
                case InterfaceDefinitionItemType:
                    return ProcessInterfaceDefinitionItemType(projectItem);
                case CompileItemType:
                    return ProcessCompileDefinitionItemType(projectItem);
                default:
                    return false;
            }
        }

        private bool ProcessInterfaceDefinitionItemType(ProjectItem projectItem)
        {
            var filename = Path.GetFileName(projectItem.EvaluatedInclude);
            var extension = Path.GetExtension(filename);

            if (extension != StoryboardFileExtension)
                return false;

            var storyboardPath = Path.Combine(Directory.GetParent(_projectPath).ToString(), projectItem.EvaluatedInclude);

            var xdoc = XDocument.Load(storyboardPath);

            var scenesElement = xdoc.Descendants()
                .First(x => x.Name == "document")
                .Descendants()
                .FirstOrDefault(x => x.Name == "scenes");

            if (scenesElement == null)
                return true;

            var viewControllers = scenesElement.Descendants()
                .Where(x => x.Name == "scene")
                .Descendants()
                .Where(x => x.Name == "objects")
                .Descendants()
                .Where(x => SupportedViewControllersTypes.Any(y => y == x.Name.ToString()))
                .ToArray();

            foreach (var viewController in viewControllers)
            {
                var referencedIdentifier = viewController.Attributes().FirstOrDefault(x => x.Name == "storyboardIdentifier");
                if (referencedIdentifier == null)
                    continue;

                var customClass = viewController.Attributes().FirstOrDefault(x => x.Name == "customClass");
                if (customClass == null)
                    continue;

                _viewControllerList.Add((referencedIdentifier.Value, customClass.Value, Path.GetFileNameWithoutExtension(filename)));
            }

            return true;
        }

        private bool ProcessCompileDefinitionItemType(ProjectItem projectItem)
        {
            var csFilePath = Path.Combine(Directory.GetParent(_projectPath).ToString(), projectItem.EvaluatedInclude);

            var code = File.ReadAllText(csFilePath);

            var classes = CSharpSyntaxTree.ParseText(code).GetRoot()
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

                _csNativeClassToCsClassDictionary.Add(registeredName.Trim('"'), $"{namespaceName}.{className}");
            }

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

            return true;
        }
    }
}
