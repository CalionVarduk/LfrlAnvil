using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlTableBuilderMock : SqlTableBuilder
{
    public SqlTableBuilderMock(SqlSchemaBuilderMock schema, string name)
        : base(
            schema,
            name,
            new SqlColumnBuilderCollectionMock( schema.Database.TypeDefinitions.GetByDataType( SqlDataTypeMock.Object ) ),
            new SqlConstraintBuilderCollectionMock() ) { }

    public new SqlDatabaseBuilderMock Database => ReinterpretCast.To<SqlDatabaseBuilderMock>( base.Database );
    public new SqlSchemaBuilderMock Schema => ReinterpretCast.To<SqlSchemaBuilderMock>( base.Schema );
    public new SqlColumnBuilderCollectionMock Columns => ReinterpretCast.To<SqlColumnBuilderCollectionMock>( base.Columns );
    public new SqlConstraintBuilderCollectionMock Constraints => ReinterpretCast.To<SqlConstraintBuilderCollectionMock>( base.Constraints );

    public new SqlTableBuilderMock SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    [Pure]
    public static SqlTableBuilderMock Create<TColumnType>(
        string name,
        string[] columns,
        string[]? pkColumns = null,
        bool areColumnsNullable = false,
        string? schemaName = null)
        where TColumnType : notnull
    {
        var db = SqlDatabaseBuilderMock.Create();
        var schema = schemaName is null ? db.Schemas.Default : db.Schemas.GetOrCreate( schemaName );
        var table = schema.Objects.CreateTable( name );

        foreach ( var c in columns )
            table.Columns.Create( c ).SetType<TColumnType>().MarkAsNullable( areColumnsNullable );

        pkColumns ??= columns;
        foreach ( var c in pkColumns )
            table.Columns.Get( c ).MarkAsNullable( false );

        table.Constraints.SetPrimaryKey( pkColumns.Select( c => table.Columns.Get( c ).Asc() ).ToArray() );
        return table;
    }
}
