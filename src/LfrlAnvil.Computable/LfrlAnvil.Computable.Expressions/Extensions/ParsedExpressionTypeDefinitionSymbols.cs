// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Computable.Expressions.Extensions;

/// <summary>
/// Represents that identify elements of a type definition construct.
/// </summary>
public readonly struct ParsedExpressionTypeDefinitionSymbols
{
    /// <summary>
    /// Default <see cref="ParsedExpressionTypeDefinitionSymbols"/> instance.
    /// </summary>
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

    /// <summary>
    /// Symbol to use as an identifier of the type and a name of its constructor.
    /// </summary>
    public StringSegment Name => _name ?? StringSegment.Empty;

    /// <summary>
    /// Symbol to use for prefix type converter. Equal to <see cref="Name"/> wrapped in square brackets by default.
    /// </summary>
    public StringSegment? PrefixTypeConverter => _customPrefixTypeConverter ?? GetDefaultPrefixTypeConverter();

    /// <summary>
    /// Optional symbol to use for postfix type converter.
    /// </summary>
    public StringSegment? PostfixTypeConverter => _postfixTypeConverter;

    /// <summary>
    /// Symbol to use for a constant of <see cref="Type"/> type. Equal to an uppercase <see cref="Name"/> by default.
    /// </summary>
    public StringSegment? Constant => _customConstant ?? GetDefaultConstant();

    /// <summary>
    /// Returns a string representation of this <see cref="ParsedExpressionTypeDefinitionSymbols"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
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

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionTypeDefinitionSymbols"/> instance with changed <see cref="Name"/> symbol.
    /// </summary>
    /// <param name="name"><see cref="Name"/> symbol to set.</param>
    /// <returns>New <see cref="ParsedExpressionTypeDefinitionSymbols"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionTypeDefinitionSymbols"/> instance with changed <see cref="PrefixTypeConverter"/> symbol.
    /// </summary>
    /// <param name="symbol"><see cref="PrefixTypeConverter"/> symbol to set.</param>
    /// <returns>New <see cref="ParsedExpressionTypeDefinitionSymbols"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionTypeDefinitionSymbols"/> instance with default <see cref="PrefixTypeConverter"/> symbol.
    /// </summary>
    /// <returns>New <see cref="ParsedExpressionTypeDefinitionSymbols"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionTypeDefinitionSymbols"/> instance with disabled <see cref="PrefixTypeConverter"/> symbol.
    /// </summary>
    /// <returns>New <see cref="ParsedExpressionTypeDefinitionSymbols"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionTypeDefinitionSymbols"/> instance with changed <see cref="PostfixTypeConverter"/> symbol.
    /// </summary>
    /// <param name="symbol"><see cref="PostfixTypeConverter"/> symbol to set.</param>
    /// <returns>New <see cref="ParsedExpressionTypeDefinitionSymbols"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionTypeDefinitionSymbols"/> instance with disabled <see cref="PostfixTypeConverter"/> symbol.
    /// </summary>
    /// <returns>New <see cref="ParsedExpressionTypeDefinitionSymbols"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionTypeDefinitionSymbols"/> instance with changed <see cref="Constant"/> symbol.
    /// </summary>
    /// <param name="symbol"><see cref="Constant"/> symbol to set.</param>
    /// <returns>New <see cref="ParsedExpressionTypeDefinitionSymbols"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionTypeDefinitionSymbols"/> instance with default <see cref="Constant"/> symbol.
    /// </summary>
    /// <returns>New <see cref="ParsedExpressionTypeDefinitionSymbols"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionTypeDefinitionSymbols"/> instance with disabled <see cref="Constant"/> symbol.
    /// </summary>
    /// <returns>New <see cref="ParsedExpressionTypeDefinitionSymbols"/> instance.</returns>
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
        return $"[{name}]";
    }

    [Pure]
    private StringSegment? GetDefaultConstant()
    {
        if ( _isConstantDisabled )
            return null;

        var name = _name ?? new StringSegment( string.Empty );
        return name.ToString().ToUpperInvariant();
    }
}
