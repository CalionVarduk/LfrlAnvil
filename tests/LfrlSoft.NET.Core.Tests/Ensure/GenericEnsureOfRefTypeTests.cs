using System;
using System.Linq;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Ensure
{
    public abstract class GenericEnsureOfRefTypeTests<T> : GenericEnsureOfComparableTypeTests<T>
        where T : class, IEquatable<T>, IComparable<T>
    {
        [Fact]
        public void IsNull_ShouldPass_WhenParamIsNull()
        {
            var param = Fixture.CreateDefault<T>();
            ShouldPass( () => Core.Ensure.IsNull( param ) );
        }

        [Fact]
        public void IsNull_ShouldPass_WhenParamIsNull_WithExplicitComparer()
        {
            IsNull_ShouldPass_WhenParamIsNull_WithExplicitComparer_Impl();
        }

        [Fact]
        public void IsNull_ShouldThrowArgumentException_WhenParamIsNotNull()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrowArgumentException( () => Core.Ensure.IsNull( param ) );
        }

        [Fact]
        public void IsNull_ShouldThrowArgumentException_WhenParamIsNotNull_WithExplicitComparer()
        {
            IsNull_ShouldThrowArgumentException_WhenParamIsNotNull_WithExplicitComparer_Impl();
        }

        [Fact]
        public void IsNotNull_ShouldPass_WhenParamIsNotNull()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Core.Ensure.IsNotNull( param ) );
        }

        [Fact]
        public void IsNotNull_ShouldPass_WhenParamIsNotNull_WithExplicitComparer()
        {
            IsNotNull_ShouldPass_WhenParamIsNotNull_WithExplicitComparer_Impl();
        }

        [Fact]
        public void IsNotNull_ShouldThrowArgumentNullException_WhenParamIsNull()
        {
            var param = Fixture.CreateDefault<T>();
            ShouldThrowExactly<ArgumentNullException>( () => Core.Ensure.IsNotNull( param ) );
        }

        [Fact]
        public void IsNotNull_ShouldThrowArgumentNullException_WhenParamIsNull_WithExplicitComparer()
        {
            IsNotNull_ShouldThrowArgumentNullException_WhenParamIsNull_WithExplicitComparer_Impl();
        }

        [Fact]
        public void IsOfType_ShouldPass_WhenTypesMatch()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Core.Ensure.IsOfType<T>( param ) );
        }

        [Fact]
        public void IsNotOfType_ShouldThrowArgumentException_WhenTypesMatch()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrowArgumentException( () => Core.Ensure.IsNotOfType<T>( param ) );
        }

        [Fact]
        public void ContainsNull_ShouldPass_WhenEnumerableContainsNullElement()
        {
            var param = Fixture.CreateMany<T>().Append( Fixture.CreateDefault<T>() );
            ShouldPass( () => Core.Ensure.ContainsNull( param ) );
        }

        [Fact]
        public void ContainsNull_ShouldPass_WhenEnumerableContainsNullElement_WithExplicitComparer()
        {
            var param = Fixture.CreateMany<T>().Append( Fixture.CreateDefault<T>() );
            ShouldPass( () => Core.Ensure.ContainsNull( param, EqualityComparer ) );
        }

        [Fact]
        public void ContainsNull_ShouldThrowArgumentException_WhenEnumerableDoesntContainNullElement()
        {
            var param = Fixture.CreateMany<T>();
            ShouldThrowArgumentException( () => Core.Ensure.ContainsNull( param ) );
        }

        [Fact]
        public void ContainsNull_ShouldThrowArgumentException_WhenEnumerableDoesntContainNullElement_WithExplicitComparer()
        {
            var param = Fixture.CreateMany<T>();
            ShouldThrowArgumentException( () => Core.Ensure.ContainsNull( param, EqualityComparer ) );
        }

        [Fact]
        public void NotContainsNull_ShouldPass_WhenEnumerableDoesntContainNullElement()
        {
            var param = Fixture.CreateMany<T>();
            ShouldPass( () => Core.Ensure.NotContainsNull( param ) );
        }

        [Fact]
        public void NotContainsNull_ShouldPass_WhenEnumerableDoesntContainNullElement_WithExplicitComparer()
        {
            var param = Fixture.CreateMany<T>();
            ShouldPass( () => Core.Ensure.NotContainsNull( param, EqualityComparer ) );
        }

        [Fact]
        public void NotContainsNull_ShouldThrowArgumentException_WhenEnumerableContainsNullElement()
        {
            var param = Fixture.CreateMany<T>().Append( Fixture.CreateDefault<T>() );
            ShouldThrowArgumentException( () => Core.Ensure.NotContainsNull( param ) );
        }

        [Fact]
        public void NotContainsNull_ShouldThrowArgumentException_WhenEnumerableContainsNullElement_WithExplicitComparer()
        {
            var param = Fixture.CreateMany<T>().Append( Fixture.CreateDefault<T>() );
            ShouldThrowArgumentException( () => Core.Ensure.NotContainsNull( param, EqualityComparer ) );
        }
    }
}
