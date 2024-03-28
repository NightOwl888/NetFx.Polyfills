// Decompiled with JetBrains decompiler
// Type: System.NotImplemented
// Assembly: System.Memory, Version=4.0.1.2, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
// MVID: 866AE087-4753-44D8-B4C3-B8D9EAD86168

namespace System
{
    internal static class NotImplemented
    {
        internal static Exception ByDesign
        {
            get
            {
                return new NotImplementedException();
            }
        }

        internal static Exception ByDesignWithMessage(string message)
        {
            return new NotImplementedException(message);
        }

        internal static Exception ActiveIssue(string issue)
        {
            return new NotImplementedException();
        }
    }
}
