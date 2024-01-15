using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.MySql.Internal.Expressions;

public sealed class MySqlDropSchemaNode : SqlNodeBase, ISqlStatementNode
{
    public MySqlDropSchemaNode(string name)
    {
        Name = name;
    }

    public string Name { get; }
    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;

    protected override void ToString(SqlNodeDebugInterpreter interpreter)
    {
        interpreter.Context.Sql.Append( "DROP" ).AppendSpace().Append( "SCHEMA" ).AppendSpace();
        interpreter.AppendDelimitedName( Name );
    }
}
