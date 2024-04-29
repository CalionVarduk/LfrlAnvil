using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using LfrlAnvil.Chrono;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;
using LfrlAnvil.Generators;
using LfrlAnvil.Identifiers.Exceptions;

namespace LfrlAnvil.Identifiers;

/// <inheritdoc />
/// <remarks>
/// Generators use the <see cref="Identifier.High"/> value as a representation of the time slice during which an <see cref="Identifier"/>
/// has been created and the <see cref="Identifier.Low"/> value as a sequential number unique within a single <see cref="Identifier.High"/>
/// value (or a time slice).
/// </remarks>
public sealed class IdentifierGenerator : IIdentifierGenerator
{
    private readonly ulong _highValueOffset;
    private readonly ulong _maxHighValue;

    /// <summary>
    /// Creates a new <see cref="IdentifierGenerator"/> instance.
    /// </summary>
    /// <param name="timestamps"><see cref="ITimestampProvider"/> to use in this generator.</param>
    /// <param name="params">Optional parameters. See <see cref="IdentifierGeneratorParams"/> for more information.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <see cref="IdentifierGeneratorParams.TimeEpsilon"/> is less than or equal to <see cref="Duration.Zero"/>
    /// or the current <see cref="Timestamp"/> returned by the <see cref="ITimestampProvider"/> instance
    /// is less than <see cref="Timestamp.Zero"/> or <see cref="IdentifierGeneratorParams.BaseTimestamp"/> is not between
    /// <see cref="Timestamp.Zero"/> and the current <see cref="Timestamp"/>.
    /// </exception>
    public IdentifierGenerator(ITimestampProvider timestamps, IdentifierGeneratorParams @params = default)
    {
        var timeEpsilon = @params.TimeEpsilon;
        Ensure.IsGreaterThan( timeEpsilon, Duration.Zero );

        var startTimestamp = timestamps.GetNow();
        Ensure.IsGreaterThanOrEqualTo( startTimestamp, Timestamp.Zero );

        var baseTimestamp = @params.BaseTimestamp;
        Ensure.IsInRange( baseTimestamp, Timestamp.Zero, startTimestamp );

        Timestamps = timestamps;
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

    /// <summary>
    /// <see cref="ITimestampProvider"/> instance used by this generator.
    /// </summary>
    public ITimestampProvider Timestamps { get; }

    /// <summary>
    /// <see cref="Timestamp"/> of the creation of this generator instance.
    /// </summary>
    public Timestamp StartTimestamp { get; }

    /// <inheritdoc />
    public Timestamp BaseTimestamp { get; }

    /// <summary>
    /// Specifies the range of available <see cref="Identifier.Low"/> values for identifiers created by this generator.
    /// </summary>
    public Bounds<ushort> LowValueBounds { get; }

    /// <summary>
    /// Specifies the time resolution of this generator.
    /// </summary>
    public Duration TimeEpsilon { get; }

    /// <summary>
    /// Specifies <see cref="LowValueOverflowStrategy"/> used by this generator.
    /// </summary>
    public LowValueOverflowStrategy LowValueOverflowStrategy { get; }

    /// <summary>
    /// Specifies the last <see cref="Identifier.High"/> value of an <see cref="Identifier"/> created by this generator.
    /// </summary>
    public ulong LastHighValue { get; private set; }

    /// <summary>
    /// Specifies the last <see cref="Identifier.Low"/> value of an <see cref="Identifier"/> created by this generator.
    /// </summary>
    public int LastLowValue { get; private set; }

    /// <summary>
    /// <see cref="Timestamp"/> of the last <see cref="Identifier"/> created by this generator.
    /// </summary>
    public Timestamp LastTimestamp => BaseTimestamp.Add( ConvertToDuration( LastHighValue, TimeEpsilon ) );

    /// <summary>
    /// Maximum possible <see cref="Timestamp"/> of an <see cref="Identifier"/> created by this generator.
    /// </summary>
    public Timestamp MaxTimestamp => BaseTimestamp.Add( ConvertToDuration( _maxHighValue, TimeEpsilon ) );

    /// <summary>
    /// Specifies the maximum number of high values that this generator can use to create new identifiers.
    /// </summary>
    /// <remarks>See <see cref="Identifier.High"/> for more information.</remarks>
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

    /// <summary>
    /// Specifies the maximum number of identifiers this generator can still create for the current time slice without having
    /// to resolve low value overflow.
    /// </summary>
    /// <remarks>See <see cref="Identifier.Low"/> for more information.</remarks>
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

    /// <summary>
    /// Specifies the maximum number of identifiers that this generate can still create.
    /// </summary>
    public ulong ValuesLeft
    {
        get
        {
            var highValue = GetCurrentHighValue();
            if ( highValue > _maxHighValue )
                return 0;

            var lowValuesPerHighValue = ( ulong )(LowValueBounds.Max - LowValueBounds.Min + 1);

            if ( highValue > LastHighValue )
            {
                var highValuesLeft = _maxHighValue - highValue + 1;
                return highValuesLeft * lowValuesPerHighValue;
            }

            var futureHighValuesLeft = _maxHighValue - LastHighValue;
            var lowValuesLeft = ( ulong )(LowValueBounds.Max - LastLowValue);
            return futureHighValuesLeft * lowValuesPerHighValue + lowValuesLeft;
        }
    }

    /// <summary>
    /// Specifies whether or not this generator can still create identifiers.
    /// </summary>
    public bool IsOutOfValues => ValuesLeft <= 0;

    /// <summary>
    /// Generates a new <see cref="Identifier"/>.
    /// </summary>
    /// <returns>New <see cref="Identifier"/> instance.</returns>
    /// <exception cref="IdentifierGenerationException">When generator has failed to create a new <see cref="Identifier"/>.</exception>
    /// <remarks>
    /// Generators will fail to create an <see cref="Identifier"/> when they are completely out of values
    /// or when low value overflow has occurred and the current <see cref="LowValueOverflowStrategy"/>
    /// is equal to <see cref="LowValueOverflowStrategy.Forbidden"/>.
    /// </remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Identifier Generate()
    {
        if ( ! TryGenerate( out var id ) )
            ExceptionThrower.Throw( new IdentifierGenerationException() );

        return id;
    }

    /// <summary>
    /// Attempts to generate a new <see cref="Identifier"/>.
    /// </summary>
    /// <param name="result"><b>out</b> parameter that returns generated <see cref="Identifier"/> if successful.</param>
    /// <returns><b>true</b> if an <see cref="Identifier"/> has been generated successfully, otherwise <b>false</b>.</returns>
    /// <remarks>
    /// Generators will fail to create an <see cref="Identifier"/> when they are completely out of values
    /// or when low value overflow has occurred and the current <see cref="LowValueOverflowStrategy"/>
    /// is equal to <see cref="LowValueOverflowStrategy.Forbidden"/>.
    /// </remarks>
    public bool TryGenerate(out Identifier result)
    {
        var highValue = GetCurrentHighValue();

        if ( highValue > LastHighValue )
        {
            if ( highValue > _maxHighValue )
            {
                result = default;
                return false;
            }

            result = CreateNextId( highValue );
            return true;
        }

        if ( LastLowValue < LowValueBounds.Max )
        {
            result = CreateNextId();
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
            result = default;
            return false;
        }

        result = CreateNextId( highValue );
        return true;
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Timestamp GetTimestamp(Identifier id)
    {
        var offset = ConvertToDuration( id.High, TimeEpsilon );
        return BaseTimestamp.Add( offset );
    }

    /// <summary>
    /// Calculates maximum possible number of identifiers that this generator can produce in the given time,
    /// without having to resort to <see cref="LowValueOverflowStrategy"/> resolution.
    /// </summary>
    /// <param name="duration">Time to calculate this generator's throughput for.</param>
    /// <returns>Maximum possible number of identifiers that this generator can produce in the given time.</returns>
    [Pure]
    public ulong CalculateThroughput(Duration duration)
    {
        duration = duration.Max( Duration.Zero );
        var fullHighValueCount = ConvertToHighValue( duration, TimeEpsilon );
        var fullLowValueCount = ( ulong )(LowValueBounds.Max - LowValueBounds.Min + 1);
        var lowValueRemainderRatio = ( double )(duration.Ticks % TimeEpsilon.Ticks) / TimeEpsilon.Ticks;
        var remainingLowValueCount = ( ulong )Math.Truncate( lowValueRemainderRatio * fullLowValueCount );
        return fullHighValueCount * fullLowValueCount + remainingLowValueCount;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ulong GetCurrentHighValue()
    {
        var elapsedTime = Timestamps.GetNow() - StartTimestamp;
        return _highValueOffset + ConvertToHighValue( elapsedTime, TimeEpsilon );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ulong GetHighValueBySpinWait()
    {
        var spinWait = new SpinWait();
        var highValue = GetCurrentHighValue();
        while ( highValue <= LastHighValue )
        {
            spinWait.SpinOnce();
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
            Thread.Sleep( ( int )Math.Ceiling( timeout.TotalMilliseconds ) );
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
        return new Identifier( highValue, unchecked( ( ushort )LastLowValue ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Identifier CreateNextId()
    {
        return new Identifier( LastHighValue, unchecked( ( ushort )++LastLowValue ) );
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
        return unchecked( ( ulong )(elapsedTime.Ticks / epsilon.Ticks) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Duration ConvertToDuration(ulong highValue, Duration epsilon)
    {
        return Duration.FromTicks( ( long )highValue * epsilon.Ticks );
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
