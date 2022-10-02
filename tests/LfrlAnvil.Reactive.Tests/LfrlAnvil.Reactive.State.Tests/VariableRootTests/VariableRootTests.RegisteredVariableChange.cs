using System.Collections.Generic;
using FluentAssertions.Execution;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.VariableRootTests;

public partial class VariableRootTests
{
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
        var variable = Variable.Create( value, errorsValidator: errorsValidator );
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
        var variable = Variable.Create( value, errorsValidator: errorsValidator, warningsValidator: warningsValidator );
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
        var variable = Variable.Create( value, errorsValidator: errorsValidator );
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
        var variable = Variable.Create( value, errorsValidator: errorsValidator, warningsValidator: warningsValidator );
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
        var variable1 = Variable.Create( value, errorsValidator: errorsValidator );
        variable1.RefreshValidation();
        var variable2 = Variable.Create( value, errorsValidator: errorsValidator );
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
        var variable1 = Variable.Create( value, errorsValidator: errorsValidator, warningsValidator: warningsValidator );
        variable1.RefreshValidation();
        var variable2 = Variable.Create( value, errorsValidator: errorsValidator, warningsValidator: warningsValidator );
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
}
