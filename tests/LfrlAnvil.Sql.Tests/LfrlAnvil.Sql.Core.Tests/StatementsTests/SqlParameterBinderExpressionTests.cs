using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Tests.Helpers.Data;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlParameterBinderExpressionTests : TestsBase
{
    [Fact]
    public void Compile_ShouldCreateCorrectParameterBinder()
    {
        var command = new DbCommand();
        var dialect = new SqlDialect( "foo" );
        var sourceType = typeof( string );

        var createParameterMethod = typeof( DbCommand ).GetMethod( nameof( DbCommand.CreateParameter ) )!;
        var parametersProperty = typeof( DbCommand ).GetProperty(
            nameof( DbCommand.Parameters ),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly )!;

        var addMethod = typeof( DbDataParameterCollection ).GetMethod(
            nameof( DbDataParameterCollection.Add ),
            BindingFlags.Public | BindingFlags.Instance,
            new[] { typeof( DbDataParameter ) } )!;

        var nameProperty = typeof( DbDataParameter ).GetProperty( nameof( DbDataParameter.ParameterName ) )!;
        var valueProperty = typeof( DbDataParameter ).GetProperty( nameof( DbDataParameter.Value ) )!;
        var lengthProperty = sourceType.GetProperty( nameof( string.Length ) )!;

        var cmdParameter = Expression.Parameter( typeof( IDbCommand ), "cmd" );
        var sourceParameter = Expression.Parameter( sourceType, "source" );
        var cmdVariable = Expression.Variable( typeof( DbCommand ), "fooCmd" );
        var parameterVariable = Expression.Variable( typeof( DbDataParameter ), "parameter" );
        var lengthConst = Expression.Constant( "length" );
        var valueConst = Expression.Constant( "value" );

        var expression = Expression.Lambda<Action<IDbCommand, string>>(
            Expression.Block(
                new[] { cmdVariable, parameterVariable },
                Expression.Assign( cmdVariable, Expression.Convert( cmdParameter, typeof( DbCommand ) ) ),
                Expression.Assign( parameterVariable, Expression.Call( cmdVariable, createParameterMethod ) ),
                Expression.Assign( Expression.MakeMemberAccess( parameterVariable, nameProperty ), lengthConst ),
                Expression.Assign(
                    Expression.MakeMemberAccess( parameterVariable, valueProperty ),
                    Expression.Convert( Expression.MakeMemberAccess( sourceParameter, lengthProperty ), typeof( object ) ) ),
                Expression.Call( Expression.MakeMemberAccess( cmdVariable, parametersProperty ), addMethod, parameterVariable ),
                Expression.Assign( parameterVariable, Expression.Call( cmdVariable, createParameterMethod ) ),
                Expression.Assign( Expression.MakeMemberAccess( parameterVariable, nameProperty ), valueConst ),
                Expression.Assign(
                    Expression.MakeMemberAccess( parameterVariable, valueProperty ),
                    Expression.Convert( sourceParameter, typeof( object ) ) ),
                Expression.Call( Expression.MakeMemberAccess( cmdVariable, parametersProperty ), addMethod, parameterVariable ) ),
            cmdParameter,
            sourceParameter );

        var @base = new SqlParameterBinderExpression( dialect, sourceType, expression );
        var sut = new SqlParameterBinderExpression<string>( @base );

        var parameterBinder = sut.Compile();
        parameterBinder.Bind( command, "lorem" );

        using ( new AssertionScope() )
        {
            sut.Dialect.Should().BeSameAs( dialect );
            sut.Expression.Should().BeSameAs( expression );
            parameterBinder.Dialect.Should().BeSameAs( dialect );
            command.Parameters.Should().HaveCount( 2 );
            command.Parameters[0].ParameterName.Should().Be( "length" );
            command.Parameters[0].Value.Should().Be( 5 );
            command.Parameters[1].ParameterName.Should().Be( "value" );
            command.Parameters[1].Value.Should().Be( "lorem" );
        }
    }
}
