using System;
using System.Linq;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Assert
{
    public abstract class AssertTestsNullableStruct<T> : AssertTests<T?>
        where T : struct
    {
        [Fact]
        public void IsNull_ShouldPass_WhenParamIsNull()
        {
            var param = Fixture.CreateDefault<T?>();
            ShouldPass( () => Core.Assert.IsNull( param ) );
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
            ShouldThrow( () => Core.Assert.IsNull( param ) );
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
            ShouldPass( () => Core.Assert.IsNotNull( param ) );
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
            ShouldThrow<ArgumentNullException>( () => Core.Assert.IsNotNull( param ) );
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
            ShouldThrow( () => Core.Assert.IsDefault( param ) );
        }

        [Fact]
        public void IsNotDefault_ShouldPass_WhenParamHasDefaultUnderlyingValue()
        {
            var param = Fixture.CreateDefaultNullable<T>();
            ShouldPass( () => Core.Assert.IsNotDefault( param ) );
        }

        [Fact]
        public void IsOfType_ShouldThrow_WhenUnderlyingValueIsNotNull()
        {
            var param = Fixture.CreateNullable<T>()!;
            ShouldThrow( () => Core.Assert.IsOfType<T?>( param ) );
        }

        [Fact]
        public void IsOfType_ShouldPass_WhenUnderlyingValueIsNotNullAndWithComparisonToUnderlyingType()
        {
            var param = Fixture.CreateNullable<T>()!;
            ShouldPass( () => Core.Assert.IsOfType<T>( param ) );
        }

        [Fact]
        public void IsNotOfType_ShouldPass_WhenUnderlyingValueIsNotNull()
        {
            var param = Fixture.CreateNullable<T>()!;
            ShouldPass( () => Core.Assert.IsNotOfType<T?>( param ) );
        }

        [Fact]
        public void IsNotOfType_ShouldThrow_WhenUnderlyingValueIsNotNullAndWithComparisonToUnderlyingType()
        {
            var param = Fixture.CreateNullable<T>()!;
            ShouldThrow( () => Core.Assert.IsNotOfType<T>( param ) );
        }

        [Fact]
        public void ContainsNull_ShouldPass_WhenEnumerableContainsNullElement()
        {
            var param = Fixture.CreateMany<T?>().Append( Fixture.CreateDefault<T?>() );
            ShouldPass( () => Core.Assert.ContainsNull( param ) );
        }

        [Fact]
        public void ContainsNull_ShouldPass_WhenEnumerableContainsNullElement_WithExplicitComparer()
        {
            var param = Fixture.CreateMany<T?>().Append( Fixture.CreateDefault<T?>() );
            ShouldPass( () => Core.Assert.ContainsNull( param, EqualityComparer ) );
        }

        [Fact]
        public void ContainsNull_ShouldThrow_WhenEnumerableDoesntContainNullElement()
        {
            var param = Fixture.CreateMany<T?>();
            ShouldThrow( () => Core.Assert.ContainsNull( param ) );
        }

        [Fact]
        public void ContainsNull_ShouldThrow_WhenEnumerableDoesntContainNullElement_WithExplicitComparer()
        {
            var param = Fixture.CreateMany<T?>();
            ShouldThrow( () => Core.Assert.ContainsNull( param, EqualityComparer ) );
        }

        [Fact]
        public void NotContainsNull_ShouldPass_WhenEnumerableDoesntContainNullElement()
        {
            var param = Fixture.CreateMany<T?>();
            ShouldPass( () => Core.Assert.NotContainsNull( param ) );
        }

        [Fact]
        public void NotContainsNull_ShouldPass_WhenEnumerableDoesntContainNullElement_WithExplicitComparer()
        {
            var param = Fixture.CreateMany<T?>();
            ShouldPass( () => Core.Assert.NotContainsNull( param, EqualityComparer ) );
        }

        [Fact]
        public void NotContainsNull_ShouldThrow_WhenEnumerableContainsNullElement()
        {
            var param = Fixture.CreateMany<T?>().Append( Fixture.CreateDefault<T?>() );
            ShouldThrow( () => Core.Assert.NotContainsNull( param ) );
        }

        [Fact]
        public void NotContainsNull_ShouldThrow_WhenEnumerableContainsNullElement_WithExplicitComparer()
        {
            var param = Fixture.CreateMany<T?>().Append( Fixture.CreateDefault<T?>() );
            ShouldThrow( () => Core.Assert.NotContainsNull( param, EqualityComparer ) );
        }
    }
}
