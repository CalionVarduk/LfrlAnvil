using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlSoft.NET.Core.Extensions;
using LfrlSoft.NET.TestExtensions;
using LfrlSoft.NET.TestExtensions.FluentAssertions;
using LfrlSoft.NET.TestExtensions.NSubstitute;
using NSubstitute;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Extensions.Func
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
                System.Linq.Enumerable.Range( 0, sourceCount ).Select( @delegate );

            var result = sut.Memoize();

            var materialized = new List<IEnumerable<TReturnValue>>();
            for ( var i = 0; i < iterationCount; ++i )
                materialized.Add( result.ToList() );

            @delegate.Verify().CallCount.Should().Be( sourceCount );
        }

        [Fact]
        public void TryInvoke_ShouldReturnCorrectResult_WhenDelegateDoesntThrow()
        {
            var value = Fixture.Create<TReturnValue>();
            Func<TReturnValue> action = () => value;

            var result = action.TryInvoke();

            using ( new AssertionScope() )
            {
                result.IsOk.Should().BeTrue();
                result.Value.Should().Be( value );
            }
        }

        [Fact]
        public void Try_ShouldReturnCorrectResult_WhenDelegateThrows()
        {
            var error = new Exception();
            Func<TReturnValue> action = () => throw error;

            var result = action.TryInvoke();

            using ( new AssertionScope() )
            {
                result.HasError.Should().BeTrue();
                result.Error.Should().Be( error );
            }
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
