using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State.Tests.CollectionVariableRootTests;

public partial class CollectionVariableRootTests
{
    [Fact]
    public void ElementChange_ShouldBeListenedToByRoot_WhenElementBecomesChanged()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );

        VariableValueChangeEvent<string, string>? onVariableChange = null;
        VariableValidationEvent<string, string>? onVariableValidate = null;
        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        element.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<string, string>>( e => onVariableChange = e ) );
        element.OnValidate.Listen( EventListener.Create<VariableValidationEvent<string, string>>( e => onVariableValidate = e ) );
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        element.Change( Fixture.Create<string>() );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Changed );
            sut.Elements.ChangedElementKeys.Should().BeEquivalentTo( element.Key );
            sut.Elements.InvalidElementKeys.Should().BeEmpty();
            sut.Elements.WarningElementKeys.Should().BeEmpty();
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEmpty();
            onChange.Should().HaveCount( 1 );
            onValidate.Should().HaveCount( 1 );

            var changeEvent = onChange[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.PreviousState.Should().Be( VariableState.Default );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.SourceEvent.Should().BeSameAs( onVariableChange );
            changeEvent.AddedElements.Should().BeEmpty();
            changeEvent.RemovedElements.Should().BeEmpty();
            changeEvent.RestoredElements.Should().BeEmpty();
            changeEvent.Source.Should().Be( VariableChangeSource.ChildNode );

            var validateEvent = onValidate[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.PreviousState.Should().Be( sut.State );
            validateEvent.NewState.Should().Be( sut.State );
            validateEvent.SourceEvent.Should().BeSameAs( onVariableValidate );
            validateEvent.PreviousErrors.Should().BeEmpty();
            validateEvent.NewErrors.Should().BeEmpty();
            validateEvent.PreviousWarnings.Should().BeEmpty();
            validateEvent.NewWarnings.Should().BeEmpty();
            validateEvent.AssociatedChange.Should().BeNull();
        }
    }

    [Fact]
    public void ElementChange_ShouldBeListenedToByRoot_WhenElementBecomesInvalid()
    {
        var validator = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var element = new VariableMock( Fixture.Create<int>(), Fixture.Create<string>(), validator, Validators<string>.Pass<string>() );
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );

        VariableValueChangeEvent<string, string>? onVariableChange = null;
        VariableValidationEvent<string, string>? onVariableValidate = null;
        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        element.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<string, string>>( e => onVariableChange = e ) );
        element.OnValidate.Listen( EventListener.Create<VariableValidationEvent<string, string>>( e => onVariableValidate = e ) );
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        element.Change( element.Value );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Invalid );
            sut.Elements.ChangedElementKeys.Should().BeEmpty();
            sut.Elements.InvalidElementKeys.Should().BeEquivalentTo( element.Key );
            sut.Elements.WarningElementKeys.Should().BeEmpty();
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEmpty();
            onChange.Should().HaveCount( 1 );
            onValidate.Should().HaveCount( 1 );

            var changeEvent = onChange[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.PreviousState.Should().Be( VariableState.Default );
            changeEvent.NewState.Should().Be( VariableState.Default );
            changeEvent.SourceEvent.Should().BeSameAs( onVariableChange );
            changeEvent.AddedElements.Should().BeEmpty();
            changeEvent.RemovedElements.Should().BeEmpty();
            changeEvent.RestoredElements.Should().BeEmpty();
            changeEvent.Source.Should().Be( VariableChangeSource.ChildNode );

            var validateEvent = onValidate[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.PreviousState.Should().Be( VariableState.Default );
            validateEvent.NewState.Should().Be( sut.State );
            validateEvent.SourceEvent.Should().BeSameAs( onVariableValidate );
            validateEvent.PreviousErrors.Should().BeEmpty();
            validateEvent.NewErrors.Should().BeEmpty();
            validateEvent.PreviousWarnings.Should().BeEmpty();
            validateEvent.NewWarnings.Should().BeEmpty();
            validateEvent.AssociatedChange.Should().BeNull();
        }
    }

    [Fact]
    public void ElementChange_ShouldBeListenedToByRoot_WhenElementBecomesWarning()
    {
        var validator = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var element = new VariableMock( Fixture.Create<int>(), Fixture.Create<string>(), Validators<string>.Pass<string>(), validator );
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );

        VariableValueChangeEvent<string, string>? onVariableChange = null;
        VariableValidationEvent<string, string>? onVariableValidate = null;
        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        element.OnChange.Listen( EventListener.Create<VariableValueChangeEvent<string, string>>( e => onVariableChange = e ) );
        element.OnValidate.Listen( EventListener.Create<VariableValidationEvent<string, string>>( e => onVariableValidate = e ) );
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        element.Change( element.Value );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Warning );
            sut.Elements.ChangedElementKeys.Should().BeEmpty();
            sut.Elements.InvalidElementKeys.Should().BeEmpty();
            sut.Elements.WarningElementKeys.Should().BeEquivalentTo( element.Key );
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEmpty();
            onChange.Should().HaveCount( 1 );
            onValidate.Should().HaveCount( 1 );

            var changeEvent = onChange[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.PreviousState.Should().Be( VariableState.Default );
            changeEvent.NewState.Should().Be( VariableState.Default );
            changeEvent.SourceEvent.Should().BeSameAs( onVariableChange );
            changeEvent.AddedElements.Should().BeEmpty();
            changeEvent.RemovedElements.Should().BeEmpty();
            changeEvent.RestoredElements.Should().BeEmpty();
            changeEvent.Source.Should().Be( VariableChangeSource.ChildNode );

            var validateEvent = onValidate[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.PreviousState.Should().Be( VariableState.Default );
            validateEvent.NewState.Should().Be( sut.State );
            validateEvent.SourceEvent.Should().BeSameAs( onVariableValidate );
            validateEvent.PreviousErrors.Should().BeEmpty();
            validateEvent.NewErrors.Should().BeEmpty();
            validateEvent.PreviousWarnings.Should().BeEmpty();
            validateEvent.NewWarnings.Should().BeEmpty();
            validateEvent.AssociatedChange.Should().BeNull();
        }
    }

    [Fact]
    public void ElementChange_ShouldBeListenedToByRoot_WhenElementStopsBeingChanged()
    {
        var element = Fixture.Create<VariableMock>();
        var oldValue = element.Value;
        element.Change( Fixture.Create<string>() );

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );

        element.Change( oldValue );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Default );
            sut.Elements.ChangedElementKeys.Should().BeEmpty();
            sut.Elements.InvalidElementKeys.Should().BeEmpty();
            sut.Elements.WarningElementKeys.Should().BeEmpty();
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void ElementChange_ShouldBeListenedToByRoot_WhenElementStopsBeingInvalid()
    {
        var validator = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var element = new VariableMock( Fixture.Create<int>(), Fixture.Create<string>(), validator, Validators<string>.Pass<string>() );
        element.RefreshValidation();

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );

        element.ClearValidation();

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Default );
            sut.Elements.ChangedElementKeys.Should().BeEmpty();
            sut.Elements.InvalidElementKeys.Should().BeEmpty();
            sut.Elements.WarningElementKeys.Should().BeEmpty();
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void ElementChange_ShouldBeListenedToByRoot_WhenElementStopsBeingWarning()
    {
        var validator = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var element = new VariableMock( Fixture.Create<int>(), Fixture.Create<string>(), Validators<string>.Pass<string>(), validator );
        element.RefreshValidation();

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );

        element.ClearValidation();

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Default );
            sut.Elements.ChangedElementKeys.Should().BeEmpty();
            sut.Elements.InvalidElementKeys.Should().BeEmpty();
            sut.Elements.WarningElementKeys.Should().BeEmpty();
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void ElementChange_ShouldBeListenedToByRoot_WhenOneOfElementsStopsBeingChanged()
    {
        var (element1, element2) = Fixture.CreateMany<VariableMock>( count: 2 ).ToList();
        var oldValue = element1.Value;
        element1.Change( Fixture.Create<string>() );
        element2.Change( Fixture.Create<string>() );

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element1, element2 }, keySelector );

        element1.Change( oldValue );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Changed );
            sut.Elements.ChangedElementKeys.Should().BeEquivalentTo( element2.Key );
            sut.Elements.InvalidElementKeys.Should().BeEmpty();
            sut.Elements.WarningElementKeys.Should().BeEmpty();
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void ElementChange_ShouldBeListenedToByRoot_WhenOneOfElementsStopsBeingInvalid()
    {
        var validator = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var element1 = new VariableMock( Fixture.Create<int>(), Fixture.Create<string>(), validator, Validators<string>.Pass<string>() );
        var element2 = new VariableMock( Fixture.Create<int>(), Fixture.Create<string>(), validator, Validators<string>.Pass<string>() );
        element1.RefreshValidation();
        element2.RefreshValidation();

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element1, element2 }, keySelector );

        element1.ClearValidation();

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Invalid );
            sut.Elements.ChangedElementKeys.Should().BeEmpty();
            sut.Elements.InvalidElementKeys.Should().BeEquivalentTo( element2.Key );
            sut.Elements.WarningElementKeys.Should().BeEmpty();
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void ElementChange_ShouldBeListenedToByRoot_WhenOneOfElementsStopsBeingWarning()
    {
        var validator = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var element1 = new VariableMock( Fixture.Create<int>(), Fixture.Create<string>(), Validators<string>.Pass<string>(), validator );
        var element2 = new VariableMock( Fixture.Create<int>(), Fixture.Create<string>(), Validators<string>.Pass<string>(), validator );
        element1.RefreshValidation();
        element2.RefreshValidation();

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element1, element2 }, keySelector );

        element1.ClearValidation();

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Warning );
            sut.Elements.ChangedElementKeys.Should().BeEmpty();
            sut.Elements.InvalidElementKeys.Should().BeEmpty();
            sut.Elements.WarningElementKeys.Should().BeEquivalentTo( element2.Key );
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void ElementChange_ShouldNotBeListenedToByRoot_WhenRemovedInitialElementChanges()
    {
        var validator = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var element = new VariableMock( Fixture.Create<int>(), Fixture.Create<string>(), validator, validator );
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );

        var sut = CollectionVariableRoot.WithoutValidators<string>.Create(
            new[] { element },
            CollectionVariableRootChanges<int, VariableMock>.Empty,
            keySelector );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        element.Change( Fixture.Create<string>() );

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Changed );
            sut.Elements.ChangedElementKeys.Should().BeEmpty();
            sut.Elements.InvalidElementKeys.Should().BeEmpty();
            sut.Elements.WarningElementKeys.Should().BeEmpty();
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEquivalentTo( element.Key );
            onChange.Should().BeEmpty();
            onValidate.Should().BeEmpty();
        }
    }

    [Fact]
    public void ElementChange_ShouldBeListenedToByRoot_WhenElementGetsDisposed()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var errorsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();
        var warningsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();

        var sut = CollectionVariableRoot.Create(
            new[] { element },
            keySelector,
            errorsValidator: errorsValidator,
            warningsValidator: warningsValidator );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        element.Dispose();

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Changed | VariableState.Dirty );
            sut.Elements.ChangedElementKeys.Should().BeEmpty();
            sut.Elements.InvalidElementKeys.Should().BeEmpty();
            sut.Elements.WarningElementKeys.Should().BeEmpty();
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEquivalentTo( element.Key );
            sut.GetChildren().Should().BeEquivalentTo( element );
            onChange.Should().HaveCount( 1 );
            onValidate.Should().HaveCount( 1 );

            var changeEvent = onChange[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.PreviousState.Should().Be( VariableState.Default );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.SourceEvent.Should().BeNull();
            changeEvent.AddedElements.Should().BeEmpty();
            changeEvent.RemovedElements.Should().BeSequentiallyEqualTo( element );
            changeEvent.RestoredElements.Should().BeEmpty();
            changeEvent.Source.Should().Be( VariableChangeSource.Change );

            var validateEvent = onValidate[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.PreviousState.Should().Be( VariableState.Default );
            validateEvent.NewState.Should().Be( sut.State );
            validateEvent.SourceEvent.Should().BeNull();
            validateEvent.PreviousErrors.Should().BeEmpty();
            validateEvent.NewErrors.Should().BeEmpty();
            validateEvent.PreviousWarnings.Should().BeEmpty();
            validateEvent.NewWarnings.Should().BeEmpty();
            validateEvent.AssociatedChange.Should().BeSameAs( changeEvent );

            errorsValidator.VerifyCalls()
                .Received(
                    v => { _ = v.Validate( sut.Elements ); },
                    1 );

            warningsValidator.VerifyCalls()
                .Received(
                    v => { _ = v.Validate( sut.Elements ); },
                    1 );
        }
    }

    [Fact]
    public void ElementChange_ShouldBeListenedToByRoot_WhenNonInitialElementGetsDisposed()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var errorsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();
        var warningsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();

        var sut = CollectionVariableRoot.Create(
            Array.Empty<VariableMock>(),
            new CollectionVariableRootChanges<int, VariableMock>( new[] { element }, Array.Empty<int>() ),
            keySelector,
            errorsValidator: errorsValidator,
            warningsValidator: warningsValidator );

        var onChange = new List<CollectionVariableRootChangeEvent<int, VariableMock, string>>();
        var onValidate = new List<CollectionVariableRootValidationEvent<int, VariableMock, string>>();
        sut.OnChange.Listen( EventListener.Create<CollectionVariableRootChangeEvent<int, VariableMock, string>>( onChange.Add ) );
        sut.OnValidate.Listen( EventListener.Create<CollectionVariableRootValidationEvent<int, VariableMock, string>>( onValidate.Add ) );

        element.Dispose();

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( VariableState.Dirty );
            sut.Elements.ChangedElementKeys.Should().BeEmpty();
            sut.Elements.InvalidElementKeys.Should().BeEmpty();
            sut.Elements.WarningElementKeys.Should().BeEmpty();
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEmpty();
            sut.GetChildren().Should().BeEmpty();
            onChange.Should().HaveCount( 1 );
            onValidate.Should().HaveCount( 1 );

            var changeEvent = onChange[0];
            changeEvent.Variable.Should().BeSameAs( sut );
            changeEvent.PreviousState.Should().Be( VariableState.Changed );
            changeEvent.NewState.Should().Be( sut.State );
            changeEvent.SourceEvent.Should().BeNull();
            changeEvent.AddedElements.Should().BeEmpty();
            changeEvent.RemovedElements.Should().BeSequentiallyEqualTo( element );
            changeEvent.RestoredElements.Should().BeEmpty();
            changeEvent.Source.Should().Be( VariableChangeSource.Change );

            var validateEvent = onValidate[0];
            validateEvent.Variable.Should().BeSameAs( sut );
            validateEvent.PreviousState.Should().Be( VariableState.Changed );
            validateEvent.NewState.Should().Be( sut.State );
            validateEvent.SourceEvent.Should().BeNull();
            validateEvent.PreviousErrors.Should().BeEmpty();
            validateEvent.NewErrors.Should().BeEmpty();
            validateEvent.PreviousWarnings.Should().BeEmpty();
            validateEvent.NewWarnings.Should().BeEmpty();
            validateEvent.AssociatedChange.Should().BeSameAs( changeEvent );

            errorsValidator.VerifyCalls()
                .Received(
                    v => { _ = v.Validate( sut.Elements ); },
                    1 );

            warningsValidator.VerifyCalls()
                .Received(
                    v => { _ = v.Validate( sut.Elements ); },
                    1 );
        }
    }
}
