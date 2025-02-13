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

        Assertion.All(
                result.Edge.TestRefEquals( sut ),
                result.From.TestRefEquals( a ),
                result.To.TestRefEquals( b ),
                result.Direction.TestEquals( direction ),
                result.CanReach.TestEquals( canReach ),
                result.CanBeReached.TestEquals( canBeReached ) )
            .Go();
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

        Assertion.All(
                result.Edge.TestRefEquals( sut ),
                result.From.TestRefEquals( b ),
                result.To.TestRefEquals( a ),
                result.Direction.TestEquals( direction.Invert() ),
                result.CanReach.TestEquals( canReach ),
                result.CanBeReached.TestEquals( canBeReached ) )
            .Go();
    }

    [Fact]
    public void GetInfo_ShouldReturnCorrectResult_WhenNodeIsSource()
    {
        var graph = new DirectedGraph<string, int, long>();
        var a = graph.AddNode( "a", Fixture.Create<int>() );
        var b = graph.AddNode( "b", Fixture.Create<int>() );
        var sut = graph.AddEdge( "a", "b", Fixture.Create<long>() );

        var result = sut.GetInfo( a );

        result.TestNotNull(
                r => Assertion.All(
                    "result",
                    r.Edge.TestRefEquals( sut ),
                    r.From.TestRefEquals( a ),
                    r.To.TestRefEquals( b ) ) )
            .Go();
    }

    [Fact]
    public void GetInfo_ShouldReturnCorrectResult_WhenNodeIsTarget()
    {
        var graph = new DirectedGraph<string, int, long>();
        var a = graph.AddNode( "a", Fixture.Create<int>() );
        var b = graph.AddNode( "b", Fixture.Create<int>() );
        var sut = graph.AddEdge( "a", "b", Fixture.Create<long>() );

        var result = sut.GetInfo( b );

        result.TestNotNull(
                r => Assertion.All(
                    "result",
                    r.Edge.TestRefEquals( sut ),
                    r.From.TestRefEquals( b ),
                    r.To.TestRefEquals( a ) ) )
            .Go();
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

        result.TestNull().Go();
    }
}
