using System.Linq;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Functional.Tests.MutationTests;

[GenericTestClass( typeof( GenericMutationTestsData<> ) )]
public abstract class GenericMutationTests<T> : TestsBase
    where T : notnull
{
    [Fact]
    public void Empty_ShouldHaveDefaultValues()
    {
        var sut = Mutation<T>.Empty;

        Assertion.All(
                sut.OldValue.TestEquals( default ),
                sut.Value.TestEquals( default ),
                sut.HasChanged.TestFalse() )
            .Go();
    }

    [Fact]
    public void Create_ShouldCreateCorrectResult_WhenValuesAreDifferent()
    {
        var (oldValue, value) = Fixture.CreateManyDistinct<T>( count: 2 );

        var sut = Mutation.Create( oldValue, value );

        Assertion.All(
                sut.OldValue.TestEquals( oldValue ),
                sut.Value.TestEquals( value ),
                sut.HasChanged.TestTrue() )
            .Go();
    }

    [Fact]
    public void Create_ShouldCreateCorrectResult_WhenValuesAreEqual()
    {
        var value = Fixture.Create<T>();

        var sut = Mutation.Create( value, value );

        Assertion.All(
                sut.OldValue.TestEquals( value ),
                sut.Value.TestEquals( value ),
                sut.HasChanged.TestFalse() )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldCreateCorrectResult_WhenValuesAreDifferent()
    {
        var (oldValue, value) = Fixture.CreateManyDistinct<T>( count: 2 );

        var sut = new Mutation<T>( oldValue, value );

        Assertion.All(
                sut.OldValue.TestEquals( oldValue ),
                sut.Value.TestEquals( value ),
                sut.HasChanged.TestTrue() )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldCreateCorrectResult_WhenValuesAreEqual()
    {
        var value = Fixture.Create<T>();

        var sut = new Mutation<T>( value, value );

        Assertion.All(
                sut.OldValue.TestEquals( value ),
                sut.Value.TestEquals( value ),
                sut.HasChanged.TestFalse() )
            .Go();
    }

    [Fact]
    public void GetHashCode_ShouldCreateCorrectResult()
    {
        var (oldValue, value) = Fixture.CreateMany<T>( count: 2 ).ToList();
        var expected = Hash.Default.Add( oldValue ).Add( value ).Value;
        var sut = new Mutation<T>( oldValue, value );

        var result = sut.GetHashCode();

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMutationTestsData<T>.CreateEqualsTestData ) )]
    public void Equals_ShouldReturnCorrectResult(T oldValue1, T value1, T oldValue2, T value2, bool expected)
    {
        var a = new Mutation<T>( oldValue1, value1 );
        var b = new Mutation<T>( oldValue2, value2 );

        var result = a.Equals( b );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Mutate_ShouldReturnCorrectResult()
    {
        var (oldValue, value, newValue) = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = new Mutation<T>( oldValue, value );

        var result = sut.Mutate( newValue );

        Assertion.All(
                result.OldValue.TestEquals( value ),
                result.Value.TestEquals( newValue ) )
            .Go();
    }

    [Fact]
    public void Replace_ShouldReturnCorrectResult()
    {
        var (oldValue, value, newValue) = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = new Mutation<T>( oldValue, value );

        var result = sut.Replace( newValue );

        Assertion.All(
                result.OldValue.TestEquals( oldValue ),
                result.Value.TestEquals( newValue ) )
            .Go();
    }

    [Fact]
    public void Revert_ShouldReturnCorrectResult()
    {
        var (oldValue, value) = Fixture.CreateManyDistinct<T>( count: 2 );
        var sut = new Mutation<T>( oldValue, value );

        var result = sut.Revert();

        Assertion.All(
                result.OldValue.TestEquals( oldValue ),
                result.Value.TestEquals( oldValue ) )
            .Go();
    }

    [Fact]
    public void Swap_ShouldReturnCorrectResult()
    {
        var (oldValue, value) = Fixture.CreateManyDistinct<T>( count: 2 );
        var sut = new Mutation<T>( oldValue, value );

        var result = sut.Swap();

        Assertion.All(
                result.OldValue.TestEquals( value ),
                result.Value.TestEquals( oldValue ) )
            .Go();
    }

    [Fact]
    public void Bind_ShouldCallChangedDelegate_WhenValueHasChanged()
    {
        var (oldValue, value) = Fixture.CreateManyDistinct<T>( count: 2 );
        var (returnedOldValue, returnedValue) = Fixture.CreateManyDistinct<T>( count: 2 );
        var changedDelegate = Substitute.For<Func<(T, T), Mutation<T>>>()
            .WithAnyArgs( _ => new Mutation<T>( returnedOldValue, returnedValue ) );

        var sut = new Mutation<T>( oldValue, value );

        var result = sut.Bind( changedDelegate );

        Assertion.All(
                result.OldValue.TestEquals( returnedOldValue ),
                result.Value.TestEquals( returnedValue ),
                changedDelegate.CallAt( 0 ).Exists.TestTrue(),
                changedDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( (oldValue, value) ) )
            .Go();
    }

    [Fact]
    public void Bind_ShouldNotCallChangedDelegateAndReturnEmpty_WhenValuesHasNotChanged()
    {
        var value = Fixture.Create<T>();
        var changedDelegate = Substitute.For<Func<(T, T), Mutation<T>>>()
            .WithAnyArgs( i => new Mutation<T>( i.ArgAt<(T, T)>( 0 ).Item1, i.ArgAt<(T, T)>( 0 ).Item2 ) );

        var sut = new Mutation<T>( value, value );

        var result = sut.Bind( changedDelegate );

        Assertion.All(
                result.OldValue.TestEquals( default ),
                result.Value.TestEquals( default ),
                changedDelegate.CallCount().TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void Bind_WithUnchanged_ShouldCallChangedDelegate_WhenValueHasChanged()
    {
        var (oldValue, value) = Fixture.CreateManyDistinct<T>( count: 2 );
        var (returnedOldValue, returnedValue) = Fixture.CreateManyDistinct<T>( count: 2 );

        var changedDelegate = Substitute.For<Func<(T, T), Mutation<T>>>()
            .WithAnyArgs( _ => new Mutation<T>( returnedOldValue, returnedValue ) );

        var unchangedDelegate = Substitute.For<Func<T, Mutation<T>>>()
            .WithAnyArgs( i => new Mutation<T>( i.ArgAt<T>( 0 ), i.ArgAt<T>( 0 ) ) );

        var sut = new Mutation<T>( oldValue, value );

        var result = sut.Bind( changedDelegate, unchangedDelegate );

        Assertion.All(
                result.OldValue.TestEquals( returnedOldValue ),
                result.Value.TestEquals( returnedValue ),
                changedDelegate.CallAt( 0 ).Exists.TestTrue(),
                changedDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( (oldValue, value) ),
                unchangedDelegate.CallCount().TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void Bind_WithUnchanged_ShouldCallUnchangedDelegate_WhenValueHasNotChanged()
    {
        var value = Fixture.Create<T>();
        var (returnedOldValue, returnedValue) = Fixture.CreateManyDistinct<T>( count: 2 );

        var changedDelegate = Substitute.For<Func<(T, T), Mutation<T>>>()
            .WithAnyArgs( i => new Mutation<T>( i.ArgAt<(T, T)>( 0 ).Item1, i.ArgAt<(T, T)>( 0 ).Item2 ) );

        var unchangedDelegate = Substitute.For<Func<T, Mutation<T>>>()
            .WithAnyArgs( _ => new Mutation<T>( returnedOldValue, returnedValue ) );

        var sut = new Mutation<T>( value, value );

        var result = sut.Bind( changedDelegate, unchangedDelegate );

        Assertion.All(
                result.OldValue.TestEquals( returnedOldValue ),
                result.Value.TestEquals( returnedValue ),
                changedDelegate.CallCount().TestEquals( 0 ),
                unchangedDelegate.CallAt( 0 ).Exists.TestTrue(),
                unchangedDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void Match_ShouldCallChangedDelegate_WhenValueHasChanged()
    {
        var (oldValue, value) = Fixture.CreateManyDistinct<T>( count: 2 );
        var returnedValue = Fixture.Create<T>();
        var changedDelegate = Substitute.For<Func<(T, T), T>>().WithAnyArgs( _ => returnedValue );
        var unchangedDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );

        var sut = new Mutation<T>( oldValue, value );

        var result = sut.Match( changedDelegate, unchangedDelegate );

        Assertion.All(
                result.TestEquals( returnedValue ),
                changedDelegate.CallAt( 0 ).Exists.TestTrue(),
                changedDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( (oldValue, value) ),
                unchangedDelegate.CallCount().TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void Match_ShouldCallUnchangedDelegate_WhenValueHasNotChanged()
    {
        var value = Fixture.Create<T>();
        var returnedValue = Fixture.Create<T>();
        var changedDelegate = Substitute.For<Func<(T, T), T>>().WithAnyArgs( i => i.ArgAt<(T, T)>( 0 ).Item1 );
        var unchangedDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( _ => returnedValue );

        var sut = new Mutation<T>( value, value );

        var result = sut.Match( changedDelegate, unchangedDelegate );

        Assertion.All(
                result.TestEquals( returnedValue ),
                changedDelegate.CallCount().TestEquals( 0 ),
                unchangedDelegate.CallAt( 0 ).Exists.TestTrue(),
                unchangedDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void Match_WithAction_ShouldCallChangedDelegate_WhenValueHasChanged()
    {
        var (oldValue, value) = Fixture.CreateManyDistinct<T>( count: 2 );
        var changedDelegate = Substitute.For<Action<(T, T)>>();
        var unchangedDelegate = Substitute.For<Action<T>>();

        var sut = new Mutation<T>( oldValue, value );

        sut.Match( changedDelegate, unchangedDelegate );

        Assertion.All(
                changedDelegate.CallAt( 0 ).Exists.TestTrue(),
                changedDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( (oldValue, value) ),
                unchangedDelegate.CallCount().TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void Match_WithAction_ShouldCallUnchangedDelegate_WhenValueHasNotChanged()
    {
        var value = Fixture.Create<T>();
        var changedDelegate = Substitute.For<Action<(T, T)>>();
        var unchangedDelegate = Substitute.For<Action<T>>();

        var sut = new Mutation<T>( value, value );

        sut.Match( changedDelegate, unchangedDelegate );

        Assertion.All(
                changedDelegate.CallCount().TestEquals( 0 ),
                unchangedDelegate.CallAt( 0 ).Exists.TestTrue(),
                unchangedDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void IfChanged_ShouldCallChangedDelegate_WhenValueHasChanged()
    {
        var (oldValue, value) = Fixture.CreateManyDistinct<T>( count: 2 );
        var returnedValue = Fixture.Create<T>();
        var changedDelegate = Substitute.For<Func<(T, T), T>>().WithAnyArgs( _ => returnedValue );

        var sut = new Mutation<T>( oldValue, value );

        var result = sut.IfChanged( changedDelegate );

        Assertion.All(
                result.Value.TestEquals( returnedValue ),
                changedDelegate.CallAt( 0 ).Exists.TestTrue(),
                changedDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( (oldValue, value) ) )
            .Go();
    }

    [Fact]
    public void IfChanged_ShouldReturnNone_WhenValueHasNotChanged()
    {
        var value = Fixture.Create<T>();
        var changedDelegate = Substitute.For<Func<(T, T), T>>().WithAnyArgs( i => i.ArgAt<(T, T)>( 0 ).Item1 );

        var sut = new Mutation<T>( value, value );

        var result = sut.IfChanged( changedDelegate );

        Assertion.All(
                result.HasValue.TestFalse(),
                changedDelegate.CallCount().TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void IfChanged_WithAction_ShouldCallChangedDelegate_WhenValueHasChanged()
    {
        var (oldValue, value) = Fixture.CreateManyDistinct<T>( count: 2 );
        var changedDelegate = Substitute.For<Action<(T, T)>>();

        var sut = new Mutation<T>( oldValue, value );

        sut.IfChanged( changedDelegate );

        Assertion.All(
                changedDelegate.CallAt( 0 ).Exists.TestTrue(),
                changedDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( (oldValue, value) ) )
            .Go();
    }

    [Fact]
    public void IfChanged_WithAction_ShouldDoNothing_WhenValueHasNotChanged()
    {
        var value = Fixture.Create<T>();
        var changedDelegate = Substitute.For<Action<(T, T)>>();

        var sut = new Mutation<T>( value, value );

        sut.IfChanged( changedDelegate );

        changedDelegate.CallCount().TestEquals( 0 ).Go();
    }

    [Fact]
    public void IfChangedOrDefault_ShouldCallChangedDelegate_WhenValueHasChanged()
    {
        var (oldValue, value) = Fixture.CreateManyDistinct<T>( count: 2 );
        var returnedValue = Fixture.Create<T>();
        var changedDelegate = Substitute.For<Func<(T, T), T>>().WithAnyArgs( _ => returnedValue );

        var sut = new Mutation<T>( oldValue, value );

        var result = sut.IfChangedOrDefault( changedDelegate );

        Assertion.All(
                result.TestEquals( returnedValue ),
                changedDelegate.CallAt( 0 ).Exists.TestTrue(),
                changedDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( (oldValue, value) ) )
            .Go();
    }

    [Fact]
    public void IfChangedOrDefault_ShouldReturnDefault_WhenValueHasNotChanged()
    {
        var value = Fixture.Create<T>();
        var changedDelegate = Substitute.For<Func<(T, T), T>>().WithAnyArgs( i => i.ArgAt<(T, T)>( 0 ).Item1 );

        var sut = new Mutation<T>( value, value );

        var result = sut.IfChangedOrDefault( changedDelegate );

        Assertion.All(
                result.TestEquals( default ),
                changedDelegate.CallCount().TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void IfChangedOrDefault_WithValue_ShouldCallChangedDelegate_WhenValueHasChanged()
    {
        var (oldValue, value) = Fixture.CreateManyDistinct<T>( count: 2 );
        var returnedValue = Fixture.Create<T>();
        var changedDelegate = Substitute.For<Func<(T, T), T>>().WithAnyArgs( _ => returnedValue );

        var sut = new Mutation<T>( oldValue, value );

        var result = sut.IfChangedOrDefault( changedDelegate, Fixture.CreateNotDefault<T>() );

        Assertion.All(
                result.TestEquals( returnedValue ),
                changedDelegate.CallAt( 0 ).Exists.TestTrue(),
                changedDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( (oldValue, value) ) )
            .Go();
    }

    [Fact]
    public void IfChangedOrDefault_WithValue_ShouldReturnDefault_WhenValueHasNotChanged()
    {
        var defaultValue = Fixture.CreateNotDefault<T>();
        var value = Fixture.Create<T>();
        var changedDelegate = Substitute.For<Func<(T, T), T>>().WithAnyArgs( i => i.ArgAt<(T, T)>( 0 ).Item1 );

        var sut = new Mutation<T>( value, value );

        var result = sut.IfChangedOrDefault( changedDelegate, defaultValue );

        Assertion.All(
                result.TestEquals( defaultValue ),
                changedDelegate.CallCount().TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void IfUnchanged_ShouldCallUnchangedDelegate_WhenValueHasNotChanged()
    {
        var value = Fixture.Create<T>();
        var returnedValue = Fixture.Create<T>();
        var unchangedDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( _ => returnedValue );

        var sut = new Mutation<T>( value, value );

        var result = sut.IfUnchanged( unchangedDelegate );

        Assertion.All(
                result.Value.TestEquals( returnedValue ),
                unchangedDelegate.CallAt( 0 ).Exists.TestTrue(),
                unchangedDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void IfUnchanged_ShouldReturnNone_WhenValueHasChanged()
    {
        var (oldValue, value) = Fixture.CreateManyDistinct<T>( count: 2 );
        var unchangedDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );

        var sut = new Mutation<T>( oldValue, value );

        var result = sut.IfUnchanged( unchangedDelegate );

        Assertion.All(
                result.HasValue.TestFalse(),
                unchangedDelegate.CallCount().TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void IfUnchanged_WithAction_ShouldCallUnchangedDelegate_WhenValueHasNotChanged()
    {
        var value = Fixture.Create<T>();
        var unchangedDelegate = Substitute.For<Action<T>>();

        var sut = new Mutation<T>( value, value );

        sut.IfUnchanged( unchangedDelegate );

        Assertion.All(
                unchangedDelegate.CallAt( 0 ).Exists.TestTrue(),
                unchangedDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void IfUnchanged_WithAction_ShouldDoNothing_WhenValueHasChanged()
    {
        var (oldValue, value) = Fixture.CreateManyDistinct<T>( count: 2 );
        var unchangedDelegate = Substitute.For<Action<T>>();

        var sut = new Mutation<T>( oldValue, value );

        sut.IfUnchanged( unchangedDelegate );

        unchangedDelegate.CallCount().TestEquals( 0 ).Go();
    }

    [Fact]
    public void IfUnchangedOrDefault_ShouldCallUnchangedDelegate_WhenValueHasNotChanged()
    {
        var value = Fixture.Create<T>();
        var returnedValue = Fixture.Create<T>();
        var unchangedDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( _ => returnedValue );

        var sut = new Mutation<T>( value, value );

        var result = sut.IfUnchangedOrDefault( unchangedDelegate );

        Assertion.All(
                result.TestEquals( returnedValue ),
                unchangedDelegate.CallAt( 0 ).Exists.TestTrue(),
                unchangedDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void IfUnchangedOrDefault_ShouldReturnDefault_WhenValueHasChanged()
    {
        var (oldValue, value) = Fixture.CreateManyDistinct<T>( count: 2 );
        var unchangedDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );

        var sut = new Mutation<T>( oldValue, value );

        var result = sut.IfUnchangedOrDefault( unchangedDelegate );

        Assertion.All(
                result.TestEquals( default ),
                unchangedDelegate.CallCount().TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void IfUnchangedOrDefault_WithValue_ShouldCallUnchangedDelegate_WhenValueHasNotChanged()
    {
        var value = Fixture.Create<T>();
        var returnedValue = Fixture.Create<T>();
        var unchangedDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( _ => returnedValue );

        var sut = new Mutation<T>( value, value );

        var result = sut.IfUnchangedOrDefault( unchangedDelegate, Fixture.CreateNotDefault<T>() );

        Assertion.All(
                result.TestEquals( returnedValue ),
                unchangedDelegate.CallAt( 0 ).Exists.TestTrue(),
                unchangedDelegate.CallAt( 0 ).Arguments.FirstOrDefault().TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void IfUnchangedOrDefault_WithValue_ShouldReturnDefault_WhenValueHasChanged()
    {
        var defaultValue = Fixture.CreateNotDefault<T>();
        var (oldValue, value) = Fixture.CreateManyDistinct<T>( count: 2 );
        var unchangedDelegate = Substitute.For<Func<T, T>>().WithAnyArgs( i => i.ArgAt<T>( 0 ) );

        var sut = new Mutation<T>( oldValue, value );

        var result = sut.IfUnchangedOrDefault( unchangedDelegate, defaultValue );

        Assertion.All(
                result.TestEquals( defaultValue ),
                unchangedDelegate.CallCount().TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void TConversionOperator_ShouldReturnCorrectResult()
    {
        var (oldValue, value) = Fixture.CreateManyDistinct<T>( count: 2 );
        var sut = new Mutation<T>( oldValue, value );

        var result = ( T )sut;

        result.TestEquals( value ).Go();
    }

    [Fact]
    public void MutationConversionOperator_FromNil_ShouldReturnCorrectResult()
    {
        var result = ( Mutation<T> )Nil.Instance;

        Assertion.All(
                result.OldValue.TestEquals( default ),
                result.Value.TestEquals( default ),
                result.HasChanged.TestFalse() )
            .Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMutationTestsData<T>.CreateEqualsTestData ) )]
    public void EqualityOperator_ShouldReturnCorrectResult(T oldValue1, T value1, T oldValue2, T value2, bool expected)
    {
        var a = new Mutation<T>( oldValue1, value1 );
        var b = new Mutation<T>( oldValue2, value2 );

        var result = a == b;

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericMutationTestsData<T>.CreateNotEqualsTestData ) )]
    public void InequalityOperator_ShouldReturnCorrectResult(T oldValue1, T value1, T oldValue2, T value2, bool expected)
    {
        var a = new Mutation<T>( oldValue1, value1 );
        var b = new Mutation<T>( oldValue2, value2 );

        var result = a != b;

        result.TestEquals( expected ).Go();
    }
}
