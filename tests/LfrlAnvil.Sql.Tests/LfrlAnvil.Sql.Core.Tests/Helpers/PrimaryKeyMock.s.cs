using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Tests.Helpers;

public static class PrimaryKeyMock
{
    [Pure]
    public static ISqlPrimaryKey Create(params ISqlIndexColumn[] columns)
    {
        var result = Substitute.For<ISqlPrimaryKey>();
        result.Type.Returns( SqlObjectType.PrimaryKey );
        var index = Substitute.For<ISqlIndex>();
        index.Columns.Returns( new ReadOnlyMemory<ISqlIndexColumn>( columns ) );
        result.Index.Returns( index );
        return result;
    }

    [Pure]
    public static ISqlPrimaryKeyBuilder CreateBuilder(params SqlIndexColumnBuilder<ISqlColumnBuilder>[] columns)
    {
        var result = Substitute.For<ISqlPrimaryKeyBuilder>();
        result.Type.Returns( SqlObjectType.PrimaryKey );
        var index = Substitute.For<ISqlIndexBuilder>();
        index.Columns.Returns( columns );
        result.Index.Returns( index );
        return result;
    }
}
