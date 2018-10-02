using Microsoft.Build.Evaluation;

namespace R.cs.Core.ProjectItemsProcessors
{
    internal interface IProjectItemProcessor
    {
        bool Process(ProjectItem projectItem);

        string GenerateSourceCode();
    }
}
