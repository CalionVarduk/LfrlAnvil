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
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Numerics;

namespace LfrlAnvil.Computable.Expressions.Internal;

/// <summary>
/// Creates instances of <see cref="IParsedExpressionNumberParser"/> type.
/// </summary>
public static class ParsedExpressionNumberParser
{
    /// <summary>
    /// Creates a new <see cref="IParsedExpressionNumberParser"/> instance for <see cref="Decimal"/> type.
    /// </summary>
    /// <param name="configuration">Underlying configuration.</param>
    /// <returns>New <see cref="IParsedExpressionNumberParser"/> instance.</returns>
    [Pure]
    public static IParsedExpressionNumberParser CreateDefaultDecimal(ParsedExpressionFactoryInternalConfiguration configuration)
    {
        return new DecimalParser( configuration.NumberFormatProvider, configuration.NumberStyles );
    }

    /// <summary>
    /// Creates a new <see cref="IParsedExpressionNumberParser"/> instance for <see cref="Double"/> type.
    /// </summary>
    /// <param name="configuration">Underlying configuration.</param>
    /// <returns>New <see cref="IParsedExpressionNumberParser"/> instance.</returns>
    [Pure]
    public static IParsedExpressionNumberParser CreateDefaultDouble(ParsedExpressionFactoryInternalConfiguration configuration)
    {
        return new DoubleParser( configuration.NumberFormatProvider, configuration.NumberStyles );
    }

    /// <summary>
    /// Creates a new <see cref="IParsedExpressionNumberParser"/> instance for <see cref="Single"/> type.
    /// </summary>
    /// <param name="configuration">Underlying configuration.</param>
    /// <returns>New <see cref="IParsedExpressionNumberParser"/> instance.</returns>
    [Pure]
    public static IParsedExpressionNumberParser CreateDefaultFloat(ParsedExpressionFactoryInternalConfiguration configuration)
    {
        return new FloatParser( configuration.NumberFormatProvider, configuration.NumberStyles );
    }

    /// <summary>
    /// Creates a new <see cref="IParsedExpressionNumberParser"/> instance for <see cref="BigInteger"/> type.
    /// </summary>
    /// <param name="configuration">Underlying configuration.</param>
    /// <returns>New <see cref="IParsedExpressionNumberParser"/> instance.</returns>
    [Pure]
    public static IParsedExpressionNumberParser CreateDefaultBigInteger(ParsedExpressionFactoryInternalConfiguration configuration)
    {
        return new BigIntegerParser( configuration.NumberFormatProvider, configuration.NumberStyles );
    }

    /// <summary>
    /// Creates a new <see cref="IParsedExpressionNumberParser"/> instance for <see cref="Int64"/> type.
    /// </summary>
    /// <param name="configuration">Underlying configuration.</param>
    /// <returns>New <see cref="IParsedExpressionNumberParser"/> instance.</returns>
    [Pure]
    public static IParsedExpressionNumberParser CreateDefaultInt64(ParsedExpressionFactoryInternalConfiguration configuration)
    {
        return new Int64Parser( configuration.NumberFormatProvider, configuration.NumberStyles );
    }

    /// <summary>
    /// Creates a new <see cref="IParsedExpressionNumberParser"/> instance for <see cref="Int32"/> type.
    /// </summary>
    /// <param name="configuration">Underlying configuration.</param>
    /// <returns>New <see cref="IParsedExpressionNumberParser"/> instance.</returns>
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

        public bool TryParse(StringSegment text, [MaybeNullWhen( false )] out object result)
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

        public bool TryParse(StringSegment text, [MaybeNullWhen( false )] out object result)
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

        public bool TryParse(StringSegment text, [MaybeNullWhen( false )] out object result)
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

        public bool TryParse(StringSegment text, [MaybeNullWhen( false )] out object result)
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

        public bool TryParse(StringSegment text, [MaybeNullWhen( false )] out object result)
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

        public bool TryParse(StringSegment text, [MaybeNullWhen( false )] out object result)
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
