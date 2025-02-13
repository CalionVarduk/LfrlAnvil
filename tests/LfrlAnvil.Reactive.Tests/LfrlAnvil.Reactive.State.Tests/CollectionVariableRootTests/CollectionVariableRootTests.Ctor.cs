using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.State.Tests.VariableRootTests;
using LfrlAnvil.Validation;
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Reactive.State.Tests.CollectionVariableRootTests;

public partial class CollectionVariableRootTests
{
    [Fact]
    public void Ctor_ShouldReturnCorrectResult_WhenInitialElementsAreEmpty()
    {
        var key = Fixture.Create<int>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var keyComparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var errorsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();
        var warningsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();

        var sut = new CollectionVariableRoot<int, VariableMock, string>(
            Array.Empty<VariableMock>(),
            keySelector,
            keyComparer,
            errorsValidator,
            warningsValidator );

        Assertion.All(
                sut.Parent.TestNull(),
                sut.InitialElements.TestEmpty(),
                sut.Elements.TestEmpty(),
                sut.Elements.Count.TestEquals( 0 ),
                sut.Elements.KeyComparer.TestRefEquals( keyComparer ),
                sut.Elements.Keys.TestEmpty(),
                sut.Elements.Values.TestEmpty(),
                sut.Elements.InvalidElementKeys.TestEmpty(),
                sut.Elements.WarningElementKeys.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestEmpty(),
                sut.Elements.ChangedElementKeys.TestEmpty(),
                sut.KeySelector.TestRefEquals( keySelector ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                sut.State.TestEquals( VariableState.Default ),
                sut.ErrorsValidator.TestRefEquals( errorsValidator ),
                sut.WarningsValidator.TestRefEquals( warningsValidator ),
                sut.Elements.ContainsKey( key ).TestFalse(),
                sut.Elements.TryGetValue( key, out var tryGetResult ).TestFalse(),
                tryGetResult.TestEquals( default ),
                (( IReadOnlyDictionary<int, VariableMock> )sut.Elements).Keys.TestRefEquals( sut.Elements.Keys ),
                (( IReadOnlyDictionary<int, VariableMock> )sut.Elements).Values.TestRefEquals( sut.Elements.Values ),
                (( ICollectionVariableRootElements )sut.Elements).Count.TestEquals( sut.Elements.Count ),
                (( ICollectionVariableRootElements )sut.Elements).Keys.TestRefEquals( sut.Elements.Keys ),
                (( ICollectionVariableRootElements )sut.Elements).Values.TestRefEquals( sut.Elements.Values ),
                (( ICollectionVariableRootElements )sut.Elements).InvalidElementKeys.TestRefEquals( sut.Elements.InvalidElementKeys ),
                (( ICollectionVariableRootElements )sut.Elements).WarningElementKeys.TestRefEquals( sut.Elements.WarningElementKeys ),
                (( ICollectionVariableRootElements )sut.Elements).AddedElementKeys.TestRefEquals( sut.Elements.AddedElementKeys ),
                (( ICollectionVariableRootElements )sut.Elements).RemovedElementKeys.TestRefEquals( sut.Elements.RemovedElementKeys ),
                (( ICollectionVariableRootElements )sut.Elements).ChangedElementKeys.TestRefEquals( sut.Elements.ChangedElementKeys ),
                (( IReadOnlyCollectionVariableRoot<int, VariableMock> )sut).Elements.TestRefEquals( sut.Elements ),
                (( IReadOnlyCollectionVariableRoot<int, VariableMock> )sut).OnChange.TestRefEquals( sut.OnChange ),
                (( IReadOnlyCollectionVariableRoot )sut).KeyType.TestEquals( typeof( int ) ),
                (( IReadOnlyCollectionVariableRoot )sut).ElementType.TestEquals( typeof( VariableMock ) ),
                (( IReadOnlyCollectionVariableRoot )sut).ValidationResultType.TestEquals( typeof( string ) ),
                (( IReadOnlyCollectionVariableRoot )sut).InitialElements.TestRefEquals( sut.InitialElements.Values ),
                (( IReadOnlyCollectionVariableRoot )sut).Elements.TestRefEquals( sut.Elements ),
                (( IReadOnlyCollectionVariableRoot )sut).Errors.Cast<object>().TestSetEqual( sut.Errors ),
                (( IReadOnlyCollectionVariableRoot )sut).Warnings.Cast<object>().TestSetEqual( sut.Warnings ),
                (( IReadOnlyCollectionVariableRoot )sut).OnValidate.TestEquals( sut.OnValidate ),
                (( IReadOnlyCollectionVariableRoot )sut).OnChange.TestEquals( sut.OnChange ),
                (( IVariableNode )sut).OnValidate.TestEquals( sut.OnValidate ),
                (( IVariableNode )sut).OnChange.TestEquals( sut.OnChange ),
                (( IVariableNode )sut).GetChildren().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldReturnCorrectResult_WhenInitialElementsAreNotEmpty()
    {
        var elements = Fixture.CreateMany<VariableMock>( count: 3 ).ToList();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var keyComparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var errorsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();
        var warningsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();

        var sut = new CollectionVariableRoot<int, VariableMock, string>(
            elements,
            keySelector,
            keyComparer,
            errorsValidator,
            warningsValidator );

        Assertion.All(
                sut.Parent.TestNull(),
                sut.InitialElements.TestSetEqual( elements.Select( e => KeyValuePair.Create( e.Key, e ) ) ),
                sut.Elements.TestSetEqual( elements.Select( e => KeyValuePair.Create( e.Key, e ) ) ),
                sut.Elements.Count.TestEquals( elements.Count ),
                sut.Elements.KeyComparer.TestRefEquals( keyComparer ),
                sut.Elements.Keys.TestSetEqual( elements.Select( e => e.Key ) ),
                sut.Elements.Values.TestSetEqual( elements ),
                sut.Elements.InvalidElementKeys.TestEmpty(),
                sut.Elements.WarningElementKeys.TestEmpty(),
                sut.Elements.AddedElementKeys.TestEmpty(),
                sut.Elements.RemovedElementKeys.TestEmpty(),
                sut.Elements.ChangedElementKeys.TestEmpty(),
                sut.KeySelector.TestRefEquals( keySelector ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                sut.State.TestEquals( VariableState.Default ),
                sut.ErrorsValidator.TestRefEquals( errorsValidator ),
                sut.WarningsValidator.TestRefEquals( warningsValidator ),
                sut.Elements[elements[0].Key].TestRefEquals( elements[0] ),
                sut.Elements[elements[1].Key].TestRefEquals( elements[1] ),
                sut.Elements[elements[2].Key].TestRefEquals( elements[2] ),
                sut.Elements.ContainsKey( elements[0].Key ).TestTrue(),
                sut.Elements.ContainsKey( elements[1].Key ).TestTrue(),
                sut.Elements.ContainsKey( elements[2].Key ).TestTrue(),
                sut.Elements.TryGetValue( elements[0].Key, out var tryGetResult1 ).TestTrue(),
                tryGetResult1.TestRefEquals( elements[0] ),
                sut.Elements.TryGetValue( elements[1].Key, out var tryGetResult2 ).TestTrue(),
                tryGetResult2.TestRefEquals( elements[1] ),
                sut.Elements.TryGetValue( elements[2].Key, out var tryGetResult3 ).TestTrue(),
                tryGetResult3.TestRefEquals( elements[2] ),
                elements.TestAll( (e, _) => e.Parent.TestRefEquals( sut ) ),
                (( IVariableNode )sut).GetChildren().TestSetEqual( elements ) )
            .Go();
    }

    [Fact]
    public void Ctor_WithElements_ShouldReturnCorrectResult()
    {
        var allElements = Fixture.CreateMany<VariableMock>( count: 4 ).ToList();
        allElements[0].Change( Fixture.Create<string>() );
        var initialElements = new[] { allElements[0], allElements[1], allElements[2] };
        var elements = new[] { allElements[0], allElements[1], allElements[3] };

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var keyComparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var errorsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();
        var warningsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();

        var sut = new CollectionVariableRoot<int, VariableMock, string>(
            initialElements,
            new CollectionVariableRootChanges<int, VariableMock>(
                new[] { allElements[3] },
                new[] { allElements[0].Key, allElements[1].Key } ),
            keySelector,
            keyComparer,
            errorsValidator,
            warningsValidator );

        Assertion.All(
                sut.Parent.TestNull(),
                sut.InitialElements.TestSetEqual( initialElements.Select( e => KeyValuePair.Create( e.Key, e ) ) ),
                sut.Elements.TestSetEqual( elements.Select( e => KeyValuePair.Create( e.Key, e ) ) ),
                sut.Elements.Count.TestEquals( elements.Length ),
                sut.Elements.KeyComparer.TestRefEquals( keyComparer ),
                sut.Elements.Keys.TestSetEqual( elements.Select( e => e.Key ) ),
                sut.Elements.Values.TestSetEqual( elements.AsEnumerable() ),
                sut.Elements.InvalidElementKeys.TestEmpty(),
                sut.Elements.WarningElementKeys.TestEmpty(),
                sut.Elements.AddedElementKeys.TestSetEqual( [ allElements[3].Key ] ),
                sut.Elements.RemovedElementKeys.TestSetEqual( [ allElements[2].Key ] ),
                sut.Elements.ChangedElementKeys.TestSetEqual( [ allElements[0].Key ] ),
                sut.KeySelector.TestRefEquals( keySelector ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                sut.State.TestEquals( VariableState.Changed ),
                sut.ErrorsValidator.TestRefEquals( errorsValidator ),
                sut.WarningsValidator.TestRefEquals( warningsValidator ),
                sut.Elements[elements[0].Key].TestRefEquals( elements[0] ),
                sut.Elements[elements[1].Key].TestRefEquals( elements[1] ),
                sut.Elements[elements[2].Key].TestRefEquals( elements[2] ),
                sut.Elements.ContainsKey( elements[0].Key ).TestTrue(),
                sut.Elements.ContainsKey( elements[1].Key ).TestTrue(),
                sut.Elements.ContainsKey( elements[2].Key ).TestTrue(),
                sut.Elements.ContainsKey( initialElements[2].Key ).TestFalse(),
                sut.Elements.TryGetValue( elements[0].Key, out var tryGetResult1 ).TestTrue(),
                tryGetResult1.TestRefEquals( elements[0] ),
                sut.Elements.TryGetValue( elements[1].Key, out var tryGetResult2 ).TestTrue(),
                tryGetResult2.TestRefEquals( elements[1] ),
                sut.Elements.TryGetValue( elements[2].Key, out var tryGetResult3 ).TestTrue(),
                tryGetResult3.TestRefEquals( elements[2] ),
                sut.Elements.TryGetValue( initialElements[2].Key, out var tryGetResult4 ).TestFalse(),
                tryGetResult4.TestEquals( default ),
                allElements.TestAll( (e, _) => e.Parent.TestRefEquals( sut ) ),
                (( IVariableNode )sut).GetChildren().TestSetEqual( allElements ) )
            .Go();
    }

    [Fact]
    public void Ctor_WithElements_ShouldReturnCorrectResult_WithDefaultParameters()
    {
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = new CollectionVariableRoot<int, VariableMock, string>(
            Array.Empty<VariableMock>(),
            new CollectionVariableRootChanges<int, VariableMock>(),
            keySelector );

        Assertion.All(
                sut.Parent.TestNull(),
                sut.Elements.KeyComparer.TestRefEquals( EqualityComparer<int>.Default ),
                sut.KeySelector.TestRefEquals( keySelector ),
                sut.ErrorsValidator.TestType().AssignableTo<PassingValidator<ICollectionVariableRootElements<int, VariableMock>, string>>(),
                sut.WarningsValidator.TestType()
                    .AssignableTo<PassingValidator<ICollectionVariableRootElements<int, VariableMock>, string>>() )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldReturnCorrectResult_WithDefaultParameters()
    {
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = new CollectionVariableRoot<int, VariableMock, string>( Array.Empty<VariableMock>(), keySelector );

        Assertion.All(
                sut.Parent.TestNull(),
                sut.Elements.KeyComparer.TestRefEquals( EqualityComparer<int>.Default ),
                sut.KeySelector.TestRefEquals( keySelector ),
                sut.ErrorsValidator.TestType().AssignableTo<PassingValidator<ICollectionVariableRootElements<int, VariableMock>, string>>(),
                sut.WarningsValidator.TestType()
                    .AssignableTo<PassingValidator<ICollectionVariableRootElements<int, VariableMock>, string>>() )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldNotInvokeErrorsValidators()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var errorsValidator = Validators<string>.Fail<ICollectionVariableRootElements<int, VariableMock>>( Fixture.Create<string>() );

        var sut = CollectionVariableRoot.Create(
            new[] { element },
            keySelector,
            errorsValidator: errorsValidator );

        sut.Errors.TestEmpty().Go();
    }

    [Fact]
    public void Ctor_ShouldNotInvokeWarningsValidators()
    {
        var element = Fixture.Create<VariableMock>();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var warningsValidator = Validators<string>.Fail<ICollectionVariableRootElements<int, VariableMock>>( Fixture.Create<string>() );

        var sut = CollectionVariableRoot.Create(
            new[] { element },
            keySelector,
            warningsValidator: warningsValidator );

        sut.Warnings.TestEmpty().Go();
    }

    [Fact]
    public void Ctor_ShouldCreateCorrectResult_WhenOneOfTheElementsIsInvalid()
    {
        var elementValidator = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var element = new VariableMock(
            Fixture.Create<int>(),
            Fixture.Create<string>(),
            elementValidator,
            Validators<string>.Pass<string>() );

        element.RefreshValidation();

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );

        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );

        Assertion.All(
                sut.Elements.InvalidElementKeys.TestSetEqual( [ element.Key ] ),
                sut.State.TestEquals( VariableState.Invalid ) )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldCreateCorrectResult_WhenOneOfTheElementsIsWarning()
    {
        var elementValidator = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var element = new VariableMock(
            Fixture.Create<int>(),
            Fixture.Create<string>(),
            Validators<string>.Pass<string>(),
            elementValidator );

        element.RefreshValidation();

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );

        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );

        Assertion.All(
                sut.Elements.WarningElementKeys.TestSetEqual( [ element.Key ] ),
                sut.State.TestEquals( VariableState.Warning ) )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldCreateCorrectResult_WhenOneOfTheElementsIsDisposed()
    {
        var element = Fixture.Create<VariableMock>();
        element.Dispose();

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );

        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );

        Assertion.All(
                sut.InitialElements.TestEmpty(),
                sut.Elements.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldCreateCorrectResult_WhenOneOfTheElementsHasAssignedParent()
    {
        var root = new VariableRootMock();
        var element = Fixture.Create<VariableMock>();
        root.ExposedRegisterNode( Fixture.Create<string>(), element );

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );

        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );

        Assertion.All(
                sut.InitialElements.TestEmpty(),
                sut.Elements.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldCreateCorrectResult_WhenOneOfTheAddedElementKeysIsAlreadyInInitialElements()
    {
        var element = Fixture.Create<VariableMock>();
        var otherElement = new VariableMock( element.Key, Fixture.Create<string>() );
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );

        var sut = CollectionVariableRoot.WithoutValidators<string>.Create(
            new[] { element },
            new CollectionVariableRootChanges<int, VariableMock>( new[] { otherElement }, new[] { element.Key } ),
            keySelector );

        Assertion.All(
                sut.InitialElements.Values.TestSetEqual( [ element ] ),
                sut.Elements.Values.TestSetEqual( [ element ] ) )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldCreateCorrectResult_WhenOneOfTheAddedElementsIsDisposed()
    {
        var (element, otherElement) = Fixture.CreateMany<VariableMock>( count: 2 ).ToList();
        otherElement.Dispose();

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );

        var sut = CollectionVariableRoot.WithoutValidators<string>.Create(
            new[] { element },
            new CollectionVariableRootChanges<int, VariableMock>( new[] { otherElement }, new[] { element.Key } ),
            keySelector );

        Assertion.All(
                sut.InitialElements.Values.TestSetEqual( [ element ] ),
                sut.Elements.Values.TestSetEqual( [ element ] ) )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldCreateCorrectResult_WhenOneOfTheAddedElementsHasAssignedParent()
    {
        var root = new VariableRootMock();
        var (element, otherElement) = Fixture.CreateMany<VariableMock>( count: 2 ).ToList();
        root.ExposedRegisterNode( Fixture.Create<string>(), otherElement );

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );

        var sut = CollectionVariableRoot.WithoutValidators<string>.Create(
            new[] { element },
            new CollectionVariableRootChanges<int, VariableMock>( new[] { otherElement }, new[] { element.Key } ),
            keySelector );

        Assertion.All(
                sut.InitialElements.Values.TestSetEqual( [ element ] ),
                sut.Elements.Values.TestSetEqual( [ element ] ) )
            .Go();
    }
}
