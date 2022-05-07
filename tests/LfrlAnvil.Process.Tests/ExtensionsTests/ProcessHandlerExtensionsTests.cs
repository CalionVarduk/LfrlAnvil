using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Process.Extensions;
using LfrlAnvil.TestExtensions;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Process.Tests.ExtensionsTests
{
    public class ProcessHandlerExtensionsTests : TestsBase
    {
        [Fact]
        public void ToSynchronous_ShouldReturnCorrectResult()
        {
            var args = new TestProcessArgs();
            var expectedResult = Fixture.Create<int>();
            var @base = Substitute.For<IAsyncProcessHandler<TestProcessArgs, int>>();
            @base.Handle( args, Arg.Any<CancellationToken>() ).Returns( _ => expectedResult );

            var sut = @base.ToSynchronous();
            var result = sut.Handle( args );

            using ( new AssertionScope() )
            {
                result.Should().Be( expectedResult );
                @base.Received().Handle( args, CancellationToken.None );
            }
        }

        [Fact]
        public async ValueTask ToAsynchronous_ShouldReturnCorrectResult()
        {
            var args = new TestProcessArgs();
            var expectedResult = Fixture.Create<int>();
            var @base = Substitute.For<IProcessHandler<TestProcessArgs, int>>();
            @base.Handle( args ).Returns( _ => expectedResult );

            var sut = @base.ToAsynchronous();
            var result = await sut.Handle( args, CancellationToken.None );

            using ( new AssertionScope() )
            {
                result.Should().Be( expectedResult );
                @base.Received().Handle( args );
            }
        }
    }
}
