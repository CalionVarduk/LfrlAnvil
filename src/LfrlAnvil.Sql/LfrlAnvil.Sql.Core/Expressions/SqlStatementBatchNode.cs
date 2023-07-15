using System;
using System.Data;
using System.Text;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlStatementBatchNode : SqlNodeBase
{
    internal SqlStatementBatchNode(IsolationLevel? isolationLevel, SqlNodeBase[] statements)
        : base( SqlNodeType.StatementBatch )
    {
        IsolationLevel = isolationLevel;
        Statements = statements;
    }

    public IsolationLevel? IsolationLevel { get; }
    public ReadOnlyMemory<SqlNodeBase> Statements { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        builder.Append( "BATCH" );

        if ( IsolationLevel is not null )
            builder
                .Append( ' ' )
                .Append( '<' )
                .Append( "ISOLATION LEVEL" )
                .Append( ' ' )
                .Append( IsolationLevel.Value.ToString().ToUpperInvariant() )
                .Append( '>' );

        if ( Statements.Length == 0 )
            return;

        builder.Indent( indent ).Append( '(' );
        var statementIndent = indent + DefaultIndent;

        foreach ( var statement in Statements.Span )
        {
            AppendTo( builder.Indent( statementIndent ), statement, statementIndent );
            builder.Append( ';' ).AppendLine();
        }

        builder.Append( ' ', repeatCount: indent ).Append( ')' );
    }
}
