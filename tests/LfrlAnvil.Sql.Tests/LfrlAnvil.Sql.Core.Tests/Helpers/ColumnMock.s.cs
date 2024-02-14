using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Tests.Helpers;

public static class ColumnMock
{
    [Pure]
    public static ISqlColumn Create<T>(string name, bool isNullable = false)
    {
        var result = Substitute.For<ISqlColumn>();
        result.Type.Returns( SqlObjectType.Column );
        result.Name.Returns( name );
        result.IsNullable.Returns( isNullable );

        var typeDef = Substitute.For<ISqlColumnTypeDefinition>();
        typeDef.RuntimeType.Returns( typeof( T ) );
        result.TypeDefinition.Returns( typeDef );

        var asc = SqlIndexColumn.CreateAsc( result );
        var desc = SqlIndexColumn.CreateDesc( result );

        result.Asc().Returns( asc );
        result.Desc().Returns( desc );
        return result;
    }

    [Pure]
    public static ISqlColumnBuilder CreateBuilder<T>(string name, bool isNullable = false)
    {
        var result = Substitute.For<ISqlColumnBuilder>();
        result.Type.Returns( SqlObjectType.Column );
        result.Name.Returns( name );
        result.IsNullable.Returns( isNullable );

        var typeDef = Substitute.For<ISqlColumnTypeDefinition>();
        typeDef.RuntimeType.Returns( typeof( T ) );
        result.TypeDefinition.Returns( typeDef );

        var asc = new SqlIndexColumnBuilder<ISqlColumnBuilder>( result, OrderBy.Asc );
        var desc = new SqlIndexColumnBuilder<ISqlColumnBuilder>( result, OrderBy.Desc );
        result.Asc().Returns( asc );
        result.Desc().Returns( desc );

        return result;
    }

    [Pure]
    public static ISqlColumn[] CreateMany<T>(bool areNullable, params string[] names)
    {
        if ( names.Length == 0 )
            return Array.Empty<ISqlColumn>();

        var result = new ISqlColumn[names.Length];
        for ( var i = 0; i < names.Length; ++i )
            result[i] = Create<T>( names[i], areNullable );

        return result;
    }

    [Pure]
    public static ISqlColumnBuilder[] CreateManyBuilders<T>(bool areNullable, params string[] names)
    {
        if ( names.Length == 0 )
            return Array.Empty<ISqlColumnBuilder>();

        var result = new ISqlColumnBuilder[names.Length];
        for ( var i = 0; i < names.Length; ++i )
            result[i] = CreateBuilder<T>( names[i], areNullable );

        return result;
    }
}
