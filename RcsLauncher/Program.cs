using Rcs;

namespace RcsLauncher
{
    internal class Program
    {
        private static void Main(string[] args) => new Generator().Do(args[0], args[1]);
    }
}
