using System;
using AutoFixture;

namespace LfrlSoft.NET.TestExtensions
{
    public abstract class TestsBase
    {
        protected readonly IFixture Fixture = new Fixture();

        protected ArrangedTest<TTestData> Arrange<TTestData>(Func<TTestData> testDataProvider)
        {
            return new ArrangedTest<TTestData>( testDataProvider );
        }
    }
}
