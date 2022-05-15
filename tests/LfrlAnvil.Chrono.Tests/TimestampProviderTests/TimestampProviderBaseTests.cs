using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Chrono.Internal;
using LfrlAnvil.Generators;
using LfrlAnvil.TestExtensions;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Chrono.Tests.TimestampProviderTests
{
    public class TimestampProviderBaseTests : TestsBase
    {
        [Fact]
        public void IGenericGeneratorGenerate_ShouldBeEquivalentToGetNow()
        {
            var expected = new Timestamp( Fixture.Create<int>() );
            var source = Substitute.For<TimestampProviderBase>();
            source.GetNow().Returns( expected );
            IGenerator<Timestamp> sut = source;

            var result = sut.Generate();

            result.Should().Be( expected );
        }

        [Fact]
        public void IGenericGeneratorTryGenerate_ShouldBeEquivalentToGetNow()
        {
            var expected = new Timestamp( Fixture.Create<int>() );
            var source = Substitute.For<TimestampProviderBase>();
            source.GetNow().Returns( expected );
            IGenerator<Timestamp> sut = source;

            var result = sut.TryGenerate( out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.Should().Be( expected );
            }
        }

        [Fact]
        public void IGeneratorGenerate_ShouldBeEquivalentToGetNow()
        {
            var expected = new Timestamp( Fixture.Create<int>() );
            var source = Substitute.For<TimestampProviderBase>();
            source.GetNow().Returns( expected );
            IGenerator sut = source;

            var result = sut.Generate();

            result.Should().Be( expected );
        }

        [Fact]
        public void IGeneratorTryGenerate_ShouldBeEquivalentToGetNow()
        {
            var expected = new Timestamp( Fixture.Create<int>() );
            var source = Substitute.For<TimestampProviderBase>();
            source.GetNow().Returns( expected );
            IGenerator sut = source;

            var result = sut.TryGenerate( out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.Should().Be( expected );
            }
        }
    }
}
