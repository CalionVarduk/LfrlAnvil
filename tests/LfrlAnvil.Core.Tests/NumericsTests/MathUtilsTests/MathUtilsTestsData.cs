using LfrlAnvil.Numerics;

namespace LfrlAnvil.Tests.NumericsTests.MathUtilsTests;

public class MathUtilsTestsData
{
    public static TheoryData<ulong, ulong, ulong> GetGcdData(IFixture fixture)
    {
        return new TheoryData<ulong, ulong, ulong>
        {
            { 0, 0, 0 },
            { 123, 0, 123 },
            { 0, 123, 123 },
            { 123, 123, 123 },
            { 123, 246, 123 },
            { 246, 123, 123 },
            { 13, 7, 1 },
            { 4, 22, 2 },
            { 26, 39, 13 }
        };
    }

    public static TheoryData<ulong, ulong, ulong> GetLcmData(IFixture fixture)
    {
        return new TheoryData<ulong, ulong, ulong>
        {
            { 123, 0, 0 },
            { 0, 123, 0 },
            { 123, 123, 123 },
            { 123, 246, 246 },
            { 246, 123, 246 },
            { 13, 7, 91 },
            { 4, 22, 44 },
            { 26, 39, 78 }
        };
    }

    public static TheoryData<Percent[], Fraction, Fraction[]> GetConvertToFractionsData(IFixture fixture)
    {
        return new TheoryData<Percent[], Fraction, Fraction[]>
        {
            { Array.Empty<Percent>(), new Fraction( 0, 1 ), Array.Empty<Fraction>() },
            { Array.Empty<Percent>(), new Fraction( 1000, 1000 ), Array.Empty<Fraction>() },
            {
                new[]
                {
                    Percent.Normalize( 50 ),
                    Percent.Normalize( 50 )
                },
                new Fraction( 0, 50 ),
                new[]
                {
                    new Fraction( 0, 50 ),
                    new Fraction( 0, 50 )
                }
            },
            { new[] { Percent.Normalize( 100 ) }, new Fraction( 100, 100 ), new[] { new Fraction( 100, 100 ) } },
            { new[] { Percent.Normalize( 50 ) }, new Fraction( 2000, 1000 ), new[] { new Fraction( 2000, 1000 ) } },
            { new[] { Percent.Normalize( 150 ) }, new Fraction( 500, 1000 ), new[] { new Fraction( 500, 1000 ) } },
            {
                new[]
                {
                    Percent.Normalize( 100m / 3 ),
                    Percent.Normalize( 100m / 3 ),
                    Percent.Normalize( 100m / 3 )
                },
                new Fraction( 100, 100 ),
                new[]
                {
                    new Fraction( 33, 100 ),
                    new Fraction( 33, 100 ),
                    new Fraction( 34, 100 )
                }
            },
            {
                new[]
                {
                    Percent.Normalize( 100m / 3 ),
                    Percent.Normalize( 100m / 3 ),
                    Percent.Normalize( 100m / 3 )
                },
                new Fraction( 10000, 10000 ),
                new[]
                {
                    new Fraction( 3333, 10000 ),
                    new Fraction( 3333, 10000 ),
                    new Fraction( 3334, 10000 )
                }
            },
            {
                new[]
                {
                    Percent.Normalize( 100m / 3 ),
                    Percent.Normalize( 100m / 3 ),
                    Percent.Normalize( 100m / 3 ),
                    Percent.Normalize( 100m / 3 ),
                    Percent.Normalize( 100m / 3 ),
                    Percent.Normalize( 100m / 3 )
                },
                new Fraction( 100000, 100000 ),
                new[]
                {
                    new Fraction( 16666, 100000 ),
                    new Fraction( 16667, 100000 ),
                    new Fraction( 16667, 100000 ),
                    new Fraction( 16666, 100000 ),
                    new Fraction( 16667, 100000 ),
                    new Fraction( 16667, 100000 )
                }
            },
            {
                new[]
                {
                    Percent.Normalize( 200m / 3 ),
                    Percent.Normalize( 100m / 3 ),
                    Percent.Normalize( 100m / 6 ),
                    Percent.Normalize( 100m / 3 ),
                    Percent.Normalize( 50 )
                },
                new Fraction( 2000000, 2000000 ),
                new[]
                {
                    new Fraction( 666667, 2000000 ),
                    new Fraction( 333333, 2000000 ),
                    new Fraction( 166667, 2000000 ),
                    new Fraction( 333333, 2000000 ),
                    new Fraction( 500000, 2000000 )
                }
            },
            {
                new[]
                {
                    Percent.Normalize( 200m / 3 ),
                    Percent.Normalize( 100m / 3 ),
                    Percent.Normalize( 100m / 6 ),
                    Percent.Normalize( 100m / 3 ),
                    Percent.Normalize( 50 )
                },
                new Fraction( 200000, 50000 ),
                new[]
                {
                    new Fraction( 66667, 50000 ),
                    new Fraction( 33333, 50000 ),
                    new Fraction( 16667, 50000 ),
                    new Fraction( 33333, 50000 ),
                    new Fraction( 50000, 50000 )
                }
            }
        };
    }
}
