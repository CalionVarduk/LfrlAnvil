namespace LfrlAnvil.Tests.NumericsTests.MathUtilsTests;

public class MathUtilsTestsData
{
    public static TheoryData<ulong, int, ulong[]> GetPartitionData(IFixture fixture)
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
}
