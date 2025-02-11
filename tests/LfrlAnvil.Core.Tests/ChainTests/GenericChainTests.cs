using System.Linq;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.ChainTests;

public abstract class GenericChainTests<T> : TestsBase
{
    [Fact]
    public void Default_ShouldReturnEmptyChainThatCanBeExtended()
    {
        var sut = default( Chain<T> );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.IsAttached.TestFalse(),
                sut.IsExtendable.TestTrue() )
            .Go();
    }

    [Fact]
    public void Empty_ShouldReturnEmptyChainThatCanBeExtended()
    {
        var sut = Chain<T>.Empty;

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.IsAttached.TestFalse(),
                sut.IsExtendable.TestTrue() )
            .Go();
    }

    [Fact]
    public void Create_WithOneValue_ShouldReturnCorrectChain()
    {
        var value = Fixture.Create<T>();
        var sut = Chain.Create( value );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut.IsAttached.TestFalse(),
                sut.IsExtendable.TestTrue(),
                sut.TestSequence( [ value ] ) )
            .Go();
    }

    [Fact]
    public void Create_WithMultipleValues_ShouldReturnCorrectChain()
    {
        var values = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = Chain.Create( values.AsEnumerable() );

        Assertion.All(
                sut.Count.TestEquals( values.Length ),
                sut.IsAttached.TestFalse(),
                sut.IsExtendable.TestTrue(),
                sut.TestSequence( values ) )
            .Go();
    }

    [Fact]
    public void Create_WithOtherChain_ShouldReturnCorrectChain()
    {
        var values = Fixture.CreateManyDistinct<T>( count: 3 );
        var other = Chain.Create( values.AsEnumerable() );
        var sut = Chain.Create( other );

        Assertion.All(
                sut.Count.TestEquals( other.Count ),
                sut.IsAttached.TestFalse(),
                sut.IsExtendable.TestTrue(),
                sut.TestSequence( other ),
                other.IsAttached.TestFalse(),
                other.IsExtendable.TestTrue() )
            .Go();
    }

    [Fact]
    public void Create_WithOtherPartialChain_ShouldReturnCorrectChain()
    {
        var values = Fixture.CreateManyDistinct<T>( count: 4 );
        var other = Chain.Create( values.Take( 3 ).AsEnumerable() );
        _ = other.Extend( values.Last() );
        var sut = Chain.Create( other );

        Assertion.All(
                sut.Count.TestEquals( other.Count ),
                sut.IsAttached.TestFalse(),
                sut.IsExtendable.TestTrue(),
                sut.TestSequence( other ),
                other.IsAttached.TestFalse(),
                other.IsExtendable.TestFalse() )
            .Go();
    }

    [Fact]
    public void Ctor_WithOneValue_ShouldReturnCorrectChain()
    {
        var value = Fixture.Create<T>();
        var sut = new Chain<T>( value );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut.IsAttached.TestFalse(),
                sut.IsExtendable.TestTrue(),
                sut.TestSequence( [ value ] ) )
            .Go();
    }

    [Fact]
    public void Ctor_WithMultipleValues_ShouldReturnCorrectChain()
    {
        var values = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = new Chain<T>( values.AsEnumerable() );

        Assertion.All(
                sut.Count.TestEquals( values.Length ),
                sut.IsAttached.TestFalse(),
                sut.IsExtendable.TestTrue(),
                sut.TestSequence( values ) )
            .Go();
    }

    [Fact]
    public void Ctor_WithEmptyEnumerable_ShouldReturnEmptyChain()
    {
        var sut = new Chain<T>( Enumerable.Empty<T>() );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.IsAttached.TestFalse(),
                sut.IsExtendable.TestTrue(),
                sut.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Ctor_WithOtherChain_ShouldReturnCorrectChain()
    {
        var values = Fixture.CreateManyDistinct<T>( count: 3 );
        var other = new Chain<T>( values.AsEnumerable() );
        var sut = new Chain<T>( other );

        Assertion.All(
                sut.Count.TestEquals( other.Count ),
                sut.IsAttached.TestFalse(),
                sut.IsExtendable.TestTrue(),
                sut.TestSequence( other ),
                other.IsAttached.TestFalse(),
                other.IsExtendable.TestTrue() )
            .Go();
    }

    [Fact]
    public void Ctor_WithOtherPartialChain_ShouldReturnCorrectChain()
    {
        var values = Fixture.CreateManyDistinct<T>( count: 4 );
        var other = Chain.Create( values.Take( 3 ).AsEnumerable() );
        _ = other.Extend( values.Last() );
        var sut = new Chain<T>( other );

        Assertion.All(
                sut.Count.TestEquals( other.Count ),
                sut.IsAttached.TestFalse(),
                sut.IsExtendable.TestTrue(),
                sut.TestSequence( other ),
                other.IsAttached.TestFalse(),
                other.IsExtendable.TestFalse() )
            .Go();
    }

    [Fact]
    public void Ctor_WithEmptyOtherChain_ShouldReturnEmptyChain()
    {
        var sut = new Chain<T>( Chain<T>.Empty );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.IsAttached.TestFalse(),
                sut.IsExtendable.TestTrue(),
                sut.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Extend_WithOneValue_ShouldReturnCorrectChain_WhenChainIsEmpty()
    {
        var value = Fixture.Create<T>();
        var sut = Chain<T>.Empty;

        var result = sut.Extend( value );

        Assertion.All(
                result.Count.TestEquals( 1 ),
                result.IsAttached.TestFalse(),
                result.IsExtendable.TestTrue(),
                result.TestSequence( [ value ] ) )
            .Go();
    }

    [Fact]
    public void Extend_WithOneValue_ShouldNotModifyOriginalChain_WhenChainIsEmpty()
    {
        var value = Fixture.Create<T>();
        var sut = Chain<T>.Empty;

        _ = sut.Extend( value );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.IsAttached.TestFalse(),
                sut.IsExtendable.TestTrue(),
                sut.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Extend_WithOneValue_ShouldReturnCorrectChain_WhenChainIsNotEmpty()
    {
        var allValues = Fixture.CreateManyDistinct<T>( count: 4 );
        var initialValues = allValues.Take( 3 );
        var value = allValues[^1];
        var sut = Chain.Create( initialValues );

        var result = sut.Extend( value );

        Assertion.All(
                result.Count.TestEquals( allValues.Length ),
                result.IsAttached.TestFalse(),
                result.IsExtendable.TestTrue(),
                result.TestSequence( allValues ) )
            .Go();
    }

    [Fact]
    public void Extend_WithOneValue_ShouldMakeOriginalChainNonExtendable_WhenChainIsNotEmpty()
    {
        var allValues = Fixture.CreateManyDistinct<T>( count: 4 );
        var initialValues = allValues.Take( 3 ).ToList();
        var value = allValues[^1];
        var sut = Chain.Create( initialValues.AsEnumerable() );

        _ = sut.Extend( value );

        Assertion.All(
                sut.Count.TestEquals( initialValues.Count ),
                sut.IsAttached.TestFalse(),
                sut.IsExtendable.TestFalse(),
                sut.TestSequence( initialValues ) )
            .Go();
    }

    [Fact]
    public void Extend_WithOneValue_ShouldThrowInvalidOperationException_WhenChainHasAlreadyBeenExtended()
    {
        var values = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = Chain.Create( values[0] );
        _ = sut.Extend( values[1] );

        var action = Lambda.Of( () => sut.Extend( values[2] ) );
        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void Extend_WithOneValue_ShouldThrowInvalidOperationException_WhenChainIsAttachedToAnotherChain()
    {
        var values = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = Chain.Create( values[0] );
        _ = Chain.Create( values[1] ).Extend( sut );

        var action = Lambda.Of( () => sut.Extend( values[2] ) );
        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void Extend_WithMultipleValues_ShouldReturnCorrectChain_WhenChainIsEmpty()
    {
        var values = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = Chain<T>.Empty;

        var result = sut.Extend( values );

        Assertion.All(
                result.Count.TestEquals( values.Length ),
                result.IsAttached.TestFalse(),
                result.IsExtendable.TestTrue(),
                result.TestSequence( values ) )
            .Go();
    }

    [Fact]
    public void Extend_WithMultipleValues_ShouldNotModifyOriginalChain_WhenChainIsEmpty()
    {
        var values = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = Chain<T>.Empty;

        _ = sut.Extend( values );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.IsAttached.TestFalse(),
                sut.IsExtendable.TestTrue(),
                sut.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Extend_WithMultipleValues_ShouldReturnCorrectChain_WhenChainIsNotEmpty()
    {
        var allValues = Fixture.CreateManyDistinct<T>( count: 6 );
        var initialValues = allValues.Take( 3 );
        var values = allValues.Skip( 3 );
        var sut = Chain.Create( initialValues );

        var result = sut.Extend( values );

        Assertion.All(
                result.Count.TestEquals( allValues.Length ),
                result.IsAttached.TestFalse(),
                result.IsExtendable.TestTrue(),
                result.TestSequence( allValues ) )
            .Go();
    }

    [Fact]
    public void Extend_WithMultipleValues_ShouldMakeOriginalChainNonExtendable_WhenChainIsNotEmpty()
    {
        var allValues = Fixture.CreateManyDistinct<T>( count: 6 );
        var initialValues = allValues.Take( 3 ).ToList();
        var values = allValues.Skip( 3 );
        var sut = Chain.Create( initialValues.AsEnumerable() );

        _ = sut.Extend( values );

        Assertion.All(
                sut.Count.TestEquals( initialValues.Count ),
                sut.IsAttached.TestFalse(),
                sut.IsExtendable.TestFalse(),
                sut.TestSequence( initialValues ) )
            .Go();
    }

    [Fact]
    public void Extend_WithMultipleValues_ShouldReturnOriginalChain_WhenEnumerableIsEmpty()
    {
        var initialValues = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = Chain.Create( initialValues.AsEnumerable() );

        var result = sut.Extend( Enumerable.Empty<T>() );

        Assertion.All(
                result.Count.TestEquals( initialValues.Length ),
                result.IsAttached.TestFalse(),
                result.IsExtendable.TestTrue(),
                result.TestSequence( initialValues ),
                sut.IsExtendable.TestTrue() )
            .Go();
    }

    [Fact]
    public void Extend_WithMultipleValues_ShouldThrowInvalidOperationException_WhenChainHasAlreadyBeenExtended()
    {
        var values = Fixture.CreateManyDistinct<T>( count: 4 );
        var sut = Chain.Create( values[0] );
        _ = sut.Extend( values[1] );

        var action = Lambda.Of( () => sut.Extend( values.Skip( 2 ) ) );
        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void Extend_WithMultipleValues_ShouldThrowInvalidOperationException_WhenChainIsAttachedToAnotherChain()
    {
        var values = Fixture.CreateManyDistinct<T>( count: 4 );
        var sut = Chain.Create( values[0] );
        _ = Chain.Create( values[1] ).Extend( sut );

        var action = Lambda.Of( () => sut.Extend( values.Skip( 2 ) ) );
        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void Extend_WithChain_ShouldReturnCorrectChain_WhenChainIsEmpty()
    {
        var values = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = Chain<T>.Empty;
        var other = Chain.Create( values.AsEnumerable() );

        var result = sut.Extend( other );

        Assertion.All(
                result.Count.TestEquals( values.Length ),
                result.IsAttached.TestFalse(),
                result.IsExtendable.TestTrue(),
                result.TestSequence( values ) )
            .Go();
    }

    [Fact]
    public void Extend_WithChain_ShouldNotModifyOriginalChain_WhenChainIsEmpty()
    {
        var values = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = Chain<T>.Empty;
        var other = Chain.Create( values.AsEnumerable() );

        _ = sut.Extend( other );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.IsAttached.TestFalse(),
                sut.IsExtendable.TestTrue(),
                sut.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Extend_WithChain_ShouldNotModifyOtherChain_WhenChainIsEmpty()
    {
        var values = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = Chain<T>.Empty;
        var other = Chain.Create( values.AsEnumerable() );

        _ = sut.Extend( other );

        Assertion.All(
                other.Count.TestEquals( values.Length ),
                other.IsAttached.TestFalse(),
                other.IsExtendable.TestTrue(),
                other.TestSequence( values ) )
            .Go();
    }

    [Fact]
    public void Extend_WithChain_ShouldReturnCorrectChain_WhenChainIsNotEmpty()
    {
        var allValues = Fixture.CreateManyDistinct<T>( count: 6 );
        var initialValues = allValues.Take( 3 );
        var values = allValues.Skip( 3 );
        var sut = Chain.Create( initialValues );
        var other = Chain.Create( values );

        var result = sut.Extend( other );

        Assertion.All(
                result.Count.TestEquals( allValues.Length ),
                result.IsAttached.TestFalse(),
                result.IsExtendable.TestTrue(),
                result.TestSequence( allValues ) )
            .Go();
    }

    [Fact]
    public void Extend_WithChain_ShouldMakeOriginalChainNonExtendable_WhenChainIsNotEmpty()
    {
        var allValues = Fixture.CreateManyDistinct<T>( count: 6 );
        var initialValues = allValues.Take( 3 ).ToList();
        var values = allValues.Skip( 3 );
        var sut = Chain.Create( initialValues.AsEnumerable() );
        var other = Chain.Create( values );

        _ = sut.Extend( other );

        Assertion.All(
                sut.Count.TestEquals( initialValues.Count ),
                sut.IsAttached.TestFalse(),
                sut.IsExtendable.TestFalse(),
                sut.TestSequence( initialValues ) )
            .Go();
    }

    [Fact]
    public void Extend_WithChain_ShouldMakeOtherChainAttached_WhenChainIsNotEmpty()
    {
        var allValues = Fixture.CreateManyDistinct<T>( count: 6 );
        var initialValues = allValues.Take( 3 );
        var values = allValues.Skip( 3 ).ToList();
        var sut = Chain.Create( initialValues );
        var other = Chain.Create( values.AsEnumerable() );

        _ = sut.Extend( other );

        Assertion.All(
                other.Count.TestEquals( values.Count ),
                other.IsAttached.TestTrue(),
                other.IsExtendable.TestFalse(),
                other.TestSequence( values ) )
            .Go();
    }

    [Fact]
    public void Extend_WithChain_ShouldReturnOriginalChain_WhenOtherChainIsEmpty()
    {
        var initialValues = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = Chain.Create( initialValues.AsEnumerable() );

        var result = sut.Extend( Chain<T>.Empty );

        Assertion.All(
                result.Count.TestEquals( initialValues.Length ),
                result.IsAttached.TestFalse(),
                result.IsExtendable.TestTrue(),
                result.TestSequence( initialValues ),
                sut.IsExtendable.TestTrue() )
            .Go();
    }

    [Fact]
    public void Extend_WithChain_ShouldReturnCorrectChainMarkedAsExtended_WhenOtherChainIsAlreadyExtended()
    {
        var allValues = Fixture.CreateManyDistinct<T>( count: 6 );
        var initialValues = allValues.Take( 3 );
        var values = allValues.Skip( 3 ).Take( 2 );
        var sut = Chain.Create( initialValues );
        var other = Chain.Create( values );
        _ = other.Extend( allValues[^1] );

        var result = sut.Extend( other );

        Assertion.All(
                result.Count.TestEquals( allValues.Length - 1 ),
                result.IsAttached.TestFalse(),
                result.IsExtendable.TestFalse(),
                result.TestSequence( allValues.SkipLast( 1 ) ) )
            .Go();
    }

    [Fact]
    public void Extend_WithChain_ShouldReturnCorrectChain_WhenOtherChainIsAlreadyAttached()
    {
        var allValues = Fixture.CreateManyDistinct<T>( count: 6 );
        var initialValues = allValues.Take( 3 );
        var values = allValues.Skip( 3 ).Take( 2 );
        var sut = Chain.Create( initialValues );
        var other = Chain.Create( values );
        _ = Chain.Create( allValues[^1] ).Extend( other );

        var result = sut.Extend( other );

        Assertion.All(
                result.Count.TestEquals( allValues.Length - 1 ),
                result.IsAttached.TestFalse(),
                result.IsExtendable.TestTrue(),
                result.TestSequence( allValues.SkipLast( 1 ) ) )
            .Go();
    }

    [Fact]
    public void Extend_WithChain_ShouldReturnCorrectChain_WhenAttachingSelf()
    {
        var values = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = Chain.Create( values.AsEnumerable() );

        var result = sut.Extend( sut );

        Assertion.All(
                result.Count.TestEquals( values.Length * 2 ),
                result.IsAttached.TestTrue(),
                result.IsExtendable.TestFalse(),
                result.TestSequence( values.Concat( values ) ) )
            .Go();
    }

    [Fact]
    public void Extend_WithChain_ShouldModifyOriginalChain_WhenAttachingSelf()
    {
        var values = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = Chain.Create( values.AsEnumerable() );

        _ = sut.Extend( sut );

        Assertion.All(
                sut.Count.TestEquals( values.Length ),
                sut.IsAttached.TestTrue(),
                sut.IsExtendable.TestFalse(),
                sut.TestSequence( values ) )
            .Go();
    }

    [Fact]
    public void Extend_WithChain_ShouldThrowInvalidOperationException_WhenChainHasAlreadyBeenExtended()
    {
        var values = Fixture.CreateManyDistinct<T>( count: 4 );
        var sut = Chain.Create( values[0] );
        _ = sut.Extend( values[1] );

        var action = Lambda.Of( () => sut.Extend( Chain.Create( values.Skip( 2 ) ) ) );
        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void Extend_WithChain_ShouldThrowInvalidOperationException_WhenChainIsAttachedToAnotherChain()
    {
        var values = Fixture.CreateManyDistinct<T>( count: 4 );
        var sut = Chain.Create( values[0] );
        _ = Chain.Create( values[1] ).Extend( sut );

        var action = Lambda.Of( () => sut.Extend( Chain.Create( values.Skip( 2 ) ) ) );
        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void ToExtendable_ShouldReturnThis_WhenChainIsExtendable()
    {
        var values = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = Chain.Create( values.AsEnumerable() );

        var result = sut.ToExtendable();

        result.TestSequence( sut ).Go();
    }

    [Fact]
    public void ToExtendable_ShouldReturnCopy_WhenChainIsNotExtendable()
    {
        var values = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = Chain.Create( values.AsEnumerable() );
        _ = Chain.Create( Fixture.Create<T>() ).Extend( sut );

        var result = sut.ToExtendable();

        Assertion.All(
                sut.IsExtendable.TestFalse(),
                result.TestSequence( sut ),
                result.IsExtendable.TestTrue() )
            .Go();
    }

    [Fact]
    public void ToExtendable_ShouldReturnCopy_WhenPartialChainIsNotExtendable()
    {
        var values = Fixture.CreateManyDistinct<T>( count: 4 );
        var sut = Chain.Create( values.Take( 3 ).AsEnumerable() );
        _ = sut.Extend( values.Last() );
        _ = Chain.Create( Fixture.Create<T>() ).Extend( sut );

        var result = sut.ToExtendable();

        Assertion.All(
                sut.IsExtendable.TestFalse(),
                result.TestSequence( sut ),
                result.IsExtendable.TestTrue() )
            .Go();
    }
}
