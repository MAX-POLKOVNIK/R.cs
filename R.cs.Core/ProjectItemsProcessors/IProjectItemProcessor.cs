using Microsoft.Build.Evaluation;

namespace R.cs.Core.ProjectItemsProcessors
{
    internal interface IProjectItemProcessor : ISourceCodeGenerator
    {
        bool Process(ProjectItem projectItem);
    }
}
