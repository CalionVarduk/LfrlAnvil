using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Statements.Compilers;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlParameterConfigurationTests : TestsBase
{
    [Fact]
    public void IgnoreMember_ShouldCreateConfigurationWithoutTargetParameterName()
    {
        var sut = SqlParameterConfiguration.IgnoreMember( "foo" );

        using ( new AssertionScope() )
        {
            sut.MemberName.Should().Be( "foo" );
            sut.TargetParameterName.Should().BeNull();
            sut.CustomSelector.Should().BeNull();
            sut.CustomSelectorSourceType.Should().BeNull();
            sut.CustomSelectorValueType.Should().BeNull();
            sut.IsIgnoredWhenNull.Should().BeTrue();
            sut.IsIgnored.Should().BeTrue();
        }
    }

    [Theory]
    [InlineData( true, null )]
    [InlineData( true, 0 )]
    [InlineData( false, null )]
    [InlineData( false, 1 )]
    public void IgnoreMemberWhenNull_ShouldCreateConfigurationWithCorrectIsIgnoredWhenNull(bool value, int? parameterIndex)
    {
        var sut = SqlParameterConfiguration.IgnoreMemberWhenNull( "foo", value, parameterIndex );

        using ( new AssertionScope() )
        {
            sut.MemberName.Should().Be( "foo" );
            sut.TargetParameterName.Should().Be( "foo" );
            sut.ParameterIndex.Should().Be( parameterIndex );
            sut.CustomSelector.Should().BeNull();
            sut.CustomSelectorSourceType.Should().BeNull();
            sut.CustomSelectorValueType.Should().BeNull();
            sut.IsIgnoredWhenNull.Should().Be( value );
            sut.IsIgnored.Should().BeFalse();
        }
    }

    [Theory]
    [InlineData( 0, null )]
    [InlineData( 1, true )]
    [InlineData( 2, null )]
    [InlineData( 3, false )]
    public void Positional_ShouldCreateConfigurationWithCorrectParameterIndex(int parameterIndex, bool? ignoreNull)
    {
        var sut = SqlParameterConfiguration.Positional( "foo", parameterIndex, ignoreNull );

        using ( new AssertionScope() )
        {
            sut.MemberName.Should().Be( "foo" );
            sut.TargetParameterName.Should().Be( "foo" );
            sut.ParameterIndex.Should().Be( parameterIndex );
            sut.CustomSelector.Should().BeNull();
            sut.CustomSelectorSourceType.Should().BeNull();
            sut.CustomSelectorValueType.Should().BeNull();
            sut.IsIgnoredWhenNull.Should().Be( ignoreNull );
            sut.IsIgnored.Should().BeFalse();
        }
    }

    [Fact]
    public void Positional_ShouldThrowArgumentOutOfRangeException_WhenParameterIndexIsLessThanZero()
    {
        var action = Lambda.Of( () => SqlParameterConfiguration.Positional( "foo", -1 ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
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

        using ( new AssertionScope() )
        {
            sut.MemberName.Should().Be( "bar" );
            sut.TargetParameterName.Should().Be( "foo" );
            sut.ParameterIndex.Should().Be( parameterIndex );
            sut.CustomSelector.Should().BeNull();
            sut.CustomSelectorSourceType.Should().BeNull();
            sut.CustomSelectorValueType.Should().BeNull();
            sut.IsIgnoredWhenNull.Should().Be( ignoreNull );
            sut.IsIgnored.Should().BeFalse();
        }
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

        using ( new AssertionScope() )
        {
            sut.MemberName.Should().BeNull();
            sut.TargetParameterName.Should().Be( "foo" );
            sut.ParameterIndex.Should().Be( parameterIndex );
            sut.CustomSelector.Should().BeSameAs( selector );
            sut.CustomSelectorSourceType.Should().Be( typeof( int ) );
            sut.CustomSelectorValueType.Should().Be( typeof( string ) );
            sut.IsIgnoredWhenNull.Should().Be( ignoreNull );
            sut.IsIgnored.Should().BeFalse();
        }
    }
}
