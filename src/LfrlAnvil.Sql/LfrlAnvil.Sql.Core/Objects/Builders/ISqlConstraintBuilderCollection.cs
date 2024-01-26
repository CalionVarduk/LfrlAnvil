using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlConstraintBuilderCollection : IReadOnlyCollection<ISqlConstraintBuilder>
{
    ISqlTableBuilder Table { get; }

    [Pure]
    bool Contains(string name);

    [Pure]
    ISqlConstraintBuilder GetConstraint(string name);

    [Pure]
    ISqlConstraintBuilder? TryGetConstraint(string name);

    [Pure]
    ISqlPrimaryKeyBuilder GetPrimaryKey();

    [Pure]
    ISqlPrimaryKeyBuilder? TryGetPrimaryKey();

    [Pure]
    ISqlIndexBuilder GetIndex(string name);

    [Pure]
    ISqlIndexBuilder? TryGetIndex(string name);

    [Pure]
    ISqlForeignKeyBuilder GetForeignKey(string name);

    [Pure]
    ISqlForeignKeyBuilder? TryGetForeignKey(string name);

    [Pure]
    ISqlCheckBuilder GetCheck(string name);

    [Pure]
    ISqlCheckBuilder? TryGetCheck(string name);

    ISqlPrimaryKeyBuilder SetPrimaryKey(ISqlIndexBuilder index);
    ISqlPrimaryKeyBuilder SetPrimaryKey(string name, ISqlIndexBuilder index);
    ISqlIndexBuilder CreateIndex(ReadOnlyMemory<ISqlIndexColumnBuilder> columns, bool isUnique = false);
    ISqlIndexBuilder CreateIndex(string name, ReadOnlyMemory<ISqlIndexColumnBuilder> columns, bool isUnique = false);
    ISqlForeignKeyBuilder CreateForeignKey(ISqlIndexBuilder originIndex, ISqlIndexBuilder referencedIndex);
    ISqlForeignKeyBuilder CreateForeignKey(string name, ISqlIndexBuilder originIndex, ISqlIndexBuilder referencedIndex);
    ISqlCheckBuilder CreateCheck(SqlConditionNode condition);
    ISqlCheckBuilder CreateCheck(string name, SqlConditionNode condition);
    bool Remove(string name);
}
