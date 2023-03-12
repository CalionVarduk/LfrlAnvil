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

        using ( new AssertionScope() )
        {
            sut.Parent.Should().BeNull();
            sut.InitialElements.Should().BeEmpty();
            sut.Elements.Should().BeEmpty();
            sut.Elements.Count.Should().Be( 0 );
            sut.Elements.KeyComparer.Should().BeSameAs( keyComparer );
            sut.Elements.Keys.Should().BeEmpty();
            sut.Elements.Values.Should().BeEmpty();
            sut.Elements.InvalidElementKeys.Should().BeEmpty();
            sut.Elements.WarningElementKeys.Should().BeEmpty();
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEmpty();
            sut.Elements.ChangedElementKeys.Should().BeEmpty();
            sut.KeySelector.Should().BeSameAs( keySelector );
            sut.Errors.Should().BeEmpty();
            sut.Warnings.Should().BeEmpty();
            sut.State.Should().Be( VariableState.Default );
            sut.ErrorsValidator.Should().BeSameAs( errorsValidator );
            sut.WarningsValidator.Should().BeSameAs( warningsValidator );

            sut.Elements.ContainsKey( key ).Should().BeFalse();
            sut.Elements.TryGetValue( key, out var tryGetResult ).Should().BeFalse();
            tryGetResult.Should().Be( default( VariableMock ) );

            ((IReadOnlyDictionary<int, VariableMock>)sut.Elements).Keys.Should().BeSameAs( sut.Elements.Keys );
            ((IReadOnlyDictionary<int, VariableMock>)sut.Elements).Values.Should().BeSameAs( sut.Elements.Values );
            ((ICollectionVariableRootElements)sut.Elements).Count.Should().Be( sut.Elements.Count );
            ((ICollectionVariableRootElements)sut.Elements).Keys.Should().BeSameAs( sut.Elements.Keys );
            ((ICollectionVariableRootElements)sut.Elements).Values.Should().BeSameAs( sut.Elements.Values );
            ((ICollectionVariableRootElements)sut.Elements).InvalidElementKeys.Should().BeSameAs( sut.Elements.InvalidElementKeys );
            ((ICollectionVariableRootElements)sut.Elements).WarningElementKeys.Should().BeSameAs( sut.Elements.WarningElementKeys );
            ((ICollectionVariableRootElements)sut.Elements).AddedElementKeys.Should().BeSameAs( sut.Elements.AddedElementKeys );
            ((ICollectionVariableRootElements)sut.Elements).RemovedElementKeys.Should().BeSameAs( sut.Elements.RemovedElementKeys );
            ((ICollectionVariableRootElements)sut.Elements).ChangedElementKeys.Should().BeSameAs( sut.Elements.ChangedElementKeys );

            ((IReadOnlyCollectionVariableRoot<int, VariableMock>)sut).Elements.Should().BeSameAs( sut.Elements );
            ((IReadOnlyCollectionVariableRoot<int, VariableMock>)sut).OnChange.Should().BeSameAs( sut.OnChange );
            ((IReadOnlyCollectionVariableRoot)sut).KeyType.Should().Be( typeof( int ) );
            ((IReadOnlyCollectionVariableRoot)sut).ElementType.Should().Be( typeof( VariableMock ) );
            ((IReadOnlyCollectionVariableRoot)sut).ValidationResultType.Should().Be( typeof( string ) );
            ((IReadOnlyCollectionVariableRoot)sut).InitialElements.Should().BeSameAs( sut.InitialElements.Values );
            ((IReadOnlyCollectionVariableRoot)sut).Elements.Should().BeSameAs( sut.Elements );
            ((IReadOnlyCollectionVariableRoot)sut).Errors.Should().BeEquivalentTo( sut.Errors );
            ((IReadOnlyCollectionVariableRoot)sut).Warnings.Should().BeEquivalentTo( sut.Warnings );
            ((IReadOnlyCollectionVariableRoot)sut).OnValidate.Should().Be( sut.OnValidate );
            ((IReadOnlyCollectionVariableRoot)sut).OnChange.Should().Be( sut.OnChange );
            ((IVariableNode)sut).OnValidate.Should().Be( sut.OnValidate );
            ((IVariableNode)sut).OnChange.Should().Be( sut.OnChange );
            ((IVariableNode)sut).GetChildren().Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            sut.Parent.Should().BeNull();
            sut.InitialElements.Should().BeEquivalentTo( elements.Select( e => KeyValuePair.Create( e.Key, e ) ) );
            sut.Elements.Should().BeEquivalentTo( elements.Select( e => KeyValuePair.Create( e.Key, e ) ) );
            sut.Elements.Count.Should().Be( elements.Count );
            sut.Elements.KeyComparer.Should().BeSameAs( keyComparer );
            sut.Elements.Keys.Should().BeEquivalentTo( elements.Select( e => e.Key ) );
            sut.Elements.Values.Should().BeEquivalentTo( elements );
            sut.Elements.InvalidElementKeys.Should().BeEmpty();
            sut.Elements.WarningElementKeys.Should().BeEmpty();
            sut.Elements.AddedElementKeys.Should().BeEmpty();
            sut.Elements.RemovedElementKeys.Should().BeEmpty();
            sut.Elements.ChangedElementKeys.Should().BeEmpty();
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

            elements.Should().OnlyContain( e => e.Parent == sut );

            ((IVariableNode)sut).GetChildren().Should().BeEquivalentTo( elements );
        }
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

        using ( new AssertionScope() )
        {
            sut.Parent.Should().BeNull();
            sut.InitialElements.Should().BeEquivalentTo( initialElements.Select( e => KeyValuePair.Create( e.Key, e ) ) );
            sut.Elements.Should().BeEquivalentTo( elements.Select( e => KeyValuePair.Create( e.Key, e ) ) );
            sut.Elements.Count.Should().Be( elements.Length );
            sut.Elements.KeyComparer.Should().BeSameAs( keyComparer );
            sut.Elements.Keys.Should().BeEquivalentTo( elements.Select( e => e.Key ) );
            sut.Elements.Values.Should().BeEquivalentTo( elements.AsEnumerable() );
            sut.Elements.InvalidElementKeys.Should().BeEmpty();
            sut.Elements.WarningElementKeys.Should().BeEmpty();
            sut.Elements.AddedElementKeys.Should().BeEquivalentTo( allElements[3].Key );
            sut.Elements.RemovedElementKeys.Should().BeEquivalentTo( allElements[2].Key );
            sut.Elements.ChangedElementKeys.Should().BeEquivalentTo( allElements[0].Key );
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
            tryGetResult4.Should().Be( default( VariableMock ) );

            allElements.Should().OnlyContain( e => e.Parent == sut );

            ((IVariableNode)sut).GetChildren().Should().BeEquivalentTo( allElements );
        }
    }

    [Fact]
    public void Ctor_WithElements_ShouldReturnCorrectResult_WithDefaultParameters()
    {
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = new CollectionVariableRoot<int, VariableMock, string>(
            Array.Empty<VariableMock>(),
            new CollectionVariableRootChanges<int, VariableMock>(),
            keySelector );

        using ( new AssertionScope() )
        {
            sut.Parent.Should().BeNull();
            sut.Elements.KeyComparer.Should().BeSameAs( EqualityComparer<int>.Default );

            sut.KeySelector.Should().BeSameAs( keySelector );
            sut.ErrorsValidator.Should().BeOfType( typeof( PassingValidator<ICollectionVariableRootElements<int, VariableMock>, string> ) );
            sut.WarningsValidator.Should()
                .BeOfType( typeof( PassingValidator<ICollectionVariableRootElements<int, VariableMock>, string> ) );
        }
    }

    [Fact]
    public void Ctor_ShouldReturnCorrectResult_WithDefaultParameters()
    {
        var keySelector = Lambda.Of( (VariableMock e) => e.Key );
        var sut = new CollectionVariableRoot<int, VariableMock, string>( Array.Empty<VariableMock>(), keySelector );

        using ( new AssertionScope() )
        {
            sut.Parent.Should().BeNull();
            sut.Elements.KeyComparer.Should().BeSameAs( EqualityComparer<int>.Default );

            sut.KeySelector.Should().BeSameAs( keySelector );
            sut.ErrorsValidator.Should().BeOfType( typeof( PassingValidator<ICollectionVariableRootElements<int, VariableMock>, string> ) );
            sut.WarningsValidator.Should()
                .BeOfType( typeof( PassingValidator<ICollectionVariableRootElements<int, VariableMock>, string> ) );
        }
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

        sut.Errors.Should().BeEmpty();
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

        sut.Warnings.Should().BeEmpty();
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

        using ( new AssertionScope() )
        {
            sut.Elements.InvalidElementKeys.Should().BeEquivalentTo( element.Key );
            sut.State.Should().Be( VariableState.Invalid );
        }
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

        using ( new AssertionScope() )
        {
            sut.Elements.WarningElementKeys.Should().BeEquivalentTo( element.Key );
            sut.State.Should().Be( VariableState.Warning );
        }
    }

    [Fact]
    public void Ctor_ShouldCreateCorrectResult_WhenOneOfTheElementsIsDisposed()
    {
        var element = Fixture.Create<VariableMock>();
        element.Dispose();

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );

        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );

        using ( new AssertionScope() )
        {
            sut.InitialElements.Should().BeEmpty();
            sut.Elements.Should().BeEmpty();
        }
    }

    [Fact]
    public void Ctor_ShouldCreateCorrectResult_WhenOneOfTheElementsHasAssignedParent()
    {
        var root = new VariableRootMock();
        var element = Fixture.Create<VariableMock>();
        root.ExposedRegisterNode( Fixture.Create<string>(), element );

        var keySelector = Lambda.Of( (VariableMock e) => e.Key );

        var sut = CollectionVariableRoot.WithoutValidators<string>.Create( new[] { element }, keySelector );

        using ( new AssertionScope() )
        {
            sut.InitialElements.Should().BeEmpty();
            sut.Elements.Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            sut.InitialElements.Values.Should().BeEquivalentTo( element );
            sut.Elements.Values.Should().BeEquivalentTo( element );
        }
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

        using ( new AssertionScope() )
        {
            sut.InitialElements.Values.Should().BeEquivalentTo( element );
            sut.Elements.Values.Should().BeEquivalentTo( element );
        }
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

        using ( new AssertionScope() )
        {
            sut.InitialElements.Values.Should().BeEquivalentTo( element );
            sut.Elements.Values.Should().BeEquivalentTo( element );
        }
    }
}
