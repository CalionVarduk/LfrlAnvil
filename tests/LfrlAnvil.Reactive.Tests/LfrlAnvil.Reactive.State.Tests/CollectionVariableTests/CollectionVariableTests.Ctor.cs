using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Validation;
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Reactive.State.Tests.CollectionVariableTests;

public partial class CollectionVariableTests
{
    [Fact]
    public void Ctor_ShouldReturnCorrectResult_WhenInitialElementsAreEmpty()
    {
        var key = Fixture.Create<int>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var keyComparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var elementComparer = EqualityComparerFactory<TestElement>.Create( ReferenceEquals );
        var errorsValidator = Substitute.For<IValidator<ICollectionVariableElements<int, TestElement, string>, string>>();
        var warningsValidator = Substitute.For<IValidator<ICollectionVariableElements<int, TestElement, string>, string>>();
        var elementErrorsValidator = Substitute.For<IValidator<TestElement, string>>();
        var elementWarningsValidator = Substitute.For<IValidator<TestElement, string>>();

        var sut = new CollectionVariable<int, TestElement, string>(
            Array.Empty<TestElement>(),
            keySelector,
            keyComparer,
            elementComparer,
            errorsValidator,
            warningsValidator,
            elementErrorsValidator,
            elementWarningsValidator );

        Assertion.All(
                sut.Parent.TestNull(),
                sut.InitialElements.TestEmpty(),
                sut.Elements.TestEmpty(),
                sut.Elements.Count.TestEquals( 0 ),
                sut.Elements.ElementComparer.TestRefEquals( elementComparer ),
                sut.Elements.KeyComparer.TestRefEquals( keyComparer ),
                sut.Elements.Keys.TestEmpty(),
                sut.Elements.Values.TestEmpty(),
                sut.Elements.InvalidElementKeys.TestEmpty(),
                sut.Elements.WarningElementKeys.TestEmpty(),
                sut.Elements.ModifiedElementKeys.TestEmpty(),
                sut.Elements.ErrorsValidator.TestRefEquals( elementErrorsValidator ),
                sut.Elements.WarningsValidator.TestRefEquals( elementWarningsValidator ),
                sut.KeySelector.TestRefEquals( keySelector ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                sut.State.TestEquals( VariableState.Default ),
                sut.ErrorsValidator.TestRefEquals( errorsValidator ),
                sut.WarningsValidator.TestRefEquals( warningsValidator ),
                sut.Elements.ContainsKey( key ).TestFalse(),
                sut.Elements.TryGetValue( key, out var tryGetResult ).TestFalse(),
                tryGetResult.TestEquals( default( TestElement ) ),
                sut.Elements.GetErrors( key ).TestEmpty(),
                sut.Elements.GetWarnings( key ).TestEmpty(),
                sut.Elements.GetState( key ).TestEquals( CollectionVariableElementState.NotFound ),
                (( IReadOnlyDictionary<int, TestElement> )sut.Elements).Keys.TestRefEquals( sut.Elements.Keys ),
                (( IReadOnlyDictionary<int, TestElement> )sut.Elements).Values.TestRefEquals( sut.Elements.Values ),
                (( ICollectionVariableElements<int, TestElement> )sut.Elements).GetErrors( key )
                .Cast<object>()
                .TestSetEqual( sut.Elements.GetErrors( key ) ),
                (( ICollectionVariableElements<int, TestElement> )sut.Elements).GetWarnings( key )
                .Cast<object>()
                .TestSetEqual( sut.Elements.GetWarnings( key ) ),
                (( ICollectionVariableElements )sut.Elements).Count.TestEquals( sut.Elements.Count ),
                (( ICollectionVariableElements )sut.Elements).Keys.TestRefEquals( sut.Elements.Keys ),
                (( ICollectionVariableElements )sut.Elements).Values.TestRefEquals( sut.Elements.Values ),
                (( ICollectionVariableElements )sut.Elements).InvalidElementKeys.TestRefEquals( sut.Elements.InvalidElementKeys ),
                (( ICollectionVariableElements )sut.Elements).WarningElementKeys.TestRefEquals( sut.Elements.WarningElementKeys ),
                (( ICollectionVariableElements )sut.Elements).ModifiedElementKeys.TestRefEquals( sut.Elements.ModifiedElementKeys ),
                (( IReadOnlyCollectionVariable<int, TestElement> )sut).Elements.TestRefEquals( sut.Elements ),
                (( IReadOnlyCollectionVariable<int, TestElement> )sut).OnChange.TestRefEquals( sut.OnChange ),
                (( IReadOnlyCollectionVariable )sut).KeyType.TestEquals( typeof( int ) ),
                (( IReadOnlyCollectionVariable )sut).ElementType.TestEquals( typeof( TestElement ) ),
                (( IReadOnlyCollectionVariable )sut).ValidationResultType.TestEquals( typeof( string ) ),
                (( IReadOnlyCollectionVariable )sut).InitialElements.TestRefEquals( sut.InitialElements.Values ),
                (( IReadOnlyCollectionVariable )sut).Elements.TestRefEquals( sut.Elements ),
                (( IReadOnlyCollectionVariable )sut).Errors.Cast<object>().TestSetEqual( sut.Errors ),
                (( IReadOnlyCollectionVariable )sut).Warnings.Cast<object>().TestSetEqual( sut.Warnings ),
                (( IReadOnlyCollectionVariable )sut).OnValidate.TestEquals( sut.OnValidate ),
                (( IReadOnlyCollectionVariable )sut).OnChange.TestEquals( sut.OnChange ),
                (( IVariableNode )sut).OnValidate.TestEquals( sut.OnValidate ),
                (( IVariableNode )sut).OnChange.TestEquals( sut.OnChange ),
                (( IVariableNode )sut).GetChildren().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldReturnCorrectResult_WhenInitialElementsAreNotEmpty()
    {
        var elements = Fixture.CreateMany<TestElement>( count: 3 ).ToList();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var keyComparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var elementComparer = EqualityComparerFactory<TestElement>.Create( ReferenceEquals );
        var errorsValidator = Substitute.For<IValidator<ICollectionVariableElements<int, TestElement, string>, string>>();
        var warningsValidator = Substitute.For<IValidator<ICollectionVariableElements<int, TestElement, string>, string>>();
        var elementErrorsValidator = Substitute.For<IValidator<TestElement, string>>();
        var elementWarningsValidator = Substitute.For<IValidator<TestElement, string>>();

        var sut = new CollectionVariable<int, TestElement, string>(
            elements,
            keySelector,
            keyComparer,
            elementComparer,
            errorsValidator,
            warningsValidator,
            elementErrorsValidator,
            elementWarningsValidator );

        Assertion.All(
                sut.Parent.TestNull(),
                sut.InitialElements.TestSetEqual( elements.Select( e => KeyValuePair.Create( e.Key, e ) ) ),
                sut.Elements.TestSetEqual( elements.Select( e => KeyValuePair.Create( e.Key, e ) ) ),
                sut.Elements.Count.TestEquals( elements.Count ),
                sut.Elements.ElementComparer.TestRefEquals( elementComparer ),
                sut.Elements.KeyComparer.TestRefEquals( keyComparer ),
                sut.Elements.Keys.TestSetEqual( elements.Select( e => e.Key ) ),
                sut.Elements.Values.TestSetEqual( elements ),
                sut.Elements.InvalidElementKeys.TestEmpty(),
                sut.Elements.WarningElementKeys.TestEmpty(),
                sut.Elements.ModifiedElementKeys.TestEmpty(),
                sut.Elements.ErrorsValidator.TestRefEquals( elementErrorsValidator ),
                sut.Elements.WarningsValidator.TestRefEquals( elementWarningsValidator ),
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
                sut.Elements.GetErrors( elements[0].Key ).TestEmpty(),
                sut.Elements.GetErrors( elements[1].Key ).TestEmpty(),
                sut.Elements.GetErrors( elements[2].Key ).TestEmpty(),
                sut.Elements.GetWarnings( elements[0].Key ).TestEmpty(),
                sut.Elements.GetWarnings( elements[1].Key ).TestEmpty(),
                sut.Elements.GetWarnings( elements[2].Key ).TestEmpty(),
                sut.Elements.GetState( elements[0].Key ).TestEquals( CollectionVariableElementState.Default ),
                sut.Elements.GetState( elements[1].Key ).TestEquals( CollectionVariableElementState.Default ),
                sut.Elements.GetState( elements[2].Key ).TestEquals( CollectionVariableElementState.Default ) )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldReturnCorrectResult_WhenInitialElementsAreNotEmptyAndSomeElementsAreNotConsideredEqualToSelf()
    {
        var element = Fixture.Create<TestElement>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var keyComparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var elementComparer = EqualityComparerFactory<TestElement>.Create( (_, _) => false );
        var errorsValidator = Substitute.For<IValidator<ICollectionVariableElements<int, TestElement, string>, string>>();
        var warningsValidator = Substitute.For<IValidator<ICollectionVariableElements<int, TestElement, string>, string>>();
        var elementErrorsValidator = Substitute.For<IValidator<TestElement, string>>();
        var elementWarningsValidator = Substitute.For<IValidator<TestElement, string>>();

        var sut = new CollectionVariable<int, TestElement, string>(
            new[] { element },
            keySelector,
            keyComparer,
            elementComparer,
            errorsValidator,
            warningsValidator,
            elementErrorsValidator,
            elementWarningsValidator );

        Assertion.All(
                sut.Parent.TestNull(),
                sut.InitialElements.TestSetEqual( [ KeyValuePair.Create( element.Key, element ) ] ),
                sut.Elements.TestSetEqual( [ KeyValuePair.Create( element.Key, element ) ] ),
                sut.Elements.Count.TestEquals( 1 ),
                sut.Elements.ElementComparer.TestRefEquals( elementComparer ),
                sut.Elements.KeyComparer.TestRefEquals( keyComparer ),
                sut.Elements.Keys.TestSetEqual( [ element.Key ] ),
                sut.Elements.Values.TestSetEqual( [ element ] ),
                sut.Elements.InvalidElementKeys.TestEmpty(),
                sut.Elements.WarningElementKeys.TestEmpty(),
                sut.Elements.ModifiedElementKeys.TestSetEqual( [ element.Key ] ),
                sut.Elements.ErrorsValidator.TestRefEquals( elementErrorsValidator ),
                sut.Elements.WarningsValidator.TestRefEquals( elementWarningsValidator ),
                sut.KeySelector.TestRefEquals( keySelector ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                sut.State.TestEquals( VariableState.Changed ),
                sut.ErrorsValidator.TestRefEquals( errorsValidator ),
                sut.WarningsValidator.TestRefEquals( warningsValidator ),
                sut.Elements[element.Key].TestRefEquals( element ),
                sut.Elements.ContainsKey( element.Key ).TestTrue(),
                sut.Elements.TryGetValue( element.Key, out var tryGetResult ).TestTrue(),
                tryGetResult.TestRefEquals( element ),
                sut.Elements.GetErrors( element.Key ).TestEmpty(),
                sut.Elements.GetWarnings( element.Key ).TestEmpty(),
                sut.Elements.GetState( element.Key ).TestEquals( CollectionVariableElementState.Changed ) )
            .Go();
    }

    [Fact]
    public void Ctor_WithElements_ShouldReturnCorrectResult()
    {
        var allElements = Fixture.CreateMany<TestElement>( count: 4 ).ToList();
        var changedElement = new TestElement( allElements[0].Key, Fixture.Create<string>() );
        var initialElements = new[] { allElements[0], allElements[1], allElements[2] };
        var elements = new[] { changedElement, allElements[1], allElements[3] };

        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var keyComparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var elementComparer = EqualityComparerFactory<TestElement>.Create( ReferenceEquals );
        var errorsValidator = Substitute.For<IValidator<ICollectionVariableElements<int, TestElement, string>, string>>();
        var warningsValidator = Substitute.For<IValidator<ICollectionVariableElements<int, TestElement, string>, string>>();
        var elementErrorsValidator = Substitute.For<IValidator<TestElement, string>>();
        var elementWarningsValidator = Substitute.For<IValidator<TestElement, string>>();

        var sut = new CollectionVariable<int, TestElement, string>(
            initialElements,
            elements,
            keySelector,
            keyComparer,
            elementComparer,
            errorsValidator,
            warningsValidator,
            elementErrorsValidator,
            elementWarningsValidator );

        Assertion.All(
                sut.Parent.TestNull(),
                sut.InitialElements.TestSetEqual( initialElements.Select( e => KeyValuePair.Create( e.Key, e ) ) ),
                sut.Elements.TestSetEqual( elements.Select( e => KeyValuePair.Create( e.Key, e ) ) ),
                sut.Elements.Count.TestEquals( elements.Length ),
                sut.Elements.ElementComparer.TestRefEquals( elementComparer ),
                sut.Elements.KeyComparer.TestRefEquals( keyComparer ),
                sut.Elements.Keys.TestSetEqual( elements.Select( e => e.Key ) ),
                sut.Elements.Values.TestSetEqual( elements.AsEnumerable() ),
                sut.Elements.InvalidElementKeys.TestEmpty(),
                sut.Elements.WarningElementKeys.TestEmpty(),
                sut.Elements.ModifiedElementKeys.TestSetEqual( [ elements[0].Key, elements[2].Key, initialElements[2].Key ] ),
                sut.Elements.ErrorsValidator.TestRefEquals( elementErrorsValidator ),
                sut.Elements.WarningsValidator.TestRefEquals( elementWarningsValidator ),
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
                tryGetResult4.TestEquals( default( TestElement ) ),
                sut.Elements.GetErrors( elements[0].Key ).TestEmpty(),
                sut.Elements.GetErrors( elements[1].Key ).TestEmpty(),
                sut.Elements.GetErrors( elements[2].Key ).TestEmpty(),
                sut.Elements.GetErrors( initialElements[2].Key ).TestEmpty(),
                sut.Elements.GetWarnings( elements[0].Key ).TestEmpty(),
                sut.Elements.GetWarnings( elements[1].Key ).TestEmpty(),
                sut.Elements.GetWarnings( elements[2].Key ).TestEmpty(),
                sut.Elements.GetWarnings( initialElements[2].Key ).TestEmpty(),
                sut.Elements.GetState( elements[0].Key ).TestEquals( CollectionVariableElementState.Changed ),
                sut.Elements.GetState( elements[1].Key ).TestEquals( CollectionVariableElementState.Default ),
                sut.Elements.GetState( elements[2].Key ).TestEquals( CollectionVariableElementState.Added ),
                sut.Elements.GetState( initialElements[2].Key ).TestEquals( CollectionVariableElementState.Removed ) )
            .Go();
    }

    [Fact]
    public void Ctor_WithElements_ShouldReturnCorrectResult_WithDefaultParameters()
    {
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = new CollectionVariable<int, TestElement, string>( Array.Empty<TestElement>(), Array.Empty<TestElement>(), keySelector );

        Assertion.All(
                sut.Parent.TestNull(),
                sut.Elements.ElementComparer.TestRefEquals( EqualityComparer<TestElement>.Default ),
                sut.Elements.KeyComparer.TestRefEquals( EqualityComparer<int>.Default ),
                sut.Elements.ErrorsValidator.TestType().AssignableTo<PassingValidator<TestElement, string>>(),
                sut.Elements.WarningsValidator.TestType().AssignableTo<PassingValidator<TestElement, string>>(),
                sut.KeySelector.TestRefEquals( keySelector ),
                sut.ErrorsValidator.TestType()
                    .AssignableTo<PassingValidator<ICollectionVariableElements<int, TestElement, string>, string>>(),
                sut.WarningsValidator.TestType()
                    .AssignableTo<PassingValidator<ICollectionVariableElements<int, TestElement, string>, string>>() )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldReturnCorrectResult_WithDefaultParameters()
    {
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = new CollectionVariable<int, TestElement, string>( Array.Empty<TestElement>(), keySelector );

        Assertion.All(
                sut.Parent.TestNull(),
                sut.Elements.ElementComparer.TestRefEquals( EqualityComparer<TestElement>.Default ),
                sut.Elements.KeyComparer.TestRefEquals( EqualityComparer<int>.Default ),
                sut.Elements.ErrorsValidator.TestType().AssignableTo<PassingValidator<TestElement, string>>(),
                sut.Elements.WarningsValidator.TestType().AssignableTo<PassingValidator<TestElement, string>>(),
                sut.KeySelector.TestRefEquals( keySelector ),
                sut.ErrorsValidator.TestType()
                    .AssignableTo<PassingValidator<ICollectionVariableElements<int, TestElement, string>, string>>(),
                sut.WarningsValidator.TestType()
                    .AssignableTo<PassingValidator<ICollectionVariableElements<int, TestElement, string>, string>>() )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldNotInvokeErrorsValidators()
    {
        var element = Fixture.Create<TestElement>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var errorsValidator = Validators<string>.Fail<ICollectionVariableElements<int, TestElement, string>>( Fixture.Create<string>() );
        var elementErrorsValidator = Validators<string>.Fail<TestElement>( Fixture.Create<string>() );

        var sut = CollectionVariable.Create(
            new[] { element },
            keySelector,
            errorsValidator: errorsValidator,
            elementErrorsValidator: elementErrorsValidator );

        Assertion.All(
                sut.Errors.TestEmpty(),
                sut.Elements.GetErrors( element.Key ).TestEmpty(),
                sut.Elements.InvalidElementKeys.TestEmpty(),
                sut.Elements.GetState( element.Key ).TestEquals( CollectionVariableElementState.Default ) )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldNotInvokeWarningsValidators()
    {
        var element = Fixture.Create<TestElement>();
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var warningsValidator = Validators<string>.Fail<ICollectionVariableElements<int, TestElement, string>>( Fixture.Create<string>() );
        var elementWarningsValidator = Validators<string>.Fail<TestElement>( Fixture.Create<string>() );

        var sut = CollectionVariable.Create(
            new[] { element },
            keySelector,
            warningsValidator: warningsValidator,
            elementWarningsValidator: elementWarningsValidator );

        Assertion.All(
                sut.Warnings.TestEmpty(),
                sut.Elements.GetWarnings( element.Key ).TestEmpty(),
                sut.Elements.WarningElementKeys.TestEmpty(),
                sut.Elements.GetState( element.Key ).TestEquals( CollectionVariableElementState.Default ) )
            .Go();
    }
}
