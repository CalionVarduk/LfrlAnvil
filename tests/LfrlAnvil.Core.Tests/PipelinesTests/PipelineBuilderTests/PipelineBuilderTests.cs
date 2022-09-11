using System.Linq;
using LfrlAnvil.Pipelines;

namespace LfrlAnvil.Tests.PipelinesTests.PipelineBuilderTests;

public class PipelineBuilderTests : TestsBase
{
    [Fact]
    public void Build_ShouldReturnCorrectPipeline()
    {
        var processors = new[]
        {
            PipelineProcessor.Create<int, int>( c => c.SetResult( c.Args + c.Result ) ),
            PipelineProcessor.Create<int, int>( c => c.SetResult( c.Result + 2 ) ),
            PipelineProcessor.Create<int, int>( c => c.SetResult( c.Result + 3 ) )
        };

        var sut = new PipelineBuilder<int, int>( defaultResult: 0 )
            .Add( processors[0] )
            .Add( processors.Skip( 1 ) )
            .SetDefaultResult( 1 );

        var pipeline = sut.Build();

        var result = pipeline.Invoke( 100 );

        result.Should().Be( 106 );
    }
}
