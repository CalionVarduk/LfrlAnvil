using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Numerics;

namespace LfrlAnvil.Mathematical.Expressions.Internal;

public static class MathExpressionNumberParser
{
    [Pure]
    public static IMathExpressionNumberParser CreateDefaultDecimal(MathExpressionFactoryInternalConfiguration configuration)
    {
        return new DecimalParser( configuration.NumberFormatProvider, configuration.GetNumberStyles() );
    }

    [Pure]
    public static IMathExpressionNumberParser CreateDefaultDouble(MathExpressionFactoryInternalConfiguration configuration)
    {
        return new DoubleParser( configuration.NumberFormatProvider, configuration.GetNumberStyles() );
    }

    [Pure]
    public static IMathExpressionNumberParser CreateDefaultFloat(MathExpressionFactoryInternalConfiguration configuration)
    {
        return new FloatParser( configuration.NumberFormatProvider, configuration.GetNumberStyles() );
    }

    [Pure]
    public static IMathExpressionNumberParser CreateDefaultBigInteger(MathExpressionFactoryInternalConfiguration configuration)
    {
        return new BigIntegerParser( configuration.NumberFormatProvider, configuration.GetNumberStyles() );
    }

    [Pure]
    public static IMathExpressionNumberParser CreateDefaultInt64(MathExpressionFactoryInternalConfiguration configuration)
    {
        return new Int64Parser( configuration.NumberFormatProvider, configuration.GetNumberStyles() );
    }

    [Pure]
    public static IMathExpressionNumberParser CreateDefaultInt32(MathExpressionFactoryInternalConfiguration configuration)
    {
        return new Int32Parser( configuration.NumberFormatProvider, configuration.GetNumberStyles() );
    }

    private sealed class DecimalParser : IMathExpressionNumberParser
    {
        private readonly IFormatProvider _formatter;
        private readonly NumberStyles _styles;

        internal DecimalParser(IFormatProvider formatter, NumberStyles styles)
        {
            _formatter = formatter;
            _styles = styles;
        }

        public bool TryParse(ReadOnlySpan<char> text, [MaybeNullWhen( false )] out object result)
        {
            if ( decimal.TryParse( text, _styles, _formatter, out var parsedResult ) )
            {
                result = parsedResult;
                return true;
            }

            result = null;
            return false;
        }
    }

    private sealed class DoubleParser : IMathExpressionNumberParser
    {
        private readonly IFormatProvider _formatter;
        private readonly NumberStyles _styles;

        internal DoubleParser(IFormatProvider formatter, NumberStyles styles)
        {
            _formatter = formatter;
            _styles = styles;
        }

        public bool TryParse(ReadOnlySpan<char> text, [MaybeNullWhen( false )] out object result)
        {
            if ( double.TryParse( text, _styles, _formatter, out var parsedResult ) )
            {
                result = parsedResult;
                return true;
            }

            result = null;
            return false;
        }
    }

    private sealed class FloatParser : IMathExpressionNumberParser
    {
        private readonly IFormatProvider _formatter;
        private readonly NumberStyles _styles;

        internal FloatParser(IFormatProvider formatter, NumberStyles styles)
        {
            _formatter = formatter;
            _styles = styles;
        }

        public bool TryParse(ReadOnlySpan<char> text, [MaybeNullWhen( false )] out object result)
        {
            if ( float.TryParse( text, _styles, _formatter, out var parsedResult ) )
            {
                result = parsedResult;
                return true;
            }

            result = null;
            return false;
        }
    }

    private sealed class BigIntegerParser : IMathExpressionNumberParser
    {
        private readonly IFormatProvider _formatter;
        private readonly NumberStyles _styles;

        internal BigIntegerParser(IFormatProvider formatter, NumberStyles styles)
        {
            _formatter = formatter;
            _styles = styles;
        }

        public bool TryParse(ReadOnlySpan<char> text, [MaybeNullWhen( false )] out object result)
        {
            if ( BigInteger.TryParse( text, _styles, _formatter, out var parsedResult ) )
            {
                result = parsedResult;
                return true;
            }

            result = null;
            return false;
        }
    }

    private sealed class Int64Parser : IMathExpressionNumberParser
    {
        private readonly IFormatProvider _formatter;
        private readonly NumberStyles _styles;

        internal Int64Parser(IFormatProvider formatter, NumberStyles styles)
        {
            _formatter = formatter;
            _styles = styles;
        }

        public bool TryParse(ReadOnlySpan<char> text, [MaybeNullWhen( false )] out object result)
        {
            if ( long.TryParse( text, _styles, _formatter, out var parsedResult ) )
            {
                result = parsedResult;
                return true;
            }

            result = null;
            return false;
        }
    }

    private sealed class Int32Parser : IMathExpressionNumberParser
    {
        private readonly IFormatProvider _formatter;
        private readonly NumberStyles _styles;

        internal Int32Parser(IFormatProvider formatter, NumberStyles styles)
        {
            _formatter = formatter;
            _styles = styles;
        }

        public bool TryParse(ReadOnlySpan<char> text, [MaybeNullWhen( false )] out object result)
        {
            if ( int.TryParse( text, _styles, _formatter, out var parsedResult ) )
            {
                result = parsedResult;
                return true;
            }

            result = null;
            return false;
        }
    }
}
