using System;
using Xunit.Sdk;

namespace System
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class OuterLoopAttribute : CategoryAttribute
    {
        public override string Type => "OuterLoop";
    }
}
