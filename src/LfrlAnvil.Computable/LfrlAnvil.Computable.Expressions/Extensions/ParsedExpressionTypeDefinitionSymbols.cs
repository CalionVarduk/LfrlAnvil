using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Extensions;

public readonly struct ParsedExpressionTypeDefinitionSymbols
{
    public static readonly ParsedExpressionTypeDefinitionSymbols Empty = new ParsedExpressionTypeDefinitionSymbols();

    private readonly bool _isPrefixTypeConverterDisabled;
    private readonly bool _isConstantDisabled;
    private readonly StringSlice? _name;
    private readonly StringSlice? _customPrefixTypeConverter;
    private readonly StringSlice? _postfixTypeConverter;
    private readonly StringSlice? _customConstant;

    private ParsedExpressionTypeDefinitionSymbols(
        bool isPrefixTypeConverterDisabled,
        bool isConstantDisabled,
        StringSlice? name,
        StringSlice? customPrefixTypeConverter,
        StringSlice? postfixTypeConverter,
        StringSlice? customConstant)
    {
        _isPrefixTypeConverterDisabled = isPrefixTypeConverterDisabled;
        _isConstantDisabled = isConstantDisabled;
        _name = name;
        _customPrefixTypeConverter = customPrefixTypeConverter;
        _postfixTypeConverter = postfixTypeConverter;
        _customConstant = customConstant;
    }

    public StringSlice Name => _name ?? StringSlice.Empty;
    public StringSlice? PrefixTypeConverter => _customPrefixTypeConverter ?? GetDefaultPrefixTypeConverter();
    public StringSlice? PostfixTypeConverter => _postfixTypeConverter;
    public StringSlice? Constant => _customConstant ?? GetDefaultConstant();

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
        return SetName( name.AsSlice() );
    }

    [Pure]
    public ParsedExpressionTypeDefinitionSymbols SetName(StringSlice name)
    {
        return new ParsedExpressionTypeDefinitionSymbols(
            _isPrefixTypeConverterDisabled,
            _isConstantDisabled,
            name,
            _customPrefixTypeConverter,
            _postfixTypeConverter,
            _customConstant );
    }

    [Pure]
    public ParsedExpressionTypeDefinitionSymbols SetPrefixTypeConverter(string symbol)
    {
        return SetPrefixTypeConverter( symbol.AsSlice() );
    }

    [Pure]
    public ParsedExpressionTypeDefinitionSymbols SetPrefixTypeConverter(StringSlice symbol)
    {
        return new ParsedExpressionTypeDefinitionSymbols(
            isPrefixTypeConverterDisabled: false,
            _isConstantDisabled,
            _name,
            symbol,
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
        return SetPostfixTypeConverter( symbol.AsSlice() );
    }

    [Pure]
    public ParsedExpressionTypeDefinitionSymbols SetPostfixTypeConverter(StringSlice symbol)
    {
        return new ParsedExpressionTypeDefinitionSymbols(
            _isPrefixTypeConverterDisabled,
            _isConstantDisabled,
            _name,
            _customPrefixTypeConverter,
            symbol,
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
        return SetConstant( symbol.AsSlice() );
    }

    [Pure]
    public ParsedExpressionTypeDefinitionSymbols SetConstant(StringSlice symbol)
    {
        return new ParsedExpressionTypeDefinitionSymbols(
            _isPrefixTypeConverterDisabled,
            isConstantDisabled: false,
            _name,
            _customPrefixTypeConverter,
            _postfixTypeConverter,
            symbol );
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
    private StringSlice? GetDefaultPrefixTypeConverter()
    {
        if ( _isPrefixTypeConverterDisabled )
            return null;

        var name = _name ?? new StringSlice( string.Empty );
        return $"[{name}]".AsSlice();
    }

    [Pure]
    private StringSlice? GetDefaultConstant()
    {
        if ( _isConstantDisabled )
            return null;

        var name = _name ?? new StringSlice( string.Empty );
        return name.ToString().ToUpperInvariant().AsSlice();
    }
}
