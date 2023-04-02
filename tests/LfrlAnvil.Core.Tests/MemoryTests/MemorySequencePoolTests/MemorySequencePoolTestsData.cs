namespace LfrlAnvil.Tests.MemoryTests.MemorySequencePoolTests;

public class MemorySequencePoolTestsData
{
    public static TheoryData<int, int, int[]> GetFirstSequenceData(IFixture fixture)
    {
        return new TheoryData<int, int, int[]>
        {
            { 1, 1, new[] { 1 } },
            { 1, 3, new[] { 1, 1, 1 } },
            { 16, 1, new[] { 1 } },
            { 16, 15, new[] { 15 } },
            { 16, 16, new[] { 16 } },
            { 16, 17, new[] { 16, 1 } },
            { 32, 50, new[] { 32, 18 } },
            { 32, 63, new[] { 32, 31 } },
            { 32, 64, new[] { 32, 32 } },
            { 32, 80, new[] { 32, 32, 16 } },
            { 32, 96, new[] { 32, 32, 32 } },
            { 32, 97, new[] { 32, 32, 32, 1 } }
        };
    }

    public static TheoryData<int, int, int, int[]> GetSecondSequenceData(IFixture fixture)
    {
        return new TheoryData<int, int, int, int[]>
        {
            { 1, 1, 3, new[] { 1, 1, 1 } },
            { 16, 1, 1, new[] { 1 } },
            { 16, 1, 15, new[] { 15 } },
            { 16, 1, 16, new[] { 15, 1 } },
            { 16, 15, 1, new[] { 1 } },
            { 16, 15, 2, new[] { 1, 1 } },
            { 16, 15, 16, new[] { 1, 15 } },
            { 16, 16, 1, new[] { 1 } },
            { 16, 16, 15, new[] { 15 } },
            { 16, 16, 16, new[] { 16 } },
            { 16, 17, 15, new[] { 15 } },
            { 16, 17, 17, new[] { 15, 2 } },
            { 32, 50, 10, new[] { 10 } },
            { 32, 50, 20, new[] { 14, 6 } },
            { 32, 50, 50, new[] { 14, 32, 4 } },
            { 32, 64, 64, new[] { 32, 32 } },
            { 32, 60, 80, new[] { 4, 32, 32, 12 } }
        };
    }

    public static TheoryData<int, int, int, int> GetReuseReturnedTailSequenceData(IFixture fixture)
    {
        return new TheoryData<int, int, int, int>
        {
            { 8, 8, 8, 8 },
            { 8, 8, 9, 9 },
            { 8, 8, 7, 7 },
            { 8, 8, 9, 8 },
            { 8, 8, 7, 8 },
            { 16, 15, 16, 16 },
            { 16, 15, 17, 17 },
            { 16, 15, 15, 15 },
            { 16, 15, 17, 16 },
            { 16, 15, 15, 16 },
            { 32, 33, 32, 32 },
            { 32, 33, 33, 33 },
            { 32, 33, 31, 31 },
            { 32, 33, 33, 32 },
            { 32, 33, 31, 32 },
            { 32, 16, 96, 8 },
            { 32, 16, 96, 160 }
        };
    }
}
