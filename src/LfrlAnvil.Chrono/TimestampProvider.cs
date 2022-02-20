using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Chrono
{
    public sealed class TimestampProvider : ITimestampProvider
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Timestamp GetNow()
        {
            return new Timestamp( DateTime.UtcNow );
        }
    }
}
