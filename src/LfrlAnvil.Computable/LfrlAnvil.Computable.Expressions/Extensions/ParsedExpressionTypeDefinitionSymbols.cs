using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Extensions;

public readonly struct ParsedExpressionTypeDefinitionSymbols
{
    public static readonly ParsedExpressionTypeDefinitionSymbols Empty = new ParsedExpressionTypeDefinitionSymbols();

    private readonly bool _isPrefixTypeConverterDisabled;
    private readonly bool _isConstantDisabled;
    private readonly StringSegment? _name;
    private readonly StringSegment? _customPrefixTypeConverter;
    private readonly StringSegment? _postfixTypeConverter;
    private readonly StringSegment? _customConstant;

    private ParsedExpressionTypeDefinitionSymbols(
        bool isPrefixTypeConverterDisabled,
        bool isConstantDisabled,
        StringSegment? name,
        StringSegment? customPrefixTypeConverter,
        StringSegment? postfixTypeConverter,
        StringSegment? customConstant)
    {
        _isPrefixTypeConverterDisabled = isPrefixTypeConverterDisabled;
        _isConstantDisabled = isConstantDisabled;
        _name = name;
        _customPrefixTypeConverter = customPrefixTypeConverter;
        _postfixTypeConverter = postfixTypeConverter;
        _customConstant = customConstant;
    }

    public StringSegment Name => _name ?? StringSegment.Empty;
    public StringSegment? PrefixTypeConverter => _customPrefixTypeConverter ?? GetDefaultPrefixTypeConverter();
    public StringSegment? PostfixTypeConverter => _postfixTypeConverter;
    public StringSegment? Constant => _customConstant ?? GetDefaultConstant();

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
        return SetName( name.AsSegment() );
    }

    [Pure]
    public ParsedExpressionTypeDefinitionSymbols SetName(StringSegment name)
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
        return SetPrefixTypeConverter( symbol.AsSegment() );
    }

    [Pure]
    public ParsedExpressionTypeDefinitionSymbols SetPrefixTypeConverter(StringSegment symbol)
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
        return SetPostfixTypeConverter( symbol.AsSegment() );
    }

    [Pure]
    public ParsedExpressionTypeDefinitionSymbols SetPostfixTypeConverter(StringSegment symbol)
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
        return SetConstant( symbol.AsSegment() );
    }

    [Pure]
    public ParsedExpressionTypeDefinitionSymbols SetConstant(StringSegment symbol)
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
    private StringSegment? GetDefaultPrefixTypeConverter()
    {
        if ( _isPrefixTypeConverterDisabled )
            return null;

        var name = _name ?? new StringSegment( string.Empty );
        return $"[{name}]".AsSegment();
    }

    [Pure]
    private StringSegment? GetDefaultConstant()
    {
        if ( _isConstantDisabled )
            return null;

        var name = _name ?? new StringSegment( string.Empty );
        return name.ToString().ToUpperInvariant().AsSegment();
    }
}
