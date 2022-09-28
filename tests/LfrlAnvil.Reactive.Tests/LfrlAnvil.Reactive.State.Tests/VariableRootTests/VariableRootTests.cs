using System.Collections.Generic;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Reactive.State.Exceptions;
using LfrlAnvil.Reactive.State.Internal;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.VariableRootTests;

public class VariableRootTests : TestsBase
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
    public void RegisteringVariables_ShouldAddNewVariableNodesCorrectly()
    {
        var (key1, key2, key3, key4) = Fixture.CreateDistinctCollection<string>( count: 4 );
        var (value1, value2, value3) = Fixture.CreateDistinctCollection<int>( count: 3 );
        var variable1 = Variable.WithoutValidators<string>.Create( value1 );
        var variable2 = Variable.WithoutValidators<string>.Create( value2 );
        var variable3 = Variable.WithoutValidators<string>.Create( value3 );
        var root1 = new VariableRootMock();
        root1.ExposedRegisterNode( key4, variable3 );

        var sut = new VariableRootMock();
        var result1 = sut.ExposedRegisterNode( key1, variable1 );
        var result2 = sut.ExposedRegisterNode( key2, variable2 );
        var result3 = sut.ExposedRegisterNode( key3, root1 );

        using ( new AssertionScope() )
        {
            result1.Should().BeSameAs( variable1 );
            result2.Should().BeSameAs( variable2 );
            result3.Should().BeSameAs( root1 );
            result1.Parent.Should().BeSameAs( sut );
            result2.Parent.Should().BeSameAs( sut );
            result3.Parent.Should().BeSameAs( sut );

            sut.State.Should().Be( VariableState.Default );
            sut.Nodes.Should()
                .BeEquivalentTo(
                    KeyValuePair.Create( key1, (IVariableNode)result1 ),
                    KeyValuePair.Create( key2, (IVariableNode)result2 ),
                    KeyValuePair.Create( key3, (IVariableNode)result3 ) );

            sut.Nodes.Count.Should().Be( 3 );
            sut.Nodes.Keys.Should().BeEquivalentTo( key1, key2, key3 );
            sut.Nodes.Values.Should().BeEquivalentTo( result1, result2, result3 );
            sut.Nodes.ChangedNodeKeys.Should().BeEmpty();
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEmpty();
            sut.Nodes.DirtyNodeKeys.Should().BeEmpty();
            sut.Nodes.ContainsKey( key1 ).Should().BeTrue();
            sut.Nodes.ContainsKey( key2 ).Should().BeTrue();
            sut.Nodes.ContainsKey( key3 ).Should().BeTrue();
            sut.Nodes.ContainsKey( key4 ).Should().BeFalse();
            sut.Nodes[key1].Should().BeSameAs( result1 );
            sut.Nodes[key2].Should().BeSameAs( result2 );
            sut.Nodes[key3].Should().BeSameAs( result3 );
            sut.Nodes.TryGetValue( key1, out var tryGetResult1 ).Should().BeTrue();
            sut.Nodes.TryGetValue( key2, out var tryGetResult2 ).Should().BeTrue();
            sut.Nodes.TryGetValue( key3, out var tryGetResult3 ).Should().BeTrue();
            sut.Nodes.TryGetValue( key4, out var tryGetResult4 ).Should().BeFalse();
            tryGetResult1.Should().BeSameAs( result1 );
            tryGetResult2.Should().BeSameAs( result2 );
            tryGetResult3.Should().BeSameAs( result3 );
            tryGetResult4.Should().BeNull();

            ((IVariableNode)sut).GetChildren().Should().BeEquivalentTo( result1, result2, result3 );
        }
    }

    [Fact]
    public void RegisterNode_ShouldThrowVariableNodeRegistrationException_WhenVariableIsDisposed()
    {
        var value = Fixture.Create<int>();
        var variable = Variable.WithoutValidators<string>.Create( value );
        variable.Dispose();

        var sut = new VariableRootMock();

        var action = Lambda.Of( () => sut.ExposedRegisterNode( Fixture.Create<string>(), variable ) );

        action.Should().ThrowExactly<VariableNodeRegistrationException>().AndMatch( e => e.Parent == sut && e.Child == variable );
    }

    [Fact]
    public void RegisterNode_ShouldThrowVariableNodeRegistrationException_WhenRootIsDisposed()
    {
        var value = Fixture.Create<int>();
        var variable = Variable.WithoutValidators<string>.Create( value );

        var sut = new VariableRootMock();
        sut.Dispose();

        var action = Lambda.Of( () => sut.ExposedRegisterNode( Fixture.Create<string>(), variable ) );

        action.Should().ThrowExactly<VariableNodeRegistrationException>().AndMatch( e => e.Parent == sut && e.Child == variable );
    }

    [Fact]
    public void RegisterNode_ShouldThrowVariableNodeRegistrationException_WhenVariableAlreadyHasParent()
    {
        var value = Fixture.Create<int>();
        var variable = Variable.WithoutValidators<string>.Create( value );
        var other = new VariableRootMock();
        other.ExposedRegisterNode( Fixture.Create<string>(), variable );

        var sut = new VariableRootMock();

        var action = Lambda.Of( () => sut.ExposedRegisterNode( Fixture.Create<string>(), variable ) );

        action.Should().ThrowExactly<VariableNodeRegistrationException>().AndMatch( e => e.Parent == sut && e.Child == variable );
    }

    [Fact]
    public void RegisterNode_ShouldThrowVariableNodeRegistrationException_WhenRootHasParent()
    {
        var value = Fixture.Create<int>();
        var variable = Variable.WithoutValidators<string>.Create( value );

        var sut = new VariableRootMock();
        var parent = new VariableRootMock();
        parent.ExposedRegisterNode( Fixture.Create<string>(), sut );

        var action = Lambda.Of( () => sut.ExposedRegisterNode( Fixture.Create<string>(), variable ) );

        action.Should().ThrowExactly<VariableNodeRegistrationException>().AndMatch( e => e.Parent == sut && e.Child == variable );
    }

    [Fact]
    public void RegisterNode_ShouldThrowVariableNodeRegistrationException_WhenAttemptingToRegisterSelf()
    {
        var sut = new VariableRootMock();
        var action = Lambda.Of( () => sut.ExposedRegisterNode( Fixture.Create<string>(), sut ) );
        action.Should().ThrowExactly<VariableNodeRegistrationException>().AndMatch( e => e.Parent == sut && e.Child == sut );
    }

    [Fact]
    public void RegisterNode_ShouldThrowArgumentException_WhenNodeKeyAlreadyExists()
    {
        var value = Fixture.Create<int>();
        var variable1 = Variable.WithoutValidators<string>.Create( value );
        var variable2 = Variable.WithoutValidators<string>.Create( value );

        var key = Fixture.Create<string>();
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key, variable1 );

        var action = Lambda.Of( () => sut.ExposedRegisterNode( key, variable2 ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void RegisterNode_ShouldAddFirstVariableNode_WhenVariableIsInDefaultState()
    {
        var key = Fixture.Create<string>();
        var value = Fixture.Create<int>();
        var variable = Variable.WithoutValidators<string>.Create( value );
        var sut = new VariableRootMock();

        sut.ExposedRegisterNode( key, variable );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Default );
            sut.Nodes.ChangedNodeKeys.Should().BeEmpty();
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEmpty();
            sut.Nodes.DirtyNodeKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void RegisterNode_ShouldAddFirstVariableNode_WhenVariableIsInChangedState()
    {
        var key = Fixture.Create<string>();
        var (initialValue, value) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var variable = Variable.WithoutValidators<string>.Create( initialValue, value );
        var sut = new VariableRootMock();

        sut.ExposedRegisterNode( key, variable );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Changed );
            sut.Nodes.ChangedNodeKeys.Should().BeEquivalentTo( key );
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEmpty();
            sut.Nodes.DirtyNodeKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void RegisterNode_ShouldAddFirstVariableNode_WhenVariableIsInInvalidState()
    {
        var key = Fixture.Create<string>();
        var value = Fixture.Create<int>();
        var errorsValidator = Validators<string>.Fail<int>( Fixture.Create<string>() );
        var variable = Variable.Create( value, errorsValidator );
        variable.RefreshValidation();
        var sut = new VariableRootMock();

        sut.ExposedRegisterNode( key, variable );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Invalid );
            sut.Nodes.ChangedNodeKeys.Should().BeEmpty();
            sut.Nodes.InvalidNodeKeys.Should().BeEquivalentTo( key );
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEmpty();
            sut.Nodes.DirtyNodeKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void RegisterNode_ShouldAddFirstVariableNode_WhenVariableIsInWarningState()
    {
        var key = Fixture.Create<string>();
        var value = Fixture.Create<int>();
        var errorsValidator = Validators<string>.Pass<int>();
        var warningsValidator = Validators<string>.Fail<int>( Fixture.Create<string>() );
        var variable = Variable.Create( value, errorsValidator, warningsValidator );
        variable.RefreshValidation();
        var sut = new VariableRootMock();

        sut.ExposedRegisterNode( key, variable );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Warning );
            sut.Nodes.ChangedNodeKeys.Should().BeEmpty();
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEquivalentTo( key );
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEmpty();
            sut.Nodes.DirtyNodeKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void RegisterNode_ShouldAddFirstVariableNode_WhenVariableIsInDirtyState()
    {
        var key = Fixture.Create<string>();
        var value = Fixture.Create<int>();
        var variable = Variable.WithoutValidators<string>.Create( value );
        variable.Refresh();
        var sut = new VariableRootMock();

        sut.ExposedRegisterNode( key, variable );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Dirty );
            sut.Nodes.ChangedNodeKeys.Should().BeEmpty();
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEmpty();
            sut.Nodes.DirtyNodeKeys.Should().BeEquivalentTo( key );
        }
    }

    [Fact]
    public void RegisterNode_ShouldAddFirstVariableNode_WhenVariableIsInReadOnlyState()
    {
        var key = Fixture.Create<string>();
        var value = Fixture.Create<int>();
        var variable = Variable.WithoutValidators<string>.Create( value );
        variable.SetReadOnly( true );
        var sut = new VariableRootMock();

        sut.ExposedRegisterNode( key, variable );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.ReadOnly );
            sut.Nodes.ChangedNodeKeys.Should().BeEmpty();
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEquivalentTo( key );
            sut.Nodes.DirtyNodeKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void RegisterNode_ShouldAddNextVariableNodeWithTheSameStateAsTheFirstVariable_WhenFirstVariableIsInDefaultState()
    {
        var (key, nextKey) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var (value, nextValue) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var variable = Variable.WithoutValidators<string>.Create( value );
        var nextVariable = Variable.WithoutValidators<string>.Create( nextValue );
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key, variable );

        sut.ExposedRegisterNode( nextKey, nextVariable );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Default );
            sut.Nodes.ChangedNodeKeys.Should().BeEmpty();
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEmpty();
            sut.Nodes.DirtyNodeKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void RegisterNode_ShouldAddNextVariableNodeWithTheSameStateAsTheFirstVariable_WhenFirstVariableIsInChangedState()
    {
        var (key, nextKey) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var (initialValue, value, nextInitialValue, nextValue) = Fixture.CreateDistinctCollection<int>( count: 4 );
        var variable = Variable.WithoutValidators<string>.Create( initialValue, value );
        var nextVariable = Variable.WithoutValidators<string>.Create( nextInitialValue, nextValue );
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key, variable );

        sut.ExposedRegisterNode( nextKey, nextVariable );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Changed );
            sut.Nodes.ChangedNodeKeys.Should().BeEquivalentTo( key, nextKey );
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEmpty();
            sut.Nodes.DirtyNodeKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void RegisterNode_ShouldAddNextVariableNodeWithTheSameStateAsTheFirstVariable_WhenFirstVariableIsInInvalidState()
    {
        var (key, nextKey) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var (value, nextValue) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var errorsValidator = Validators<string>.Fail<int>( Fixture.Create<string>() );
        var variable = Variable.Create( value, errorsValidator );
        variable.RefreshValidation();
        var nextVariable = Variable.Create( nextValue, errorsValidator );
        nextVariable.RefreshValidation();
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key, variable );

        sut.ExposedRegisterNode( nextKey, nextVariable );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Invalid );
            sut.Nodes.ChangedNodeKeys.Should().BeEmpty();
            sut.Nodes.InvalidNodeKeys.Should().BeEquivalentTo( key, nextKey );
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEmpty();
            sut.Nodes.DirtyNodeKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void RegisterNode_ShouldAddNextVariableNodeWithTheSameStateAsTheFirstVariable_WhenFirstVariableIsInWarningState()
    {
        var (key, nextKey) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var (value, nextValue) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var errorsValidator = Validators<string>.Pass<int>();
        var warningsValidator = Validators<string>.Fail<int>( Fixture.Create<string>() );
        var variable = Variable.Create( value, errorsValidator, warningsValidator );
        variable.RefreshValidation();
        var nextVariable = Variable.Create( nextValue, errorsValidator, warningsValidator );
        nextVariable.RefreshValidation();
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key, variable );

        sut.ExposedRegisterNode( nextKey, nextVariable );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Warning );
            sut.Nodes.ChangedNodeKeys.Should().BeEmpty();
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEquivalentTo( key, nextKey );
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEmpty();
            sut.Nodes.DirtyNodeKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void RegisterNode_ShouldAddNextVariableNodeWithTheSameStateAsTheFirstVariable_WhenFirstVariableIsInReadOnlyState()
    {
        var (key, nextKey) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var (value, nextValue) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var variable = Variable.WithoutValidators<string>.Create( value );
        variable.SetReadOnly( true );
        var nextVariable = Variable.WithoutValidators<string>.Create( nextValue );
        nextVariable.SetReadOnly( true );
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key, variable );

        sut.ExposedRegisterNode( nextKey, nextVariable );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.ReadOnly );
            sut.Nodes.ChangedNodeKeys.Should().BeEmpty();
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEquivalentTo( key, nextKey );
            sut.Nodes.DirtyNodeKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void RegisterNode_ShouldAddNextVariableNodeWithTheSameStateAsTheFirstVariable_WhenFirstVariableIsInDirtyState()
    {
        var (key, nextKey) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var (value, nextValue) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var variable = Variable.WithoutValidators<string>.Create( value );
        variable.Refresh();
        var nextVariable = Variable.WithoutValidators<string>.Create( nextValue );
        nextVariable.Refresh();
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key, variable );

        sut.ExposedRegisterNode( nextKey, nextVariable );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Dirty );
            sut.Nodes.ChangedNodeKeys.Should().BeEmpty();
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEmpty();
            sut.Nodes.DirtyNodeKeys.Should().BeEquivalentTo( key, nextKey );
        }
    }

    [Fact]
    public void RegisterNode_ShouldAddNextVariableNodeWithDifferentState_WhenFirstVariableIsInChangedState()
    {
        var (key, nextKey) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var (initialValue, value, nextValue) = Fixture.CreateDistinctCollection<int>( count: 3 );
        var variable = Variable.WithoutValidators<string>.Create( initialValue, value );
        var nextVariable = Variable.WithoutValidators<string>.Create( nextValue );
        nextVariable.SetReadOnly( true );
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key, variable );

        sut.ExposedRegisterNode( nextKey, nextVariable );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Changed );
            sut.Nodes.ChangedNodeKeys.Should().BeEquivalentTo( key );
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEquivalentTo( nextKey );
            sut.Nodes.DirtyNodeKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void RegisterNode_ShouldAddNextVariableNodeWithDifferentState_WhenFirstVariableIsInInvalidState()
    {
        var (key, nextKey) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var (value, nextInitialValue, nextValue) = Fixture.CreateDistinctCollection<int>( count: 3 );
        var errorsValidator = Validators<string>.Fail<int>( Fixture.Create<string>() );
        var variable = Variable.Create( value, errorsValidator );
        variable.RefreshValidation();
        var nextVariable = Variable.WithoutValidators<string>.Create( nextInitialValue, nextValue );
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key, variable );

        sut.ExposedRegisterNode( nextKey, nextVariable );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Invalid | VariableState.Changed );
            sut.Nodes.ChangedNodeKeys.Should().BeEquivalentTo( nextKey );
            sut.Nodes.InvalidNodeKeys.Should().BeEquivalentTo( key );
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEmpty();
            sut.Nodes.DirtyNodeKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void RegisterNode_ShouldAddNextVariableNodeWithDifferentState_WhenFirstVariableIsInWarningState()
    {
        var (key, nextKey) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var (value, nextInitialValue, nextValue) = Fixture.CreateDistinctCollection<int>( count: 3 );
        var errorsValidator = Validators<string>.Pass<int>();
        var warningsValidator = Validators<string>.Fail<int>( Fixture.Create<string>() );
        var variable = Variable.Create( value, errorsValidator, warningsValidator );
        variable.RefreshValidation();
        var nextVariable = Variable.WithoutValidators<string>.Create( nextInitialValue, nextValue );
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key, variable );

        sut.ExposedRegisterNode( nextKey, nextVariable );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Warning | VariableState.Changed );
            sut.Nodes.ChangedNodeKeys.Should().BeEquivalentTo( nextKey );
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEquivalentTo( key );
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEmpty();
            sut.Nodes.DirtyNodeKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void RegisterNode_ShouldAddNextVariableNodeWithDifferentState_WhenFirstVariableIsInReadOnlyState()
    {
        var (key, nextKey) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var (value, nextInitialValue, nextValue) = Fixture.CreateDistinctCollection<int>( count: 3 );
        var variable = Variable.WithoutValidators<string>.Create( value );
        variable.SetReadOnly( true );
        var nextVariable = Variable.WithoutValidators<string>.Create( nextInitialValue, nextValue );
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key, variable );

        sut.ExposedRegisterNode( nextKey, nextVariable );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Changed );
            sut.Nodes.ChangedNodeKeys.Should().BeEquivalentTo( nextKey );
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEquivalentTo( key );
            sut.Nodes.DirtyNodeKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void RegisterNode_ShouldAddNextVariableNodeWithDifferentState_WhenFirstVariableIsInDirtyState()
    {
        var (key, nextKey) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var (value, nextInitialValue, nextValue) = Fixture.CreateDistinctCollection<int>( count: 3 );
        var variable = Variable.WithoutValidators<string>.Create( value );
        variable.Refresh();
        var nextVariable = Variable.WithoutValidators<string>.Create( nextInitialValue, nextValue );
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key, variable );

        sut.ExposedRegisterNode( nextKey, nextVariable );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Changed | VariableState.Dirty );
            sut.Nodes.ChangedNodeKeys.Should().BeEquivalentTo( nextKey );
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEmpty();
            sut.Nodes.DirtyNodeKeys.Should().BeEquivalentTo( key );
        }
    }

    [Fact]
    public void RegisteredVariableChange_ShouldBeListenedToByRoot_WhenVariableBecomesChangedAndDirty()
    {
        var key = Fixture.Create<string>();
        var (value, newValue) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var variable = Variable.WithoutValidators<string>.Create( value );
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key, variable );

        VariableValueChangeEvent<int, string>? onVariableChange = null;
        VariableValidationEvent<int, string>? onVariableValidate = null;
        var onChange = new List<VariableRootChangeEvent<string>>();
        var onValidate = new List<VariableRootValidationEvent<string>>();
        variable.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( e => onVariableChange = e ) );
        variable.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( e => onVariableValidate = e ) );
        sut.OnChange.Listen( EventListener.Create<VariableRootChangeEvent<string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<VariableRootValidationEvent<string>>( onValidate.Add ) );

        variable.Change( newValue );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Changed | VariableState.Dirty );
            sut.Nodes.ChangedNodeKeys.Should().BeEquivalentTo( key );
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEmpty();
            sut.Nodes.DirtyNodeKeys.Should().BeEquivalentTo( key );
            onChange.Should().HaveCount( 1 );
            onValidate.Should().HaveCount( 1 );

            var changeEvent = onChange[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.NodeKey.Should().Be( key );
            changeEvent.PreviousState.Should().Be( VariableState.Default );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.SourceEvent.Should().BeSameAs( onVariableChange );

            var validateEvent = onValidate[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.NodeKey.Should().Be( key );
            validateEvent.PreviousState.Should().Be( sut.State );
            validateEvent.NewState.Should().Be( sut.State );
            validateEvent.SourceEvent.Should().BeSameAs( onVariableValidate );
        }
    }

    [Fact]
    public void RegisteredVariableChange_ShouldBeListenedToByRoot_WhenVariableBecomesInvalid()
    {
        var key = Fixture.Create<string>();
        var value = Fixture.Create<int>();
        var errorsValidator = Validators<string>.Fail<int>( Fixture.Create<string>() );
        var variable = Variable.Create( value, errorsValidator );
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key, variable );

        VariableValueChangeEvent<int, string>? onVariableChange = null;
        VariableValidationEvent<int, string>? onVariableValidate = null;
        var onChange = new List<VariableRootChangeEvent<string>>();
        var onValidate = new List<VariableRootValidationEvent<string>>();
        variable.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( e => onVariableChange = e ) );
        variable.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( e => onVariableValidate = e ) );
        sut.OnChange.Listen( EventListener.Create<VariableRootChangeEvent<string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<VariableRootValidationEvent<string>>( onValidate.Add ) );

        variable.Change( value );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Invalid | VariableState.Dirty );
            sut.Nodes.ChangedNodeKeys.Should().BeEmpty();
            sut.Nodes.InvalidNodeKeys.Should().BeEquivalentTo( key );
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEmpty();
            sut.Nodes.DirtyNodeKeys.Should().BeEquivalentTo( key );
            onChange.Should().HaveCount( 1 );
            onValidate.Should().HaveCount( 1 );

            var changeEvent = onChange[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.NodeKey.Should().Be( key );
            changeEvent.PreviousState.Should().Be( VariableState.Default );
            changeEvent.NewState.Should().Be( VariableState.Dirty );
            changeEvent.SourceEvent.Should().BeSameAs( onVariableChange );

            var validateEvent = onValidate[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.NodeKey.Should().Be( key );
            validateEvent.PreviousState.Should().Be( VariableState.Dirty );
            validateEvent.NewState.Should().Be( sut.State );
            validateEvent.SourceEvent.Should().BeSameAs( onVariableValidate );
        }
    }

    [Fact]
    public void RegisteredVariableChange_ShouldBeListenedToByRoot_WhenVariableBecomesWarning()
    {
        var key = Fixture.Create<string>();
        var value = Fixture.Create<int>();
        var errorsValidator = Validators<string>.Pass<int>();
        var warningsValidator = Validators<string>.Fail<int>( Fixture.Create<string>() );
        var variable = Variable.Create( value, errorsValidator, warningsValidator );
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key, variable );

        VariableValueChangeEvent<int, string>? onVariableChange = null;
        VariableValidationEvent<int, string>? onVariableValidate = null;
        var onChange = new List<VariableRootChangeEvent<string>>();
        var onValidate = new List<VariableRootValidationEvent<string>>();
        variable.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( e => onVariableChange = e ) );
        variable.OnValidate.Listen( EventListener.Create<VariableValidationEvent<int, string>>( e => onVariableValidate = e ) );
        sut.OnChange.Listen( EventListener.Create<VariableRootChangeEvent<string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<VariableRootValidationEvent<string>>( onValidate.Add ) );

        variable.Change( value );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Warning | VariableState.Dirty );
            sut.Nodes.ChangedNodeKeys.Should().BeEmpty();
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEquivalentTo( key );
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEmpty();
            sut.Nodes.DirtyNodeKeys.Should().BeEquivalentTo( key );
            onChange.Should().HaveCount( 1 );
            onValidate.Should().HaveCount( 1 );

            var changeEvent = onChange[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.NodeKey.Should().Be( key );
            changeEvent.PreviousState.Should().Be( VariableState.Default );
            changeEvent.NewState.Should().Be( VariableState.Dirty );
            changeEvent.SourceEvent.Should().BeSameAs( onVariableChange );

            var validateEvent = onValidate[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.NodeKey.Should().Be( key );
            validateEvent.PreviousState.Should().Be( VariableState.Dirty );
            validateEvent.NewState.Should().Be( sut.State );
            validateEvent.SourceEvent.Should().BeSameAs( onVariableValidate );
        }
    }

    [Fact]
    public void RegisteredVariableChange_ShouldBeListenedToByRoot_WhenVariableBecomesReadOnly()
    {
        var key = Fixture.Create<string>();
        var value = Fixture.Create<int>();
        var variable = Variable.WithoutValidators<string>.Create( value );
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key, variable );

        VariableValueChangeEvent<int, string>? onVariableChange = null;
        var onChange = new List<VariableRootChangeEvent<string>>();
        var onValidate = new List<VariableRootValidationEvent<string>>();
        variable.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<int, string>>( e => onVariableChange = e ) );
        sut.OnChange.Listen( EventListener.Create<VariableRootChangeEvent<string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<VariableRootValidationEvent<string>>( onValidate.Add ) );

        variable.SetReadOnly( true );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.ReadOnly );
            sut.Nodes.ChangedNodeKeys.Should().BeEmpty();
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEquivalentTo( key );
            sut.Nodes.DirtyNodeKeys.Should().BeEmpty();
            onChange.Should().HaveCount( 1 );
            onValidate.Should().BeEmpty();

            var changeEvent = onChange[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.NodeKey.Should().Be( key );
            changeEvent.PreviousState.Should().Be( VariableState.Default );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.SourceEvent.Should().BeSameAs( onVariableChange );
        }
    }

    [Fact]
    public void RegisteredVariableChange_ShouldBeListenedToByRoot_WhenOneOfVariablesBecomesReadOnly()
    {
        var (key1, key2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var value = Fixture.Create<int>();
        var variable1 = Variable.WithoutValidators<string>.Create( value );
        var variable2 = Variable.WithoutValidators<string>.Create( value );
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key1, variable1 );
        sut.ExposedRegisterNode( key2, variable2 );

        variable1.SetReadOnly( true );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Default );
            sut.Nodes.ChangedNodeKeys.Should().BeEmpty();
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEquivalentTo( key1 );
            sut.Nodes.DirtyNodeKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void RegisteredVariableChange_ShouldBeListenedToByRoot_WhenVariableStopsBeingChanged()
    {
        var key = Fixture.Create<string>();
        var (value, newValue) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var variable = Variable.WithoutValidators<string>.Create( value );
        variable.Change( newValue );
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key, variable );

        variable.Change( value );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Dirty );
            sut.Nodes.ChangedNodeKeys.Should().BeEmpty();
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEmpty();
            sut.Nodes.DirtyNodeKeys.Should().BeEquivalentTo( key );
        }
    }

    [Fact]
    public void RegisteredVariableChange_ShouldBeListenedToByRoot_WhenVariableStopsBeingDirty()
    {
        var key = Fixture.Create<string>();
        var value = Fixture.Create<int>();
        var variable = Variable.WithoutValidators<string>.Create( value );
        variable.Refresh();
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key, variable );

        variable.Reset( value, value );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Default );
            sut.Nodes.ChangedNodeKeys.Should().BeEmpty();
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEmpty();
            sut.Nodes.DirtyNodeKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void RegisteredVariableChange_ShouldBeListenedToByRoot_WhenVariableStopsBeingInvalid()
    {
        var enableValidator = true;
        var key = Fixture.Create<string>();
        var value = Fixture.Create<int>();
        var errorsValidator = Validators<string>.IfTrue( _ => enableValidator, Validators<string>.Fail<int>( Fixture.Create<string>() ) );
        var variable = Variable.Create( value, errorsValidator );
        variable.RefreshValidation();
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key, variable );

        enableValidator = false;
        variable.RefreshValidation();

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Default );
            sut.Nodes.ChangedNodeKeys.Should().BeEmpty();
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEmpty();
            sut.Nodes.DirtyNodeKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void RegisteredVariableChange_ShouldBeListenedToByRoot_WhenVariableStopsBeingWarning()
    {
        var enableValidator = true;
        var key = Fixture.Create<string>();
        var value = Fixture.Create<int>();
        var errorsValidator = Validators<string>.Pass<int>();
        var warningsValidator = Validators<string>.IfTrue( _ => enableValidator, Validators<string>.Fail<int>( Fixture.Create<string>() ) );
        var variable = Variable.Create( value, errorsValidator, warningsValidator );
        variable.RefreshValidation();
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key, variable );

        enableValidator = false;
        variable.RefreshValidation();

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Default );
            sut.Nodes.ChangedNodeKeys.Should().BeEmpty();
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEmpty();
            sut.Nodes.DirtyNodeKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void RegisteredVariableChange_ShouldBeListenedToByRoot_WhenVariableStopsBeingReadOnly()
    {
        var key = Fixture.Create<string>();
        var value = Fixture.Create<int>();
        var variable = Variable.WithoutValidators<string>.Create( value );
        variable.SetReadOnly( true );
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key, variable );

        variable.SetReadOnly( false );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Default );
            sut.Nodes.ChangedNodeKeys.Should().BeEmpty();
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEmpty();
            sut.Nodes.DirtyNodeKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void RegisteredVariableChange_ShouldBeListenedToByRoot_WhenOneOfVariablesStopsBeingChanged()
    {
        var (key1, key2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var (value, newValue) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var variable1 = Variable.WithoutValidators<string>.Create( value, newValue );
        var variable2 = Variable.WithoutValidators<string>.Create( value, newValue );
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key1, variable1 );
        sut.ExposedRegisterNode( key2, variable2 );

        variable1.Change( value );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Changed | VariableState.Dirty );
            sut.Nodes.ChangedNodeKeys.Should().BeEquivalentTo( key2 );
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEmpty();
            sut.Nodes.DirtyNodeKeys.Should().BeEquivalentTo( key1 );
        }
    }

    [Fact]
    public void RegisteredVariableChange_ShouldBeListenedToByRoot_WhenOneOfVariablesStopsBeingInvalid()
    {
        var enableValidator = true;
        var (key1, key2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var value = Fixture.Create<int>();
        var errorsValidator = Validators<string>.IfTrue( _ => enableValidator, Validators<string>.Fail<int>( Fixture.Create<string>() ) );
        var variable1 = Variable.Create( value, errorsValidator );
        variable1.RefreshValidation();
        var variable2 = Variable.Create( value, errorsValidator );
        variable2.RefreshValidation();
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key1, variable1 );
        sut.ExposedRegisterNode( key2, variable2 );

        enableValidator = false;
        variable1.RefreshValidation();

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Invalid );
            sut.Nodes.ChangedNodeKeys.Should().BeEmpty();
            sut.Nodes.InvalidNodeKeys.Should().BeEquivalentTo( key2 );
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEmpty();
            sut.Nodes.DirtyNodeKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void RegisteredVariableChange_ShouldBeListenedToByRoot_WhenOneOfVariablesStopsBeingWarning()
    {
        var enableValidator = true;
        var (key1, key2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var value = Fixture.Create<int>();
        var errorsValidator = Validators<string>.Pass<int>();
        var warningsValidator = Validators<string>.IfTrue( _ => enableValidator, Validators<string>.Fail<int>( Fixture.Create<string>() ) );
        var variable1 = Variable.Create( value, errorsValidator, warningsValidator );
        variable1.RefreshValidation();
        var variable2 = Variable.Create( value, errorsValidator, warningsValidator );
        variable2.RefreshValidation();
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key1, variable1 );
        sut.ExposedRegisterNode( key2, variable2 );

        enableValidator = false;
        variable1.RefreshValidation();

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Warning );
            sut.Nodes.ChangedNodeKeys.Should().BeEmpty();
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEquivalentTo( key2 );
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEmpty();
            sut.Nodes.DirtyNodeKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void RegisteredVariableChange_ShouldBeListenedToByRoot_WhenOneOfVariablesStopsBeingReadOnly()
    {
        var (key1, key2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var value = Fixture.Create<int>();
        var variable1 = Variable.WithoutValidators<string>.Create( value );
        variable1.SetReadOnly( true );
        var variable2 = Variable.WithoutValidators<string>.Create( value );
        variable2.SetReadOnly( true );
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key1, variable1 );
        sut.ExposedRegisterNode( key2, variable2 );

        variable1.SetReadOnly( false );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Default );
            sut.Nodes.ChangedNodeKeys.Should().BeEmpty();
            sut.Nodes.InvalidNodeKeys.Should().BeEmpty();
            sut.Nodes.WarningNodeKeys.Should().BeEmpty();
            sut.Nodes.ReadOnlyNodeKeys.Should().BeEquivalentTo( key2 );
            sut.Nodes.DirtyNodeKeys.Should().BeEmpty();
        }
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
        var variable1 = Variable.Create( value1, validator, validator );
        var variable2 = Variable.Create( value2, validator, validator );
        var variable3 = Variable.Create( value3, validator, validator );
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
        var variable = Variable.Create( value, validator, validator );
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
        var variable1 = Variable.Create( value1, validator, validator );
        var variable2 = Variable.Create( value2, validator, validator );
        var variable3 = Variable.Create( value3, validator, validator );
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
        var variable = Variable.Create( value, validator, validator );
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
