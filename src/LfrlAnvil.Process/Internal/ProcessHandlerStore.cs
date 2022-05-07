using System;

namespace LfrlAnvil.Process.Internal
{
    internal readonly struct ProcessHandlerStore
    {
        internal ProcessHandlerStore(Delegate @delegate, bool isAsync)
        {
            Delegate = @delegate;
            IsAsync = isAsync;
        }

        internal Delegate Delegate { get; }
        internal bool IsAsync { get; }
    }
}
