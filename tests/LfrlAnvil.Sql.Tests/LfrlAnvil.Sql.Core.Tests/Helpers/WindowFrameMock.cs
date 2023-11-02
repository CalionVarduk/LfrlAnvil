using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class WindowFrameMock : SqlWindowFrameNode
{
    public WindowFrameMock(SqlWindowFrameBoundary? start = null, SqlWindowFrameBoundary? end = null)
        : base( start ?? SqlWindowFrameBoundary.CurrentRow, end ?? SqlWindowFrameBoundary.CurrentRow ) { }
}
