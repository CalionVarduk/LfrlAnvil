using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.Common.Tests.Extensions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests.Equality
{
    public abstract class EqualityTestsRef<T> : EqualityTests<T>
        where T : class
    {
        [Theory]
        [GenericMethodData( nameof( CreateCtorNullTestData ) )]
        public void Ctor_ShouldNotThrowWhenValueIsNull(T? first, T? second, bool expected)
        {
            var sut = new Equality<T>( first, second );

            sut.Should()
                .BeEquivalentTo(
                    new
                    {
                        First = first,
                        Second = second,
                        Result = expected
                    } );
        }

        public static IEnumerable<object?[]> CreateCtorNullTestData(IFixture fixture)
        {
            var result = new List<object?[]>();
            var value = fixture.CreateNotDefault<T>();

            result.Add( new object?[] { null, value, false } );
            result.Add( new object?[] { value, null, false } );
            result.Add( new object?[] { null, null, true } );

            return result;
        }
    }
}
