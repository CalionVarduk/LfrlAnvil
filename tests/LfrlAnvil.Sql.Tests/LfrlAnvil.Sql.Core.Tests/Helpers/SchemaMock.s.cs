using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Tests.Helpers;

public static class SchemaMock
{
    [Pure]
    public static ISqlSchema Create()
    {
        return Create( string.Empty );
    }

    [Pure]
    public static ISqlSchemaBuilder CreateBuilder()
    {
        return CreateBuilder( string.Empty );
    }

    [Pure]
    public static ISqlSchema Create(string name)
    {
        var result = Substitute.For<ISqlSchema>();
        result.Type.Returns( SqlObjectType.Schema );
        result.Name.Returns( name );
        result.FullName.Returns( name );
        return result;
    }

    [Pure]
    public static ISqlSchemaBuilder CreateBuilder(string name)
    {
        var result = Substitute.For<ISqlSchemaBuilder>();
        result.Type.Returns( SqlObjectType.Schema );
        result.Name.Returns( name );
        result.FullName.Returns( name );
        return result;
    }
}
