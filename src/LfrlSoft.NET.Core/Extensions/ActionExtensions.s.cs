using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlSoft.NET.Core.Functional;
using Unsafe = LfrlSoft.NET.Core.Functional.Unsafe;

namespace LfrlSoft.NET.Core.Extensions
{
    public static class ActionExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Unsafe<Nil> TryInvoke(this Action source)
        {
            return Unsafe.Try( source );
        }
    }
}
