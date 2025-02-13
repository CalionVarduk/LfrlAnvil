using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Validation;
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Reactive.State.Tests.CollectionVariableTests;

public class StaticCollectionVariableTests : TestsBase
{
    [Fact]
    public void Create_WithoutValidators_ShouldReturnCorrectVariable()
    {
        var allElements = Fixture.CreateManyDistinct<int>( count: 6 );
        var initialElements = allElements.Take( 3 ).ToList();
        var elements = allElements.Skip( 3 ).ToList();
        var keySelector = Lambda.Of( (int e) => e );
        var keyComparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var elementComparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var sut = CollectionVariable.WithoutValidators<string>.Create(
            initialElements,
            elements,
            keySelector,
            keyComparer,
            elementComparer );

        Assertion.All(
                sut.Parent.TestNull(),
                sut.InitialElements.Values.TestSetEqual( initialElements ),
                sut.Elements.Values.TestSetEqual( elements ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                sut.State.TestEquals( VariableState.Changed ),
                sut.KeySelector.TestRefEquals( keySelector ),
                sut.ErrorsValidator.TestType().AssignableTo<PassingValidator<ICollectionVariableElements<int, int, string>, string>>(),
                sut.WarningsValidator.TestType().AssignableTo<PassingValidator<ICollectionVariableElements<int, int, string>, string>>(),
                sut.Elements.ErrorsValidator.TestType().AssignableTo<PassingValidator<int, string>>(),
                sut.Elements.WarningsValidator.TestType().AssignableTo<PassingValidator<int, string>>(),
                sut.Elements.KeyComparer.TestRefEquals( keyComparer ),
                sut.Elements.ElementComparer.TestRefEquals( elementComparer ) )
            .Go();
    }

    [Fact]
    public void Create_WithoutValidatorsAndWithInitialElementsOnly_ShouldReturnCorrectVariable()
    {
        var initialElements = Fixture.CreateManyDistinct<int>( count: 3 );
        var keySelector = Lambda.Of( (int e) => e );
        var keyComparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var elementComparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var sut = CollectionVariable.WithoutValidators<string>.Create( initialElements, keySelector, keyComparer, elementComparer );

        Assertion.All(
                sut.Parent.TestNull(),
                sut.InitialElements.Values.TestSetEqual( initialElements ),
                sut.Elements.Values.TestSetEqual( initialElements ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                sut.State.TestEquals( VariableState.Default ),
                sut.KeySelector.TestRefEquals( keySelector ),
                sut.ErrorsValidator.TestType().AssignableTo<PassingValidator<ICollectionVariableElements<int, int, string>, string>>(),
                sut.WarningsValidator.TestType().AssignableTo<PassingValidator<ICollectionVariableElements<int, int, string>, string>>(),
                sut.Elements.ErrorsValidator.TestType().AssignableTo<PassingValidator<int, string>>(),
                sut.Elements.WarningsValidator.TestType().AssignableTo<PassingValidator<int, string>>(),
                sut.Elements.KeyComparer.TestRefEquals( keyComparer ),
                sut.Elements.ElementComparer.TestRefEquals( elementComparer ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldReturnCorrectVariable()
    {
        var allElements = Fixture.CreateManyDistinct<int>( count: 6 );
        var initialElements = allElements.Take( 3 ).ToList();
        var elements = allElements.Skip( 3 ).ToList();
        var keySelector = Lambda.Of( (int e) => e );
        var keyComparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var elementComparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var errorsValidator = Substitute.For<IValidator<ICollectionVariableElements<int, int, string>, string>>();
        var warningsValidator = Substitute.For<IValidator<ICollectionVariableElements<int, int, string>, string>>();
        var elementErrorsValidator = Substitute.For<IValidator<int, string>>();
        var elementWarningsValidator = Substitute.For<IValidator<int, string>>();
        var sut = CollectionVariable.Create(
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
                sut.InitialElements.Values.TestSetEqual( initialElements ),
                sut.Elements.Values.TestSetEqual( elements ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                sut.State.TestEquals( VariableState.Changed ),
                sut.KeySelector.TestRefEquals( keySelector ),
                sut.ErrorsValidator.TestRefEquals( errorsValidator ),
                sut.WarningsValidator.TestRefEquals( warningsValidator ),
                sut.Elements.ErrorsValidator.TestRefEquals( elementErrorsValidator ),
                sut.Elements.WarningsValidator.TestRefEquals( elementWarningsValidator ),
                sut.Elements.KeyComparer.TestRefEquals( keyComparer ),
                sut.Elements.ElementComparer.TestRefEquals( elementComparer ) )
            .Go();
    }

    [Fact]
    public void Create_WithInitialElementsOnly_ShouldReturnCorrectVariable()
    {
        var initialElements = Fixture.CreateManyDistinct<int>( count: 3 );
        var keySelector = Lambda.Of( (int e) => e );
        var keyComparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var elementComparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var errorsValidator = Substitute.For<IValidator<ICollectionVariableElements<int, int, string>, string>>();
        var warningsValidator = Substitute.For<IValidator<ICollectionVariableElements<int, int, string>, string>>();
        var elementErrorsValidator = Substitute.For<IValidator<int, string>>();
        var elementWarningsValidator = Substitute.For<IValidator<int, string>>();
        var sut = CollectionVariable.Create(
            initialElements,
            keySelector,
            keyComparer,
            elementComparer,
            errorsValidator,
            warningsValidator,
            elementErrorsValidator,
            elementWarningsValidator );

        Assertion.All(
                sut.Parent.TestNull(),
                sut.InitialElements.Values.TestSetEqual( initialElements ),
                sut.Elements.Values.TestSetEqual( initialElements ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                sut.State.TestEquals( VariableState.Default ),
                sut.KeySelector.TestRefEquals( keySelector ),
                sut.ErrorsValidator.TestRefEquals( errorsValidator ),
                sut.WarningsValidator.TestRefEquals( warningsValidator ),
                sut.Elements.ErrorsValidator.TestRefEquals( elementErrorsValidator ),
                sut.Elements.WarningsValidator.TestRefEquals( elementWarningsValidator ),
                sut.Elements.KeyComparer.TestRefEquals( keyComparer ),
                sut.Elements.ElementComparer.TestRefEquals( elementComparer ) )
            .Go();
    }
}
