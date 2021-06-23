using AutoFixture;

namespace LfrlSoft.NET.TestExtensions
{
    public abstract class TestsBase
    {
        protected readonly IFixture Fixture = new Fixture();
    }
}
