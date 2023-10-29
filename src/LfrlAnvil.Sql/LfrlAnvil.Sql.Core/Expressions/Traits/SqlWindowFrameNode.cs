namespace LfrlAnvil.Sql.Expressions.Traits;

public class SqlWindowFrameNode : SqlNodeBase
{
    protected SqlWindowFrameNode(SqlWindowFrameBoundary start, SqlWindowFrameBoundary end)
        : this( SqlWindowFrameType.Custom, start, end ) { }

    internal SqlWindowFrameNode(SqlWindowFrameType frameType, SqlWindowFrameBoundary start, SqlWindowFrameBoundary end)
        : base( SqlNodeType.WindowFrame )
    {
        Assume.IsDefined( frameType, nameof( frameType ) );
        FrameType = frameType;
        Start = start;
        End = end;
    }

    public SqlWindowFrameType FrameType { get; }
    public SqlWindowFrameBoundary Start { get; }
    public SqlWindowFrameBoundary End { get; }
}
