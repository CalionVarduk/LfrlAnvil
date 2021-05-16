using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.Common.Tests.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace LfrlSoft.NET.Common.Tests
{
    public class AssertTests
    {
        private readonly IFixture _fixture = new Fixture();
        private readonly EqualityComparer<string> _refEqComparer = EqualityComparer<string>.Default;
        private readonly EqualityComparer<int> _structEqComparer = EqualityComparer<int>.Default;
        private readonly EqualityComparer<int?> _nullStructEqComparer = EqualityComparer<int?>.Default;
        private readonly Comparer<int> _structComparer = Comparer<int>.Default;

        [Fact]
        public void IsNull_ShouldPass_WhenRefParamIsNull()
        {
            var param = _fixture.CreateDefault<string>();
            ShouldPass( () => Assert.IsNull( param ) );
        }

        [Fact]
        public void IsNull_ShouldPass_WhenStructParamIsNull()
        {
            var param = _fixture.CreateDefault<int?>();
            ShouldPass( () => Assert.IsNull( param ) );
        }

        [Fact]
        public void IsNull_ShouldPass_WhenRefParamIsNull_WithExplicitComparer()
        {
            var param = _fixture.CreateDefault<string>();
            ShouldPass( () => Assert.IsNull( param, _refEqComparer ) );
        }

        [Fact]
        public void IsNull_ShouldPass_WhenStructParamIsNull_WithExplicitComparer()
        {
            var param = _fixture.CreateDefault<int?>();
            ShouldPass( () => Assert.IsNull( param, _nullStructEqComparer ) );
        }

        [Fact]
        public void IsNull_ShouldThrow_WhenRefParamIsNotNull()
        {
            var param = _fixture.Create<string>();
            ShouldThrow( () => Assert.IsNull( param ) );
        }

        [Fact]
        public void IsNull_ShouldThrow_WhenStructParamIsNotNull()
        {
            var param = _fixture.CreateNullable<int>();
            ShouldThrow( () => Assert.IsNull( param ) );
        }

        [Fact]
        public void IsNull_ShouldThrow_WhenRefParamIsNotNull_WithExplicitComparer()
        {
            var param = _fixture.Create<string>();
            ShouldThrow( () => Assert.IsNull( param, _refEqComparer ) );
        }

        [Fact]
        public void IsNull_ShouldThrow_WhenStructParamIsNotNull_WithExplicitComparer()
        {
            var param = _fixture.CreateNullable<int>();
            ShouldThrow( () => Assert.IsNull( param, _nullStructEqComparer ) );
        }

        [Fact]
        public void IsNull_ShouldThrow_WhenStructParamIsNotNullable()
        {
            var param = _fixture.Create<int>();
            ShouldThrow( () => Assert.IsNull( param, _structEqComparer ) );
        }

        [Fact]
        public void IsNotNull_ShouldPass_WhenRefParamIsNotNull()
        {
            var param = _fixture.Create<string>();
            ShouldPass( () => Assert.IsNotNull( param ) );
        }

        [Fact]
        public void IsNotNull_ShouldPass_WhenStructParamIsNotNull()
        {
            var param = _fixture.CreateNullable<int>();
            ShouldPass( () => Assert.IsNotNull( param ) );
        }

        [Fact]
        public void IsNotNull_ShouldPass_WhenRefParamIsNotNull_WithExplicitComparer()
        {
            var param = _fixture.Create<string>();
            ShouldPass( () => Assert.IsNotNull( param, _refEqComparer ) );
        }

        [Fact]
        public void IsNotNull_ShouldPass_WhenStructParamIsNotNull_WithExplicitComparer()
        {
            var param = _fixture.CreateNullable<int>();
            ShouldPass( () => Assert.IsNotNull( param, _nullStructEqComparer ) );
        }

        [Fact]
        public void IsNotNull_ShouldPass_WhenStructParamIsNotNullable()
        {
            var param = _fixture.Create<int>();
            ShouldPass( () => Assert.IsNotNull( param, _structEqComparer ) );
        }

        [Fact]
        public void IsNotNull_ShouldThrow_WhenRefParamIsNull()
        {
            var param = _fixture.CreateDefault<string>();
            ShouldThrow<ArgumentNullException>( () => Assert.IsNotNull( param ) );
        }

        [Fact]
        public void IsNotNull_ShouldThrow_WhenStructParamIsNull()
        {
            var param = _fixture.CreateDefault<int?>();
            ShouldThrow<ArgumentNullException>( () => Assert.IsNotNull( param ) );
        }

        [Fact]
        public void IsNotNull_ShouldThrow_WhenRefParamIsNull_WithExplicitComparer()
        {
            var param = _fixture.CreateDefault<string>();
            ShouldThrow<ArgumentNullException>( () => Assert.IsNotNull( param, _refEqComparer ) );
        }

        [Fact]
        public void IsNotNull_ShouldThrow_WhenStructParamIsNull_WithExplicitComparer()
        {
            var param = _fixture.CreateDefault<int?>();
            ShouldThrow<ArgumentNullException>( () => Assert.IsNotNull( param, _nullStructEqComparer ) );
        }

        [Fact]
        public void IsDefault_ShouldPass_WhenRefParamIsNull()
        {
            var param = _fixture.CreateDefault<string>();
            ShouldPass( () => Assert.IsDefault( param ) );
        }

        [Fact]
        public void IsDefault_ShouldPass_WhenNullableStructParamIsNull()
        {
            var param = _fixture.CreateDefault<int?>();
            ShouldPass( () => Assert.IsDefault( param ) );
        }

        [Fact]
        public void IsDefault_ShouldPass_WhenStructParamIsDefault()
        {
            var param = _fixture.CreateDefault<int>();
            ShouldPass( () => Assert.IsDefault( param ) );
        }

        [Fact]
        public void IsDefault_ShouldThrow_WhenRefParamIsNotNull()
        {
            var param = _fixture.Create<string>();
            ShouldThrow( () => Assert.IsDefault( param ) );
        }

        [Fact]
        public void IsDefault_ShouldThrow_WhenNullableStructParamIsNotNull()
        {
            var param = _fixture.CreateNullable<int>();
            ShouldThrow( () => Assert.IsDefault( param ) );
        }

        [Fact]
        public void IsDefault_ShouldThrow_WhenNullableStructParamHasDefaultValue()
        {
            var param = _fixture.CreateDefaultNullable<int>();
            ShouldThrow( () => Assert.IsDefault( param ) );
        }

        [Fact]
        public void IsDefault_ShouldThrow_WhenStructParamIsNotDefault()
        {
            var param = _fixture.Create<Generator<int>>().First( v => v != default );
            ShouldThrow( () => Assert.IsDefault( param ) );
        }

        [Fact]
        public void IsNotDefault_ShouldPass_WhenRefParamIsNotNull()
        {
            var param = _fixture.Create<string>();
            ShouldPass( () => Assert.IsNotDefault( param ) );
        }

        [Fact]
        public void IsNotDefault_ShouldPass_WhenNullableStructParamIsNotNull()
        {
            var param = _fixture.CreateNullable<int>();
            ShouldPass( () => Assert.IsNotDefault( param ) );
        }

        [Fact]
        public void IsNotDefault_ShouldPass_WhenStructParamIsNotDefault()
        {
            var param = _fixture.Create<Generator<int>>().First( v => v != default );
            ShouldPass( () => Assert.IsNotDefault( param ) );
        }

        [Fact]
        public void IsNotDefault_ShouldPass_WhenNullableStructParamHasDefaultValue()
        {
            var param = _fixture.CreateDefaultNullable<int>();
            ShouldPass( () => Assert.IsNotDefault( param ) );
        }

        [Fact]
        public void IsNotDefault_ShouldThrow_WhenRefParamIsNull()
        {
            var param = _fixture.CreateDefault<string>();
            ShouldThrow( () => Assert.IsNotDefault( param ) );
        }

        [Fact]
        public void IsNotDefault_ShouldThrow_WhenNullableStructParamIsNull()
        {
            var param = _fixture.CreateDefault<int?>();
            ShouldThrow( () => Assert.IsNotDefault( param ) );
        }

        [Fact]
        public void IsNotDefault_ShouldThrow_WhenStructParamIsDefault()
        {
            var param = _fixture.CreateDefault<int>();
            ShouldThrow( () => Assert.IsNotDefault( param ) );
        }

        [Fact]
        public void IsOfType_ShouldPass_WhenTypesMatch()
        {
            var param = _fixture.Create<string>();
            ShouldPass( () => Assert.IsOfType<string>( param ) );
        }

        [Fact]
        public void IsOfType_ShouldThrow_WhenTypesDontMatch()
        {
            var param = _fixture.Create<string>();
            ShouldThrow( () => Assert.IsOfType<IEnumerable<char>>( param ) );
        }

        [Fact]
        public void IsNotOfType_ShouldPass_WhenTypesDontMatch()
        {
            var param = _fixture.Create<string>();
            ShouldPass( () => Assert.IsNotOfType<IEnumerable<char>>( param ) );
        }

        [Fact]
        public void IsNotOfType_ShouldThrow_WhenTypesMatch()
        {
            var param = _fixture.Create<string>();
            ShouldThrow( () => Assert.IsNotOfType<string>( param ) );
        }

        [Fact]
        public void IsAssignableToType_ShouldPass_WhenParamIsAssignableToType()
        {
            var param = _fixture.Create<string>();
            ShouldPass( () => Assert.IsAssignableToType<IEnumerable<char>>( param ) );
        }

        [Fact]
        public void IsAssignableToType_ShouldPass_WhenParamIsOfExactType()
        {
            var param = _fixture.Create<string>();
            ShouldPass( () => Assert.IsAssignableToType<string>( param ) );
        }

        [Fact]
        public void IsAssignableToType_ShouldThrow_WhenParamIsNotAssignableToType()
        {
            var param = _fixture.Create<string>();
            ShouldThrow( () => Assert.IsAssignableToType<IEnumerable<string>>( param ) );
        }

        [Fact]
        public void IsNotAssignableToType_ShouldPass_WhenParamIsNotAssignableToType()
        {
            var param = _fixture.Create<string>();
            ShouldPass( () => Assert.IsNotAssignableToType<IEnumerable<string>>( param ) );
        }

        [Fact]
        public void IsNotAssignableToType_ShouldThrow_WhenParamIsOfExactType()
        {
            var param = _fixture.Create<string>();
            ShouldThrow( () => Assert.IsNotAssignableToType<string>( param ) );
        }

        [Fact]
        public void IsNotAssignableToType_ShouldThrow_WhenParamIsAssignableToType()
        {
            var param = _fixture.Create<string>();
            ShouldThrow( () => Assert.IsNotAssignableToType<IEnumerable<char>>( param ) );
        }

        [Fact]
        public void Equals_ShouldPass_WhenParamIsEqualToValue()
        {
            var param = _fixture.Create<int>();
            ShouldPass( () => Assert.Equals( param, param ) );
        }

        [Fact]
        public void Equals_ShouldPass_WhenParamIsEqualToValue_WithExplicitComparer()
        {
            var param = _fixture.Create<int>();
            ShouldPass( () => Assert.Equals( param, param, _structEqComparer ) );
        }

        [Fact]
        public void Equals_ShouldThrow_WhenParamIsNotEqualToValue()
        {
            var (param, value) = _fixture.CreateDistinctPair<int>();
            ShouldThrow( () => Assert.Equals( param, value ) );
        }

        [Fact]
        public void Equals_ShouldThrow_WhenParamIsNotEqualToValue_WithExplicitComparer()
        {
            var (param, value) = _fixture.CreateDistinctPair<int>();
            ShouldThrow( () => Assert.Equals( param, value, _structEqComparer ) );
        }

        [Fact]
        public void NotEquals_ShouldPass_WhenParamIsNotEqualToValue()
        {
            var (param, value) = _fixture.CreateDistinctPair<int>();
            ShouldPass( () => Assert.NotEquals( param, value ) );
        }

        [Fact]
        public void NotEquals_ShouldPass_WhenParamIsNotEqualToValue_WithExplicitComparer()
        {
            var (param, value) = _fixture.CreateDistinctPair<int>();
            ShouldPass( () => Assert.NotEquals( param, value, _structEqComparer ) );
        }

        [Fact]
        public void NotEquals_ShouldThrow_WhenParamIsEqualToValue()
        {
            var param = _fixture.Create<int>();
            ShouldThrow( () => Assert.NotEquals( param, param ) );
        }

        [Fact]
        public void NotEquals_ShouldThrow_WhenParamIsEqualToValue_WithExplicitComparer()
        {
            var param = _fixture.Create<int>();
            ShouldThrow( () => Assert.NotEquals( param, param, _structEqComparer ) );
        }

        [Fact]
        public void IsGreaterThan_ShouldPass_WhenParamIsGreaterThanValue()
        {
            var (value, param) = _fixture.CreateDistinctPair<int>();
            ShouldPass( () => Assert.IsGreaterThan( param, value ) );
        }

        [Fact]
        public void IsGreaterThan_ShouldPass_WhenParamIsGreaterThanValue_WithExplicitComparer()
        {
            var (value, param) = _fixture.CreateDistinctPair<int>();
            ShouldPass( () => Assert.IsGreaterThan( param, value, _structComparer ) );
        }

        [Fact]
        public void IsGreaterThan_ShouldThrow_WhenParamIsLessThanValue()
        {
            var (param, value) = _fixture.CreateDistinctPair<int>();
            ShouldThrow( () => Assert.IsGreaterThan( param, value ) );
        }

        [Fact]
        public void IsGreaterThan_ShouldThrow_WhenParamIsEqualToValue()
        {
            var param = _fixture.Create<int>();
            ShouldThrow( () => Assert.IsGreaterThan( param, param ) );
        }

        [Fact]
        public void IsGreaterThan_ShouldThrow_WhenParamIsLessThanValue_WithExplicitComparer()
        {
            var (param, value) = _fixture.CreateDistinctPair<int>();
            ShouldThrow( () => Assert.IsGreaterThan( param, value, _structComparer ) );
        }

        [Fact]
        public void IsGreaterThan_ShouldThrow_WhenParamIsEqualToValue_WithExplicitComparer()
        {
            var param = _fixture.Create<int>();
            ShouldThrow( () => Assert.IsGreaterThan( param, param, _structComparer ) );
        }

        [Fact]
        public void IsGreaterThanOrEqualTo_ShouldPass_WhenParamIsGreaterThanValue()
        {
            var (value, param) = _fixture.CreateDistinctPair<int>();
            ShouldPass( () => Assert.IsGreaterThanOrEqualTo( param, value ) );
        }

        [Fact]
        public void IsGreaterThanOrEqualTo_ShouldPass_WhenParamIsEqualToValue()
        {
            var param = _fixture.Create<int>();
            ShouldPass( () => Assert.IsGreaterThanOrEqualTo( param, param ) );
        }

        [Fact]
        public void IsGreaterThanOrEqualTo_ShouldPass_WhenParamIsGreaterThanValue_WithExplicitComparer()
        {
            var (value, param) = _fixture.CreateDistinctPair<int>();
            ShouldPass( () => Assert.IsGreaterThanOrEqualTo( param, value, _structComparer ) );
        }

        [Fact]
        public void IsGreaterThanOrEqualTo_ShouldPass_WhenParamIsEqualToValue_WithExplicitComparer()
        {
            var param = _fixture.Create<int>();
            ShouldPass( () => Assert.IsGreaterThanOrEqualTo( param, param, _structComparer ) );
        }

        [Fact]
        public void IsGreaterThanOrEqualTo_ShouldThrow_WhenParamIsLessThanValue()
        {
            var (param, value) = _fixture.CreateDistinctPair<int>();
            ShouldThrow( () => Assert.IsGreaterThanOrEqualTo( param, value ) );
        }

        [Fact]
        public void IsGreaterThanOrEqualTo_ShouldThrow_WhenParamIsLessThanValue_WithExplicitComparer()
        {
            var (param, value) = _fixture.CreateDistinctPair<int>();
            ShouldThrow( () => Assert.IsGreaterThanOrEqualTo( param, value, _structComparer ) );
        }

        [Fact]
        public void IsLessThan_ShouldPass_WhenParamIsLessThanValue()
        {
            var (param, value) = _fixture.CreateDistinctPair<int>();
            ShouldPass( () => Assert.IsLessThan( param, value ) );
        }

        [Fact]
        public void IsLessThan_ShouldPass_WhenParamIsLessThanValue_WithExplicitComparer()
        {
            var (param, value) = _fixture.CreateDistinctPair<int>();
            ShouldPass( () => Assert.IsLessThan( param, value, _structComparer ) );
        }

        [Fact]
        public void IsLessThan_ShouldThrow_WhenParamIsGreaterThanValue()
        {
            var (value, param) = _fixture.CreateDistinctPair<int>();
            ShouldThrow( () => Assert.IsLessThan( param, value ) );
        }

        [Fact]
        public void IsLessThan_ShouldThrow_WhenParamIsEqualToValue()
        {
            var param = _fixture.Create<int>();
            ShouldThrow( () => Assert.IsLessThan( param, param ) );
        }

        [Fact]
        public void IsLessThan_ShouldThrow_WhenParamIsGreaterThanValue_WithExplicitComparer()
        {
            var (value, param) = _fixture.CreateDistinctPair<int>();
            ShouldThrow( () => Assert.IsLessThan( param, value, _structComparer ) );
        }

        [Fact]
        public void IsLessThan_ShouldThrow_WhenParamIsEqualToValue_WithExplicitComparer()
        {
            var param = _fixture.Create<int>();
            ShouldThrow( () => Assert.IsLessThan( param, param, _structComparer ) );
        }

        [Fact]
        public void IsLessThanOrEqualTo_ShouldPass_WhenParamIsLessThanValue()
        {
            var (param, value) = _fixture.CreateDistinctPair<int>();
            ShouldPass( () => Assert.IsLessThanOrEqualTo( param, value ) );
        }

        [Fact]
        public void IsLessThanOrEqualTo_ShouldPass_WhenParamIsEqualToValue()
        {
            var param = _fixture.Create<int>();
            ShouldPass( () => Assert.IsLessThanOrEqualTo( param, param ) );
        }

        [Fact]
        public void IsLessThanOrEqualTo_ShouldPass_WhenParamIsLessThanValue_WithExplicitComparer()
        {
            var (param, value) = _fixture.CreateDistinctPair<int>();
            ShouldPass( () => Assert.IsLessThanOrEqualTo( param, value, _structComparer ) );
        }

        [Fact]
        public void IsLessThanOrEqualTo_ShouldPass_WhenParamIsEqualToValue_WithExplicitComparer()
        {
            var param = _fixture.Create<int>();
            ShouldPass( () => Assert.IsLessThanOrEqualTo( param, param, _structComparer ) );
        }

        [Fact]
        public void IsLessThanOrEqualTo_ShouldThrow_WhenParamIsGreaterThanValue()
        {
            var (value, param) = _fixture.CreateDistinctPair<int>();
            ShouldThrow( () => Assert.IsLessThanOrEqualTo( param, value ) );
        }

        [Fact]
        public void IsLessThanOrEqualTo_ShouldThrow_WhenParamIsGreaterThanValue_WithExplicitComparer()
        {
            var (value, param) = _fixture.CreateDistinctPair<int>();
            ShouldThrow( () => Assert.IsLessThanOrEqualTo( param, value, _structComparer ) );
        }

        [Fact]
        public void IsBetween_ShouldPass_WhenParamIsBetweenTwoDistinctValues()
        {
            var (min, param, max) = _fixture.CreateDistinctTriple<int>();
            ShouldPass( () => Assert.IsBetween( param, min, max ) );
        }

        [Fact]
        public void IsBetween_ShouldPass_WhenParamIsEqualToMinValue()
        {
            var (param, max) = _fixture.CreateDistinctPair<int>();
            ShouldPass( () => Assert.IsBetween( param, param, max ) );
        }

        [Fact]
        public void IsBetween_ShouldPass_WhenParamIsEqualToMaxValue()
        {
            var (min, param) = _fixture.CreateDistinctPair<int>();
            ShouldPass( () => Assert.IsBetween( param, min, param ) );
        }

        [Fact]
        public void IsBetween_ShouldPass_WhenParamIsEqualToMinAndMaxValues()
        {
            var param = _fixture.Create<int>();
            ShouldPass( () => Assert.IsBetween( param, param, param ) );
        }

        [Fact]
        public void IsBetween_ShouldThrow_WhenParamIsLessThanMinValue()
        {
            var (param, min, max) = _fixture.CreateDistinctTriple<int>();
            ShouldThrow( () => Assert.IsBetween( param, min, max ) );
        }

        [Fact]
        public void IsBetween_ShouldThrow_WhenParamIsGreaterThanMaxValue()
        {
            var (min, max, param) = _fixture.CreateDistinctTriple<int>();
            ShouldThrow( () => Assert.IsBetween( param, min, max ) );
        }

        [Fact]
        public void IsBetween_ShouldThrow_WhenMinIsGreaterThanMax()
        {
            var param = _fixture.Create<int>();
            var (max, min) = _fixture.CreateDistinctPair<int>();
            ShouldThrow( () => Assert.IsBetween( param, min, max ) );
        }

        [Fact]
        public void IsNotBetween_ShouldPass_WhenParamIsLessThanMinValue()
        {
            var (param, min, max) = _fixture.CreateDistinctTriple<int>();
            ShouldPass( () => Assert.IsNotBetween( param, min, max ) );
        }

        [Fact]
        public void IsNotBetween_ShouldPass_WhenParamIsGreaterThanMaxValue()
        {
            var (min, max, param) = _fixture.CreateDistinctTriple<int>();
            ShouldPass( () => Assert.IsNotBetween( param, min, max ) );
        }

        [Fact]
        public void IsNotBetween_ShouldPass_WhenMinIsGreaterThanMax()
        {
            var param = _fixture.Create<int>();
            var (max, min) = _fixture.CreateDistinctPair<int>();
            ShouldPass( () => Assert.IsNotBetween( param, min, max ) );
        }

        [Fact]
        public void IsNotBetween_ShouldThrow_WhenParamIsBetweenTwoDistinctValues()
        {
            var (min, param, max) = _fixture.CreateDistinctTriple<int>();
            ShouldThrow( () => Assert.IsNotBetween( param, min, max ) );
        }

        [Fact]
        public void IsNotBetween_ShouldThrow_WhenParamIsEqualToMinValue()
        {
            var (param, max) = _fixture.CreateDistinctPair<int>();
            ShouldThrow( () => Assert.IsNotBetween( param, param, max ) );
        }

        [Fact]
        public void IsNotBetween_ShouldThrow_WhenParamIsEqualToMaxValue()
        {
            var (min, param) = _fixture.CreateDistinctPair<int>();
            ShouldThrow( () => Assert.IsNotBetween( param, min, param ) );
        }

        [Fact]
        public void IsNotBetween_ShouldThrow_WhenParamIsEqualToMinAndMaxValues()
        {
            var param = _fixture.Create<int>();
            ShouldThrow( () => Assert.IsNotBetween( param, param, param ) );
        }

        [Fact]
        public void IsEmpty_ShouldPass_WhenEnumerableIsEmpty()
        {
            var param = Enumerable.Empty<int>();
            ShouldPass( () => Assert.IsEmpty( param ) );
        }

        [Fact]
        public void IsEmpty_ShouldPass_WhenCollectionIsEmpty()
        {
            var param = Array.Empty<int>();
            ShouldPass( () => Assert.IsEmpty( param ) );
        }

        [Fact]
        public void IsEmpty_ShouldPass_WhenStringIsEmpty()
        {
            var param = string.Empty;
            ShouldPass( () => Assert.IsEmpty( param ) );
        }

        [Fact]
        public void IsEmpty_ShouldThrow_WhenEnumerableIsNotEmpty()
        {
            var param = _fixture.CreateMany<int>();
            ShouldThrow( () => Assert.IsEmpty( param ) );
        }

        [Fact]
        public void IsEmpty_ShouldThrow_WhenCollectionIsNotEmpty()
        {
            var param = _fixture.CreateMany<int>().ToList();
            ShouldThrow( () => Assert.IsEmpty( param ) );
        }

        [Fact]
        public void IsEmpty_ShouldThrow_WhenStringIsNotEmpty()
        {
            var param = _fixture.Create<string>();
            ShouldThrow( () => Assert.IsEmpty( param ) );
        }

        [Fact]
        public void IsNotEmpty_ShouldPass_WhenEnumerableIsNotEmpty()
        {
            var param = _fixture.CreateMany<int>();
            ShouldPass( () => Assert.IsNotEmpty( param ) );
        }

        [Fact]
        public void IsNotEmpty_ShouldPass_WhenCollectionIsNotEmpty()
        {
            var param = _fixture.CreateMany<int>().ToList();
            ShouldPass( () => Assert.IsNotEmpty( param ) );
        }

        [Fact]
        public void IsNotEmpty_ShouldPass_WhenStringIsNotEmpty()
        {
            var param = _fixture.Create<string>();
            ShouldPass( () => Assert.IsNotEmpty( param ) );
        }

        [Fact]
        public void IsNotEmpty_ShouldThrow_WhenEnumerableIsEmpty()
        {
            var param = Enumerable.Empty<int>();
            ShouldThrow( () => Assert.IsNotEmpty( param ) );
        }

        [Fact]
        public void IsNotEmpty_ShouldThrow_WhenCollectionIsEmpty()
        {
            var param = Array.Empty<int>();
            ShouldThrow( () => Assert.IsNotEmpty( param ) );
        }

        [Fact]
        public void IsNotEmpty_ShouldThrow_WhenStringIsEmpty()
        {
            var param = string.Empty;
            ShouldThrow( () => Assert.IsNotEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldPass_WhenEnumerableIsNull()
        {
            var param = _fixture.CreateDefault<IEnumerable<int>>();
            ShouldPass( () => Assert.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldPass_WhenEnumerableIsEmpty()
        {
            var param = Enumerable.Empty<int>();
            ShouldPass( () => Assert.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldPass_WhenCollectionIsNull()
        {
            var param = _fixture.CreateDefault<int[]>();
            ShouldPass( () => Assert.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldPass_WhenCollectionIsEmpty()
        {
            var param = Array.Empty<int>();
            ShouldPass( () => Assert.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldPass_WhenStringIsNull()
        {
            var param = _fixture.CreateDefault<string>();
            ShouldPass( () => Assert.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldPass_WhenStringIsEmpty()
        {
            var param = string.Empty;
            ShouldPass( () => Assert.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldThrow_WhenEnumerableIsNotEmpty()
        {
            var param = _fixture.CreateMany<int>();
            ShouldThrow( () => Assert.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldThrow_WhenCollectionIsNotEmpty()
        {
            var param = _fixture.CreateMany<int>().ToList();
            ShouldThrow( () => Assert.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNullOrEmpty_ShouldThrow_WhenStringIsNotEmpty()
        {
            var param = _fixture.Create<string>();
            ShouldThrow( () => Assert.IsNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldPass_WhenEnumerableIsNotEmpty()
        {
            var param = _fixture.CreateMany<int>();
            ShouldPass( () => Assert.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldPass_WhenCollectionIsNotEmpty()
        {
            var param = _fixture.CreateMany<int>().ToList();
            ShouldPass( () => Assert.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldPass_WhenStringIsNotEmpty()
        {
            var param = _fixture.Create<string>();
            ShouldPass( () => Assert.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldThrow_WhenEnumerableIsNull()
        {
            var param = _fixture.CreateDefault<IEnumerable<int>>();
            ShouldThrow( () => Assert.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldThrow_WhenEnumerableIsEmpty()
        {
            var param = Enumerable.Empty<int>();
            ShouldThrow( () => Assert.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldThrow_WhenCollectionIsNull()
        {
            var param = _fixture.CreateDefault<int[]>();
            ShouldThrow( () => Assert.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldThrow_WhenCollectionIsEmpty()
        {
            var param = Array.Empty<int>();
            ShouldThrow( () => Assert.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldThrow_WhenStringIsNull()
        {
            var param = _fixture.CreateDefault<string>();
            ShouldThrow( () => Assert.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrEmpty_ShouldThrow_WhenStringIsEmpty()
        {
            var param = string.Empty;
            ShouldThrow( () => Assert.IsNotNullOrEmpty( param ) );
        }

        [Fact]
        public void IsNotNullOrWhiteSpace_ShouldPass_WhenStringIsNotNullOrWhiteSpace()
        {
            var param = _fixture.Create<string>();
            ShouldPass( () => Assert.IsNotNullOrWhiteSpace( param ) );
        }

        [Fact]
        public void IsNotNullOrWhiteSpace_ShouldThrow_WhenStringIsNull()
        {
            var param = _fixture.CreateDefault<string>();
            ShouldThrow( () => Assert.IsNotNullOrWhiteSpace( param ) );
        }

        [Fact]
        public void IsNotNullOrWhiteSpace_ShouldThrow_WhenStringIsEmpty()
        {
            var param = string.Empty;
            ShouldThrow( () => Assert.IsNotNullOrWhiteSpace( param ) );
        }

        [Fact]
        public void IsNotNullOrWhiteSpace_ShouldThrow_WhenStringIsWhiteSpaceOnly()
        {
            var param = " \t\n\r";
            ShouldThrow( () => Assert.IsNotNullOrWhiteSpace( param ) );
        }

        [Fact]
        public void ContainsNull_ShouldPass_WhenEnumerableContainsNullRefElement()
        {
            var param = _fixture.CreateMany<string>().Concat( new[] { _fixture.CreateDefault<string>() } );
            ShouldPass( () => Assert.ContainsNull( param ) );
        }

        [Fact]
        public void ContainsNull_ShouldPass_WhenEnumerableContainsNullStructElement()
        {
            var param = _fixture.CreateMany<int?>().Concat( new[] { _fixture.CreateDefault<int?>() } );
            ShouldPass( () => Assert.ContainsNull( param ) );
        }

        [Fact]
        public void ContainsNull_ShouldPass_WhenEnumerableContainsNullRefElement_WithExplicitComparer()
        {
            var param = _fixture.CreateMany<string>().Concat( new[] { _fixture.CreateDefault<string>() } );
            ShouldPass( () => Assert.ContainsNull( param, _refEqComparer ) );
        }

        [Fact]
        public void ContainsNull_ShouldPass_WhenEnumerableContainsNullStructElement_WithExplicitComparer()
        {
            var param = _fixture.CreateMany<int?>().Concat( new[] { _fixture.CreateDefault<int?>() } );
            ShouldPass( () => Assert.ContainsNull( param, _nullStructEqComparer ) );
        }

        [Fact]
        public void ContainsNull_ShouldThrow_WhenEnumerableDoesntContainNullRefElement()
        {
            var param = _fixture.CreateMany<string>();
            ShouldThrow( () => Assert.ContainsNull( param ) );
        }

        [Fact]
        public void ContainsNull_ShouldThrow_WhenEnumerableDoesntContainNullStructElement()
        {
            var param = _fixture.CreateMany<int?>();
            ShouldThrow( () => Assert.ContainsNull( param ) );
        }

        [Fact]
        public void ContainsNull_ShouldThrow_WhenEnumerableDoesntContainNullRefElement_WithExplicitComparer()
        {
            var param = _fixture.CreateMany<string>();
            ShouldThrow( () => Assert.ContainsNull( param, _refEqComparer ) );
        }

        [Fact]
        public void ContainsNull_ShouldThrow_WhenEnumerableDoesntContainNullStructElement_WithExplicitComparer()
        {
            var param = _fixture.CreateMany<int?>();
            ShouldThrow( () => Assert.ContainsNull( param, _nullStructEqComparer ) );
        }

        [Fact]
        public void ContainsNull_ShouldThrow_WhenEnumerableElementStructTypeIsNotNullable()
        {
            var param = _fixture.CreateMany<int>();
            ShouldThrow( () => Assert.ContainsNull( param, _structEqComparer ) );
        }

        [Fact]
        public void NotContainsNull_ShouldPass_WhenEnumerableDoesntContainNullRefElement()
        {
            var param = _fixture.CreateMany<string>();
            ShouldPass( () => Assert.NotContainsNull( param ) );
        }

        [Fact]
        public void NotContainsNull_ShouldPass_WhenEnumerableDoesntContainNullStructElement()
        {
            var param = _fixture.CreateMany<int?>();
            ShouldPass( () => Assert.NotContainsNull( param ) );
        }

        [Fact]
        public void NotContainsNull_ShouldPass_WhenEnumerableDoesntContainNullRefElement_WithExplicitComparer()
        {
            var param = _fixture.CreateMany<string>();
            ShouldPass( () => Assert.NotContainsNull( param, _refEqComparer ) );
        }

        [Fact]
        public void NotContainsNull_ShouldPass_WhenEnumerableDoesntContainNullStructElement_WithExplicitComparer()
        {
            var param = _fixture.CreateMany<int?>();
            ShouldPass( () => Assert.NotContainsNull( param, _nullStructEqComparer ) );
        }

        [Fact]
        public void NotContainsNull_ShouldPass_WhenEnumerableElementStructTypeIsNotNullable()
        {
            var param = _fixture.CreateMany<int>();
            ShouldPass( () => Assert.NotContainsNull( param, _structEqComparer ) );
        }

        [Fact]
        public void NotContainsNull_ShouldThrow_WhenEnumerableContainsNullRefElement()
        {
            var param = _fixture.CreateMany<string>().Concat( new[] { _fixture.CreateDefault<string>() } );
            ShouldThrow( () => Assert.NotContainsNull( param ) );
        }

        [Fact]
        public void NotContainsNull_ShouldThrow_WhenEnumerableContainsNullStructElement()
        {
            var param = _fixture.CreateMany<int?>().Concat( new[] { _fixture.CreateDefault<int?>() } );
            ShouldThrow( () => Assert.NotContainsNull( param ) );
        }

        [Fact]
        public void NotContainsNull_ShouldThrow_WhenEnumerableContainsNullRefElement_WithExplicitComparer()
        {
            var param = _fixture.CreateMany<string>().Concat( new[] { _fixture.CreateDefault<string>() } );
            ShouldThrow( () => Assert.NotContainsNull( param, _refEqComparer ) );
        }

        [Fact]
        public void NotContainsNull_ShouldThrow_WhenEnumerableContainsNullStructElement_WithExplicitComparer()
        {
            var param = _fixture.CreateMany<int?>().Concat( new[] { _fixture.CreateDefault<int?>() } );
            ShouldThrow( () => Assert.NotContainsNull( param, _nullStructEqComparer ) );
        }

        [Fact]
        public void Contains_ShouldPass_WhenEnumerableContainsElement()
        {
            var value = _fixture.Create<int>();
            var param = _fixture.CreateMany<int>().Concat( new[] { value } );
            ShouldPass( () => Assert.Contains( param, value ) );
        }

        [Fact]
        public void Contains_ShouldPass_WhenEnumerableContainsElement_WithExplicitComparer()
        {
            var value = _fixture.Create<int>();
            var param = _fixture.CreateMany<int>().Concat( new[] { value } );
            ShouldPass( () => Assert.Contains( param, value, _structEqComparer ) );
        }

        [Fact]
        public void Contains_ShouldThrow_WhenEnumerableDoesntContainElement()
        {
            var value = _fixture.Create<int>();
            var param = _fixture.CreateMany<int>().Except( new[] { value } );
            ShouldThrow( () => Assert.Contains( param, value ) );
        }

        [Fact]
        public void Contains_ShouldThrow_WhenEnumerableDoesntContainElement_WithExplicitComparer()
        {
            var value = _fixture.Create<int>();
            var param = _fixture.CreateMany<int>().Except( new[] { value } );
            ShouldThrow( () => Assert.Contains( param, value, _structEqComparer ) );
        }

        [Fact]
        public void NotContains_ShouldPass_WhenEnumerableDoesntContainElement()
        {
            var value = _fixture.Create<int>();
            var param = _fixture.CreateMany<int>().Except( new[] { value } );
            ShouldPass( () => Assert.NotContains( param, value ) );
        }

        [Fact]
        public void NotContains_ShouldPass_WhenEnumerableDoesntContainElement_WithExplicitComparer()
        {
            var value = _fixture.Create<int>();
            var param = _fixture.CreateMany<int>().Except( new[] { value } );
            ShouldPass( () => Assert.NotContains( param, value, _structEqComparer ) );
        }

        [Fact]
        public void NotContains_ShouldThrow_WhenEnumerableContainsElement()
        {
            var value = _fixture.Create<int>();
            var param = _fixture.CreateMany<int>().Concat( new[] { value } );
            ShouldThrow( () => Assert.NotContains( param, value ) );
        }

        [Fact]
        public void NotContains_ShouldThrow_WhenEnumerableContainsElement_WithExplicitComparer()
        {
            var value = _fixture.Create<int>();
            var param = _fixture.CreateMany<int>().Concat( new[] { value } );
            ShouldThrow( () => Assert.NotContains( param, value, _structEqComparer ) );
        }

        [Fact]
        public void ForAny_ShouldPass_WhenAtLeastOneElementPassesThePredicate()
        {
            var value = _fixture.Create<int>();
            var param = _fixture.CreateMany<int>().Concat( new[] { value } );
            ShouldPass( () => Assert.ForAny( param, e => e == value ) );
        }

        [Fact]
        public void ForAny_ShouldThrow_WhenNoElementsPassThePredicate()
        {
            var value = _fixture.Create<int>();
            var param = _fixture.CreateMany<int>();
            ShouldThrow( () => Assert.ForAny( param, e => e == value ) );
        }

        [Fact]
        public void ForAny_ShouldThrow_WhenEnumerableIsEmpty()
        {
            var param = Enumerable.Empty<int>();
            ShouldThrow( () => Assert.ForAny( param, _ => true ) );
        }

        [Fact]
        public void ForAll_ShouldPass_WhenAllElementsPassThePredicate()
        {
            var value = _fixture.Create<int>();
            var param = Enumerable.Range( 0, 3 ).Select( _ => value );
            ShouldPass( () => Assert.ForAll( param, e => e == value ) );
        }

        [Fact]
        public void ForAll_ShouldPass_WhenEnumerableIsEmpty()
        {
            var param = Enumerable.Empty<int>();
            ShouldPass( () => Assert.ForAll( param, _ => false ) );
        }

        [Fact]
        public void ForAll_ShouldThrow_WhenAtLeastOneElementFailsThePredicate()
        {
            var value = _fixture.Create<int>();
            var param = Enumerable.Range( 0, 3 ).Select( _ => value ).Concat( new[] { _fixture.Create<int>() } );
            ShouldThrow( () => Assert.ForAll( param, e => e == value ) );
        }

        [Fact]
        public void True_ShouldPass_WhenConditionIsTrue()
        {
            ShouldPass( () => Assert.True( true ) );
        }

        [Fact]
        public void True_ShouldThrow_WhenConditionIsFalse()
        {
            ShouldThrow( () => Assert.True( false ) );
        }

        [Fact]
        public void False_ShouldPass_WhenConditionIsFalse()
        {
            ShouldPass( () => Assert.False( false ) );
        }

        [Fact]
        public void False_ShouldThrow_WhenConditionIsTrue()
        {
            ShouldThrow( () => Assert.False( true ) );
        }

        private static void ShouldPass(Action action)
        {
            action.Should().NotThrow();
        }

        private static void ShouldThrow(Action action)
        {
            ShouldThrow<ArgumentException>( action );
        }

        private static void ShouldThrow<TException>(Action action)
            where TException : Exception
        {
            action.Should().Throw<TException>();
        }
    }
}
