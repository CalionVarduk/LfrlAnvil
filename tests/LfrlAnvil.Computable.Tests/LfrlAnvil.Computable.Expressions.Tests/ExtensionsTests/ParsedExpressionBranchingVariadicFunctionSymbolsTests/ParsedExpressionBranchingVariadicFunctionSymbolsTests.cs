using LfrlAnvil.Computable.Expressions.Extensions;

namespace LfrlAnvil.Computable.Expressions.Tests.ExtensionsTests.ParsedExpressionBranchingVariadicFunctionSymbolsTests;

public class ParsedExpressionBranchingVariadicFunctionSymbolsTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnWithEmptyName()
    {
        var sut = default( ParsedExpressionBranchingVariadicFunctionSymbols );

        Assertion.All(
                sut.If.ToString().TestEquals( "if" ),
                sut.SwitchCase.ToString().TestEquals( "case" ),
                sut.Switch.ToString().TestEquals( "switch" ),
                sut.Throw.ToString().TestEquals( "throw" ) )
            .Go();
    }

    [Fact]
    public void StaticDefault_ShouldReturnWithEmptyName()
    {
        var sut = ParsedExpressionBranchingVariadicFunctionSymbols.Default;

        Assertion.All(
                sut.If.ToString().TestEquals( "if" ),
                sut.SwitchCase.ToString().TestEquals( "case" ),
                sut.Switch.ToString().TestEquals( "switch" ),
                sut.Throw.ToString().TestEquals( "throw" ) )
            .Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var sut = new ParsedExpressionBranchingVariadicFunctionSymbols()
            .SetIf( "foo" )
            .SetSwitchCase( "bar" )
            .SetSwitch( "qux" )
            .SetThrow( "foobar" );

        var result = sut.ToString();

        result.TestEquals( "If: 'foo', Switch: 'qux', SwitchCase: 'bar', Throw: 'foobar'" ).Go();
    }

    [Fact]
    public void SetIf_ShouldReturnCorrectResult()
    {
        var sut = new ParsedExpressionBranchingVariadicFunctionSymbols().SetIf( "foo" );

        Assertion.All(
                sut.If.ToString().TestEquals( "foo" ),
                sut.SwitchCase.ToString().TestEquals( "case" ),
                sut.Switch.ToString().TestEquals( "switch" ),
                sut.Throw.ToString().TestEquals( "throw" ) )
            .Go();
    }

    [Fact]
    public void SetSwitchCase_ShouldReturnCorrectResult()
    {
        var sut = new ParsedExpressionBranchingVariadicFunctionSymbols().SetSwitchCase( "foo" );

        Assertion.All(
                sut.If.ToString().TestEquals( "if" ),
                sut.SwitchCase.ToString().TestEquals( "foo" ),
                sut.Switch.ToString().TestEquals( "switch" ),
                sut.Throw.ToString().TestEquals( "throw" ) )
            .Go();
    }

    [Fact]
    public void SetSwitch_ShouldReturnCorrectResult()
    {
        var sut = new ParsedExpressionBranchingVariadicFunctionSymbols().SetSwitch( "foo" );

        Assertion.All(
                sut.If.ToString().TestEquals( "if" ),
                sut.SwitchCase.ToString().TestEquals( "case" ),
                sut.Switch.ToString().TestEquals( "foo" ),
                sut.Throw.ToString().TestEquals( "throw" ) )
            .Go();
    }

    [Fact]
    public void SetThrow_ShouldReturnCorrectResult()
    {
        var sut = new ParsedExpressionBranchingVariadicFunctionSymbols().SetThrow( "foo" );

        Assertion.All(
                sut.If.ToString().TestEquals( "if" ),
                sut.SwitchCase.ToString().TestEquals( "case" ),
                sut.Switch.ToString().TestEquals( "switch" ),
                sut.Throw.ToString().TestEquals( "foo" ) )
            .Go();
    }
}
