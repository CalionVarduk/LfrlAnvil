﻿using System;
using System.Linq;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests.Assert
{
    public abstract class AssertComparableTests<T> : AssertTests<T>
        where T : IEquatable<T>, IComparable<T>
    {
        [Fact]
        public void Equals_ShouldPass_WhenParamIsEqualToValue()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldPass( () => Common.Assert.Equals( param, param ) );
        }

        [Fact]
        public void Equals_ShouldThrow_WhenParamIsNotEqualToValue()
        {
            var (param, value) = Fixture.CreateDistinctCollection<T>( 2 );
            ShouldThrow( () => Common.Assert.Equals( param, value ) );
        }

        [Fact]
        public void NotEquals_ShouldPass_WhenParamIsNotEqualToValue()
        {
            var (param, value) = Fixture.CreateDistinctCollection<T>( 2 );
            ShouldPass( () => Common.Assert.NotEquals( param, value ) );
        }

        [Fact]
        public void NotEquals_ShouldThrow_WhenParamIsEqualToValue()
        {
            var param = Fixture.CreateNotDefault<T>()!;
            ShouldThrow( () => Common.Assert.NotEquals( param, param ) );
        }

        [Fact]
        public void IsGreaterThan_ShouldPass_WhenParamIsGreaterThanValue()
        {
            var (value, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Common.Assert.IsGreaterThan( param, value ) );
        }

        [Fact]
        public void IsGreaterThan_ShouldThrow_WhenParamIsLessThanValue()
        {
            var (param, value) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow( () => Common.Assert.IsGreaterThan( param, value ) );
        }

        [Fact]
        public void IsGreaterThan_ShouldThrow_WhenParamIsEqualToValue()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrow( () => Common.Assert.IsGreaterThan( param, param ) );
        }

        [Fact]
        public void IsGreaterThanOrEqualTo_ShouldPass_WhenParamIsGreaterThanValue()
        {
            var (value, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Common.Assert.IsGreaterThanOrEqualTo( param, value ) );
        }

        [Fact]
        public void IsGreaterThanOrEqualTo_ShouldPass_WhenParamIsEqualToValue()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Common.Assert.IsGreaterThanOrEqualTo( param, param ) );
        }

        [Fact]
        public void IsGreaterThanOrEqualTo_ShouldThrow_WhenParamIsLessThanValue()
        {
            var (param, value) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow( () => Common.Assert.IsGreaterThanOrEqualTo( param, value ) );
        }

        [Fact]
        public void IsLessThan_ShouldPass_WhenParamIsLessThanValue()
        {
            var (param, value) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Common.Assert.IsLessThan( param, value ) );
        }

        [Fact]
        public void IsLessThan_ShouldThrow_WhenParamIsGreaterThanValue()
        {
            var (value, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow( () => Common.Assert.IsLessThan( param, value ) );
        }

        [Fact]
        public void IsLessThan_ShouldThrow_WhenParamIsEqualToValue()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrow( () => Common.Assert.IsLessThan( param, param ) );
        }

        [Fact]
        public void IsLessThanOrEqualTo_ShouldPass_WhenParamIsLessThanValue()
        {
            var (param, value) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Common.Assert.IsLessThanOrEqualTo( param, value ) );
        }

        [Fact]
        public void IsLessThanOrEqualTo_ShouldPass_WhenParamIsEqualToValue()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Common.Assert.IsLessThanOrEqualTo( param, param ) );
        }

        [Fact]
        public void IsLessThanOrEqualTo_ShouldThrow_WhenParamIsGreaterThanValue()
        {
            var (value, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow( () => Common.Assert.IsLessThanOrEqualTo( param, value ) );
        }

        [Fact]
        public void IsBetween_ShouldPass_WhenParamIsBetweenTwoDistinctValues()
        {
            var (min, param, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldPass( () => Common.Assert.IsBetween( param, min, max ) );
        }

        [Fact]
        public void IsBetween_ShouldPass_WhenParamIsEqualToMinValue()
        {
            var (param, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Common.Assert.IsBetween( param, param, max ) );
        }

        [Fact]
        public void IsBetween_ShouldPass_WhenParamIsEqualToMaxValue()
        {
            var (min, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Common.Assert.IsBetween( param, min, param ) );
        }

        [Fact]
        public void IsBetween_ShouldPass_WhenParamIsEqualToMinAndMaxValues()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldPass( () => Common.Assert.IsBetween( param, param, param ) );
        }

        [Fact]
        public void IsBetween_ShouldThrow_WhenParamIsLessThanMinValue()
        {
            var (param, min, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldThrow( () => Common.Assert.IsBetween( param, min, max ) );
        }

        [Fact]
        public void IsBetween_ShouldThrow_WhenParamIsGreaterThanMaxValue()
        {
            var (min, max, param) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldThrow( () => Common.Assert.IsBetween( param, min, max ) );
        }

        [Fact]
        public void IsBetween_ShouldThrow_WhenMinIsGreaterThanMax()
        {
            var param = Fixture.CreateNotDefault<T>();
            var (max, min) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow( () => Common.Assert.IsBetween( param, min, max ) );
        }

        [Fact]
        public void IsNotBetween_ShouldPass_WhenParamIsLessThanMinValue()
        {
            var (param, min, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldPass( () => Common.Assert.IsNotBetween( param, min, max ) );
        }

        [Fact]
        public void IsNotBetween_ShouldPass_WhenParamIsGreaterThanMaxValue()
        {
            var (min, max, param) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldPass( () => Common.Assert.IsNotBetween( param, min, max ) );
        }

        [Fact]
        public void IsNotBetween_ShouldPass_WhenMinIsGreaterThanMax()
        {
            var param = Fixture.CreateNotDefault<T>();
            var (max, min) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldPass( () => Common.Assert.IsNotBetween( param, min, max ) );
        }

        [Fact]
        public void IsNotBetween_ShouldThrow_WhenParamIsBetweenTwoDistinctValues()
        {
            var (min, param, max) = Fixture.CreateDistinctSortedCollection<T>( 3 );
            ShouldThrow( () => Common.Assert.IsNotBetween( param, min, max ) );
        }

        [Fact]
        public void IsNotBetween_ShouldThrow_WhenParamIsEqualToMinValue()
        {
            var (param, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow( () => Common.Assert.IsNotBetween( param, param, max ) );
        }

        [Fact]
        public void IsNotBetween_ShouldThrow_WhenParamIsEqualToMaxValue()
        {
            var (min, param) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            ShouldThrow( () => Common.Assert.IsNotBetween( param, min, param ) );
        }

        [Fact]
        public void IsNotBetween_ShouldThrow_WhenParamIsEqualToMinAndMaxValues()
        {
            var param = Fixture.CreateNotDefault<T>();
            ShouldThrow( () => Common.Assert.IsNotBetween( param, param, param ) );
        }

        [Fact]
        public void Contains_ShouldPass_WhenEnumerableContainsElement()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Append( value );
            ShouldPass( () => Common.Assert.Contains( param, value ) );
        }

        [Fact]
        public void Contains_ShouldThrow_WhenEnumerableDoesntContainElement()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Except( new[] { value } );
            ShouldThrow( () => Common.Assert.Contains( param, value ) );
        }

        [Fact]
        public void NotContains_ShouldPass_WhenEnumerableDoesntContainElement()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Except( new[] { value } );
            ShouldPass( () => Common.Assert.NotContains( param, value ) );
        }

        [Fact]
        public void NotContains_ShouldThrow_WhenEnumerableContainsElement()
        {
            var value = Fixture.Create<T>();
            var param = Fixture.CreateMany<T>().Concat( new[] { value } );
            ShouldThrow( () => Common.Assert.NotContains( param, value ) );
        }
    }
}
