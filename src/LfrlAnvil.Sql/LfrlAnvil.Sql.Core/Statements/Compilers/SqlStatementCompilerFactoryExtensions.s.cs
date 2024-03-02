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
    public static SqlScalarReaderExpression<T> CreateScalarExpression<T>(this ISqlQueryReaderFactory factory, bool isNullable = false)
    {
        var expression = factory.CreateScalarExpression( typeof( T ), isNullable );
        return new SqlScalarReaderExpression<T>( expression );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlScalarReader<T> CreateScalar<T>(this ISqlQueryReaderFactory factory, bool isNullable = false)
    {
        return factory.CreateScalarExpression<T>( isNullable ).Compile();
    }

    [Pure]
    public static SqlAsyncScalarReaderExpression<T> CreateAsyncScalarExpression<T>(
        this ISqlQueryReaderFactory factory,
        bool isNullable = false)
    {
        var expression = factory.CreateAsyncScalarExpression( typeof( T ), isNullable );
        return new SqlAsyncScalarReaderExpression<T>( expression );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAsyncScalarReader<T> CreateAsyncScalar<T>(this ISqlQueryReaderFactory factory, bool isNullable = false)
    {
        return factory.CreateAsyncScalarExpression<T>( isNullable ).Compile();
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
