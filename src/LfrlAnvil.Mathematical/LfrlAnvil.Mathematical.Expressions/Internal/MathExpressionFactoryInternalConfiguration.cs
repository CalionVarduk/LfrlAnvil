using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using LfrlAnvil.Mathematical.Expressions.Constructs;
using LfrlAnvil.Mathematical.Expressions.Exceptions;

namespace LfrlAnvil.Mathematical.Expressions.Internal;

public sealed class MathExpressionFactoryInternalConfiguration : IMathExpressionFactoryConfiguration
{
    internal MathExpressionFactoryInternalConfiguration(
        IReadOnlyDictionary<StringSlice, MathExpressionConstructTokenDefinition> constructs,
        IMathExpressionFactoryConfiguration configuration)
    {
        Constructs = constructs;
        DecimalPoint = configuration.DecimalPoint;
        IntegerDigitSeparator = configuration.IntegerDigitSeparator;
        ScientificNotationExponents = new string( configuration.ScientificNotationExponents.Distinct().ToArray() );
        AllowNonIntegerNumbers = configuration.AllowNonIntegerNumbers;
        AllowScientificNotation = configuration.AllowScientificNotation;
        StringDelimiter = configuration.StringDelimiter;
        ConvertResultToOutputTypeAutomatically = configuration.ConvertResultToOutputTypeAutomatically;
        NumberFormatProvider = new FormatProvider( this );
    }

    public char DecimalPoint { get; }
    public char IntegerDigitSeparator { get; }
    public string ScientificNotationExponents { get; }
    public bool AllowNonIntegerNumbers { get; }
    public bool AllowScientificNotation { get; }
    public char StringDelimiter { get; }
    public bool ConvertResultToOutputTypeAutomatically { get; }
    public IFormatProvider NumberFormatProvider { get; }
    internal IReadOnlyDictionary<StringSlice, MathExpressionConstructTokenDefinition> Constructs { get; }

    [Pure]
    public NumberStyles GetNumberStyles()
    {
        var result = NumberStyles.AllowThousands;

        if ( AllowNonIntegerNumbers )
            result |= NumberStyles.AllowDecimalPoint;

        if ( AllowScientificNotation )
            result |= NumberStyles.AllowExponent;

        return result;
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
    internal MathExpressionTypeConverter? FindFirstValidTypeConverter(Type inputType, Type outputType)
    {
        foreach ( var (_, definition) in Constructs )
        {
            if ( definition.Type != MathExpressionConstructTokenType.TypeConverter )
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

    private sealed class FormatProvider : IFormatProvider
    {
        private readonly NumberFormatInfo _info;

        internal FormatProvider(MathExpressionFactoryInternalConfiguration configuration)
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
            Debug.Assert( formatType == typeof( NumberFormatInfo ), "only NumberFormatInfo is supported" );
            return _info;
        }
    }
}
