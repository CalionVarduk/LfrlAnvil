using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using LfrlAnvil.Chrono;
using LfrlAnvil.Extensions;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Identifiers
{
    public sealed class IdentifierGenerator : IIdentifierGenerator
    {
        private readonly ITimestampProvider _timestampProvider;
        private readonly ulong _highValueOffset;
        private readonly ulong _maxHighValue;

        public IdentifierGenerator(ITimestampProvider timestampProvider)
            : this( timestampProvider, Bounds.Create( ushort.MinValue, ushort.MaxValue ) ) { }

        public IdentifierGenerator(ITimestampProvider timestampProvider, Timestamp baseTimestamp)
            : this( timestampProvider, baseTimestamp, Bounds.Create( ushort.MinValue, ushort.MaxValue ) ) { }

        public IdentifierGenerator(ITimestampProvider timestampProvider, Bounds<ushort> lowValueBounds)
            : this( timestampProvider, lowValueBounds, default ) { }

        public IdentifierGenerator(ITimestampProvider timestampProvider, Timestamp baseTimestamp, Bounds<ushort> lowValueBounds)
            : this( timestampProvider, baseTimestamp, lowValueBounds, default ) { }

        public IdentifierGenerator(
            ITimestampProvider timestampProvider,
            Bounds<ushort> lowValueBounds,
            LowValueExceededHandlingStrategy lowValueExceededHandlingStrategy)
            : this(
                timestampProvider,
                Duplicate( timestampProvider.GetNow() ),
                lowValueBounds,
                lowValueExceededHandlingStrategy ) { }

        public IdentifierGenerator(
            ITimestampProvider timestampProvider,
            Timestamp baseTimestamp,
            Bounds<ushort> lowValueBounds,
            LowValueExceededHandlingStrategy lowValueExceededHandlingStrategy)
            : this( timestampProvider, baseTimestamp, timestampProvider.GetNow(), lowValueBounds, lowValueExceededHandlingStrategy ) { }

        private IdentifierGenerator(
            ITimestampProvider timestampProvider,
            (Timestamp Base, Timestamp Start) timestamps,
            Bounds<ushort> lowValueBounds,
            LowValueExceededHandlingStrategy lowValueExceededHandlingStrategy)
            : this( timestampProvider, timestamps.Base, timestamps.Start, lowValueBounds, lowValueExceededHandlingStrategy ) { }

        private IdentifierGenerator(
            ITimestampProvider timestampProvider,
            Timestamp baseTimestamp,
            Timestamp startTimestamp,
            Bounds<ushort> lowValueBounds,
            LowValueExceededHandlingStrategy lowValueExceededHandlingStrategy)
        {
            baseTimestamp = TrimTicks( baseTimestamp );
            startTimestamp = TrimTicks( startTimestamp );
            Ensure.IsLessThanOrEqualTo( baseTimestamp, startTimestamp, nameof( baseTimestamp ) );

            _timestampProvider = timestampProvider;
            BaseTimestamp = baseTimestamp;
            StartTimestamp = startTimestamp;
            LowValueBounds = lowValueBounds;
            LowValueExceededHandlingStrategy = lowValueExceededHandlingStrategy;

            LastLowValue = lowValueBounds.Min - 1;
            _highValueOffset = (ulong)(startTimestamp - baseTimestamp).FullMilliseconds;
            LastHighValue = _highValueOffset;

            var maxPossibleHighValueOffset = TrimTicks( new Timestamp( DateTime.MaxValue ) ).Subtract( BaseTimestamp );
            var maxExpectedHighValueOffset = Duration.FromMilliseconds( (long)Identifier.MaxHighValue );
            _maxHighValue = (ulong)maxExpectedHighValueOffset.Min( maxPossibleHighValueOffset ).FullMilliseconds;
        }

        public Timestamp StartTimestamp { get; }
        public Timestamp BaseTimestamp { get; }
        public Bounds<ushort> LowValueBounds { get; }
        public ulong LastHighValue { get; private set; }
        public int LastLowValue { get; private set; }
        public LowValueExceededHandlingStrategy LowValueExceededHandlingStrategy { get; }

        public Timestamp LastTimestamp => BaseTimestamp.Add( Duration.FromMilliseconds( (long)LastHighValue ) );
        public Timestamp MaxTimestamp => BaseTimestamp.Add( Duration.FromMilliseconds( (long)_maxHighValue ) );

        public ulong HighValuesLeft
        {
            get
            {
                var high = GetElapsedMilliseconds();
                if ( high > _maxHighValue )
                    return 0;

                if ( high > LastHighValue )
                    return _maxHighValue - high + 1;

                var lowValuesLeft = LowValueBounds.Max - LastLowValue;
                if ( lowValuesLeft > 0 )
                    return _maxHighValue - LastHighValue + 1;

                return _maxHighValue - LastHighValue;
            }
        }

        public int LowValuesLeft
        {
            get
            {
                var high = GetElapsedMilliseconds();
                if ( high > _maxHighValue )
                    return 0;

                if ( high > LastHighValue )
                    return LowValueBounds.Max - LowValueBounds.Min + 1;

                return LowValueBounds.Max - LastLowValue;
            }
        }

        public ulong ValuesLeft
        {
            get
            {
                var high = GetElapsedMilliseconds();
                if ( high > _maxHighValue )
                    return 0;

                var lowValuesPerHighValue = (ulong)(LowValueBounds.Max - LowValueBounds.Min + 1);

                if ( high > LastHighValue )
                {
                    var highValuesLeft = _maxHighValue - high + 1;
                    return highValuesLeft * lowValuesPerHighValue;
                }

                var futureHighValuesLeft = _maxHighValue - LastHighValue;
                var lowValuesLeft = (ulong)(LowValueBounds.Max - LastLowValue);
                return futureHighValuesLeft * lowValuesPerHighValue + lowValuesLeft;
            }
        }

        public bool IsOutOfValues => ValuesLeft <= 0;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Identifier Generate()
        {
            if ( ! TryGenerate( out var id ) )
                throw new InvalidOperationException( "failed to generate a new identifier" );

            return id;
        }

        public bool TryGenerate(out Identifier id)
        {
            var highValue = GetElapsedMilliseconds();

            if ( highValue > LastHighValue )
            {
                if ( highValue > _maxHighValue )
                {
                    id = default;
                    return false;
                }

                id = CreateNextId( highValue );
                return true;
            }

            if ( LastLowValue < LowValueBounds.Max )
            {
                id = CreateNextId();
                return true;
            }

            highValue = LowValueExceededHandlingStrategy switch
            {
                LowValueExceededHandlingStrategy.AddMs => LastHighValue + 1,
                LowValueExceededHandlingStrategy.BusyWait => GetHighValueByBusyWait(),
                LowValueExceededHandlingStrategy.Sleep => GetHighValueBySleep( highValue ),
                _ => _maxHighValue + 1
            };

            if ( highValue > _maxHighValue )
            {
                id = default;
                return false;
            }

            id = CreateNextId( highValue );
            return true;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Timestamp GetTimestamp(Identifier id)
        {
            return BaseTimestamp.Add( Duration.FromMilliseconds( (long)id.High ) );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private ulong GetElapsedMilliseconds()
        {
            return _highValueOffset + (ulong)(_timestampProvider.GetNow() - StartTimestamp).FullMilliseconds;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private ulong GetHighValueByBusyWait()
        {
            ulong highValue;
            do
            {
                highValue = GetElapsedMilliseconds();
            }
            while ( highValue <= LastHighValue );

            return highValue;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private ulong GetHighValueBySleep(ulong highValue)
        {
            do
            {
                Thread.Sleep( (int)(LastHighValue - highValue) + 1 );
                highValue = GetElapsedMilliseconds();
            }
            while ( highValue <= LastHighValue );

            return highValue;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private Identifier CreateNextId(ulong highValue)
        {
            LastHighValue = highValue;
            LastLowValue = LowValueBounds.Min;
            return new Identifier( highValue, (ushort)LastLowValue );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private Identifier CreateNextId()
        {
            return new Identifier( LastHighValue, (ushort)++LastLowValue );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static (Timestamp, Timestamp) Duplicate(Timestamp timestamp)
        {
            return (timestamp, timestamp);
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static Timestamp TrimTicks(Timestamp timestamp)
        {
            var ticksToSubtract = timestamp.UnixEpochTicks % ChronoConstants.TicksPerMillisecond;
            return timestamp.Subtract( Duration.FromTicks( ticksToSubtract ) );
        }

        object IGenerator.Generate()
        {
            return Generate();
        }

        bool IGenerator.TryGenerate(out object? result)
        {
            if ( TryGenerate( out var internalResult ) )
            {
                result = internalResult;
                return true;
            }

            result = null;
            return false;
        }
    }
}
