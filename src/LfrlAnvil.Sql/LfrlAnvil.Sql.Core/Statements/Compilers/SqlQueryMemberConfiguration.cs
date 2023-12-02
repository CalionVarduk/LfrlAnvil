using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sql.Statements.Compilers;

public readonly struct SqlQueryMemberConfiguration
{
    private SqlQueryMemberConfiguration(string memberName, string? sourceFieldName, LambdaExpression? customMapping)
    {
        MemberName = memberName;
        SourceFieldName = sourceFieldName;
        CustomMapping = customMapping;
    }

    public string MemberName { get; }
    public string? SourceFieldName { get; }
    public LambdaExpression? CustomMapping { get; }
    public bool IsIgnored => SourceFieldName is null && CustomMapping is null;
    public Type? CustomMappingDataReaderType => CustomMapping?.Parameters[0].Type.GetGenericArguments()[0];
    public Type? CustomMappingMemberType => CustomMapping?.Body.Type;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlQueryMemberConfiguration Ignore(string memberName)
    {
        return new SqlQueryMemberConfiguration( memberName, null, null );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlQueryMemberConfiguration From(string memberName, string sourceFieldName)
    {
        return new SqlQueryMemberConfiguration( memberName, sourceFieldName, null );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlQueryMemberConfiguration From<TDataRecord, TMemberType>(
        string memberName,
        Expression<Func<ISqlDataRecordFacade<TDataRecord>, TMemberType>> mapping)
        where TDataRecord : IDataRecord
    {
        var constPropagator = new ConstantFieldNamePropagator( mapping.Parameters[0] );
        var body = constPropagator.Visit( mapping.Body );

        if ( constPropagator.Errors.Count > 0 )
            throw new SqlCompilerConfigurationException( constPropagator.Errors );

        var processedMapping = ReferenceEquals( body, mapping.Body )
            ? mapping
            : Expression.Lambda<Func<ISqlDataRecordFacade<TDataRecord>, TMemberType>>( body, mapping.Parameters[0] );

        return new SqlQueryMemberConfiguration( memberName, null, processedMapping );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlQueryMemberConfiguration From<T>(string memberName, Expression<Func<ISqlDataRecordFacade<IDataRecord>, T>> mapping)
    {
        return From<IDataRecord, T>( memberName, mapping );
    }

    private sealed class ConstantFieldNamePropagator : ExpressionVisitor
    {
        private readonly ParameterExpression _readerFacade;

        internal ConstantFieldNamePropagator(ParameterExpression readerFacade)
        {
            _readerFacade = readerFacade;
            Errors = Chain<Pair<Expression, Exception>>.Empty;
        }

        internal Chain<Pair<Expression, Exception>> Errors { get; private set; }

        [Pure]
        [return: NotNullIfNotNull( "node" )]
        public override Expression? Visit(Expression? node)
        {
            return node is not null && node.NodeType == ExpressionType.Call && node is MethodCallExpression call
                ? TryVisitFacadeMethodCall( call )
                : base.Visit( node );
        }

        [Pure]
        private Expression TryVisitFacadeMethodCall(MethodCallExpression node)
        {
            if ( ! ReferenceEquals( node.Object, _readerFacade ) )
                return base.Visit( node );

            Assume.IsNotEmpty( node.Arguments );
            var nameArgument = node.Arguments[0];
            Assume.Equals( nameArgument.Type, typeof( string ) );

            if ( nameArgument is ConstantExpression || ! TryGetNameValue( nameArgument, out var constName ) )
                return base.Visit( node );

            var arguments = new Expression[node.Arguments.Count];
            arguments[0] = Expression.Constant( constName, typeof( string ) );
            for ( var i = 1; i < arguments.Length; ++i )
                arguments[i] = base.Visit( node.Arguments[i] );

            return Expression.Call( base.Visit( node.Object ), node.Method, arguments );
        }

        private bool TryGetNameValue(Expression node, [MaybeNullWhen( false )] out string name)
        {
            try
            {
                var expression = Expression.Lambda<Func<string>>( node );
                var @delegate = expression.Compile();
                name = @delegate();
                return true;
            }
            catch ( Exception exc )
            {
                Errors = Errors.Extend( Pair.Create( node, exc ) );
                name = default;
                return false;
            }
        }
    }
}
