using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using LfrlAnvil.Extensions;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Tests.ExtensionsTests.FuncTests
{
    public abstract class GenericParameterlessFuncExtensionsTests<TReturnValue> : TestsBase
    {
        [Fact]
        public void ToLazy_ShouldReturnCorrectResult()
        {
            var value = Fixture.Create<TReturnValue>();
            Func<TReturnValue> sut = () => value;

            var result = sut.ToLazy();

            result.Value.Should().Be( value );
        }

        [Fact]
        public void Memoize_ShouldMaterializeSourceAfterFirstEnumeration()
        {
            var iterationCount = 5;
            var sourceCount = 3;
            var @delegate = Substitute.For<Func<int, TReturnValue>>().WithAnyArgs( _ => Fixture.Create<TReturnValue>() );

            Func<IEnumerable<TReturnValue>> sut = () =>
                Enumerable.Range( 0, sourceCount ).Select( @delegate );

            var result = sut.Memoize();

            var materialized = new List<IEnumerable<TReturnValue>>();
            for ( var i = 0; i < iterationCount; ++i )
                materialized.Add( result.ToList() );

            @delegate.Verify().CallCount.Should().Be( sourceCount );
        }

        [Fact]
        public void IgnoreResult_ShouldReturnCorrectResult()
        {
            var value = Fixture.Create<TReturnValue>();
            var sut = Substitute.For<Func<TReturnValue>>().WithAnyArgs( _ => value );

            var result = sut.IgnoreResult();
            result();

            sut.Verify().CallCount.Should().Be( 1 );
        }
    }
}
