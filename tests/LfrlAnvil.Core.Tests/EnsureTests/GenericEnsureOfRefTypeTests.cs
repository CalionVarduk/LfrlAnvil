using System.Linq;

namespace LfrlAnvil.Tests.EnsureTests;

public abstract class GenericEnsureOfRefTypeTests<T> : GenericEnsureOfComparableTypeTests<T>
    where T : class, IEquatable<T>, IComparable<T>
{
    [Fact]
    public void IsNull_ShouldPass_WhenParamIsNull()
    {
        var param = Fixture.CreateDefault<T>();
        ShouldPass( () => Ensure.IsNull( param ) );
    }

    [Fact]
    public void IsNull_ShouldPass_WhenParamIsNull_WithExplicitComparer()
    {
        IsNull_ShouldPass_WhenParamIsNull_WithExplicitComparer_Impl();
    }

    [Fact]
    public void IsNull_ShouldThrowArgumentException_WhenParamIsNotNull()
    {
        var param = Fixture.CreateNotDefault<T>();
        ShouldThrowArgumentException( () => Ensure.IsNull( param ) );
    }

    [Fact]
    public void IsNull_ShouldThrowArgumentException_WhenParamIsNotNull_WithExplicitComparer()
    {
        IsNull_ShouldThrowArgumentException_WhenParamIsNotNull_WithExplicitComparer_Impl();
    }

    [Fact]
    public void IsNotNull_ShouldPass_WhenParamIsNotNull()
    {
        var param = Fixture.CreateNotDefault<T>();
        ShouldPass( () => Ensure.IsNotNull( param ) );
    }

    [Fact]
    public void IsNotNull_ShouldPass_WhenParamIsNotNull_WithExplicitComparer()
    {
        IsNotNull_ShouldPass_WhenParamIsNotNull_WithExplicitComparer_Impl();
    }

    [Fact]
    public void IsNotNull_ShouldThrowArgumentNullException_WhenParamIsNull()
    {
        var param = Fixture.CreateDefault<T>();
        ShouldThrowExactly<ArgumentNullException>( () => Ensure.IsNotNull( param ) );
    }

    [Fact]
    public void IsNotNull_ShouldThrowArgumentNullException_WhenParamIsNull_WithExplicitComparer()
    {
        IsNotNull_ShouldThrowArgumentNullException_WhenParamIsNull_WithExplicitComparer_Impl();
    }

    [Fact]
    public void IsOfType_ShouldPass_WhenTypesMatch()
    {
        var param = Fixture.CreateNotDefault<T>();
        ShouldPass( () => Ensure.IsOfType<T>( param ) );
    }

    [Fact]
    public void IsNotOfType_ShouldThrowArgumentException_WhenTypesMatch()
    {
        var param = Fixture.CreateNotDefault<T>();
        ShouldThrowArgumentException( () => Ensure.IsNotOfType<T>( param ) );
    }

    [Fact]
    public void RefEquals_ShouldPass_WhenParamsReferenceTheSameObject()
    {
        var param = Fixture.CreateNotDefault<T>();
        ShouldPass( () => Ensure.RefEquals( param, param ) );
    }

    [Fact]
    public void RefEquals_ShouldPass_WhenBothParamsAreNull()
    {
        var param = Fixture.CreateDefault<T>();
        ShouldPass( () => Ensure.RefEquals( param, param ) );
    }

    [Fact]
    public void RefEquals_ShouldThrowArgumentException_WhenValueIsNull()
    {
        var param = Fixture.CreateNotDefault<T>();
        var value = Fixture.CreateDefault<T>();
        ShouldThrowArgumentException( () => Ensure.RefEquals( param, value ) );
    }

    [Fact]
    public void RefEquals_ShouldThrowArgumentException_WhenParamIsNull()
    {
        var param = Fixture.CreateDefault<T>();
        var value = Fixture.CreateNotDefault<T>();
        ShouldThrowArgumentException( () => Ensure.RefEquals( param, value ) );
    }

    [Fact]
    public void RefEquals_ShouldThrowArgumentException_WhenParamsDontReferenceTheSameObject()
    {
        var (param, value) = Fixture.CreateDistinctCollection<T>( 2 );
        ShouldThrowArgumentException( () => Ensure.RefEquals( param, value ) );
    }

    [Fact]
    public void NotRefEquals_ShouldThrowArgumentException_WhenParamsReferenceTheSameObject()
    {
        var param = Fixture.CreateNotDefault<T>();
        ShouldThrowArgumentException( () => Ensure.NotRefEquals( param, param ) );
    }

    [Fact]
    public void NotRefEquals_ShouldThrowArgumentException_WhenBothParamsAreNull()
    {
        var param = Fixture.CreateDefault<T>();
        ShouldThrowArgumentException( () => Ensure.NotRefEquals( param, param ) );
    }

    [Fact]
    public void NotRefEquals_ShouldPass_WhenValueIsNull()
    {
        var param = Fixture.CreateNotDefault<T>();
        var value = Fixture.CreateDefault<T>();
        ShouldPass( () => Ensure.NotRefEquals( param, value ) );
    }

    [Fact]
    public void NotRefEquals_ShouldPass_WhenParamIsNull()
    {
        var param = Fixture.CreateDefault<T>();
        var value = Fixture.CreateNotDefault<T>();
        ShouldPass( () => Ensure.NotRefEquals( param, value ) );
    }

    [Fact]
    public void NotRefEquals_ShouldPass_WhenParamsDontReferenceTheSameObject()
    {
        var (param, value) = Fixture.CreateDistinctCollection<T>( 2 );
        ShouldPass( () => Ensure.NotRefEquals( param, value ) );
    }

    [Fact]
    public void ContainsNull_ShouldPass_WhenEnumerableContainsNullElement()
    {
        var param = Fixture.CreateMany<T>().Append( Fixture.CreateDefault<T>() );
        ShouldPass( () => Ensure.ContainsNull( param ) );
    }

    [Fact]
    public void ContainsNull_ShouldPass_WhenEnumerableContainsNullElement_WithExplicitComparer()
    {
        var param = Fixture.CreateMany<T>().Append( Fixture.CreateDefault<T>() );
        ShouldPass( () => Ensure.ContainsNull( param, EqualityComparer ) );
    }

    [Fact]
    public void ContainsNull_ShouldThrowArgumentException_WhenEnumerableDoesntContainNullElement()
    {
        var param = Fixture.CreateMany<T>();
        ShouldThrowArgumentException( () => Ensure.ContainsNull( param ) );
    }

    [Fact]
    public void ContainsNull_ShouldThrowArgumentException_WhenEnumerableDoesntContainNullElement_WithExplicitComparer()
    {
        var param = Fixture.CreateMany<T>();
        ShouldThrowArgumentException( () => Ensure.ContainsNull( param, EqualityComparer ) );
    }

    [Fact]
    public void NotContainsNull_ShouldPass_WhenEnumerableDoesntContainNullElement()
    {
        var param = Fixture.CreateMany<T>();
        ShouldPass( () => Ensure.NotContainsNull( param ) );
    }

    [Fact]
    public void NotContainsNull_ShouldPass_WhenEnumerableDoesntContainNullElement_WithExplicitComparer()
    {
        var param = Fixture.CreateMany<T>();
        ShouldPass( () => Ensure.NotContainsNull( param, EqualityComparer ) );
    }

    [Fact]
    public void NotContainsNull_ShouldThrowArgumentException_WhenEnumerableContainsNullElement()
    {
        var param = Fixture.CreateMany<T>().Append( Fixture.CreateDefault<T>() );
        ShouldThrowArgumentException( () => Ensure.NotContainsNull( param ) );
    }

    [Fact]
    public void NotContainsNull_ShouldThrowArgumentException_WhenEnumerableContainsNullElement_WithExplicitComparer()
    {
        var param = Fixture.CreateMany<T>().Append( Fixture.CreateDefault<T>() );
        ShouldThrowArgumentException( () => Ensure.NotContainsNull( param, EqualityComparer ) );
    }
}
