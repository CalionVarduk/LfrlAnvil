using System.Threading.Tasks;
using LfrlAnvil.Pipelines;

namespace LfrlAnvil.Tests.PipelinesTests.PipelineTests;

public class PipelineTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldReturnCorrectResult()
    {
        var defaultResult = Fixture.Create<int>();
        var sut = new Pipeline<string, int>( Array.Empty<IPipelineProcessor<string, int>>(), defaultResult );
        sut.DefaultResult.Should().Be( defaultResult );
    }

    [Fact]
    public void Invoke_ShouldReturnDefaultResult_WhenPipelineDoesNotHaveAnyProcessors()
    {
        var args = Fixture.Create<string>();
        var defaultResult = Fixture.Create<int>();
        var sut = new Pipeline<string, int>( Array.Empty<IPipelineProcessor<string, int>>(), defaultResult );

        var result = sut.Invoke( args );

        result.Should().Be( defaultResult );
    }

    [Fact]
    public void Invoke_ShouldReturnCorrectResult_WhenPipelineHasProcessorsAndNonOfThemCompleteTheContext()
    {
        var processors = new[]
        {
            PipelineProcessor.Create<int, int>( c => c.SetResult( c.Args + c.Result ) ),
            PipelineProcessor.Create<int, int>( c => c.SetResult( c.Result + 2 ) ),
            PipelineProcessor.Create<int, int>( c => c.SetResult( c.Result + 3 ) )
        };

        var sut = new Pipeline<int, int>( processors, defaultResult: 1 );

        var result = sut.Invoke( 100 );

        result.Should().Be( 106 );
    }

    [Fact]
    public void Invoke_ShouldReturnCorrectResult_WhenPipelineHasProcessorsAndOneOfThemCompletesTheContext()
    {
        var processors = new[]
        {
            PipelineProcessor.Create<int, int>(
                c =>
                {
                    c.SetResult( c.Args + c.Result );
                    c.Complete();
                } ),
            PipelineProcessor.Create<int, int>( c => c.SetResult( c.Result + 2 ) ),
            PipelineProcessor.Create<int, int>( c => c.SetResult( c.Result + 3 ) )
        };

        var sut = new Pipeline<int, int>( processors, defaultResult: 1 );

        var result = sut.Invoke( 100 );

        result.Should().Be( 101 );
    }

    [Fact]
    public async Task Invoke_ShouldReturnCorrectResult_WhenPipelineHasProcessorsAndReturnsTaskBuiltWithContinueWith()
    {
        var processors = new[]
        {
            PipelineProcessor.Create<int, Task<int>>( c => c.SetResult( c.Result.ContinueWith( t => c.Args + t.Result ) ) ),
            PipelineProcessor.Create<int, Task<int>>( c => c.SetResult( c.Result.ContinueWith( t => t.Result + 2 ) ) ),
            PipelineProcessor.Create<int, Task<int>>( c => c.SetResult( c.Result.ContinueWith( t => t.Result + 3 ) ) )
        };

        var sut = new Pipeline<int, Task<int>>( processors, defaultResult: Task.FromResult( 1 ) );

        var result = await sut.Invoke( 100 );

        result.Should().Be( 106 );
    }

    [Fact]
    public async Task Invoke_ShouldReturnCorrectResult_WhenPipelineHasProcessorsAndReturnsTaskBuiltWithAwaitAsync()
    {
        var processors = new[]
        {
            PipelineProcessor.Create<int, Task<int>>(
                c =>
                {
                    var factory = async (Task<int> t) => c.Args + await t;
                    c.SetResult( factory( c.Result ) );
                } ),
            PipelineProcessor.Create<int, Task<int>>(
                c =>
                {
                    var factory = async (Task<int> t) => await t + 2;
                    c.SetResult( factory( c.Result ) );
                } ),
            PipelineProcessor.Create<int, Task<int>>(
                c =>
                {
                    var factory = async (Task<int> t) => await t + 3;
                    c.SetResult( factory( c.Result ) );
                } )
        };

        var sut = new Pipeline<int, Task<int>>( processors, defaultResult: Task.FromResult( 1 ) );

        var result = await sut.Invoke( 100 );

        result.Should().Be( 106 );
    }
}
