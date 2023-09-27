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

        var asc = Substitute.For<ISqlIndexColumn>();
        asc.Column.Returns( result );
        asc.Ordering.Returns( OrderBy.Asc );

        var desc = Substitute.For<ISqlIndexColumn>();
        desc.Column.Returns( result );
        desc.Ordering.Returns( OrderBy.Desc );

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

        var asc = Substitute.For<ISqlIndexColumnBuilder>();
        asc.Column.Returns( result );
        asc.Ordering.Returns( OrderBy.Asc );

        var desc = Substitute.For<ISqlIndexColumnBuilder>();
        desc.Column.Returns( result );
        desc.Ordering.Returns( OrderBy.Desc );

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
