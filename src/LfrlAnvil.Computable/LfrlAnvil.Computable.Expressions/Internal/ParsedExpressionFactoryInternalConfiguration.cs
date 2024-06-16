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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Exceptions;

namespace LfrlAnvil.Computable.Expressions.Internal;

/// <summary>
/// Represents an internal <see cref="IParsedExpressionFactory"/> configuration.
/// </summary>
public sealed class ParsedExpressionFactoryInternalConfiguration : IParsedExpressionFactoryConfiguration
{
    internal ParsedExpressionFactoryInternalConfiguration(
        IReadOnlyDictionary<StringSegment, ConstructTokenDefinition> constructs,
        IParsedExpressionFactoryConfiguration configuration)
    {
        Constructs = constructs;
        DecimalPoint = configuration.DecimalPoint;
        IntegerDigitSeparator = configuration.IntegerDigitSeparator;
        ScientificNotationExponents = new string( configuration.ScientificNotationExponents.Distinct().ToArray() );
        AllowNonIntegerNumbers = configuration.AllowNonIntegerNumbers;
        AllowScientificNotation = configuration.AllowScientificNotation;
        StringDelimiter = configuration.StringDelimiter;
        ConvertResultToOutputTypeAutomatically = configuration.ConvertResultToOutputTypeAutomatically;
        AllowNonPublicMemberAccess = configuration.AllowNonPublicMemberAccess;
        IgnoreMemberNameCase = configuration.IgnoreMemberNameCase;
        PostponeStaticInlineDelegateCompilation = configuration.PostponeStaticInlineDelegateCompilation;
        DiscardUnusedArguments = configuration.DiscardUnusedArguments;

        NumberStyles = NumberStyles.AllowThousands;

        if ( AllowNonIntegerNumbers )
            NumberStyles |= NumberStyles.AllowDecimalPoint;

        if ( AllowScientificNotation )
            NumberStyles |= NumberStyles.AllowExponent;

        MemberBindingFlags = BindingFlags.Instance | BindingFlags.Public;
        if ( AllowNonPublicMemberAccess )
            MemberBindingFlags |= BindingFlags.NonPublic;

        NumberFormatProvider = new FormatProvider( this );
    }

    /// <inheritdoc />
    public char DecimalPoint { get; }

    /// <inheritdoc />
    public char IntegerDigitSeparator { get; }

    /// <inheritdoc />
    public string ScientificNotationExponents { get; }

    /// <inheritdoc />
    public bool AllowNonIntegerNumbers { get; }

    /// <inheritdoc />
    public bool AllowScientificNotation { get; }

    /// <inheritdoc />
    public char StringDelimiter { get; }

    /// <inheritdoc />
    public bool ConvertResultToOutputTypeAutomatically { get; }

    /// <inheritdoc />
    public bool AllowNonPublicMemberAccess { get; }

    /// <inheritdoc />
    public bool IgnoreMemberNameCase { get; }

    /// <inheritdoc />
    public bool PostponeStaticInlineDelegateCompilation { get; }

    /// <inheritdoc />
    public bool DiscardUnusedArguments { get; }

    /// <summary>
    /// Represents used <see cref="NumberStyles"/> by this configuration.
    /// </summary>
    public NumberStyles NumberStyles { get; }

    /// <summary>
    /// Represents used <see cref="BindingFlags"/> by this configuration used for locating members.
    /// </summary>
    public BindingFlags MemberBindingFlags { get; }

    /// <summary>
    /// Underlying <see cref="IFormatProvider"/> instance.
    /// </summary>
    public IFormatProvider NumberFormatProvider { get; }

    internal IReadOnlyDictionary<StringSegment, ConstructTokenDefinition> Constructs { get; }

    /// <summary>
    /// Attempts to find a range of all valid fields and properties for the provided type with a given name.
    /// </summary>
    /// <param name="type">Target type.</param>
    /// <param name="name">Member name.</param>
    /// <returns><see cref="MemberInfo"/> range of all valid fields and properties.</returns>
    [Pure]
    public MemberInfo[] FindTypeFieldsAndProperties(Type type, string name)
    {
        var result = MemberInfoLocator.FindFieldsAndProperties( type, MemberBindingFlags, GetAccessibleMemberFilter( name ) );
        return result;
    }

    /// <summary>
    /// Attempts to find a <see cref="MemberInfo"/> that represents an indexer for the provided type and parameter types.
    /// </summary>
    /// <param name="type">Target type.</param>
    /// <param name="parameterTypes">Target parameter types.</param>
    /// <returns><see cref="MemberInfo"/> or null when it was not found.</returns>
    [Pure]
    public MemberInfo? TryFindTypeIndexer(Type type, Type[] parameterTypes)
    {
        var result = MemberInfoLocator.TryFindIndexer( type, parameterTypes, MemberBindingFlags );
        return result;
    }

    /// <summary>
    /// Attempts to find a <see cref="ConstructorInfo"/> for the provided type and parameter types.
    /// </summary>
    /// <param name="type">Target type.</param>
    /// <param name="parameterTypes">Target parameter types.</param>
    /// <returns><see cref="ConstructorInfo"/> or null when it was not found.</returns>
    [Pure]
    public ConstructorInfo? TryFindTypeCtor(Type type, Type[] parameterTypes)
    {
        var result = MemberInfoLocator.TryFindCtor( type, parameterTypes, MemberBindingFlags );
        return result;
    }

    /// <summary>
    /// Attempts to find a range of all valid methods for the provided type with a given name and parameter types.
    /// </summary>
    /// <param name="type">Target type.</param>
    /// <param name="name">Member name.</param>
    ///  <param name="parameterTypes">Target parameter types.</param>
    /// <returns><see cref="MethodInfo"/> range of all valid methods.</returns>
    [Pure]
    public MethodInfo[] FindTypeMethods(Type type, string name, Type[] parameterTypes)
    {
        var result = MemberInfoLocator.FindMethods(
            type,
            parameterTypes,
            MemberBindingFlags,
            GetAccessibleMemberFilter( name ) );

        return result;
    }

    /// <summary>
    /// Creates a <see cref="MemberFilter"/> from this configuration and the given symbol.
    /// </summary>
    /// <param name="symbol">Symbol to filter by.</param>
    /// <returns>New <see cref="MemberFilter"/> instance.</returns>
    [Pure]
    public MemberFilter GetAccessibleMemberFilter(StringSegment symbol)
    {
        return IgnoreMemberNameCase
            ? (m, _) => symbol.Equals( m.Name, StringComparison.OrdinalIgnoreCase ) && IsMemberAccessible( m )
            : (m, _) => symbol.Equals( m.Name ) && IsMemberAccessible( m );
    }

    [Pure]
    internal bool TypeContainsMethod(Type type, StringSegment symbol)
    {
        var methods = type.GetMethods( MemberBindingFlags );
        var filter = GetAccessibleMemberFilter( symbol );

        for ( var i = 0; i < methods.Length; ++i )
        {
            if ( filter( methods[i], null ) )
                return true;
        }

        return false;
    }

    [Pure]
    internal Chain<string> Validate()
    {
        var errors = Chain<string>.Empty;

        if ( DecimalPoint == IntegerDigitSeparator )
            errors = errors.Extend( Resources.DecimalPointAndIntegerDigitSeparatorMustBeDifferent() );

        if ( DecimalPoint == StringDelimiter )
            errors = errors.Extend( Resources.DecimalPointAndStringDelimiterMustBeDifferent() );

        if ( ScientificNotationExponents.Contains( DecimalPoint ) )
            errors = errors.Extend( Resources.DecimalPointAndScientificNotationExponentsMustBeDifferent() );

        if ( IntegerDigitSeparator == StringDelimiter )
            errors = errors.Extend( Resources.IntegerDigitSeparatorAndStringDelimiterMustBeDifferent() );

        if ( ScientificNotationExponents.Contains( IntegerDigitSeparator ) )
            errors = errors.Extend( Resources.IntegerDigitSeparatorAndScientificNotationExponentsMustBeDifferent() );

        if ( ScientificNotationExponents.Contains( StringDelimiter ) )
            errors = errors.Extend( Resources.StringDelimiterAndScientificNotationExponentsMustBeDifferent() );

        if ( ! TokenValidation.IsNumberSymbolValid( DecimalPoint ) )
            errors = errors.Extend( Resources.InvalidDecimalPointSymbol( DecimalPoint ) );

        if ( ! TokenValidation.IsNumberSymbolValid( IntegerDigitSeparator ) )
            errors = errors.Extend( Resources.InvalidIntegerDigitSeparatorSymbol( IntegerDigitSeparator ) );

        if ( ! TokenValidation.IsStringDelimiterSymbolValid( StringDelimiter ) )
            errors = errors.Extend( Resources.InvalidStringDelimiterSymbol( StringDelimiter ) );

        foreach ( var symbol in ScientificNotationExponents )
        {
            if ( ! TokenValidation.IsExponentSymbolValid( symbol ) )
                errors = errors.Extend( Resources.InvalidScientificNotationExponentsSymbol( symbol ) );
        }

        if ( AllowScientificNotation && ScientificNotationExponents.Length == 0 )
            errors = errors.Extend( Resources.AtLeastOneScientificNotationExponentSymbolMustBeDefined() );

        return errors;
    }

    [Pure]
    internal ParsedExpressionTypeConverter? FindFirstValidTypeConverter(Type inputType, Type outputType)
    {
        foreach ( var (_, definition) in Constructs )
        {
            if ( ! definition.IsAny( ParsedExpressionConstructType.TypeConverter ) )
                continue;

            if ( definition.PrefixTypeConverters.TargetType == outputType )
            {
                var construct = definition.PrefixTypeConverters.FindConstruct( inputType );
                if ( construct is not null )
                    return construct;
            }

            if ( definition.PostfixTypeConverters.TargetType == outputType )
            {
                var construct = definition.PostfixTypeConverters.FindConstruct( inputType );
                if ( construct is not null )
                    return construct;
            }
        }

        return null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool IsMemberAccessible(MemberInfo member)
    {
        if ( member is not PropertyInfo property )
            return true;

        var getter = property.GetGetMethod( AllowNonPublicMemberAccess );
        return getter is not null && getter.GetParameters().Length == 0;
    }

    private sealed class FormatProvider : IFormatProvider
    {
        private readonly NumberFormatInfo _info;

        internal FormatProvider(ParsedExpressionFactoryInternalConfiguration configuration)
        {
            _info = CultureInfo.InvariantCulture.NumberFormat;

            if ( ! TokenConstants.AreEqual( _info.NumberDecimalSeparator, configuration.DecimalPoint ) )
            {
                _info = ReinterpretCast.To<NumberFormatInfo>( _info.Clone() );
                _info.NumberDecimalSeparator = configuration.DecimalPoint.ToString();
            }

            if ( ! TokenConstants.AreEqual( _info.NumberGroupSeparator, configuration.IntegerDigitSeparator ) )
            {
                if ( _info.IsReadOnly )
                    _info = ReinterpretCast.To<NumberFormatInfo>( _info.Clone() );

                _info.NumberGroupSeparator = configuration.IntegerDigitSeparator.ToString();
            }
        }

        [Pure]
        public object GetFormat(Type? formatType)
        {
            Assume.IsNotNull( formatType );
            Assume.Equals( formatType, typeof( NumberFormatInfo ) );
            return _info;
        }
    }
}
