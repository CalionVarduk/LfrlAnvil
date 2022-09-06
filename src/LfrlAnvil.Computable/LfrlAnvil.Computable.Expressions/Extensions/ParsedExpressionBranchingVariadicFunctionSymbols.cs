using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Extensions;

public readonly struct ParsedExpressionBranchingVariadicFunctionSymbols
{
    public static readonly ParsedExpressionBranchingVariadicFunctionSymbols Default =
        new ParsedExpressionBranchingVariadicFunctionSymbols();

    private readonly StringSlice? _if;
    private readonly StringSlice? _switchCase;
    private readonly StringSlice? _switch;
    private readonly StringSlice? _throw;

    private ParsedExpressionBranchingVariadicFunctionSymbols(
        StringSlice? @if,
        StringSlice? switchCase,
        StringSlice? @switch,
        StringSlice? @throw)
    {
        _if = @if;
        _switchCase = switchCase;
        _switch = @switch;
        _throw = @throw;
    }

    public StringSlice If => _if ?? ParsedExpressionConstructDefaults.IfSymbol.AsSlice();
    public StringSlice SwitchCase => _switchCase ?? ParsedExpressionConstructDefaults.SwitchCaseSymbol.AsSlice();
    public StringSlice Switch => _switch ?? ParsedExpressionConstructDefaults.SwitchSymbol.AsSlice();
    public StringSlice Throw => _throw ?? ParsedExpressionConstructDefaults.ThrowSymbol.AsSlice();

    [Pure]
    public override string ToString()
    {
        return
            $"{nameof( If )}: '{If}', {nameof( Switch )}: '{Switch}', {nameof( SwitchCase )}: '{SwitchCase}', {nameof( Throw )}: '{Throw}'";
    }

    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetIf(string symbol)
    {
        return SetIf( symbol.AsSlice() );
    }

    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetIf(StringSlice symbol)
    {
        return new ParsedExpressionBranchingVariadicFunctionSymbols( symbol, _switchCase, _switch, _throw );
    }

    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetSwitchCase(string symbol)
    {
        return SetSwitchCase( symbol.AsSlice() );
    }

    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetSwitchCase(StringSlice symbol)
    {
        return new ParsedExpressionBranchingVariadicFunctionSymbols( _if, symbol, _switch, _throw );
    }

    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetSwitch(string symbol)
    {
        return SetSwitch( symbol.AsSlice() );
    }

    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetSwitch(StringSlice symbol)
    {
        return new ParsedExpressionBranchingVariadicFunctionSymbols( _if, _switchCase, symbol, _throw );
    }

    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetThrow(string symbol)
    {
        return SetThrow( symbol.AsSlice() );
    }

    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetThrow(StringSlice symbol)
    {
        return new ParsedExpressionBranchingVariadicFunctionSymbols( _if, _switchCase, _switch, symbol );
    }
}
