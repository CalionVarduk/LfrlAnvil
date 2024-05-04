using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Expressions.Constructs;

namespace LfrlAnvil.Computable.Expressions.Extensions;

/// <summary>
/// Represents symbols that identify built-in branching variadic functions.
/// </summary>
public readonly struct ParsedExpressionBranchingVariadicFunctionSymbols
{
    /// <summary>
    /// Default <see cref="ParsedExpressionBranchingVariadicFunctionSymbols"/> instance.
    /// </summary>
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

    /// <summary>
    /// Symbol to use for the <b>if</b> variadic function. Equal to <b>if</b> by default.
    /// </summary>
    public StringSegment If => _if ?? ParsedExpressionConstructDefaults.IfSymbol;

    /// <summary>
    /// Symbol to use for the <b>switch-case</b> variadic function element. Equal to <b>case</b> by default.
    /// </summary>
    public StringSegment SwitchCase => _switchCase ?? ParsedExpressionConstructDefaults.SwitchCaseSymbol;

    /// <summary>
    /// Symbol to use for the <b>switch</b> variadic function. Equal to <b>switch</b> by default.
    /// </summary>
    public StringSegment Switch => _switch ?? ParsedExpressionConstructDefaults.SwitchSymbol;

    /// <summary>
    /// Symbol to use for the <b>throw</b> variadic function. Equal to <b>throw</b> by default.
    /// </summary>
    public StringSegment Throw => _throw ?? ParsedExpressionConstructDefaults.ThrowSymbol;

    /// <summary>
    /// Returns a string representation of this <see cref="ParsedExpressionBranchingVariadicFunctionSymbols"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return
            $"{nameof( If )}: '{If}', {nameof( Switch )}: '{Switch}', {nameof( SwitchCase )}: '{SwitchCase}', {nameof( Throw )}: '{Throw}'";
    }

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionBranchingVariadicFunctionSymbols"/> instance with changed <see cref="If"/> symbol.
    /// </summary>
    /// <param name="symbol"><see cref="If"/> symbol to set.</param>
    /// <returns>New <see cref="ParsedExpressionBranchingVariadicFunctionSymbols"/> instance.</returns>
    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetIf(StringSegment symbol)
    {
        return new ParsedExpressionBranchingVariadicFunctionSymbols( symbol, _switchCase, _switch, _throw );
    }

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionBranchingVariadicFunctionSymbols"/> instance with changed <see cref="SwitchCase"/> symbol.
    /// </summary>
    /// <param name="symbol"><see cref="SwitchCase"/> symbol to set.</param>
    /// <returns>New <see cref="ParsedExpressionBranchingVariadicFunctionSymbols"/> instance.</returns>
    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetSwitchCase(StringSegment symbol)
    {
        return new ParsedExpressionBranchingVariadicFunctionSymbols( _if, symbol, _switch, _throw );
    }

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionBranchingVariadicFunctionSymbols"/> instance with changed <see cref="Switch"/> symbol.
    /// </summary>
    /// <param name="symbol"><see cref="Switch"/> symbol to set.</param>
    /// <returns>New <see cref="ParsedExpressionBranchingVariadicFunctionSymbols"/> instance.</returns>
    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetSwitch(StringSegment symbol)
    {
        return new ParsedExpressionBranchingVariadicFunctionSymbols( _if, _switchCase, symbol, _throw );
    }

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionBranchingVariadicFunctionSymbols"/> instance with changed <see cref="Throw"/> symbol.
    /// </summary>
    /// <param name="symbol"><see cref="Throw"/> symbol to set.</param>
    /// <returns>New <see cref="ParsedExpressionBranchingVariadicFunctionSymbols"/> instance.</returns>
    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetThrow(StringSegment symbol)
    {
        return new ParsedExpressionBranchingVariadicFunctionSymbols( _if, _switchCase, _switch, symbol );
    }
}
