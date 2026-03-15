using LfrlAnvil.Async;

namespace LfrlAnvil.Tests.AsyncTests;

public abstract class GenericInterlockedRefTests<T> : TestsBase
    where T : class?
{
    [Fact]
    public void Default_ShouldBeZero()
    {
        var sut = default( InterlockedRef<T?> );
        sut.Value.TestNull().Go();
    }

    [Fact]
    public void Ctor_ShouldCreateWithCorrectValue()
    {
        var value = Fixture.Create<T>();
        var sut = new InterlockedRef<T?>( value );
        sut.Value.TestRefEquals( value ).Go();
    }

    [Fact]
    public void Ctor_ShouldCreateWithNullValue()
    {
        var sut = new InterlockedRef<T?>( null );
        sut.Value.TestNull().Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<T>();
        var sut = new InterlockedRef<T>( value );
        var result = sut.ToString();
        result.TestEquals( value?.ToString() ?? string.Empty ).Go();
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<T>();
        var sut = new InterlockedRef<T>( value );
        var expected = value?.GetHashCode() ?? 0;

        var result = sut.GetHashCode();

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Exchange_ShouldUpdateValueAndReturnOldValue()
    {
        var (value, newValue) = Fixture.CreateManyDistinct<T>( count: 2 );
        var sut = new InterlockedRef<T>( value );

        var result = sut.Exchange( newValue );

        Assertion.All(
                result.TestRefEquals( value ),
                sut.Value.TestRefEquals( newValue ) )
            .Go();
    }

    [Fact]
    public void Exchange_ShouldDoNothing_WhenValuesAreRefEqual()
    {
        var value = Fixture.Create<T>();
        var sut = new InterlockedRef<T>( value );

        var result = sut.Exchange( value );

        Assertion.All(
                result.TestRefEquals( value ),
                sut.Value.TestRefEquals( value ) )
            .Go();
    }

    [Fact]
    public void Exchange_ShouldUpdateValueToNullAndReturnOldValue()
    {
        var value = Fixture.Create<T>();
        var sut = new InterlockedRef<T?>( value );

        var result = sut.Exchange( null );

        Assertion.All(
                result.TestRefEquals( value ),
                sut.Value.TestNull() )
            .Go();
    }

    [Fact]
    public void Exchange_ShouldUpdateValueAndReturnOldNullValue()
    {
        var newValue = Fixture.Create<T>();
        var sut = new InterlockedRef<T?>( null );

        var result = sut.Exchange( newValue );

        Assertion.All(
                result.TestNull(),
                sut.Value.TestRefEquals( newValue ) )
            .Go();
    }

    [Fact]
    public void Exchange_ShouldDoNothing_WhenValuesAreNull()
    {
        var sut = new InterlockedRef<T?>( null );

        var result = sut.Exchange( null );

        Assertion.All(
                result.TestNull(),
                sut.Value.TestNull() )
            .Go();
    }

    [Fact]
    public void CompareExchange_ShouldUpdateValueWhenOldValueRefEqualsComparandAndReturnOldValue()
    {
        var (value, newValue) = Fixture.CreateManyDistinct<T>( count: 2 );
        var sut = new InterlockedRef<T>( value );

        var result = sut.CompareExchange( newValue, value );

        Assertion.All(
                result.TestRefEquals( value ),
                sut.Value.TestRefEquals( newValue ) )
            .Go();
    }

    [Fact]
    public void CompareExchange_ShouldDoNothing_WhenOldValueIsNotRefEqualToComparand()
    {
        var (value, newValue, comparand) = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = new InterlockedRef<T>( value );

        var result = sut.CompareExchange( newValue, comparand );

        Assertion.All(
                result.TestRefEquals( value ),
                sut.Value.TestRefEquals( value ) )
            .Go();
    }

    [Fact]
    public void CompareExchange_ShouldDoNothing_WhenValuesAreRefEqual()
    {
        var value = Fixture.Create<T>();
        var sut = new InterlockedRef<T>( value );

        var result = sut.CompareExchange( value, value );

        Assertion.All(
                result.TestRefEquals( value ),
                sut.Value.TestRefEquals( value ) )
            .Go();
    }

    [Fact]
    public void CompareExchange_ShouldDoNothing_WhenValuesAreNull()
    {
        var sut = new InterlockedRef<T?>( null );

        var result = sut.CompareExchange( null, null );

        Assertion.All(
                result.TestNull(),
                sut.Value.TestNull() )
            .Go();
    }

    [Fact]
    public void CompareExchange_ShouldUpdateValueToNullAndReturnOldValue()
    {
        var value = Fixture.Create<T>();
        var sut = new InterlockedRef<T?>( value );

        var result = sut.CompareExchange( null, value );

        Assertion.All(
                result.TestRefEquals( value ),
                sut.Value.TestNull() )
            .Go();
    }

    [Fact]
    public void CompareExchange_ShouldUpdateValueAndReturnOldNullValue()
    {
        var value = Fixture.Create<T>();
        var sut = new InterlockedRef<T?>( null );

        var result = sut.CompareExchange( value, null );

        Assertion.All(
                result.TestNull(),
                sut.Value.TestRefEquals( value ) )
            .Go();
    }

    [Fact]
    public void Write_ShouldUpdateValueAndReturnTrue()
    {
        var (value, newValue) = Fixture.CreateManyDistinct<T>( count: 2 );
        var sut = new InterlockedRef<T>( value );

        var result = sut.Write( newValue );

        Assertion.All(
                result.TestTrue(),
                sut.Value.TestRefEquals( newValue ) )
            .Go();
    }

    [Fact]
    public void Write_ShouldReturnFalse_WhenValuesAreRefEqual()
    {
        var value = Fixture.Create<T>();
        var sut = new InterlockedRef<T>( value );

        var result = sut.Write( value );

        Assertion.All(
                result.TestFalse(),
                sut.Value.TestRefEquals( value ) )
            .Go();
    }

    [Fact]
    public void Write_ShouldUpdateValueToNullAndReturnTrue()
    {
        var value = Fixture.Create<T>();
        var sut = new InterlockedRef<T?>( value );

        var result = sut.Write( null );

        Assertion.All(
                result.TestTrue(),
                sut.Value.TestNull() )
            .Go();
    }

    [Fact]
    public void Write_ShouldUpdateNullValueAndReturnTrue()
    {
        var newValue = Fixture.Create<T>();
        var sut = new InterlockedRef<T?>( null );

        var result = sut.Write( newValue );

        Assertion.All(
                result.TestTrue(),
                sut.Value.TestRefEquals( newValue ) )
            .Go();
    }

    [Fact]
    public void Write_ShouldReturnFalse_WhenValuesAreNull()
    {
        var sut = new InterlockedRef<T?>( null );

        var result = sut.Write( null );

        Assertion.All(
                result.TestFalse(),
                sut.Value.TestNull() )
            .Go();
    }

    [Fact]
    public void Write_WithComparand_ShouldUpdateValueWhenOldValueRefEqualsComparandAndReturnTrue()
    {
        var (value, newValue) = Fixture.CreateManyDistinct<T>( count: 2 );
        var sut = new InterlockedRef<T>( value );

        var result = sut.Write( newValue, value );

        Assertion.All(
                result.TestTrue(),
                sut.Value.TestRefEquals( newValue ) )
            .Go();
    }

    [Fact]
    public void Write_WithComparand_ShouldReturnFalse_WhenOldValueIsNotRefEqualToComparand()
    {
        var (value, newValue, comparand) = Fixture.CreateManyDistinct<T>( count: 3 );
        var sut = new InterlockedRef<T>( value );

        var result = sut.Write( newValue, comparand );

        Assertion.All(
                result.TestFalse(),
                sut.Value.TestRefEquals( value ) )
            .Go();
    }

    [Fact]
    public void Write_WithComparand_ShouldReturnFalse_WhenValuesAreRefEqual()
    {
        var value = Fixture.Create<T>();
        var sut = new InterlockedRef<T>( value );

        var result = sut.Write( value, value );

        Assertion.All(
                result.TestFalse(),
                sut.Value.TestRefEquals( value ) )
            .Go();
    }

    [Fact]
    public void Write_WithComparand_ShouldReturnFalse_WhenValuesAreNull()
    {
        var sut = new InterlockedRef<T?>( null );

        var result = sut.Write( null, null );

        Assertion.All(
                result.TestFalse(),
                sut.Value.TestNull() )
            .Go();
    }

    [Fact]
    public void Write_WithComparand_ShouldUpdateValueToNullAndReturnTrue()
    {
        var value = Fixture.Create<T>();
        var sut = new InterlockedRef<T?>( value );

        var result = sut.Write( null, value );

        Assertion.All(
                result.TestTrue(),
                sut.Value.TestNull() )
            .Go();
    }

    [Fact]
    public void Write_WithComparand_ShouldUpdateNullValueAndReturnTrue()
    {
        var value = Fixture.Create<T>();
        var sut = new InterlockedRef<T?>( null );

        var result = sut.Write( value, null );

        Assertion.All(
                result.TestTrue(),
                sut.Value.TestRefEquals( value ) )
            .Go();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrue_WhenValuesAreRefEqual()
    {
        var value = Fixture.Create<T>();
        var a = new InterlockedRef<T>( value );
        var b = new InterlockedRef<T>( value );

        var result = a == b;

        result.TestTrue().Go();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalse_WhenValuesAreNotRefEqual()
    {
        var (value1, value2) = Fixture.CreateManyDistinct<T>( count: 2 );
        var a = new InterlockedRef<T>( value1 );
        var b = new InterlockedRef<T>( value2 );

        var result = a == b;

        result.TestFalse().Go();
    }

    [Fact]
    public void InequalityOperator_ShouldReturnFalse_WhenValuesAreRefEqual()
    {
        var value = Fixture.Create<T>();
        var a = new InterlockedRef<T>( value );
        var b = new InterlockedRef<T>( value );

        var result = a != b;

        result.TestFalse().Go();
    }

    [Fact]
    public void InequalityOperator_ShouldReturnTrue_WhenValuesAreNotRefEqual()
    {
        var (value1, value2) = Fixture.CreateManyDistinct<T>( count: 2 );
        var a = new InterlockedRef<T>( value1 );
        var b = new InterlockedRef<T>( value2 );

        var result = a != b;

        result.TestTrue().Go();
    }
}
