using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Extensions;

public readonly struct ParsedExpressionTypeDefinitionSymbols
{
    public static readonly ParsedExpressionTypeDefinitionSymbols Empty = new ParsedExpressionTypeDefinitionSymbols();

    private readonly bool _isPrefixTypeConverterDisabled;
    private readonly bool _isConstantDisabled;
    private readonly StringSliceOld? _name;
    private readonly StringSliceOld? _customPrefixTypeConverter;
    private readonly StringSliceOld? _postfixTypeConverter;
    private readonly StringSliceOld? _customConstant;

    private ParsedExpressionTypeDefinitionSymbols(
        bool isPrefixTypeConverterDisabled,
        bool isConstantDisabled,
        StringSliceOld? name,
        StringSliceOld? customPrefixTypeConverter,
        StringSliceOld? postfixTypeConverter,
        StringSliceOld? customConstant)
    {
        _isPrefixTypeConverterDisabled = isPrefixTypeConverterDisabled;
        _isConstantDisabled = isConstantDisabled;
        _name = name;
        _customPrefixTypeConverter = customPrefixTypeConverter;
        _postfixTypeConverter = postfixTypeConverter;
        _customConstant = customConstant;
    }

    public ReadOnlyMemory<char> Name => _name?.AsMemory() ?? string.Empty.AsMemory();
    public ReadOnlyMemory<char>? PrefixTypeConverter => _customPrefixTypeConverter?.AsMemory() ?? GetDefaultPrefixTypeConverter();
    public ReadOnlyMemory<char>? PostfixTypeConverter => _postfixTypeConverter?.AsMemory();
    public ReadOnlyMemory<char>? Constant => _customConstant?.AsMemory() ?? GetDefaultConstant();

    [Pure]
    public override string ToString()
    {
        var prefix = PrefixTypeConverter;
        var postfix = PostfixTypeConverter;
        var constant = Constant;
        var prefixText = prefix is null ? string.Empty : $", {nameof( PrefixTypeConverter )}: '{prefix}'";
        var postfixText = postfix is null ? string.Empty : $", {nameof( PostfixTypeConverter )}: '{postfix}'";
        var constantText = constant is null ? string.Empty : $", {nameof( Constant )}: '{constant}'";
        return $"{nameof( Name )}: '{Name}'{prefixText}{postfixText}{constantText}";
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
            _isConstantDisabled,
            StringSliceOld.Create( name ),
            _customPrefixTypeConverter,
            _postfixTypeConverter,
            _customConstant );
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
            _isConstantDisabled,
            _name,
            StringSliceOld.Create( symbol ),
            _postfixTypeConverter,
            _customConstant );
    }

    [Pure]
    public ParsedExpressionTypeDefinitionSymbols SetDefaultPrefixTypeConverter()
    {
        return new ParsedExpressionTypeDefinitionSymbols(
            isPrefixTypeConverterDisabled: false,
            _isConstantDisabled,
            _name,
            customPrefixTypeConverter: null,
            _postfixTypeConverter,
            _customConstant );
    }

    [Pure]
    public ParsedExpressionTypeDefinitionSymbols DisablePrefixTypeConverter()
    {
        return new ParsedExpressionTypeDefinitionSymbols(
            isPrefixTypeConverterDisabled: true,
            _isConstantDisabled,
            _name,
            customPrefixTypeConverter: null,
            _postfixTypeConverter,
            _customConstant );
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
            _isConstantDisabled,
            _name,
            _customPrefixTypeConverter,
            StringSliceOld.Create( symbol ),
            _customConstant );
    }

    [Pure]
    public ParsedExpressionTypeDefinitionSymbols DisablePostfixTypeConverter()
    {
        return new ParsedExpressionTypeDefinitionSymbols(
            _isPrefixTypeConverterDisabled,
            _isConstantDisabled,
            _name,
            _customPrefixTypeConverter,
            postfixTypeConverter: null,
            _customConstant );
    }

    [Pure]
    public ParsedExpressionTypeDefinitionSymbols SetConstant(string symbol)
    {
        return SetConstant( symbol.AsMemory() );
    }

    [Pure]
    public ParsedExpressionTypeDefinitionSymbols SetConstant(ReadOnlyMemory<char> symbol)
    {
        return new ParsedExpressionTypeDefinitionSymbols(
            _isPrefixTypeConverterDisabled,
            isConstantDisabled: false,
            _name,
            _customPrefixTypeConverter,
            _postfixTypeConverter,
            StringSliceOld.Create( symbol ) );
    }

    [Pure]
    public ParsedExpressionTypeDefinitionSymbols SetDefaultConstant()
    {
        return new ParsedExpressionTypeDefinitionSymbols(
            _isPrefixTypeConverterDisabled,
            isConstantDisabled: false,
            _name,
            _customPrefixTypeConverter,
            _postfixTypeConverter,
            customConstant: null );
    }

    [Pure]
    public ParsedExpressionTypeDefinitionSymbols DisableConstant()
    {
        return new ParsedExpressionTypeDefinitionSymbols(
            _isPrefixTypeConverterDisabled,
            isConstantDisabled: true,
            _name,
            _customPrefixTypeConverter,
            _postfixTypeConverter,
            customConstant: null );
    }

    [Pure]
    private ReadOnlyMemory<char>? GetDefaultPrefixTypeConverter()
    {
        if ( _isPrefixTypeConverterDisabled )
            return null;

        var name = _name ?? StringSliceOld.Create( string.Empty );
        return $"[{name}]".AsMemory();
    }

    [Pure]
    private ReadOnlyMemory<char>? GetDefaultConstant()
    {
        if ( _isConstantDisabled )
            return null;

        var name = _name ?? StringSliceOld.Create( string.Empty );
        return name.ToString().ToUpperInvariant().AsMemory();
    }
}
