using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Functions;

namespace LfrlAnvil.Sql.Expressions;

public static partial class SqlNode
{
    public static class Functions
    {
        private static SqlRecordsAffectedFunctionExpressionNode? _recordsAffected;
        private static SqlCurrentDateFunctionExpressionNode? _currentDate;
        private static SqlCurrentTimeFunctionExpressionNode? _currentTime;
        private static SqlCurrentDateTimeFunctionExpressionNode? _currentDateTime;
        private static SqlCurrentTimestampFunctionExpressionNode? _currentTimestamp;
        private static SqlNewGuidFunctionExpressionNode? _newGuid;

        [Pure]
        public static SqlRecordsAffectedFunctionExpressionNode RecordsAffected()
        {
            return _recordsAffected ??= new SqlRecordsAffectedFunctionExpressionNode();
        }

        [Pure]
        public static SqlCoalesceFunctionExpressionNode Coalesce(params SqlExpressionNode[] arguments)
        {
            return new SqlCoalesceFunctionExpressionNode( arguments );
        }

        [Pure]
        public static SqlCurrentDateFunctionExpressionNode CurrentDate()
        {
            return _currentDate ??= new SqlCurrentDateFunctionExpressionNode();
        }

        [Pure]
        public static SqlCurrentTimeFunctionExpressionNode CurrentTime()
        {
            return _currentTime ??= new SqlCurrentTimeFunctionExpressionNode();
        }

        [Pure]
        public static SqlCurrentDateTimeFunctionExpressionNode CurrentDateTime()
        {
            return _currentDateTime ??= new SqlCurrentDateTimeFunctionExpressionNode();
        }

        [Pure]
        public static SqlCurrentTimestampFunctionExpressionNode CurrentTimestamp()
        {
            return _currentTimestamp ??= new SqlCurrentTimestampFunctionExpressionNode();
        }

        [Pure]
        public static SqlNewGuidFunctionExpressionNode NewGuid()
        {
            return _newGuid ??= new SqlNewGuidFunctionExpressionNode();
        }

        [Pure]
        public static SqlLengthFunctionExpressionNode Length(SqlExpressionNode argument)
        {
            return new SqlLengthFunctionExpressionNode( argument );
        }

        [Pure]
        public static SqlToLowerFunctionExpressionNode ToLower(SqlExpressionNode argument)
        {
            return new SqlToLowerFunctionExpressionNode( argument );
        }

        [Pure]
        public static SqlToUpperFunctionExpressionNode ToUpper(SqlExpressionNode argument)
        {
            return new SqlToUpperFunctionExpressionNode( argument );
        }

        [Pure]
        public static SqlTrimStartFunctionExpressionNode TrimStart(SqlExpressionNode argument, SqlExpressionNode? characters = null)
        {
            return new SqlTrimStartFunctionExpressionNode( argument, characters );
        }

        [Pure]
        public static SqlTrimEndFunctionExpressionNode TrimEnd(SqlExpressionNode argument, SqlExpressionNode? characters = null)
        {
            return new SqlTrimEndFunctionExpressionNode( argument, characters );
        }

        [Pure]
        public static SqlTrimFunctionExpressionNode Trim(SqlExpressionNode argument, SqlExpressionNode? characters = null)
        {
            return new SqlTrimFunctionExpressionNode( argument, characters );
        }

        [Pure]
        public static SqlSubstringFunctionExpressionNode Substring(
            SqlExpressionNode argument,
            SqlExpressionNode startIndex,
            SqlExpressionNode? length = null)
        {
            return new SqlSubstringFunctionExpressionNode( argument, startIndex, length );
        }

        [Pure]
        public static SqlReplaceFunctionExpressionNode Replace(
            SqlExpressionNode argument,
            SqlExpressionNode oldValue,
            SqlExpressionNode newValue)
        {
            return new SqlReplaceFunctionExpressionNode( argument, oldValue, newValue );
        }

        [Pure]
        public static SqlIndexOfFunctionExpressionNode IndexOf(SqlExpressionNode argument, SqlExpressionNode value)
        {
            return new SqlIndexOfFunctionExpressionNode( argument, value );
        }

        [Pure]
        public static SqlLastIndexOfFunctionExpressionNode LastIndexOf(SqlExpressionNode argument, SqlExpressionNode value)
        {
            return new SqlLastIndexOfFunctionExpressionNode( argument, value );
        }

        [Pure]
        public static SqlSignFunctionExpressionNode Sign(SqlExpressionNode argument)
        {
            return new SqlSignFunctionExpressionNode( argument );
        }

        [Pure]
        public static SqlAbsFunctionExpressionNode Abs(SqlExpressionNode argument)
        {
            return new SqlAbsFunctionExpressionNode( argument );
        }

        [Pure]
        public static SqlFloorFunctionExpressionNode Floor(SqlExpressionNode argument)
        {
            return new SqlFloorFunctionExpressionNode( argument );
        }

        [Pure]
        public static SqlCeilingFunctionExpressionNode Ceiling(SqlExpressionNode argument)
        {
            return new SqlCeilingFunctionExpressionNode( argument );
        }

        [Pure]
        public static SqlTruncateFunctionExpressionNode Truncate(SqlExpressionNode argument)
        {
            return new SqlTruncateFunctionExpressionNode( argument );
        }

        [Pure]
        public static SqlPowerFunctionExpressionNode Power(SqlExpressionNode argument, SqlExpressionNode power)
        {
            return new SqlPowerFunctionExpressionNode( argument, power );
        }

        [Pure]
        public static SqlSquareRootFunctionExpressionNode SquareRoot(SqlExpressionNode argument)
        {
            return new SqlSquareRootFunctionExpressionNode( argument );
        }

        [Pure]
        public static SqlMinFunctionExpressionNode Min(params SqlExpressionNode[] arguments)
        {
            return new SqlMinFunctionExpressionNode( arguments );
        }

        [Pure]
        public static SqlMaxFunctionExpressionNode Max(params SqlExpressionNode[] arguments)
        {
            return new SqlMaxFunctionExpressionNode( arguments );
        }
    }
}
