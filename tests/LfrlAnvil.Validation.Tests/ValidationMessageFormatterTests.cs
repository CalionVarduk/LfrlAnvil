using System.Globalization;
using System.Text;

namespace LfrlAnvil.Validation.Tests;

public class ValidationMessageFormatterTests : TestsBase
{
    [Fact]
    public void Format_ShouldReturnNull_WhenStringBuilderIsNullAndMessagesAreEmpty()
    {
        var sut = new Formatter( () => ValidationMessageFormatterArgs.Default );
        var result = sut.Format( null, Chain<ValidationMessage<string>>.Empty );
        result.Should().BeNull();
    }

    [Fact]
    public void Format_ShouldDoNothingAndReturnBuilder_WhenStringBuilderIsNotNullAndMessagesAreEmpty()
    {
        var builder = new StringBuilder();
        var sut = new Formatter( () => ValidationMessageFormatterArgs.Default );

        var result = sut.Format( builder, Chain<ValidationMessage<string>>.Empty );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder );
            result.Length.Should().Be( 0 );
        }
    }

    [Fact]
    public void Format_ShouldCreateNewBuilderWithText_WhenStringBuilderIsNullAndMessagesAreNotEmpty()
    {
        var sut = new Formatter( () => ValidationMessageFormatterArgs.Default );

        var message1 = ValidationMessage.Create( "RESOURCE_ONE_{0}", 123 );
        var message2 = ValidationMessage.Create( "RESOURCE_TWO_{0}_{1}", 456, 789 );
        var expected = $"resource one 123{Environment.NewLine}resource two 456 789";

        var result = sut.Format( null, Chain.Create( message1 ).Extend( message2 ), CultureInfo.InvariantCulture );

        using ( new AssertionScope() )
        {
            result.Should().NotBeNull();
            if ( result is null )
                return;

            result.ToString().Should().Be( expected );
        }
    }

    [Fact]
    public void Format_ShouldAppendTextToBuilder_WhenStringBuilderIsNotNullAndMessagesAreNotEmpty()
    {
        var sut = new Formatter( () => ValidationMessageFormatterArgs.Default );

        var builder = new StringBuilder( "foobar " );
        var message1 = ValidationMessage.Create( "RESOURCE_ONE_{0}", 123 );
        var message2 = ValidationMessage.Create( "RESOURCE_TWO_{0}_{1}", 456, 789 );
        var expected = $"foobar resource one 123{Environment.NewLine}resource two 456 789";

        var result = sut.Format( builder, Chain.Create( message1 ).Extend( message2 ), CultureInfo.InvariantCulture );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( builder );
            result.ToString().Should().Be( expected );
        }
    }

    [Fact]
    public void Format_ShouldCreateNewBuilderWithText_WhenStringBuilderIsNullAndMessagesAreNotEmpty_WithPrefixAndPostfixAll()
    {
        var sut = new Formatter(
            () => ValidationMessageFormatterArgs.Default
                .SetPrefixAll( "[prefix all: {0}] " )
                .SetPostfixAll( " [postfix all]" ) );

        var message1 = ValidationMessage.Create( "RESOURCE_ONE_{0}", 123 );
        var message2 = ValidationMessage.Create( "RESOURCE_TWO_{0}_{1}", 456, 789 );
        var expected = $"[prefix all: 2] resource one 123{Environment.NewLine}resource two 456 789 [postfix all]";

        var result = sut.Format( null, Chain.Create( message1 ).Extend( message2 ), CultureInfo.InvariantCulture );

        using ( new AssertionScope() )
        {
            result.Should().NotBeNull();
            if ( result is null )
                return;

            result.ToString().Should().Be( expected );
        }
    }

    [Fact]
    public void Format_ShouldCreateNewBuilderWithText_WhenStringBuilderIsNullAndMessagesAreNotEmpty_WithPrefixAndPostfixEach()
    {
        var sut = new Formatter(
            () => ValidationMessageFormatterArgs.Default
                .SetPrefixEach( "[prefix each] " )
                .SetPostfixEach( " [postfix each]" ) );

        var message1 = ValidationMessage.Create( "RESOURCE_ONE_{0}", 123 );
        var message2 = ValidationMessage.Create( "RESOURCE_TWO_{0}_{1}", 456, 789 );
        var expected =
            $"[prefix each] resource one 123 [postfix each]{Environment.NewLine}[prefix each] resource two 456 789 [postfix each]";

        var result = sut.Format( null, Chain.Create( message1 ).Extend( message2 ), CultureInfo.InvariantCulture );

        using ( new AssertionScope() )
        {
            result.Should().NotBeNull();
            if ( result is null )
                return;

            result.ToString().Should().Be( expected );
        }
    }

    [Fact]
    public void Format_ShouldCreateNewBuilderWithText_WhenStringBuilderIsNullAndMessagesAreNotEmpty_WithIncludedIndex()
    {
        var sut = new Formatter(
            () => ValidationMessageFormatterArgs.Default
                .SetIncludeIndex( true ) );

        var message1 = ValidationMessage.Create( "RESOURCE_ONE_{0}", 123 );
        var message2 = ValidationMessage.Create( "RESOURCE_TWO_{0}_{1}", 456, 789 );
        var expected = $"1. resource one 123{Environment.NewLine}2. resource two 456 789";

        var result = sut.Format( null, Chain.Create( message1 ).Extend( message2 ), CultureInfo.InvariantCulture );

        using ( new AssertionScope() )
        {
            result.Should().NotBeNull();
            if ( result is null )
                return;

            result.ToString().Should().Be( expected );
        }
    }

    [Fact]
    public void Format_ShouldCreateNewBuilderWithText_WhenStringBuilderIsNullAndMessagesAreNotEmpty_WithAllArgsChanged()
    {
        var sut = new Formatter(
            () => ValidationMessageFormatterArgs.Default
                .SetPrefixAll( "[prefix all: {0}] " )
                .SetPostfixAll( " [postfix all]" )
                .SetPrefixEach( "[prefix each] " )
                .SetPostfixEach( " [postfix each]" )
                .SetSeparator( " & " )
                .SetIncludeIndex( true ) );

        var message1 = ValidationMessage.Create( "RESOURCE_ONE_{0}", 123 );
        var message2 = ValidationMessage.Create( "RESOURCE_TWO_{0}_{1}", 456, 789 );
        var expected =
            $"[prefix all: 2] 1. [prefix each] resource one 123 [postfix each] & 2. [prefix each] resource two 456 789 [postfix each] [postfix all]";

        var result = sut.Format( null, Chain.Create( message1 ).Extend( message2 ), CultureInfo.InvariantCulture );

        using ( new AssertionScope() )
        {
            result.Should().NotBeNull();
            if ( result is null )
                return;

            result.ToString().Should().Be( expected );
        }
    }

    private sealed class Formatter : ValidationMessageFormatter<string>
    {
        private readonly Func<ValidationMessageFormatterArgs> _args;

        internal Formatter(Func<ValidationMessageFormatterArgs> args)
        {
            _args = args;
        }

        public override string GetResourceTemplate(string resource, IFormatProvider? formatProvider)
        {
            return resource.ToLower().Replace( '_', ' ' );
        }

        public override ValidationMessageFormatterArgs GetArgs(IFormatProvider? formatProvider)
        {
            return _args();
        }
    }
}
