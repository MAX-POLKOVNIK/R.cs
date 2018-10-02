using System;

namespace R.cs.Core
{
    internal static class Mono
    {
        public static bool IsMono() => Type.GetType("Mono.Runtime") != null;
    }
}
