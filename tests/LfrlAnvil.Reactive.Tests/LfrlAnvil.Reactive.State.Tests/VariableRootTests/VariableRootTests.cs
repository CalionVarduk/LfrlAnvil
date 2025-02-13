using System.Collections.Generic;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Reactive.State.Internal;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.VariableRootTests;

public partial class VariableRootTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldReturnEmptyRoot()
    {
        var sut = new VariableRootMock();

        Assertion.All(
                sut.Parent.TestNull(),
                sut.State.TestEquals( VariableState.ReadOnly ),
                sut.Nodes.TestEmpty(),
                sut.Nodes.Count.TestEquals( 0 ),
                sut.Nodes.Comparer.TestRefEquals( EqualityComparer<string>.Default ),
                sut.Nodes.Keys.TestEmpty(),
                sut.Nodes.Values.TestEmpty(),
                sut.Nodes.ChangedNodeKeys.TestEmpty(),
                sut.Nodes.InvalidNodeKeys.TestEmpty(),
                sut.Nodes.WarningNodeKeys.TestEmpty(),
                sut.Nodes.ReadOnlyNodeKeys.TestEmpty(),
                sut.Nodes.DirtyNodeKeys.TestEmpty(),
                (( IVariableNodeCollection )sut.Nodes).Keys.TestRefEquals( sut.Nodes.Keys ),
                (( IVariableNodeCollection )sut.Nodes).Values.TestRefEquals( sut.Nodes.Values ),
                (( IVariableNodeCollection )sut.Nodes).ChangedNodeKeys.TestRefEquals( sut.Nodes.ChangedNodeKeys ),
                (( IVariableNodeCollection )sut.Nodes).InvalidNodeKeys.TestRefEquals( sut.Nodes.InvalidNodeKeys ),
                (( IVariableNodeCollection )sut.Nodes).WarningNodeKeys.TestRefEquals( sut.Nodes.WarningNodeKeys ),
                (( IVariableNodeCollection )sut.Nodes).ReadOnlyNodeKeys.TestRefEquals( sut.Nodes.ReadOnlyNodeKeys ),
                (( IVariableNodeCollection )sut.Nodes).DirtyNodeKeys.TestRefEquals( sut.Nodes.DirtyNodeKeys ),
                (( IReadOnlyDictionary<string, IVariableNode> )sut.Nodes).Keys.TestRefEquals( sut.Nodes.Keys ),
                (( IReadOnlyDictionary<string, IVariableNode> )sut.Nodes).Values.TestRefEquals( sut.Nodes.Values ),
                (( IReadOnlyVariableRoot<string> )sut).OnChange.TestRefEquals( sut.OnChange ),
                (( IReadOnlyVariableRoot<string> )sut).OnValidate.TestRefEquals( sut.OnValidate ),
                (( object )(( IReadOnlyVariableRoot )sut).Nodes).TestRefEquals( sut.Nodes ),
                (( IReadOnlyVariableRoot )sut).OnChange.TestRefEquals( sut.OnChange ),
                (( IReadOnlyVariableRoot )sut).OnValidate.TestRefEquals( sut.OnValidate ),
                (( IVariableNode )sut).OnChange.TestRefEquals( sut.OnChange ),
                (( IVariableNode )sut).OnValidate.TestRefEquals( sut.OnValidate ),
                (( IVariableNode )sut).GetChildren().TestEmpty() )
            .Go();
    }

    [Fact]
    public void ToString_ShouldReturnInformationAboutNodeCountAndState()
    {
        var variable = Variable.WithoutValidators<string>.Create( Fixture.Create<int>(), Fixture.Create<int>() );
        var expected = "Nodes: 1, State: Changed, ReadOnly";
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( Fixture.Create<string>(), variable );
        sut.SetReadOnly( true );

        var result = sut.ToString();

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Refresh_ShouldRefreshForAllRegisteredNodes()
    {
        var (key1, key2, key3, key4) = Fixture.CreateManyDistinct<string>( count: 4 );
        var (value1, value2, value3) = Fixture.CreateManyDistinct<int>( count: 3 );
        var variable1 = Variable.WithoutValidators<string>.Create( value1 );
        var variable2 = Variable.WithoutValidators<string>.Create( value2 );
        var variable3 = Variable.WithoutValidators<string>.Create( value3 );
        var root1 = new VariableRootMock();
        root1.ExposedRegisterNode( key4, variable3 );

        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key1, variable1 );
        sut.ExposedRegisterNode( key2, variable2 );
        sut.ExposedRegisterNode( key3, root1 );

        var changeListener = Substitute.For<IEventListener<VariableRootChangeEvent<string>>>();
        var validateListener = Substitute.For<IEventListener<VariableRootValidationEvent<string>>>();
        sut.OnChange.Listen( changeListener );
        sut.OnValidate.Listen( validateListener );

        sut.Refresh();

        Assertion.All(
                sut.State.TestEquals( VariableState.Dirty ),
                variable1.State.TestEquals( VariableState.Dirty ),
                variable2.State.TestEquals( VariableState.Dirty ),
                variable3.State.TestEquals( VariableState.Dirty ),
                root1.State.TestEquals( VariableState.Dirty ),
                changeListener.TestReceivedCalls( x => x.React( Arg.Any<VariableRootChangeEvent<string>>() ), count: 3 ),
                validateListener.TestReceivedCalls( x => x.React( Arg.Any<VariableRootValidationEvent<string>>() ), count: 3 ) )
            .Go();
    }

    [Fact]
    public void Refresh_ShouldDoNothing_WhenRootIsAlreadyDisposed()
    {
        var value = Fixture.Create<int>();
        var variable = Variable.WithoutValidators<string>.Create( value );
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( Fixture.Create<string>(), variable );
        sut.Dispose();

        sut.Refresh();

        sut.State.TestEquals( VariableState.ReadOnly | VariableState.Disposed ).Go();
    }

    [Fact]
    public void RefreshValidation_ShouldRefreshValidationForAllRegisteredNodes()
    {
        var (key1, key2, key3, key4) = Fixture.CreateManyDistinct<string>( count: 4 );
        var (value1, value2, value3) = Fixture.CreateManyDistinct<int>( count: 3 );
        var validator = Validators<string>.Fail<int>( Fixture.Create<string>() );
        var variable1 = Variable.Create( value1, errorsValidator: validator, warningsValidator: validator );
        var variable2 = Variable.Create( value2, errorsValidator: validator, warningsValidator: validator );
        var variable3 = Variable.Create( value3, errorsValidator: validator, warningsValidator: validator );
        var root1 = new VariableRootMock();
        root1.ExposedRegisterNode( key4, variable3 );

        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key1, variable1 );
        sut.ExposedRegisterNode( key2, variable2 );
        sut.ExposedRegisterNode( key3, root1 );

        var validateListener = Substitute.For<IEventListener<VariableRootValidationEvent<string>>>();
        sut.OnValidate.Listen( validateListener );

        sut.RefreshValidation();

        Assertion.All(
                sut.State.TestEquals( VariableState.Invalid | VariableState.Warning ),
                sut.Nodes.InvalidNodeKeys.TestSetEqual( [ key1, key2, key3 ] ),
                sut.Nodes.WarningNodeKeys.TestSetEqual( [ key1, key2, key3 ] ),
                variable1.State.TestEquals( VariableState.Invalid | VariableState.Warning ),
                variable2.State.TestEquals( VariableState.Invalid | VariableState.Warning ),
                variable3.State.TestEquals( VariableState.Invalid | VariableState.Warning ),
                root1.State.TestEquals( VariableState.Invalid | VariableState.Warning ),
                validateListener.TestReceivedCalls( x => x.React( Arg.Any<VariableRootValidationEvent<string>>() ), count: 3 ) )
            .Go();
    }

    [Fact]
    public void RefreshValidation_ShouldDoNothing_WhenRootIsAlreadyDisposed()
    {
        var value = Fixture.Create<int>();
        var validator = Validators<string>.Fail<int>( Fixture.Create<string>() );
        var variable = Variable.Create( value, errorsValidator: validator, warningsValidator: validator );
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( Fixture.Create<string>(), variable );
        sut.Dispose();

        sut.RefreshValidation();

        sut.State.TestEquals( VariableState.ReadOnly | VariableState.Disposed ).Go();
    }

    [Fact]
    public void ClearValidation_ShouldClearValidationForAllRegisteredNodes()
    {
        var (key1, key2, key3, key4) = Fixture.CreateManyDistinct<string>( count: 4 );
        var (value1, value2, value3) = Fixture.CreateManyDistinct<int>( count: 3 );
        var validator = Validators<string>.Fail<int>( Fixture.Create<string>() );
        var variable1 = Variable.Create( value1, errorsValidator: validator, warningsValidator: validator );
        var variable2 = Variable.Create( value2, errorsValidator: validator, warningsValidator: validator );
        var variable3 = Variable.Create( value3, errorsValidator: validator, warningsValidator: validator );
        variable1.RefreshValidation();
        variable2.RefreshValidation();
        variable3.RefreshValidation();
        var root1 = new VariableRootMock();
        root1.ExposedRegisterNode( key4, variable3 );

        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key1, variable1 );
        sut.ExposedRegisterNode( key2, variable2 );
        sut.ExposedRegisterNode( key3, root1 );

        var validateListener = Substitute.For<IEventListener<VariableRootValidationEvent<string>>>();
        sut.OnValidate.Listen( validateListener );

        sut.ClearValidation();

        Assertion.All(
                sut.State.TestEquals( VariableState.Default ),
                sut.Nodes.InvalidNodeKeys.TestEmpty(),
                sut.Nodes.WarningNodeKeys.TestEmpty(),
                variable1.State.TestEquals( VariableState.Default ),
                variable2.State.TestEquals( VariableState.Default ),
                variable3.State.TestEquals( VariableState.Default ),
                root1.State.TestEquals( VariableState.Default ),
                validateListener.TestReceivedCalls( x => x.React( Arg.Any<VariableRootValidationEvent<string>>() ), count: 3 ) )
            .Go();
    }

    [Fact]
    public void ClearValidation_ShouldDoNothing_WhenRootIsAlreadyDisposed()
    {
        var value = Fixture.Create<int>();
        var validator = Validators<string>.Fail<int>( Fixture.Create<string>() );
        var variable = Variable.Create( value, errorsValidator: validator, warningsValidator: validator );
        variable.RefreshValidation();
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( Fixture.Create<string>(), variable );
        sut.Dispose();

        sut.ClearValidation();

        sut.State.TestEquals( VariableState.ReadOnly | VariableState.Disposed | VariableState.Invalid | VariableState.Warning ).Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetReadOnly_ShouldSetReadOnlyForAllRegisteredNodes(bool enabled)
    {
        var (key1, key2, key3, key4) = Fixture.CreateManyDistinct<string>( count: 4 );
        var (value1, value2, value3) = Fixture.CreateManyDistinct<int>( count: 3 );
        var variable1 = Variable.WithoutValidators<string>.Create( value1 );
        var variable2 = Variable.WithoutValidators<string>.Create( value2 );
        var variable3 = Variable.WithoutValidators<string>.Create( value3 );
        variable1.SetReadOnly( true );
        variable3.SetReadOnly( true );
        var root1 = new VariableRootMock();
        root1.ExposedRegisterNode( key4, variable3 );

        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key1, variable1 );
        sut.ExposedRegisterNode( key2, variable2 );
        sut.ExposedRegisterNode( key3, root1 );

        var changeListener = Substitute.For<IEventListener<VariableRootChangeEvent<string>>>();
        sut.OnChange.Listen( changeListener );

        sut.SetReadOnly( enabled );

        var expectedState = enabled ? VariableState.ReadOnly : VariableState.Default;
        Assertion.All(
                sut.State.TestEquals( expectedState ),
                variable1.State.TestEquals( expectedState ),
                variable2.State.TestEquals( expectedState ),
                variable3.State.TestEquals( expectedState ),
                root1.State.TestEquals( expectedState ),
                changeListener.TestReceivedCalls( x => x.React( Arg.Any<VariableRootChangeEvent<string>>() ) ) )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetReadOnly_ShouldDoNothing_WhenRootIsAlreadyDisposed(bool enabled)
    {
        var (key1, key2) = Fixture.CreateManyDistinct<string>( count: 2 );
        var value = Fixture.Create<int>();
        var variable1 = Variable.WithoutValidators<string>.Create( value );
        var variable2 = Variable.WithoutValidators<string>.Create( value );
        variable2.SetReadOnly( true );
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key1, variable1 );
        sut.ExposedRegisterNode( key2, variable2 );
        sut.Dispose();

        sut.SetReadOnly( enabled );

        sut.State.TestEquals( VariableState.ReadOnly | VariableState.Disposed ).Go();
    }

    [Fact]
    public void Dispose_ShouldDisposeAllRegisteredNodesAndAddDisposedState()
    {
        var (key1, key2, key3, key4) = Fixture.CreateManyDistinct<string>( count: 4 );
        var (value1, value2, value3) = Fixture.CreateManyDistinct<int>( count: 3 );
        var variable1 = Variable.WithoutValidators<string>.Create( value1 );
        var variable2 = Variable.WithoutValidators<string>.Create( value2 );
        var variable3 = Variable.WithoutValidators<string>.Create( value3 );
        var root1 = new VariableRootMock();
        root1.ExposedRegisterNode( key4, variable3 );

        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key1, variable1 );
        sut.ExposedRegisterNode( key2, variable2 );
        sut.ExposedRegisterNode( key3, root1 );

        var onChange = sut.OnChange.Listen( Substitute.For<IEventListener<VariableRootChangeEvent<string>>>() );
        var onValidate = sut.OnValidate.Listen( Substitute.For<IEventListener<VariableRootValidationEvent<string>>>() );

        sut.Dispose();

        Assertion.All(
                sut.State.TestEquals( VariableState.ReadOnly | VariableState.Disposed ),
                sut.Nodes.ReadOnlyNodeKeys.TestSetEqual( [ key1, key2, key3 ] ),
                onChange.IsDisposed.TestTrue(),
                onValidate.IsDisposed.TestTrue(),
                (variable1.State & VariableState.Disposed).TestEquals( VariableState.Disposed ),
                (variable2.State & VariableState.Disposed).TestEquals( VariableState.Disposed ),
                (variable3.State & VariableState.Disposed).TestEquals( VariableState.Disposed ),
                (root1.State & VariableState.Disposed).TestEquals( VariableState.Disposed ) )
            .Go();
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenRootIsAlreadyDisposed()
    {
        var sut = new VariableRootMock();
        sut.Dispose();

        sut.Dispose();

        sut.State.TestEquals( VariableState.ReadOnly | VariableState.Disposed ).Go();
    }
}

internal sealed class VariableRootMock : VariableRoot<string>
{
    internal TNode ExposedRegisterNode<TNode>(string key, TNode node)
        where TNode : VariableNode, IDisposable, IMutableVariableNode
    {
        return RegisterNode( key, node );
    }
}
