using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlViewDataFieldCollectionMock : SqlViewDataFieldCollection
{
    public SqlViewDataFieldCollectionMock(SqlQueryExpressionNode source)
        : base( source ) { }

    [Pure]
    protected override SqlViewDataFieldMock CreateDataField(string name)
    {
        return new SqlViewDataFieldMock( View, name );
    }
}
