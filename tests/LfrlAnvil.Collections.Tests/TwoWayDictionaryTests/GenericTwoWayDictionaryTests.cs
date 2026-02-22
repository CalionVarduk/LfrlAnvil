using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Collections.Tests.TwoWayDictionaryTests;

public abstract class GenericTwoWayDictionaryTests<T1, T2> : GenericCollectionTestsBase<Pair<T1, T2>>
    where T1 : notnull
    where T2 : notnull
{
    protected GenericTwoWayDictionaryTests()
    {
        Fixture.Customize<Pair<T1, T2>>( (_, _) => f => Pair.Create( f.Create<T1>(), f.Create<T2>() ) );
    }

    [Fact]
    public void Ctor_ShouldCreateEmptyTwoWayDictionary()
    {
        var sut = new TwoWayDictionary<T1, T2>();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.ForwardComparer.TestEquals( EqualityComparer<T1>.Default ),
                sut.ReverseComparer.TestEquals( EqualityComparer<T2>.Default ) )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldCreateWithExplicitComparers()
    {
        var forwardComparer = EqualityComparerFactory<T1>.Create( (a, b) => a!.Equals( b ) );
        var reverseComparer = EqualityComparerFactory<T2>.Create( (a, b) => a!.Equals( b ) );

        var sut = new TwoWayDictionary<T1, T2>( forwardComparer, reverseComparer );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.ForwardComparer.TestRefEquals( forwardComparer ),
                sut.ReverseComparer.TestRefEquals( reverseComparer ) )
            .Go();
    }

    [Fact]
    public void TryAdd_ShouldReturnFalseAndDoNothing_WhenFirstAlreadyExists()
    {
        var first = Fixture.Create<T1>();
        var (oldSecond, newSecond) = Fixture.CreateManyDistinct<T2>( count: 2 );

        var sut = new TwoWayDictionary<T1, T2> { { first, oldSecond } };

        var result = sut.TryAdd( first, newSecond );

        Assertion.All(
                result.TestFalse(),
                sut.Count.TestEquals( 1 ),
                sut.Forward[first].TestEquals( oldSecond ),
                sut.Reverse[oldSecond].TestEquals( first ) )
            .Go();
    }

    [Fact]
    public void TryAdd_ShouldReturnFalseAndDoNothing_WhenSecondAlreadyExists()
    {
        var (oldFirst, newFirst) = Fixture.CreateManyDistinct<T1>( count: 2 );
        var second = Fixture.Create<T2>();

        var sut = new TwoWayDictionary<T1, T2> { { oldFirst, second } };

        var result = sut.TryAdd( newFirst, second );

        Assertion.All(
                result.TestFalse(),
                sut.Count.TestEquals( 1 ),
                sut.Forward[oldFirst].TestEquals( second ),
                sut.Reverse[second].TestEquals( oldFirst ) )
            .Go();
    }

    [Fact]
    public void TryAdd_ShouldReturnTrueAndAddNewValues_WhenDictionaryIsEmpty()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();

        var sut = new TwoWayDictionary<T1, T2>();

        var result = sut.TryAdd( first, second );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 1 ),
                sut.Forward[first].TestEquals( second ),
                sut.Reverse[second].TestEquals( first ) )
            .Go();
    }

    [Fact]
    public void TryAdd_ShouldReturnTrueAndAddNewValues_WhenFirstAndSecondDontExist()
    {
        var (first, otherFirst) = Fixture.CreateManyDistinct<T1>( count: 2 );
        var (second, otherSecond) = Fixture.CreateManyDistinct<T2>( count: 2 );

        var sut = new TwoWayDictionary<T1, T2> { { otherFirst, otherSecond } };

        var result = sut.TryAdd( first, second );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 2 ),
                sut.Forward[first].TestEquals( second ),
                sut.Reverse[second].TestEquals( first ),
                sut.Forward[otherFirst].TestEquals( otherSecond ),
                sut.Reverse[otherSecond].TestEquals( otherFirst ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldThrowArgumentException_WhenFirstAlreadyExists()
    {
        var first = Fixture.Create<T1>();
        var (oldSecond, newSecond) = Fixture.CreateManyDistinct<T2>( count: 2 );

        var sut = new TwoWayDictionary<T1, T2> { { first, oldSecond } };

        var action = Lambda.Of( () => sut.Add( first, newSecond ) );

        action.Test( exc => Assertion.All(
                exc.TestType().Exact<ArgumentException>(),
                sut.Count.TestEquals( 1 ),
                sut.Forward[first].TestEquals( oldSecond ),
                sut.Reverse[oldSecond].TestEquals( first ) ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldThrowArgumentException_WhenSecondAlreadyExists()
    {
        var (oldFirst, newFirst) = Fixture.CreateManyDistinct<T1>( count: 2 );
        var second = Fixture.Create<T2>();

        var sut = new TwoWayDictionary<T1, T2> { { oldFirst, second } };

        var action = Lambda.Of( () => sut.Add( newFirst, second ) );

        action.Test( exc => Assertion.All(
                exc.TestType().Exact<ArgumentException>(),
                sut.Count.TestEquals( 1 ),
                sut.Forward[oldFirst].TestEquals( second ),
                sut.Reverse[second].TestEquals( oldFirst ) ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddNewValues_WhenDictionaryIsEmpty()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();

        var sut = new TwoWayDictionary<T1, T2>();

        sut.Add( first, second );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut.Forward[first].TestEquals( second ),
                sut.Reverse[second].TestEquals( first ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddNewValues_WhenFirstAndSecondDontExist()
    {
        var (first, otherFirst) = Fixture.CreateManyDistinct<T1>( count: 2 );
        var (second, otherSecond) = Fixture.CreateManyDistinct<T2>( count: 2 );

        var sut = new TwoWayDictionary<T1, T2> { { otherFirst, otherSecond } };

        sut.Add( first, second );

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut.Forward[first].TestEquals( second ),
                sut.Reverse[second].TestEquals( first ),
                sut.Forward[otherFirst].TestEquals( otherSecond ),
                sut.Reverse[otherSecond].TestEquals( otherFirst ) )
            .Go();
    }

    [Fact]
    public void TryUpdateForward_ShouldReturnFalseAndDoNothing_WhenSecondAlreadyExists()
    {
        var (first1, first2) = Fixture.CreateManyDistinct<T1>( count: 2 );
        var second = Fixture.Create<T2>();

        var sut = new TwoWayDictionary<T1, T2> { { first1, second } };

        var result = sut.TryUpdateForward( first2, second );

        Assertion.All(
                result.TestFalse(),
                sut.Count.TestEquals( 1 ),
                sut.Forward[first1].TestEquals( second ),
                sut.Reverse[second].TestEquals( first1 ) )
            .Go();
    }

    [Fact]
    public void TryUpdateForward_ShouldReturnFalseAndDoNothing_WhenFirstDoesntExist()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();

        var sut = new TwoWayDictionary<T1, T2>();

        var result = sut.TryUpdateForward( first, second );

        Assertion.All(
                result.TestFalse(),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void TryUpdateForward_ShouldReturnTrueAndUpdate_WhenFirstExistsAndSecondDoesntExist()
    {
        var first = Fixture.Create<T1>();
        var (second1, second2) = Fixture.CreateManyDistinct<T2>( count: 2 );

        var sut = new TwoWayDictionary<T1, T2> { { first, second1 } };

        var result = sut.TryUpdateForward( first, second2 );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 1 ),
                sut.Forward[first].TestEquals( second2 ),
                sut.Reverse[second2].TestEquals( first ),
                sut.Reverse.ContainsKey( second1 ).TestFalse() )
            .Go();
    }

    [Fact]
    public void UpdateForward_ShouldThrowArgumentException_WhenSecondAlreadyExists()
    {
        var (first1, first2) = Fixture.CreateManyDistinct<T1>( count: 2 );
        var second = Fixture.Create<T2>();

        var sut = new TwoWayDictionary<T1, T2> { { first1, second } };

        var action = Lambda.Of( () => sut.UpdateForward( first2, second ) );

        action.Test( exc => Assertion.All(
                exc.TestType().Exact<ArgumentException>(),
                sut.Count.TestEquals( 1 ),
                sut.Forward[first1].TestEquals( second ),
                sut.Reverse[second].TestEquals( first1 ) ) )
            .Go();
    }

    [Fact]
    public void UpdateForward_ShouldThrowKeyNotFoundException_WhenFirstDoesntExist()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();

        var sut = new TwoWayDictionary<T1, T2>();

        var action = Lambda.Of( () => sut.UpdateForward( first, second ) );

        action.Test( exc => Assertion.All(
                exc.TestType().Exact<KeyNotFoundException>(),
                sut.Count.TestEquals( 0 ) ) )
            .Go();
    }

    [Fact]
    public void UpdateForward_ShouldUpdate_WhenFirstExistsAndSecondDoesntExist()
    {
        var first = Fixture.Create<T1>();
        var (second1, second2) = Fixture.CreateManyDistinct<T2>( count: 2 );

        var sut = new TwoWayDictionary<T1, T2> { { first, second1 } };

        sut.UpdateForward( first, second2 );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut.Forward[first].TestEquals( second2 ),
                sut.Reverse[second2].TestEquals( first ),
                sut.Reverse.ContainsKey( second1 ).TestFalse() )
            .Go();
    }

    [Fact]
    public void TryUpdateReverse_ShouldReturnFalseAndDoNothing_WhenFirstAlreadyExists()
    {
        var first = Fixture.Create<T1>();
        var (second1, second2) = Fixture.CreateManyDistinct<T2>( count: 2 );

        var sut = new TwoWayDictionary<T1, T2> { { first, second1 } };

        var result = sut.TryUpdateReverse( second2, first );

        Assertion.All(
                result.TestFalse(),
                sut.Count.TestEquals( 1 ),
                sut.Forward[first].TestEquals( second1 ),
                sut.Reverse[second1].TestEquals( first ) )
            .Go();
    }

    [Fact]
    public void TryUpdateReverse_ShouldReturnFalseAndDoNothing_WhenSecondDoesntExist()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();

        var sut = new TwoWayDictionary<T1, T2>();

        var result = sut.TryUpdateReverse( second, first );

        Assertion.All(
                result.TestFalse(),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void TryUpdateReverse_ShouldReturnTrueAndUpdate_WhenSecondExistsAndFirstDoesntExist()
    {
        var (first1, first2) = Fixture.CreateManyDistinct<T1>( count: 2 );
        var second = Fixture.Create<T2>();

        var sut = new TwoWayDictionary<T1, T2> { { first1, second } };

        var result = sut.TryUpdateReverse( second, first2 );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 1 ),
                sut.Reverse[second].TestEquals( first2 ),
                sut.Forward[first2].TestEquals( second ),
                sut.Forward.ContainsKey( first1 ).TestFalse() )
            .Go();
    }

    [Fact]
    public void UpdateReverse_ShouldThrowArgumentException_WhenFirstAlreadyExists()
    {
        var first = Fixture.Create<T1>();
        var (second1, second2) = Fixture.CreateManyDistinct<T2>( count: 2 );

        var sut = new TwoWayDictionary<T1, T2> { { first, second1 } };

        var action = Lambda.Of( () => sut.UpdateReverse( second2, first ) );

        action.Test( exc => Assertion.All(
                exc.TestType().Exact<ArgumentException>(),
                sut.Count.TestEquals( 1 ),
                sut.Forward[first].TestEquals( second1 ),
                sut.Reverse[second1].TestEquals( first ) ) )
            .Go();
    }

    [Fact]
    public void UpdateReverse_ShouldThrowKeyNotFoundException_WhenSecondDoesntExist()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();

        var sut = new TwoWayDictionary<T1, T2>();

        var action = Lambda.Of( () => sut.UpdateReverse( second, first ) );

        action.Test( exc => Assertion.All(
                exc.TestType().Exact<KeyNotFoundException>(),
                sut.Count.TestEquals( 0 ) ) )
            .Go();
    }

    [Fact]
    public void UpdateReverse_ShouldUpdate_WhenSecondExistsAndFirstDoesntExist()
    {
        var (first1, first2) = Fixture.CreateManyDistinct<T1>( count: 2 );
        var second = Fixture.Create<T2>();

        var sut = new TwoWayDictionary<T1, T2> { { first1, second } };

        sut.UpdateReverse( second, first2 );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut.Reverse[second].TestEquals( first2 ),
                sut.Forward[first2].TestEquals( second ),
                sut.Forward.ContainsKey( first1 ).TestFalse() )
            .Go();
    }

    [Fact]
    public void RemoveForward_ShouldReturnFalse_WhenValueDoesntExist()
    {
        var first = Fixture.Create<T1>();

        var sut = new TwoWayDictionary<T1, T2>();

        var result = sut.RemoveForward( first );

        result.TestFalse().Go();
    }

    [Fact]
    public void RemoveForward_ShouldReturnTrueAndRemove_WhenValueExists()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();

        var sut = new TwoWayDictionary<T1, T2> { { first, second } };

        var result = sut.RemoveForward( first );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void RemoveReverse_ShouldReturnFalse_WhenValueDoesntExist()
    {
        var second = Fixture.Create<T2>();

        var sut = new TwoWayDictionary<T1, T2>();

        var result = sut.RemoveReverse( second );

        result.TestFalse().Go();
    }

    [Fact]
    public void RemoveReverse_ShouldReturnTrueAndRemove_WhenValueExists()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();

        var sut = new TwoWayDictionary<T1, T2> { { first, second } };

        var result = sut.RemoveReverse( second );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void RemoveForward_ShouldReturnFalse_WhenValueDoesntExist_WithOutParam()
    {
        var first = Fixture.Create<T1>();

        var sut = new TwoWayDictionary<T1, T2>();

        var result = sut.RemoveForward( first, out var removed );

        Assertion.All(
                result.TestFalse(),
                removed.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void RemoveForward_ShouldReturnTrueAndRemove_WhenValueExists_WithOutParam()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();

        var sut = new TwoWayDictionary<T1, T2> { { first, second } };

        var result = sut.RemoveForward( first, out var removed );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 0 ),
                removed.TestEquals( second ) )
            .Go();
    }

    [Fact]
    public void RemoveReverse_ShouldReturnFalse_WhenValueDoesntExist_WithOutParam()
    {
        var second = Fixture.Create<T2>();

        var sut = new TwoWayDictionary<T1, T2>();

        var result = sut.RemoveReverse( second, out var removed );

        Assertion.All(
                result.TestFalse(),
                removed.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void RemoveReverse_ShouldReturnTrueAndRemove_WhenValueExists_WithOutParam()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();

        var sut = new TwoWayDictionary<T1, T2> { { first, second } };

        var result = sut.RemoveReverse( second, out var removed );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 0 ),
                removed.TestEquals( first ) )
            .Go();
    }

    [Fact]
    public void Clear_ShouldRemoveAll()
    {
        var (first1, first2, first3) = Fixture.CreateManyDistinct<T1>( count: 3 );
        var (second1, second2, second3) = Fixture.CreateManyDistinct<T2>( count: 3 );

        var sut = new TwoWayDictionary<T1, T2>
        {
            { first1, second1 },
            { first2, second2 },
            { first3, second3 }
        };

        sut.Clear();

        sut.Count.TestEquals( 0 ).Go();
    }

    [Fact]
    public void Contains_ShouldReturnTrue_WhenFirstAndSecondExistAndAreLinked()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();

        var sut = new TwoWayDictionary<T1, T2> { { first, second } };

        var result = sut.Contains( first, second );

        result.TestTrue().Go();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenFirstExistsButIsNotLinkedWithSecond()
    {
        var first = Fixture.Create<T1>();
        var (second1, second2) = Fixture.CreateManyDistinct<T2>( count: 2 );

        var sut = new TwoWayDictionary<T1, T2> { { first, second1 } };

        var result = sut.Contains( first, second2 );

        result.TestFalse().Go();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenSecondExistsButIsNotLinkedWithFirst()
    {
        var (first1, first2) = Fixture.CreateManyDistinct<T1>( count: 2 );
        var second = Fixture.Create<T2>();

        var sut = new TwoWayDictionary<T1, T2> { { first1, second } };

        var result = sut.Contains( first2, second );

        result.TestFalse().Go();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenFirstAndSecondDontExist()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();

        var sut = new TwoWayDictionary<T1, T2>();

        var result = sut.Contains( first, second );

        result.TestFalse().Go();
    }

    [Fact]
    public void Contains_WithPair_ShouldReturnTrue_WhenFirstAndSecondExistAndAreLinked()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();

        var sut = new TwoWayDictionary<T1, T2> { { first, second } };

        var result = sut.Contains( Pair.Create( first, second ) );

        result.TestTrue().Go();
    }

    [Fact]
    public void Contains_WithPair_ShouldReturnFalse_WhenFirstExistsButIsNotLinkedWithSecond()
    {
        var first = Fixture.Create<T1>();
        var (second1, second2) = Fixture.CreateManyDistinct<T2>( count: 2 );

        var sut = new TwoWayDictionary<T1, T2> { { first, second1 } };

        var result = sut.Contains( Pair.Create( first, second2 ) );

        result.TestFalse().Go();
    }

    [Fact]
    public void Contains_WithPair_ShouldReturnFalse_WhenSecondExistsButIsNotLinkedWithFirst()
    {
        var (first1, first2) = Fixture.CreateManyDistinct<T1>( count: 2 );
        var second = Fixture.Create<T2>();

        var sut = new TwoWayDictionary<T1, T2> { { first1, second } };

        var result = sut.Contains( Pair.Create( first2, second ) );

        result.TestFalse().Go();
    }

    [Fact]
    public void Contains_WithPair_ShouldReturnFalse_WhenFirstAndSecondDontExist()
    {
        var first = Fixture.Create<T1>();
        var second = Fixture.Create<T2>();

        var sut = new TwoWayDictionary<T1, T2>();

        var result = sut.Contains( Pair.Create( first, second ) );

        result.TestFalse().Go();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectResult()
    {
        var (first1, first2, first3) = Fixture.CreateManyDistinct<T1>( count: 3 );
        var (second1, second2, second3) = Fixture.CreateManyDistinct<T2>( count: 3 );

        var expected
            = new[] { Pair.Create( first1, second1 ), Pair.Create( first2, second2 ), Pair.Create( first3, second3 ) }.AsEnumerable();

        var sut = new TwoWayDictionary<T1, T2>
        {
            { first1, second1 },
            { first2, second2 },
            { first3, second3 }
        };

        sut.TestSetEqual( expected ).Go();
    }

    [Fact]
    public void ICollectionRemove_ShouldReturnFalseAndDoNothing_WhenFirstExistsButIsNotLinkedWithSecond()
    {
        var first = Fixture.Create<T1>();
        var (second1, second2) = Fixture.CreateManyDistinct<T2>( count: 2 );

        var dictionary = new TwoWayDictionary<T1, T2> { { first, second1 } };
        var sut = ( ICollection<Pair<T1, T2>> )dictionary;

        var result = sut.Remove( Pair.Create( first, second2 ) );

        result.TestFalse().Go();
    }

    [Fact]
    public void ICollectionRemove_ShouldReturnFalseAndDoNothing_WhenSecondExistsButIsNotLinkedWithFirst()
    {
        var (first1, first2) = Fixture.CreateManyDistinct<T1>( count: 2 );
        var second = Fixture.Create<T2>();

        var dictionary = new TwoWayDictionary<T1, T2> { { first1, second } };
        var sut = ( ICollection<Pair<T1, T2>> )dictionary;

        var result = sut.Remove( Pair.Create( first2, second ) );

        result.TestFalse().Go();
    }

    protected sealed override ICollection<Pair<T1, T2>> CreateEmptyCollection()
    {
        return new TwoWayDictionary<T1, T2>();
    }
}
