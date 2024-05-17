using System.Runtime.CompilerServices;
using LfrlAnvil.PostgreSql.Exceptions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlColumnBuilder : SqlColumnBuilder
{
    internal PostgreSqlColumnBuilder(PostgreSqlTableBuilder table, string name, SqlColumnTypeDefinition typeDefinition)
        : base( table, name, typeDefinition ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new PostgreSqlDatabaseBuilder Database => ReinterpretCast.To<PostgreSqlDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlColumnBuilder.Table" />
    public new PostgreSqlTableBuilder Table => ReinterpretCast.To<PostgreSqlTableBuilder>( base.Table );

    /// <inheritdoc cref="SqlColumnBuilder.SetName(string)" />
    public new PostgreSqlColumnBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc cref="SqlColumnBuilder.SetType(SqlColumnTypeDefinition)" />
    public new PostgreSqlColumnBuilder SetType(SqlColumnTypeDefinition definition)
    {
        base.SetType( definition );
        return this;
    }

    /// <inheritdoc cref="SqlColumnBuilder.MarkAsNullable(bool)" />
    public new PostgreSqlColumnBuilder MarkAsNullable(bool enabled = true)
    {
        base.MarkAsNullable( enabled );
        return this;
    }

    /// <inheritdoc cref="SqlColumnBuilder.SetDefaultValue(SqlExpressionNode)" />
    public new PostgreSqlColumnBuilder SetDefaultValue(SqlExpressionNode? value)
    {
        base.SetDefaultValue( value );
        return this;
    }

    /// <inheritdoc cref="SqlColumnBuilder.SetComputation(SqlColumnComputation?)" />
    public new PostgreSqlColumnBuilder SetComputation(SqlColumnComputation? computation)
    {
        base.SetComputation( computation );
        return this;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void UpdateDefaultValueBasedOnDataType()
    {
        SetDefaultValueBasedOnDataType();
    }

    /// <inheritdoc />
    protected override SqlPropertyChange<SqlColumnComputation?> BeforeComputationChange(SqlColumnComputation? newValue)
    {
        if ( newValue is null
            || newValue.Value.Storage == SqlColumnComputationStorage.Stored
            || Database.VirtualGeneratedColumnStorageResolution == SqlOptionalFunctionalityResolution.Include )
            return base.BeforeComputationChange( newValue );

        if ( Database.VirtualGeneratedColumnStorageResolution == SqlOptionalFunctionalityResolution.Ignore )
            return base.BeforeComputationChange( SqlColumnComputation.Stored( newValue.Value.Expression ) );

        throw SqlHelpers.CreateObjectBuilderException(
            Database,
            Resources.GeneratedColumnsWithVirtualStorageAreForbidden( this, newValue.Value ) );
    }
}
