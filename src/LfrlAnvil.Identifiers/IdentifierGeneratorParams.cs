using System.Diagnostics.Contracts;
using LfrlAnvil.Chrono;

namespace LfrlAnvil.Identifiers
{
    public struct IdentifierGeneratorParams
    {
        private Timestamp? _baseTimestamp;
        private Bounds<ushort>? _lowValueBounds;
        private Duration? _timeEpsilon;

        public LowValueExceededHandlingStrategy LowValueExceededHandlingStrategy { get; set; }

        public Timestamp BaseTimestamp
        {
            get => _baseTimestamp ?? Timestamp.Zero;
            set => _baseTimestamp = value;
        }

        public Bounds<ushort> LowValueBounds
        {
            get => _lowValueBounds ?? Bounds.Create( ushort.MinValue, ushort.MaxValue );
            set => _lowValueBounds = value;
        }

        public Duration TimeEpsilon
        {
            get => _timeEpsilon ?? Duration.FromMilliseconds( 1 );
            set => _timeEpsilon = value;
        }

        [Pure]
        public override string ToString()
        {
            var baseTimestampText = $"{nameof( BaseTimestamp )}={BaseTimestamp}";
            var timeEpsilonText = $"{nameof( TimeEpsilon )}={TimeEpsilon}";
            var lowValueBoundsText = $"{nameof( LowValueBounds )}={LowValueBounds}";
            var strategyText = $"{nameof( LowValueExceededHandlingStrategy )}={LowValueExceededHandlingStrategy}";
            return $"{{ {baseTimestampText}, {timeEpsilonText}, {lowValueBoundsText}, {strategyText} }}";
        }
    }
}
