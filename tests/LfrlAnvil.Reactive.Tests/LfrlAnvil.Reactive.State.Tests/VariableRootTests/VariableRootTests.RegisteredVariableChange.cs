using System.Collections.Generic;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.VariableRootTests;

public partial class VariableRootTests
{
    [Fact]
    public void RegisteredVariableChange_ShouldBeListenedToByRoot_WhenVariableBecomesChangedAndDirty()
    {
        var key = Fixture.Create<string>();
        var (value, newValue) = Fixture.CreateManyDistinct<int>( count: 2 );
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

        Assertion.All(
                sut.State.TestEquals( VariableState.Changed | VariableState.Dirty ),
                sut.Nodes.ChangedNodeKeys.TestSetEqual( [ key ] ),
                sut.Nodes.InvalidNodeKeys.TestEmpty(),
                sut.Nodes.WarningNodeKeys.TestEmpty(),
                sut.Nodes.ReadOnlyNodeKeys.TestEmpty(),
                sut.Nodes.DirtyNodeKeys.TestSetEqual( [ key ] ),
                onChange.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => Assertion.All(
                        "changeEvent",
                        e[0].Variable.TestRefEquals( sut ),
                        e[0].NodeKey.TestEquals( key ),
                        e[0].PreviousState.TestEquals( VariableState.Default ),
                        e[0].NewState.TestEquals( sut.State ),
                        e[0].SourceEvent.TestRefEquals( onVariableChange ) ) ),
                onValidate.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => Assertion.All(
                        "validateEvent",
                        e[0].Variable.TestRefEquals( sut ),
                        e[0].NodeKey.TestEquals( key ),
                        e[0].PreviousState.TestEquals( sut.State ),
                        e[0].NewState.TestEquals( sut.State ),
                        e[0].SourceEvent.TestRefEquals( onVariableValidate ) ) ) )
            .Go();
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

        Assertion.All(
                sut.State.TestEquals( VariableState.Invalid | VariableState.Dirty ),
                sut.Nodes.ChangedNodeKeys.TestEmpty(),
                sut.Nodes.InvalidNodeKeys.TestSetEqual( [ key ] ),
                sut.Nodes.WarningNodeKeys.TestEmpty(),
                sut.Nodes.ReadOnlyNodeKeys.TestEmpty(),
                sut.Nodes.DirtyNodeKeys.TestSetEqual( [ key ] ),
                onChange.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => Assertion.All(
                        "changeEvent",
                        e[0].Variable.TestRefEquals( sut ),
                        e[0].NodeKey.TestEquals( key ),
                        e[0].PreviousState.TestEquals( VariableState.Default ),
                        e[0].NewState.TestEquals( VariableState.Dirty ),
                        e[0].SourceEvent.TestRefEquals( onVariableChange ) ) ),
                onValidate.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => Assertion.All(
                        "validateEvent",
                        e[0].Variable.TestRefEquals( sut ),
                        e[0].NodeKey.TestEquals( key ),
                        e[0].PreviousState.TestEquals( VariableState.Dirty ),
                        e[0].NewState.TestEquals( sut.State ),
                        e[0].SourceEvent.TestRefEquals( onVariableValidate ) ) ) )
            .Go();
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

        Assertion.All(
                sut.State.TestEquals( VariableState.Warning | VariableState.Dirty ),
                sut.Nodes.ChangedNodeKeys.TestEmpty(),
                sut.Nodes.InvalidNodeKeys.TestEmpty(),
                sut.Nodes.WarningNodeKeys.TestSetEqual( [ key ] ),
                sut.Nodes.ReadOnlyNodeKeys.TestEmpty(),
                sut.Nodes.DirtyNodeKeys.TestSetEqual( [ key ] ),
                onChange.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => Assertion.All(
                        "changeEvent",
                        e[0].Variable.TestRefEquals( sut ),
                        e[0].NodeKey.TestEquals( key ),
                        e[0].PreviousState.TestEquals( VariableState.Default ),
                        e[0].NewState.TestEquals( VariableState.Dirty ),
                        e[0].SourceEvent.TestRefEquals( onVariableChange ) ) ),
                onValidate.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => Assertion.All(
                        "validateEvent",
                        e[0].Variable.TestRefEquals( sut ),
                        e[0].NodeKey.TestEquals( key ),
                        e[0].PreviousState.TestEquals( VariableState.Dirty ),
                        e[0].NewState.TestEquals( sut.State ),
                        e[0].SourceEvent.TestRefEquals( onVariableValidate ) ) ) )
            .Go();
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

        Assertion.All(
                sut.State.TestEquals( VariableState.ReadOnly ),
                sut.Nodes.ChangedNodeKeys.TestEmpty(),
                sut.Nodes.InvalidNodeKeys.TestEmpty(),
                sut.Nodes.WarningNodeKeys.TestEmpty(),
                sut.Nodes.ReadOnlyNodeKeys.TestSetEqual( [ key ] ),
                sut.Nodes.DirtyNodeKeys.TestEmpty(),
                onChange.TestCount( count => count.TestEquals( 1 ) )
                    .Then( e => Assertion.All(
                        "changeEvent",
                        e[0].Variable.TestRefEquals( sut ),
                        e[0].NodeKey.TestEquals( key ),
                        e[0].PreviousState.TestEquals( VariableState.Default ),
                        e[0].NewState.TestEquals( sut.State ),
                        e[0].SourceEvent.TestRefEquals( onVariableChange ) ) ),
                onValidate.TestEmpty() )
            .Go();
    }

    [Fact]
    public void RegisteredVariableChange_ShouldBeListenedToByRoot_WhenOneOfVariablesBecomesReadOnly()
    {
        var (key1, key2) = Fixture.CreateManyDistinct<string>( count: 2 );
        var value = Fixture.Create<int>();
        var variable1 = Variable.WithoutValidators<string>.Create( value );
        var variable2 = Variable.WithoutValidators<string>.Create( value );
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key1, variable1 );
        sut.ExposedRegisterNode( key2, variable2 );

        variable1.SetReadOnly( true );

        Assertion.All(
                sut.State.TestEquals( VariableState.Default ),
                sut.Nodes.ChangedNodeKeys.TestEmpty(),
                sut.Nodes.InvalidNodeKeys.TestEmpty(),
                sut.Nodes.WarningNodeKeys.TestEmpty(),
                sut.Nodes.ReadOnlyNodeKeys.TestSetEqual( [ key1 ] ),
                sut.Nodes.DirtyNodeKeys.TestEmpty() )
            .Go();
    }

    [Fact]
    public void RegisteredVariableChange_ShouldBeListenedToByRoot_WhenVariableStopsBeingChanged()
    {
        var key = Fixture.Create<string>();
        var (value, newValue) = Fixture.CreateManyDistinct<int>( count: 2 );
        var variable = Variable.WithoutValidators<string>.Create( value );
        variable.Change( newValue );
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key, variable );

        variable.Change( value );

        Assertion.All(
                sut.State.TestEquals( VariableState.Dirty ),
                sut.Nodes.ChangedNodeKeys.TestEmpty(),
                sut.Nodes.InvalidNodeKeys.TestEmpty(),
                sut.Nodes.WarningNodeKeys.TestEmpty(),
                sut.Nodes.ReadOnlyNodeKeys.TestEmpty(),
                sut.Nodes.DirtyNodeKeys.TestSetEqual( [ key ] ) )
            .Go();
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

        Assertion.All(
                sut.State.TestEquals( VariableState.Default ),
                sut.Nodes.ChangedNodeKeys.TestEmpty(),
                sut.Nodes.InvalidNodeKeys.TestEmpty(),
                sut.Nodes.WarningNodeKeys.TestEmpty(),
                sut.Nodes.ReadOnlyNodeKeys.TestEmpty(),
                sut.Nodes.DirtyNodeKeys.TestEmpty() )
            .Go();
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

        Assertion.All(
                sut.State.TestEquals( VariableState.Default ),
                sut.Nodes.ChangedNodeKeys.TestEmpty(),
                sut.Nodes.InvalidNodeKeys.TestEmpty(),
                sut.Nodes.WarningNodeKeys.TestEmpty(),
                sut.Nodes.ReadOnlyNodeKeys.TestEmpty(),
                sut.Nodes.DirtyNodeKeys.TestEmpty() )
            .Go();
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

        Assertion.All(
                sut.State.TestEquals( VariableState.Default ),
                sut.Nodes.ChangedNodeKeys.TestEmpty(),
                sut.Nodes.InvalidNodeKeys.TestEmpty(),
                sut.Nodes.WarningNodeKeys.TestEmpty(),
                sut.Nodes.ReadOnlyNodeKeys.TestEmpty(),
                sut.Nodes.DirtyNodeKeys.TestEmpty() )
            .Go();
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

        Assertion.All(
                sut.State.TestEquals( VariableState.Default ),
                sut.Nodes.ChangedNodeKeys.TestEmpty(),
                sut.Nodes.InvalidNodeKeys.TestEmpty(),
                sut.Nodes.WarningNodeKeys.TestEmpty(),
                sut.Nodes.ReadOnlyNodeKeys.TestEmpty(),
                sut.Nodes.DirtyNodeKeys.TestEmpty() )
            .Go();
    }

    [Fact]
    public void RegisteredVariableChange_ShouldBeListenedToByRoot_WhenOneOfVariablesStopsBeingChanged()
    {
        var (key1, key2) = Fixture.CreateManyDistinct<string>( count: 2 );
        var (value, newValue) = Fixture.CreateManyDistinct<int>( count: 2 );
        var variable1 = Variable.WithoutValidators<string>.Create( value, newValue );
        var variable2 = Variable.WithoutValidators<string>.Create( value, newValue );
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key1, variable1 );
        sut.ExposedRegisterNode( key2, variable2 );

        variable1.Change( value );

        Assertion.All(
                sut.State.TestEquals( VariableState.Changed | VariableState.Dirty ),
                sut.Nodes.ChangedNodeKeys.TestSetEqual( [ key2 ] ),
                sut.Nodes.InvalidNodeKeys.TestEmpty(),
                sut.Nodes.WarningNodeKeys.TestEmpty(),
                sut.Nodes.ReadOnlyNodeKeys.TestEmpty(),
                sut.Nodes.DirtyNodeKeys.TestSetEqual( [ key1 ] ) )
            .Go();
    }

    [Fact]
    public void RegisteredVariableChange_ShouldBeListenedToByRoot_WhenOneOfVariablesStopsBeingInvalid()
    {
        var enableValidator = true;
        var (key1, key2) = Fixture.CreateManyDistinct<string>( count: 2 );
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

        Assertion.All(
                sut.State.TestEquals( VariableState.Invalid ),
                sut.Nodes.ChangedNodeKeys.TestEmpty(),
                sut.Nodes.InvalidNodeKeys.TestSetEqual( [ key2 ] ),
                sut.Nodes.WarningNodeKeys.TestEmpty(),
                sut.Nodes.ReadOnlyNodeKeys.TestEmpty(),
                sut.Nodes.DirtyNodeKeys.TestEmpty() )
            .Go();
    }

    [Fact]
    public void RegisteredVariableChange_ShouldBeListenedToByRoot_WhenOneOfVariablesStopsBeingWarning()
    {
        var enableValidator = true;
        var (key1, key2) = Fixture.CreateManyDistinct<string>( count: 2 );
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

        Assertion.All(
                sut.State.TestEquals( VariableState.Warning ),
                sut.Nodes.ChangedNodeKeys.TestEmpty(),
                sut.Nodes.InvalidNodeKeys.TestEmpty(),
                sut.Nodes.WarningNodeKeys.TestSetEqual( [ key2 ] ),
                sut.Nodes.ReadOnlyNodeKeys.TestEmpty(),
                sut.Nodes.DirtyNodeKeys.TestEmpty() )
            .Go();
    }

    [Fact]
    public void RegisteredVariableChange_ShouldBeListenedToByRoot_WhenOneOfVariablesStopsBeingReadOnly()
    {
        var (key1, key2) = Fixture.CreateManyDistinct<string>( count: 2 );
        var value = Fixture.Create<int>();
        var variable1 = Variable.WithoutValidators<string>.Create( value );
        variable1.SetReadOnly( true );
        var variable2 = Variable.WithoutValidators<string>.Create( value );
        variable2.SetReadOnly( true );
        var sut = new VariableRootMock();
        sut.ExposedRegisterNode( key1, variable1 );
        sut.ExposedRegisterNode( key2, variable2 );

        variable1.SetReadOnly( false );

        Assertion.All(
                sut.State.TestEquals( VariableState.Default ),
                sut.Nodes.ChangedNodeKeys.TestEmpty(),
                sut.Nodes.InvalidNodeKeys.TestEmpty(),
                sut.Nodes.WarningNodeKeys.TestEmpty(),
                sut.Nodes.ReadOnlyNodeKeys.TestSetEqual( [ key2 ] ),
                sut.Nodes.DirtyNodeKeys.TestEmpty() )
            .Go();
    }
}
