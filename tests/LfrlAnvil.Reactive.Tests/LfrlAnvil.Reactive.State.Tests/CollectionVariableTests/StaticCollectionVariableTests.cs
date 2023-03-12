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
        var allElements = Fixture.CreateDistinctCollection<int>( count: 6 );
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

        using ( new AssertionScope() )
        {
            sut.Parent.Should().BeNull();
            sut.InitialElements.Values.Should().BeEquivalentTo( initialElements );
            sut.Elements.Values.Should().BeEquivalentTo( elements );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Changed );
            sut.KeySelector.Should().BeSameAs( keySelector );
            sut.ErrorsValidator.Should().BeOfType( typeof( PassingValidator<ICollectionVariableElements<int, int, string>, string> ) );
            sut.WarningsValidator.Should().BeOfType( typeof( PassingValidator<ICollectionVariableElements<int, int, string>, string> ) );
            sut.Elements.ErrorsValidator.Should().BeOfType( typeof( PassingValidator<int, string> ) );
            sut.Elements.WarningsValidator.Should().BeOfType( typeof( PassingValidator<int, string> ) );
            sut.Elements.KeyComparer.Should().BeSameAs( keyComparer );
            sut.Elements.ElementComparer.Should().BeSameAs( elementComparer );
        }
    }

    [Fact]
    public void Create_WithoutValidatorsAndWithInitialElementsOnly_ShouldReturnCorrectVariable()
    {
        var initialElements = Fixture.CreateDistinctCollection<int>( count: 3 );
        var keySelector = Lambda.Of( (int e) => e );
        var keyComparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var elementComparer = EqualityComparerFactory<int>.Create( (a, b) => a == b );
        var sut = CollectionVariable.WithoutValidators<string>.Create( initialElements, keySelector, keyComparer, elementComparer );

        using ( new AssertionScope() )
        {
            sut.Parent.Should().BeNull();
            sut.InitialElements.Values.Should().BeEquivalentTo( initialElements );
            sut.Elements.Values.Should().BeEquivalentTo( initialElements );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Default );
            sut.KeySelector.Should().BeSameAs( keySelector );
            sut.ErrorsValidator.Should().BeOfType( typeof( PassingValidator<ICollectionVariableElements<int, int, string>, string> ) );
            sut.WarningsValidator.Should().BeOfType( typeof( PassingValidator<ICollectionVariableElements<int, int, string>, string> ) );
            sut.Elements.ErrorsValidator.Should().BeOfType( typeof( PassingValidator<int, string> ) );
            sut.Elements.WarningsValidator.Should().BeOfType( typeof( PassingValidator<int, string> ) );
            sut.Elements.KeyComparer.Should().BeSameAs( keyComparer );
            sut.Elements.ElementComparer.Should().BeSameAs( elementComparer );
        }
    }

    [Fact]
    public void Create_ShouldReturnCorrectVariable()
    {
        var allElements = Fixture.CreateDistinctCollection<int>( count: 6 );
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
            sut.Elements.ErrorsValidator.Should().BeSameAs( elementErrorsValidator );
            sut.Elements.WarningsValidator.Should().BeSameAs( elementWarningsValidator );
            sut.Elements.KeyComparer.Should().BeSameAs( keyComparer );
            sut.Elements.ElementComparer.Should().BeSameAs( elementComparer );
        }
    }

    [Fact]
    public void Create_WithInitialElementsOnly_ShouldReturnCorrectVariable()
    {
        var initialElements = Fixture.CreateDistinctCollection<int>( count: 3 );
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
            sut.Elements.ErrorsValidator.Should().BeSameAs( elementErrorsValidator );
            sut.Elements.WarningsValidator.Should().BeSameAs( elementWarningsValidator );
            sut.Elements.KeyComparer.Should().BeSameAs( keyComparer );
            sut.Elements.ElementComparer.Should().BeSameAs( elementComparer );
        }
    }
}
