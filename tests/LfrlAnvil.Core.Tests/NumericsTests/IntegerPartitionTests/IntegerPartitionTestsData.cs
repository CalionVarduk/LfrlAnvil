using LfrlAnvil.Numerics;

namespace LfrlAnvil.Tests.NumericsTests.IntegerPartitionTests;

public class IntegerPartitionTestsData
{
    public static TheoryData<ulong, int, ulong[]> GetFixedData(IFixture fixture)
    {
        return new TheoryData<ulong, int, ulong[]>
        {
            { 0, 1, new[] { 0UL } },
            { 999, 1, new[] { 999UL } },
            { 7, 3, new[] { 2UL, 2UL, 3UL } },
            { 8, 3, new[] { 2UL, 3UL, 3UL } },
            { 9, 3, new[] { 3UL, 3UL, 3UL } },
            { 1000, 3, new[] { 333UL, 333UL, 334UL } },
            { 1000, 6, new[] { 166UL, 167UL, 167UL, 166UL, 167UL, 167UL } },
            { 1000, 5, new[] { 200UL, 200UL, 200UL, 200UL, 200UL } }
        };
    }

    public static TheoryData<ulong, Fraction[], ulong[]> GetFractionData(IFixture fixture)
    {
        return new TheoryData<ulong, Fraction[], ulong[]>
        {
            { 0, new[] { Fraction.One, Fraction.One, Fraction.One, }, new[] { 0UL, 0UL, 0UL } },
            { 999, new[] { new Fraction( 1, 3 ), new Fraction( 1, 3 ), new Fraction( 1, 3 ) }, new[] { 333UL, 333UL, 333UL } },
            { 999, new[] { Fraction.One, Fraction.One, Fraction.One, }, new[] { 999UL, 999UL, 999UL } },
            { 7, new[] { Fraction.Zero, new Fraction( 1, 2 ), new Fraction( 1, 2 ) }, new[] { 0UL, 3UL, 4UL } },
            { 7, new[] { new Fraction( 1, 2 ), Fraction.Zero, new Fraction( 1, 2 ) }, new[] { 3UL, 0UL, 4UL } },
            { 7, new[] { new Fraction( 1, 2 ), new Fraction( 1, 2 ), Fraction.Zero }, new[] { 3UL, 4UL, 0UL } },
            {
                1000,
                new[]
                {
                    new Fraction( 1, 8 ),
                    new Fraction( 1, 4 ),
                    new Fraction( 1, 8 ),
                    new Fraction( 3, 8 ),
                    new Fraction( 1, 8 )
                },
                new[] { 125UL, 250UL, 125UL, 375UL, 125UL }
            },
            { 100, new[] { new Fraction( 1, 3 ), new Fraction( 1, 4 ) }, new[] { 33UL, 25UL } },
            { 122, new[] { Fraction.One, new Fraction( 1, 2 ) }, new[] { 122UL, 61UL } },
            { 122, new[] { new Fraction( 1, 2 ), Fraction.One }, new[] { 61UL, 122UL } },
            { 123, new[] { Fraction.One, new Fraction( 1, 2 ) }, new[] { 122UL, 62UL } },
            { 123, new[] { new Fraction( 1, 2 ), Fraction.One }, new[] { 61UL, 123UL } },
            {
                1234,
                new[]
                {
                    new Fraction( 1, 4 ),
                    new Fraction( 1, 8 ),
                    new Fraction( 2, 5 ),
                    new Fraction( 2, 16 ),
                    new Fraction( 4, 40 )
                },
                new[] { 308UL, 154UL, 494UL, 154UL, 124UL }
            }
        };
    }
}
