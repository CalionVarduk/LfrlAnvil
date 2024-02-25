using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlSchemaBuilderCollectionMock : SqlSchemaBuilderCollection
{
    public new SqlDatabaseBuilderMock Database => ReinterpretCast.To<SqlDatabaseBuilderMock>( base.Database );
    public new SqlSchemaBuilderMock Default => ReinterpretCast.To<SqlSchemaBuilderMock>( base.Default );

    [Pure]
    public new SqlSchemaBuilderMock Get(string name)
    {
        return ReinterpretCast.To<SqlSchemaBuilderMock>( base.Get( name ) );
    }

    [Pure]
    public new SqlSchemaBuilderMock? TryGet(string name)
    {
        return ReinterpretCast.To<SqlSchemaBuilderMock>( base.TryGet( name ) );
    }

    public new SqlSchemaBuilderMock Create(string name)
    {
        return ReinterpretCast.To<SqlSchemaBuilderMock>( base.Create( name ) );
    }

    public new SqlSchemaBuilderMock GetOrCreate(string name)
    {
        return ReinterpretCast.To<SqlSchemaBuilderMock>( base.GetOrCreate( name ) );
    }

    [Pure]
    public new SqlObjectBuilderEnumerator<SqlSchemaBuilder, SqlSchemaBuilderMock> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<SqlSchemaBuilderMock>();
    }

    protected override SqlSchemaBuilder CreateSchemaBuilder(string name)
    {
        return new SqlSchemaBuilderMock( Database, name );
    }
}
