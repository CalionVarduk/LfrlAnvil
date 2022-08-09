using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Extensions;

public readonly struct ParsedExpressionTypeDefinitionSymbols
{
    public static readonly ParsedExpressionTypeDefinitionSymbols Empty = new ParsedExpressionTypeDefinitionSymbols();

    private readonly bool _isPrefixTypeConverterDisabled;
    private readonly StringSlice? _name;
    private readonly StringSlice? _customPrefixTypeConverter;
    private readonly StringSlice? _postfixTypeConverter;

    private ParsedExpressionTypeDefinitionSymbols(
        bool isPrefixTypeConverterDisabled,
        StringSlice? name,
        StringSlice? customPrefixTypeConverter,
        StringSlice? postfixTypeConverter)
    {
        _isPrefixTypeConverterDisabled = isPrefixTypeConverterDisabled;
        _name = name;
        _customPrefixTypeConverter = customPrefixTypeConverter;
        _postfixTypeConverter = postfixTypeConverter;
    }

    public ReadOnlyMemory<char> Name => _name?.AsMemory() ?? string.Empty.AsMemory();
    public ReadOnlyMemory<char>? PrefixTypeConverter => _customPrefixTypeConverter?.AsMemory() ?? GetDefaultPrefixTypeConverter();
    public ReadOnlyMemory<char>? PostfixTypeConverter => _postfixTypeConverter?.AsMemory();

    [Pure]
    public override string ToString()
    {
        var prefix = PrefixTypeConverter;
        var postfix = PostfixTypeConverter;
        var prefixText = prefix is null ? string.Empty : $", {nameof( PrefixTypeConverter )}: '{prefix}'";
        var postfixText = postfix is null ? string.Empty : $", {nameof( PostfixTypeConverter )}: '{postfix}'";
        return $"{nameof( Name )}: '{Name}'{prefixText}{postfixText}";
    }

    [Pure]
    public ParsedExpressionTypeDefinitionSymbols SetName(string name)
    {
        return SetName( name.AsMemory() );
    }

    [Pure]
    public ParsedExpressionTypeDefinitionSymbols SetName(ReadOnlyMemory<char> name)
    {
        return new ParsedExpressionTypeDefinitionSymbols(
            _isPrefixTypeConverterDisabled,
            StringSlice.Create( name ),
            _customPrefixTypeConverter,
            _postfixTypeConverter );
    }

    [Pure]
    public ParsedExpressionTypeDefinitionSymbols SetPrefixTypeConverter(string symbol)
    {
        return SetPrefixTypeConverter( symbol.AsMemory() );
    }

    [Pure]
    public ParsedExpressionTypeDefinitionSymbols SetPrefixTypeConverter(ReadOnlyMemory<char> symbol)
    {
        return new ParsedExpressionTypeDefinitionSymbols(
            isPrefixTypeConverterDisabled: false,
            _name,
            StringSlice.Create( symbol ),
            _postfixTypeConverter );
    }

    [Pure]
    public ParsedExpressionTypeDefinitionSymbols SetDefaultPrefixTypeConverter()
    {
        return new ParsedExpressionTypeDefinitionSymbols(
            isPrefixTypeConverterDisabled: false,
            _name,
            customPrefixTypeConverter: null,
            _postfixTypeConverter );
    }

    [Pure]
    public ParsedExpressionTypeDefinitionSymbols DisablePrefixTypeConverter()
    {
        return new ParsedExpressionTypeDefinitionSymbols(
            isPrefixTypeConverterDisabled: true,
            _name,
            customPrefixTypeConverter: null,
            _postfixTypeConverter );
    }

    [Pure]
    public ParsedExpressionTypeDefinitionSymbols SetPostfixTypeConverter(string symbol)
    {
        return SetPostfixTypeConverter( symbol.AsMemory() );
    }

    [Pure]
    public ParsedExpressionTypeDefinitionSymbols SetPostfixTypeConverter(ReadOnlyMemory<char> symbol)
    {
        return new ParsedExpressionTypeDefinitionSymbols(
            _isPrefixTypeConverterDisabled,
            _name,
            _customPrefixTypeConverter,
            StringSlice.Create( symbol ) );
    }

    [Pure]
    public ParsedExpressionTypeDefinitionSymbols DisablePostfixTypeConverter()
    {
        return new ParsedExpressionTypeDefinitionSymbols(
            _isPrefixTypeConverterDisabled,
            _name,
            _customPrefixTypeConverter,
            postfixTypeConverter: null );
    }

    [Pure]
    private ReadOnlyMemory<char>? GetDefaultPrefixTypeConverter()
    {
        if ( _isPrefixTypeConverterDisabled )
            return null;

        var name = _name ?? StringSlice.Create( string.Empty );
        return $"[{name}]".AsMemory();
    }
}
