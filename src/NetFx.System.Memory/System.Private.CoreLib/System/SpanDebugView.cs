// Decompiled with JetBrains decompiler
// Type: System.SpanDebugView`1
// Assembly: System.Memory, Version=4.0.1.2, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
// MVID: 866AE087-4753-44D8-B4C3-B8D9EAD86168
// Assembly location: F:\Users\shad\source\repos\CheckSystemMemoryDependencies\CheckSystemMemoryDependencies\bin\Debug\net45\System.Memory.dll

using System.Diagnostics;

namespace System
{
    internal sealed class SpanDebugView<T>
    {
        private readonly T[] _array;

        public SpanDebugView(Span<T> span)
        {
            _array = span.ToArray();
        }

        public SpanDebugView(ReadOnlySpan<T> span)
        {
            _array = span.ToArray();
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items => _array;
    }
}
