﻿using LfrlAnvil.Functional.Extensions;

namespace LfrlAnvil.Functional.Tests.ExtensionsTests.EitherTests;

public abstract class GenericEitherExtensionsOfRefTypeTests<T1, T2> : GenericEitherExtensionsTests<T1, T2>
    where T1 : class
{
    [Fact]
    public void ToMaybe_ShouldReturnWithoutValue_WhenHasNullFirst()
    {
        var value = default( T1 );
        var sut = ( Either<T1, T2> )value!;

        var result = sut.ToMaybe();

        result.HasValue.Should().BeFalse();
    }
}
