using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteColumnBuilder : SqlColumnBuilder
{
    internal SqliteColumnBuilder(SqliteTableBuilder table, string name, SqlColumnTypeDefinition typeDefinition)
        : base( table, name, typeDefinition ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new SqliteDatabaseBuilder Database => ReinterpretCast.To<SqliteDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlColumnBuilder.Table" />
    public new SqliteTableBuilder Table => ReinterpretCast.To<SqliteTableBuilder>( base.Table );

    /// <summary>
    /// Returns a string representation of this <see cref="SqliteColumnBuilder"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( Table.Schema.Name, Table.Name, Name )}";
    }

    /// <inheritdoc cref="SqlColumnBuilder.SetName(string)" />
    public new SqliteColumnBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc cref="SqlColumnBuilder.SetType(SqlColumnTypeDefinition)" />
    public new SqliteColumnBuilder SetType(SqlColumnTypeDefinition definition)
    {
        base.SetType( definition );
        return this;
    }

    /// <inheritdoc cref="SqlColumnBuilder.MarkAsNullable(bool)" />
    public new SqliteColumnBuilder MarkAsNullable(bool enabled = true)
    {
        base.MarkAsNullable( enabled );
        return this;
    }

    /// <inheritdoc cref="SqlColumnBuilder.SetDefaultValue(SqlExpressionNode)" />
    public new SqliteColumnBuilder SetDefaultValue(SqlExpressionNode? value)
    {
        base.SetDefaultValue( value );
        return this;
    }

    /// <inheritdoc cref="SqlColumnBuilder.SetComputation(SqlColumnComputation?)" />
    public new SqliteColumnBuilder SetComputation(SqlColumnComputation? computation)
    {
        base.SetComputation( computation );
        return this;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void UpdateDefaultValueBasedOnDataType()
    {
        SetDefaultValueBasedOnDataType();
    }
}
