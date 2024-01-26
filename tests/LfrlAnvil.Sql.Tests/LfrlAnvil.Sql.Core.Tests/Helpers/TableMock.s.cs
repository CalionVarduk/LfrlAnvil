using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Tests.Helpers;

public static class TableMock
{
    [Pure]
    public static ISqlTable Create(
        string name,
        ISqlSchema schema,
        Func<ISqlColumnCollection, ISqlPrimaryKey>? primaryKey = null,
        params ISqlColumn[] columns)
    {
        var fullName = schema.Name.Length > 0 ? $"{schema.Name}.{name}" : name;
        var result = Substitute.For<ISqlTable>();
        result.Type.Returns( SqlObjectType.Table );
        result.Schema.Returns( schema );
        result.Name.Returns( name );
        result.FullName.Returns( fullName );
        var info = SqlRecordSetInfo.Create( schema.Name, name );
        result.Info.Returns( info );

        var columnsCollection = Substitute.For<ISqlColumnCollection>();
        columnsCollection.Table.Returns( result );
        columnsCollection.Count.Returns( columns.Length );
        columnsCollection.GetEnumerator().Returns( _ => columns.AsEnumerable().GetEnumerator() );
        columnsCollection.Contains( Arg.Any<string>() ).Returns( i => columns.Any( c => c.Name == i.ArgAt<string>( 0 ) ) );
        columnsCollection.GetColumn( Arg.Any<string>() ).Returns( i => columns.First( c => c.Name == i.ArgAt<string>( 0 ) ) );
        columnsCollection.TryGetColumn( Arg.Any<string>() ).Returns( i => columns.FirstOrDefault( c => c.Name == i.ArgAt<string>( 0 ) ) );

        var pk = primaryKey?.Invoke( columnsCollection ) ?? PrimaryKeyMock.Create();

        var constraintsCollection = Substitute.For<ISqlConstraintCollection>();
        constraintsCollection.Table.Returns( result );
        constraintsCollection.Count.Returns( 1 );
        constraintsCollection.PrimaryKey.Returns( pk );

        result.Columns.Returns( columnsCollection );
        result.Constraints.Returns( constraintsCollection );

        var recordSet = SqlNode.Table( result );
        result.RecordSet.Returns( recordSet );
        foreach ( var column in columns )
        {
            var columnName = column.Name;
            var columnNode = recordSet[columnName];
            column.Node.Returns( columnNode );
        }

        return result;
    }

    [Pure]
    public static ISqlTableBuilder CreateBuilder(
        string name,
        ISqlSchemaBuilder schema,
        Func<ISqlColumnBuilderCollection, ISqlPrimaryKeyBuilder>? primaryKey = null,
        params ISqlColumnBuilder[] columns)
    {
        var fullName = schema.Name.Length > 0 ? $"{schema.Name}.{name}" : name;
        var result = Substitute.For<ISqlTableBuilder>();
        result.Type.Returns( SqlObjectType.Table );
        result.Schema.Returns( schema );
        result.Name.Returns( name );
        result.FullName.Returns( fullName );
        var info = SqlRecordSetInfo.Create( schema.Name, name );
        result.Info.Returns( info );

        var columnsCollection = Substitute.For<ISqlColumnBuilderCollection>();
        columnsCollection.Table.Returns( result );
        columnsCollection.Count.Returns( columns.Length );
        columnsCollection.GetEnumerator().Returns( _ => columns.AsEnumerable().GetEnumerator() );
        columnsCollection.Contains( Arg.Any<string>() ).Returns( i => columns.Any( c => c.Name == i.ArgAt<string>( 0 ) ) );
        columnsCollection.GetColumn( Arg.Any<string>() ).Returns( i => columns.First( c => c.Name == i.ArgAt<string>( 0 ) ) );
        columnsCollection.TryGetColumn( Arg.Any<string>() ).Returns( i => columns.FirstOrDefault( c => c.Name == i.ArgAt<string>( 0 ) ) );

        var pk = primaryKey?.Invoke( columnsCollection );

        var constraintsCollection = Substitute.For<ISqlConstraintBuilderCollection>();
        constraintsCollection.Table.Returns( result );
        constraintsCollection.Count.Returns( pk is null ? 0 : 1 );
        constraintsCollection.GetPrimaryKey().Returns( _ => pk ?? throw new Exception( "PK is missing" ) );
        constraintsCollection.TryGetPrimaryKey().Returns( pk );

        result.Columns.Returns( columnsCollection );
        result.Constraints.Returns( constraintsCollection );

        var recordSet = SqlNode.Table( result );
        result.RecordSet.Returns( recordSet );
        foreach ( var column in columns )
        {
            var columnName = column.Name;
            var columnNode = recordSet[columnName];
            column.Node.Returns( columnNode );
        }

        return result;
    }

    [Pure]
    public static ISqlTable Create(string name, params ISqlColumn[] columns)
    {
        return Create( name, SchemaMock.Create(), null, columns );
    }

    [Pure]
    public static ISqlTableBuilder CreateBuilder(string name, params ISqlColumnBuilder[] columns)
    {
        return CreateBuilder( name, SchemaMock.CreateBuilder(), null, columns );
    }
}
