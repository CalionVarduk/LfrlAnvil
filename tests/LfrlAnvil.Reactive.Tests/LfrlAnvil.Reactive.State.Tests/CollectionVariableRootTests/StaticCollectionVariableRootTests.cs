using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Validation;
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Reactive.State.Tests.CollectionVariableRootTests;

public class StaticCollectionVariableRootTests : TestsBase
{
    public StaticCollectionVariableRootTests()
    {
        Fixture.Customize<VariableMock>( (_, _) => f => new VariableMock( f.Create<int>(), f.Create<string>() ) );
    }

    [Fact]
    public void Create_WithoutValidators_ShouldReturnCorrectVariable()
    {
        var allElements = Fixture.CreateMany<VariableMock>( count: 6 ).ToList();
        var initialElements = allElements.Take( 3 ).ToList();
        var elements = allElements.Skip( 3 ).ToList();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var keyComparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create(
            initialElements,
            new CollectionVariableRootChanges<int, VariableMock>( elements, Array.Empty<int>() ),
            keySelector,
            keyComparer );

        Assertion.All(
                sut.Parent.TestNull(),
                sut.InitialElements.Values.TestSetEqual( initialElements ),
                sut.Elements.Values.TestSetEqual( elements ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                sut.State.TestEquals( VariableState.Changed ),
                sut.KeySelector.TestRefEquals( keySelector ),
                sut.ErrorsValidator.TestType().AssignableTo<PassingValidator<ICollectionVariableRootElements<int, VariableMock>, string>>(),
                sut.WarningsValidator.TestType()
                    .AssignableTo<PassingValidator<ICollectionVariableRootElements<int, VariableMock>, string>>(),
                sut.Elements.KeyComparer.TestRefEquals( keyComparer ) )
            .Go();
    }

    [Fact]
    public void Create_WithoutValidatorsAndWithInitialElementsOnly_ShouldReturnCorrectVariable()
    {
        var initialElements = Fixture.CreateMany<VariableMock>( count: 3 ).ToList();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var keyComparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( initialElements, keySelector, keyComparer );

        Assertion.All(
                sut.Parent.TestNull(),
                sut.InitialElements.Values.TestSetEqual( initialElements ),
                sut.Elements.Values.TestSetEqual( initialElements ),
                sut.Errors.TestEmpty(),
                sut.Warnings.TestEmpty(),
                sut.State.TestEquals( VariableState.Default ),
                sut.KeySelector.TestRefEquals( keySelector ),
                sut.ErrorsValidator.TestType().AssignableTo<PassingValidator<ICollectionVariableRootElements<int, VariableMock>, string>>(),
                sut.WarningsValidator.TestType()
                    .AssignableTo<PassingValidator<ICollectionVariableRootElements<int, VariableMock>, string>>(),
                sut.Elements.KeyComparer.TestRefEquals( keyComparer ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldReturnCorrectVariable()
    {
        var allElements = Fixture.CreateMany<VariableMock>( count: 6 ).ToList();
        var initialElements = allElements.Take( 3 ).ToList();
        var elements = allElements.Skip( 3 ).ToList();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var keyComparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var errorsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();
        var warningsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();
        var sut = CollectionVariableRoot.Create(
            initialElements,
            new CollectionVariableRootChanges<int, VariableMock>( elements, Array.Empty<int>() ),
            keySelector,
            keyComparer,
            errorsValidator,
            warningsValidator );

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
                sut.Elements.KeyComparer.TestRefEquals( keyComparer ) )
            .Go();
    }

    [Fact]
    public void Create_WithInitialElementsOnly_ShouldReturnCorrectVariable()
    {
        var initialElements = Fixture.CreateMany<VariableMock>( count: 3 ).ToList();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var keyComparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var errorsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();
        var warningsValidator = Substitute.For<IValidator<ICollectionVariableRootElements<int, VariableMock>, string>>();
        var sut = CollectionVariableRoot.Create(
            initialElements,
            keySelector,
            keyComparer,
            errorsValidator,
            warningsValidator );

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
                sut.Elements.KeyComparer.TestRefEquals( keyComparer ) )
            .Go();
    }
}
