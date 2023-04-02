using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Extensions;

public readonly struct ParsedExpressionBranchingVariadicFunctionSymbols
{
    public static readonly ParsedExpressionBranchingVariadicFunctionSymbols Default =
        new ParsedExpressionBranchingVariadicFunctionSymbols();

    private readonly StringSegment? _if;
    private readonly StringSegment? _switchCase;
    private readonly StringSegment? _switch;
    private readonly StringSegment? _throw;

    private ParsedExpressionBranchingVariadicFunctionSymbols(
        StringSegment? @if,
        StringSegment? switchCase,
        StringSegment? @switch,
        StringSegment? @throw)
    {
        _if = @if;
        _switchCase = switchCase;
        _switch = @switch;
        _throw = @throw;
    }

    public StringSegment If => _if ?? ParsedExpressionConstructDefaults.IfSymbol.AsSegment();
    public StringSegment SwitchCase => _switchCase ?? ParsedExpressionConstructDefaults.SwitchCaseSymbol.AsSegment();
    public StringSegment Switch => _switch ?? ParsedExpressionConstructDefaults.SwitchSymbol.AsSegment();
    public StringSegment Throw => _throw ?? ParsedExpressionConstructDefaults.ThrowSymbol.AsSegment();

    [Pure]
    public override string ToString()
    {
        return
            $"{nameof( If )}: '{If}', {nameof( Switch )}: '{Switch}', {nameof( SwitchCase )}: '{SwitchCase}', {nameof( Throw )}: '{Throw}'";
    }

    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetIf(string symbol)
    {
        return SetIf( symbol.AsSegment() );
    }

    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetIf(StringSegment symbol)
    {
        return new ParsedExpressionBranchingVariadicFunctionSymbols( symbol, _switchCase, _switch, _throw );
    }

    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetSwitchCase(string symbol)
    {
        return SetSwitchCase( symbol.AsSegment() );
    }

    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetSwitchCase(StringSegment symbol)
    {
        return new ParsedExpressionBranchingVariadicFunctionSymbols( _if, symbol, _switch, _throw );
    }

    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetSwitch(string symbol)
    {
        return SetSwitch( symbol.AsSegment() );
    }

    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetSwitch(StringSegment symbol)
    {
        return new ParsedExpressionBranchingVariadicFunctionSymbols( _if, _switchCase, symbol, _throw );
    }

    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetThrow(string symbol)
    {
        return SetThrow( symbol.AsSegment() );
    }

    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetThrow(StringSegment symbol)
    {
        return new ParsedExpressionBranchingVariadicFunctionSymbols( _if, _switchCase, _switch, symbol );
    }
}
