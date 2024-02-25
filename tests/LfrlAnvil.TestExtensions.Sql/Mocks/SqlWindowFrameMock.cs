using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlWindowFrameMock : SqlWindowFrameNode
{
    public SqlWindowFrameMock(SqlWindowFrameBoundary? start = null, SqlWindowFrameBoundary? end = null)
        : base( start ?? SqlWindowFrameBoundary.CurrentRow, end ?? SqlWindowFrameBoundary.CurrentRow ) { }
}
