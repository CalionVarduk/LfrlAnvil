using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Functions;

namespace LfrlAnvil.Sql.Expressions;

public static partial class SqlNode
{
    /// <summary>
    /// Creates instances of <see cref="SqlFunctionExpressionNode"/> type.
    /// </summary>
    public static class Functions
    {
        private static SqlCurrentDateFunctionExpressionNode? _currentDate;
        private static SqlCurrentTimeFunctionExpressionNode? _currentTime;
        private static SqlCurrentDateTimeFunctionExpressionNode? _currentDateTime;
        private static SqlCurrentUtcDateTimeFunctionExpressionNode? _currentUtcDateTime;
        private static SqlCurrentTimestampFunctionExpressionNode? _currentTimestamp;
        private static SqlNewGuidFunctionExpressionNode? _newGuid;

        /// <summary>
        /// Creates a new <see cref="SqlNamedFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="name">Function's name.</param>
        /// <param name="arguments">Collection of function's arguments.</param>
        /// <returns>New <see cref="SqlNamedFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlNamedFunctionExpressionNode Named(SqlSchemaObjectName name, params SqlExpressionNode[] arguments)
        {
            return new SqlNamedFunctionExpressionNode( name, arguments );
        }

        /// <summary>
        /// Creates a new <see cref="SqlCoalesceFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="arguments">Collection of function's arguments.</param>
        /// <returns>New <see cref="SqlCoalesceFunctionExpressionNode"/> instance.</returns>
        /// <exception cref="ArgumentException">When collection of arguments is empty.</exception>
        [Pure]
        public static SqlCoalesceFunctionExpressionNode Coalesce(params SqlExpressionNode[] arguments)
        {
            return new SqlCoalesceFunctionExpressionNode( arguments );
        }

        /// <summary>
        /// Creates a new <see cref="SqlCurrentDateFunctionExpressionNode"/> instance.
        /// </summary>
        /// <returns>New <see cref="SqlCurrentDateFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlCurrentDateFunctionExpressionNode CurrentDate()
        {
            return _currentDate ??= new SqlCurrentDateFunctionExpressionNode();
        }

        /// <summary>
        /// Creates a new <see cref="SqlCurrentTimeFunctionExpressionNode"/> instance.
        /// </summary>
        /// <returns>New <see cref="SqlCurrentTimeFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlCurrentTimeFunctionExpressionNode CurrentTime()
        {
            return _currentTime ??= new SqlCurrentTimeFunctionExpressionNode();
        }

        /// <summary>
        /// Creates a new <see cref="SqlCurrentDateTimeFunctionExpressionNode"/> instance.
        /// </summary>
        /// <returns>New <see cref="SqlCurrentDateTimeFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlCurrentDateTimeFunctionExpressionNode CurrentDateTime()
        {
            return _currentDateTime ??= new SqlCurrentDateTimeFunctionExpressionNode();
        }

        /// <summary>
        /// Creates a new <see cref="SqlCurrentUtcDateTimeFunctionExpressionNode"/> instance.
        /// </summary>
        /// <returns>New <see cref="SqlCurrentUtcDateTimeFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlCurrentUtcDateTimeFunctionExpressionNode CurrentUtcDateTime()
        {
            return _currentUtcDateTime ??= new SqlCurrentUtcDateTimeFunctionExpressionNode();
        }

        /// <summary>
        /// Creates a new <see cref="SqlCurrentTimestampFunctionExpressionNode"/> instance.
        /// </summary>
        /// <returns>New <see cref="SqlCurrentTimestampFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlCurrentTimestampFunctionExpressionNode CurrentTimestamp()
        {
            return _currentTimestamp ??= new SqlCurrentTimestampFunctionExpressionNode();
        }

        /// <summary>
        /// Creates a new <see cref="SqlExtractDateFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="expression">Expression to extract date part from.</param>
        /// <returns>New <see cref="SqlExtractDateFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlExtractDateFunctionExpressionNode ExtractDate(SqlExpressionNode expression)
        {
            return new SqlExtractDateFunctionExpressionNode( expression );
        }

        /// <summary>
        /// Creates a new <see cref="SqlExtractTimeOfDayFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="expression">Expression to extract time of day part from.</param>
        /// <returns>New <see cref="SqlExtractTimeOfDayFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlExtractTimeOfDayFunctionExpressionNode ExtractTimeOfDay(SqlExpressionNode expression)
        {
            return new SqlExtractTimeOfDayFunctionExpressionNode( expression );
        }

        /// <summary>
        /// Creates a new <see cref="SqlExtractDayFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="expression">Expression to extract day of year component from.</param>
        /// <returns>New <see cref="SqlExtractDayFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlExtractDayFunctionExpressionNode ExtractDayOfYear(SqlExpressionNode expression)
        {
            return new SqlExtractDayFunctionExpressionNode( expression, SqlTemporalUnit.Year );
        }

        /// <summary>
        /// Creates a new <see cref="SqlExtractDayFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="expression">Expression to extract day of month component from.</param>
        /// <returns>New <see cref="SqlExtractDayFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlExtractDayFunctionExpressionNode ExtractDayOfMonth(SqlExpressionNode expression)
        {
            return new SqlExtractDayFunctionExpressionNode( expression, SqlTemporalUnit.Month );
        }

        /// <summary>
        /// Creates a new <see cref="SqlExtractDayFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="expression">Expression to extract day of week component from.</param>
        /// <returns>New <see cref="SqlExtractDayFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlExtractDayFunctionExpressionNode ExtractDayOfWeek(SqlExpressionNode expression)
        {
            return new SqlExtractDayFunctionExpressionNode( expression, SqlTemporalUnit.Week );
        }

        /// <summary>
        /// Creates a new <see cref="SqlExtractTemporalUnitFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="expression">Expression to extract the desired date or time component from.</param>
        /// <param name="unit"><see cref="SqlTemporalUnit"/> that specifies the date or time component to extract.</param>
        /// <returns>New <see cref="SqlExtractTemporalUnitFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlExtractTemporalUnitFunctionExpressionNode ExtractTemporalUnit(SqlExpressionNode expression, SqlTemporalUnit unit)
        {
            return new SqlExtractTemporalUnitFunctionExpressionNode( expression, unit );
        }

        /// <summary>
        /// Creates a new <see cref="SqlTemporalAddFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="expression">Expression to add value to.</param>
        /// <param name="value">Value to add.</param>
        /// <param name="unit"><see cref="SqlTemporalUnit"/> that specifies the unit of the added value.</param>
        /// <returns>New <see cref="SqlTemporalAddFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlTemporalAddFunctionExpressionNode TemporalAdd(
            SqlExpressionNode expression,
            SqlExpressionNode value,
            SqlTemporalUnit unit)
        {
            return new SqlTemporalAddFunctionExpressionNode( expression, value, unit );
        }

        /// <summary>
        /// Creates a new <see cref="SqlTemporalDiffFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="start">Expression that defines the start value.</param>
        /// <param name="end">Expression that defines the end value.</param>
        /// <param name="unit"><see cref="SqlTemporalUnit"/> that specifies the unit of the returned result.</param>
        /// <returns>New <see cref="SqlTemporalDiffFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlTemporalDiffFunctionExpressionNode TemporalDiff(
            SqlExpressionNode start,
            SqlExpressionNode end,
            SqlTemporalUnit unit)
        {
            return new SqlTemporalDiffFunctionExpressionNode( start, end, unit );
        }

        /// <summary>
        /// Creates a new <see cref="SqlNewGuidFunctionExpressionNode"/> instance.
        /// </summary>
        /// <returns>New <see cref="SqlNewGuidFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlNewGuidFunctionExpressionNode NewGuid()
        {
            return _newGuid ??= new SqlNewGuidFunctionExpressionNode();
        }

        /// <summary>
        /// Creates a new <see cref="SqlLengthFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="argument">Expression to calculate length from.</param>
        /// <returns>New <see cref="SqlLengthFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlLengthFunctionExpressionNode Length(SqlExpressionNode argument)
        {
            return new SqlLengthFunctionExpressionNode( argument );
        }

        /// <summary>
        /// Creates a new <see cref="SqlByteLengthFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="argument">Expression to calculate byte length from.</param>
        /// <returns>New <see cref="SqlByteLengthFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlByteLengthFunctionExpressionNode ByteLength(SqlExpressionNode argument)
        {
            return new SqlByteLengthFunctionExpressionNode( argument );
        }

        /// <summary>
        /// Creates a new <see cref="SqlToLowerFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="argument">Expression to convert to lowercase.</param>
        /// <returns>New <see cref="SqlToLowerFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlToLowerFunctionExpressionNode ToLower(SqlExpressionNode argument)
        {
            return new SqlToLowerFunctionExpressionNode( argument );
        }

        /// <summary>
        /// Creates a new <see cref="SqlToUpperFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="argument">Expression to convert to uppercase.</param>
        /// <returns>New <see cref="SqlToUpperFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlToUpperFunctionExpressionNode ToUpper(SqlExpressionNode argument)
        {
            return new SqlToUpperFunctionExpressionNode( argument );
        }

        /// <summary>
        /// Creates a new <see cref="SqlTrimStartFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="argument">Expression to trim at the start.</param>
        /// <param name="characters">Optional characters to trim away. Equal to null by default.</param>
        /// <returns>New <see cref="SqlTrimStartFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlTrimStartFunctionExpressionNode TrimStart(SqlExpressionNode argument, SqlExpressionNode? characters = null)
        {
            return new SqlTrimStartFunctionExpressionNode( argument, characters );
        }

        /// <summary>
        /// Creates a new <see cref="SqlTrimEndFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="argument">Expression to trim at the end.</param>
        /// <param name="characters">Optional characters to trim away. Equal to null by default.</param>
        /// <returns>New <see cref="SqlTrimEndFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlTrimEndFunctionExpressionNode TrimEnd(SqlExpressionNode argument, SqlExpressionNode? characters = null)
        {
            return new SqlTrimEndFunctionExpressionNode( argument, characters );
        }

        /// <summary>
        /// Creates a new <see cref="SqlTrimFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="argument">Expression to trim at both ends.</param>
        /// <param name="characters">Optional characters to trim away. Equal to null by default.</param>
        /// <returns>New <see cref="SqlTrimFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlTrimFunctionExpressionNode Trim(SqlExpressionNode argument, SqlExpressionNode? characters = null)
        {
            return new SqlTrimFunctionExpressionNode( argument, characters );
        }

        /// <summary>
        /// Creates a new <see cref="SqlSubstringFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="argument">Expression to extract a substring from.</param>
        /// <param name="startIndex">Position of the first character of the substring.</param>
        /// <param name="length">Optional length of the substring. Equal to null by default.</param>
        /// <returns>New <see cref="SqlSubstringFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlSubstringFunctionExpressionNode Substring(
            SqlExpressionNode argument,
            SqlExpressionNode startIndex,
            SqlExpressionNode? length = null)
        {
            return new SqlSubstringFunctionExpressionNode( argument, startIndex, length );
        }

        /// <summary>
        /// Creates a new <see cref="SqlReplaceFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="argument">Expression to replace occurrences in.</param>
        /// <param name="oldValue">Value to replace.</param>
        /// <param name="newValue">Replacement value.</param>
        /// <returns>New <see cref="SqlReplaceFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlReplaceFunctionExpressionNode Replace(
            SqlExpressionNode argument,
            SqlExpressionNode oldValue,
            SqlExpressionNode newValue)
        {
            return new SqlReplaceFunctionExpressionNode( argument, oldValue, newValue );
        }

        /// <summary>
        /// Creates a new <see cref="SqlReverseFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="argument">Expression to reverse.</param>
        /// <returns>New <see cref="SqlReverseFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlReverseFunctionExpressionNode Reverse(SqlExpressionNode argument)
        {
            return new SqlReverseFunctionExpressionNode( argument );
        }

        /// <summary>
        /// Creates a new <see cref="SqlIndexOfFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="argument">Expression to find the first occurrence in.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>New <see cref="SqlIndexOfFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlIndexOfFunctionExpressionNode IndexOf(SqlExpressionNode argument, SqlExpressionNode value)
        {
            return new SqlIndexOfFunctionExpressionNode( argument, value );
        }

        /// <summary>
        /// Creates a new <see cref="SqlLastIndexOfFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="argument">Expression to find the last occurrence in.</param>
        /// <param name="value">Value to search for.</param>
        /// <returns>New <see cref="SqlLastIndexOfFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlLastIndexOfFunctionExpressionNode LastIndexOf(SqlExpressionNode argument, SqlExpressionNode value)
        {
            return new SqlLastIndexOfFunctionExpressionNode( argument, value );
        }

        /// <summary>
        /// Creates a new <see cref="SqlSignFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="argument">Expression to calculate the sign from.</param>
        /// <returns>New <see cref="SqlSignFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlSignFunctionExpressionNode Sign(SqlExpressionNode argument)
        {
            return new SqlSignFunctionExpressionNode( argument );
        }

        /// <summary>
        /// Creates a new <see cref="SqlAbsFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="argument">Expression to calculate the absolute value from.</param>
        /// <returns>New <see cref="SqlAbsFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlAbsFunctionExpressionNode Abs(SqlExpressionNode argument)
        {
            return new SqlAbsFunctionExpressionNode( argument );
        }

        /// <summary>
        /// Creates a new <see cref="SqlFloorFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="argument">Expression to calculate the floor value from.</param>
        /// <returns>New <see cref="SqlFloorFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlFloorFunctionExpressionNode Floor(SqlExpressionNode argument)
        {
            return new SqlFloorFunctionExpressionNode( argument );
        }

        /// <summary>
        /// Creates a new <see cref="SqlCeilingFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="argument">Expression to calculate the ceiling value from.</param>
        /// <returns>New <see cref="SqlCeilingFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlCeilingFunctionExpressionNode Ceiling(SqlExpressionNode argument)
        {
            return new SqlCeilingFunctionExpressionNode( argument );
        }

        /// <summary>
        /// Creates a new <see cref="SqlTruncateFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="argument">Expression to calculate the truncated value from.</param>
        /// <param name="precision">Optional decimal precision of the truncation. Equal to null by default.</param>
        /// <returns>New <see cref="SqlTruncateFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlTruncateFunctionExpressionNode Truncate(SqlExpressionNode argument, SqlExpressionNode? precision = null)
        {
            return new SqlTruncateFunctionExpressionNode( argument, precision );
        }

        /// <summary>
        /// Creates a new <see cref="SqlRoundFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="argument">Expression to calculate the rounded value from.</param>
        /// <param name="precision">Decimal rounding precision.</param>
        /// <returns>New <see cref="SqlRoundFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlRoundFunctionExpressionNode Round(SqlExpressionNode argument, SqlExpressionNode precision)
        {
            return new SqlRoundFunctionExpressionNode( argument, precision );
        }

        /// <summary>
        /// Creates a new <see cref="SqlPowerFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="argument">Expression to raise to the desired power.</param>
        /// <param name="power">Expression that defines the desired power to raise to.</param>
        /// <returns>New <see cref="SqlPowerFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlPowerFunctionExpressionNode Power(SqlExpressionNode argument, SqlExpressionNode power)
        {
            return new SqlPowerFunctionExpressionNode( argument, power );
        }

        /// <summary>
        /// Creates a new <see cref="SqlSquareRootFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="argument">Expression to calculate the square root from.</param>
        /// <returns>New <see cref="SqlSquareRootFunctionExpressionNode"/> instance.</returns>
        [Pure]
        public static SqlSquareRootFunctionExpressionNode SquareRoot(SqlExpressionNode argument)
        {
            return new SqlSquareRootFunctionExpressionNode( argument );
        }

        /// <summary>
        /// Creates a new <see cref="SqlMinFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="arguments">Collection of expressions to calculate the minimum value from.</param>
        /// <returns>New <see cref="SqlMinFunctionExpressionNode"/> instance.</returns>
        /// <exception cref="ArgumentException">When collection of arguments is empty.</exception>
        [Pure]
        public static SqlMinFunctionExpressionNode Min(params SqlExpressionNode[] arguments)
        {
            return new SqlMinFunctionExpressionNode( arguments );
        }

        /// <summary>
        /// Creates a new <see cref="SqlMaxFunctionExpressionNode"/> instance.
        /// </summary>
        /// <param name="arguments">Collection of expressions to calculate the maximum value from.</param>
        /// <returns>New <see cref="SqlMaxFunctionExpressionNode"/> instance.</returns>
        /// <exception cref="ArgumentException">When collection of arguments is empty.</exception>
        [Pure]
        public static SqlMaxFunctionExpressionNode Max(params SqlExpressionNode[] arguments)
        {
            return new SqlMaxFunctionExpressionNode( arguments );
        }
    }
}
