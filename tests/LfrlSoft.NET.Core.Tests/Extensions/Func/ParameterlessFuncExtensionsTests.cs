using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.Core.Extensions;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Extensions.Func
{
    public abstract class ParameterlessFuncExtensionsTests<TReturnValue> : TestsBase
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
            var callCount = 0;

            Func<IEnumerable<TReturnValue>> sut = () =>
                System.Linq.Enumerable.Range( 0, sourceCount )
                    .Select(
                        _ =>
                        {
                            ++callCount;
                            return Fixture.Create<TReturnValue>();
                        } );

            var result = sut.Memoize();

            var materialized = new List<IEnumerable<TReturnValue>>();
            for ( var i = 0; i < iterationCount; ++i )
                materialized.Add( result.ToList() );

            callCount.Should().Be( sourceCount );
        }
    }
}
