namespace LfrlAnvil.Sql.Expressions.Traits;

/// <summary>
/// Represents an SQL syntax tree node that defines a window frame.
/// </summary>
public class SqlWindowFrameNode : SqlNodeBase
{
    /// <summary>
    /// Creates a new <see cref="SqlWindowFrameNode"/> instance with <see cref="SqlWindowFrameType.Custom"/> type.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    protected SqlWindowFrameNode(SqlWindowFrameBoundary start, SqlWindowFrameBoundary end)
        : this( SqlWindowFrameType.Custom, start, end ) { }

    internal SqlWindowFrameNode(SqlWindowFrameType frameType, SqlWindowFrameBoundary start, SqlWindowFrameBoundary end)
        : base( SqlNodeType.WindowFrame )
    {
        Assume.IsDefined( frameType );
        FrameType = frameType;
        Start = start;
        End = end;
    }

    /// <summary>
    /// <see cref="SqlWindowFrameType"/> of this frame.
    /// </summary>
    public SqlWindowFrameType FrameType { get; }

    /// <summary>
    /// Beginning <see cref="SqlWindowFrameBoundary"/> of this frame.
    /// </summary>
    public SqlWindowFrameBoundary Start { get; }

    /// <summary>
    /// Ending <see cref="SqlWindowFrameBoundary"/> of this frame.
    /// </summary>
    public SqlWindowFrameBoundary End { get; }
}
