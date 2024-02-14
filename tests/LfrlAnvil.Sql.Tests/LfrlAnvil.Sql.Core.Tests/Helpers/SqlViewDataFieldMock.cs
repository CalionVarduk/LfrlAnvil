using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlViewDataFieldMock : SqlViewDataField
{
    public SqlViewDataFieldMock(SqlView view, string name)
        : base( view, name ) { }
}
