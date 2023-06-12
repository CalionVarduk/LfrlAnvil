using System.Text;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlRawSelectFieldNode : SqlSelectNode
{
    internal SqlRawSelectFieldNode(string? recordSetName, string name, string? alias, SqlExpressionType? type)
        : base( SqlNodeType.RawSelectField )
    {
        RecordSetName = recordSetName;
        Name = name;
        Alias = alias;
        Type = type;
    }

    public string? RecordSetName { get; }
    public string Name { get; }
    public string? Alias { get; }
    public override SqlExpressionType? Type { get; }
    public string FieldName => Alias ?? Name;

    public override void RegisterKnownFields(SqlQueryRecordSetNode.FieldInitializer initializer)
    {
        initializer.AddField( FieldName, Type );
    }

    public override void RegisterCompoundSelection(SqlCompoundQueryExpressionNode.SelectionInitializer initializer)
    {
        initializer.AddSelection( FieldName, Type );
    }

    protected override void ToString(StringBuilder builder, int indent)
    {
        builder.Append( '(' );
        if ( RecordSetName is not null )
            builder.Append( '[' ).Append( RecordSetName ).Append( ']' ).Append( '.' );

        builder.Append( '[' ).Append( Name ).Append( ']' );
        AppendTypeTo( builder, Type );
        builder.Append( ')' );

        if ( Alias is not null )
            builder.Append( ' ' ).Append( "AS" ).Append( ' ' ).Append( '[' ).Append( Alias ).Append( ']' );
    }
}
