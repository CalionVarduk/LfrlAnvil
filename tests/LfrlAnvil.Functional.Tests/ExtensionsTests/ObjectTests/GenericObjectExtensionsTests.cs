using FluentAssertions.Execution;
using LfrlAnvil.Functional.Extensions;

namespace LfrlAnvil.Functional.Tests.ExtensionsTests.ObjectTests;

public abstract class GenericObjectExtensionsTests<T> : TestsBase
    where T : notnull
{
    [Fact]
    public void ToMaybe_ShouldReturnCorrectResult_WhenNotNull()
    {
        var value = Fixture.CreateNotDefault<T>();

        var sut = value.ToMaybe();

        using ( new AssertionScope() )
        {
            sut.HasValue.Should().BeTrue();
            sut.Value.Should().Be( value );
        }
    }

    [Fact]
    public void ToEither_ShouldReturnCorrectResult()
    {
        var value = Fixture.CreateNotDefault<T>();
        var sut = value.ToEither();
        sut.Value.Should().Be( value );
    }

    [Fact]
    public void ToUnsafe_WithValue_ShouldReturnCorrectResult()
    {
        var value = Fixture.CreateNotDefault<T>();

        var sut = value.ToUnsafe();

        using ( new AssertionScope() )
        {
            sut.IsOk.Should().BeTrue();
            sut.Value.Should().Be( value );
        }
    }

    [Fact]
    public void ToUnsafe_WithException_ShouldReturnCorrectResult()
    {
        var error = new Exception();

        var sut = error.ToUnsafe();

        using ( new AssertionScope() )
        {
            sut.HasError.Should().BeTrue();
            sut.Error.Should().Be( error );
        }
    }

    [Fact]
    public void TypeCast_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<T>();
        var sut = value.TypeCast();
        sut.Value.Should().Be( value );
    }

    [Fact]
    public void ToMutation_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<T>();

        var sut = value.ToMutation();

        using ( new AssertionScope() )
        {
            sut.OldValue.Should().Be( value );
            sut.Value.Should().Be( value );
            sut.HasChanged.Should().BeFalse();
        }
    }

    [Fact]
    public void Mutate_ShouldReturnCorrectResult()
    {
        var (value, newValue) = Fixture.CreateDistinctCollection<T>( 2 );

        var sut = value.Mutate( newValue );

        using ( new AssertionScope() )
        {
            sut.OldValue.Should().Be( value );
            sut.Value.Should().Be( newValue );
            sut.HasChanged.Should().BeTrue();
        }
    }
}
