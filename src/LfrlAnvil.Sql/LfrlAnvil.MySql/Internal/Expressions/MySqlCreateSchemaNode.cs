using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.MySql.Internal.Expressions;

public sealed class MySqlCreateSchemaNode : SqlNodeBase, ISqlStatementNode
{
    public MySqlCreateSchemaNode(string name, bool ifNotExists)
    {
        Name = name;
        IfNotExists = ifNotExists;
    }

    public string Name { get; }
    public bool IfNotExists { get; }
    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;

    protected override void ToString(SqlNodeDebugInterpreter interpreter)
    {
        interpreter.Context.Sql.Append( "CREATE" ).AppendSpace().Append( "SCHEMA" ).AppendSpace();
        if ( IfNotExists )
            interpreter.Context.Sql.Append( "IF" ).AppendSpace().Append( "NOT" ).AppendSpace().Append( "EXISTS" ).AppendSpace();

        interpreter.AppendDelimitedName( Name );
    }
}
