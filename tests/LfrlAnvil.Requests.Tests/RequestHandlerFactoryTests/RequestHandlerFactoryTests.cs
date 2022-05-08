using FluentAssertions;
using LfrlAnvil.TestExtensions;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Requests.Tests.RequestHandlerFactoryTests
{
    public class RequestHandlerFactoryTests : TestsBase
    {
        [Fact]
        public void TryCreate_ShouldReturnNull_WhenFactoryDoesNotExist()
        {
            var sut = new RequestHandlerFactory();
            var result = sut.TryCreate<TestRequestClass, int>();
            result.Should().BeNull();
        }

        [Fact]
        public void TryCreate_ShouldReturnCorrectResult_WhenFactoryExists()
        {
            var expected = Substitute.For<IRequestHandler<TestRequestClass, int>>();
            var sut = new RequestHandlerFactory().Register( () => expected );

            var result = sut.TryCreate<TestRequestClass, int>();

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void TryCreate_ShouldUseLatestFactory_WhenFactoryHasBeenRegisteredMoreThanOnce()
        {
            var expected = Substitute.For<IRequestHandler<TestRequestClass, int>>();
            var sut = new RequestHandlerFactory()
                .Register( () => Substitute.For<IRequestHandler<TestRequestClass, int>>() )
                .Register( () => expected );

            var result = sut.TryCreate<TestRequestClass, int>();

            result.Should().BeSameAs( expected );
        }
    }
}
