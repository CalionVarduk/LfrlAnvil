﻿using System;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Persistence;

public sealed class SqlInsertIntoNode : SqlNodeBase
{
    internal SqlInsertIntoNode(SqlQueryExpressionNode query, SqlRecordSetNode recordSet, SqlDataFieldNode[] dataFields)
        : base( SqlNodeType.InsertInto )
    {
        Source = query;
        RecordSet = recordSet;
        DataFields = dataFields;
    }

    internal SqlInsertIntoNode(SqlValuesNode values, SqlRecordSetNode recordSet, SqlDataFieldNode[] dataFields)
        : base( SqlNodeType.InsertInto )
    {
        Source = values;
        RecordSet = recordSet;
        DataFields = dataFields;
    }

    public SqlNodeBase Source { get; }
    public SqlRecordSetNode RecordSet { get; }
    public ReadOnlyMemory<SqlDataFieldNode> DataFields { get; }
}