using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace R.cs.Core
{
    internal sealed class ControllerGenerator
    {
        private readonly string[] _storyboardsPaths;
        private readonly string _projectPath;

        public ControllerGenerator(string projectPath, string[] storyboardsPaths)
        {
            _storyboardsPaths = storyboardsPaths ?? throw new ArgumentNullException(nameof(storyboardsPaths));
            _projectPath = projectPath ?? throw new ArgumentNullException(nameof(projectPath));
        }

        public async Task<string> Do()
        {
            var registeredTouchClasses = await GetRegisteredTouchClasses();

            return null;
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

                    registeredTouchClasses.Add(registeredName, $"{namespaceName}.{className}");
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
