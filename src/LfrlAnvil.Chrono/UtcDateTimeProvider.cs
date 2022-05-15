using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Internal;

namespace LfrlAnvil.Chrono
{
    public sealed class UtcDateTimeProvider : DateTimeProviderBase
    {
        public UtcDateTimeProvider()
            : base( DateTimeKind.Utc ) { }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public override DateTime GetNow()
        {
            return DateTime.UtcNow;
        }
    }
}
