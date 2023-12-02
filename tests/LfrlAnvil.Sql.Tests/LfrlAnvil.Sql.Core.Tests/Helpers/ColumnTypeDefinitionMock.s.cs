using System.Data;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Tests.Helpers.Data;

namespace LfrlAnvil.Sql.Tests.Helpers;

public static class ColumnTypeDefinitionMock
{
    [Pure]
    public static ISqlColumnTypeDefinition<T> Create<T>(
        ISqlDataType dataType,
        Expression<Func<DbDataReader, int, T>> outputMapping,
        T? defaultValue = default)
        where T : notnull
    {
        var defaultNode = (SqlLiteralNode<T>)SqlNode.Literal( defaultValue );
        var result = Substitute.For<ISqlColumnTypeDefinition<T>>();
        result.DataType.Returns( dataType );
        result.RuntimeType.Returns( typeof( T ) );
        result.DefaultValue.Returns( defaultNode );
        ((ISqlColumnTypeDefinition)result).DefaultValue.Returns( defaultNode );
        result.OutputMapping.Returns( outputMapping );

        var dbType = dataType.DbType;
        result.When( d => d.SetParameterInfo( Arg.Any<IDbDataParameter>(), Arg.Any<bool>() ) )
            .Do(
                i =>
                {
                    var p = i.ArgAt<DbDataParameter>( 0 );
                    p.IsNullable = i.ArgAt<bool>( 1 );
                    p.DbType = dbType;
                } );

        result.TryToParameterValue( Arg.Any<object>() ).Returns( i => i.ArgAt<object>( 0 ) is T t ? t : null );
        result.ToParameterValue( Arg.Any<T>() ).Returns( i => i.ArgAt<T>( 0 ) );

        return result;
    }
}
