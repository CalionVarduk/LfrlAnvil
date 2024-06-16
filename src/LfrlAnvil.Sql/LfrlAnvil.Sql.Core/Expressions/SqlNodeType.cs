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

using LfrlAnvil.Sql.Expressions.Arithmetic;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents a type of an <see cref="SqlNodeBase"/> instance.
/// </summary>
public enum SqlNodeType : byte
{
    /// <summary>
    /// Specifies a node of unknown type.
    ///</summary>
    Unknown,

    /// <summary>
    /// Specifies an <see cref="SqlRawExpressionNode" />.
    ///</summary>
    RawExpression,

    /// <summary>
    /// Specifies an <see cref="SqlRawDataFieldNode" />.
    ///</summary>
    RawDataField,

    /// <summary>
    /// Specifies an <see cref="SqlNullNode" />.
    ///</summary>
    Null,

    /// <summary>
    /// Specifies an <see cref="SqlLiteralNode" />.
    ///</summary>
    Literal,

    /// <summary>
    /// Specifies an <see cref="SqlParameterNode" />.
    ///</summary>
    Parameter,

    /// <summary>
    /// Specifies an <see cref="SqlColumnNode" />.
    ///</summary>
    Column,

    /// <summary>
    /// Specifies an <see cref="SqlColumnBuilderNode" />.
    ///</summary>
    ColumnBuilder,

    /// <summary>
    /// Specifies an <see cref="SqlQueryDataFieldNode" />.
    ///</summary>
    QueryDataField,

    /// <summary>
    /// Specifies an <see cref="SqlViewDataFieldNode" />.
    ///</summary>
    ViewDataField,

    /// <summary>
    /// Specifies an <see cref="SqlNegateExpressionNode" />.
    ///</summary>
    Negate,

    /// <summary>
    /// Specifies an <see cref="SqlAddExpressionNode" />.
    ///</summary>
    Add,

    /// <summary>
    /// Specifies an <see cref="SqlConcatExpressionNode" />.
    ///</summary>
    Concat,

    /// <summary>
    /// Specifies an <see cref="SqlSubtractExpressionNode" />.
    ///</summary>
    Subtract,

    /// <summary>
    /// Specifies an <see cref="SqlMultiplyExpressionNode" />.
    ///</summary>
    Multiply,

    /// <summary>
    /// Specifies an <see cref="SqlDivideExpressionNode" />.
    ///</summary>
    Divide,

    /// <summary>
    /// Specifies an <see cref="SqlModuloExpressionNode" />.
    ///</summary>
    Modulo,

    /// <summary>
    /// Specifies an <see cref="SqlBitwiseNotExpressionNode" />.
    ///</summary>
    BitwiseNot,

    /// <summary>
    /// Specifies an <see cref="SqlBitwiseAndExpressionNode" />.
    ///</summary>
    BitwiseAnd,

    /// <summary>
    /// Specifies an <see cref="SqlBitwiseOrExpressionNode" />.
    ///</summary>
    BitwiseOr,

    /// <summary>
    /// Specifies an <see cref="SqlBitwiseXorExpressionNode" />.
    ///</summary>
    BitwiseXor,

    /// <summary>
    /// Specifies an <see cref="SqlBitwiseLeftShiftExpressionNode" />.
    ///</summary>
    BitwiseLeftShift,

    /// <summary>
    /// Specifies an <see cref="SqlBitwiseRightShiftExpressionNode" />.
    ///</summary>
    BitwiseRightShift,

    /// <summary>
    /// Specifies an <see cref="SqlSwitchCaseNode" />.
    ///</summary>
    SwitchCase,

    /// <summary>
    /// Specifies an <see cref="SqlSwitchExpressionNode" />.
    ///</summary>
    Switch,

    /// <summary>
    /// Specifies an <see cref="SqlFunctionExpressionNode" />.
    ///</summary>
    FunctionExpression,

    /// <summary>
    /// Specifies an <see cref="SqlAggregateFunctionExpressionNode" />.
    ///</summary>
    AggregateFunctionExpression,

    /// <summary>
    /// Specifies an <see cref="SqlRawConditionNode" />.
    ///</summary>
    RawCondition,

    /// <summary>
    /// Specifies an <see cref="SqlTrueNode" />.
    ///</summary>
    True,

    /// <summary>
    /// Specifies an <see cref="SqlFalseNode" />.
    ///</summary>
    False,

    /// <summary>
    /// Specifies an <see cref="SqlEqualToConditionNode" />.
    ///</summary>
    EqualTo,

    /// <summary>
    /// Specifies an <see cref="SqlNotEqualToConditionNode" />.
    ///</summary>
    NotEqualTo,

    /// <summary>
    /// Specifies an <see cref="SqlGreaterThanConditionNode" />.
    ///</summary>
    GreaterThan,

    /// <summary>
    /// Specifies an <see cref="SqlLessThanConditionNode" />.
    ///</summary>
    LessThan,

    /// <summary>
    /// Specifies an <see cref="SqlGreaterThanOrEqualToConditionNode" />.
    ///</summary>
    GreaterThanOrEqualTo,

    /// <summary>
    /// Specifies an <see cref="SqlLessThanOrEqualToConditionNode" />.
    ///</summary>
    LessThanOrEqualTo,

    /// <summary>
    /// Specifies an <see cref="SqlAndConditionNode" />.
    ///</summary>
    And,

    /// <summary>
    /// Specifies an <see cref="SqlOrConditionNode" />.
    ///</summary>
    Or,

    /// <summary>
    /// Specifies an <see cref="SqlConditionValueNode" />.
    ///</summary>
    ConditionValue,

    /// <summary>
    /// Specifies an <see cref="SqlBetweenConditionNode" />.
    ///</summary>
    Between,

    /// <summary>
    /// Specifies an <see cref="SqlExistsConditionNode" />.
    ///</summary>
    Exists,

    /// <summary>
    /// Specifies an <see cref="SqlLikeConditionNode" />.
    ///</summary>
    Like,

    /// <summary>
    /// Specifies an <see cref="SqlInConditionNode" />.
    ///</summary>
    In,

    /// <summary>
    /// Specifies an <see cref="SqlInQueryConditionNode" />.
    ///</summary>
    InQuery,

    /// <summary>
    /// Specifies an <see cref="SqlRawRecordSetNode" />.
    ///</summary>
    RawRecordSet,

    /// <summary>
    /// Specifies an <see cref="SqlNamedFunctionRecordSetNode" />.
    ///</summary>
    NamedFunctionRecordSet,

    /// <summary>
    /// Specifies an <see cref="SqlTableNode" />.
    ///</summary>
    Table,

    /// <summary>
    /// Specifies an <see cref="SqlTableBuilderNode" />.
    ///</summary>
    TableBuilder,

    /// <summary>
    /// Specifies an <see cref="SqlViewNode" />.
    ///</summary>
    View,

    /// <summary>
    /// Specifies an <see cref="SqlViewBuilderNode" />.
    ///</summary>
    ViewBuilder,

    /// <summary>
    /// Specifies an <see cref="SqlQueryRecordSetNode" />.
    ///</summary>
    QueryRecordSet,

    /// <summary>
    /// Specifies an <see cref="SqlCommonTableExpressionRecordSetNode" />.
    ///</summary>
    CommonTableExpressionRecordSet,

    /// <summary>
    /// Specifies an <see cref="SqlNewTableNode" />.
    ///</summary>
    NewTable,

    /// <summary>
    /// Specifies an <see cref="SqlNewViewNode" />.
    ///</summary>
    NewView,

    /// <summary>
    /// Specifies an <see cref="SqlDataSourceJoinOnNode" />.
    ///</summary>
    JoinOn,

    /// <summary>
    /// Specifies an <see cref="SqlDataSourceNode" />.
    ///</summary>
    DataSource,

    /// <summary>
    /// Specifies an <see cref="SqlSelectFieldNode" />.
    ///</summary>
    SelectField,

    /// <summary>
    /// Specifies an <see cref="SqlSelectCompoundFieldNode" />.
    ///</summary>
    SelectCompoundField,

    /// <summary>
    /// Specifies an <see cref="SqlSelectRecordSetNode" />.
    ///</summary>
    SelectRecordSet,

    /// <summary>
    /// Specifies an <see cref="SqlSelectAllNode" />.
    ///</summary>
    SelectAll,

    /// <summary>
    /// Specifies an <see cref="SqlSelectExpressionNode" />.
    ///</summary>
    SelectExpression,

    /// <summary>
    /// Specifies an <see cref="SqlRawQueryExpressionNode" />.
    ///</summary>
    RawQuery,

    /// <summary>
    /// Specifies an <see cref="SqlDataSourceQueryExpressionNode" />.
    ///</summary>
    DataSourceQuery,

    /// <summary>
    /// Specifies an <see cref="SqlCompoundQueryExpressionNode" />.
    ///</summary>
    CompoundQuery,

    /// <summary>
    /// Specifies an <see cref="SqlCompoundQueryComponentNode" />.
    ///</summary>
    CompoundQueryComponent,

    /// <summary>
    /// Specifies an <see cref="SqlDistinctTraitNode" />.
    ///</summary>
    DistinctTrait,

    /// <summary>
    /// Specifies an <see cref="SqlFilterTraitNode" />.
    ///</summary>
    FilterTrait,

    /// <summary>
    /// Specifies an <see cref="SqlAggregationTraitNode" />.
    ///</summary>
    AggregationTrait,

    /// <summary>
    /// Specifies an <see cref="SqlAggregationFilterTraitNode" />.
    ///</summary>
    AggregationFilterTrait,

    /// <summary>
    /// Specifies an <see cref="SqlSortTraitNode" />.
    ///</summary>
    SortTrait,

    /// <summary>
    /// Specifies an <see cref="SqlLimitTraitNode" />.
    ///</summary>
    LimitTrait,

    /// <summary>
    /// Specifies an <see cref="SqlOffsetTraitNode" />.
    ///</summary>
    OffsetTrait,

    /// <summary>
    /// Specifies an <see cref="SqlCommonTableExpressionTraitNode" />.
    ///</summary>
    CommonTableExpressionTrait,

    /// <summary>
    /// Specifies an <see cref="SqlWindowDefinitionTraitNode" />.
    ///</summary>
    WindowDefinitionTrait,

    /// <summary>
    /// Specifies an <see cref="SqlWindowTraitNode" />.
    ///</summary>
    WindowTrait,

    /// <summary>
    /// Specifies an <see cref="SqlOrderByNode" />.
    ///</summary>
    OrderBy,

    /// <summary>
    /// Specifies an <see cref="SqlCommonTableExpressionNode" />.
    ///</summary>
    CommonTableExpression,

    /// <summary>
    /// Specifies an <see cref="SqlWindowDefinitionNode" />.
    ///</summary>
    WindowDefinition,

    /// <summary>
    /// Specifies an <see cref="SqlWindowFrameNode" />.
    ///</summary>
    WindowFrame,

    /// <summary>
    /// Specifies an <see cref="SqlTypeCastExpressionNode" />.
    ///</summary>
    TypeCast,

    /// <summary>
    /// Specifies an <see cref="SqlValuesNode" />.
    ///</summary>
    Values,

    /// <summary>
    /// Specifies an <see cref="SqlRawStatementNode" />.
    ///</summary>
    RawStatement,

    /// <summary>
    /// Specifies an <see cref="SqlInsertIntoNode" />.
    ///</summary>
    InsertInto,

    /// <summary>
    /// Specifies an <see cref="SqlUpdateNode" />.
    ///</summary>
    Update,

    /// <summary>
    /// Specifies an <see cref="SqlUpsertNode" />.
    ///</summary>
    Upsert,

    /// <summary>
    /// Specifies an <see cref="SqlValueAssignmentNode" />.
    ///</summary>
    ValueAssignment,

    /// <summary>
    /// Specifies an <see cref="SqlDeleteFromNode" />.
    ///</summary>
    DeleteFrom,

    /// <summary>
    /// Specifies an <see cref="SqlTruncateNode" />.
    ///</summary>
    Truncate,

    /// <summary>
    /// Specifies an <see cref="SqlColumnDefinitionNode" />.
    ///</summary>
    ColumnDefinition,

    /// <summary>
    /// Specifies an <see cref="SqlPrimaryKeyDefinitionNode" />.
    ///</summary>
    PrimaryKeyDefinition,

    /// <summary>
    /// Specifies an <see cref="SqlForeignKeyDefinitionNode" />.
    ///</summary>
    ForeignKeyDefinition,

    /// <summary>
    /// Specifies an <see cref="SqlCheckDefinitionNode" />.
    ///</summary>
    CheckDefinition,

    /// <summary>
    /// Specifies an <see cref="SqlCreateTableNode" />.
    ///</summary>
    CreateTable,

    /// <summary>
    /// Specifies an <see cref="SqlCreateViewNode" />.
    ///</summary>
    CreateView,

    /// <summary>
    /// Specifies an <see cref="SqlCreateIndexNode" />.
    ///</summary>
    CreateIndex,

    /// <summary>
    /// Specifies an <see cref="SqlRenameTableNode" />.
    ///</summary>
    RenameTable,

    /// <summary>
    /// Specifies an <see cref="SqlRenameColumnNode" />.
    ///</summary>
    RenameColumn,

    /// <summary>
    /// Specifies an <see cref="SqlAddColumnNode" />.
    ///</summary>
    AddColumn,

    /// <summary>
    /// Specifies an <see cref="SqlDropColumnNode" />.
    ///</summary>
    DropColumn,

    /// <summary>
    /// Specifies an <see cref="SqlDropTableNode" />.
    ///</summary>
    DropTable,

    /// <summary>
    /// Specifies an <see cref="SqlDropViewNode" />.
    ///</summary>
    DropView,

    /// <summary>
    /// Specifies an <see cref="SqlDropIndexNode" />.
    ///</summary>
    DropIndex,

    /// <summary>
    /// Specifies an <see cref="SqlStatementBatchNode" />.
    ///</summary>
    StatementBatch,

    /// <summary>
    /// Specifies an <see cref="SqlBeginTransactionNode" />.
    ///</summary>
    BeginTransaction,

    /// <summary>
    /// Specifies an <see cref="SqlCommitTransactionNode" />.
    ///</summary>
    CommitTransaction,

    /// <summary>
    /// Specifies an <see cref="SqlRollbackTransactionNode" />.
    ///</summary>
    RollbackTransaction
}
