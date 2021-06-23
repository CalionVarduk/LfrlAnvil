using System;
using System.Linq;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests.Assert
{
    public abstract class AssertTestsNullableStruct<T> : AssertTests<T?>
        where T : struct
    {
        [Fact]
        public void IsNull_ShouldPass_WhenParamIsNull()
        {
            var param = Fixture.CreateDefault<T?>();
            ShouldPass( () => Common.Assert.IsNull( param ) );
        }

        [Fact]
        public void IsNull_ShouldPass_WhenParamIsNull_WithExplicitComparer()
        {
            IsNull_ShouldPass_WhenParamIsNull_WithExplicitComparer_Impl();
        }

        [Fact]
        public void IsNull_ShouldThrow_WhenParamIsNotNull()
        {
            var param = Fixture.CreateNullable<T>();
            ShouldThrow( () => Common.Assert.IsNull( param ) );
        }

        [Fact]
        public void IsNull_ShouldThrow_WhenParamIsNotNull_WithExplicitComparer()
        {
            IsNull_ShouldThrow_WhenParamIsNotNull_WithExplicitComparer_Impl();
        }

        [Fact]
        public void IsNotNull_ShouldPass_WhenParamIsNotNull()
        {
            var param = Fixture.CreateNullable<T>();
            ShouldPass( () => Common.Assert.IsNotNull( param ) );
        }

        [Fact]
        public void IsNotNull_ShouldPass_WhenParamIsNotNull_WithExplicitComparer()
        {
            IsNotNull_ShouldPass_WhenParamIsNotNull_WithExplicitComparer_Impl();
        }

        [Fact]
        public void IsNotNull_ShouldThrow_WhenParamIsNull()
        {
            var param = Fixture.CreateDefault<T?>();
            ShouldThrow<ArgumentNullException>( () => Common.Assert.IsNotNull( param ) );
        }

        [Fact]
        public void IsNotNull_ShouldThrow_WhenParamIsNull_WithExplicitComparer()
        {
            IsNotNull_ShouldThrow_WhenParamIsNull_WithExplicitComparer_Impl();
        }

        [Fact]
        public void IsDefault_ShouldThrow_WhenParamHasDefaultUnderlyingValue()
        {
            var param = Fixture.CreateDefaultNullable<T>();
            ShouldThrow( () => Common.Assert.IsDefault( param ) );
        }

        [Fact]
        public void IsNotDefault_ShouldPass_WhenParamHasDefaultUnderlyingValue()
        {
            var param = Fixture.CreateDefaultNullable<T>();
            ShouldPass( () => Common.Assert.IsNotDefault( param ) );
        }

        [Fact]
        public void IsOfType_ShouldThrow_WhenUnderlyingValueIsNotNull()
        {
            var param = Fixture.CreateNullable<T>()!;
            ShouldThrow( () => Common.Assert.IsOfType<T?>( param ) );
        }

        [Fact]
        public void IsOfType_ShouldPass_WhenUnderlyingValueIsNotNullAndWithComparisonToUnderlyingType()
        {
            var param = Fixture.CreateNullable<T>()!;
            ShouldPass( () => Common.Assert.IsOfType<T>( param ) );
        }

        [Fact]
        public void IsNotOfType_ShouldPass_WhenUnderlyingValueIsNotNull()
        {
            var param = Fixture.CreateNullable<T>()!;
            ShouldPass( () => Common.Assert.IsNotOfType<T?>( param ) );
        }

        [Fact]
        public void IsNotOfType_ShouldThrow_WhenUnderlyingValueIsNotNullAndWithComparisonToUnderlyingType()
        {
            var param = Fixture.CreateNullable<T>()!;
            ShouldThrow( () => Common.Assert.IsNotOfType<T>( param ) );
        }

        [Fact]
        public void ContainsNull_ShouldPass_WhenEnumerableContainsNullElement()
        {
            var param = Fixture.CreateMany<T?>().Append( Fixture.CreateDefault<T?>() );
            ShouldPass( () => Common.Assert.ContainsNull( param ) );
        }

        [Fact]
        public void ContainsNull_ShouldPass_WhenEnumerableContainsNullElement_WithExplicitComparer()
        {
            var param = Fixture.CreateMany<T?>().Append( Fixture.CreateDefault<T?>() );
            ShouldPass( () => Common.Assert.ContainsNull( param, EqualityComparer ) );
        }

        [Fact]
        public void ContainsNull_ShouldThrow_WhenEnumerableDoesntContainNullElement()
        {
            var param = Fixture.CreateMany<T?>();
            ShouldThrow( () => Common.Assert.ContainsNull( param ) );
        }

        [Fact]
        public void ContainsNull_ShouldThrow_WhenEnumerableDoesntContainNullElement_WithExplicitComparer()
        {
            var param = Fixture.CreateMany<T?>();
            ShouldThrow( () => Common.Assert.ContainsNull( param, EqualityComparer ) );
        }

        [Fact]
        public void NotContainsNull_ShouldPass_WhenEnumerableDoesntContainNullElement()
        {
            var param = Fixture.CreateMany<T?>();
            ShouldPass( () => Common.Assert.NotContainsNull( param ) );
        }

        [Fact]
        public void NotContainsNull_ShouldPass_WhenEnumerableDoesntContainNullElement_WithExplicitComparer()
        {
            var param = Fixture.CreateMany<T?>();
            ShouldPass( () => Common.Assert.NotContainsNull( param, EqualityComparer ) );
        }

        [Fact]
        public void NotContainsNull_ShouldThrow_WhenEnumerableContainsNullElement()
        {
            var param = Fixture.CreateMany<T?>().Append( Fixture.CreateDefault<T?>() );
            ShouldThrow( () => Common.Assert.NotContainsNull( param ) );
        }

        [Fact]
        public void NotContainsNull_ShouldThrow_WhenEnumerableContainsNullElement_WithExplicitComparer()
        {
            var param = Fixture.CreateMany<T?>().Append( Fixture.CreateDefault<T?>() );
            ShouldThrow( () => Common.Assert.NotContainsNull( param, EqualityComparer ) );
        }
    }
}
