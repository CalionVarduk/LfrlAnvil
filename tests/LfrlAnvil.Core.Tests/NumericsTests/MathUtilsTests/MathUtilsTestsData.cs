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
}
