using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Internal.Resolvers;
using LfrlAnvil.Dependencies.Internal.Resolvers.Factories;
using LfrlAnvil.Extensions;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies.Internal;

internal sealed class ExpressionBuilder
{
    private static readonly MethodInfo ArrayEmptyGenericMethod = typeof( Array ).GetMethod( nameof( Array.Empty ) )!;

    private static readonly MethodInfo ResolverCreateMethod =
        typeof( DependencyResolver ).GetMethod( nameof( DependencyResolver.Create ), BindingFlags.Instance | BindingFlags.NonPublic )!;

    private static readonly ConstructorInfo ExceptionCtor =
        typeof( InvalidDependencyCastException ).GetConstructor( new[] { typeof( Type ) } )!;

    private Expression? _abstractScopeParameter;
    private ConstantExpression? _nullObject;
    private int _variableIndex;
    private int _blockIndex;

    internal ExpressionBuilder(
        int dependencyCount,
        int defaultDependencyCount,
        bool hasRequiredValueTypeDependency,
        Type implementorType,
        Action<object, Type, IDependencyScope>? onCreatedCallback = null)
    {
        Assume.IsGreaterThanOrEqualTo( dependencyCount, 0 );
        Assume.IsInRange( defaultDependencyCount, 0, dependencyCount );

        ScopeParameter = Expression.Parameter( typeof( DependencyScope ), "scope" );

        var variableCount = dependencyCount;
        var blockCount = defaultDependencyCount + (dependencyCount - defaultDependencyCount) * 2;

        if ( hasRequiredValueTypeDependency )
        {
            Assume.IsGreaterThan( dependencyCount - defaultDependencyCount, 0 );
            BufferVariable = Expression.Variable( typeof( object ), "b" );
            ++variableCount;
        }

        if ( onCreatedCallback is not null )
        {
            blockCount += 2;
            if ( BufferVariable is null )
            {
                BufferVariable = Expression.Variable( typeof( object ), "b" );
                ++variableCount;
            }
        }

        if ( blockCount > 0 )
            ++blockCount;

        Variables = variableCount > 0 ? new ParameterExpression[variableCount] : Array.Empty<ParameterExpression>();
        Block = blockCount > 0 ? new Expression[blockCount] : Array.Empty<Expression>();

        if ( BufferVariable is not null )
        {
            Variables[^1] = BufferVariable;

            if ( onCreatedCallback is not null )
            {
                var callback = Expression.Invoke(
                    Expression.Constant( onCreatedCallback ),
                    BufferVariable,
                    Expression.Constant( implementorType, typeof( Type ) ),
                    AbstractScopeParameter );

                Block[^2] = callback;
                Block[^1] = BufferVariable;
            }
        }
    }

    internal ParameterExpression ScopeParameter { get; }
    internal ParameterExpression? BufferVariable { get; }
    internal ParameterExpression[] Variables { get; }
    internal Expression[] Block { get; }

    internal Expression AbstractScopeParameter =>
        _abstractScopeParameter ??= Expression.Convert( ScopeParameter, typeof( IDependencyScope ) );

    internal ConstantExpression NullObject => _nullObject ??= Expression.Constant( null );

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal IEnumerable<ParameterExpression> GetVariableRange(int count)
    {
        return count < Variables.Length ? Variables.Take( count ) : Variables;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal MemberBinding CreateMemberBindingForLastVariable(MemberInfo member, ConstructorInfo memberTypeCtor)
    {
        Assume.IsGreaterThan( _variableIndex, 0 );
        var variable = Variables[_variableIndex - 1];
        var result = Expression.Bind( member, Expression.New( memberTypeCtor, variable ) );
        return result;
    }

    internal ParameterExpression AddDefaultResolution(
        Type variableType,
        string variableName,
        bool hasDefaultValue = false,
        object? defaultValue = null)
    {
        var variable = AddVariable( variableType, variableName );
        var value = GetDefaultResolutionValue( variableType, hasDefaultValue, defaultValue );
        AddAssignment( variable, value );
        return variable;
    }

    internal ParameterExpression AddExpressionResolution(
        Type variableType,
        string variableName,
        Expression<Func<IDependencyScope, object>> expression)
    {
        var variable = AddVariable( variableType, variableName );
        var rawValue = GetExpressionResolutionRawValue( expression );
        AddRawValueAssignment( variable, rawValue );
        return variable;
    }

    internal ParameterExpression AddDependencyResolverFactoryResolution(
        Type variableType,
        string variableName,
        DependencyResolverFactory factory,
        UlongSequenceGenerator idGenerator)
    {
        var variable = AddVariable( variableType, variableName );
        var rawValue = GetDependencyResolverFactoryResolutionRawValue( variableType, factory, idGenerator );
        AddRawValueAssignment( variable, rawValue );
        return variable;
    }

    internal Expression<Func<DependencyScope, object>> Build(Expression instance)
    {
        if ( Block.Length == 0 )
            return Expression.Lambda<Func<DependencyScope, object>>( instance, ScopeParameter );

        if ( ReferenceEquals( Block[^1], null ) )
        {
            AddBlockExpression( instance );
            Assume.Equals( _blockIndex, Block.Length );
        }
        else
        {
            Assume.IsNotNull( Block[^2] );
            Assume.IsNotNull( BufferVariable );
            AddAssignment( BufferVariable, instance );
            Assume.Equals( _blockIndex, Block.Length - 2 );
        }

        var result = Expression.Lambda<Func<DependencyScope, object>>( Expression.Block( Variables, Block ), ScopeParameter );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MethodInfo GetClosedArrayEmptyMethod(Type elementType)
    {
        return ArrayEmptyGenericMethod.MakeGenericMethod( elementType );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Expression CreateArrayEmptyCallExpression(Type elementType)
    {
        return Expression.Call( null, GetClosedArrayEmptyMethod( elementType ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ParameterExpression AddVariable(Type type, string name)
    {
        var variable = Expression.Variable( type, name );
        AddVariable( variable );
        return variable;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void AddAssignment(ParameterExpression variable, Expression value)
    {
        var assignment = Expression.Assign( variable, value );
        AddBlockExpression( assignment );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Expression GetDefaultResolutionValue(Type type, bool hasDefaultValue, object? defaultValue)
    {
        if ( hasDefaultValue )
            return Expression.Constant( defaultValue, type );

        if ( type.IsGenericType && type.GetGenericTypeDefinition() == typeof( IEnumerable<> ) )
        {
            var elementType = type.GetGenericArguments()[0];
            return Expression.Call( null, GetClosedArrayEmptyMethod( elementType ) );
        }

        return Expression.Default( type );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Expression GetExpressionResolutionRawValue(Expression<Func<IDependencyScope, object>> expression)
    {
        var expressionParameter = expression.Parameters[0];

        var result = expressionParameter.Name is not null
            ? expression.Body.ReplaceParameters(
                new Dictionary<string, Expression> { { expressionParameter.Name, AbstractScopeParameter } } )
            : Expression.Invoke( expression, AbstractScopeParameter );

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Expression GetDependencyResolverFactoryResolutionRawValue(
        Type variableType,
        DependencyResolverFactory factory,
        UlongSequenceGenerator idGenerator)
    {
        factory.Build( idGenerator );
        var resolver = factory.GetResolver();

        return Expression.Call(
            Expression.Constant( resolver ),
            ResolverCreateMethod,
            ScopeParameter,
            Expression.Constant( variableType, typeof( Type ) ) );
    }

    private void AddRawValueAssignment(ParameterExpression variable, Expression rawValue)
    {
        if ( ! variable.Type.IsValueType )
        {
            AddRefRawValueAssignment( variable, rawValue );
            return;
        }

        if ( Nullable.GetUnderlyingType( variable.Type ) is not null )
        {
            AddNullableStructRawValueAssignment( variable, rawValue );
            return;
        }

        AddRequiredStructRawValueAssignment( variable, rawValue );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void AddRefRawValueAssignment(ParameterExpression variable, Expression rawValue)
    {
        var value = Expression.TypeAs( rawValue, variable.Type );
        var nullEquality = Expression.ReferenceEqual( variable, NullObject );
        var ifNull = Expression.IfThen( nullEquality, Expression.Throw( GetExceptionExpression( variable ) ) );

        AddAssignment( variable, value );
        AddBlockExpression( ifNull );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void AddNullableStructRawValueAssignment(ParameterExpression variable, Expression rawValue)
    {
        var value = Expression.TypeAs( rawValue, variable.Type );
        var nullEquality = Expression.Equal( variable, Expression.Constant( null, variable.Type ) );
        var ifNull = Expression.IfThen( nullEquality, Expression.Throw( GetExceptionExpression( variable ) ) );

        AddAssignment( variable, value );
        AddBlockExpression( ifNull );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void AddRequiredStructRawValueAssignment(ParameterExpression variable, Expression rawValue)
    {
        Assume.IsNotNull( BufferVariable );

        var typeCheck = Expression.TypeIs( BufferVariable, variable.Type );
        var value = Expression.Condition(
            typeCheck,
            Expression.Convert( BufferVariable, variable.Type ),
            Expression.Throw( GetExceptionExpression( variable ), variable.Type ) );

        AddAssignment( BufferVariable, rawValue );
        AddAssignment( variable, value );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void AddVariable(ParameterExpression variable)
    {
        Assume.IsLessThan( _variableIndex, Variables.Length );
        Assume.IsNull( Variables[_variableIndex] );
        Variables[_variableIndex++] = variable;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void AddBlockExpression(Expression expression)
    {
        Assume.IsLessThan( _blockIndex, Block.Length );
        Assume.IsNull( Block[_blockIndex] );
        Block[_blockIndex++] = expression;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static NewExpression GetExceptionExpression(ParameterExpression variable)
    {
        return Expression.New( ExceptionCtor, Expression.Constant( variable.Type, typeof( Type ) ) );
    }
}
