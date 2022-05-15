using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Internal;

namespace LfrlAnvil.Chrono
{
    public sealed class FrozenTimestampProvider : TimestampProviderBase
    {
        private readonly Timestamp _now;

        public FrozenTimestampProvider(Timestamp now)
        {
            _now = now;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public override Timestamp GetNow()
        {
            return _now;
        }
    }
}
