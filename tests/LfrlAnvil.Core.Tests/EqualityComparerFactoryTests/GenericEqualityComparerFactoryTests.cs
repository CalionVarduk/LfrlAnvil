using System.Collections.Generic;

namespace LfrlAnvil.Tests.EqualityComparerFactoryTests;

public abstract class GenericEqualityComparerFactoryTests<T> : TestsBase
{
    [Fact]
    public void Create_ShouldCreateComparerWithCorrectEqualsImplementation()
    {
        var obj1 = Fixture.Create<T>();
        var obj2 = Fixture.Create<T>();

        var defaultComparer = EqualityComparer<T>.Default;
        var customComparerCalled = false;

        var sut = EqualityComparerFactory<T>.Create( (a, b) =>
        {
            customComparerCalled = true;
            return defaultComparer.Equals( a, b );
        } );

        var result = sut.Equals( obj1, obj2 );

        Assertion.All(
                customComparerCalled.TestTrue(),
                result.TestEquals( defaultComparer.Equals( obj1, obj2 ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldCreateComparerWithCorrectDefaultGetHashCodeImplementation()
    {
        var obj = Fixture.CreateNotDefault<T>();

        var defaultComparer = EqualityComparer<T>.Default;

        var sut = EqualityComparerFactory<T>.Create( (a, b) => defaultComparer.Equals( a, b ) );

        var result = sut.GetHashCode( obj! );

        result.TestEquals( obj!.GetHashCode() ).Go();
    }

    [Fact]
    public void Create_ShouldCreateComparerWithCorrectExplicitGetHashCodeImplementation()
    {
        var obj = Fixture.CreateNotDefault<T>();
        var expected = Fixture.Create<int>();

        var defaultComparer = EqualityComparer<T>.Default;

        int HashCodeCalculator(T _)
        {
            return expected;
        }

        var sut = EqualityComparerFactory<T>.Create(
            (a, b) => defaultComparer.Equals( a, b ),
            HashCodeCalculator );

        var result = sut.GetHashCode( obj! );

        result.TestEquals( expected ).Go();
    }
}
