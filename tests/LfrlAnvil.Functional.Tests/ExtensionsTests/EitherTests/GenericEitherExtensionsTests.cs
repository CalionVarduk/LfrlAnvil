using System;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional.Extensions;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Functional.Tests.ExtensionsTests.EitherTests;

public abstract class GenericEitherExtensionsTests<T1, T2> : TestsBase
    where T1 : notnull
{
    [Fact]
    public void ToMaybe_ShouldReturnWithValue_WhenHasNonNullFirst()
    {
        var value = Fixture.CreateNotDefault<T1>();

        var sut = (Either<T1, T2>)value;

        var result = sut.ToMaybe();

        using ( new AssertionScope() )
        {
            result.HasValue.Should().BeTrue();
            result.Value.Should().Be( value );
        }
    }

    [Fact]
    public void ToMaybe_ShouldReturnWithoutValue_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();

        var sut = (Either<T1, T2>)value;

        var result = sut.ToMaybe();

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void ToUnsafe_ShouldReturnOk_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();

        var sut = (Either<T1, Exception>)value;

        var result = sut.ToUnsafe();

        using ( new AssertionScope() )
        {
            result.IsOk.Should().BeTrue();
            result.Value.Should().Be( value );
        }
    }

    [Fact]
    public void ToUnsafe_ShouldReturnWithError_WhenHasSecond()
    {
        var error = new Exception();

        var sut = (Either<T1, Exception>)error;

        var result = sut.ToUnsafe();

        using ( new AssertionScope() )
        {
            result.HasError.Should().BeTrue();
            result.Error.Should().Be( error );
        }
    }

    [Fact]
    public void Reduce_WithEitherAndEither_ShouldReturnCorrectResult_WhenHasFirstWithFirst()
    {
        var value = Fixture.Create<T1>();
        var first = (Either<T1, T2>)value;
        var sut = first.ToEither().WithSecond<Either<T1, T2>>();

        var result = sut.Reduce();

        using ( new AssertionScope() )
        {
            result.HasFirst.Should().BeTrue();
            result.First.Should().Be( value );
        }
    }

    [Fact]
    public void Reduce_WithEitherAndEither_ShouldReturnCorrectResult_WhenHasFirstWithSecond()
    {
        var value = Fixture.Create<T2>();
        var first = (Either<T1, T2>)value;
        var sut = first.ToEither().WithSecond<Either<T1, T2>>();

        var result = sut.Reduce();

        using ( new AssertionScope() )
        {
            result.HasSecond.Should().BeTrue();
            result.Second.Should().Be( value );
        }
    }

    [Fact]
    public void Reduce_WithEitherAndEither_ShouldReturnCorrectResult_WhenHasSecondWithFirst()
    {
        var value = Fixture.Create<T1>();
        var first = (Either<T1, T2>)value;
        var sut = first.ToEither().WithFirst<Either<T1, T2>>();

        var result = sut.Reduce();

        using ( new AssertionScope() )
        {
            result.HasFirst.Should().BeTrue();
            result.First.Should().Be( value );
        }
    }

    [Fact]
    public void Reduce_WithEitherAndEither_ShouldReturnCorrectResult_WhenHasSecondWithSecond()
    {
        var value = Fixture.Create<T2>();
        var first = (Either<T1, T2>)value;
        var sut = first.ToEither().WithFirst<Either<T1, T2>>();

        var result = sut.Reduce();

        using ( new AssertionScope() )
        {
            result.HasSecond.Should().BeTrue();
            result.Second.Should().Be( value );
        }
    }

    [Fact]
    public void Reduce_WithEitherAndT1_ShouldReturnCorrectResult_WhenHasFirstWithFirst()
    {
        var value = Fixture.Create<T1>();
        var first = (Either<T1, T2>)value;
        var sut = first.ToEither().WithSecond<T1>();

        var result = sut.Reduce();

        using ( new AssertionScope() )
        {
            result.HasFirst.Should().BeTrue();
            result.First.Should().Be( value );
        }
    }

    [Fact]
    public void Reduce_WithEitherAndT1_ShouldReturnCorrectResult_WhenHasFirstWithSecond()
    {
        var value = Fixture.Create<T2>();
        var first = (Either<T1, T2>)value;
        var sut = first.ToEither().WithSecond<T1>();

        var result = sut.Reduce();

        using ( new AssertionScope() )
        {
            result.HasSecond.Should().BeTrue();
            result.Second.Should().Be( value );
        }
    }

    [Fact]
    public void Reduce_WithEitherAndT1_ShouldReturnCorrectResult_WhenHasSecond()
    {
        var value = Fixture.Create<T1>();
        var sut = value.ToEither().WithFirst<Either<T1, T2>>();

        var result = sut.Reduce();

        using ( new AssertionScope() )
        {
            result.HasFirst.Should().BeTrue();
            result.First.Should().Be( value );
        }
    }

    [Fact]
    public void Reduce_WithEitherAndT2_ShouldReturnCorrectResult_WhenHasFirstWithFirst()
    {
        var value = Fixture.Create<T1>();
        var first = (Either<T1, T2>)value;
        var sut = first.ToEither().WithSecond<T2>();

        var result = sut.Reduce();

        using ( new AssertionScope() )
        {
            result.HasFirst.Should().BeTrue();
            result.First.Should().Be( value );
        }
    }

    [Fact]
    public void Reduce_WithEitherAndT2_ShouldReturnCorrectResult_WhenHasFirstWithSecond()
    {
        var value = Fixture.Create<T2>();
        var first = (Either<T1, T2>)value;
        var sut = first.ToEither().WithSecond<T2>();

        var result = sut.Reduce();

        using ( new AssertionScope() )
        {
            result.HasSecond.Should().BeTrue();
            result.Second.Should().Be( value );
        }
    }

    [Fact]
    public void Reduce_WithEitherAndT2_ShouldReturnCorrectResult_WhenHasSecond()
    {
        var value = Fixture.Create<T2>();
        var sut = value.ToEither().WithFirst<Either<T1, T2>>();

        var result = sut.Reduce();

        using ( new AssertionScope() )
        {
            result.HasSecond.Should().BeTrue();
            result.Second.Should().Be( value );
        }
    }

    [Fact]
    public void Reduce_WithT1AndEither_ShouldReturnCorrectResult_WhenHasFirst()
    {
        var value = Fixture.Create<T1>();
        var sut = value.ToEither().WithSecond<Either<T1, T2>>();

        var result = sut.Reduce();

        using ( new AssertionScope() )
        {
            result.HasFirst.Should().BeTrue();
            result.First.Should().Be( value );
        }
    }

    [Fact]
    public void Reduce_WithT1AndEither_ShouldReturnCorrectResult_WhenHasSecondWithFirst()
    {
        var value = Fixture.Create<T1>();
        var second = (Either<T1, T2>)value;
        var sut = second.ToEither().WithFirst<T1>();

        var result = sut.Reduce();

        using ( new AssertionScope() )
        {
            result.HasFirst.Should().BeTrue();
            result.First.Should().Be( value );
        }
    }

    [Fact]
    public void Reduce_WithT1AndEither_ShouldReturnCorrectResult_WhenHasSecondWithSecond()
    {
        var value = Fixture.Create<T2>();
        var second = (Either<T1, T2>)value;
        var sut = second.ToEither().WithFirst<T1>();

        var result = sut.Reduce();

        using ( new AssertionScope() )
        {
            result.HasSecond.Should().BeTrue();
            result.Second.Should().Be( value );
        }
    }

    [Fact]
    public void Reduce_WithT2AndEither_ShouldReturnCorrectResult_WhenHasFirst()
    {
        var value = Fixture.Create<T2>();
        var sut = value.ToEither().WithSecond<Either<T1, T2>>();

        var result = sut.Reduce();

        using ( new AssertionScope() )
        {
            result.HasSecond.Should().BeTrue();
            result.Second.Should().Be( value );
        }
    }

    [Fact]
    public void Reduce_WithT2AndEither_ShouldReturnCorrectResult_WhenHasSecondWithFirst()
    {
        var value = Fixture.Create<T1>();
        var second = (Either<T1, T2>)value;
        var sut = second.ToEither().WithFirst<T2>();

        var result = sut.Reduce();

        using ( new AssertionScope() )
        {
            result.HasFirst.Should().BeTrue();
            result.First.Should().Be( value );
        }
    }

    [Fact]
    public void Reduce_WithT2AndEither_ShouldReturnCorrectResult_WhenHasSecondWithSecond()
    {
        var value = Fixture.Create<T2>();
        var second = (Either<T1, T2>)value;
        var sut = second.ToEither().WithFirst<T2>();

        var result = sut.Reduce();

        using ( new AssertionScope() )
        {
            result.HasSecond.Should().BeTrue();
            result.Second.Should().Be( value );
        }
    }
}