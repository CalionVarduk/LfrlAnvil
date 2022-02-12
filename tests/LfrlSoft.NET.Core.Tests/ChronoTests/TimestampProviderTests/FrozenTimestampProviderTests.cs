using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlSoft.NET.Core.Chrono;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.ChronoTests.TimestampProviderTests
{
    public class FrozenTimestampProviderTests : TestsBase
    {
        [Fact]
        public void GetNow_ShouldReturnCorrectResult()
        {
            var expected = new Timestamp( Fixture.Create<int>() );
            var sut = new FrozenTimestampProvider( expected );

            var firstResult = sut.GetNow();
            Task.Delay( TimeSpan.FromMilliseconds( 1 ) ).Wait();
            var secondResult = sut.GetNow();

            using ( new AssertionScope() )
            {
                firstResult.Should().Be( expected );
                secondResult.Should().Be( expected );
            }
        }
    }
}
