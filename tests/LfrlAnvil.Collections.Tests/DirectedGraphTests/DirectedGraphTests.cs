using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Collections.Tests.DirectedGraphTests;

public class DirectedGraphTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateGraphWithoutAnyNodes()
    {
        var sut = new DirectedGraph<string, int, long>();
        Assertion.All(
                sut.Nodes.TestEmpty(),
                sut.Edges.TestEmpty(),
                sut.KeyComparer.TestRefEquals( EqualityComparer<string>.Default ),
                (( IDirectedGraph<string, int, long> )sut).Nodes.TestRefEquals( sut.Nodes ) )
            .Go();
    }

    [Fact]
    public void Ctor_WithExplicitComparer_ShouldCreateGraphWithoutAnyNodes()
    {
        var comparer = EqualityComparerFactory<string>.Create( (a, b) => a == b );
        var sut = new DirectedGraph<string, int, long>( comparer );

        Assertion.All(
                sut.Nodes.TestEmpty(),
                sut.Edges.TestEmpty(),
                sut.KeyComparer.TestRefEquals( comparer ),
                (( IDirectedGraph<string, int, long> )sut).Nodes.TestRefEquals( sut.Nodes ) )
            .Go();
    }

    [Fact]
    public void AddNode_ShouldAddFirstNode()
    {
        var key = Fixture.Create<string>();
        var value = Fixture.Create<int>();
        var sut = new DirectedGraph<string, int, long>();

        var result = sut.AddNode( key, value );

        Assertion.All(
                result.Key.TestEquals( key ),
                result.Value.TestEquals( value ),
                result.Graph.TestRefEquals( sut ),
                result.Edges.TestEmpty(),
                sut.Nodes.TestSequence( [ result ] ) )
            .Go();
    }

    [Fact]
    public void AddNode_ShouldAddAnotherNodeWithDifferentKey()
    {
        var (otherKey, key) = Fixture.CreateManyDistinct<string>( count: 2 );
        var (otherValue, value) = Fixture.CreateManyDistinct<int>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        var first = sut.AddNode( otherKey, otherValue );

        var result = sut.AddNode( key, value );

        Assertion.All(
                result.TestNotRefEquals( first ),
                result.Key.TestEquals( key ),
                result.Value.TestEquals( value ),
                result.Graph.TestRefEquals( sut ),
                result.Edges.TestEmpty(),
                sut.Nodes.TestSetEqual( [ first, result ] ) )
            .Go();
    }

    [Fact]
    public void AddNode_ShouldThrowArgumentException_WhenKeyAlreadyExists()
    {
        var key = Fixture.Create<string>();
        var (otherValue, value) = Fixture.CreateManyDistinct<int>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key, value );

        var action = Lambda.Of( () => sut.AddNode( key, otherValue ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void TryAddNode_ShouldAddFirstNode()
    {
        var key = Fixture.Create<string>();
        var value = Fixture.Create<int>();
        var sut = new DirectedGraph<string, int, long>();

        var result = sut.TryAddNode( key, value, out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestNotNull(),
                outResult.TestIf()
                    .NotNull(
                        r => Assertion.All(
                            "outResult",
                            r.Key.TestEquals( key ),
                            r.Value.TestEquals( value ),
                            r.Graph.TestRefEquals( sut ),
                            r.Edges.TestEmpty() ) ),
                sut.Nodes.TestSequence( [ outResult ] ) )
            .Go();
    }

    [Fact]
    public void TryAddNode_ShouldAddAnotherNodeWithDifferentKey()
    {
        var (otherKey, key) = Fixture.CreateManyDistinct<string>( count: 2 );
        var (otherValue, value) = Fixture.CreateManyDistinct<int>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        var first = sut.AddNode( otherKey, otherValue );

        var result = sut.TryAddNode( key, value, out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestNotNull(),
                outResult.TestNotRefEquals( first ),
                outResult.TestIf()
                    .NotNull(
                        r => Assertion.All(
                            "outResult",
                            r.Key.TestEquals( key ),
                            r.Value.TestEquals( value ),
                            r.Graph.TestRefEquals( sut ),
                            r.Edges.TestEmpty() ) ),
                sut.Nodes.TestSetEqual( [ first, outResult ] ) )
            .Go();
    }

    [Fact]
    public void TryAddNode_ShouldDoNothing_WhenKeyAlreadyExists()
    {
        var key = Fixture.Create<string>();
        var (otherValue, value) = Fixture.CreateManyDistinct<int>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        var first = sut.AddNode( key, value );

        var result = sut.TryAddNode( key, otherValue, out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestNull(),
                first.Value.TestEquals( value ),
                sut.Nodes.TestSequence( [ first ] ) )
            .Go();
    }

    [Fact]
    public void GetOrAddNode_ShouldAddNewNode_WhenKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var value = Fixture.Create<int>();
        var sut = new DirectedGraph<string, int, long>();

        var result = sut.GetOrAddNode( key, value );

        Assertion.All(
                result.TestNotNull(),
                result.Key.TestEquals( key ),
                result.Value.TestEquals( value ),
                result.Graph.TestRefEquals( sut ),
                result.Edges.TestEmpty(),
                sut.Nodes.TestSequence( [ result ] ) )
            .Go();
    }

    [Fact]
    public void GetOrAddNode_ShouldReturnExistingNode_WhenKeyAlreadyExists()
    {
        var key = Fixture.Create<string>();
        var (otherValue, value) = Fixture.CreateManyDistinct<int>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        var first = sut.AddNode( key, value );

        var result = sut.GetOrAddNode( key, otherValue );

        Assertion.All(
                result.TestRefEquals( first ),
                result.Value.TestEquals( value ),
                sut.Nodes.TestSequence( [ first ] ) )
            .Go();
    }

    [Fact]
    public void ContainsNode_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();

        var result = sut.ContainsNode( key );

        result.TestFalse().Go();
    }

    [Fact]
    public void ContainsNode_ShouldReturnTrue_WhenKeyExists()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key, Fixture.Create<int>() );

        var result = sut.ContainsNode( key );

        result.TestTrue().Go();
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

        result.TestFalse().Go();
    }

    [Fact]
    public void Contains_WithNode_ShouldReturnTrue_WhenNodeBelongsToTheGraph()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        var node = sut.AddNode( key, Fixture.Create<int>() );

        var result = sut.Contains( node );

        result.TestTrue().Go();
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

        result.TestFalse().Go();
    }

    [Fact]
    public void Contains_WithAbstractNode_ShouldReturnFalse_WhenNodeIsOfUnknownType()
    {
        var node = Substitute.For<IDirectedGraphNode<string, int, long>>();
        var sut = new DirectedGraph<string, int, long>();

        var result = sut.Contains( node );

        result.TestFalse().Go();
    }

    [Fact]
    public void Contains_WithAbstractNode_ShouldReturnTrue_WhenNodeBelongsToTheGraph()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        IDirectedGraphNode<string, int, long> node = sut.AddNode( key, Fixture.Create<int>() );

        var result = sut.Contains( node );

        result.TestTrue().Go();
    }

    [Fact]
    public void GetNode_ShouldThrowKeyNotFoundException_WhenKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();

        var action = Lambda.Of( () => sut.GetNode( key ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void GetNode_ShouldReturnCorrectNode_WhenKeyExists()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        var node = sut.AddNode( key, Fixture.Create<int>() );

        var result = sut.GetNode( key );

        result.TestRefEquals( node ).Go();
    }

    [Fact]
    public void TryGetNode_ShouldReturnNull_WhenKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();

        var result = sut.TryGetNode( key, out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestNull() )
            .Go();
    }

    [Fact]
    public void TryGetNode_ShouldReturnCorrectNode_WhenKeyExists()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        var node = sut.AddNode( key, Fixture.Create<int>() );

        var result = sut.TryGetNode( key, out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestRefEquals( node ) )
            .Go();
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

        Assertion.All(
                result.Source.TestRefEquals( a ),
                result.Target.TestRefEquals( a ),
                result.Value.TestEquals( value ),
                result.Direction.TestEquals( GraphDirection.Both ),
                a.Edges.TestSequence( [ result ] ) )
            .Go();
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

        Assertion.All(
                result.Source.TestRefEquals( a ),
                result.Target.TestRefEquals( b ),
                result.Value.TestEquals( value ),
                result.Direction.TestEquals( direction ),
                a.Edges.TestSequence( [ result ] ),
                b.Edges.TestSequence( [ result ] ) )
            .Go();
    }

    [Fact]
    public void AddEdge_ShouldThrowArgumentException_WhenDirectionEqualsNone()
    {
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", 10 );

        var action = Lambda.Of( () => sut.AddEdge( a.Key, a.Key, Fixture.Create<long>(), GraphDirection.None ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void AddEdge_ShouldThrowKeyNotFoundException_WhenSourceNodeDoesNotExist()
    {
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", 10 );

        var action = Lambda.Of( () => sut.AddEdge( "b", a.Key, Fixture.Create<long>() ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void AddEdge_ShouldThrowKeyNotFoundException_WhenTargetNodeDoesNotExist()
    {
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", 10 );

        var action = Lambda.Of( () => sut.AddEdge( a.Key, "b", Fixture.Create<long>() ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
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

        Assertion.All(
                result.TestTrue(),
                outResult.TestNotNull(),
                outResult.TestIf()
                    .NotNull(
                        r => Assertion.All(
                            "outResult",
                            r.Source.TestRefEquals( a ),
                            r.Source.TestRefEquals( a ),
                            r.Value.TestEquals( value ),
                            r.Direction.TestEquals( GraphDirection.Both ) ) ),
                a.Edges.TestSequence( [ outResult ] ) )
            .Go();
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

        Assertion.All(
                result.TestTrue(),
                outResult.TestNotNull(),
                outResult.TestIf()
                    .NotNull(
                        r => Assertion.All(
                            "outResult",
                            r.Source.TestRefEquals( a ),
                            r.Target.TestRefEquals( b ),
                            r.Value.TestEquals( value ),
                            r.Direction.TestEquals( direction ) ) ),
                a.Edges.TestSequence( [ outResult ] ),
                b.Edges.TestSequence( [ outResult ] ) )
            .Go();
    }

    [Fact]
    public void TryAddEdge_ShouldThrowArgumentException_WhenDirectionEqualsNone()
    {
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", 10 );

        var action = Lambda.Of( () => sut.TryAddEdge( a.Key, a.Key, Fixture.Create<long>(), GraphDirection.None, out _ ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void TryAddEdge_ShouldReturnFalse_WhenSourceNodeDoesNotExist()
    {
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", 10 );

        var result = sut.TryAddEdge( "b", a.Key, Fixture.Create<long>(), GraphDirection.Both, out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestNull() )
            .Go();
    }

    [Fact]
    public void TryAddEdge_ShouldReturnFalse_WhenTargetNodeDoesNotExist()
    {
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", 10 );

        var result = sut.TryAddEdge( a.Key, "b", Fixture.Create<long>(), GraphDirection.Both, out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestNull() )
            .Go();
    }

    [Fact]
    public void ContainsEdge_ShouldReturnFalse_WhenSourceKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key, Fixture.Create<int>() );

        var result = sut.ContainsEdge( "a", key );

        result.TestFalse().Go();
    }

    [Fact]
    public void ContainsEdge_ShouldReturnFalse_WhenTargetKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key, Fixture.Create<int>() );

        var result = sut.ContainsEdge( key, "a" );

        result.TestFalse().Go();
    }

    [Fact]
    public void ContainsEdge_ShouldReturnFalse_WhenEdgeDoesNotExist()
    {
        var (key1, key2) = Fixture.CreateManyDistinct<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );

        var result = sut.ContainsEdge( key1, key2 );

        result.TestFalse().Go();
    }

    [Fact]
    public void ContainsEdge_ShouldReturnTrue_WhenEdgeExistsAndEdgeSourceIsFirst()
    {
        var (key1, key2) = Fixture.CreateManyDistinct<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );
        sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.ContainsEdge( key1, key2 );

        result.TestTrue().Go();
    }

    [Fact]
    public void ContainsEdge_ShouldReturnTrue_WhenEdgeExistsAndEdgeTargetIsFirst()
    {
        var (key1, key2) = Fixture.CreateManyDistinct<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );
        sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.ContainsEdge( key2, key1 );

        result.TestTrue().Go();
    }

    [Fact]
    public void Contains_WithEdge_ShouldReturnFalse_WhenEdgeBelongsToDifferentGraph()
    {
        var (key1, key2) = Fixture.CreateManyDistinct<string>( count: 2 );
        var other = new DirectedGraph<string, int, long>();
        other.AddNode( key1, Fixture.Create<int>() );
        other.AddNode( key2, Fixture.Create<int>() );
        var edge = other.AddEdge( key1, key2, Fixture.Create<long>() );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );
        sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.Contains( edge );

        result.TestFalse().Go();
    }

    [Fact]
    public void Contains_WithEdge_ShouldReturnTrue_WhenEdgeBelongsToTheGraph()
    {
        var (key1, key2) = Fixture.CreateManyDistinct<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );
        var edge = sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.Contains( edge );

        result.TestTrue().Go();
    }

    [Fact]
    public void Contains_WithEdge_ShouldReturnFalse_WhenEdgeHasBeenRemovedFromTheGraph()
    {
        var (key1, key2) = Fixture.CreateManyDistinct<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );
        var edge = sut.AddEdge( key1, key2, Fixture.Create<long>() );
        sut.Remove( edge );

        var result = sut.Contains( edge );

        result.TestFalse().Go();
    }

    [Fact]
    public void Contains_WithAbstractEdge_ShouldReturnFalse_WhenEdgeBelongsToDifferentGraph()
    {
        var (key1, key2) = Fixture.CreateManyDistinct<string>( count: 2 );
        var other = new DirectedGraph<string, int, long>();
        other.AddNode( key1, Fixture.Create<int>() );
        other.AddNode( key2, Fixture.Create<int>() );
        IDirectedGraphEdge<string, int, long> edge = other.AddEdge( key1, key2, Fixture.Create<long>() );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );
        sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.Contains( edge );

        result.TestFalse().Go();
    }

    [Fact]
    public void Contains_WithAbstractEdge_ShouldReturnFalse_WhenEdgeIsOfUnknownType()
    {
        var edge = Substitute.For<IDirectedGraphEdge<string, int, long>>();
        var sut = new DirectedGraph<string, int, long>();

        var result = sut.Contains( edge );

        result.TestFalse().Go();
    }

    [Fact]
    public void Contains_WithAbstractEdge_ShouldReturnTrue_WhenEdgeBelongsToTheGraph()
    {
        var (key1, key2) = Fixture.CreateManyDistinct<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );
        IDirectedGraphEdge<string, int, long> edge = sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.Contains( edge );

        result.TestTrue().Go();
    }

    [Fact]
    public void Contains_WithAbstractEdge_ShouldReturnFalse_WhenEdgeHasBeenRemovedFromTheGraph()
    {
        var (key1, key2) = Fixture.CreateManyDistinct<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );
        IDirectedGraphEdge<string, int, long> edge = sut.AddEdge( key1, key2, Fixture.Create<long>() );
        sut.Remove( edge );

        var result = sut.Contains( edge );

        result.TestFalse().Go();
    }

    [Fact]
    public void GetEdge_ShouldThrowKeyNotFoundException_WhenSourceKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );

        var action = Lambda.Of( () => sut.GetEdge( key, "a" ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void GetEdge_ShouldThrowKeyNotFoundException_WhenTargetKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );

        var action = Lambda.Of( () => sut.GetEdge( "a", key ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void GetEdge_ShouldThrowKeyNotFoundException_WhenEdgeDoesNotExist()
    {
        var (key1, key2) = Fixture.CreateManyDistinct<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );

        var action = Lambda.Of( () => sut.GetEdge( key1, key2 ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void GetEdge_ShouldReturnCorrectEdge_WhenEdgeExistsAndSourceKeyIsFirst()
    {
        var (key1, key2) = Fixture.CreateManyDistinct<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );
        var edge = sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.GetEdge( key1, key2 );

        result.TestRefEquals( edge ).Go();
    }

    [Fact]
    public void GetEdge_ShouldReturnCorrectEdge_WhenEdgeExistsAndTargetKeyIsFirst()
    {
        var (key1, key2) = Fixture.CreateManyDistinct<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );
        var edge = sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.GetEdge( key2, key1 );

        result.TestRefEquals( edge ).Go();
    }

    [Fact]
    public void TryGetEdge_ShouldReturnFalse_WhenSourceKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );

        var result = sut.TryGetEdge( key, "a", out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestNull() )
            .Go();
    }

    [Fact]
    public void TryGetEdge_ShouldReturnFalse_WhenTargetKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );

        var result = sut.TryGetEdge( "a", key, out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestNull() )
            .Go();
    }

    [Fact]
    public void TryGetEdge_ShouldReturnFalse_WhenEdgeDoesNotExist()
    {
        var (key1, key2) = Fixture.CreateManyDistinct<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );

        var result = sut.TryGetEdge( key1, key2, out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestNull() )
            .Go();
    }

    [Fact]
    public void TryGetEdge_ShouldReturnCorrectEdge_WhenEdgeExistsAndSourceKeyIsFirst()
    {
        var (key1, key2) = Fixture.CreateManyDistinct<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );
        var edge = sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.TryGetEdge( key1, key2, out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestRefEquals( edge ) )
            .Go();
    }

    [Fact]
    public void TryGetEdge_ShouldReturnCorrectEdge_WhenEdgeExistsAndTargetKeyIsFirst()
    {
        var (key1, key2) = Fixture.CreateManyDistinct<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );
        var edge = sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.TryGetEdge( key2, key1, out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestRefEquals( edge ) )
            .Go();
    }

    [Fact]
    public void RemoveNode_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var sut = new DirectedGraph<string, int, long>();
        var node = sut.AddNode( "a", Fixture.Create<int>() );

        var result = sut.RemoveNode( "b" );

        Assertion.All(
                result.TestFalse(),
                sut.Nodes.TestSequence( [ node ] ) )
            .Go();
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

        Assertion.All(
                result.TestTrue(),
                sut.Nodes.TestSetEqual( [ b, c ] ),
                a.Graph.TestNull(),
                a.Edges.TestEmpty(),
                b.Edges.TestSequence( [ bc ] ),
                c.Edges.TestSequence( [ bc ] ),
                aa.Direction.TestEquals( GraphDirection.None ),
                ab.Direction.TestEquals( GraphDirection.None ),
                ca.Direction.TestEquals( GraphDirection.None ) )
            .Go();
    }

    [Fact]
    public void RemoveNode_WithOutValue_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var sut = new DirectedGraph<string, int, long>();
        var node = sut.AddNode( "a", Fixture.Create<int>() );

        var result = sut.RemoveNode( "b", out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestEquals( default ),
                sut.Nodes.TestSequence( [ node ] ) )
            .Go();
    }

    [Fact]
    public void RemoveNode_WithOutValue_ShouldReturnTrueAndRemoveNodeAndAssociatedEdges_WhenKeyExists()
    {
        var values = Fixture.CreateManyDistinct<int>( count: 3 );
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", values[0] );
        var b = sut.AddNode( "b", values[1] );
        var c = sut.AddNode( "c", values[2] );
        var aa = sut.AddEdge( a.Key, a.Key, Fixture.Create<long>() );
        var ab = sut.AddEdge( a.Key, b.Key, Fixture.Create<long>() );
        var ca = sut.AddEdge( c.Key, a.Key, Fixture.Create<long>() );
        var bc = sut.AddEdge( b.Key, c.Key, Fixture.Create<long>() );

        var result = sut.RemoveNode( a.Key, out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( values[0] ),
                sut.Nodes.TestSetEqual( [ b, c ] ),
                a.Graph.TestNull(),
                a.Edges.TestEmpty(),
                b.Edges.TestSequence( [ bc ] ),
                c.Edges.TestSequence( [ bc ] ),
                aa.Direction.TestEquals( GraphDirection.None ),
                ab.Direction.TestEquals( GraphDirection.None ),
                ca.Direction.TestEquals( GraphDirection.None ) )
            .Go();
    }

    [Fact]
    public void Remove_WithNode_ShouldReturnFalse_WhenNodeBelongsToDifferentGraph()
    {
        var other = new DirectedGraph<string, int, long>();
        var node = other.AddNode( "a", Fixture.Create<int>() );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );

        var result = sut.Remove( node );

        Assertion.All(
                result.TestFalse(),
                sut.Nodes.Count.TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void Remove_WithNode_ShouldReturnFalse_WhenNodeHasAlreadyBeenRemoved()
    {
        var sut = new DirectedGraph<string, int, long>();
        var node = sut.AddNode( "a", Fixture.Create<int>() );
        sut.RemoveNode( node.Key );

        var result = sut.Remove( node );

        result.TestFalse().Go();
    }

    [Fact]
    public void Remove_WithNode_ShouldReturnTrueAndRemoveNodeAndAssociatedEdges_WhenNodeBelongsToTheGraph()
    {
        var values = Fixture.CreateManyDistinct<int>( count: 3 );
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", values[0] );
        var b = sut.AddNode( "b", values[1] );
        var c = sut.AddNode( "c", values[2] );
        var aa = sut.AddEdge( a.Key, a.Key, Fixture.Create<long>() );
        var ab = sut.AddEdge( a.Key, b.Key, Fixture.Create<long>() );
        var ca = sut.AddEdge( c.Key, a.Key, Fixture.Create<long>() );
        var bc = sut.AddEdge( b.Key, c.Key, Fixture.Create<long>() );

        var result = sut.Remove( a );

        Assertion.All(
                result.TestTrue(),
                sut.Nodes.TestSetEqual( [ b, c ] ),
                a.Graph.TestNull(),
                a.Edges.TestEmpty(),
                b.Edges.TestSequence( [ bc ] ),
                c.Edges.TestSequence( [ bc ] ),
                aa.Direction.TestEquals( GraphDirection.None ),
                ab.Direction.TestEquals( GraphDirection.None ),
                ca.Direction.TestEquals( GraphDirection.None ) )
            .Go();
    }

    [Fact]
    public void Remove_WithAbstractNode_ShouldReturnFalse_WhenNodeIsOfUnknownType()
    {
        var node = Substitute.For<IDirectedGraphNode<string, int, long>>();
        var sut = new DirectedGraph<string, int, long>();

        var result = sut.Remove( node );

        result.TestFalse().Go();
    }

    [Fact]
    public void Remove_WithAbstractNode_ShouldReturnFalse_WhenNodeBelongsToDifferentGraph()
    {
        var other = new DirectedGraph<string, int, long>();
        IDirectedGraphNode<string, int, long> node = other.AddNode( "a", Fixture.Create<int>() );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );

        var result = sut.Remove( node );

        Assertion.All(
                result.TestFalse(),
                sut.Nodes.Count.TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void Remove_WithAbstractNode_ShouldReturnFalse_WhenNodeHasAlreadyBeenRemoved()
    {
        var sut = new DirectedGraph<string, int, long>();
        IDirectedGraphNode<string, int, long> node = sut.AddNode( "a", Fixture.Create<int>() );
        sut.RemoveNode( node.Key );

        var result = sut.Remove( node );

        result.TestFalse().Go();
    }

    [Fact]
    public void Remove_WithAbstractNode_ShouldReturnTrueAndRemoveNodeAndAssociatedEdges_WhenNodeBelongsToTheGraph()
    {
        var values = Fixture.CreateManyDistinct<int>( count: 3 );
        var sut = new DirectedGraph<string, int, long>();
        IDirectedGraphNode<string, int, long> a = sut.AddNode( "a", values[0] );
        var b = sut.AddNode( "b", values[1] );
        var c = sut.AddNode( "c", values[2] );
        var aa = sut.AddEdge( a.Key, a.Key, Fixture.Create<long>() );
        var ab = sut.AddEdge( a.Key, b.Key, Fixture.Create<long>() );
        var ca = sut.AddEdge( c.Key, a.Key, Fixture.Create<long>() );
        var bc = sut.AddEdge( b.Key, c.Key, Fixture.Create<long>() );

        var result = sut.Remove( a );

        Assertion.All(
                result.TestTrue(),
                sut.Nodes.TestSetEqual( [ b, c ] ),
                a.Graph.TestNull(),
                a.Edges.TestEmpty(),
                b.Edges.TestSequence( [ bc ] ),
                c.Edges.TestSequence( [ bc ] ),
                aa.Direction.TestEquals( GraphDirection.None ),
                ab.Direction.TestEquals( GraphDirection.None ),
                ca.Direction.TestEquals( GraphDirection.None ) )
            .Go();
    }

    [Fact]
    public void RemoveEdge_ShouldReturnFalse_WhenSourceKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );

        var result = sut.RemoveEdge( key, "a" );

        result.TestFalse().Go();
    }

    [Fact]
    public void RemoveEdge_ShouldReturnFalse_WhenTargetKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );

        var result = sut.RemoveEdge( "a", key );

        result.TestFalse().Go();
    }

    [Fact]
    public void RemoveEdge_ShouldReturnFalse_WhenEdgeDoesNotExist()
    {
        var (key1, key2) = Fixture.CreateManyDistinct<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );

        var result = sut.RemoveEdge( key1, key2 );

        result.TestFalse().Go();
    }

    [Fact]
    public void RemoveEdge_ShouldReturnTrueAndRemoveEdge_WhenEdgeToSelfExists()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        var node = sut.AddNode( key, Fixture.Create<int>() );
        var edge = sut.AddEdge( key, key, Fixture.Create<long>() );

        var result = sut.RemoveEdge( key, key );

        Assertion.All(
                result.TestTrue(),
                node.Edges.TestEmpty(),
                edge.Direction.TestEquals( GraphDirection.None ) )
            .Go();
    }

    [Fact]
    public void RemoveEdge_ShouldReturnTrueAndRemoveEdge_WhenEdgeExistsAndSourceKeyIsFirst()
    {
        var (key1, key2) = Fixture.CreateManyDistinct<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        var source = sut.AddNode( key1, Fixture.Create<int>() );
        var target = sut.AddNode( key2, Fixture.Create<int>() );
        var edge = sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.RemoveEdge( key1, key2 );

        Assertion.All(
                result.TestTrue(),
                source.Edges.TestEmpty(),
                target.Edges.TestEmpty(),
                edge.Direction.TestEquals( GraphDirection.None ) )
            .Go();
    }

    [Fact]
    public void RemoveEdge_ShouldReturnTrueAndRemoveEdge_WhenEdgeExistsAndTargetKeyIsFirst()
    {
        var (key1, key2) = Fixture.CreateManyDistinct<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        var source = sut.AddNode( key1, Fixture.Create<int>() );
        var target = sut.AddNode( key2, Fixture.Create<int>() );
        var edge = sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.RemoveEdge( key2, key1 );

        Assertion.All(
                result.TestTrue(),
                source.Edges.TestEmpty(),
                target.Edges.TestEmpty(),
                edge.Direction.TestEquals( GraphDirection.None ) )
            .Go();
    }

    [Fact]
    public void RemoveEdge_WithOutResult_ShouldReturnFalse_WhenSourceKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );

        var result = sut.RemoveEdge( key, "a", out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void RemoveEdge_WithOutResult_ShouldReturnFalse_WhenTargetKeyDoesNotExist()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );

        var result = sut.RemoveEdge( "a", key, out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void RemoveEdge_WithOutResult_ShouldReturnFalse_WhenEdgeDoesNotExist()
    {
        var (key1, key2) = Fixture.CreateManyDistinct<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( key1, Fixture.Create<int>() );
        sut.AddNode( key2, Fixture.Create<int>() );

        var result = sut.RemoveEdge( key1, key2, out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void RemoveEdge_WithOutResult_ShouldReturnTrueAndRemoveEdge_WhenEdgeToSelfExists()
    {
        var key = Fixture.Create<string>();
        var sut = new DirectedGraph<string, int, long>();
        var node = sut.AddNode( key, Fixture.Create<int>() );
        var edge = sut.AddEdge( key, key, Fixture.Create<long>() );

        var result = sut.RemoveEdge( key, key, out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( edge.Value ),
                node.Edges.TestEmpty(),
                edge.Direction.TestEquals( GraphDirection.None ) )
            .Go();
    }

    [Fact]
    public void RemoveEdge_WithOutResult_ShouldReturnTrueAndRemoveEdge_WhenEdgeExistsAndSourceKeyIsFirst()
    {
        var (key1, key2) = Fixture.CreateManyDistinct<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        var source = sut.AddNode( key1, Fixture.Create<int>() );
        var target = sut.AddNode( key2, Fixture.Create<int>() );
        var edge = sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.RemoveEdge( key1, key2, out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( edge.Value ),
                source.Edges.TestEmpty(),
                target.Edges.TestEmpty(),
                edge.Direction.TestEquals( GraphDirection.None ) )
            .Go();
    }

    [Fact]
    public void RemoveEdge_WithOutResult_ShouldReturnTrueAndRemoveEdge_WhenEdgeExistsAndTargetKeyIsFirst()
    {
        var (key1, key2) = Fixture.CreateManyDistinct<string>( count: 2 );
        var sut = new DirectedGraph<string, int, long>();
        var source = sut.AddNode( key1, Fixture.Create<int>() );
        var target = sut.AddNode( key2, Fixture.Create<int>() );
        var edge = sut.AddEdge( key1, key2, Fixture.Create<long>() );

        var result = sut.RemoveEdge( key2, key1, out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( edge.Value ),
                source.Edges.TestEmpty(),
                target.Edges.TestEmpty(),
                edge.Direction.TestEquals( GraphDirection.None ) )
            .Go();
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

        Assertion.All(
                result.TestFalse(),
                a.Edges.Count.TestEquals( 1 ),
                b.Edges.Count.TestEquals( 1 ) )
            .Go();
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

        result.TestFalse().Go();
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

        result.TestFalse().Go();
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

        result.TestFalse().Go();
    }

    [Fact]
    public void Remove_WithEdge_ShouldReturnTrueAndRemoveEdge_WhenEdgeBelongsToTheGraph()
    {
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", Fixture.Create<int>() );
        var b = sut.AddNode( "b", Fixture.Create<int>() );
        var edge = sut.AddEdge( "a", "b", Fixture.Create<long>() );

        var result = sut.Remove( edge );

        Assertion.All(
                result.TestTrue(),
                edge.Direction.TestEquals( GraphDirection.None ),
                a.Edges.TestEmpty(),
                b.Edges.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Remove_WithAbstractEdge_ShouldReturnFalse_WhenEdgeIsOfUnknownType()
    {
        var edge = Substitute.For<IDirectedGraphEdge<string, int, long>>();
        var sut = new DirectedGraph<string, int, long>();

        var result = sut.Remove( edge );

        result.TestFalse().Go();
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

        Assertion.All(
                result.TestFalse(),
                a.Edges.Count.TestEquals( 1 ),
                b.Edges.Count.TestEquals( 1 ) )
            .Go();
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

        result.TestFalse().Go();
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

        result.TestFalse().Go();
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

        result.TestFalse().Go();
    }

    [Fact]
    public void Remove_WithAbstractEdge_ShouldReturnTrueAndRemoveEdge_WhenEdgeBelongsToTheGraph()
    {
        var sut = new DirectedGraph<string, int, long>();
        var a = sut.AddNode( "a", Fixture.Create<int>() );
        var b = sut.AddNode( "b", Fixture.Create<int>() );
        IDirectedGraphEdge<string, int, long> edge = sut.AddEdge( "a", "b", Fixture.Create<long>() );

        var result = sut.Remove( edge );

        Assertion.All(
                result.TestTrue(),
                edge.Direction.TestEquals( GraphDirection.None ),
                a.Edges.TestEmpty(),
                b.Edges.TestEmpty() )
            .Go();
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

        Assertion.All(
                sut.Nodes.TestEmpty(),
                sut.Edges.TestEmpty(),
                a.Graph.TestNull(),
                a.Edges.TestEmpty(),
                b.Graph.TestNull(),
                b.Edges.TestEmpty(),
                c.Graph.TestNull(),
                c.Edges.TestEmpty(),
                aa.Direction.TestEquals( GraphDirection.None ),
                ab.Direction.TestEquals( GraphDirection.None ),
                ca.Direction.TestEquals( GraphDirection.None ),
                bc.Direction.TestEquals( GraphDirection.None ) )
            .Go();
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

        var result = sut.Edges.ToList();

        Assertion.All( result.Count.TestEquals( 4 ), result.TestSetEqual( [ aa, ab, ca, bc ] ) ).Go();
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

        var result = (( IReadOnlyDirectedGraph<string, int, long> )sut).Edges;

        result.TestSequence( expected ).Go();
    }

    [Fact]
    public void IReadOnlyDirectedGraph_GetNode_ShouldBeEquivalentToGetNode()
    {
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );
        var expected = sut.GetNode( "a" );

        var result = (( IReadOnlyDirectedGraph<string, int, long> )sut).GetNode( "a" );

        result.TestRefEquals( expected ).Go();
    }

    [Fact]
    public void IReadOnlyDirectedGraph_TryGetNode_ShouldBeEquivalentToTryGetNode_WhenNodeExists()
    {
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );
        var expected = sut.TryGetNode( "a", out var outExpected );

        var result = (( IReadOnlyDirectedGraph<string, int, long> )sut).TryGetNode( "a", out var outResult );

        Assertion.All(
                result.TestEquals( expected ),
                outResult.TestRefEquals( outExpected ) )
            .Go();
    }

    [Fact]
    public void IReadOnlyDirectedGraph_TryGetNode_ShouldBeEquivalentToTryGetNode_WhenNodeDoesNotExist()
    {
        var sut = new DirectedGraph<string, int, long>();
        var expected = sut.TryGetNode( "a", out var outExpected );

        var result = (( IReadOnlyDirectedGraph<string, int, long> )sut).TryGetNode( "a", out var outResult );

        Assertion.All(
                result.TestEquals( expected ),
                outResult.TestRefEquals( outExpected ) )
            .Go();
    }

    [Fact]
    public void IReadOnlyDirectedGraph_GetEdge_ShouldBeEquivalentToGetEdge()
    {
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );
        sut.AddEdge( "a", "a", Fixture.Create<long>() );
        var expected = sut.GetEdge( "a", "a" );

        var result = (( IReadOnlyDirectedGraph<string, int, long> )sut).GetEdge( "a", "a" );

        result.TestRefEquals( expected ).Go();
    }

    [Fact]
    public void IReadOnlyDirectedGraph_TryGetEdge_ShouldBeEquivalentToTryGetEdge_WhenEdgeExists()
    {
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );
        sut.AddEdge( "a", "a", Fixture.Create<long>() );
        var expected = sut.TryGetEdge( "a", "a", out var outExpected );

        var result = (( IReadOnlyDirectedGraph<string, int, long> )sut).TryGetEdge( "a", "a", out var outResult );

        Assertion.All(
                result.TestEquals( expected ),
                outResult.TestRefEquals( outExpected ) )
            .Go();
    }

    [Fact]
    public void IReadOnlyDirectedGraph_TryGetEdge_ShouldBeEquivalentToTryGetEdge_WhenEdgeDoesNotExist()
    {
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );
        var expected = sut.TryGetEdge( "a", "a", out var outExpected );

        var result = (( IReadOnlyDirectedGraph<string, int, long> )sut).TryGetEdge( "a", "a", out var outResult );

        Assertion.All(
                result.TestEquals( expected ),
                outResult.TestRefEquals( outExpected ) )
            .Go();
    }

    [Fact]
    public void IDirectedGraph_AddNode_ShouldBeEquivalentToAddNode()
    {
        var sut = new DirectedGraph<string, int, long>();
        var result = (( IDirectedGraph<string, int, long> )sut).AddNode( "a", Fixture.Create<int>() );
        result.TestRefEquals( sut.GetNode( "a" ) ).Go();
    }

    [Fact]
    public void IDirectedGraph_TryAddNode_ShouldBeEquivalentToTryAddNode()
    {
        var sut = new DirectedGraph<string, int, long>();

        var result = (( IDirectedGraph<string, int, long> )sut).TryAddNode( "a", Fixture.Create<int>(), out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestRefEquals( sut.GetNode( "a" ) ) )
            .Go();
    }

    [Fact]
    public void IDirectedGraph_GetOrAddNode_ShouldBeEquivalentToGetOrAddNode()
    {
        var sut = new DirectedGraph<string, int, long>();
        var result = (( IDirectedGraph<string, int, long> )sut).GetOrAddNode( "a", Fixture.Create<int>() );
        result.TestRefEquals( sut.GetOrAddNode( "a", Fixture.Create<int>() ) ).Go();
    }

    [Fact]
    public void IDirectedGraph_AddEdge_ShouldBeEquivalentToAddEdge()
    {
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );

        var result = (( IDirectedGraph<string, int, long> )sut).AddEdge( "a", "a", Fixture.Create<long>() );

        result.TestRefEquals( sut.GetEdge( "a", "a" ) ).Go();
    }

    [Fact]
    public void IDirectedGraph_TryAddEdge_ShouldBeEquivalentToTryAddEdge()
    {
        var sut = new DirectedGraph<string, int, long>();
        sut.AddNode( "a", Fixture.Create<int>() );

        var result = (( IDirectedGraph<string, int, long> )sut).TryAddEdge(
            "a",
            "a",
            Fixture.Create<long>(),
            GraphDirection.Both,
            out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestRefEquals( sut.GetEdge( "a", "a" ) ) )
            .Go();
    }
}
