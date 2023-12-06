using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements.Compilers;

public static class SqlStatementCompilerFactoryExtensions
{
    [Pure]
    public static SqlQueryReaderExpression<TRow> CreateExpression<TRow>(
        this ISqlQueryReaderFactory factory,
        SqlQueryReaderCreationOptions? options = null)
        where TRow : notnull
    {
        var expression = factory.CreateExpression( typeof( TRow ), options );
        return new SqlQueryReaderExpression<TRow>( expression );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlQueryReader<TRow> Create<TRow>(this ISqlQueryReaderFactory factory, SqlQueryReaderCreationOptions? options = null)
        where TRow : notnull
    {
        return factory.CreateExpression<TRow>( options ).Compile();
    }

    [Pure]
    public static SqlAsyncQueryReaderExpression<TRow> CreateAsyncExpression<TRow>(
        this ISqlQueryReaderFactory factory,
        SqlQueryReaderCreationOptions? options = null)
        where TRow : notnull
    {
        var expression = factory.CreateAsyncExpression( typeof( TRow ), options );
        return new SqlAsyncQueryReaderExpression<TRow>( expression );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAsyncQueryReader<TRow> CreateAsync<TRow>(
        this ISqlQueryReaderFactory factory,
        SqlQueryReaderCreationOptions? options = null)
        where TRow : notnull
    {
        return factory.CreateAsyncExpression<TRow>( options ).Compile();
    }

    [Pure]
    public static SqlParameterBinderExpression<TSource> CreateExpression<TSource>(
        this ISqlParameterBinderFactory factory,
        SqlParameterBinderCreationOptions? options = null)
        where TSource : notnull
    {
        var expression = factory.CreateExpression( typeof( TSource ), options );
        return new SqlParameterBinderExpression<TSource>( expression );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlParameterBinder<TSource> Create<TSource>(
        this ISqlParameterBinderFactory factory,
        SqlParameterBinderCreationOptions? options = null)
        where TSource : notnull
    {
        return factory.CreateExpression<TSource>( options ).Compile();
    }
}
