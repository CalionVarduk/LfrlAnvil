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

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents a type of an <see cref="SqlFunctionExpressionNode"/> or <see cref="SqlAggregateFunctionExpressionNode"/> instance.
/// </summary>
public enum SqlFunctionType : byte
{
    /// <summary>
    /// Specifies a custom function node.
    ///</summary>
    Custom,

    /// <summary>
    /// Specifies an <see cref="SqlNamedFunctionExpressionNode" />.
    ///</summary>
    Named,

    /// <summary>
    /// Specifies an <see cref="SqlCoalesceFunctionExpressionNode" />.
    ///</summary>
    Coalesce,

    /// <summary>
    /// Specifies an <see cref="SqlCurrentDateFunctionExpressionNode" />.
    ///</summary>
    CurrentDate,

    /// <summary>
    /// Specifies an <see cref="SqlCurrentTimeFunctionExpressionNode" />.
    ///</summary>
    CurrentTime,

    /// <summary>
    /// Specifies an <see cref="SqlCurrentDateTimeFunctionExpressionNode" />.
    ///</summary>
    CurrentDateTime,

    /// <summary>
    /// Specifies an <see cref="SqlCurrentUtcDateTimeFunctionExpressionNode" />.
    ///</summary>
    CurrentUtcDateTime,

    /// <summary>
    /// Specifies an <see cref="SqlCurrentTimestampFunctionExpressionNode" />.
    ///</summary>
    CurrentTimestamp,

    /// <summary>
    /// Specifies an <see cref="SqlExtractDateFunctionExpressionNode" />.
    ///</summary>
    ExtractDate,

    /// <summary>
    /// Specifies an <see cref="SqlExtractTimeOfDayFunctionExpressionNode" />.
    ///</summary>
    ExtractTimeOfDay,

    /// <summary>
    /// Specifies an <see cref="SqlExtractDayFunctionExpressionNode" />.
    ///</summary>
    ExtractDay,

    /// <summary>
    /// Specifies an <see cref="SqlExtractTemporalUnitFunctionExpressionNode" />.
    ///</summary>
    ExtractTemporalUnit,

    /// <summary>
    /// Specifies an <see cref="SqlTemporalAddFunctionExpressionNode" />.
    ///</summary>
    TemporalAdd,

    /// <summary>
    /// Specifies an <see cref="SqlTemporalDiffFunctionExpressionNode" />.
    ///</summary>
    TemporalDiff,

    /// <summary>
    /// Specifies an <see cref="SqlNewGuidFunctionExpressionNode" />.
    ///</summary>
    NewGuid,

    /// <summary>
    /// Specifies an <see cref="SqlLengthFunctionExpressionNode" />.
    ///</summary>
    Length,

    /// <summary>
    /// Specifies an <see cref="SqlByteLengthFunctionExpressionNode" />.
    ///</summary>
    ByteLength,

    /// <summary>
    /// Specifies an <see cref="SqlToLowerFunctionExpressionNode" />.
    ///</summary>
    ToLower,

    /// <summary>
    /// Specifies an <see cref="SqlToUpperFunctionExpressionNode" />.
    ///</summary>
    ToUpper,

    /// <summary>
    /// Specifies an <see cref="SqlTrimStartFunctionExpressionNode" />.
    ///</summary>
    TrimStart,

    /// <summary>
    /// Specifies an <see cref="SqlTrimEndFunctionExpressionNode" />.
    ///</summary>
    TrimEnd,

    /// <summary>
    /// Specifies an <see cref="SqlTrimFunctionExpressionNode" />.
    ///</summary>
    Trim,

    /// <summary>
    /// Specifies an <see cref="SqlSubstringFunctionExpressionNode" />.
    ///</summary>
    Substring,

    /// <summary>
    /// Specifies an <see cref="SqlReplaceFunctionExpressionNode" />.
    ///</summary>
    Replace,

    /// <summary>
    /// Specifies an <see cref="SqlReverseFunctionExpressionNode" />.
    ///</summary>
    Reverse,

    /// <summary>
    /// Specifies an <see cref="SqlIndexOfFunctionExpressionNode" />.
    ///</summary>
    IndexOf,

    /// <summary>
    /// Specifies an <see cref="SqlLastIndexOfFunctionExpressionNode" />.
    ///</summary>
    LastIndexOf,

    /// <summary>
    /// Specifies an <see cref="SqlSignFunctionExpressionNode" />.
    ///</summary>
    Sign,

    /// <summary>
    /// Specifies an <see cref="SqlAbsFunctionExpressionNode" />.
    ///</summary>
    Abs,

    /// <summary>
    /// Specifies an <see cref="SqlCeilingFunctionExpressionNode" />.
    ///</summary>
    Ceiling,

    /// <summary>
    /// Specifies an <see cref="SqlFloorFunctionExpressionNode" />.
    ///</summary>
    Floor,

    /// <summary>
    /// Specifies an <see cref="SqlTruncateFunctionExpressionNode" />.
    ///</summary>
    Truncate,

    /// <summary>
    /// Specifies an <see cref="SqlRoundFunctionExpressionNode" />.
    ///</summary>
    Round,

    /// <summary>
    /// Specifies an <see cref="SqlPowerFunctionExpressionNode" />.
    ///</summary>
    Power,

    /// <summary>
    /// Specifies an <see cref="SqlSquareRootFunctionExpressionNode" />.
    ///</summary>
    SquareRoot,

    /// <summary>
    /// Specifies an <see cref="SqlMinFunctionExpressionNode" />.
    ///</summary>
    Min,

    /// <summary>
    /// Specifies an <see cref="SqlMaxFunctionExpressionNode" />.
    ///</summary>
    Max,

    /// <summary>
    /// Specifies an <see cref="SqlAverageAggregateFunctionExpressionNode" />.
    ///</summary>
    Average,

    /// <summary>
    /// Specifies an <see cref="SqlSumAggregateFunctionExpressionNode" />.
    ///</summary>
    Sum,

    /// <summary>
    /// Specifies an <see cref="SqlCountAggregateFunctionExpressionNode" />.
    ///</summary>
    Count,

    /// <summary>
    /// Specifies an <see cref="SqlStringConcatAggregateFunctionExpressionNode" />.
    ///</summary>
    StringConcat,

    /// <summary>
    /// Specifies an <see cref="SqlRowNumberWindowFunctionExpressionNode" />.
    ///</summary>
    RowNumber,

    /// <summary>
    /// Specifies an <see cref="SqlRankWindowFunctionExpressionNode" />.
    ///</summary>
    Rank,

    /// <summary>
    /// Specifies an <see cref="SqlDenseRankWindowFunctionExpressionNode" />.
    ///</summary>
    DenseRank,

    /// <summary>
    /// Specifies an <see cref="SqlCumulativeDistributionWindowFunctionExpressionNode" />.
    ///</summary>
    CumulativeDistribution,

    /// <summary>
    /// Specifies an <see cref="SqlNTileWindowFunctionExpressionNode" />.
    ///</summary>
    NTile,

    /// <summary>
    /// Specifies an <see cref="SqlLagWindowFunctionExpressionNode" />.
    ///</summary>
    Lag,

    /// <summary>
    /// Specifies an <see cref="SqlLeadWindowFunctionExpressionNode" />.
    ///</summary>
    Lead,

    /// <summary>
    /// Specifies an <see cref="SqlFirstValueWindowFunctionExpressionNode" />.
    ///</summary>
    FirstValue,

    /// <summary>
    /// Specifies an <see cref="SqlLastValueWindowFunctionExpressionNode" />.
    ///</summary>
    LastValue,

    /// <summary>
    /// Specifies an <see cref="SqlNthValueWindowFunctionExpressionNode" />.
    ///</summary>
    NthValue
}
