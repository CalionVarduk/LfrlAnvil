using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Numerics;

namespace LfrlAnvil.Computable.Expressions.Internal;

public static class ParsedExpressionNumberParser
{
    [Pure]
    public static IParsedExpressionNumberParser CreateDefaultDecimal(ParsedExpressionFactoryInternalConfiguration configuration)
    {
        return new DecimalParser( configuration.NumberFormatProvider, configuration.NumberStyles );
    }

    [Pure]
    public static IParsedExpressionNumberParser CreateDefaultDouble(ParsedExpressionFactoryInternalConfiguration configuration)
    {
        return new DoubleParser( configuration.NumberFormatProvider, configuration.NumberStyles );
    }

    [Pure]
    public static IParsedExpressionNumberParser CreateDefaultFloat(ParsedExpressionFactoryInternalConfiguration configuration)
    {
        return new FloatParser( configuration.NumberFormatProvider, configuration.NumberStyles );
    }

    [Pure]
    public static IParsedExpressionNumberParser CreateDefaultBigInteger(ParsedExpressionFactoryInternalConfiguration configuration)
    {
        return new BigIntegerParser( configuration.NumberFormatProvider, configuration.NumberStyles );
    }

    [Pure]
    public static IParsedExpressionNumberParser CreateDefaultInt64(ParsedExpressionFactoryInternalConfiguration configuration)
    {
        return new Int64Parser( configuration.NumberFormatProvider, configuration.NumberStyles );
    }

    [Pure]
    public static IParsedExpressionNumberParser CreateDefaultInt32(ParsedExpressionFactoryInternalConfiguration configuration)
    {
        return new Int32Parser( configuration.NumberFormatProvider, configuration.NumberStyles );
    }

    private sealed class DecimalParser : IParsedExpressionNumberParser
    {
        private readonly IFormatProvider _formatter;
        private readonly NumberStyles _styles;

        internal DecimalParser(IFormatProvider formatter, NumberStyles styles)
        {
            _formatter = formatter;
            _styles = styles;
        }

        public bool TryParse(StringSlice text, [MaybeNullWhen( false )] out object result)
        {
            if ( decimal.TryParse( text.AsSpan(), _styles, _formatter, out var parsedResult ) )
            {
                result = parsedResult;
                return true;
            }

            result = null;
            return false;
        }
    }

    private sealed class DoubleParser : IParsedExpressionNumberParser
    {
        private readonly IFormatProvider _formatter;
        private readonly NumberStyles _styles;

        internal DoubleParser(IFormatProvider formatter, NumberStyles styles)
        {
            _formatter = formatter;
            _styles = styles;
        }

        public bool TryParse(StringSlice text, [MaybeNullWhen( false )] out object result)
        {
            if ( double.TryParse( text.AsSpan(), _styles, _formatter, out var parsedResult ) )
            {
                result = parsedResult;
                return true;
            }

            result = null;
            return false;
        }
    }

    private sealed class FloatParser : IParsedExpressionNumberParser
    {
        private readonly IFormatProvider _formatter;
        private readonly NumberStyles _styles;

        internal FloatParser(IFormatProvider formatter, NumberStyles styles)
        {
            _formatter = formatter;
            _styles = styles;
        }

        public bool TryParse(StringSlice text, [MaybeNullWhen( false )] out object result)
        {
            if ( float.TryParse( text.AsSpan(), _styles, _formatter, out var parsedResult ) )
            {
                result = parsedResult;
                return true;
            }

            result = null;
            return false;
        }
    }

    private sealed class BigIntegerParser : IParsedExpressionNumberParser
    {
        private readonly IFormatProvider _formatter;
        private readonly NumberStyles _styles;

        internal BigIntegerParser(IFormatProvider formatter, NumberStyles styles)
        {
            _formatter = formatter;
            _styles = styles;
        }

        public bool TryParse(StringSlice text, [MaybeNullWhen( false )] out object result)
        {
            if ( BigInteger.TryParse( text.AsSpan(), _styles, _formatter, out var parsedResult ) )
            {
                result = parsedResult;
                return true;
            }

            result = null;
            return false;
        }
    }

    private sealed class Int64Parser : IParsedExpressionNumberParser
    {
        private readonly IFormatProvider _formatter;
        private readonly NumberStyles _styles;

        internal Int64Parser(IFormatProvider formatter, NumberStyles styles)
        {
            _formatter = formatter;
            _styles = styles;
        }

        public bool TryParse(StringSlice text, [MaybeNullWhen( false )] out object result)
        {
            if ( long.TryParse( text.AsSpan(), _styles, _formatter, out var parsedResult ) )
            {
                result = parsedResult;
                return true;
            }

            result = null;
            return false;
        }
    }

    private sealed class Int32Parser : IParsedExpressionNumberParser
    {
        private readonly IFormatProvider _formatter;
        private readonly NumberStyles _styles;

        internal Int32Parser(IFormatProvider formatter, NumberStyles styles)
        {
            _formatter = formatter;
            _styles = styles;
        }

        public bool TryParse(StringSlice text, [MaybeNullWhen( false )] out object result)
        {
            if ( int.TryParse( text.AsSpan(), _styles, _formatter, out var parsedResult ) )
            {
                result = parsedResult;
                return true;
            }

            result = null;
            return false;
        }
    }
}
