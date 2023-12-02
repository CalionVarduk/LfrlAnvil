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
    [InlineData( true )]
    [InlineData( false )]
    public void IgnoreMemberWhenNull_ShouldCreateConfigurationWithCorrectIsIgnoredWhenNull(bool value)
    {
        var sut = SqlParameterConfiguration.IgnoreMemberWhenNull( "foo", value );

        using ( new AssertionScope() )
        {
            sut.MemberName.Should().Be( "foo" );
            sut.TargetParameterName.Should().Be( "foo" );
            sut.CustomSelector.Should().BeNull();
            sut.CustomSelectorSourceType.Should().BeNull();
            sut.CustomSelectorValueType.Should().BeNull();
            sut.IsIgnoredWhenNull.Should().Be( value );
            sut.IsIgnored.Should().BeFalse();
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    [InlineData( null )]
    public void From_WithMemberName_ShouldCreateConfigurationWithDifferentMemberAndTargetParameterNames(bool? ignoreNull)
    {
        var sut = SqlParameterConfiguration.From( "foo", "bar", ignoreNull );

        using ( new AssertionScope() )
        {
            sut.MemberName.Should().Be( "bar" );
            sut.TargetParameterName.Should().Be( "foo" );
            sut.CustomSelector.Should().BeNull();
            sut.CustomSelectorSourceType.Should().BeNull();
            sut.CustomSelectorValueType.Should().BeNull();
            sut.IsIgnoredWhenNull.Should().Be( ignoreNull );
            sut.IsIgnored.Should().BeFalse();
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    [InlineData( null )]
    public void From_WithSelector_ShouldCreateConfigurationWithCustomSelector(bool? ignoreNull)
    {
        var selector = Lambda.ExpressionOf( (int source) => source.ToString() );
        var sut = SqlParameterConfiguration.From( "foo", selector, ignoreNull );

        using ( new AssertionScope() )
        {
            sut.MemberName.Should().BeNull();
            sut.TargetParameterName.Should().Be( "foo" );
            sut.CustomSelector.Should().BeSameAs( selector );
            sut.CustomSelectorSourceType.Should().Be( typeof( int ) );
            sut.CustomSelectorValueType.Should().Be( typeof( string ) );
            sut.IsIgnoredWhenNull.Should().Be( ignoreNull );
            sut.IsIgnored.Should().BeFalse();
        }
    }
}
