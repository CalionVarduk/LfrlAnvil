using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Process.Tests.ProcessRunnerTests
{
    public class ProcessRunnerTests : TestsBase
    {
        [Fact]
        public void Run_ShouldCallProcessHandler_WhenHandlerExists()
        {
            var args = new TestProcessArgs();
            var expectedResult = Fixture.Create<int>();
            var handler = Substitute.For<IProcessHandler<TestProcessArgs, int>>();
            handler.Handle( args ).Returns( _ => expectedResult );

            var factory = Substitute.For<IProcessHandlerFactory>();
            factory.TryCreate<TestProcessArgs, int>().Returns( _ => handler );

            var sut = new ProcessRunner( factory );

            var result = sut.Run<TestProcessArgs, int>( args );

            using ( new AssertionScope() )
            {
                result.Should().Be( expectedResult );
                handler.Received().Handle( args );
            }
        }

        [Fact]
        public void Run_ShouldThrowMissingProcessHandlerException_WhenHandlerDoesNotExist()
        {
            var factory = Substitute.For<IProcessHandlerFactory>();
            factory.TryCreate<TestProcessArgs, int>().Returns( _ => null );
            var sut = new ProcessRunner( factory );

            var action = Lambda.Of( () => sut.Run<TestProcessArgs, int>( new TestProcessArgs() ) );

            using ( new AssertionScope() )
            {
                var exception = action.Should().ThrowExactly<MissingProcessHandlerException>().And;
                exception.ArgsType.Should().Be( typeof( TestProcessArgs ) );
            }
        }

        [Fact]
        public void TryRun_ShouldCallProcessHandler_WhenHandlerExists()
        {
            var args = new TestProcessArgs();
            var expectedResult = Fixture.Create<int>();
            var handler = Substitute.For<IProcessHandler<TestProcessArgs, int>>();
            handler.Handle( args ).Returns( _ => expectedResult );

            var factory = Substitute.For<IProcessHandlerFactory>();
            factory.TryCreate<TestProcessArgs, int>().Returns( _ => handler );

            var sut = new ProcessRunner( factory );

            var result = sut.TryRun( args, out int outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.Should().Be( expectedResult );
                handler.Received().Handle( args );
            }
        }

        [Fact]
        public void TryRun_ShouldReturnFalse_WhenHandlerDoesNotExist()
        {
            var factory = Substitute.For<IProcessHandlerFactory>();
            factory.TryCreate<TestProcessArgs, int>().Returns( _ => null );
            var sut = new ProcessRunner( factory );

            var result = sut.TryRun( new TestProcessArgs(), out int outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().Be( default );
            }
        }

        [Fact]
        public async ValueTask RunAsync_ShouldCallProcessHandler_WhenHandlerExists()
        {
            var args = new TestProcessArgs();
            var expectedResult = Fixture.Create<int>();
            var token = new CancellationToken();
            var handler = Substitute.For<IAsyncProcessHandler<TestProcessArgs, int>>();
            handler.Handle( args, Arg.Any<CancellationToken>() ).Returns( _ => expectedResult );

            var factory = Substitute.For<IProcessHandlerFactory>();
            factory.TryCreateAsync<TestProcessArgs, int>().Returns( _ => handler );

            var sut = new ProcessRunner( factory );

            var result = await sut.RunAsync<TestProcessArgs, int>( args, token );

            using ( new AssertionScope() )
            {
                result.Should().Be( expectedResult );
                handler.Received().Handle( args, token );
            }
        }

        [Fact]
        public void RunAsync_ShouldThrowMissingProcessHandlerException_WhenHandlerDoesNotExist()
        {
            var factory = Substitute.For<IProcessHandlerFactory>();
            factory.TryCreateAsync<TestProcessArgs, int>().Returns( _ => null );
            var sut = new ProcessRunner( factory );

            var action = Lambda.Of( () => sut.RunAsync<TestProcessArgs, int>( new TestProcessArgs(), CancellationToken.None ) );

            using ( new AssertionScope() )
            {
                var exception = action.Should().ThrowExactly<MissingProcessHandlerException>().And;
                exception.ArgsType.Should().Be( typeof( TestProcessArgs ) );
            }
        }

        [Fact]
        public async ValueTask TryRunAsync_ShouldCallProcessHandler_WhenHandlerExists()
        {
            var args = new TestProcessArgs();
            var expectedResult = Fixture.Create<int>();
            var token = new CancellationToken();
            var handler = Substitute.For<IAsyncProcessHandler<TestProcessArgs, int>>();
            handler.Handle( args, Arg.Any<CancellationToken>() ).Returns( _ => expectedResult );

            var factory = Substitute.For<IProcessHandlerFactory>();
            factory.TryCreateAsync<TestProcessArgs, int>().Returns( _ => handler );

            var sut = new ProcessRunner( factory );

            var result = sut.TryRunAsync<TestProcessArgs, int>( args, token );
            var outResult = result is null ? (int?)null : (await result.Value);

            using ( new AssertionScope() )
            {
                outResult.Should().Be( expectedResult );
                handler.Received().Handle( args, token );
            }
        }

        [Fact]
        public void TryRunAsync_ShouldReturnFalse_WhenHandlerDoesNotExist()
        {
            var factory = Substitute.For<IProcessHandlerFactory>();
            factory.TryCreateAsync<TestProcessArgs, int>().Returns( _ => null );
            var sut = new ProcessRunner( factory );

            var result = sut.TryRunAsync<TestProcessArgs, int>( new TestProcessArgs(), CancellationToken.None );

            result.Should().BeNull();
        }
    }
}
