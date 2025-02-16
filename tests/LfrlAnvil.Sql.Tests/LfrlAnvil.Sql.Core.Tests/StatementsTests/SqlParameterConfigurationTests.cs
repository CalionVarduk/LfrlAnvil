using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Statements.Compilers;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlParameterConfigurationTests : TestsBase
{
    [Fact]
    public void IgnoreMember_ShouldCreateConfigurationWithoutTargetParameterName()
    {
        var sut = SqlParameterConfiguration.IgnoreMember( "foo" );

        Assertion.All(
                sut.MemberName.TestEquals( "foo" ),
                sut.TargetParameterName.TestNull(),
                sut.CustomSelector.TestNull(),
                sut.CustomSelectorSourceType.TestNull(),
                sut.CustomSelectorValueType.TestNull(),
                sut.IsIgnoredWhenNull.TestEquals( true ),
                sut.IsIgnored.TestTrue() )
            .Go();
    }

    [Theory]
    [InlineData( true, null )]
    [InlineData( true, 0 )]
    [InlineData( false, null )]
    [InlineData( false, 1 )]
    public void IgnoreMemberWhenNull_ShouldCreateConfigurationWithCorrectIsIgnoredWhenNull(bool value, int? parameterIndex)
    {
        var sut = SqlParameterConfiguration.IgnoreMemberWhenNull( "foo", value, parameterIndex );

        Assertion.All(
                sut.MemberName.TestEquals( "foo" ),
                sut.TargetParameterName.TestEquals( "foo" ),
                sut.ParameterIndex.TestEquals( parameterIndex ),
                sut.CustomSelector.TestNull(),
                sut.CustomSelectorSourceType.TestNull(),
                sut.CustomSelectorValueType.TestNull(),
                sut.IsIgnoredWhenNull.TestEquals( value ),
                sut.IsIgnored.TestFalse() )
            .Go();
    }

    [Theory]
    [InlineData( 0, null )]
    [InlineData( 1, true )]
    [InlineData( 2, null )]
    [InlineData( 3, false )]
    public void Positional_ShouldCreateConfigurationWithCorrectParameterIndex(int parameterIndex, bool? ignoreNull)
    {
        var sut = SqlParameterConfiguration.Positional( "foo", parameterIndex, ignoreNull );

        Assertion.All(
                sut.MemberName.TestEquals( "foo" ),
                sut.TargetParameterName.TestEquals( "foo" ),
                sut.ParameterIndex.TestEquals( parameterIndex ),
                sut.CustomSelector.TestNull(),
                sut.CustomSelectorSourceType.TestNull(),
                sut.CustomSelectorValueType.TestNull(),
                sut.IsIgnoredWhenNull.TestEquals( ignoreNull ),
                sut.IsIgnored.TestFalse() )
            .Go();
    }

    [Fact]
    public void Positional_ShouldThrowArgumentOutOfRangeException_WhenParameterIndexIsLessThanZero()
    {
        var action = Lambda.Of( () => SqlParameterConfiguration.Positional( "foo", -1 ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( true, null )]
    [InlineData( true, 0 )]
    [InlineData( false, null )]
    [InlineData( false, 1 )]
    [InlineData( null, null )]
    [InlineData( null, 2 )]
    public void From_WithMemberName_ShouldCreateConfigurationWithDifferentMemberAndTargetParameterNames(
        bool? ignoreNull,
        int? parameterIndex)
    {
        var sut = SqlParameterConfiguration.From( "foo", "bar", ignoreNull, parameterIndex );

        Assertion.All(
                sut.MemberName.TestEquals( "bar" ),
                sut.TargetParameterName.TestEquals( "foo" ),
                sut.ParameterIndex.TestEquals( parameterIndex ),
                sut.CustomSelector.TestNull(),
                sut.CustomSelectorSourceType.TestNull(),
                sut.CustomSelectorValueType.TestNull(),
                sut.IsIgnoredWhenNull.TestEquals( ignoreNull ),
                sut.IsIgnored.TestFalse() )
            .Go();
    }

    [Theory]
    [InlineData( true, null )]
    [InlineData( true, 0 )]
    [InlineData( false, null )]
    [InlineData( false, 1 )]
    [InlineData( null, null )]
    [InlineData( null, 2 )]
    public void From_WithSelector_ShouldCreateConfigurationWithCustomSelector(bool? ignoreNull, int? parameterIndex)
    {
        var selector = Lambda.ExpressionOf( (int source) => source.ToString() );
        var sut = SqlParameterConfiguration.From( "foo", selector, ignoreNull, parameterIndex );

        Assertion.All(
                sut.MemberName.TestNull(),
                sut.TargetParameterName.TestEquals( "foo" ),
                sut.ParameterIndex.TestEquals( parameterIndex ),
                sut.CustomSelector.TestRefEquals( selector ),
                sut.CustomSelectorSourceType.TestEquals( typeof( int ) ),
                sut.CustomSelectorValueType.TestEquals( typeof( string ) ),
                sut.IsIgnoredWhenNull.TestEquals( ignoreNull ),
                sut.IsIgnored.TestFalse() )
            .Go();
    }
}
