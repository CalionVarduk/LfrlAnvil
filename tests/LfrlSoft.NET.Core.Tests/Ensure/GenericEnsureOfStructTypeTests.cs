using System;
using AutoFixture;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Ensure
{
    public abstract class GenericEnsureOfStructTypeTests<T> : GenericEnsureOfComparableTypeTests<T>
        where T : struct, IEquatable<T>, IComparable<T>
    {
        [Fact]
        public void IsNull_ShouldThrow()
        {
            var param = Fixture.Create<T>();
            ShouldThrow( () => Core.Ensure.IsNull( param, EqualityComparer ) );
        }

        [Fact]
        public void IsNotNull_ShouldPass()
        {
            var param = Fixture.Create<T>();
            ShouldPass( () => Core.Ensure.IsNotNull( param, EqualityComparer ) );
        }

        [Fact]
        public void IsOfType_ShouldPass_WhenTypesMatch()
        {
            var param = Fixture.Create<T>();
            ShouldPass( () => Core.Ensure.IsOfType<T>( param ) );
        }

        [Fact]
        public void IsNotOfType_ShouldThrow_WhenTypesMatch()
        {
            var param = Fixture.Create<T>();
            ShouldThrow( () => Core.Ensure.IsNotOfType<T>( param ) );
        }

        [Fact]
        public void ContainsNull_ShouldThrow()
        {
            var param = Fixture.CreateMany<T>();
            ShouldThrow( () => Core.Ensure.ContainsNull( param, EqualityComparer ) );
        }

        [Fact]
        public void NotContainsNull_ShouldPass()
        {
            var param = Fixture.CreateMany<T>();
            ShouldPass( () => Core.Ensure.NotContainsNull( param, EqualityComparer ) );
        }
    }
}
