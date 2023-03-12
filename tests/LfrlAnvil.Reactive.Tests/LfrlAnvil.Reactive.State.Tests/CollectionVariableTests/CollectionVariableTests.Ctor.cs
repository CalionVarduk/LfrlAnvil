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

        using ( new AssertionScope() )
        {
            sut.Parent.Should().BeNull();
            sut.InitialElements.Should().BeEmpty();
            sut.Elements.Should().BeEmpty();
            sut.Elements.Count.Should().Be( 0 );
            sut.Elements.ElementComparer.Should().BeSameAs( elementComparer );
            sut.Elements.KeyComparer.Should().BeSameAs( keyComparer );
            sut.Elements.Keys.Should().BeEmpty();
            sut.Elements.Values.Should().BeEmpty();
            sut.Elements.InvalidElementKeys.Should().BeEmpty();
            sut.Elements.WarningElementKeys.Should().BeEmpty();
            sut.Elements.ModifiedElementKeys.Should().BeEmpty();
            sut.Elements.ErrorsValidator.Should().BeSameAs( elementErrorsValidator );
            sut.Elements.WarningsValidator.Should().BeSameAs( elementWarningsValidator );
            sut.KeySelector.Should().BeSameAs( keySelector );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Default );
            sut.ErrorsValidator.Should().BeSameAs( errorsValidator );
            sut.WarningsValidator.Should().BeSameAs( warningsValidator );

            sut.Elements.ContainsKey( key ).Should().BeFalse();
            sut.Elements.TryGetValue( key, out var tryGetResult ).Should().BeFalse();
            tryGetResult.Should().Be( default( TestElement ) );
            sut.Elements.GetErrors( key ).Should().BeEmpty();
            sut.Elements.GetWarnings( key ).Should().BeEmpty();
            sut.Elements.GetState( key ).Should().Be( CollectionVariableElementState.NotFound );

            ((IReadOnlyDictionary<int, TestElement>)sut.Elements).Keys.Should().BeSameAs( sut.Elements.Keys );
            ((IReadOnlyDictionary<int, TestElement>)sut.Elements).Values.Should().BeSameAs( sut.Elements.Values );

            ((ICollectionVariableElements<int, TestElement>)sut.Elements).GetErrors( key )
                .Should()
                .BeEquivalentTo( sut.Elements.GetErrors( key ) );

            ((ICollectionVariableElements<int, TestElement>)sut.Elements).GetWarnings( key )
                .Should()
                .BeEquivalentTo( sut.Elements.GetWarnings( key ) );

            ((ICollectionVariableElements)sut.Elements).Count.Should().Be( sut.Elements.Count );
            ((ICollectionVariableElements)sut.Elements).Keys.Should().BeSameAs( sut.Elements.Keys );
            ((ICollectionVariableElements)sut.Elements).Values.Should().BeSameAs( sut.Elements.Values );
            ((ICollectionVariableElements)sut.Elements).InvalidElementKeys.Should().BeSameAs( sut.Elements.InvalidElementKeys );
            ((ICollectionVariableElements)sut.Elements).WarningElementKeys.Should().BeSameAs( sut.Elements.WarningElementKeys );
            ((ICollectionVariableElements)sut.Elements).ModifiedElementKeys.Should().BeSameAs( sut.Elements.ModifiedElementKeys );

            ((IReadOnlyCollectionVariable<int, TestElement>)sut).Elements.Should().BeSameAs( sut.Elements );
            ((IReadOnlyCollectionVariable<int, TestElement>)sut).OnChange.Should().BeSameAs( sut.OnChange );
            ((IReadOnlyCollectionVariable)sut).KeyType.Should().Be( typeof( int ) );
            ((IReadOnlyCollectionVariable)sut).ElementType.Should().Be( typeof( TestElement ) );
            ((IReadOnlyCollectionVariable)sut).ValidationResultType.Should().Be( typeof( string ) );
            ((IReadOnlyCollectionVariable)sut).InitialElements.Should().BeSameAs( sut.InitialElements.Values );
            ((IReadOnlyCollectionVariable)sut).Elements.Should().BeSameAs( sut.Elements );
            ((IReadOnlyCollectionVariable)sut).Errors.Should().BeEquivalentTo( sut.Errors );
            ((IReadOnlyCollectionVariable)sut).Warnings.Should().BeEquivalentTo( sut.Warnings );
            ((IReadOnlyCollectionVariable)sut).OnValidate.Should().Be( sut.OnValidate );
            ((IReadOnlyCollectionVariable)sut).OnChange.Should().Be( sut.OnChange );
            ((IVariableNode)sut).OnValidate.Should().Be( sut.OnValidate );
            ((IVariableNode)sut).OnChange.Should().Be( sut.OnChange );
            ((IVariableNode)sut).GetChildren().Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            sut.Parent.Should().BeNull();
            sut.InitialElements.Should().BeEquivalentTo( elements.Select( e => KeyValuePair.Create( e.Key, e ) ) );
            sut.Elements.Should().BeEquivalentTo( elements.Select( e => KeyValuePair.Create( e.Key, e ) ) );
            sut.Elements.Count.Should().Be( elements.Count );
            sut.Elements.ElementComparer.Should().BeSameAs( elementComparer );
            sut.Elements.KeyComparer.Should().BeSameAs( keyComparer );
            sut.Elements.Keys.Should().BeEquivalentTo( elements.Select( e => e.Key ) );
            sut.Elements.Values.Should().BeEquivalentTo( elements );
            sut.Elements.InvalidElementKeys.Should().BeEmpty();
            sut.Elements.WarningElementKeys.Should().BeEmpty();
            sut.Elements.ModifiedElementKeys.Should().BeEmpty();
            sut.Elements.ErrorsValidator.Should().BeSameAs( elementErrorsValidator );
            sut.Elements.WarningsValidator.Should().BeSameAs( elementWarningsValidator );
            sut.KeySelector.Should().BeSameAs( keySelector );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Default );
            sut.ErrorsValidator.Should().BeSameAs( errorsValidator );
            sut.WarningsValidator.Should().BeSameAs( warningsValidator );

            sut.Elements[elements[0].Key].Should().BeSameAs( elements[0] );
            sut.Elements[elements[1].Key].Should().BeSameAs( elements[1] );
            sut.Elements[elements[2].Key].Should().BeSameAs( elements[2] );
            sut.Elements.ContainsKey( elements[0].Key ).Should().BeTrue();
            sut.Elements.ContainsKey( elements[1].Key ).Should().BeTrue();
            sut.Elements.ContainsKey( elements[2].Key ).Should().BeTrue();
            sut.Elements.TryGetValue( elements[0].Key, out var tryGetResult1 ).Should().BeTrue();
            tryGetResult1.Should().BeSameAs( elements[0] );
            sut.Elements.TryGetValue( elements[1].Key, out var tryGetResult2 ).Should().BeTrue();
            tryGetResult2.Should().BeSameAs( elements[1] );
            sut.Elements.TryGetValue( elements[2].Key, out var tryGetResult3 ).Should().BeTrue();
            tryGetResult3.Should().BeSameAs( elements[2] );
            sut.Elements.GetErrors( elements[0].Key ).Should().BeEmpty();
            sut.Elements.GetErrors( elements[1].Key ).Should().BeEmpty();
            sut.Elements.GetErrors( elements[2].Key ).Should().BeEmpty();
            sut.Elements.GetWarnings( elements[0].Key ).Should().BeEmpty();
            sut.Elements.GetWarnings( elements[1].Key ).Should().BeEmpty();
            sut.Elements.GetWarnings( elements[2].Key ).Should().BeEmpty();
            sut.Elements.GetState( elements[0].Key ).Should().Be( CollectionVariableElementState.Default );
            sut.Elements.GetState( elements[1].Key ).Should().Be( CollectionVariableElementState.Default );
            sut.Elements.GetState( elements[2].Key ).Should().Be( CollectionVariableElementState.Default );
        }
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

        using ( new AssertionScope() )
        {
            sut.Parent.Should().BeNull();
            sut.InitialElements.Should().BeEquivalentTo( KeyValuePair.Create( element.Key, element ) );
            sut.Elements.Should().BeEquivalentTo( KeyValuePair.Create( element.Key, element ) );
            sut.Elements.Count.Should().Be( 1 );
            sut.Elements.ElementComparer.Should().BeSameAs( elementComparer );
            sut.Elements.KeyComparer.Should().BeSameAs( keyComparer );
            sut.Elements.Keys.Should().BeEquivalentTo( element.Key );
            sut.Elements.Values.Should().BeEquivalentTo( element );
            sut.Elements.InvalidElementKeys.Should().BeEmpty();
            sut.Elements.WarningElementKeys.Should().BeEmpty();
            sut.Elements.ModifiedElementKeys.Should().BeEquivalentTo( element.Key );
            sut.Elements.ErrorsValidator.Should().BeSameAs( elementErrorsValidator );
            sut.Elements.WarningsValidator.Should().BeSameAs( elementWarningsValidator );
            sut.KeySelector.Should().BeSameAs( keySelector );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Changed );
            sut.ErrorsValidator.Should().BeSameAs( errorsValidator );
            sut.WarningsValidator.Should().BeSameAs( warningsValidator );
            sut.Elements[element.Key].Should().BeSameAs( element );
            sut.Elements.ContainsKey( element.Key ).Should().BeTrue();
            sut.Elements.TryGetValue( element.Key, out var tryGetResult ).Should().BeTrue();
            tryGetResult.Should().BeSameAs( element );
            sut.Elements.GetErrors( element.Key ).Should().BeEmpty();
            sut.Elements.GetWarnings( element.Key ).Should().BeEmpty();
            sut.Elements.GetState( element.Key ).Should().Be( CollectionVariableElementState.Changed );
        }
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

        using ( new AssertionScope() )
        {
            sut.Parent.Should().BeNull();
            sut.InitialElements.Should().BeEquivalentTo( initialElements.Select( e => KeyValuePair.Create( e.Key, e ) ) );
            sut.Elements.Should().BeEquivalentTo( elements.Select( e => KeyValuePair.Create( e.Key, e ) ) );
            sut.Elements.Count.Should().Be( elements.Length );
            sut.Elements.ElementComparer.Should().BeSameAs( elementComparer );
            sut.Elements.KeyComparer.Should().BeSameAs( keyComparer );
            sut.Elements.Keys.Should().BeEquivalentTo( elements.Select( e => e.Key ) );
            sut.Elements.Values.Should().BeEquivalentTo( elements.AsEnumerable() );
            sut.Elements.InvalidElementKeys.Should().BeEmpty();
            sut.Elements.WarningElementKeys.Should().BeEmpty();
            sut.Elements.ModifiedElementKeys.Should().BeEquivalentTo( elements[0].Key, elements[2].Key, initialElements[2].Key );
            sut.Elements.ErrorsValidator.Should().BeSameAs( elementErrorsValidator );
            sut.Elements.WarningsValidator.Should().BeSameAs( elementWarningsValidator );
            sut.KeySelector.Should().BeSameAs( keySelector );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Changed );
            sut.ErrorsValidator.Should().BeSameAs( errorsValidator );
            sut.WarningsValidator.Should().BeSameAs( warningsValidator );

            sut.Elements[elements[0].Key].Should().BeSameAs( elements[0] );
            sut.Elements[elements[1].Key].Should().BeSameAs( elements[1] );
            sut.Elements[elements[2].Key].Should().BeSameAs( elements[2] );
            sut.Elements.ContainsKey( elements[0].Key ).Should().BeTrue();
            sut.Elements.ContainsKey( elements[1].Key ).Should().BeTrue();
            sut.Elements.ContainsKey( elements[2].Key ).Should().BeTrue();
            sut.Elements.ContainsKey( initialElements[2].Key ).Should().BeFalse();
            sut.Elements.TryGetValue( elements[0].Key, out var tryGetResult1 ).Should().BeTrue();
            tryGetResult1.Should().BeSameAs( elements[0] );
            sut.Elements.TryGetValue( elements[1].Key, out var tryGetResult2 ).Should().BeTrue();
            tryGetResult2.Should().BeSameAs( elements[1] );
            sut.Elements.TryGetValue( elements[2].Key, out var tryGetResult3 ).Should().BeTrue();
            tryGetResult3.Should().BeSameAs( elements[2] );
            sut.Elements.TryGetValue( initialElements[2].Key, out var tryGetResult4 ).Should().BeFalse();
            tryGetResult4.Should().Be( default( TestElement ) );
            sut.Elements.GetErrors( elements[0].Key ).Should().BeEmpty();
            sut.Elements.GetErrors( elements[1].Key ).Should().BeEmpty();
            sut.Elements.GetErrors( elements[2].Key ).Should().BeEmpty();
            sut.Elements.GetErrors( initialElements[2].Key ).Should().BeEmpty();
            sut.Elements.GetWarnings( elements[0].Key ).Should().BeEmpty();
            sut.Elements.GetWarnings( elements[1].Key ).Should().BeEmpty();
            sut.Elements.GetWarnings( elements[2].Key ).Should().BeEmpty();
            sut.Elements.GetWarnings( initialElements[2].Key ).Should().BeEmpty();
            sut.Elements.GetState( elements[0].Key ).Should().Be( CollectionVariableElementState.Changed );
            sut.Elements.GetState( elements[1].Key ).Should().Be( CollectionVariableElementState.Default );
            sut.Elements.GetState( elements[2].Key ).Should().Be( CollectionVariableElementState.Added );
            sut.Elements.GetState( initialElements[2].Key ).Should().Be( CollectionVariableElementState.Removed );
        }
    }

    [Fact]
    public void Ctor_WithElements_ShouldReturnCorrectResult_WithDefaultParameters()
    {
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = new CollectionVariable<int, TestElement, string>( Array.Empty<TestElement>(), Array.Empty<TestElement>(), keySelector );

        using ( new AssertionScope() )
        {
            sut.Parent.Should().BeNull();
            sut.Elements.ElementComparer.Should().BeSameAs( EqualityComparer<TestElement>.Default );
            sut.Elements.KeyComparer.Should().BeSameAs( EqualityComparer<int>.Default );
            sut.Elements.ErrorsValidator.Should().BeOfType( typeof( PassingValidator<TestElement, string> ) );
            sut.Elements.WarningsValidator.Should().BeOfType( typeof( PassingValidator<TestElement, string> ) );

            sut.KeySelector.Should().BeSameAs( keySelector );
            sut.ErrorsValidator.Should()
                .BeOfType( typeof( PassingValidator<ICollectionVariableElements<int, TestElement, string>, string> ) );

            sut.WarningsValidator.Should()
                .BeOfType( typeof( PassingValidator<ICollectionVariableElements<int, TestElement, string>, string> ) );
        }
    }

    [Fact]
    public void Ctor_ShouldReturnCorrectResult_WithDefaultParameters()
    {
        var keySelector = Lambda.Of( (TestElement e) => e.Key );
        var sut = new CollectionVariable<int, TestElement, string>( Array.Empty<TestElement>(), keySelector );

        using ( new AssertionScope() )
        {
            sut.Parent.Should().BeNull();
            sut.Elements.ElementComparer.Should().BeSameAs( EqualityComparer<TestElement>.Default );
            sut.Elements.KeyComparer.Should().BeSameAs( EqualityComparer<int>.Default );
            sut.Elements.ErrorsValidator.Should().BeOfType( typeof( PassingValidator<TestElement, string> ) );
            sut.Elements.WarningsValidator.Should().BeOfType( typeof( PassingValidator<TestElement, string> ) );

            sut.KeySelector.Should().BeSameAs( keySelector );
            sut.ErrorsValidator.Should()
                .BeOfType( typeof( PassingValidator<ICollectionVariableElements<int, TestElement, string>, string> ) );

            sut.WarningsValidator.Should()
                .BeOfType( typeof( PassingValidator<ICollectionVariableElements<int, TestElement, string>, string> ) );
        }
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

        using ( new AssertionScope() )
        {
            sut.Errors.Should().BeEmpty();
            sut.Elements.GetErrors( element.Key ).Should().BeEmpty();
            sut.Elements.InvalidElementKeys.Should().BeEmpty();
            sut.Elements.GetState( element.Key ).Should().Be( CollectionVariableElementState.Default );
        }
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

        using ( new AssertionScope() )
        {
            sut.Warnings.Should().BeEmpty();
            sut.Elements.GetWarnings( element.Key ).Should().BeEmpty();
            sut.Elements.WarningElementKeys.Should().BeEmpty();
            sut.Elements.GetState( element.Key ).Should().Be( CollectionVariableElementState.Default );
        }
    }
}
