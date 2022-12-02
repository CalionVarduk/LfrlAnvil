using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Diagnostics;

public readonly struct TimeMeasurement
{
    public static readonly TimeMeasurement Zero = new TimeMeasurement();

    public TimeMeasurement(TimeSpan preparation, TimeSpan invocation, TimeSpan teardown)
    {
        Preparation = preparation;
        Invocation = invocation;
        Teardown = teardown;
    }

    public TimeSpan Preparation { get; }
    public TimeSpan Invocation { get; }
    public TimeSpan Teardown { get; }
    public TimeSpan Total => Preparation + Invocation + Teardown;

    [Pure]
    public override string ToString()
    {
        var preparation = $"{nameof( Preparation )}: {Stringify( Preparation )}";
        var invocation = $"{nameof( Invocation )}: {Stringify( Invocation )}";
        var teardown = $"{nameof( Teardown )}: {Stringify( Teardown )}";
        var total = $"{nameof( Total )}: {Stringify( Total )}";
        return $"{preparation}, {invocation}, {teardown} ({total})";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static string Stringify(TimeSpan timeSpan)
    {
        return $"{timeSpan.TotalSeconds.ToString( "N7", CultureInfo.InvariantCulture )}s";
    }
}
