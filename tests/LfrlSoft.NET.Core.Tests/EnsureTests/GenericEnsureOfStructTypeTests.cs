using System;
using AutoFixture;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.EnsureTests
{
    public abstract class GenericEnsureOfStructTypeTests<T> : GenericEnsureOfComparableTypeTests<T>
        where T : struct, IEquatable<T>, IComparable<T>
    {
        [Fact]
        public void IsNull_ShouldThrowArgumentException()
        {
            var param = Fixture.Create<T>();
            ShouldThrowArgumentException( () => Ensure.IsNull( param, EqualityComparer ) );
        }

        [Fact]
        public void IsNotNull_ShouldPass()
        {
            var param = Fixture.Create<T>();
            ShouldPass( () => Ensure.IsNotNull( param, EqualityComparer ) );
        }

        [Fact]
        public void IsOfType_ShouldPass_WhenTypesMatch()
        {
            var param = Fixture.Create<T>();
            ShouldPass( () => Ensure.IsOfType<T>( param ) );
        }

        [Fact]
        public void IsNotOfType_ShouldThrowArgumentException_WhenTypesMatch()
        {
            var param = Fixture.Create<T>();
            ShouldThrowArgumentException( () => Ensure.IsNotOfType<T>( param ) );
        }

        [Fact]
        public void ContainsNull_ShouldThrowArgumentException()
        {
            var param = Fixture.CreateMany<T>();
            ShouldThrowArgumentException( () => Ensure.ContainsNull( param, EqualityComparer ) );
        }

        [Fact]
        public void NotContainsNull_ShouldPass()
        {
            var param = Fixture.CreateMany<T>();
            ShouldPass( () => Ensure.NotContainsNull( param, EqualityComparer ) );
        }
    }
}
