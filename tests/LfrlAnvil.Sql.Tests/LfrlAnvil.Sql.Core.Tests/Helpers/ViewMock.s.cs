using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Tests.Helpers;

public static class ViewMock
{
    [Pure]
    public static ISqlView Create(string name, ISqlSchema schema, SqlQueryExpressionNode? source = null)
    {
        var result = Substitute.For<ISqlView>();
        result.Type.Returns( SqlObjectType.View );
        result.Schema.Returns( schema );
        result.Name.Returns( name );
        var info = SqlRecordSetInfo.Create( schema.Name, name );
        result.Info.Returns( info );

        var fields = new List<ISqlViewDataField>();
        source?.ReduceKnownDataFieldExpressions(
            x =>
            {
                var field = Substitute.For<ISqlViewDataField>();
                field.Type.Returns( SqlObjectType.ViewDataField );
                field.View.Returns( result );
                field.Name.Returns( x.Key );
                fields.Add( field );
            } );

        var dataFields = Substitute.For<ISqlViewDataFieldCollection>();
        dataFields.View.Returns( result );
        dataFields.Count.Returns( fields.Count );
        dataFields.GetEnumerator().Returns( _ => fields.GetEnumerator() );
        dataFields.Contains( Arg.Any<string>() ).Returns( i => fields.Any( f => f.Name == i.ArgAt<string>( 0 ) ) );
        dataFields.GetField( Arg.Any<string>() ).Returns( i => fields.First( f => f.Name == i.ArgAt<string>( 0 ) ) );
        dataFields.TryGetField( Arg.Any<string>() ).Returns( i => fields.FirstOrDefault( f => f.Name == i.ArgAt<string>( 0 ) ) );
        result.DataFields.Returns( dataFields );

        var recordSet = SqlNode.View( result );
        result.RecordSet.Returns( recordSet );
        foreach ( var field in fields )
        {
            var fieldName = field.Name;
            var fieldNode = recordSet[fieldName];
            field.Node.Returns( fieldNode );
        }

        return result;
    }

    [Pure]
    public static ISqlViewBuilder CreateBuilder(string name, ISqlSchemaBuilder schema, SqlQueryExpressionNode? source = null)
    {
        var result = Substitute.For<ISqlViewBuilder>();
        result.Type.Returns( SqlObjectType.View );
        result.Schema.Returns( schema );
        result.Name.Returns( name );
        var info = SqlRecordSetInfo.Create( schema.Name, name );
        result.Info.Returns( info );
        result.Source.Returns( source ?? SqlNode.RawQuery( string.Empty ) );
        result.RecordSet.Returns( SqlNode.View( result ) );
        return result;
    }

    [Pure]
    public static ISqlView Create(string name, SqlQueryExpressionNode? source = null)
    {
        return Create( name, SchemaMock.Create(), source );
    }

    [Pure]
    public static ISqlViewBuilder CreateBuilder(string name, SqlQueryExpressionNode? source = null)
    {
        return CreateBuilder( name, SchemaMock.CreateBuilder(), source );
    }
}
