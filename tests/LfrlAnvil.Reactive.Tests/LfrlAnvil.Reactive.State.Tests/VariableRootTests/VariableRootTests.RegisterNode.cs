using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Exceptions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.VariableRootTests;

public partial class VariableRootTests
{
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
            sut.Nodes[key].Should().BeSameAs( variable );
            sut.Nodes.ContainsKey( key ).Should().BeTrue();
            sut.Nodes.TryGetValue( key, out var outResult ).Should().BeTrue();
            outResult.Should().BeSameAs( variable );
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
        var variable = Variable.Create( value, errorsValidator: errorsValidator );
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
        var variable = Variable.Create( value, errorsValidator: errorsValidator, warningsValidator: warningsValidator );
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
        var variable = Variable.Create( value, errorsValidator: errorsValidator );
        variable.RefreshValidation();
        var nextVariable = Variable.Create( nextValue, errorsValidator: errorsValidator );
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
        var variable = Variable.Create( value, errorsValidator: errorsValidator, warningsValidator: warningsValidator );
        variable.RefreshValidation();
        var nextVariable = Variable.Create( nextValue, errorsValidator: errorsValidator, warningsValidator: warningsValidator );
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
        var variable = Variable.Create( value, errorsValidator: errorsValidator );
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
        var variable = Variable.Create( value, errorsValidator: errorsValidator, warningsValidator: warningsValidator );
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
}
