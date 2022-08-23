﻿using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Extensions;

public readonly struct ParsedExpressionBranchingVariadicFunctionSymbols
{
    public static readonly ParsedExpressionBranchingVariadicFunctionSymbols
        Default = new ParsedExpressionBranchingVariadicFunctionSymbols();

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

    public ReadOnlyMemory<char> If => _if?.AsMemory() ?? ParsedExpressionConstructDefaults.IfSymbol.AsMemory();
    public ReadOnlyMemory<char> SwitchCase => _switchCase?.AsMemory() ?? ParsedExpressionConstructDefaults.SwitchCaseSymbol.AsMemory();
    public ReadOnlyMemory<char> Switch => _switch?.AsMemory() ?? ParsedExpressionConstructDefaults.SwitchSymbol.AsMemory();
    public ReadOnlyMemory<char> Throw => _throw?.AsMemory() ?? ParsedExpressionConstructDefaults.ThrowSymbol.AsMemory();

    [Pure]
    public override string ToString()
    {
        return
            $"{nameof( If )}: '{If}', {nameof( Switch )}: '{Switch}', {nameof( SwitchCase )}: '{SwitchCase}', {nameof( Throw )}: '{Throw}'";
    }

    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetIf(string symbol)
    {
        return SetIf( symbol.AsMemory() );
    }

    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetIf(ReadOnlyMemory<char> symbol)
    {
        return new ParsedExpressionBranchingVariadicFunctionSymbols( StringSlice.Create( symbol ), _switchCase, _switch, _throw );
    }

    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetSwitchCase(string symbol)
    {
        return SetSwitchCase( symbol.AsMemory() );
    }

    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetSwitchCase(ReadOnlyMemory<char> symbol)
    {
        return new ParsedExpressionBranchingVariadicFunctionSymbols( _if, StringSlice.Create( symbol ), _switch, _throw );
    }

    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetSwitch(string symbol)
    {
        return SetSwitch( symbol.AsMemory() );
    }

    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetSwitch(ReadOnlyMemory<char> symbol)
    {
        return new ParsedExpressionBranchingVariadicFunctionSymbols( _if, _switchCase, StringSlice.Create( symbol ), _throw );
    }

    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetThrow(string symbol)
    {
        return SetThrow( symbol.AsMemory() );
    }

    [Pure]
    public ParsedExpressionBranchingVariadicFunctionSymbols SetThrow(ReadOnlyMemory<char> symbol)
    {
        return new ParsedExpressionBranchingVariadicFunctionSymbols( _if, _switchCase, _switch, StringSlice.Create( symbol ) );
    }
}