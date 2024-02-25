using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlViewDataFieldMock : SqlViewDataField
{
    public SqlViewDataFieldMock(SqlView view, string name)
        : base( view, name ) { }
}
