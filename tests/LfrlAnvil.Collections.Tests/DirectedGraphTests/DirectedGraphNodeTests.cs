using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Collections.Tests.DirectedGraphTests;

public class DirectedGraphNodeTests : TestsBase
{
    [Fact]
    public void ValueSet_ShouldUpdateValue()
    {
        var (oldValue, newValue) = Fixture.CreateManyDistinct<int>( count: 2 );
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", oldValue );

        sut.Value = newValue;

        sut.Value.TestEquals( newValue ).Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", 10 );

        var result = sut.ToString();

        result.TestEquals( "a => 10" ).Go();
    }

    [Fact]
    public void ContainsEdgeTo_ShouldReturnTrue_WhenEdgeExists()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );
        graph.AddEdge( "a", "b", Fixture.Create<long>() );

        var result = sut.ContainsEdgeTo( "b" );

        result.TestTrue().Go();
    }

    [Fact]
    public void ContainsEdgeTo_ShouldReturnFalse_WhenEdgeDoesNotExist()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );

        var result = sut.ContainsEdgeTo( "b" );

        result.TestFalse().Go();
    }

    [Fact]
    public void GetEdgeTo_ShouldReturnCorrectEdge_WhenEdgeExists()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );
        var edge = graph.AddEdge( "a", "b", Fixture.Create<long>() );

        var result = sut.GetEdgeTo( "b" );

        result.TestRefEquals( edge ).Go();
    }

    [Fact]
    public void GetEdgeTo_ShouldThrowKeyNotFoundException_WhenEdgeDoesNotExist()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );

        var action = Lambda.Of( () => sut.GetEdgeTo( "b" ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void TryGetEdgeTo_ShouldReturnCorrectEdge_WhenEdgeExists()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );
        var edge = graph.AddEdge( "a", "b", Fixture.Create<long>() );

        var result = sut.TryGetEdgeTo( "b", out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestRefEquals( edge ) )
            .Go();
    }

    [Fact]
    public void TryGetEdgeTo_ShouldReturnNull_WhenEdgeDoesNotExist()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );

        var result = sut.TryGetEdgeTo( "b", out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestNull() )
            .Go();
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

        Assertion.All(
                sut.Edges.TestSequence( [ result ] ),
                result.Source.TestRefEquals( sut ),
                result.Target.TestRefEquals( sut ),
                result.Value.TestEquals( value ),
                result.Direction.TestEquals( GraphDirection.Both ) )
            .Go();
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

        Assertion.All(
                sut.Edges.TestSequence( [ result ] ),
                target.Edges.TestSequence( [ result ] ),
                result.Source.TestRefEquals( sut ),
                result.Target.TestRefEquals( target ),
                result.Value.TestEquals( value ),
                result.Direction.TestEquals( direction ) )
            .Go();
    }

    [Fact]
    public void AddEdgeTo_ShouldThrowKeyNotFoundException_WhenTargetDoesNotExist()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );

        var action = Lambda.Of( () => sut.AddEdgeTo( "b", Fixture.Create<long>() ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void AddEdgeTo_ShouldThrowArgumentException_WhenEdgeAlreadyExists()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );
        graph.AddEdge( "a", "b", Fixture.Create<long>() );

        var action = Lambda.Of( () => sut.AddEdgeTo( "b", Fixture.Create<long>() ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void AddEdgeTo_ShouldThrowArgumentException_WhenDirectionIsEqualToNone()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );

        var action = Lambda.Of( () => sut.AddEdgeTo( "b", Fixture.Create<long>(), GraphDirection.None ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void AddEdgeTo_ShouldThrowInvalidOperationException_WhenNodeHasBeenRemovedFromGraph()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );
        graph.Remove( sut );

        var action = Lambda.Of( () => sut.AddEdgeTo( "b", Fixture.Create<long>() ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
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

        Assertion.All(
                result.TestTrue(),
                sut.Edges.TestSequence( [ outResult ] ),
                outResult.TestNotNull(),
                outResult.TestIf()
                    .NotNull(
                        r => Assertion.All(
                            "outResult",
                            r.Source.TestRefEquals( sut ),
                            r.Target.TestRefEquals( sut ),
                            r.Value.TestEquals( value ),
                            r.Direction.TestEquals( GraphDirection.Both ) ) ) )
            .Go();
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

        Assertion.All(
                result.TestTrue(),
                sut.Edges.TestSequence( [ outResult ] ),
                target.Edges.TestSequence( [ outResult ] ),
                outResult.TestNotNull(),
                outResult.TestIf()
                    .NotNull(
                        r => Assertion.All(
                            "outResult",
                            r.Source.TestRefEquals( sut ),
                            r.Target.TestRefEquals( target ),
                            r.Value.TestEquals( value ),
                            r.Direction.TestEquals( direction ) ) ) )
            .Go();
    }

    [Fact]
    public void TryAddEdgeTo_ShouldReturnFalse_WhenTargetDoesNotExist()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );

        var result = sut.TryAddEdgeTo( "b", Fixture.Create<long>(), GraphDirection.Both, out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestNull() )
            .Go();
    }

    [Fact]
    public void TryAddEdgeTo_ShouldReturnFalse_WhenEdgeAlreadyExists()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );
        graph.AddEdge( "a", "b", Fixture.Create<long>() );

        var result = sut.TryAddEdgeTo( "b", Fixture.Create<long>(), GraphDirection.Both, out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestNull() )
            .Go();
    }

    [Fact]
    public void TryAddEdgeTo_ShouldThrowArgumentException_WhenDirectionIsEqualToNone()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );

        var action = Lambda.Of( () => sut.TryAddEdgeTo( "b", Fixture.Create<long>(), GraphDirection.None, out _ ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void TryAddEdgeTo_ShouldThrowInvalidOperationException_WhenNodeHasBeenRemovedFromGraph()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );
        graph.Remove( sut );

        var action = Lambda.Of( () => sut.TryAddEdgeTo( "b", Fixture.Create<long>(), GraphDirection.Both, out _ ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
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

        Assertion.All(
                sut.Edges.TestSequence( [ result ] ),
                result.Source.TestRefEquals( sut ),
                result.Target.TestRefEquals( sut ),
                result.Value.TestEquals( value ),
                result.Direction.TestEquals( GraphDirection.Both ) )
            .Go();
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

        Assertion.All(
                sut.Edges.TestSequence( [ result ] ),
                target.Edges.TestSequence( [ result ] ),
                result.Source.TestRefEquals( sut ),
                result.Target.TestRefEquals( target ),
                result.Value.TestEquals( value ),
                result.Direction.TestEquals( direction ) )
            .Go();
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

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void AddEdgeTo_WithNode_ShouldThrowArgumentException_WhenEdgeAlreadyExists()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var target = graph.AddNode( "b", Fixture.Create<int>() );
        graph.AddEdge( "a", "b", Fixture.Create<long>() );

        var action = Lambda.Of( () => sut.AddEdgeTo( target, Fixture.Create<long>() ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void AddEdgeTo_WithNode_ShouldThrowArgumentException_WhenDirectionIsEqualToNone()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var target = graph.AddNode( "b", Fixture.Create<int>() );

        var action = Lambda.Of( () => sut.AddEdgeTo( target, Fixture.Create<long>(), GraphDirection.None ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void AddEdgeTo_WithNode_ShouldThrowInvalidOperationException_WhenNodeHasBeenRemovedFromGraph()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var target = graph.AddNode( "b", Fixture.Create<int>() );
        graph.Remove( sut );

        var action = Lambda.Of( () => sut.AddEdgeTo( target, Fixture.Create<long>() ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
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

        Assertion.All(
                result.TestTrue(),
                sut.Edges.TestSequence( [ outResult ] ),
                outResult.TestNotNull(),
                outResult.TestIf()
                    .NotNull(
                        r => Assertion.All(
                            "outResult",
                            r.Source.TestRefEquals( sut ),
                            r.Target.TestRefEquals( sut ),
                            r.Value.TestEquals( value ),
                            r.Direction.TestEquals( GraphDirection.Both ) ) ) )
            .Go();
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

        Assertion.All(
                result.TestTrue(),
                sut.Edges.TestSequence( [ outResult ] ),
                target.Edges.TestSequence( [ outResult ] ),
                outResult.TestNotNull(),
                outResult.TestIf()
                    .NotNull(
                        r => Assertion.All(
                            "outResult",
                            r.Source.TestRefEquals( sut ),
                            r.Target.TestRefEquals( target ),
                            r.Value.TestEquals( value ),
                            r.Direction.TestEquals( direction ) ) ) )
            .Go();
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

        Assertion.All(
                result.TestFalse(),
                outResult.TestNull() )
            .Go();
    }

    [Fact]
    public void TryAddEdgeTo_WithNode_ShouldReturnFalse_WhenEdgeAlreadyExists()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var target = graph.AddNode( "b", Fixture.Create<int>() );
        graph.AddEdge( "a", "b", Fixture.Create<long>() );

        var result = sut.TryAddEdgeTo( target, Fixture.Create<long>(), GraphDirection.Both, out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestNull() )
            .Go();
    }

    [Fact]
    public void TryAddEdgeTo_WithNode_ShouldThrowArgumentException_WhenDirectionIsEqualToNone()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var target = graph.AddNode( "b", Fixture.Create<int>() );

        var action = Lambda.Of( () => sut.TryAddEdgeTo( target, Fixture.Create<long>(), GraphDirection.None, out _ ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void TryAddEdgeTo_WithNode_ShouldThrowInvalidOperationException_WhenNodeHasBeenRemovedFromGraph()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var target = graph.AddNode( "b", Fixture.Create<int>() );
        graph.Remove( sut );

        var action = Lambda.Of( () => sut.TryAddEdgeTo( target, Fixture.Create<long>(), GraphDirection.Both, out _ ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void RemoveEdgeTo_ShouldReturnFalse_WhenEdgeDoesNotExist()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );

        var result = sut.RemoveEdgeTo( "b" );

        result.TestFalse().Go();
    }

    [Fact]
    public void RemoveEdgeTo_ShouldReturnTrueAndRemoveEdge_WhenEdgeToSelfExists()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var edge = graph.AddEdge( "a", "a", Fixture.Create<long>() );

        var result = sut.RemoveEdgeTo( "a" );

        Assertion.All(
                result.TestTrue(),
                sut.Edges.TestEmpty(),
                edge.Direction.TestEquals( GraphDirection.None ) )
            .Go();
    }

    [Fact]
    public void RemoveEdgeTo_ShouldReturnTrueAndRemoveEdge_WhenEdgeToOtherTargetExists()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var other = graph.AddNode( "b", Fixture.Create<int>() );
        var edge = graph.AddEdge( "a", "b", Fixture.Create<long>() );

        var result = sut.RemoveEdgeTo( "b" );

        Assertion.All(
                result.TestTrue(),
                sut.Edges.TestEmpty(),
                other.Edges.TestEmpty(),
                edge.Direction.TestEquals( GraphDirection.None ) )
            .Go();
    }

    [Fact]
    public void RemoveEdgeTo_ShouldReturnTrueAndRemoveEdge_WhenEdgeToOtherSourceExists()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var other = graph.AddNode( "b", Fixture.Create<int>() );
        var edge = graph.AddEdge( "b", "a", Fixture.Create<long>() );

        var result = sut.RemoveEdgeTo( "b" );

        Assertion.All(
                result.TestTrue(),
                sut.Edges.TestEmpty(),
                other.Edges.TestEmpty(),
                edge.Direction.TestEquals( GraphDirection.None ) )
            .Go();
    }

    [Fact]
    public void RemoveEdgeTo_WithNode_ShouldThrowInvalidOperationException_WhenNodeHasBeenRemovedFromGraph()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.Remove( sut );

        var action = Lambda.Of( () => sut.RemoveEdgeTo( "a" ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void RemoveEdgeTo_WithOutResult_ShouldReturnFalse_WhenEdgeDoesNotExist()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );

        var result = sut.RemoveEdgeTo( "b", out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void RemoveEdgeTo_WithOutResult_ShouldReturnTrueAndRemoveEdge_WhenEdgeToSelfExists()
    {
        var value = Fixture.Create<long>();
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var edge = graph.AddEdge( "a", "a", value );

        var result = sut.RemoveEdgeTo( "a", out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( value ),
                sut.Edges.TestEmpty(),
                edge.Direction.TestEquals( GraphDirection.None ) )
            .Go();
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

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( value ),
                sut.Edges.TestEmpty(),
                other.Edges.TestEmpty(),
                edge.Direction.TestEquals( GraphDirection.None ) )
            .Go();
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

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( value ),
                sut.Edges.TestEmpty(),
                other.Edges.TestEmpty(),
                edge.Direction.TestEquals( GraphDirection.None ) )
            .Go();
    }

    [Fact]
    public void RemoveEdgeTo_WithOutResult_ShouldThrowInvalidOperationException_WhenNodeHasBeenRemovedFromGraph()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.Remove( sut );

        var action = Lambda.Of( () => sut.RemoveEdgeTo( "a", out _ ) );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void Remove_WithEdge_ShouldReturnFalse_WhenEdgeDoesNotExist()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddNode( "b", Fixture.Create<int>() );
        var edge = graph.AddEdge( "b", "b", Fixture.Create<long>() );

        var result = sut.Remove( edge );

        result.TestFalse().Go();
    }

    [Fact]
    public void Remove_WithEdge_ShouldReturnFalse_WhenEdgeHasAlreadyBeenRemoved()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var edge = graph.AddEdge( "a", "a", Fixture.Create<long>() );
        graph.RemoveEdge( "a", "a" );

        var result = sut.Remove( edge );

        result.TestFalse().Go();
    }

    [Fact]
    public void Remove_WithEdge_ShouldReturnTrueAndRemoveEdge_WhenEdgeToSelfExists()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var edge = graph.AddEdge( "a", "a", Fixture.Create<long>() );

        var result = sut.Remove( edge );

        Assertion.All(
                result.TestTrue(),
                sut.Edges.TestEmpty(),
                edge.Direction.TestEquals( GraphDirection.None ) )
            .Go();
    }

    [Fact]
    public void Remove_WithEdge_ShouldReturnTrueAndRemoveEdge_WhenEdgeToOtherTargetExists()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        var other = graph.AddNode( "b", Fixture.Create<int>() );
        var edge = graph.AddEdge( "a", "b", Fixture.Create<long>() );

        var result = sut.Remove( edge );

        Assertion.All(
                result.TestTrue(),
                sut.Edges.TestEmpty(),
                other.Edges.TestEmpty(),
                edge.Direction.TestEquals( GraphDirection.None ) )
            .Go();
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

        Assertion.All(
                result.TestTrue(),
                sut.Edges.TestEmpty(),
                other.Edges.TestEmpty(),
                edge.Direction.TestEquals( GraphDirection.None ) )
            .Go();
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

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
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

        Assertion.All(
                graph.Nodes.TestSequence( [ other ] ),
                sut.Graph.TestNull(),
                sut.Edges.TestEmpty(),
                other.Edges.TestEmpty(),
                aa.Direction.TestEquals( GraphDirection.None ),
                ab.Direction.TestEquals( GraphDirection.None ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldThrowInvalidOperationException_WhenNodeHasBeenRemovedFromGraph()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.Remove( sut );

        var action = Lambda.Of( () => sut.Remove() );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
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

        Assertion.All(
                result.TestEquals( 0 ),
                sut.Edges.TestSetEqual( [ aa, ab, ac, ad, ea, fa, ga ] ),
                b.Edges.TestSequence( [ ab ] ),
                c.Edges.TestSequence( [ ac ] ),
                d.Edges.TestSequence( [ ad ] ),
                e.Edges.TestSequence( [ ea ] ),
                f.Edges.TestSequence( [ fa ] ),
                g.Edges.TestSequence( [ ga ] ),
                aa.Direction.TestEquals( GraphDirection.Both ),
                ab.Direction.TestEquals( GraphDirection.In ),
                ac.Direction.TestEquals( GraphDirection.Out ),
                ad.Direction.TestEquals( GraphDirection.Both ),
                ea.Direction.TestEquals( GraphDirection.In ),
                fa.Direction.TestEquals( GraphDirection.Out ),
                ga.Direction.TestEquals( GraphDirection.Both ) )
            .Go();
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

        Assertion.All(
                result.TestEquals( 2 ),
                sut.Edges.TestSetEqual( [ aa, ac, ad, ea, ga ] ),
                b.Edges.TestEmpty(),
                c.Edges.TestSequence( [ ac ] ),
                d.Edges.TestSequence( [ ad ] ),
                e.Edges.TestSequence( [ ea ] ),
                f.Edges.TestEmpty(),
                g.Edges.TestSequence( [ ga ] ),
                aa.Direction.TestEquals( GraphDirection.Both ),
                ab.Direction.TestEquals( GraphDirection.None ),
                ac.Direction.TestEquals( GraphDirection.Out ),
                ad.Direction.TestEquals( GraphDirection.Out ),
                ea.Direction.TestEquals( GraphDirection.In ),
                fa.Direction.TestEquals( GraphDirection.None ),
                ga.Direction.TestEquals( GraphDirection.In ) )
            .Go();
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

        Assertion.All(
                result.TestEquals( 2 ),
                sut.Edges.TestSetEqual( [ aa, ab, ad, fa, ga ] ),
                b.Edges.TestSequence( [ ab ] ),
                c.Edges.TestEmpty(),
                d.Edges.TestSequence( [ ad ] ),
                e.Edges.TestEmpty(),
                f.Edges.TestSequence( [ fa ] ),
                g.Edges.TestSequence( [ ga ] ),
                aa.Direction.TestEquals( GraphDirection.Both ),
                ab.Direction.TestEquals( GraphDirection.In ),
                ac.Direction.TestEquals( GraphDirection.None ),
                ad.Direction.TestEquals( GraphDirection.In ),
                ea.Direction.TestEquals( GraphDirection.None ),
                fa.Direction.TestEquals( GraphDirection.Out ),
                ga.Direction.TestEquals( GraphDirection.Out ) )
            .Go();
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

        Assertion.All(
                result.TestEquals( 7 ),
                sut.Edges.TestEmpty(),
                b.Edges.TestEmpty(),
                c.Edges.TestEmpty(),
                d.Edges.TestEmpty(),
                e.Edges.TestEmpty(),
                f.Edges.TestEmpty(),
                g.Edges.TestEmpty(),
                aa.Direction.TestEquals( GraphDirection.None ),
                ab.Direction.TestEquals( GraphDirection.None ),
                ac.Direction.TestEquals( GraphDirection.None ),
                ad.Direction.TestEquals( GraphDirection.None ),
                ea.Direction.TestEquals( GraphDirection.None ),
                fa.Direction.TestEquals( GraphDirection.None ),
                ga.Direction.TestEquals( GraphDirection.None ) )
            .Go();
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

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
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

        result.TestEmpty().Go();
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

        result.TestEmpty().Go();
    }

    [Fact]
    public void GetReachableNodes_ShouldReturnEmptyCollection_WhenDirectionIsEqualToNone()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddEdge( "a", "a", Fixture.Create<long>() );

        var result = sut.GetReachableNodes( GraphDirection.None );

        result.TestEmpty().Go();
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

        var result = sut.GetReachableNodes( GraphDirection.Out ).ToList();

        Assertion.All( result.Count.TestEquals( 7 ), result.TestSetEqual( [ sut, b, c, h, i, j, k ] ) ).Go();
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

        var result = sut.GetReachableNodes( GraphDirection.In ).ToList();

        Assertion.All( result.Count.TestEquals( 7 ), result.TestSetEqual( [ sut, e, f, h, i, j, k ] ) ).Go();
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

        var result = sut.GetReachableNodes( GraphDirection.Both ).ToList();

        Assertion.All( result.Count.TestEquals( 11 ), result.TestSetEqual( [ sut, b, c, d, e, f, g, h, i, j, k ] ) ).Go();
    }

    [Fact]
    public void IDirectedGraphNode_Graph_ShouldBeSameAsGraph()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", 10 );

        var result = (( IDirectedGraphNode<string, int, long> )sut).Graph;

        result.TestRefEquals( sut.Graph ).Go();
    }

    [Fact]
    public void IDirectedGraphNode_Edges_ShouldBeSameAsEdges()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", 10 );

        var result = (( IDirectedGraphNode<string, int, long> )sut).Edges;

        result.TestRefEquals( sut.Edges ).Go();
    }

    [Fact]
    public void IDirectedGraphNode_GetEdgeTo_ShouldBeEquivalentToGetEdgeTo()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddEdge( "a", "a", Fixture.Create<long>() );

        var result = (( IDirectedGraphNode<string, int, long> )sut).GetEdgeTo( "a" );

        result.TestRefEquals( sut.GetEdgeTo( "a" ) ).Go();
    }

    [Fact]
    public void IDirectedGraphNode_TryGetEdgeTo_ShouldBeEquivalentToTryGetEdgeTo()
    {
        var graph = new DirectedGraph<string, int, long>();
        var sut = graph.AddNode( "a", Fixture.Create<int>() );
        graph.AddEdge( "a", "a", Fixture.Create<long>() );
        var expected = sut.TryGetEdgeTo( "a", out var outExpected );

        var result = (( IDirectedGraphNode<string, int, long> )sut).TryGetEdgeTo( "a", out var outResult );

        Assertion.All(
                result.TestEquals( expected ),
                outResult.TestRefEquals( outExpected ) )
            .Go();
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

        var result = (( IDirectedGraphNode<string, int, long> )sut).GetReachableNodes();

        result.TestSequence( sut.GetReachableNodes() ).Go();
    }
}
