using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlColumnCollectionMock : SqlColumnCollection
{
    public SqlColumnCollectionMock(SqlColumnBuilderCollection source)
        : base( source ) { }

    [Pure]
    protected override SqlColumnMock CreateColumn(SqlColumnBuilder builder)
    {
        return new SqlColumnMock( Table, builder );
    }
}
