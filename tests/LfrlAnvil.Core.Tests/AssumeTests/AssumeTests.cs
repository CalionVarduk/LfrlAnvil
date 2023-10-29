using System.Diagnostics;
using LfrlAnvil.Functional;
using LfrlAnvil.Tests.EnsureTests;

namespace LfrlAnvil.Tests.AssumeTests;

public class AssumeTests : TestsBase
{
    [Fact]
    [Conditional( "DEBUG" )]
    public void IsNull_ForRefType_ShouldPass_WhenParamIsNull()
    {
        string? param = null;
        var action = Lambda.Of( () => Assume.IsNull( param ) );
        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsNull_ForRefType_ShouldFail_WhenParamIsNotNull()
    {
        var param = Fixture.Create<string>();
        var action = Lambda.Of( () => Assume.IsNull( param ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsNull_ForNullableStructType_ShouldPass_WhenParamIsNull()
    {
        int? param = null;
        var action = Lambda.Of( () => Assume.IsNull( param ) );
        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsNull_ForNullableStructType_ShouldFail_WhenParamIsNotNull()
    {
        var param = Fixture.CreateNullable<int>();
        var action = Lambda.Of( () => Assume.IsNull( param ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsNotNull_ForRefType_ShouldPass_WhenParamIsNotNull()
    {
        var param = Fixture.Create<string>();
        var action = Lambda.Of( () => Assume.IsNotNull( param ) );
        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsNotNull_ForRefType_ShouldFail_WhenParamIsNull()
    {
        string? param = null;
        var action = Lambda.Of( () => Assume.IsNotNull( param ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsNotNull_ForNullableStructType_ShouldPass_WhenParamIsNotNull()
    {
        var param = Fixture.CreateNullable<int>();
        var action = Lambda.Of( () => Assume.IsNotNull( param ) );
        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsNotNull_ForNullableStructType_ShouldFail_WhenParamIsNull()
    {
        int? param = null;
        var action = Lambda.Of( () => Assume.IsNotNull( param ) );
        action.Should().Throw<Exception>();
    }

    [Theory]
    [Conditional( "DEBUG" )]
    [InlineData( TestEnum.Foo )]
    [InlineData( TestEnum.Bar )]
    public void IsDefined_ShouldPass_WhenParamIsDefined(TestEnum param)
    {
        var action = Lambda.Of( () => Assume.IsDefined( param ) );
        action.Should().NotThrow();
    }

    [Theory]
    [Conditional( "DEBUG" )]
    [InlineData( -1 )]
    [InlineData( (int)TestEnum.Bar + 1 )]
    public void IsDefined_ShouldFail_WhenParamIsNotDefined(int param)
    {
        var action = Lambda.Of( () => Assume.IsDefined( (TestEnum)param ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void Equals_ShouldPass_WhenParamIsEqualToValue()
    {
        var param = Fixture.Create<int>();
        var action = Lambda.Of( () => Assume.Equals( param, param ) );
        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void Equals_ShouldFail_WhenParamIsNotEqualToValue()
    {
        var (param, value) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var action = Lambda.Of( () => Assume.Equals( param, value ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void NotEquals_ShouldPass_WhenParamIsNotEqualToValue()
    {
        var (param, value) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var action = Lambda.Of( () => Assume.NotEquals( param, value ) );
        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void NotEquals_ShouldFail_WhenParamIsEqualToValue()
    {
        var param = Fixture.Create<int>();
        var action = Lambda.Of( () => Assume.NotEquals( param, param ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsGreaterThan_ShouldPass_WhenParamIsGreaterThanValue()
    {
        var (value, param) = Fixture.CreateDistinctSortedCollection<int>( count: 2 );
        var action = Lambda.Of( () => Assume.IsGreaterThan( param, value ) );
        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsGreaterThan_ShouldFail_WhenParamIsEqualToValue()
    {
        var param = Fixture.Create<int>();
        var action = Lambda.Of( () => Assume.IsGreaterThan( param, param ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsGreaterThan_ShouldFail_WhenParamIsLessThanValue()
    {
        var (param, value) = Fixture.CreateDistinctSortedCollection<int>( count: 2 );
        var action = Lambda.Of( () => Assume.IsGreaterThan( param, value ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsGreaterThanOrEqualTo_ShouldPass_WhenParamIsGreaterThanValue()
    {
        var (value, param) = Fixture.CreateDistinctSortedCollection<int>( count: 2 );
        var action = Lambda.Of( () => Assume.IsGreaterThanOrEqualTo( param, value ) );
        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsGreaterThanOrEqualTo_ShouldPass_WhenParamIsEqualToValue()
    {
        var param = Fixture.Create<int>();
        var action = Lambda.Of( () => Assume.IsGreaterThanOrEqualTo( param, param ) );
        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsGreaterThanOrEqualTo_ShouldFail_WhenParamIsLessThanValue()
    {
        var (param, value) = Fixture.CreateDistinctSortedCollection<int>( count: 2 );
        var action = Lambda.Of( () => Assume.IsGreaterThanOrEqualTo( param, value ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsLessThan_ShouldPass_WhenParamIsLessThanValue()
    {
        var (param, value) = Fixture.CreateDistinctSortedCollection<int>( count: 2 );
        var action = Lambda.Of( () => Assume.IsLessThan( param, value ) );
        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsLessThan_ShouldFail_WhenParamIsEqualToValue()
    {
        var param = Fixture.Create<int>();
        var action = Lambda.Of( () => Assume.IsLessThan( param, param ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsLessThan_ShouldFail_WhenParamIsGreaterThanValue()
    {
        var (value, param) = Fixture.CreateDistinctSortedCollection<int>( count: 2 );
        var action = Lambda.Of( () => Assume.IsLessThan( param, value ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsLessThanOrEqualTo_ShouldPass_WhenParamIsLessThanValue()
    {
        var (param, value) = Fixture.CreateDistinctSortedCollection<int>( count: 2 );
        var action = Lambda.Of( () => Assume.IsLessThanOrEqualTo( param, value ) );
        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsLessThanOrEqualTo_ShouldPass_WhenParamIsEqualToValue()
    {
        var param = Fixture.Create<int>();
        var action = Lambda.Of( () => Assume.IsLessThanOrEqualTo( param, param ) );
        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsLessThanOrEqualTo_ShouldFail_WhenParamIsGreaterThanValue()
    {
        var (value, param) = Fixture.CreateDistinctSortedCollection<int>( count: 2 );
        var action = Lambda.Of( () => Assume.IsLessThanOrEqualTo( param, value ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsInRange_ShouldPass_WhenParamIsBetweenMinAndMax()
    {
        var (min, param, max) = Fixture.CreateDistinctSortedCollection<int>( count: 3 );
        var action = Lambda.Of( () => Assume.IsInRange( param, min, max ) );
        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsInRange_ShouldPass_WhenParamIsEqualToMin()
    {
        var (param, max) = Fixture.CreateDistinctSortedCollection<int>( count: 3 );
        var action = Lambda.Of( () => Assume.IsInRange( param, param, max ) );
        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsInRange_ShouldPass_WhenParamIsEqualToMax()
    {
        var (min, param) = Fixture.CreateDistinctSortedCollection<int>( count: 3 );
        var action = Lambda.Of( () => Assume.IsInRange( param, min, param ) );
        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsInRange_ShouldFail_WhenParamIsLessThanMin()
    {
        var (param, min, max) = Fixture.CreateDistinctSortedCollection<int>( count: 3 );
        var action = Lambda.Of( () => Assume.IsInRange( param, min, max ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsInRange_ShouldFail_WhenParamIsGreaterThanMin()
    {
        var (min, max, param) = Fixture.CreateDistinctSortedCollection<int>( count: 3 );
        var action = Lambda.Of( () => Assume.IsInRange( param, min, max ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsNotInRange_ShouldFail_WhenParamIsBetweenMinAndMax()
    {
        var (min, param, max) = Fixture.CreateDistinctSortedCollection<int>( count: 3 );
        var action = Lambda.Of( () => Assume.IsNotInRange( param, min, max ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsNotInRange_ShouldFail_WhenParamIsEqualToMin()
    {
        var (param, max) = Fixture.CreateDistinctSortedCollection<int>( count: 3 );
        var action = Lambda.Of( () => Assume.IsNotInRange( param, param, max ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsNotInRange_ShouldFail_WhenParamIsEqualToMax()
    {
        var (min, param) = Fixture.CreateDistinctSortedCollection<int>( count: 3 );
        var action = Lambda.Of( () => Assume.IsNotInRange( param, min, param ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsNotInRange_ShouldPass_WhenParamIsLessThanMin()
    {
        var (param, min, max) = Fixture.CreateDistinctSortedCollection<int>( count: 3 );
        var action = Lambda.Of( () => Assume.IsNotInRange( param, min, max ) );
        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsNotInRange_ShouldPass_WhenParamIsGreaterThanMin()
    {
        var (min, max, param) = Fixture.CreateDistinctSortedCollection<int>( count: 3 );
        var action = Lambda.Of( () => Assume.IsNotInRange( param, min, max ) );
        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsInExclusiveRange_ShouldPass_WhenParamIsBetweenMinAndMax()
    {
        var (min, param, max) = Fixture.CreateDistinctSortedCollection<int>( count: 3 );
        var action = Lambda.Of( () => Assume.IsInExclusiveRange( param, min, max ) );
        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsInExclusiveRange_ShouldFail_WhenParamIsEqualToMin()
    {
        var (param, max) = Fixture.CreateDistinctSortedCollection<int>( count: 3 );
        var action = Lambda.Of( () => Assume.IsInExclusiveRange( param, param, max ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsInExclusiveRange_ShouldFail_WhenParamIsEqualToMax()
    {
        var (min, param) = Fixture.CreateDistinctSortedCollection<int>( count: 3 );
        var action = Lambda.Of( () => Assume.IsInExclusiveRange( param, min, param ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsInExclusiveRange_ShouldFail_WhenParamIsLessThanMin()
    {
        var (param, min, max) = Fixture.CreateDistinctSortedCollection<int>( count: 3 );
        var action = Lambda.Of( () => Assume.IsInExclusiveRange( param, min, max ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsInExclusiveRange_ShouldFail_WhenParamIsGreaterThanMin()
    {
        var (min, max, param) = Fixture.CreateDistinctSortedCollection<int>( count: 3 );
        var action = Lambda.Of( () => Assume.IsInExclusiveRange( param, min, max ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsNotInExclusiveRange_ShouldFail_WhenParamIsBetweenMinAndMax()
    {
        var (min, param, max) = Fixture.CreateDistinctSortedCollection<int>( count: 3 );
        var action = Lambda.Of( () => Assume.IsNotInExclusiveRange( param, min, max ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsNotInExclusiveRange_ShouldPass_WhenParamIsEqualToMin()
    {
        var (param, max) = Fixture.CreateDistinctSortedCollection<int>( count: 3 );
        var action = Lambda.Of( () => Assume.IsNotInExclusiveRange( param, param, max ) );
        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsNotInExclusiveRange_ShouldPass_WhenParamIsEqualToMax()
    {
        var (min, param) = Fixture.CreateDistinctSortedCollection<int>( count: 3 );
        var action = Lambda.Of( () => Assume.IsNotInExclusiveRange( param, min, param ) );
        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsNotInExclusiveRange_ShouldPass_WhenParamIsLessThanMin()
    {
        var (param, min, max) = Fixture.CreateDistinctSortedCollection<int>( count: 3 );
        var action = Lambda.Of( () => Assume.IsNotInExclusiveRange( param, min, max ) );
        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsNotInExclusiveRange_ShouldPass_WhenParamIsGreaterThanMin()
    {
        var (min, max, param) = Fixture.CreateDistinctSortedCollection<int>( count: 3 );
        var action = Lambda.Of( () => Assume.IsNotInExclusiveRange( param, min, max ) );
        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsEmpty_ShouldPass_WhenParamIsEmpty()
    {
        var param = string.Empty;
        var action = Lambda.Of( () => Assume.IsEmpty( param ) );
        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsEmpty_ShouldFail_WhenParamIsNotEmpty()
    {
        var param = Fixture.Create<string>();
        var action = Lambda.Of( () => Assume.IsEmpty( param ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsNotEmpty_ShouldFail_WhenParamIsEmpty()
    {
        var param = string.Empty;
        var action = Lambda.Of( () => Assume.IsNotEmpty( param ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void IsNotEmpty_ShouldPass_WhenParamIsNotEmpty()
    {
        var param = Fixture.Create<string>();
        var action = Lambda.Of( () => Assume.IsNotEmpty( param ) );
        action.Should().NotThrow();
    }

    [Theory]
    [Conditional( "DEBUG" )]
    [InlineData( -1 )]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void ContainsAtLeast_ShouldPass_WhenParamContainsEnoughElements(int count)
    {
        var param = Fixture.CreateMany<int>( count: 3 );
        var action = Lambda.Of( () => Assume.ContainsAtLeast( param, count ) );
        action.Should().NotThrow();
    }

    [Theory]
    [Conditional( "DEBUG" )]
    [InlineData( 4 )]
    [InlineData( 5 )]
    public void ContainsAtLeast_ShouldFail_WhenParamDoesNotContainEnoughElements(int count)
    {
        var param = Fixture.CreateMany<int>( count: 3 );
        var action = Lambda.Of( () => Assume.ContainsAtLeast( param, count ) );
        action.Should().Throw<Exception>();
    }

    [Theory]
    [Conditional( "DEBUG" )]
    [InlineData( -1 )]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 2 )]
    public void ContainsAtMost_ShouldFail_WhenParamContainsMoreElements(int count)
    {
        var param = Fixture.CreateMany<int>( count: 3 );
        var action = Lambda.Of( () => Assume.ContainsAtMost( param, count ) );
        action.Should().Throw<Exception>();
    }

    [Theory]
    [Conditional( "DEBUG" )]
    [InlineData( 3 )]
    [InlineData( 4 )]
    [InlineData( 5 )]
    public void ContainsAtMost_ShouldPass_WhenParamDoesNotContainMoreElements(int count)
    {
        var param = Fixture.CreateMany<int>( count: 3 );
        var action = Lambda.Of( () => Assume.ContainsAtMost( param, count ) );
        action.Should().NotThrow();
    }

    [Theory]
    [Conditional( "DEBUG" )]
    [InlineData( -1, 3 )]
    [InlineData( 0, 3 )]
    [InlineData( 1, 3 )]
    [InlineData( 2, 3 )]
    [InlineData( 3, 3 )]
    [InlineData( -1, 4 )]
    [InlineData( 0, 4 )]
    [InlineData( 1, 4 )]
    [InlineData( 2, 4 )]
    [InlineData( 3, 4 )]
    public void ContainsInRange_ShouldPass_WhenParamContainsCorrectAmountOfElements(int min, int max)
    {
        var param = Fixture.CreateMany<int>( count: 3 );
        var action = Lambda.Of( () => Assume.ContainsInRange( param, min, max ) );
        action.Should().NotThrow();
    }

    [Theory]
    [Conditional( "DEBUG" )]
    [InlineData( -1, 2 )]
    [InlineData( 0, 2 )]
    [InlineData( 1, 2 )]
    [InlineData( 2, 2 )]
    [InlineData( 4, 4 )]
    [InlineData( 4, 5 )]
    [InlineData( 4, 6 )]
    public void ContainsInRange_ShouldFail_WhenParamDoesNotContainCorrectAmountOfElements(int min, int max)
    {
        var param = Fixture.CreateMany<int>( count: 3 );
        var action = Lambda.Of( () => Assume.ContainsInRange( param, min, max ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void ContainsExactly_ShouldPass_WhenParamContainsExactAmountOfElements()
    {
        var param = Fixture.CreateMany<int>( count: 3 );
        var action = Lambda.Of( () => Assume.ContainsExactly( param, count: 3 ) );
        action.Should().NotThrow();
    }

    [Theory]
    [Conditional( "DEBUG" )]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 4 )]
    [InlineData( 5 )]
    public void ContainsExactly_ShouldFail_WhenParamContainsDifferentAmountOfElements(int count)
    {
        var param = Fixture.CreateMany<int>( count: 3 );
        var action = Lambda.Of( () => Assume.ContainsExactly( param, count ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void Unreachable_ShouldFail()
    {
        var action = Lambda.Of( () => Assume.Unreachable() );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void True_ShouldPass_WhenConditionIsTrue()
    {
        var action = Lambda.Of( () => Assume.True( true, string.Empty ) );
        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void True_ShouldFail_WhenConditionIsFalse()
    {
        var action = Lambda.Of( () => Assume.True( false, string.Empty ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void False_ShouldPass_WhenConditionIsFalse()
    {
        var action = Lambda.Of( () => Assume.False( false, string.Empty ) );
        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void False_ShouldFail_WhenConditionIsTrue()
    {
        var action = Lambda.Of( () => Assume.False( true, string.Empty ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void Conditional_ShouldPass_WhenConditionIsFalse()
    {
        var action = Lambda.Of( () => Assume.Conditional( false, () => Assume.True( false, string.Empty ) ) );
        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void Conditional_ShouldPass_WhenConditionIsTrueAndAssumptionPasses()
    {
        var action = Lambda.Of( () => Assume.Conditional( true, () => Assume.True( true, string.Empty ) ) );
        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void Conditional_ShouldFail_WhenConditionIsTrueAndAssumptionFails()
    {
        var action = Lambda.Of( () => Assume.Conditional( true, () => Assume.True( false, string.Empty ) ) );
        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void Conditional_WithTwoParameters_ShouldPass_WhenConditionIsFalseAndIfFalseAssumptionPasses()
    {
        var action = Lambda.Of(
            () => Assume.Conditional( false, () => Assume.True( false, string.Empty ), () => Assume.True( true, string.Empty ) ) );

        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void Conditional_WithTwoParameters_ShouldFail_WhenConditionIsFalseAndIfFalseAssumptionFails()
    {
        var action = Lambda.Of(
            () => Assume.Conditional( false, () => Assume.True( true, string.Empty ), () => Assume.True( false, string.Empty ) ) );

        action.Should().Throw<Exception>();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void Conditional_WithTwoParameters_ShouldPass_WhenConditionIsTrueAndIfTrueAssumptionPasses()
    {
        var action = Lambda.Of(
            () => Assume.Conditional( true, () => Assume.True( true, string.Empty ), () => Assume.True( false, string.Empty ) ) );

        action.Should().NotThrow();
    }

    [Fact]
    [Conditional( "DEBUG" )]
    public void Conditional_WithTwoParameters_ShouldFail_WhenConditionIsTrueAndIfTrueAssumptionFails()
    {
        var action = Lambda.Of(
            () => Assume.Conditional( true, () => Assume.True( false, string.Empty ), () => Assume.True( true, string.Empty ) ) );

        action.Should().Throw<Exception>();
    }
}
