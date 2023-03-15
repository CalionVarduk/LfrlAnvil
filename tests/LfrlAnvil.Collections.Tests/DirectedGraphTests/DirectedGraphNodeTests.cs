using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Collections.Tests.DirectedGraphTests;

public class DirectedGraphNodeTests : TestsBase
{
    [Fact]
    public void ValueSet_ShouldUpdateValue()
    {
        var (oldValue, newValue) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", oldValue );

        sut.Value = newValue;

        sut.Value.Should().Be( newValue );
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", 10 );

        var result = sut.ToString();

        result.Should().Be( "a => 10" );
    }

    [Fact]
    public void ContainsEdgeTo_ShouldReturnTrue_WhenEdgeExists()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );
        graph.AddEdge( "a", "b", Fixture.Create<long>() );

        var result = sut.ContainsEdgeTo( "b" );

        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsEdgeTo_ShouldReturnFalse_WhenEdgeDoesNotExist()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );

        var result = sut.ContainsEdgeTo( "b" );

        result.Should().BeFalse();
    }

    [Fact]
    public void GetEdgeTo_ShouldReturnCorrectEdge_WhenEdgeExists()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );
        var edge = graph.AddEdge( "a", "b", Fixture.Create<long>() );

        var result = sut.GetEdgeTo( "b" );

        result.Should().BeSameAs( edge );
    }

    [Fact]
    public void GetEdgeTo_ShouldThrowKeyNotFoundException_WhenEdgeDoesNotExist()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );

        var action = Lambda.Of( () => sut.GetEdgeTo( "b" ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void TryGetEdgeTo_ShouldReturnCorrectEdge_WhenEdgeExists()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );
        var edge = graph.AddEdge( "a", "b", Fixture.Create<long>() );

        var result = sut.TryGetEdgeTo( "b", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().BeSameAs( edge );
        }
    }

    [Fact]
    public void TryGetEdgeTo_ShouldReturnNull_WhenEdgeDoesNotExist()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );

        var result = sut.TryGetEdgeTo( "b", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Theory]
    [InlineData( GraphDirection.In )]
    [InlineData( GraphDirection.Out )]
    [InlineData( GraphDirection.Both )]
    public void AddEdgeTo_ShouldAddEdgeWithBothDirection_WhenAddingEdgeToSelf(GraphDirection direction)
    {
        var value = Fixture.Create<long>();
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );

        var result = sut.AddEdgeTo( "a", value, direction );

        using ( new AssertionScope() )
        {
            sut.Edges.Should().BeEquivalentTo( result );
            result.Source.Should().BeSameAs( sut );
            result.Target.Should().BeSameAs( sut );
            result.Value.Should().Be( value );
            result.Direction.Should().Be( GraphDirection.Both );
        }
    }

    [Theory]
    [InlineData( GraphDirection.In )]
    [InlineData( GraphDirection.Out )]
    [InlineData( GraphDirection.Both )]
    public void AddEdgeTo_ShouldAddEdge_WhenTargetExists(GraphDirection direction)
    {
        var value = Fixture.Create<long>();
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var target = graph.AddNode( "b", Fixture.Create<int>() );

        var result = sut.AddEdgeTo( "b", value, direction );

        using ( new AssertionScope() )
        {
            sut.Edges.Should().BeEquivalentTo( result );
            target.Edges.Should().BeEquivalentTo( result );
            result.Source.Should().BeSameAs( sut );
            result.Target.Should().BeSameAs( target );
            result.Value.Should().Be( value );
            result.Direction.Should().Be( direction );
        }
    }

    [Fact]
    public void AddEdgeTo_ShouldThrowKeyNotFoundException_WhenTargetDoesNotExist()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );

        var action = Lambda.Of( () => sut.AddEdgeTo( "b", Fixture.Create<long>() ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void AddEdgeTo_ShouldThrowArgumentException_WhenEdgeAlreadyExists()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );
        graph.AddEdge( "a", "b", Fixture.Create<long>() );

        var action = Lambda.Of( () => sut.AddEdgeTo( "b", Fixture.Create<long>() ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void AddEdgeTo_ShouldThrowArgumentException_WhenDirectionIsEqualToNone()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );

        var action = Lambda.Of( () => sut.AddEdgeTo( "b", Fixture.Create<long>(), GraphDirection.None ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void AddEdgeTo_ShouldThrowInvalidOperationException_WhenNodeHasBeenRemovedFromGraph()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );
        graph.Remove( sut );

        var action = Lambda.Of( () => sut.AddEdgeTo( "b", Fixture.Create<long>() ) );

        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Theory]
    [InlineData( GraphDirection.In )]
    [InlineData( GraphDirection.Out )]
    [InlineData( GraphDirection.Both )]
    public void TryAddEdgeTo_ShouldAddEdgeWithBothDirection_WhenAddingEdgeToSelf(GraphDirection direction)
    {
        var value = Fixture.Create<long>();
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );

        var result = sut.TryAddEdgeTo( "a", value, direction, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Edges.Should().BeEquivalentTo( outResult );
            outResult.Should().NotBeNull();
            outResult?.Source.Should().BeSameAs( sut );
            outResult?.Target.Should().BeSameAs( sut );
            outResult?.Value.Should().Be( value );
            outResult?.Direction.Should().Be( GraphDirection.Both );
        }
    }

    [Theory]
    [InlineData( GraphDirection.In )]
    [InlineData( GraphDirection.Out )]
    [InlineData( GraphDirection.Both )]
    public void TryAddEdgeTo_ShouldAddEdge_WhenTargetExists(GraphDirection direction)
    {
        var value = Fixture.Create<long>();
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var target = graph.AddNode( "b", Fixture.Create<int>() );

        var result = sut.TryAddEdgeTo( "b", value, direction, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Edges.Should().BeEquivalentTo( outResult );
            target.Edges.Should().BeEquivalentTo( outResult );
            outResult.Should().NotBeNull();
            outResult?.Source.Should().BeSameAs( sut );
            outResult?.Target.Should().BeSameAs( target );
            outResult?.Value.Should().Be( value );
            outResult?.Direction.Should().Be( direction );
        }
    }

    [Fact]
    public void TryAddEdgeTo_ShouldReturnFalse_WhenTargetDoesNotExist()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );

        var result = sut.TryAddEdgeTo( "b", Fixture.Create<long>(), GraphDirection.Both, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void TryAddEdgeTo_ShouldReturnFalse_WhenEdgeAlreadyExists()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );
        graph.AddEdge( "a", "b", Fixture.Create<long>() );

        var result = sut.TryAddEdgeTo( "b", Fixture.Create<long>(), GraphDirection.Both, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void TryAddEdgeTo_ShouldThrowArgumentException_WhenDirectionIsEqualToNone()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );

        var action = Lambda.Of( () => sut.TryAddEdgeTo( "b", Fixture.Create<long>(), GraphDirection.None, out _ ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void TryAddEdgeTo_ShouldThrowInvalidOperationException_WhenNodeHasBeenRemovedFromGraph()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );
        graph.Remove( sut );

        var action = Lambda.Of( () => sut.TryAddEdgeTo( "b", Fixture.Create<long>(), GraphDirection.Both, out _ ) );

        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Theory]
    [InlineData( GraphDirection.In )]
    [InlineData( GraphDirection.Out )]
    [InlineData( GraphDirection.Both )]
    public void AddEdgeTo_WithNode_ShouldAddEdgeWithBothDirection_WhenAddingEdgeToSelf(GraphDirection direction)
    {
        var value = Fixture.Create<long>();
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );

        var result = sut.AddEdgeTo( sut, value, direction );

        using ( new AssertionScope() )
        {
            sut.Edges.Should().BeEquivalentTo( result );
            result.Source.Should().BeSameAs( sut );
            result.Target.Should().BeSameAs( sut );
            result.Value.Should().Be( value );
            result.Direction.Should().Be( GraphDirection.Both );
        }
    }

    [Theory]
    [InlineData( GraphDirection.In )]
    [InlineData( GraphDirection.Out )]
    [InlineData( GraphDirection.Both )]
    public void AddEdgeTo_WithNode_ShouldAddEdge_WhenTargetExists(GraphDirection direction)
    {
        var value = Fixture.Create<long>();
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var target = graph.AddNode( "b", Fixture.Create<int>() );

        var result = sut.AddEdgeTo( target, value, direction );

        using ( new AssertionScope() )
        {
            sut.Edges.Should().BeEquivalentTo( result );
            target.Edges.Should().BeEquivalentTo( result );
            result.Source.Should().BeSameAs( sut );
            result.Target.Should().BeSameAs( target );
            result.Value.Should().Be( value );
            result.Direction.Should().Be( direction );
        }
    }

    [Fact]
    public void AddEdgeTo_WithNode_ShouldThrowArgumentException_WhenTargetIsFromDifferentGraph()
    {
        var other = new DirectedGraph<string, int, long>();
        var target = other.AddNode( "b", Fixture.Create<int>() );
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );

        var action = Lambda.Of( () => sut.AddEdgeTo( target, Fixture.Create<long>() ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void AddEdgeTo_WithNode_ShouldThrowArgumentException_WhenEdgeAlreadyExists()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var target = graph.AddNode( "b", Fixture.Create<int>() );
        graph.AddEdge( "a", "b", Fixture.Create<long>() );

        var action = Lambda.Of( () => sut.AddEdgeTo( target, Fixture.Create<long>() ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void AddEdgeTo_WithNode_ShouldThrowArgumentException_WhenDirectionIsEqualToNone()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var target = graph.AddNode( "b", Fixture.Create<int>() );

        var action = Lambda.Of( () => sut.AddEdgeTo( target, Fixture.Create<long>(), GraphDirection.None ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void AddEdgeTo_WithNode_ShouldThrowInvalidOperationException_WhenNodeHasBeenRemovedFromGraph()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var target = graph.AddNode( "b", Fixture.Create<int>() );
        graph.Remove( sut );

        var action = Lambda.Of( () => sut.AddEdgeTo( target, Fixture.Create<long>() ) );

        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Theory]
    [InlineData( GraphDirection.In )]
    [InlineData( GraphDirection.Out )]
    [InlineData( GraphDirection.Both )]
    public void TryAddEdgeTo_WithNode_ShouldAddEdgeWithBothDirection_WhenAddingEdgeToSelf(GraphDirection direction)
    {
        var value = Fixture.Create<long>();
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );

        var result = sut.TryAddEdgeTo( sut, value, direction, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Edges.Should().BeEquivalentTo( outResult );
            outResult.Should().NotBeNull();
            outResult?.Source.Should().BeSameAs( sut );
            outResult?.Target.Should().BeSameAs( sut );
            outResult?.Value.Should().Be( value );
            outResult?.Direction.Should().Be( GraphDirection.Both );
        }
    }

    [Theory]
    [InlineData( GraphDirection.In )]
    [InlineData( GraphDirection.Out )]
    [InlineData( GraphDirection.Both )]
    public void TryAddEdgeTo_WithNode_ShouldAddEdge_WhenTargetExists(GraphDirection direction)
    {
        var value = Fixture.Create<long>();
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var target = graph.AddNode( "b", Fixture.Create<int>() );

        var result = sut.TryAddEdgeTo( target, value, direction, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Edges.Should().BeEquivalentTo( outResult );
            target.Edges.Should().BeEquivalentTo( outResult );
            outResult.Should().NotBeNull();
            outResult?.Source.Should().BeSameAs( sut );
            outResult?.Target.Should().BeSameAs( target );
            outResult?.Value.Should().Be( value );
            outResult?.Direction.Should().Be( direction );
        }
    }

    [Fact]
    public void TryAddEdgeTo_WithNode_ShouldReturnFalse_WhenTargetIsFromDifferentGraph()
    {
        var other = new DirectedGraph<string, int, long>();
        var target = other.AddNode( "b", Fixture.Create<int>() );
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );

        var result = sut.TryAddEdgeTo( target, Fixture.Create<long>(), GraphDirection.Both, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void TryAddEdgeTo_WithNode_ShouldReturnFalse_WhenEdgeAlreadyExists()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var target = graph.AddNode( "b", Fixture.Create<int>() );
        graph.AddEdge( "a", "b", Fixture.Create<long>() );

        var result = sut.TryAddEdgeTo( target, Fixture.Create<long>(), GraphDirection.Both, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void TryAddEdgeTo_WithNode_ShouldThrowArgumentException_WhenDirectionIsEqualToNone()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var target = graph.AddNode( "b", Fixture.Create<int>() );

        var action = Lambda.Of( () => sut.TryAddEdgeTo( target, Fixture.Create<long>(), GraphDirection.None, out _ ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void TryAddEdgeTo_WithNode_ShouldThrowInvalidOperationException_WhenNodeHasBeenRemovedFromGraph()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var target = graph.AddNode( "b", Fixture.Create<int>() );
        graph.Remove( sut );

        var action = Lambda.Of( () => sut.TryAddEdgeTo( target, Fixture.Create<long>(), GraphDirection.Both, out _ ) );

        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void RemoveEdgeTo_ShouldReturnFalse_WhenEdgeDoesNotExist()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );

        var result = sut.RemoveEdgeTo( "b" );

        result.Should().BeFalse();
    }

    [Fact]
    public void RemoveEdgeTo_ShouldReturnTrueAndRemoveEdge_WhenEdgeToSelfExists()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var edge = graph.AddEdge( "a", "a", Fixture.Create<long>() );

        var result = sut.RemoveEdgeTo( "a" );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Edges.Should().BeEmpty();
            edge.Direction.Should().Be( GraphDirection.None );
        }
    }

    [Fact]
    public void RemoveEdgeTo_ShouldReturnTrueAndRemoveEdge_WhenEdgeToOtherTargetExists()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var other = graph.AddNode( "b", Fixture.Create<int>() );
        var edge = graph.AddEdge( "a", "b", Fixture.Create<long>() );

        var result = sut.RemoveEdgeTo( "b" );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Edges.Should().BeEmpty();
            other.Edges.Should().BeEmpty();
            edge.Direction.Should().Be( GraphDirection.None );
        }
    }

    [Fact]
    public void RemoveEdgeTo_ShouldReturnTrueAndRemoveEdge_WhenEdgeToOtherSourceExists()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var other = graph.AddNode( "b", Fixture.Create<int>() );
        var edge = graph.AddEdge( "b", "a", Fixture.Create<long>() );

        var result = sut.RemoveEdgeTo( "b" );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Edges.Should().BeEmpty();
            other.Edges.Should().BeEmpty();
            edge.Direction.Should().Be( GraphDirection.None );
        }
    }

    [Fact]
    public void RemoveEdgeTo_WithNode_ShouldThrowInvalidOperationException_WhenNodeHasBeenRemovedFromGraph()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.Remove( sut );

        var action = Lambda.Of( () => sut.RemoveEdgeTo( "a" ) );

        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void RemoveEdgeTo_WithOutResult_ShouldReturnFalse_WhenEdgeDoesNotExist()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );

        var result = sut.RemoveEdgeTo( "b", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().Be( default );
        }
    }

    [Fact]
    public void RemoveEdgeTo_WithOutResult_ShouldReturnTrueAndRemoveEdge_WhenEdgeToSelfExists()
    {
        var value = Fixture.Create<long>();
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var edge = graph.AddEdge( "a", "a", value );

        var result = sut.RemoveEdgeTo( "a", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( value );
            sut.Edges.Should().BeEmpty();
            edge.Direction.Should().Be( GraphDirection.None );
        }
    }

    [Fact]
    public void RemoveEdgeTo_WithOutResult_ShouldReturnTrueAndRemoveEdge_WhenEdgeToOtherTargetExists()
    {
        var value = Fixture.Create<long>();
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var other = graph.AddNode( "b", Fixture.Create<int>() );
        var edge = graph.AddEdge( "a", "b", value );

        var result = sut.RemoveEdgeTo( "b", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( value );
            sut.Edges.Should().BeEmpty();
            other.Edges.Should().BeEmpty();
            edge.Direction.Should().Be( GraphDirection.None );
        }
    }

    [Fact]
    public void RemoveEdgeTo_WithOutResult_ShouldReturnTrueAndRemoveEdge_WhenEdgeToOtherSourceExists()
    {
        var value = Fixture.Create<long>();
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var other = graph.AddNode( "b", Fixture.Create<int>() );
        var edge = graph.AddEdge( "b", "a", value );

        var result = sut.RemoveEdgeTo( "b", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( value );
            sut.Edges.Should().BeEmpty();
            other.Edges.Should().BeEmpty();
            edge.Direction.Should().Be( GraphDirection.None );
        }
    }

    [Fact]
    public void RemoveEdgeTo_WithOutResult_ShouldThrowInvalidOperationException_WhenNodeHasBeenRemovedFromGraph()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.Remove( sut );

        var action = Lambda.Of( () => sut.RemoveEdgeTo( "a", out _ ) );

        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void Remove_WithEdge_ShouldReturnFalse_WhenEdgeDoesNotExist()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );
        var edge = graph.AddEdge( "b", "b", Fixture.Create<long>() );

        var result = sut.Remove( edge );

        result.Should().BeFalse();
    }

    [Fact]
    public void Remove_WithEdge_ShouldReturnFalse_WhenEdgeHasAlreadyBeenRemoved()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var edge = graph.AddEdge( "a", "a", Fixture.Create<long>() );
        graph.RemoveEdge( "a", "a" );

        var result = sut.Remove( edge );

        result.Should().BeFalse();
    }

    [Fact]
    public void Remove_WithEdge_ShouldReturnTrueAndRemoveEdge_WhenEdgeToSelfExists()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var edge = graph.AddEdge( "a", "a", Fixture.Create<long>() );

        var result = sut.Remove( edge );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Edges.Should().BeEmpty();
            edge.Direction.Should().Be( GraphDirection.None );
        }
    }

    [Fact]
    public void Remove_WithEdge_ShouldReturnTrueAndRemoveEdge_WhenEdgeToOtherTargetExists()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var other = graph.AddNode( "b", Fixture.Create<int>() );
        var edge = graph.AddEdge( "a", "b", Fixture.Create<long>() );

        var result = sut.Remove( edge );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Edges.Should().BeEmpty();
            other.Edges.Should().BeEmpty();
            edge.Direction.Should().Be( GraphDirection.None );
        }
    }

    [Fact]
    public void Remove_WithEdge_ShouldReturnTrueAndRemoveEdge_WhenEdgeToOtherSourceExists()
    {
        var value = Fixture.Create<long>();
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var other = graph.AddNode( "b", Fixture.Create<int>() );
        var edge = graph.AddEdge( "b", "a", value );

        var result = sut.Remove( edge );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Edges.Should().BeEmpty();
            other.Edges.Should().BeEmpty();
            edge.Direction.Should().Be( GraphDirection.None );
        }
    }

    [Fact]
    public void Remove_WithEdge_ShouldThrowInvalidOperationException_WhenNodeHasBeenRemovedFromGraph()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );
        var edge = graph.AddEdge( "b", "b", Fixture.Create<long>() );
        graph.Remove( sut );

        var action = Lambda.Of( () => sut.Remove( edge ) );

        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void Remove_ShouldRemoveNodeAndItsEdgesFromGraph()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var other = graph.AddNode( "b", Fixture.Create<int>() );
        var aa = graph.AddEdge( "a", "a", Fixture.Create<long>() );
        var ab = graph.AddEdge( "a", "b", Fixture.Create<long>() );

        sut.Remove();

        using ( new AssertionScope() )
        {
            graph.Nodes.Should().BeEquivalentTo( other );
            sut.Graph.Should().BeNull();
            sut.Edges.Should().BeEmpty();
            other.Edges.Should().BeEmpty();
            aa.Direction.Should().Be( GraphDirection.None );
            ab.Direction.Should().Be( GraphDirection.None );
        }
    }

    [Fact]
    public void Remove_ShouldThrowInvalidOperationException_WhenNodeHasBeenRemovedFromGraph()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.Remove( sut );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void RemoveEdges_ShouldDoNothing_WhenDirectionIsEqualToNone()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var b = graph.AddNode( "b", Fixture.Create<int>() );
        var c = graph.AddNode( "c", Fixture.Create<int>() );
        var d = graph.AddNode( "d", Fixture.Create<int>() );
        var e = graph.AddNode( "e", Fixture.Create<int>() );
        var f = graph.AddNode( "f", Fixture.Create<int>() );
        var g = graph.AddNode( "g", Fixture.Create<int>() );
        var aa = graph.AddEdge( "a", "a", Fixture.Create<long>() );
        var ab = graph.AddEdge( "a", "b", Fixture.Create<long>(), GraphDirection.In );
        var ac = graph.AddEdge( "a", "c", Fixture.Create<long>(), GraphDirection.Out );
        var ad = graph.AddEdge( "a", "d", Fixture.Create<long>(), GraphDirection.Both );
        var ea = graph.AddEdge( "e", "a", Fixture.Create<long>(), GraphDirection.In );
        var fa = graph.AddEdge( "f", "a", Fixture.Create<long>(), GraphDirection.Out );
        var ga = graph.AddEdge( "g", "a", Fixture.Create<long>(), GraphDirection.Both );

        var result = sut.RemoveEdges( GraphDirection.None );

        using ( new AssertionScope() )
        {
            result.Should().Be( 0 );
            sut.Edges.Should().BeEquivalentTo( aa, ab, ac, ad, ea, fa, ga );
            b.Edges.Should().BeEquivalentTo( ab );
            c.Edges.Should().BeEquivalentTo( ac );
            d.Edges.Should().BeEquivalentTo( ad );
            e.Edges.Should().BeEquivalentTo( ea );
            f.Edges.Should().BeEquivalentTo( fa );
            g.Edges.Should().BeEquivalentTo( ga );
            aa.Direction.Should().Be( GraphDirection.Both );
            ab.Direction.Should().Be( GraphDirection.In );
            ac.Direction.Should().Be( GraphDirection.Out );
            ad.Direction.Should().Be( GraphDirection.Both );
            ea.Direction.Should().Be( GraphDirection.In );
            fa.Direction.Should().Be( GraphDirection.Out );
            ga.Direction.Should().Be( GraphDirection.Both );
        }
    }

    [Fact]
    public void RemoveEdges_ShouldRemoveIncomingConnections_WhenDirectionIsEqualToIn()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var b = graph.AddNode( "b", Fixture.Create<int>() );
        var c = graph.AddNode( "c", Fixture.Create<int>() );
        var d = graph.AddNode( "d", Fixture.Create<int>() );
        var e = graph.AddNode( "e", Fixture.Create<int>() );
        var f = graph.AddNode( "f", Fixture.Create<int>() );
        var g = graph.AddNode( "g", Fixture.Create<int>() );
        var aa = graph.AddEdge( "a", "a", Fixture.Create<long>() );
        var ab = graph.AddEdge( "a", "b", Fixture.Create<long>(), GraphDirection.In );
        var ac = graph.AddEdge( "a", "c", Fixture.Create<long>(), GraphDirection.Out );
        var ad = graph.AddEdge( "a", "d", Fixture.Create<long>(), GraphDirection.Both );
        var ea = graph.AddEdge( "e", "a", Fixture.Create<long>(), GraphDirection.In );
        var fa = graph.AddEdge( "f", "a", Fixture.Create<long>(), GraphDirection.Out );
        var ga = graph.AddEdge( "g", "a", Fixture.Create<long>(), GraphDirection.Both );

        var result = sut.RemoveEdges( GraphDirection.In );

        using ( new AssertionScope() )
        {
            result.Should().Be( 2 );
            sut.Edges.Should().BeEquivalentTo( aa, ac, ad, ea, ga );
            b.Edges.Should().BeEmpty();
            c.Edges.Should().BeEquivalentTo( ac );
            d.Edges.Should().BeEquivalentTo( ad );
            e.Edges.Should().BeEquivalentTo( ea );
            f.Edges.Should().BeEmpty();
            g.Edges.Should().BeEquivalentTo( ga );
            aa.Direction.Should().Be( GraphDirection.Both );
            ab.Direction.Should().Be( GraphDirection.None );
            ac.Direction.Should().Be( GraphDirection.Out );
            ad.Direction.Should().Be( GraphDirection.Out );
            ea.Direction.Should().Be( GraphDirection.In );
            fa.Direction.Should().Be( GraphDirection.None );
            ga.Direction.Should().Be( GraphDirection.In );
        }
    }

    [Fact]
    public void RemoveEdges_ShouldRemoveOutgoingConnections_WhenDirectionIsEqualToOut()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var b = graph.AddNode( "b", Fixture.Create<int>() );
        var c = graph.AddNode( "c", Fixture.Create<int>() );
        var d = graph.AddNode( "d", Fixture.Create<int>() );
        var e = graph.AddNode( "e", Fixture.Create<int>() );
        var f = graph.AddNode( "f", Fixture.Create<int>() );
        var g = graph.AddNode( "g", Fixture.Create<int>() );
        var aa = graph.AddEdge( "a", "a", Fixture.Create<long>() );
        var ab = graph.AddEdge( "a", "b", Fixture.Create<long>(), GraphDirection.In );
        var ac = graph.AddEdge( "a", "c", Fixture.Create<long>(), GraphDirection.Out );
        var ad = graph.AddEdge( "a", "d", Fixture.Create<long>(), GraphDirection.Both );
        var ea = graph.AddEdge( "e", "a", Fixture.Create<long>(), GraphDirection.In );
        var fa = graph.AddEdge( "f", "a", Fixture.Create<long>(), GraphDirection.Out );
        var ga = graph.AddEdge( "g", "a", Fixture.Create<long>(), GraphDirection.Both );

        var result = sut.RemoveEdges( GraphDirection.Out );

        using ( new AssertionScope() )
        {
            result.Should().Be( 2 );
            sut.Edges.Should().BeEquivalentTo( aa, ab, ad, fa, ga );
            b.Edges.Should().BeEquivalentTo( ab );
            c.Edges.Should().BeEmpty();
            d.Edges.Should().BeEquivalentTo( ad );
            e.Edges.Should().BeEmpty();
            f.Edges.Should().BeEquivalentTo( fa );
            g.Edges.Should().BeEquivalentTo( ga );
            aa.Direction.Should().Be( GraphDirection.Both );
            ab.Direction.Should().Be( GraphDirection.In );
            ac.Direction.Should().Be( GraphDirection.None );
            ad.Direction.Should().Be( GraphDirection.In );
            ea.Direction.Should().Be( GraphDirection.None );
            fa.Direction.Should().Be( GraphDirection.Out );
            ga.Direction.Should().Be( GraphDirection.Out );
        }
    }

    [Fact]
    public void RemoveEdges_ShouldRemoveAllEdges_WhenDirectionIsEqualToBoth()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var b = graph.AddNode( "b", Fixture.Create<int>() );
        var c = graph.AddNode( "c", Fixture.Create<int>() );
        var d = graph.AddNode( "d", Fixture.Create<int>() );
        var e = graph.AddNode( "e", Fixture.Create<int>() );
        var f = graph.AddNode( "f", Fixture.Create<int>() );
        var g = graph.AddNode( "g", Fixture.Create<int>() );
        var aa = graph.AddEdge( "a", "a", Fixture.Create<long>() );
        var ab = graph.AddEdge( "a", "b", Fixture.Create<long>(), GraphDirection.In );
        var ac = graph.AddEdge( "a", "c", Fixture.Create<long>(), GraphDirection.Out );
        var ad = graph.AddEdge( "a", "d", Fixture.Create<long>(), GraphDirection.Both );
        var ea = graph.AddEdge( "e", "a", Fixture.Create<long>(), GraphDirection.In );
        var fa = graph.AddEdge( "f", "a", Fixture.Create<long>(), GraphDirection.Out );
        var ga = graph.AddEdge( "g", "a", Fixture.Create<long>(), GraphDirection.Both );

        var result = sut.RemoveEdges( GraphDirection.Both );

        using ( new AssertionScope() )
        {
            result.Should().Be( 7 );
            sut.Edges.Should().BeEmpty();
            b.Edges.Should().BeEmpty();
            c.Edges.Should().BeEmpty();
            d.Edges.Should().BeEmpty();
            e.Edges.Should().BeEmpty();
            f.Edges.Should().BeEmpty();
            g.Edges.Should().BeEmpty();
            aa.Direction.Should().Be( GraphDirection.None );
            ab.Direction.Should().Be( GraphDirection.None );
            ac.Direction.Should().Be( GraphDirection.None );
            ad.Direction.Should().Be( GraphDirection.None );
            ea.Direction.Should().Be( GraphDirection.None );
            fa.Direction.Should().Be( GraphDirection.None );
            ga.Direction.Should().Be( GraphDirection.None );
        }
    }

    [Theory]
    [InlineData( GraphDirection.None )]
    [InlineData( GraphDirection.In )]
    [InlineData( GraphDirection.Out )]
    [InlineData( GraphDirection.Both )]
    public void RemoveEdges_ShouldThrowInvalidOperationException_WhenNodeHasBeenRemovedFromGraph(GraphDirection direction)
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.Remove( sut );

        var action = Lambda.Of( () => sut.RemoveEdges( direction ) );

        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Theory]
    [InlineData( GraphDirection.None )]
    [InlineData( GraphDirection.In )]
    [InlineData( GraphDirection.Out )]
    [InlineData( GraphDirection.Both )]
    public void GetReachableNodes_ShouldReturnEmptyCollection_WhenNodeHasBeenRemovedFromGraph(GraphDirection direction)
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.Remove( sut );

        var result = sut.GetReachableNodes( direction );

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData( GraphDirection.In )]
    [InlineData( GraphDirection.Out )]
    [InlineData( GraphDirection.Both )]
    public void GetReachableNodes_ShouldReturnEmptyCollection_WhenNodeDoesNotContainAnyEdges(GraphDirection direction)
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );

        var result = sut.GetReachableNodes( direction );

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetReachableNodes_ShouldReturnEmptyCollection_WhenDirectionIsEqualToNone()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddEdge( "a", "a", Fixture.Create<long>() );

        var result = sut.GetReachableNodes( GraphDirection.None );

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetReachableNodes_ShouldReturnCorrectEdges_WhenDirectionIsEqualToOut()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var b = graph.AddNode( "b", Fixture.Create<int>() );
        var c = graph.AddNode( "c", Fixture.Create<int>() );
        var d = graph.AddNode( "d", Fixture.Create<int>() );
        var e = graph.AddNode( "e", Fixture.Create<int>() );
        var f = graph.AddNode( "f", Fixture.Create<int>() );
        var g = graph.AddNode( "g", Fixture.Create<int>() );
        var h = graph.AddNode( "h", Fixture.Create<int>() );
        var i = graph.AddNode( "i", Fixture.Create<int>() );
        var j = graph.AddNode( "j", Fixture.Create<int>() );
        var k = graph.AddNode( "k", Fixture.Create<int>() );
        graph.AddEdge( "a", "a", Fixture.Create<long>() );
        graph.AddEdge( "a", "b", Fixture.Create<long>(), GraphDirection.Out );
        graph.AddEdge( "c", "b", Fixture.Create<long>(), GraphDirection.In );
        graph.AddEdge( "c", "c", Fixture.Create<long>() );
        graph.AddEdge( "d", "b", Fixture.Create<long>(), GraphDirection.Out );
        graph.AddEdge( "a", "e", Fixture.Create<long>(), GraphDirection.In );
        graph.AddEdge( "f", "e", Fixture.Create<long>(), GraphDirection.Out );
        graph.AddEdge( "f", "f", Fixture.Create<long>() );
        graph.AddEdge( "g", "e", Fixture.Create<long>(), GraphDirection.In );
        graph.AddEdge( "a", "h", Fixture.Create<long>(), GraphDirection.Both );
        graph.AddEdge( "h", "i", Fixture.Create<long>(), GraphDirection.Both );
        graph.AddEdge( "i", "j", Fixture.Create<long>(), GraphDirection.Both );
        graph.AddEdge( "j", "k", Fixture.Create<long>(), GraphDirection.Both );

        var result = sut.GetReachableNodes( GraphDirection.Out );

        result.Should().HaveCount( 7 ).And.BeEquivalentTo( sut, b, c, h, i, j, k );
    }

    [Fact]
    public void GetReachableNodes_ShouldReturnCorrectEdges_WhenDirectionIsEqualToIn()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var b = graph.AddNode( "b", Fixture.Create<int>() );
        var c = graph.AddNode( "c", Fixture.Create<int>() );
        var d = graph.AddNode( "d", Fixture.Create<int>() );
        var e = graph.AddNode( "e", Fixture.Create<int>() );
        var f = graph.AddNode( "f", Fixture.Create<int>() );
        var g = graph.AddNode( "g", Fixture.Create<int>() );
        var h = graph.AddNode( "h", Fixture.Create<int>() );
        var i = graph.AddNode( "i", Fixture.Create<int>() );
        var j = graph.AddNode( "j", Fixture.Create<int>() );
        var k = graph.AddNode( "k", Fixture.Create<int>() );
        graph.AddEdge( "a", "a", Fixture.Create<long>() );
        graph.AddEdge( "a", "b", Fixture.Create<long>(), GraphDirection.Out );
        graph.AddEdge( "c", "b", Fixture.Create<long>(), GraphDirection.In );
        graph.AddEdge( "c", "c", Fixture.Create<long>() );
        graph.AddEdge( "d", "b", Fixture.Create<long>(), GraphDirection.Out );
        graph.AddEdge( "a", "e", Fixture.Create<long>(), GraphDirection.In );
        graph.AddEdge( "f", "e", Fixture.Create<long>(), GraphDirection.Out );
        graph.AddEdge( "f", "f", Fixture.Create<long>() );
        graph.AddEdge( "g", "e", Fixture.Create<long>(), GraphDirection.In );
        graph.AddEdge( "a", "h", Fixture.Create<long>(), GraphDirection.Both );
        graph.AddEdge( "h", "i", Fixture.Create<long>(), GraphDirection.Both );
        graph.AddEdge( "i", "j", Fixture.Create<long>(), GraphDirection.Both );
        graph.AddEdge( "j", "k", Fixture.Create<long>(), GraphDirection.Both );

        var result = sut.GetReachableNodes( GraphDirection.In );

        result.Should().HaveCount( 7 ).And.BeEquivalentTo( sut, e, f, h, i, j, k );
    }

    [Fact]
    public void GetReachableNodes_ShouldReturnCorrectEdges_WhenDirectionIsEqualToBoth()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var b = graph.AddNode( "b", Fixture.Create<int>() );
        var c = graph.AddNode( "c", Fixture.Create<int>() );
        var d = graph.AddNode( "d", Fixture.Create<int>() );
        var e = graph.AddNode( "e", Fixture.Create<int>() );
        var f = graph.AddNode( "f", Fixture.Create<int>() );
        var g = graph.AddNode( "g", Fixture.Create<int>() );
        var h = graph.AddNode( "h", Fixture.Create<int>() );
        var i = graph.AddNode( "i", Fixture.Create<int>() );
        var j = graph.AddNode( "j", Fixture.Create<int>() );
        var k = graph.AddNode( "k", Fixture.Create<int>() );
        graph.AddEdge( "a", "a", Fixture.Create<long>() );
        graph.AddEdge( "a", "b", Fixture.Create<long>(), GraphDirection.Out );
        graph.AddEdge( "c", "b", Fixture.Create<long>(), GraphDirection.In );
        graph.AddEdge( "c", "c", Fixture.Create<long>() );
        graph.AddEdge( "d", "b", Fixture.Create<long>(), GraphDirection.Out );
        graph.AddEdge( "a", "e", Fixture.Create<long>(), GraphDirection.In );
        graph.AddEdge( "f", "e", Fixture.Create<long>(), GraphDirection.Out );
        graph.AddEdge( "f", "f", Fixture.Create<long>() );
        graph.AddEdge( "g", "e", Fixture.Create<long>(), GraphDirection.In );
        graph.AddEdge( "a", "h", Fixture.Create<long>(), GraphDirection.Both );
        graph.AddEdge( "h", "i", Fixture.Create<long>(), GraphDirection.Both );
        graph.AddEdge( "i", "j", Fixture.Create<long>(), GraphDirection.Both );
        graph.AddEdge( "j", "k", Fixture.Create<long>(), GraphDirection.Both );

        var result = sut.GetReachableNodes( GraphDirection.Both );

        result.Should().HaveCount( 11 ).And.BeEquivalentTo( sut, b, c, d, e, f, g, h, i, j, k );
    }

    [Fact]
    public void IDirectedGraphNode_Graph_ShouldBeSameAsGraph()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", 10 );

        var result = ((IDirectedGraphNode<string, int, long>)sut).Graph;

        result.Should().BeSameAs( sut.Graph );
    }

    [Fact]
    public void IDirectedGraphNode_Edges_ShouldBeSameAsEdges()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", 10 );

        var result = ((IDirectedGraphNode<string, int, long>)sut).Edges;

        result.Should().BeSameAs( sut.Edges );
    }

    [Fact]
    public void IDirectedGraphNode_GetEdgeTo_ShouldBeEquivalentToGetEdgeTo()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddEdge( "a", "a", Fixture.Create<long>() );

        var result = ((IDirectedGraphNode<string, int, long>)sut).GetEdgeTo( "a" );

        result.Should().BeSameAs( sut.GetEdgeTo( "a" ) );
    }

    [Fact]
    public void IDirectedGraphNode_TryGetEdgeTo_ShouldBeEquivalentToTryGetEdgeTo()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddEdge( "a", "a", Fixture.Create<long>() );
        var expected = sut.TryGetEdgeTo( "a", out var outExpected );

        var result = ((IDirectedGraphNode<string, int, long>)sut).TryGetEdgeTo( "a", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().Be( expected );
            outResult.Should().BeSameAs( outExpected );
        }
    }

    [Fact]
    public void IDirectedGraphNode_GetReachableNodes_ShouldBeEquivalentToGetReachableNodes()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );
        graph.AddNode( "c", Fixture.Create<int>() );
        graph.AddEdge( "a", "b", Fixture.Create<long>() );
        graph.AddEdge( "c", "a", Fixture.Create<long>() );

        var result = ((IDirectedGraphNode<string, int, long>)sut).GetReachableNodes();

        result.Should().BeSequentiallyEqualTo( sut.GetReachableNodes() );
    }
}
