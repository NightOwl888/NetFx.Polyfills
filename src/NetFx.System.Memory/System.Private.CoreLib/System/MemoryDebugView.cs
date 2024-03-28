// Decompiled with JetBrains decompiler
// Type: System.MemoryDebugView`1
// Assembly: System.Memory, Version=4.0.1.2, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
// MVID: 866AE087-4753-44D8-B4C3-B8D9EAD86168

using System.Diagnostics;

namespace System
{
    internal sealed class MemoryDebugView<T>
    {
        private readonly ReadOnlyMemory<T> _memory;

        public MemoryDebugView(Memory<T> memory)
        {
            this._memory = (ReadOnlyMemory<T>)memory;
        }

        public MemoryDebugView(ReadOnlyMemory<T> memory)
        {
            this._memory = memory;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items => _memory.ToArray();
    }
}
