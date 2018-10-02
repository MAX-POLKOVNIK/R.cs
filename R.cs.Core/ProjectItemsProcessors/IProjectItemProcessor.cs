using Microsoft.Build.Evaluation;

namespace R.cs.Core.ProjectItemsProcessors
{
    internal interface IProjectItemProcessor
    {
        bool Accept(ProjectItem projectItem);

        string GenerateSourceCode();
    }
}
