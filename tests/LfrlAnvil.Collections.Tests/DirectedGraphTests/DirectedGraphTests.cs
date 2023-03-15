using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Collections.Tests.DirectedGraphTests;

public class DirectedGraphTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateGraphWithoutAnyNodes()
    {
        var sut = new DirectedGraph<string, int, long>();
        using ( new AssertionScope() )
        {
            sut.Nodes.Should().BeEmpty();
            sut.Edges.Should().BeEmpty();
            sut.KeyComparer.Should().BeSameAs( EqualityComparer<string>.Default );
            ((IDirectedGraph<string, int, long>)sut).Nodes.Should().BeSameAs( sut.Nodes );
        }
    }

    [Fact]
    public void Ctor_WithExplicitComparer_ShouldCreateGraphWithoutAnyNodes()
    {
        var comparer = EqualityComparerFactory<string>.Create( (a, b) => a == b );
        var sut = new DirectedGraph<string, int, long>( comparer );

        using ( new AssertionScope() )
        {
            sut.Nodes.Should().BeEmpty();
            sut.Edges.Should().BeEmpty();
            sut.KeyComparer.Should().BeSameAs( comparer );
            ((IDirectedGraph<string, int, long>)sut).Nodes.Should().BeSameAs( sut.Nodes );
        }
    }

    [Fact]
    public void AddNode_ShouldAddFirstNode()
    {
        var key = Fixture.Create<string>();
        var value = Fixture.Create<int>();
        var sut = new DirectedGraph<string, int, long>();

        var result = sut.AddNode( key, value );

        using ( new AssertionScope() )
        {
            result.Key.Should().Be( key );
            result.Value.Should().Be( value );
            result.Graph.Should().BeSameAs( sut );
            result.Edges.Should().BeEmpty();
            sut.Nodes.Should().BeEquivalentTo( result );
        }
    }

    [Fact]
    public void AddNode_ShouldAddAnotherNodeWithDifferentKey()
    {
        var (otherKey, key) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var (otherValue, value) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        var first = sut.AddNode( otherKey, otherValue );

        var result = sut.AddNode( key, value );

        using ( new AssertionScope() )
        {
            result.Should().NotBeSameAs( first );
            result.Key.Should().Be( key );
            result.Value.Should().Be( value );
            result.Graph.Should().BeSameAs( sut );
            result.Edges.Should().BeEmpty();
            sut.Nodes.Should().BeEquivalentTo( first, result );
        }
    }

    [Fact]
    public void AddNode_ShouldThrowArgumentException_WhenKeyAlreadyExists()
    {
        var key = Fixture.Create<string>();
        var (otherValue, value) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key, value );

        var action = Lambda.Of( () => sut.AddNode( key, otherValue ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void TryAddNode_ShouldAddFirstNode()
    {
        var key = Fixture.Create<string>();
        var value = Fixture.Create<int>();
        var sut = new DirectedGraph<string, int, long>();

        var result = sut.TryAddNode( key, value, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().NotBeNull();
            outResult?.Key.Should().Be( key );
            outResult?.Value.Should().Be( value );
            outResult?.Graph.Should().BeSameAs( sut );
            outResult?.Edges.Should().BeEmpty();
            sut.Nodes.Should().BeEquivalentTo( outResult );
        }
    }

    [Fact]
    public void TryAddNode_ShouldAddAnotherNodeWithDifferentKey()
    {
        var (otherKey, key) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var (otherValue, value) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        var first = sut.AddNode( otherKey, otherValue );

        var result = sut.TryAddNode( key, value, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().NotBeNull();
            outResult.Should().NotBeSameAs( first );
            outResult?.Key.Should().Be( key );
            outResult?.Value.Should().Be( value );
            outResult?.Graph.Should().BeSameAs( sut );
            outResult?.Edges.Should().BeEmpty();
            sut.Nodes.Should().BeEquivalentTo( first, outResult );
        }
    }

    [Fact]
    public void TryAddNode_ShouldDoNothing_WhenKeyAlreadyExists()
    {
        var key = Fixture.Create<string>();
        var (otherValue, value) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        var first = sut.AddNode( key, value );

        var result = sut.TryAddNode( key, otherValue, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
            first.Value.Should().Be( value );
            sut.Nodes.Should().BeEquivalentTo( first );
        }
    }

    [Fact]
    public void GetOrAddNode_ShouldAddNewNode_WhenKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var value = Fixture.Create<int>();
        var sut = new DirectedGraph<string, int, long>();

        var result = sut.GetOrAddNode( key, value );

        using ( new AssertionScope() )
        {
            result.Should().NotBeNull();
            result.Key.Should().Be( key );
            result.Value.Should().Be( value );
            result.Graph.Should().BeSameAs( sut );
            result.Edges.Should().BeEmpty();
            sut.Nodes.Should().BeEquivalentTo( result );
        }
    }

    [Fact]
    public void GetOrAddNode_ShouldReturnExistingNode_WhenKeyAlreadyExists()
    {
        var key = Fixture.Create<string>();
        var (otherValue, value) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        var first = sut.AddNode( key, value );

        var result = sut.GetOrAddNode( key, otherValue );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( first );
            result.Value.Should().Be( value );
            sut.Nodes.Should().BeEquivalentTo( first );
        }
    }

    [Fact]
    public void ContainsNode_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();

        var result = sut.ContainsNode( key );

        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsNode_ShouldReturnTrue_WhenKeyExists()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key, Fixture.Create<int>() );

        var result = sut.ContainsNode( key );

        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_WithNode_ShouldReturnFalse_WhenNodeBelongsToDifferentGraph()
    {
        var key = Fixture.Create<string>();
        var other = new DirectedGraph<string, int, long>();
        var node = other.AddNode( key, Fixture.Create<int>() );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key, Fixture.Create<int>() );

        var result = sut.Contains( node );

        result.Should().BeFalse();
    }

    [Fact]
    public void Contains_WithNode_ShouldReturnTrue_WhenNodeBelongsToTheGraph()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        var node = sut.AddNode( key, Fixture.Create<int>() );

        var result = sut.Contains( node );

        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_WithAbstractNode_ShouldReturnFalse_WhenNodeBelongsToDifferentGraph()
    {
        var key = Fixture.Create<string>();
        var other = new DirectedGraph<string, int, long>();
        IDirectedGraphNode<string, int, long> node = other.AddNode( key, Fixture.Create<int>() );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key, Fixture.Create<int>() );

        var result = sut.Contains( node );

        result.Should().BeFalse();
    }

    [Fact]
    public void Contains_WithAbstractNode_ShouldReturnFalse_WhenNodeIsOfUnknownType()
    {
        var node = Substitute.For<IDirectedGraphNode<string, int, long>>();
        var sut = new DirectedGraph<string, int, long>();

        var result = sut.Contains( node );

        result.Should().BeFalse();
    }

    [Fact]
    public void Contains_WithAbstractNode_ShouldReturnTrue_WhenNodeBelongsToTheGraph()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        IDirectedGraphNode<string, int, long> node = sut.AddNode( key, Fixture.Create<int>() );

        var result = sut.Contains( node );

        result.Should().BeTrue();
    }

    [Fact]
    public void GetNode_ShouldThrowKeyNotFoundException_WhenKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();

        var action = Lambda.Of( () => sut.GetNode( key ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void GetNode_ShouldReturnCorrectNode_WhenKeyExists()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        var node = sut.AddNode( key, Fixture.Create<int>() );

        var result = sut.GetNode( key );

        result.Should().BeSameAs( node );
    }

    [Fact]
    public void TryGetNode_ShouldReturnNull_WhenKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();

        var result = sut.TryGetNode( key, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void TryGetNode_ShouldReturnCorrectNode_WhenKeyExists()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        var node = sut.AddNode( key, Fixture.Create<int>() );

        var result = sut.TryGetNode( key, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().BeSameAs( node );
        }
    }

    [Theory]
    [InlineData( GraphDirection.In )]
    [InlineData( GraphDirection.Out )]
    [InlineData( GraphDirection.Both )]
    public void AddEdge_ShouldAddEdgeToSelfWithBothDirection(GraphDirection direction)
    {
        var value = Fixture.Create<long>();
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", 10 );

        var result = sut.AddEdge( a.Key, a.Key, value, direction );

        using ( new AssertionScope() )
        {
            result.Source.Should().BeSameAs( a );
            result.Target.Should().BeSameAs( a );
            result.Value.Should().Be( value );
            result.Direction.Should().Be( GraphDirection.Both );
            a.Edges.Should().BeEquivalentTo( result );
        }
    }

    [Theory]
    [InlineData( GraphDirection.In )]
    [InlineData( GraphDirection.Out )]
    [InlineData( GraphDirection.Both )]
    public void AddEdge_ShouldAddEdgeToOtherNode(GraphDirection direction)
    {
        var value = Fixture.Create<long>();
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", 10 );
        var b = sut.AddNode( "b", 20 );

        var result = sut.AddEdge( a.Key, b.Key, value, direction );

        using ( new AssertionScope() )
        {
            result.Source.Should().BeSameAs( a );
            result.Target.Should().BeSameAs( b );
            result.Value.Should().Be( value );
            result.Direction.Should().Be( direction );
            a.Edges.Should().BeEquivalentTo( result );
            b.Edges.Should().BeEquivalentTo( result );
        }
    }

    [Fact]
    public void AddEdge_ShouldThrowArgumentException_WhenDirectionEqualsNone()
    {
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", 10 );

        var action = Lambda.Of( () => sut.AddEdge( a.Key, a.Key, Fixture.Create<long>(), GraphDirection.None ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void AddEdge_ShouldThrowKeyNotFoundException_WhenSourceNodeDoesNotExist()
    {
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", 10 );

        var action = Lambda.Of( () => sut.AddEdge( "b", a.Key, Fixture.Create<long>() ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void AddEdge_ShouldThrowKeyNotFoundException_WhenTargetNodeDoesNotExist()
    {
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", 10 );

        var action = Lambda.Of( () => sut.AddEdge( a.Key, "b", Fixture.Create<long>() ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Theory]
    [InlineData( GraphDirection.In )]
    [InlineData( GraphDirection.Out )]
    [InlineData( GraphDirection.Both )]
    public void TryAddEdge_ShouldAddEdgeToSelfWithBothDirection(GraphDirection direction)
    {
        var value = Fixture.Create<long>();
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", 10 );

        var result = sut.TryAddEdge( a.Key, a.Key, value, direction, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().NotBeNull();
            outResult?.Source.Should().BeSameAs( a );
            outResult?.Source.Should().BeSameAs( a );
            outResult?.Value.Should().Be( value );
            outResult?.Direction.Should().Be( GraphDirection.Both );
            a.Edges.Should().BeEquivalentTo( outResult );
        }
    }

    [Theory]
    [InlineData( GraphDirection.In )]
    [InlineData( GraphDirection.Out )]
    [InlineData( GraphDirection.Both )]
    public void TryAddEdge_ShouldAddEdgeToOtherNode(GraphDirection direction)
    {
        var value = Fixture.Create<long>();
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", 10 );
        var b = sut.AddNode( "b", 20 );

        var result = sut.TryAddEdge( a.Key, b.Key, value, direction, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().NotBeNull();
            outResult?.Source.Should().BeSameAs( a );
            outResult?.Target.Should().BeSameAs( b );
            outResult?.Value.Should().Be( value );
            outResult?.Direction.Should().Be( direction );
            a.Edges.Should().BeEquivalentTo( outResult );
            b.Edges.Should().BeEquivalentTo( outResult );
        }
    }

    [Fact]
    public void TryAddEdge_ShouldThrowArgumentException_WhenDirectionEqualsNone()
    {
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", 10 );

        var action = Lambda.Of( () => sut.TryAddEdge( a.Key, a.Key, Fixture.Create<long>(), GraphDirection.None, out _ ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void TryAddEdge_ShouldReturnFalse_WhenSourceNodeDoesNotExist()
    {
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", 10 );

        var result = sut.TryAddEdge( "b", a.Key, Fixture.Create<long>(), GraphDirection.Both, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void TryAddEdge_ShouldReturnFalse_WhenTargetNodeDoesNotExist()
    {
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", 10 );

        var result = sut.TryAddEdge( a.Key, "b", Fixture.Create<long>(), GraphDirection.Both, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void ContainsEdge_ShouldReturnFalse_WhenSourceKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key, Fixture.Create<int>() );

        var result = sut.ContainsEdge( "a", key );

        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsEdge_ShouldReturnFalse_WhenTargetKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key, Fixture.Create<int>() );

        var result = sut.ContainsEdge( key, "a" );

        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsEdge_ShouldReturnFalse_WhenEdgeDoesNotExist()
    {
        var (key1, key2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );

        var result = sut.ContainsEdge( key1, key2 );

        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsEdge_ShouldReturnTrue_WhenEdgeExistsAndEdgeSourceIsFirst()
    {
        var (key1, key2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );
        sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.ContainsEdge( key1, key2 );

        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsEdge_ShouldReturnTrue_WhenEdgeExistsAndEdgeTargetIsFirst()
    {
        var (key1, key2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );
        sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.ContainsEdge( key2, key1 );

        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_WithEdge_ShouldReturnFalse_WhenEdgeBelongsToDifferentGraph()
    {
        var (key1, key2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var other = new DirectedGraph<string, int, long>();
        other.AddNode( key1, Fixture.Create<int>() );
        other.AddNode( key2, Fixture.Create<int>() );
        var edge = other.AddEdge( key1, key2, Fixture.Create<long>() );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );
        sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.Contains( edge );

        result.Should().BeFalse();
    }

    [Fact]
    public void Contains_WithEdge_ShouldReturnTrue_WhenEdgeBelongsToTheGraph()
    {
        var (key1, key2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );
        var edge = sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.Contains( edge );

        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_WithEdge_ShouldReturnFalse_WhenEdgeHasBeenRemovedFromTheGraph()
    {
        var (key1, key2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );
        var edge = sut.AddEdge( key1, key2, Fixture.Create<long>() );
        sut.Remove( edge );

        var result = sut.Contains( edge );

        result.Should().BeFalse();
    }

    [Fact]
    public void Contains_WithAbstractEdge_ShouldReturnFalse_WhenEdgeBelongsToDifferentGraph()
    {
        var (key1, key2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var other = new DirectedGraph<string, int, long>();
        other.AddNode( key1, Fixture.Create<int>() );
        other.AddNode( key2, Fixture.Create<int>() );
        IDirectedGraphEdge<string, int, long> edge = other.AddEdge( key1, key2, Fixture.Create<long>() );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );
        sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.Contains( edge );

        result.Should().BeFalse();
    }

    [Fact]
    public void Contains_WithAbstractEdge_ShouldReturnFalse_WhenEdgeIsOfUnknownType()
    {
        var edge = Substitute.For<IDirectedGraphEdge<string, int, long>>();
        var sut = new DirectedGraph<string, int, long>();

        var result = sut.Contains( edge );

        result.Should().BeFalse();
    }

    [Fact]
    public void Contains_WithAbstractEdge_ShouldReturnTrue_WhenEdgeBelongsToTheGraph()
    {
        var (key1, key2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );
        IDirectedGraphEdge<string, int, long> edge = sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.Contains( edge );

        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_WithAbstractEdge_ShouldReturnFalse_WhenEdgeHasBeenRemovedFromTheGraph()
    {
        var (key1, key2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );
        IDirectedGraphEdge<string, int, long> edge = sut.AddEdge( key1, key2, Fixture.Create<long>() );
        sut.Remove( edge );

        var result = sut.Contains( edge );

        result.Should().BeFalse();
    }

    [Fact]
    public void GetEdge_ShouldThrowKeyNotFoundException_WhenSourceKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );

        var action = Lambda.Of( () => sut.GetEdge( key, "a" ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void GetEdge_ShouldThrowKeyNotFoundException_WhenTargetKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );

        var action = Lambda.Of( () => sut.GetEdge( "a", key ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void GetEdge_ShouldThrowKeyNotFoundException_WhenEdgeDoesNotExist()
    {
        var (key1, key2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );

        var action = Lambda.Of( () => sut.GetEdge( key1, key2 ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void GetEdge_ShouldReturnCorrectEdge_WhenEdgeExistsAndSourceKeyIsFirst()
    {
        var (key1, key2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );
        var edge = sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.GetEdge( key1, key2 );

        result.Should().BeSameAs( edge );
    }

    [Fact]
    public void GetEdge_ShouldReturnCorrectEdge_WhenEdgeExistsAndTargetKeyIsFirst()
    {
        var (key1, key2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );
        var edge = sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.GetEdge( key2, key1 );

        result.Should().BeSameAs( edge );
    }

    [Fact]
    public void TryGetEdge_ShouldReturnFalse_WhenSourceKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );

        var result = sut.TryGetEdge( key, "a", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void TryGetEdge_ShouldReturnFalse_WhenTargetKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );

        var result = sut.TryGetEdge( "a", key, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void TryGetEdge_ShouldReturnFalse_WhenEdgeDoesNotExist()
    {
        var (key1, key2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );

        var result = sut.TryGetEdge( key1, key2, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void TryGetEdge_ShouldReturnCorrectEdge_WhenEdgeExistsAndSourceKeyIsFirst()
    {
        var (key1, key2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );
        var edge = sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.TryGetEdge( key1, key2, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().BeSameAs( edge );
        }
    }

    [Fact]
    public void TryGetEdge_ShouldReturnCorrectEdge_WhenEdgeExistsAndTargetKeyIsFirst()
    {
        var (key1, key2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );
        var edge = sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.TryGetEdge( key2, key1, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().BeSameAs( edge );
        }
    }

    [Fact]
    public void RemoveNode_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var sut = new DirectedGraph<string, int, long>();
        var node = sut.AddNode( "a", Fixture.Create<int>() );

        var result = sut.RemoveNode( "b" );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Nodes.Should().BeEquivalentTo( node );
        }
    }

    [Fact]
    public void RemoveNode_ShouldReturnTrueAndRemoveNodeAndAssociatedEdges_WhenKeyExists()
    {
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", Fixture.Create<int>() );
        var b = sut.AddNode( "b", Fixture.Create<int>() );
        var c = sut.AddNode( "c", Fixture.Create<int>() );
        var aa = sut.AddEdge( a.Key, a.Key, Fixture.Create<long>() );
        var ab = sut.AddEdge( a.Key, b.Key, Fixture.Create<long>() );
        var ca = sut.AddEdge( c.Key, a.Key, Fixture.Create<long>() );
        var bc = sut.AddEdge( b.Key, c.Key, Fixture.Create<long>() );

        var result = sut.RemoveNode( a.Key );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Nodes.Should().BeEquivalentTo( b, c );
            a.Graph.Should().BeNull();
            a.Edges.Should().BeEmpty();
            b.Edges.Should().BeEquivalentTo( bc );
            c.Edges.Should().BeEquivalentTo( bc );
            aa.Direction.Should().Be( GraphDirection.None );
            ab.Direction.Should().Be( GraphDirection.None );
            ca.Direction.Should().Be( GraphDirection.None );
        }
    }

    [Fact]
    public void RemoveNode_WithOutValue_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var sut = new DirectedGraph<string, int, long>();
        var node = sut.AddNode( "a", Fixture.Create<int>() );

        var result = sut.RemoveNode( "b", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().Be( default );
            sut.Nodes.Should().BeEquivalentTo( node );
        }
    }

    [Fact]
    public void RemoveNode_WithOutValue_ShouldReturnTrueAndRemoveNodeAndAssociatedEdges_WhenKeyExists()
    {
        var values = Fixture.CreateDistinctCollection<int>( count: 3 );
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", values[0] );
        var b = sut.AddNode( "b", values[1] );
        var c = sut.AddNode( "c", values[2] );
        var aa = sut.AddEdge( a.Key, a.Key, Fixture.Create<long>() );
        var ab = sut.AddEdge( a.Key, b.Key, Fixture.Create<long>() );
        var ca = sut.AddEdge( c.Key, a.Key, Fixture.Create<long>() );
        var bc = sut.AddEdge( b.Key, c.Key, Fixture.Create<long>() );

        var result = sut.RemoveNode( a.Key, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( values[0] );
            sut.Nodes.Should().BeEquivalentTo( b, c );
            a.Graph.Should().BeNull();
            a.Edges.Should().BeEmpty();
            b.Edges.Should().BeEquivalentTo( bc );
            c.Edges.Should().BeEquivalentTo( bc );
            aa.Direction.Should().Be( GraphDirection.None );
            ab.Direction.Should().Be( GraphDirection.None );
            ca.Direction.Should().Be( GraphDirection.None );
        }
    }

    [Fact]
    public void Remove_WithNode_ShouldReturnFalse_WhenNodeBelongsToDifferentGraph()
    {
        var other = new DirectedGraph<string, int, long>();
        var node = other.AddNode( "a", Fixture.Create<int>() );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );

        var result = sut.Remove( node );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Nodes.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void Remove_WithNode_ShouldReturnFalse_WhenNodeHasAlreadyBeenRemoved()
    {
        var sut = new DirectedGraph<string, int, long>();
        var node = sut.AddNode( "a", Fixture.Create<int>() );
        sut.RemoveNode( node.Key );

        var result = sut.Remove( node );

        result.Should().BeFalse();
    }

    [Fact]
    public void Remove_WithNode_ShouldReturnTrueAndRemoveNodeAndAssociatedEdges_WhenNodeBelongsToTheGraph()
    {
        var values = Fixture.CreateDistinctCollection<int>( count: 3 );
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", values[0] );
        var b = sut.AddNode( "b", values[1] );
        var c = sut.AddNode( "c", values[2] );
        var aa = sut.AddEdge( a.Key, a.Key, Fixture.Create<long>() );
        var ab = sut.AddEdge( a.Key, b.Key, Fixture.Create<long>() );
        var ca = sut.AddEdge( c.Key, a.Key, Fixture.Create<long>() );
        var bc = sut.AddEdge( b.Key, c.Key, Fixture.Create<long>() );

        var result = sut.Remove( a );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Nodes.Should().BeEquivalentTo( b, c );
            a.Graph.Should().BeNull();
            a.Edges.Should().BeEmpty();
            b.Edges.Should().BeEquivalentTo( bc );
            c.Edges.Should().BeEquivalentTo( bc );
            aa.Direction.Should().Be( GraphDirection.None );
            ab.Direction.Should().Be( GraphDirection.None );
            ca.Direction.Should().Be( GraphDirection.None );
        }
    }

    [Fact]
    public void Remove_WithAbstractNode_ShouldReturnFalse_WhenNodeIsOfUnknownType()
    {
        var node = Substitute.For<IDirectedGraphNode<string, int, long>>();
        var sut = new DirectedGraph<string, int, long>();

        var result = sut.Remove( node );

        result.Should().BeFalse();
    }

    [Fact]
    public void Remove_WithAbstractNode_ShouldReturnFalse_WhenNodeBelongsToDifferentGraph()
    {
        var other = new DirectedGraph<string, int, long>();
        IDirectedGraphNode<string, int, long> node = other.AddNode( "a", Fixture.Create<int>() );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );

        var result = sut.Remove( node );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Nodes.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void Remove_WithAbstractNode_ShouldReturnFalse_WhenNodeHasAlreadyBeenRemoved()
    {
        var sut = new DirectedGraph<string, int, long>();
        IDirectedGraphNode<string, int, long> node = sut.AddNode( "a", Fixture.Create<int>() );
        sut.RemoveNode( node.Key );

        var result = sut.Remove( node );

        result.Should().BeFalse();
    }

    [Fact]
    public void Remove_WithAbstractNode_ShouldReturnTrueAndRemoveNodeAndAssociatedEdges_WhenNodeBelongsToTheGraph()
    {
        var values = Fixture.CreateDistinctCollection<int>( count: 3 );
        var sut = new DirectedGraph<string, int, long>();
        IDirectedGraphNode<string, int, long> a = sut.AddNode( "a", values[0] );
        var b = sut.AddNode( "b", values[1] );
        var c = sut.AddNode( "c", values[2] );
        var aa = sut.AddEdge( a.Key, a.Key, Fixture.Create<long>() );
        var ab = sut.AddEdge( a.Key, b.Key, Fixture.Create<long>() );
        var ca = sut.AddEdge( c.Key, a.Key, Fixture.Create<long>() );
        var bc = sut.AddEdge( b.Key, c.Key, Fixture.Create<long>() );

        var result = sut.Remove( a );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Nodes.Should().BeEquivalentTo( b, c );
            a.Graph.Should().BeNull();
            a.Edges.Should().BeEmpty();
            b.Edges.Should().BeEquivalentTo( bc );
            c.Edges.Should().BeEquivalentTo( bc );
            aa.Direction.Should().Be( GraphDirection.None );
            ab.Direction.Should().Be( GraphDirection.None );
            ca.Direction.Should().Be( GraphDirection.None );
        }
    }

    [Fact]
    public void RemoveEdge_ShouldReturnFalse_WhenSourceKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );

        var result = sut.RemoveEdge( key, "a" );

        result.Should().BeFalse();
    }

    [Fact]
    public void RemoveEdge_ShouldReturnFalse_WhenTargetKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );

        var result = sut.RemoveEdge( "a", key );

        result.Should().BeFalse();
    }

    [Fact]
    public void RemoveEdge_ShouldReturnFalse_WhenEdgeDoesNotExist()
    {
        var (key1, key2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );

        var result = sut.RemoveEdge( key1, key2 );

        result.Should().BeFalse();
    }

    [Fact]
    public void RemoveEdge_ShouldReturnTrueAndRemoveEdge_WhenEdgeToSelfExists()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        var node = sut.AddNode( key, Fixture.Create<int>() );
        var edge = sut.AddEdge( key, key, Fixture.Create<long>() );

        var result = sut.RemoveEdge( key, key );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            node.Edges.Should().BeEmpty();
            edge.Direction.Should().Be( GraphDirection.None );
        }
    }

    [Fact]
    public void RemoveEdge_ShouldReturnTrueAndRemoveEdge_WhenEdgeExistsAndSourceKeyIsFirst()
    {
        var (key1, key2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        var source = sut.AddNode( key1, Fixture.Create<int>() );
        var target = sut.AddNode( key2, Fixture.Create<int>() );
        var edge = sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.RemoveEdge( key1, key2 );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            source.Edges.Should().BeEmpty();
            target.Edges.Should().BeEmpty();
            edge.Direction.Should().Be( GraphDirection.None );
        }
    }

    [Fact]
    public void RemoveEdge_ShouldReturnTrueAndRemoveEdge_WhenEdgeExistsAndTargetKeyIsFirst()
    {
        var (key1, key2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        var source = sut.AddNode( key1, Fixture.Create<int>() );
        var target = sut.AddNode( key2, Fixture.Create<int>() );
        var edge = sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.RemoveEdge( key2, key1 );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            source.Edges.Should().BeEmpty();
            target.Edges.Should().BeEmpty();
            edge.Direction.Should().Be( GraphDirection.None );
        }
    }

    [Fact]
    public void RemoveEdge_WithOutResult_ShouldReturnFalse_WhenSourceKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );

        var result = sut.RemoveEdge( key, "a", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().Be( default );
        }
    }

    [Fact]
    public void RemoveEdge_WithOutResult_ShouldReturnFalse_WhenTargetKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );

        var result = sut.RemoveEdge( "a", key, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().Be( default );
        }
    }

    [Fact]
    public void RemoveEdge_WithOutResult_ShouldReturnFalse_WhenEdgeDoesNotExist()
    {
        var (key1, key2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );

        var result = sut.RemoveEdge( key1, key2, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().Be( default );
        }
    }

    [Fact]
    public void RemoveEdge_WithOutResult_ShouldReturnTrueAndRemoveEdge_WhenEdgeToSelfExists()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        var node = sut.AddNode( key, Fixture.Create<int>() );
        var edge = sut.AddEdge( key, key, Fixture.Create<long>() );

        var result = sut.RemoveEdge( key, key, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( edge.Value );
            node.Edges.Should().BeEmpty();
            edge.Direction.Should().Be( GraphDirection.None );
        }
    }

    [Fact]
    public void RemoveEdge_WithOutResult_ShouldReturnTrueAndRemoveEdge_WhenEdgeExistsAndSourceKeyIsFirst()
    {
        var (key1, key2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        var source = sut.AddNode( key1, Fixture.Create<int>() );
        var target = sut.AddNode( key2, Fixture.Create<int>() );
        var edge = sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.RemoveEdge( key1, key2, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( edge.Value );
            source.Edges.Should().BeEmpty();
            target.Edges.Should().BeEmpty();
            edge.Direction.Should().Be( GraphDirection.None );
        }
    }

    [Fact]
    public void RemoveEdge_WithOutResult_ShouldReturnTrueAndRemoveEdge_WhenEdgeExistsAndTargetKeyIsFirst()
    {
        var (key1, key2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        var source = sut.AddNode( key1, Fixture.Create<int>() );
        var target = sut.AddNode( key2, Fixture.Create<int>() );
        var edge = sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.RemoveEdge( key2, key1, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( edge.Value );
            source.Edges.Should().BeEmpty();
            target.Edges.Should().BeEmpty();
            edge.Direction.Should().Be( GraphDirection.None );
        }
    }

    [Fact]
    public void Remove_WithEdge_ShouldReturnFalse_WhenEdgeIsFromDifferentGraph()
    {
        var other = new DirectedGraph<string, int, long>();
        other.AddNode( "a", Fixture.Create<int>() );
        other.AddNode( "b", Fixture.Create<int>() );
        var edge = other.AddEdge( "a", "b", Fixture.Create<long>() );
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", Fixture.Create<int>() );
        var b = sut.AddNode( "b", Fixture.Create<int>() );
        sut.AddEdge( "a", "b", Fixture.Create<long>() );

        var result = sut.Remove( edge );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            a.Edges.Should().HaveCount( 1 );
            b.Edges.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void Remove_WithEdge_ShouldReturnFalse_WhenEdgeHasAlreadyBeenRemoved()
    {
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );
        sut.AddNode( "b", Fixture.Create<int>() );
        var edge = sut.AddEdge( "a", "b", Fixture.Create<long>() );
        sut.RemoveEdge( "a", "b" );

        var result = sut.Remove( edge );

        result.Should().BeFalse();
    }

    [Fact]
    public void Remove_WithEdge_ShouldReturnFalse_WhenSourceNodeHasBeenRemoved()
    {
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );
        sut.AddNode( "b", Fixture.Create<int>() );
        var edge = sut.AddEdge( "a", "b", Fixture.Create<long>() );
        sut.RemoveNode( "a" );

        var result = sut.Remove( edge );

        result.Should().BeFalse();
    }

    [Fact]
    public void Remove_WithEdge_ShouldReturnFalse_WhenTargetNodeHasBeenRemoved()
    {
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );
        sut.AddNode( "b", Fixture.Create<int>() );
        var edge = sut.AddEdge( "a", "b", Fixture.Create<long>() );
        sut.RemoveNode( "b" );

        var result = sut.Remove( edge );

        result.Should().BeFalse();
    }

    [Fact]
    public void Remove_WithEdge_ShouldReturnTrueAndRemoveEdge_WhenEdgeBelongsToTheGraph()
    {
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", Fixture.Create<int>() );
        var b = sut.AddNode( "b", Fixture.Create<int>() );
        var edge = sut.AddEdge( "a", "b", Fixture.Create<long>() );

        var result = sut.Remove( edge );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            edge.Direction.Should().Be( GraphDirection.None );
            a.Edges.Should().BeEmpty();
            b.Edges.Should().BeEmpty();
        }
    }

    [Fact]
    public void Remove_WithAbstractEdge_ShouldReturnFalse_WhenEdgeIsOfUnknownType()
    {
        var edge = Substitute.For<IDirectedGraphEdge<string, int, long>>();
        var sut = new DirectedGraph<string, int, long>();

        var result = sut.Remove( edge );

        result.Should().BeFalse();
    }

    [Fact]
    public void Remove_WithAbstractEdge_ShouldReturnFalse_WhenEdgeIsFromDifferentGraph()
    {
        var other = new DirectedGraph<string, int, long>();
        other.AddNode( "a", Fixture.Create<int>() );
        other.AddNode( "b", Fixture.Create<int>() );
        IDirectedGraphEdge<string, int, long> edge = other.AddEdge( "a", "b", Fixture.Create<long>() );
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", Fixture.Create<int>() );
        var b = sut.AddNode( "b", Fixture.Create<int>() );
        sut.AddEdge( "a", "b", Fixture.Create<long>() );

        var result = sut.Remove( edge );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            a.Edges.Should().HaveCount( 1 );
            b.Edges.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void Remove_WithAbstractEdge_ShouldReturnFalse_WhenEdgeHasAlreadyBeenRemoved()
    {
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );
        sut.AddNode( "b", Fixture.Create<int>() );
        IDirectedGraphEdge<string, int, long> edge = sut.AddEdge( "a", "b", Fixture.Create<long>() );
        sut.RemoveEdge( "a", "b" );

        var result = sut.Remove( edge );

        result.Should().BeFalse();
    }

    [Fact]
    public void Remove_WithAbstractEdge_ShouldReturnFalse_WhenSourceNodeHasBeenRemoved()
    {
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );
        sut.AddNode( "b", Fixture.Create<int>() );
        IDirectedGraphEdge<string, int, long> edge = sut.AddEdge( "a", "b", Fixture.Create<long>() );
        sut.RemoveNode( "a" );

        var result = sut.Remove( edge );

        result.Should().BeFalse();
    }

    [Fact]
    public void Remove_WithAbstractEdge_ShouldReturnFalse_WhenTargetNodeHasBeenRemoved()
    {
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );
        sut.AddNode( "b", Fixture.Create<int>() );
        IDirectedGraphEdge<string, int, long> edge = sut.AddEdge( "a", "b", Fixture.Create<long>() );
        sut.RemoveNode( "b" );

        var result = sut.Remove( edge );

        result.Should().BeFalse();
    }

    [Fact]
    public void Remove_WithAbstractEdge_ShouldReturnTrueAndRemoveEdge_WhenEdgeBelongsToTheGraph()
    {
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", Fixture.Create<int>() );
        var b = sut.AddNode( "b", Fixture.Create<int>() );
        IDirectedGraphEdge<string, int, long> edge = sut.AddEdge( "a", "b", Fixture.Create<long>() );

        var result = sut.Remove( edge );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            edge.Direction.Should().Be( GraphDirection.None );
            a.Edges.Should().BeEmpty();
            b.Edges.Should().BeEmpty();
        }
    }

    [Fact]
    public void Clear_ShouldRemoveAllNodesAndEdges()
    {
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", Fixture.Create<int>() );
        var b = sut.AddNode( "b", Fixture.Create<int>() );
        var c = sut.AddNode( "c", Fixture.Create<int>() );
        var aa = sut.AddEdge( a.Key, a.Key, Fixture.Create<long>() );
        var ab = sut.AddEdge( a.Key, b.Key, Fixture.Create<long>() );
        var ca = sut.AddEdge( c.Key, a.Key, Fixture.Create<long>() );
        var bc = sut.AddEdge( b.Key, c.Key, Fixture.Create<long>() );

        sut.Clear();

        using ( new AssertionScope() )
        {
            sut.Nodes.Should().BeEmpty();
            sut.Edges.Should().BeEmpty();
            a.Graph.Should().BeNull();
            a.Edges.Should().BeEmpty();
            b.Graph.Should().BeNull();
            b.Edges.Should().BeEmpty();
            c.Graph.Should().BeNull();
            c.Edges.Should().BeEmpty();
            aa.Direction.Should().Be( GraphDirection.None );
            ab.Direction.Should().Be( GraphDirection.None );
            ca.Direction.Should().Be( GraphDirection.None );
            bc.Direction.Should().Be( GraphDirection.None );
        }
    }

    [Fact]
    public void Edges_ShouldReturnEachEdgeExactlyOnce()
    {
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", Fixture.Create<int>() );
        var b = sut.AddNode( "b", Fixture.Create<int>() );
        var c = sut.AddNode( "c", Fixture.Create<int>() );
        var aa = sut.AddEdge( a.Key, a.Key, Fixture.Create<long>() );
        var ab = sut.AddEdge( a.Key, b.Key, Fixture.Create<long>() );
        var ca = sut.AddEdge( c.Key, a.Key, Fixture.Create<long>() );
        var bc = sut.AddEdge( b.Key, c.Key, Fixture.Create<long>() );

        var result = sut.Edges;

        result.Should().HaveCount( 4 ).And.BeEquivalentTo( aa, ab, ca, bc );
    }

    [Fact]
    public void IReadOnlyDirectedGraph_Edges_ShouldBeEquivalentToEdges()
    {
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", Fixture.Create<int>() );
        var b = sut.AddNode( "b", Fixture.Create<int>() );
        var c = sut.AddNode( "c", Fixture.Create<int>() );
        sut.AddEdge( a.Key, a.Key, Fixture.Create<long>() );
        sut.AddEdge( a.Key, b.Key, Fixture.Create<long>() );
        sut.AddEdge( c.Key, a.Key, Fixture.Create<long>() );
        sut.AddEdge( b.Key, c.Key, Fixture.Create<long>() );
        var expected = sut.Edges;

        var result = ((IReadOnlyDirectedGraph<string, int, long>)sut).Edges;

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void IReadOnlyDirectedGraph_GetNode_ShouldBeEquivalentToGetNode()
    {
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );
        var expected = sut.GetNode( "a" );

        var result = ((IReadOnlyDirectedGraph<string, int, long>)sut).GetNode( "a" );

        result.Should().BeSameAs( expected );
    }

    [Fact]
    public void IReadOnlyDirectedGraph_TryGetNode_ShouldBeEquivalentToTryGetNode_WhenNodeExists()
    {
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );
        var expected = sut.TryGetNode( "a", out var outExpected );

        var result = ((IReadOnlyDirectedGraph<string, int, long>)sut).TryGetNode( "a", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().Be( expected );
            outResult.Should().BeSameAs( outExpected );
        }
    }

    [Fact]
    public void IReadOnlyDirectedGraph_TryGetNode_ShouldBeEquivalentToTryGetNode_WhenNodeDoesNotExist()
    {
        var sut = new DirectedGraph<string, int, long>();
        var expected = sut.TryGetNode( "a", out var outExpected );

        var result = ((IReadOnlyDirectedGraph<string, int, long>)sut).TryGetNode( "a", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().Be( expected );
            outResult.Should().BeSameAs( outExpected );
        }
    }

    [Fact]
    public void IReadOnlyDirectedGraph_GetEdge_ShouldBeEquivalentToGetEdge()
    {
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );
        sut.AddEdge( "a", "a", Fixture.Create<long>() );
        var expected = sut.GetEdge( "a", "a" );

        var result = ((IReadOnlyDirectedGraph<string, int, long>)sut).GetEdge( "a", "a" );

        result.Should().BeSameAs( expected );
    }

    [Fact]
    public void IReadOnlyDirectedGraph_TryGetEdge_ShouldBeEquivalentToTryGetEdge_WhenEdgeExists()
    {
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );
        sut.AddEdge( "a", "a", Fixture.Create<long>() );
        var expected = sut.TryGetEdge( "a", "a", out var outExpected );

        var result = ((IReadOnlyDirectedGraph<string, int, long>)sut).TryGetEdge( "a", "a", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().Be( expected );
            outResult.Should().BeSameAs( outExpected );
        }
    }

    [Fact]
    public void IReadOnlyDirectedGraph_TryGetEdge_ShouldBeEquivalentToTryGetEdge_WhenEdgeDoesNotExist()
    {
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );
        var expected = sut.TryGetEdge( "a", "a", out var outExpected );

        var result = ((IReadOnlyDirectedGraph<string, int, long>)sut).TryGetEdge( "a", "a", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().Be( expected );
            outResult.Should().BeSameAs( outExpected );
        }
    }

    [Fact]
    public void IDirectedGraph_AddNode_ShouldBeEquivalentToAddNode()
    {
        var sut = new DirectedGraph<string, int, long>();
        var result = ((IDirectedGraph<string, int, long>)sut).AddNode( "a", Fixture.Create<int>() );
        result.Should().BeSameAs( sut.GetNode( "a" ) );
    }

    [Fact]
    public void IDirectedGraph_TryAddNode_ShouldBeEquivalentToTryAddNode()
    {
        var sut = new DirectedGraph<string, int, long>();

        var result = ((IDirectedGraph<string, int, long>)sut).TryAddNode( "a", Fixture.Create<int>(), out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().BeSameAs( sut.GetNode( "a" ) );
        }
    }

    [Fact]
    public void IDirectedGraph_GetOrAddNode_ShouldBeEquivalentToGetOrAddNode()
    {
        var sut = new DirectedGraph<string, int, long>();
        var result = ((IDirectedGraph<string, int, long>)sut).GetOrAddNode( "a", Fixture.Create<int>() );
        result.Should().BeSameAs( sut.GetOrAddNode( "a", Fixture.Create<int>() ) );
    }

    [Fact]
    public void IDirectedGraph_AddEdge_ShouldBeEquivalentToAddEdge()
    {
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );

        var result = ((IDirectedGraph<string, int, long>)sut).AddEdge( "a", "a", Fixture.Create<long>() );

        result.Should().BeSameAs( sut.GetEdge( "a", "a" ) );
    }

    [Fact]
    public void IDirectedGraph_TryAddEdge_ShouldBeEquivalentToTryAddEdge()
    {
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );

        var result = ((IDirectedGraph<string, int, long>)sut).TryAddEdge(
            "a",
            "a",
            Fixture.Create<long>(),
            GraphDirection.Both,
            out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().BeSameAs( sut.GetEdge( "a", "a" ) );
        }
    }
}
