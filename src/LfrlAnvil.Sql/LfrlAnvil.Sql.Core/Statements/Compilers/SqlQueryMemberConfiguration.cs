using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sql.Statements.Compilers;

/// <summary>
/// Represents an explicit row member configuration for <see cref="ISqlQueryReaderFactory"/>.
/// </summary>
public readonly struct SqlQueryMemberConfiguration
{
    private SqlQueryMemberConfiguration(string memberName, string? sourceFieldName, LambdaExpression? customMapping)
    {
        MemberName = memberName;
        SourceFieldName = sourceFieldName;
        CustomMapping = customMapping;
    }

    /// <summary>
    /// Row type's field or property name.
    /// </summary>
    public string MemberName { get; }

    /// <summary>
    /// Name of the source field to read a value from.
    /// </summary>
    public string? SourceFieldName { get; }

    /// <summary>
    /// Custom value mapping from source to row type's member.
    /// </summary>
    public LambdaExpression? CustomMapping { get; }

    /// <summary>
    /// Specifies whether or not the associated row type member should be completely ignored.
    /// </summary>
    public bool IsIgnored => SourceFieldName is null && CustomMapping is null;

    /// <summary>
    /// Source data reader type from the <see cref="CustomMapping"/>.
    /// </summary>
    public Type? CustomMappingDataReaderType => CustomMapping?.Parameters[0].Type.GetGenericArguments()[0];

    /// <summary>
    /// Value type from the <see cref="CustomMapping"/>.
    /// </summary>
    public Type? CustomMappingMemberType => CustomMapping?.Body.Type;

    /// <summary>
    /// Creates a new <see cref="SqlQueryMemberConfiguration"/> instance that causes the provided row type member to be ignored.
    /// </summary>
    /// <param name="memberName">Row type's field or property name.</param>
    /// <returns>New <see cref="SqlQueryMemberConfiguration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlQueryMemberConfiguration Ignore(string memberName)
    {
        return new SqlQueryMemberConfiguration( memberName, null, null );
    }

    /// <summary>
    /// Creates a new <see cref="SqlQueryMemberConfiguration"/> instance that causes the provided row type member's value to be read
    /// from another source field.
    /// </summary>
    /// <param name="memberName">Row type's field or property name.</param>
    /// <param name="sourceFieldName">Name of the source field to read a value from..</param>
    /// <returns>New <see cref="SqlQueryMemberConfiguration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlQueryMemberConfiguration From(string memberName, string sourceFieldName)
    {
        return new SqlQueryMemberConfiguration( memberName, sourceFieldName, null );
    }

    /// <summary>
    /// Creates a new <see cref="SqlQueryMemberConfiguration"/> instance that causes the provided row type member's value to be read
    /// from a custom expression.
    /// </summary>
    /// <param name="memberName">Row type's field or property name.</param>
    /// <param name="mapping">Custom value mapping from source to row type's member.</param>
    /// <typeparam name="TDataRecord">Source DB data record type.</typeparam>
    /// <typeparam name="TMemberType">Row member type.</typeparam>
    /// <returns>New <see cref="SqlQueryMemberConfiguration"/> instance.</returns>
    /// <exception cref="SqlCompilerConfigurationException">
    /// When any field name used in <see cref="ISqlDataRecordFacade{TDataRecord}"/> method calls could not be resolved as a constant value.
    /// </exception>
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

    /// <summary>
    /// Creates a new <see cref="SqlQueryMemberConfiguration"/> instance that causes the provided row type member's value to be read
    /// from a custom expression.
    /// </summary>
    /// <param name="memberName">Row type's field or property name.</param>
    /// <param name="mapping">Custom value mapping from source to row type's member.</param>
    /// <typeparam name="T">Row member type.</typeparam>
    /// <returns>New <see cref="SqlQueryMemberConfiguration"/> instance.</returns>
    /// <exception cref="SqlCompilerConfigurationException">
    /// When any field name used in <see cref="ISqlDataRecordFacade{TDataRecord}"/> method calls could not be resolved as a constant value.
    /// </exception>
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
