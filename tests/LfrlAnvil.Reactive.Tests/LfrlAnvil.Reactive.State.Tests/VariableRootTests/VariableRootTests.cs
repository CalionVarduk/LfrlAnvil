using System.Collections.Generic;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Reactive.State.Internal;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.VariableRootTests;

public partial class VariableRootTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldReturnEmptyRoot()
    {
        var sut = new VariableRootMock();

        using ( new AssertionScope() )
        {
            sut.Parent.Should().BeNull();
            sut.State.Should().Be( VariableState.ReadOnly );
            sut.Nodes.Should().BeEmpty();
            sut.Nodes.Count.Should().Be( 0 );
            sut.Nodes.Comparer.Should().BeSameAs( EqualityComparer<string>.Default );
            sut.Nodes.Keys.Should().BeEmpty();
            sut.Nodes.Values.Should().BeEmpty();
            sut.Nodes.ChangedNodeKeys.Should().BeEmpty();
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEmpty();
            sut.Nodes.DirtyNodeKeys.Should().BeEmpty();

            ((IVariableNodeCollection)sut.Nodes).Keys.Should().BeSameAs( sut.Nodes.Keys );
            ((IVariableNodeCollection)sut.Nodes).Values.Should().BeSameAs( sut.Nodes.Values );
            ((IVariableNodeCollection)sut.Nodes).ChangedNodeKeys.Should().BeSameAs( sut.Nodes.ChangedNodeKeys );
            ((IVariableNodeCollection)sut.Nodes).InvalidNodeKeys.Should().BeSameAs( sut.Nodes.InvalidNodeKeys );
            ((IVariableNodeCollection)sut.Nodes).WarningNodeKeys.Should().BeSameAs( sut.Nodes.WarningNodeKeys );
            ((IVariableNodeCollection)sut.Nodes).ReadOnlyNodeKeys.Should().BeSameAs( sut.Nodes.ReadOnlyNodeKeys );
            ((IVariableNodeCollection)sut.Nodes).DirtyNodeKeys.Should().BeSameAs( sut.Nodes.DirtyNodeKeys );

            ((IReadOnlyDictionary<string, IVariableNode>)sut.Nodes).Keys.Should().BeSameAs( sut.Nodes.Keys );
            ((IReadOnlyDictionary<string, IVariableNode>)sut.Nodes).Values.Should().BeSameAs( sut.Nodes.Values );

            ((IReadOnlyVariableRoot<string>)sut).OnChange.Should().BeSameAs( sut.OnChange );
            ((IReadOnlyVariableRoot<string>)sut).OnValidate.Should().BeSameAs( sut.OnValidate );

            ((object)((IReadOnlyVariableRoot)sut).Nodes).Should().BeSameAs( sut.Nodes );
            ((IReadOnlyVariableRoot)sut).OnChange.Should().BeSameAs( sut.OnChange );
            ((IReadOnlyVariableRoot)sut).OnValidate.Should().BeSameAs( sut.OnValidate );
            ((IVariableNode)sut).OnChange.Should().BeSameAs( sut.OnChange );
            ((IVariableNode)sut).OnValidate.Should().BeSameAs( sut.OnValidate );
            ((IVariableNode)sut).GetChildren().Should().BeEmpty();
        }
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

        result.Should().Be( expected );
    }

    [Fact]
    public void Refresh_ShouldRefreshForAllRegisteredNodes()
    {
        var (key1, key2, key3, key4) = Fixture.CreateDistinctCollection<string>( count: 4 );
        var (value1, value2, value3) = Fixture.CreateDistinctCollection<int>( count: 3 );
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

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Dirty );
            variable1.State.Should().Be( VariableState.Dirty );
            variable2.State.Should().Be( VariableState.Dirty );
            variable3.State.Should().Be( VariableState.Dirty );
            root1.State.Should().Be( VariableState.Dirty );
            changeListener.VerifyCalls().Received( x => x.React( Arg.Any<VariableRootChangeEvent<string>>() ), 3 );
            validateListener.VerifyCalls().Received( x => x.React( Arg.Any<VariableRootValidationEvent<string>>() ), 3 );
        }
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

        sut.State.Should().Be( VariableState.ReadOnly | VariableState.Disposed );
    }

    [Fact]
    public void RefreshValidation_ShouldRefreshValidationForAllRegisteredNodes()
    {
        var (key1, key2, key3, key4) = Fixture.CreateDistinctCollection<string>( count: 4 );
        var (value1, value2, value3) = Fixture.CreateDistinctCollection<int>( count: 3 );
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

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Invalid | VariableState.Warning );
            sut.Nodes.InvalidNodeKeys.Should().BeEquivalentTo( key1, key2, key3 );
            sut.Nodes.WarningNodeKeys.Should().BeEquivalentTo( key1, key2, key3 );
            variable1.State.Should().Be( VariableState.Invalid | VariableState.Warning );
            variable2.State.Should().Be( VariableState.Invalid | VariableState.Warning );
            variable3.State.Should().Be( VariableState.Invalid | VariableState.Warning );
            root1.State.Should().Be( VariableState.Invalid | VariableState.Warning );
            validateListener.VerifyCalls().Received( x => x.React( Arg.Any<VariableRootValidationEvent<string>>() ), 3 );
        }
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

        sut.State.Should().Be( VariableState.ReadOnly | VariableState.Disposed );
    }

    [Fact]
    public void ClearValidation_ShouldClearValidationForAllRegisteredNodes()
    {
        var (key1, key2, key3, key4) = Fixture.CreateDistinctCollection<string>( count: 4 );
        var (value1, value2, value3) = Fixture.CreateDistinctCollection<int>( count: 3 );
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

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Default );
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            variable1.State.Should().Be( VariableState.Default );
            variable2.State.Should().Be( VariableState.Default );
            variable3.State.Should().Be( VariableState.Default );
            root1.State.Should().Be( VariableState.Default );
            validateListener.VerifyCalls().Received( x => x.React( Arg.Any<VariableRootValidationEvent<string>>() ), 3 );
        }
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

        sut.State.Should().Be( VariableState.ReadOnly | VariableState.Disposed | VariableState.Invalid | VariableState.Warning );
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetReadOnly_ShouldSetReadOnlyForAllRegisteredNodes(bool enabled)
    {
        var (key1, key2, key3, key4) = Fixture.CreateDistinctCollection<string>( count: 4 );
        var (value1, value2, value3) = Fixture.CreateDistinctCollection<int>( count: 3 );
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

        using ( new AssertionScope() )
        {
            var expectedState = enabled ? VariableState.ReadOnly : VariableState.Default;
            sut.State.Should().Be( expectedState );
            variable1.State.Should().Be( expectedState );
            variable2.State.Should().Be( expectedState );
            variable3.State.Should().Be( expectedState );
            root1.State.Should().Be( expectedState );
            changeListener.VerifyCalls().Received( x => x.React( Arg.Any<VariableRootChangeEvent<string>>() ) );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetReadOnly_ShouldDoNothing_WhenRootIsAlreadyDisposed(bool enabled)
    {
        var (key1, key2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var value = Fixture.Create<int>();
        var variable1 = Variable.WithoutValidators<string>.Create( value );
        var variable2 = Variable.WithoutValidators<string>.Create( value );
        variable2.SetReadOnly( true );
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key1, variable1 );
        sut.ExposedRegisterNode( key2, variable2 );
        sut.Dispose();

        sut.SetReadOnly( enabled );

        sut.State.Should().Be( VariableState.ReadOnly | VariableState.Disposed );
    }

    [Fact]
    public void Dispose_ShouldDisposeAllRegisteredNodesAndAddDisposedState()
    {
        var (key1, key2, key3, key4) = Fixture.CreateDistinctCollection<string>( count: 4 );
        var (value1, value2, value3) = Fixture.CreateDistinctCollection<int>( count: 3 );
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

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.ReadOnly | VariableState.Disposed );
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEquivalentTo( key1, key2, key3 );
            onChange.IsDisposed.Should().BeTrue();
            onValidate.IsDisposed.Should().BeTrue();
            (variable1.State & VariableState.Disposed).Should().Be( VariableState.Disposed );
            (variable2.State & VariableState.Disposed).Should().Be( VariableState.Disposed );
            (variable3.State & VariableState.Disposed).Should().Be( VariableState.Disposed );
            (root1.State & VariableState.Disposed).Should().Be( VariableState.Disposed );
        }
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenRootIsAlreadyDisposed()
    {
        var sut = new VariableRootMock();
        sut.Dispose();

        sut.Dispose();

        sut.State.Should().Be( VariableState.ReadOnly | VariableState.Disposed );
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
