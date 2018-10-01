using Microsoft.Build.Framework;
using Task = Microsoft.Build.Utilities.Task;

namespace Rcs
{
    public class RcsBuildTask : Task
    {
        [Required]
        public string ProjectPath { get; set; }

        [Required]
        public string RootNamespace { get; set; }

        public override bool Execute()
        {
            new Generator().Do(ProjectPath, RootNamespace);
            return true;
        }
    }
}
