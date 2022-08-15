﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Exceptions;

namespace LfrlAnvil.Computable.Expressions.Internal;

public sealed class ParsedExpressionFactoryInternalConfiguration : IParsedExpressionFactoryConfiguration
{
    internal ParsedExpressionFactoryInternalConfiguration(
        IReadOnlyDictionary<StringSlice, ConstructTokenDefinition> constructs,
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

    public char DecimalPoint { get; }
    public char IntegerDigitSeparator { get; }
    public string ScientificNotationExponents { get; }
    public bool AllowNonIntegerNumbers { get; }
    public bool AllowScientificNotation { get; }
    public char StringDelimiter { get; }
    public bool ConvertResultToOutputTypeAutomatically { get; }
    public bool AllowNonPublicMemberAccess { get; }
    public bool IgnoreMemberNameCase { get; }
    public NumberStyles NumberStyles { get; }
    public BindingFlags MemberBindingFlags { get; }
    public IFormatProvider NumberFormatProvider { get; }
    internal IReadOnlyDictionary<StringSlice, ConstructTokenDefinition> Constructs { get; }

    [Pure]
    internal MemberFilter GetAccessibleMemberFilter(StringSlice symbol)
    {
        return IgnoreMemberNameCase
            ? (m, _) => symbol.EqualsIgnoreCase( StringSlice.Create( m.Name ) ) && IsMemberAccessible( m )
            : (m, _) => symbol.Equals( StringSlice.Create( m.Name ) ) && IsMemberAccessible( m );
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
            if ( ! definition.IsAny( ConstructTokenType.TypeConverter ) )
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

            if ( ! StringSlice.Create( _info.NumberDecimalSeparator ).Equals( configuration.DecimalPoint ) )
            {
                _info = (NumberFormatInfo)_info.Clone();
                _info.NumberDecimalSeparator = configuration.DecimalPoint.ToString();
            }

            if ( ! StringSlice.Create( _info.NumberGroupSeparator ).Equals( configuration.IntegerDigitSeparator ) )
            {
                if ( _info.IsReadOnly )
                    _info = (NumberFormatInfo)_info.Clone();

                _info.NumberGroupSeparator = configuration.IntegerDigitSeparator.ToString();
            }
        }

        [Pure]
        public object GetFormat(Type? formatType)
        {
            Assume.IsNotNull( formatType, nameof( formatType ) );
            Assume.Equals( formatType, typeof( NumberFormatInfo ), nameof( formatType ) );
            return _info;
        }
    }
}
