using FluentAssertions.Execution;
using LfrlAnvil.Computable.Expressions.Extensions;

namespace LfrlAnvil.Computable.Expressions.Tests.ExtensionsTests.ParsedExpressionBranchingVariadicFunctionSymbolsTests;

public class ParsedExpressionBranchingVariadicFunctionSymbolsTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnWithEmptyName()
    {
        var sut = default( ParsedExpressionBranchingVariadicFunctionSymbols );

        using ( new AssertionScope() )
        {
            sut.If.ToString().Should().Be( "if" );
            sut.SwitchCase.ToString().Should().Be( "case" );
            sut.Switch.ToString().Should().Be( "switch" );
        }
    }

    [Fact]
    public void StaticDefault_ShouldReturnWithEmptyName()
    {
        var sut = ParsedExpressionBranchingVariadicFunctionSymbols.Default;

        using ( new AssertionScope() )
        {
            sut.If.ToString().Should().Be( "if" );
            sut.SwitchCase.ToString().Should().Be( "case" );
            sut.Switch.ToString().Should().Be( "switch" );
        }
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var sut = new ParsedExpressionBranchingVariadicFunctionSymbols()
            .SetIf( "foo" )
            .SetSwitchCase( "bar" )
            .SetSwitch( "qux" );

        var result = sut.ToString();

        result.Should().Be( "If: 'foo', Switch: 'qux', SwitchCase: 'bar'" );
    }

    [Fact]
    public void SetIf_ShouldReturnCorrectResult()
    {
        var sut = new ParsedExpressionBranchingVariadicFunctionSymbols()
            .SetIf( "foo" );

        using ( new AssertionScope() )
        {
            sut.If.ToString().Should().Be( "foo" );
            sut.SwitchCase.ToString().Should().Be( "case" );
            sut.Switch.ToString().Should().Be( "switch" );
        }
    }

    [Fact]
    public void SetSwitchCase_ShouldReturnCorrectResult()
    {
        var sut = new ParsedExpressionBranchingVariadicFunctionSymbols()
            .SetSwitchCase( "foo" );

        using ( new AssertionScope() )
        {
            sut.If.ToString().Should().Be( "if" );
            sut.SwitchCase.ToString().Should().Be( "foo" );
            sut.Switch.ToString().Should().Be( "switch" );
        }
    }

    [Fact]
    public void SetSwitch_ShouldReturnCorrectResult()
    {
        var sut = new ParsedExpressionBranchingVariadicFunctionSymbols()
            .SetSwitch( "foo" );

        using ( new AssertionScope() )
        {
            sut.If.ToString().Should().Be( "if" );
            sut.SwitchCase.ToString().Should().Be( "case" );
            sut.Switch.ToString().Should().Be( "foo" );
        }
    }
}
