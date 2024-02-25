using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlColumnBuilderCollectionMock : SqlColumnBuilderCollection
{
    public SqlColumnBuilderCollectionMock(SqlColumnTypeDefinition typeDefinition)
        : base( typeDefinition ) { }

    public new SqlTableBuilderMock Table => ReinterpretCast.To<SqlTableBuilderMock>( base.Table );

    public new SqlColumnBuilderCollectionMock SetDefaultTypeDefinition(SqlColumnTypeDefinition definition)
    {
        base.SetDefaultTypeDefinition( definition );
        return this;
    }

    [Pure]
    public new SqlColumnBuilderMock Get(string name)
    {
        return ReinterpretCast.To<SqlColumnBuilderMock>( base.Get( name ) );
    }

    [Pure]
    public new SqlColumnBuilderMock? TryGet(string name)
    {
        return ReinterpretCast.To<SqlColumnBuilderMock>( base.TryGet( name ) );
    }

    public new SqlColumnBuilderMock Create(string name)
    {
        return ReinterpretCast.To<SqlColumnBuilderMock>( base.Create( name ) );
    }

    public new SqlColumnBuilderMock GetOrCreate(string name)
    {
        return ReinterpretCast.To<SqlColumnBuilderMock>( base.GetOrCreate( name ) );
    }

    [Pure]
    public new SqlObjectBuilderEnumerator<SqlColumnBuilder, SqlColumnBuilderMock> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<SqlColumnBuilderMock>();
    }

    protected override SqlColumnBuilder CreateColumnBuilder(string name)
    {
        return new SqlColumnBuilderMock( Table, name, DefaultTypeDefinition );
    }
}
