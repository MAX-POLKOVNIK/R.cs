using System;
using R.cs.Core;

namespace R.cs.Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write(new Generator().Do(args[0], args[1]));
            Console.Read();
        }
    }
}
