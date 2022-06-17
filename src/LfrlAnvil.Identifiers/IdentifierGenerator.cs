using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using LfrlAnvil.Chrono;
using LfrlAnvil.Extensions;
using LfrlAnvil.Generators;
using LfrlAnvil.Identifiers.Exceptions;

namespace LfrlAnvil.Identifiers
{
    public sealed class IdentifierGenerator : IIdentifierGenerator
    {
        private readonly ITimestampProvider _timestampProvider;
        private readonly ulong _highValueOffset;
        private readonly ulong _maxHighValue;

        public IdentifierGenerator(ITimestampProvider timestampProvider)
            : this( timestampProvider, new IdentifierGeneratorParams() ) { }

        public IdentifierGenerator(ITimestampProvider timestampProvider, IdentifierGeneratorParams @params)
        {
            var timeEpsilon = @params.TimeEpsilon;
            Ensure.IsInRange( timeEpsilon, Duration.FromTicks( 1 ), Duration.FromMilliseconds( 3 ), nameof( timeEpsilon ) );

            var startTimestamp = timestampProvider.GetNow();
            Ensure.IsGreaterThanOrEqualTo( startTimestamp, Timestamp.Zero, nameof( startTimestamp ) );

            var baseTimestamp = @params.BaseTimestamp;
            Ensure.IsInRange( baseTimestamp, Timestamp.Zero, startTimestamp, nameof( baseTimestamp ) );

            _timestampProvider = timestampProvider;
            BaseTimestamp = ConvertTimestamp( baseTimestamp, timeEpsilon );
            StartTimestamp = ConvertTimestamp( startTimestamp, timeEpsilon );
            TimeEpsilon = timeEpsilon;
            LowValueBounds = @params.LowValueBounds;
            LowValueOverflowStrategy = @params.LowValueOverflowStrategy;

            LastLowValue = LowValueBounds.Min - 1;
            _highValueOffset = ConvertToHighValue( StartTimestamp.Subtract( BaseTimestamp ), timeEpsilon );
            LastHighValue = _highValueOffset;

            var maxPossibleHighValueOffset = ConvertTimestamp( new Timestamp( DateTime.MaxValue ), timeEpsilon ).Subtract( BaseTimestamp );
            var maxExpectedHighValueOffset = ConvertToDuration( Identifier.MaxHighValue - 1, timeEpsilon );
            _maxHighValue = ConvertToHighValue( maxExpectedHighValueOffset.Min( maxPossibleHighValueOffset ), timeEpsilon );
        }

        public Timestamp StartTimestamp { get; }
        public Timestamp BaseTimestamp { get; }
        public Bounds<ushort> LowValueBounds { get; }
        public Duration TimeEpsilon { get; }
        public LowValueOverflowStrategy LowValueOverflowStrategy { get; }
        public ulong LastHighValue { get; private set; }
        public int LastLowValue { get; private set; }

        public Timestamp LastTimestamp => BaseTimestamp.Add( ConvertToDuration( LastHighValue, TimeEpsilon ) );
        public Timestamp MaxTimestamp => BaseTimestamp.Add( ConvertToDuration( _maxHighValue, TimeEpsilon ) );

        public ulong HighValuesLeft
        {
            get
            {
                var highValue = GetCurrentHighValue();
                if ( highValue > _maxHighValue )
                    return 0;

                if ( highValue > LastHighValue )
                    return _maxHighValue - highValue + 1;

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
                var highValue = GetCurrentHighValue();
                if ( highValue > _maxHighValue )
                    return 0;

                if ( highValue > LastHighValue )
                    return LowValueBounds.Max - LowValueBounds.Min + 1;

                return LowValueBounds.Max - LastLowValue;
            }
        }

        public ulong ValuesLeft
        {
            get
            {
                var highValue = GetCurrentHighValue();
                if ( highValue > _maxHighValue )
                    return 0;

                var lowValuesPerHighValue = (ulong)(LowValueBounds.Max - LowValueBounds.Min + 1);

                if ( highValue > LastHighValue )
                {
                    var highValuesLeft = _maxHighValue - highValue + 1;
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
                throw new IdentifierGenerationException();

            return id;
        }

        public bool TryGenerate(out Identifier id)
        {
            var highValue = GetCurrentHighValue();

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

            highValue = LowValueOverflowStrategy switch
            {
                LowValueOverflowStrategy.AddHighValue => LastHighValue + 1,
                LowValueOverflowStrategy.SpinWait => GetHighValueBySpinWait(),
                LowValueOverflowStrategy.Sleep => GetHighValueBySleep( highValue ),
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
            var offset = ConvertToDuration( id.High, TimeEpsilon );
            return BaseTimestamp.Add( offset );
        }

        [Pure]
        public ulong CalculateThroughput(Duration duration)
        {
            duration = duration.Max( Duration.Zero );
            var fullHighValueCount = ConvertToHighValue( duration, TimeEpsilon );
            var fullLowValueCount = (ulong)(LowValueBounds.Max - LowValueBounds.Min + 1);
            var lowValueRemainderRatio = (double)(duration.Ticks % TimeEpsilon.Ticks) / TimeEpsilon.Ticks;
            var remainingLowValueCount = (ulong)Math.Truncate( lowValueRemainderRatio * fullLowValueCount );
            return fullHighValueCount * fullLowValueCount + remainingLowValueCount;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private ulong GetCurrentHighValue()
        {
            var elapsedTime = _timestampProvider.GetNow() - StartTimestamp;
            return _highValueOffset + ConvertToHighValue( elapsedTime, TimeEpsilon );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private ulong GetHighValueBySpinWait()
        {
            var highValue = GetCurrentHighValue();
            while ( highValue <= LastHighValue )
            {
                Thread.SpinWait( 1 );
                highValue = GetCurrentHighValue();
            }

            return highValue;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private ulong GetHighValueBySleep(ulong highValue)
        {
            do
            {
                var timeout = ConvertToDuration( LastHighValue - highValue, TimeEpsilon ).Max( Duration.FromMilliseconds( 1 ) );
                Thread.Sleep( (int)Math.Ceiling( timeout.TotalMilliseconds ) );
                highValue = GetCurrentHighValue();
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
        private static Timestamp ConvertTimestamp(Timestamp timestamp, Duration epsilon)
        {
            var ticksToSubtract = timestamp.UnixEpochTicks % epsilon.Ticks;
            return timestamp.Subtract( Duration.FromTicks( ticksToSubtract ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static ulong ConvertToHighValue(Duration elapsedTime, Duration epsilon)
        {
            return (ulong)(elapsedTime.Ticks / epsilon.Ticks);
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static Duration ConvertToDuration(ulong highValue, Duration epsilon)
        {
            return Duration.FromTicks( (long)highValue * epsilon.Ticks );
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
