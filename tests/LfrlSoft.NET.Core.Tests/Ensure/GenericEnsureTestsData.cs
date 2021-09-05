using AutoFixture;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Ensure
{
    public class GenericEnsureTestsData<T>
    {
        public static TheoryData<int> GetContainsAtLeastPassData(IFixture fixture)
        {
            return new()
            {
                -1,
                0,
                1,
                2,
                3
            };
        }

        public static TheoryData<int> GetContainsAtLeastThrowData(IFixture fixture)
        {
            return new()
            {
                4,
                5
            };
        }

        public static TheoryData<int> GetContainsAtMostPassData(IFixture fixture)
        {
            return new()
            {
                3,
                4,
                5
            };
        }

        public static TheoryData<int> GetContainsAtMostThrowData(IFixture fixture)
        {
            return new()
            {
                -1,
                0,
                1,
                2
            };
        }

        public static TheoryData<int> GetContainsExactlyThrowData(IFixture fixture)
        {
            return new()
            {
                2,
                4
            };
        }

        public static TheoryData<int, int> GetContainsBetweenPassData(IFixture fixture)
        {
            return new()
            {
                { 0, 3 },
                { 1, 3 },
                { 2, 3 },
                { 3, 3 },
                { 0, 4 },
                { 1, 4 },
                { 2, 4 },
                { 3, 4 },
                { 3, 5 },
                { 3, 6 }
            };
        }

        public static TheoryData<int, int> GetContainsBetweenThrowData(IFixture fixture)
        {
            return new()
            {
                { 0, 2 },
                { 1, 2 },
                { 2, 2 },
                { 4, 4 },
                { 4, 5 },
                { 4, 6 },
                { 3, 2 }
            };
        }
    }
}
