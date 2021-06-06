using System;
using AutoFixture;
using LfrlSoft.NET.Common.Tests.Extensions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests.Assert
{
    public abstract class AssertTestsStruct<T> : AssertComparableTests<T>
        where T : struct, IEquatable<T>, IComparable<T>
    {
        [Fact]
        public void IsNull_ShouldThrow()
        {
            var param = Fixture.Create<T>();
            ShouldThrow( () => Common.Assert.IsNull( param, EqualityComparer ) );
        }

        [Fact]
        public void IsNotNull_ShouldPass()
        {
            var param = Fixture.Create<T>();
            ShouldPass( () => Common.Assert.IsNotNull( param, EqualityComparer ) );
        }

        [Fact]
        public void IsOfType_ShouldPass_WhenTypesMatch()
        {
            var param = Fixture.Create<T>();
            ShouldPass( () => Common.Assert.IsOfType<T>( param ) );
        }

        [Fact]
        public void IsNotOfType_ShouldThrow_WhenTypesMatch()
        {
            var param = Fixture.Create<T>();
            ShouldThrow( () => Common.Assert.IsNotOfType<T>( param ) );
        }

        [Fact]
        public void ContainsNull_ShouldThrow()
        {
            var param = Fixture.CreateMany<T>();
            ShouldThrow( () => Common.Assert.ContainsNull( param, EqualityComparer ) );
        }

        [Fact]
        public void NotContainsNull_ShouldPass()
        {
            var param = Fixture.CreateMany<T>();
            ShouldPass( () => Common.Assert.NotContainsNull( param, EqualityComparer ) );
        }
    }
}
