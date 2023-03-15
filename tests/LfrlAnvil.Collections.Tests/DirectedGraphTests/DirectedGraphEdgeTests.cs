using LfrlAnvil.Functional;

namespace LfrlAnvil.Collections.Tests.DirectedGraphTests;

public class DirectedGraphEdgeTests : TestsBase
{
    [Fact]
    public void ValueSet_ShouldUpdateValue()
    {
        var (oldValue, newValue) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var graph = new DirectedGraph<string, int, long>();
        graph.AddNode( "a", oldValue );
        var sut = graph.AddEdge( "a", "a", oldValue );

        sut.Value = newValue;

        sut.Value.Should().Be( newValue );
    }

    [Theory]
    [InlineData( GraphDirection.In, "<=" )]
    [InlineData( GraphDirection.Out, "=>" )]
    [InlineData( GraphDirection.Both, "<=>" )]
    public void ToString_ShouldReturnCorrectResult(GraphDirection direction, string expectedDirectionText)
    {
        var graph = new DirectedGraph<string, int, long>();
        graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );
        var sut = graph.AddEdge( "a", "b", Fixture.Create<long>(), direction );
        var expected = $"a {expectedDirectionText} b";

        var result = sut.ToString();

        result.Should().Be( expected );
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WhenEdgeHasBeenRemovedFromGraph()
    {
        var graph = new DirectedGraph<string, int, long>();
        graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );
        var sut = graph.AddEdge( "a", "b", Fixture.Create<long>() );
        graph.RemoveEdge( "a", "b" );

        var result = sut.ToString();

        result.Should().Be( "a =/= b" );
    }

    [Theory]
    [InlineData( GraphDirection.None )]
    [InlineData( GraphDirection.In )]
    [InlineData( GraphDirection.Out )]
    [InlineData( GraphDirection.Both )]
    public void ChangeDirection_ShouldThrowInvalidOperationException_WhenEdgeHasBeenRemovedFromGraph(GraphDirection direction)
    {
        var graph = new DirectedGraph<string, int, long>();
        graph.AddNode( "a", Fixture.Create<int>() );
        var sut = graph.AddEdge( "a", "a", Fixture.Create<long>() );
        graph.RemoveEdge( "a", "a" );

        var action = Lambda.Of( () => sut.ChangeDirection( direction ) );

        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void ChangeDirection_ShouldThrowArgumentException_WhenDirectionIsEqualToNone()
    {
        var graph = new DirectedGraph<string, int, long>();
        graph.AddNode( "a", Fixture.Create<int>() );
        var sut = graph.AddEdge( "a", "a", Fixture.Create<long>() );

        var action = Lambda.Of( () => sut.ChangeDirection( GraphDirection.None ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Theory]
    [InlineData( GraphDirection.In, GraphDirection.In )]
    [InlineData( GraphDirection.In, GraphDirection.Out )]
    [InlineData( GraphDirection.In, GraphDirection.Both )]
    [InlineData( GraphDirection.Out, GraphDirection.In )]
    [InlineData( GraphDirection.Out, GraphDirection.Out )]
    [InlineData( GraphDirection.Out, GraphDirection.Both )]
    [InlineData( GraphDirection.Both, GraphDirection.In )]
    [InlineData( GraphDirection.Both, GraphDirection.Out )]
    [InlineData( GraphDirection.Both, GraphDirection.Both )]
    public void ChangeDirection_ShouldUpdateDirectionCorrectly(GraphDirection oldDirection, GraphDirection newDirection)
    {
        var graph = new DirectedGraph<string, int, long>();
        graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );
        var sut = graph.AddEdge( "a", "b", Fixture.Create<long>(), oldDirection );

        sut.ChangeDirection( newDirection );

        sut.Direction.Should().Be( newDirection );
    }

    [Theory]
    [InlineData( GraphDirection.In )]
    [InlineData( GraphDirection.Out )]
    [InlineData( GraphDirection.Both )]
    public void ChangeDirection_ShouldDoNothing_WhenEdgeConnectsNodeToSelf(GraphDirection direction)
    {
        var graph = new DirectedGraph<string, int, long>();
        graph.AddNode( "a", Fixture.Create<int>() );
        var sut = graph.AddEdge( "a", "a", Fixture.Create<long>() );

        sut.ChangeDirection( direction );

        sut.Direction.Should().Be( GraphDirection.Both );
    }

    [Fact]
    public void Remove_ShouldThrowInvalidOperationException_WhenEdgeHasBeenRemovedFromGraph()
    {
        var graph = new DirectedGraph<string, int, long>();
        graph.AddNode( "a", Fixture.Create<int>() );
        var sut = graph.AddEdge( "a", "a", Fixture.Create<long>() );
        graph.RemoveEdge( "a", "a" );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void Remove_ShouldRemoveEdgeFromGraph()
    {
        var graph = new DirectedGraph<string, int, long>();
        var a = graph.AddNode( "a", Fixture.Create<int>() );
        var b = graph.AddNode( "b", Fixture.Create<int>() );
        var sut = graph.AddEdge( "a", "b", Fixture.Create<long>() );

        sut.Remove();

        using ( new AssertionScope() )
        {
            a.Edges.Should().BeEmpty();
            b.Edges.Should().BeEmpty();
            sut.Direction.Should().Be( GraphDirection.None );
        }
    }

    [Fact]
    public void IDirectedGraphEdge_Source_ShouldBeSameAsSource()
    {
        var graph = new DirectedGraph<string, int, long>();
        graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );
        var sut = graph.AddEdge( "a", "b", Fixture.Create<long>() );

        var result = ((IDirectedGraphEdge<string, int, long>)sut).Source;

        result.Should().BeSameAs( sut.Source );
    }

    [Fact]
    public void IDirectedGraphEdge_Target_ShouldBeSameAsTarget()
    {
        var graph = new DirectedGraph<string, int, long>();
        graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );
        var sut = graph.AddEdge( "a", "b", Fixture.Create<long>() );

        var result = ((IDirectedGraphEdge<string, int, long>)sut).Target;

        result.Should().BeSameAs( sut.Target );
    }
}
