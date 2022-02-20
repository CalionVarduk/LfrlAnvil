﻿using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlSoft.NET.Core.Functional;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.FunctionalTests.TypeCastTests
{
    public abstract class GenericInvalidPartialTypeCastTests<TSource, TDestination> : GenericPartialTypeCastTests<TSource>
    {
        [Fact]
        public void To_ShouldReturnCorrectTypeCast()
        {
            var value = Fixture.Create<TSource>();

            var sut = new PartialTypeCast<TSource>( value );

            var result = sut.To<TDestination>();

            using ( new AssertionScope() )
            {
                result.IsValid.Should().BeFalse();
                result.IsInvalid.Should().BeTrue();
                result.Source.Should().Be( value );
                result.Result.Should().Be( default( TDestination ) );
            }
        }
    }
}