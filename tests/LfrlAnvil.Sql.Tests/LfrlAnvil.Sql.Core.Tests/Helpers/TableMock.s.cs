using System.Diagnostics.Contracts;
using System.Linq;
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

        var columnsCollection = Substitute.For<ISqlColumnCollection>();
        columnsCollection.Table.Returns( result );
        columnsCollection.Count.Returns( columns.Length );
        columnsCollection.GetEnumerator().Returns( _ => columns.AsEnumerable().GetEnumerator() );
        columnsCollection.Contains( Arg.Any<string>() ).Returns( i => columns.Any( c => c.Name == i.ArgAt<string>( 0 ) ) );
        columnsCollection.Get( Arg.Any<string>() ).Returns( i => columns.First( c => c.Name == i.ArgAt<string>( 0 ) ) );
        columnsCollection.TryGet( Arg.Any<string>(), out Arg.Any<ISqlColumn?>() )
            .Returns(
                i =>
                {
                    i[1] = columns.FirstOrDefault( c => c.Name == i.ArgAt<string>( 0 ) );
                    return i[1] is not null;
                } );

        var pk = primaryKey?.Invoke( columnsCollection ) ?? PrimaryKeyMock.Create();
        result.Columns.Returns( columnsCollection );
        result.PrimaryKey.Returns( pk );
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

        var columnsCollection = Substitute.For<ISqlColumnBuilderCollection>();
        columnsCollection.Table.Returns( result );
        columnsCollection.Count.Returns( columns.Length );
        columnsCollection.GetEnumerator().Returns( _ => columns.AsEnumerable().GetEnumerator() );
        columnsCollection.Contains( Arg.Any<string>() ).Returns( i => columns.Any( c => c.Name == i.ArgAt<string>( 0 ) ) );
        columnsCollection.Get( Arg.Any<string>() ).Returns( i => columns.First( c => c.Name == i.ArgAt<string>( 0 ) ) );
        columnsCollection.TryGet( Arg.Any<string>(), out Arg.Any<ISqlColumnBuilder?>() )
            .Returns(
                i =>
                {
                    i[1] = columns.FirstOrDefault( c => c.Name == i.ArgAt<string>( 0 ) );
                    return i[1] is not null;
                } );

        var pk = primaryKey?.Invoke( columnsCollection );
        result.Columns.Returns( columnsCollection );
        result.PrimaryKey.Returns( pk );
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
