﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Evaluation;
using R.cs.Core.ProjectItemsProcessors;

namespace R.cs.Core
{
    public class Generator
    {
        private static readonly string GeneratedFileDescription = "// =============================================================================================\n" +
                                                                  $"// This file generated by R.cs v{typeof(Generator).Assembly.GetName().Version.ToString(2)}\n" +
                                                                  "// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated\n" +
                                                                  "// Site: https://github.com/MAX-POLKOVNIK/R.cs \n" +
                                                                  "// =============================================================================================\n";

        private static readonly string PathToRcs = Path.Combine("Resources", "R.cs");

        private IProjectItemProcessor[] _projectItemProcessors;
        
        
        public string Do(string path, string rootNamespace)
        {
            var project = new Project(path, null, null, new ProjectCollection());

            _projectItemProcessors = new IProjectItemProcessor[]
            {
                new StoryboardsProcessor(),
                new XibsProcessor(),
                new FontsProcessor(),
                new ColorsProcessor(),
                new ImagesProcessor(),
                new ControllersProcessor(path)
            };

            foreach (var projectEvaluatedItem in project.AllEvaluatedItems)
            {
                foreach (var projectItemProcessor in _projectItemProcessors)
                {
                    try
                    {
                        projectItemProcessor.Process(projectEvaluatedItem);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error processing: {projectEvaluatedItem.EvaluatedInclude}", ex);
                    }
                }
            }
            
            var fileContent = GenerateRcsContent($"{rootNamespace}", classes: _projectItemProcessors.Select(x => x.GenerateSourceCode()).ToArray());

            var resourceClassItem = project.AllEvaluatedItems.FirstOrDefault(x => x.ItemType == "Compile" && x.EvaluatedInclude == PathToRcs);

            if (resourceClassItem == null)
            {
                project.AddItem("Compile", PathToRcs);
                project.Save();
            }

            var fullRcsFilePath = Path.Combine(Directory.GetParent(path).ToString(), PathToRcs);

            File.WriteAllText(fullRcsFilePath, fileContent);

            ProjectCollection.GlobalProjectCollection.UnloadAllProjects();

            return fileContent;
        }

        private static string GenerateRcsContent(string @namespace, string[] classes)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(GeneratedFileDescription);
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine($"namespace {@namespace}");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine("static class R");
            stringBuilder.AppendLine("{");

            foreach (var @class in classes)
            {
                stringBuilder.Append(@class);
            }

            stringBuilder.AppendLine("}");
            stringBuilder.AppendLine("}");
            
            return stringBuilder.ToString();
        }
    }
}
