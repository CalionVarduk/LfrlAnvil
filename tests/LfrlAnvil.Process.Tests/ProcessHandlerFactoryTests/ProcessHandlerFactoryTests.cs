using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Process.Internal;
using LfrlAnvil.TestExtensions;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Process.Tests.ProcessHandlerFactoryTests
{
    public class ProcessHandlerFactoryTests : TestsBase
    {
        [Fact]
        public void TryCreate_ShouldReturnNull_WhenFactoryDoesNotExist()
        {
            var sut = new ProcessHandlerFactory();
            var result = sut.TryCreate<TestProcessArgs, int>();
            result.Should().BeNull();
        }

        [Fact]
        public void TryCreate_ShouldReturnCorrectResult_WhenSynchronousFactoryExists()
        {
            var expected = Substitute.For<IProcessHandler<TestProcessArgs, int>>();
            var sut = new ProcessHandlerFactory().Register( () => expected );

            var result = sut.TryCreate<TestProcessArgs, int>();

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void TryCreate_ShouldReturnCorrectResult_WhenAsynchronousFactoryExists()
        {
            var expected = Substitute.For<IAsyncProcessHandler<TestProcessArgs, int>>();
            var sut = new ProcessHandlerFactory().Register( () => expected );

            var result = sut.TryCreate<TestProcessArgs, int>();

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( expected );
                result.Should().BeOfType<ForcedSyncProcessHandler<TestProcessArgs, int>>();
            }
        }

        [Fact]
        public void TryCreate_ShouldUseLatestFactory_WhenFactoryHasBeenRegisteredMoreThanOnce()
        {
            var expected = Substitute.For<IProcessHandler<TestProcessArgs, int>>();
            var sut = new ProcessHandlerFactory()
                .Register( () => Substitute.For<IAsyncProcessHandler<TestProcessArgs, int>>() )
                .Register( () => expected );

            var result = sut.TryCreate<TestProcessArgs, int>();

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void TryCreateAsync_ShouldReturnNull_WhenFactoryDoesNotExist()
        {
            var sut = new ProcessHandlerFactory();
            var result = sut.TryCreateAsync<TestProcessArgs, int>();
            result.Should().BeNull();
        }

        [Fact]
        public void TryCreateAsync_ShouldReturnCorrectResult_WhenAsynchronousFactoryExists()
        {
            var expected = Substitute.For<IAsyncProcessHandler<TestProcessArgs, int>>();
            var sut = new ProcessHandlerFactory().Register( () => expected );

            var result = sut.TryCreateAsync<TestProcessArgs, int>();

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void TryCreateAsync_ShouldReturnCorrectResult_WhenSynchronousFactoryExists()
        {
            var expected = Substitute.For<IProcessHandler<TestProcessArgs, int>>();
            var sut = new ProcessHandlerFactory().Register( () => expected );

            var result = sut.TryCreateAsync<TestProcessArgs, int>();

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( expected );
                result.Should().BeOfType<ForcedAsyncProcessHandler<TestProcessArgs, int>>();
            }
        }

        [Fact]
        public void TryCreateAsync_ShouldUseLatestFactory_WhenFactoryHasBeenRegisteredMoreThanOnce()
        {
            var expected = Substitute.For<IAsyncProcessHandler<TestProcessArgs, int>>();
            var sut = new ProcessHandlerFactory()
                .Register( () => Substitute.For<IProcessHandler<TestProcessArgs, int>>() )
                .Register( () => expected );

            var result = sut.TryCreateAsync<TestProcessArgs, int>();

            result.Should().BeSameAs( expected );
        }
    }
}
