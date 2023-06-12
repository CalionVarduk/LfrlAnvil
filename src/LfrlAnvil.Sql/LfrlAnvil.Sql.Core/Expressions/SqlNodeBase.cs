using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Sql.Expressions;

public abstract class SqlNodeBase
{
    public const int DefaultIndent = 4;

    protected SqlNodeBase(SqlNodeType nodeType)
    {
        Assume.IsDefined( nodeType, nameof( nodeType ) );
        NodeType = nodeType;
    }

    public SqlNodeType NodeType { get; }

    [Pure]
    public sealed override string ToString()
    {
        var builder = new StringBuilder( capacity: 255 );
        ToString( builder, indent: 0 );
        return builder.ToString();
    }

    protected virtual void ToString(StringBuilder builder, int indent)
    {
        builder.Append( '{' ).Append( GetType().GetDebugString() ).Append( '}' );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AppendChildTo(StringBuilder builder, SqlNodeBase node, int indent)
    {
        node.ToString( builder.Append( '(' ), indent );
        builder.Append( ')' );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AppendTo(StringBuilder builder, SqlNodeBase node, int indent)
    {
        node.ToString( builder, indent );
    }

    protected static void AppendTypeTo(StringBuilder builder, SqlExpressionType? type)
    {
        builder.Append( ' ' ).Append( ':' ).Append( ' ' );
        if ( type is null )
            builder.Append( '?' );
        else
            builder.Append( type.Value.ToString() );
    }

    protected static void AppendInfixBinaryTo(
        StringBuilder builder,
        SqlNodeBase left,
        string @operator,
        SqlNodeBase right,
        int indent)
    {
        AppendChildTo( builder, left, indent );
        AppendChildTo( builder.Append( ' ' ).Append( @operator ).Append( ' ' ), right, indent );
    }
}
