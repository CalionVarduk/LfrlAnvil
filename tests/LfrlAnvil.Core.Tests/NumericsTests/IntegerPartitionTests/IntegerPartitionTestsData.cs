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
}
