using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlSoft.NET.Core.Chrono
{
    public sealed class FrozenTimestampProvider : ITimestampProvider
    {
        private readonly Timestamp _now;

        public FrozenTimestampProvider(Timestamp now)
        {
            _now = now;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Timestamp GetNow()
        {
            return _now;
        }
    }
}
