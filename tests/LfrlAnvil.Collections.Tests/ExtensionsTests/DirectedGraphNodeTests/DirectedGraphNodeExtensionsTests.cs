using LfrlAnvil.Collections.Extensions;

namespace LfrlAnvil.Collections.Tests.ExtensionsTests.DirectedGraphNodeTests;

public class DirectedGraphNodeExtensionsTests : TestsBase
{
    [Fact]
    public void IsRoot_ShouldReturnTrue_WhenNodeDoesNotContainAnyEdges()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );

        var result = sut.IsRoot();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsRoot_ShouldReturnTrue_WhenNodeContainsEdgesAndNoneOfThemHasIncomingDirection()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );
        graph.AddNode( "c", Fixture.Create<int>() );
        graph.AddEdge( "a", "b", Fixture.Create<long>(), GraphDirection.Out );
        graph.AddEdge( "c", "a", Fixture.Create<long>(), GraphDirection.In );

        var result = sut.IsRoot();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsRoot_ShouldReturnFalse_WhenNodeContainsEdgeToSelf()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        sut.AddEdgeTo( sut.Key, Fixture.Create<long>() );

        var result = sut.IsRoot();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsRoot_ShouldReturnFalse_WhenNodeContainsEdgesAndAtLeastOneOfThemHasIncomingDirection()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );
        graph.AddNode( "c", Fixture.Create<int>() );
        graph.AddEdge( "a", "b", Fixture.Create<long>(), GraphDirection.Out );
        graph.AddEdge( "c", "a", Fixture.Create<long>(), GraphDirection.Out );

        var result = sut.IsRoot();

        result.Should().BeFalse();
    }
}
