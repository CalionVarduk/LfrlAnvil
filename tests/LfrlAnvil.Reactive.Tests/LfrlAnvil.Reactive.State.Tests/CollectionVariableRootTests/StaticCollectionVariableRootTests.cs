using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Validation;
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Reactive.State.Tests.CollectionVariableRootTests;

public class StaticCollectionVariableRootTests : TestsBase
{
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

        using ( new AssertionScope() )
        {
            sut.Parent.Should().BeNull();
            sut.InitialElements.Values.Should().BeEquivalentTo( initialElements );
            sut.Elements.Values.Should().BeEquivalentTo( elements );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Changed );
            sut.KeySelector.Should().BeSameAs( keySelector );
            sut.ErrorsValidator.Should().BeOfType( typeof( PassingValidator<ICollectionVariableRootElements<int, VariableMock>, string> ) );
            sut.WarningsValidator.Should()
                .BeOfType( typeof( PassingValidator<ICollectionVariableRootElements<int, VariableMock>, string> ) );

            sut.Elements.KeyComparer.Should().BeSameAs( keyComparer );
        }
    }

    [Fact]
    public void Create_WithoutValidatorsAndWithInitialElementsOnly_ShouldReturnCorrectVariable()
    {
        var initialElements = Fixture.CreateMany<VariableMock>( count: 3 ).ToList();
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var keyComparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( initialElements, keySelector, keyComparer );

        using ( new AssertionScope() )
        {
            sut.Parent.Should().BeNull();
            sut.InitialElements.Values.Should().BeEquivalentTo( initialElements );
            sut.Elements.Values.Should().BeEquivalentTo( initialElements );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Default );
            sut.KeySelector.Should().BeSameAs( keySelector );
            sut.ErrorsValidator.Should().BeOfType( typeof( PassingValidator<ICollectionVariableRootElements<int, VariableMock>, string> ) );
            sut.WarningsValidator.Should()
                .BeOfType( typeof( PassingValidator<ICollectionVariableRootElements<int, VariableMock>, string> ) );

            sut.Elements.KeyComparer.Should().BeSameAs( keyComparer );
        }
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

        using ( new AssertionScope() )
        {
            sut.Parent.Should().BeNull();
            sut.InitialElements.Values.Should().BeEquivalentTo( initialElements );
            sut.Elements.Values.Should().BeEquivalentTo( elements );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Changed );
            sut.KeySelector.Should().BeSameAs( keySelector );
            sut.ErrorsValidator.Should().BeSameAs( errorsValidator );
            sut.WarningsValidator.Should().BeSameAs( warningsValidator );
            sut.Elements.KeyComparer.Should().BeSameAs( keyComparer );
        }
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

        using ( new AssertionScope() )
        {
            sut.Parent.Should().BeNull();
            sut.InitialElements.Values.Should().BeEquivalentTo( initialElements );
            sut.Elements.Values.Should().BeEquivalentTo( initialElements );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Default );
            sut.KeySelector.Should().BeSameAs( keySelector );
            sut.ErrorsValidator.Should().BeSameAs( errorsValidator );
            sut.WarningsValidator.Should().BeSameAs( warningsValidator );
            sut.Elements.KeyComparer.Should().BeSameAs( keyComparer );
        }
    }
}
