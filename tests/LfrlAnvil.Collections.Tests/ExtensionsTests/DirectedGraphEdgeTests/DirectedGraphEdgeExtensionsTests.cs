using LfrlAnvil.Collections.Extensions;

namespace LfrlAnvil.Collections.Tests.ExtensionsTests.DirectedGraphEdgeTests;

public class DirectedGraphEdgeExtensionsTests : TestsBase
{
    [Theory]
    [InlineData( GraphDirection.In, false, true )]
    [InlineData( GraphDirection.Out, true, false )]
    [InlineData( GraphDirection.Both, true, true )]
    public void GetSourceInfo_ShouldReturnCorrectResult(GraphDirection direction, bool canReach, bool canBeReached)
    {
        var graph = new DirectedGraph<string, int, long>();
        var a = graph.AddNode( "a", Fixture.Create<int>() );
        var b = graph.AddNode( "b", Fixture.Create<int>() );
        var sut = graph.AddEdge( "a", "b", Fixture.Create<long>(), direction );

        var result = sut.GetSourceInfo();

        using ( new AssertionScope() )
        {
            result.Edge.Should().BeSameAs( sut );
            result.From.Should().BeSameAs( a );
            result.To.Should().BeSameAs( b );
            result.Direction.Should().Be( direction );
            result.CanReach.Should().Be( canReach );
            result.CanBeReached.Should().Be( canBeReached );
        }
    }

    [Theory]
    [InlineData( GraphDirection.In, true, false )]
    [InlineData( GraphDirection.Out, false, true )]
    [InlineData( GraphDirection.Both, true, true )]
    public void GetTargetInfo_ShouldReturnCorrectResult(GraphDirection direction, bool canReach, bool canBeReached)
    {
        var graph = new DirectedGraph<string, int, long>();
        var a = graph.AddNode( "a", Fixture.Create<int>() );
        var b = graph.AddNode( "b", Fixture.Create<int>() );
        var sut = graph.AddEdge( "a", "b", Fixture.Create<long>(), direction );

        var result = sut.GetTargetInfo();

        using ( new AssertionScope() )
        {
            result.Edge.Should().BeSameAs( sut );
            result.From.Should().BeSameAs( b );
            result.To.Should().BeSameAs( a );
            result.Direction.Should().Be( direction.Invert() );
            result.CanReach.Should().Be( canReach );
            result.CanBeReached.Should().Be( canBeReached );
        }
    }

    [Fact]
    public void GetInfo_ShouldReturnCorrectResult_WhenNodeIsSource()
    {
        var graph = new DirectedGraph<string, int, long>();
        var a = graph.AddNode( "a", Fixture.Create<int>() );
        var b = graph.AddNode( "b", Fixture.Create<int>() );
        var sut = graph.AddEdge( "a", "b", Fixture.Create<long>() );

        var result = sut.GetInfo( a );

        using ( new AssertionScope() )
        {
            result.Should().NotBeNull();
            result?.Edge.Should().BeSameAs( sut );
            result?.From.Should().BeSameAs( a );
            result?.To.Should().BeSameAs( b );
        }
    }

    [Fact]
    public void GetInfo_ShouldReturnCorrectResult_WhenNodeIsTarget()
    {
        var graph = new DirectedGraph<string, int, long>();
        var a = graph.AddNode( "a", Fixture.Create<int>() );
        var b = graph.AddNode( "b", Fixture.Create<int>() );
        var sut = graph.AddEdge( "a", "b", Fixture.Create<long>() );

        var result = sut.GetInfo( b );

        using ( new AssertionScope() )
        {
            result.Should().NotBeNull();
            result?.Edge.Should().BeSameAs( sut );
            result?.From.Should().BeSameAs( b );
            result?.To.Should().BeSameAs( a );
        }
    }

    [Fact]
    public void GetInfo_ShouldReturnNull_WhenNodeIsNeitherSourceNorTarget()
    {
        var graph = new DirectedGraph<string, int, long>();
        graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );
        var c = graph.AddNode( "c", Fixture.Create<int>() );
        var sut = graph.AddEdge( "a", "b", Fixture.Create<long>() );

        var result = sut.GetInfo( c );

        result.Should().BeNull();
    }
}
