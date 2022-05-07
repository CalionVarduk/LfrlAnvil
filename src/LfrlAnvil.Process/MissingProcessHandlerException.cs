using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Process
{
    public class MissingProcessHandlerException : Exception
    {
        public MissingProcessHandlerException(Type argsType)
            : base( GetMessage( argsType ) )
        {
            ArgsType = argsType;
        }

        public Type ArgsType { get; }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static string GetMessage(Type argsType)
        {
            var argsText = argsType.FullName;
            return $"Handler is missing for a process with args of {argsText} type.";
        }
    }
}
