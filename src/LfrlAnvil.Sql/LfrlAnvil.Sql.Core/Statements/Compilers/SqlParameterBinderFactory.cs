using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.Sql.Statements.Compilers;

public class SqlParameterBinderFactory : ISqlParameterBinderFactory
{
    private readonly object _sync = new object();
    private Toolbox _toolbox;

    internal SqlParameterBinderFactory(Type commandType, SqlDialect dialect, ISqlColumnTypeDefinitionProvider columnTypeDefinitions)
    {
        Assume.True( commandType.IsAssignableTo( typeof( IDbCommand ) ) );
        CommandType = commandType;
        Dialect = dialect;
        ColumnTypeDefinitions = columnTypeDefinitions;
        _toolbox = default;
    }

    public Type CommandType { get; }
    public SqlDialect Dialect { get; }
    public ISqlColumnTypeDefinitionProvider ColumnTypeDefinitions { get; }

    [Pure]
    public SqlParameterBinder Create(SqlParameterBinderCreationOptions? options = null)
    {
        var opt = options ?? SqlParameterBinderCreationOptions.Default;

        var @delegate = opt.ReduceCollections
            ? CreateDelegateWithReducedCollections( ColumnTypeDefinitions, opt.IgnoreNullValues )
            : CreateDelegateWithoutReducedCollections( ColumnTypeDefinitions, opt.IgnoreNullValues );

        return new SqlParameterBinder( Dialect, @delegate );
    }

    [Pure]
    public SqlParameterBinderExpression CreateExpression(Type sourceType, SqlParameterBinderCreationOptions? options = null)
    {
        var errors = Chain<string>.Empty;
        if ( sourceType.IsAbstract )
            errors = errors.Extend( ExceptionResources.SourceTypeCannotBeAbstract );

        if ( sourceType.IsGenericTypeDefinition )
            errors = errors.Extend( ExceptionResources.SourceTypeCannotBeOpenGeneric );

        if ( sourceType.IsValueType && Nullable.GetUnderlyingType( sourceType ) is not null )
            errors = errors.Extend( ExceptionResources.SourceTypeCannotBeNullable );

        if ( errors.Count > 0 )
            throw new SqlCompilerException( Dialect, errors );

        var source = Expression.Parameter( sourceType, "source" );
        var opt = options ?? SqlParameterBinderCreationOptions.Default;
        var body = CreateLambdaExpressionBody( source, in opt );

        var lambda = Expression.Lambda( body, _toolbox.AbstractCommand, source );
        return new SqlParameterBinderExpression( Dialect, sourceType, lambda );
    }

    [Pure]
    private Expression CreateLambdaExpressionBody(ParameterExpression source, in SqlParameterBinderCreationOptions options)
    {
        lock ( _sync )
        {
            if ( ! _toolbox.IsInitialized )
                _toolbox = Toolbox.Create( Dialect, CommandType, _sync );
        }

        var parameterSources = CreateParameterSources( source.Type, in options );
        var body = new Expression[parameterSources.Length + 5];
        body[0] = _toolbox.CommandAssignment;
        body[1] = _toolbox.CommandParametersAssignment;
        body[2] = _toolbox.OriginalCountAssignment;
        body[3] = _toolbox.IndexAssignment;

        var index = 4;
        foreach ( var p in parameterSources )
        {
            var parameterAssignment = _toolbox.CreateParameterAssignment( p, source, ColumnTypeDefinitions );
            body[index++] = parameterAssignment;
        }

        body[index] = _toolbox.ClearExcessParametersConditional;
        return Expression.Block( _toolbox.BlockParameters, body );
    }

    private readonly record struct StatementParameterInfo(
        string Name,
        object Source,
        SqlStatementTypeInfo Type,
        bool IgnoreWhenNull,
        bool IsReducibleCollection);

    [Pure]
    private StatementParameterInfo[] CreateParameterSources(Type sourceType, in SqlParameterBinderCreationOptions options)
    {
        var lookups = options.CreateParameterConfigurationLookups( sourceType );
        var fields = sourceType.GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
        var properties = sourceType.GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );

        var sourceCount = fields.Length + properties.Length + (lookups.SelectorsByParameterName?.Count ?? 0);
        if ( sourceCount == 0 )
            throw new SqlCompilerException( Dialect, ExceptionResources.RowTypeDoesNotHaveAnyValidMembers );

        var i = 0;
        var sources = new StatementParameterInfo[sourceCount];
        var predicate = options.SourceTypeMemberPredicate;

        foreach ( var f in fields )
        {
            if ( f.GetBackedProperty() is not null )
                continue;

            var cfg = lookups.GetMemberConfiguration( f.Name );
            if ( ! cfg.IsIgnored && (predicate is null || predicate( f )) )
                sources[i++] = CreateStatementParameter( f, cfg, options.IgnoreNullValues, options.ReduceCollections );
        }

        foreach ( var p in properties )
        {
            var getMethod = p.GetGetMethod( nonPublic: true );
            if ( getMethod is null )
                continue;

            var cfg = lookups.GetMemberConfiguration( p.Name );
            if ( ! cfg.IsIgnored && (predicate is null || predicate( p )) )
                sources[i++] = CreateStatementParameter( p, cfg, options.IgnoreNullValues, options.ReduceCollections );
        }

        if ( lookups.SelectorsByParameterName is not null )
        {
            foreach ( var cfg in lookups.SelectorsByParameterName.Values )
                sources[i++] = CreateStatementParameter( cfg, options.IgnoreNullValues, options.ReduceCollections );
        }

        if ( i == 0 )
            throw new SqlCompilerException( Dialect, ExceptionResources.RowTypeDoesNotHaveAnyValidMembers );

        var result = new StatementParameterInfo[i];
        sources.AsSpan( 0, i ).CopyTo( result );

        ValidateParameterNameDuplicates( result );

        if ( options.Context is not null )
            ValidateContext( result, options.Context );

        return result;
    }

    [Pure]
    private StatementParameterInfo CreateStatementParameter(
        MemberInfo member,
        SqlParameterConfiguration configuration,
        bool ignoreNullValues,
        bool reduceCollections)
    {
        Assume.IsNull( configuration.CustomSelector );
        Assume.IsNotNull( configuration.TargetParameterName );
        var type = SqlStatementTypeInfo.Create( member, _toolbox.NullContext );
        return new StatementParameterInfo(
            configuration.TargetParameterName,
            member,
            type,
            configuration.IsIgnoredWhenNull ?? ignoreNullValues,
            reduceCollections && type.IsReducibleCollection );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static StatementParameterInfo CreateStatementParameter(
        SqlParameterConfiguration configuration,
        bool ignoreNullValues,
        bool reduceCollections)
    {
        Assume.IsNotNull( configuration.CustomSelector );
        Assume.IsNotNull( configuration.TargetParameterName );
        var type = SqlStatementTypeInfo.Create( configuration.CustomSelector.Body.Type );
        return new StatementParameterInfo(
            configuration.TargetParameterName,
            configuration.CustomSelector,
            type,
            configuration.IsIgnoredWhenNull ?? ignoreNullValues,
            reduceCollections && type.IsReducibleCollection );
    }

    private void ValidateParameterNameDuplicates(ReadOnlySpan<StatementParameterInfo> sources)
    {
        var names = new HashSet<string>( capacity: sources.Length, comparer: StringComparer.OrdinalIgnoreCase );
        HashSet<string>? duplicatedNames = null;

        foreach ( var s in sources )
        {
            if ( names.Add( s.Name ) )
                continue;

            duplicatedNames ??= new HashSet<string>( comparer: StringComparer.OrdinalIgnoreCase );
            duplicatedNames.Add( s.Name );
        }

        if ( duplicatedNames is null || duplicatedNames.Count == 0 )
            return;

        var errors = Chain<string>.Empty;
        foreach ( var name in duplicatedNames )
            errors = errors.Extend( ExceptionResources.ParameterAppearsMoreThanOnce( name ) );

        throw new SqlCompilerException( Dialect, errors );
    }

    private void ValidateContext(ReadOnlySpan<StatementParameterInfo> sources, SqlNodeInterpreterContext context)
    {
        var existingParameters = new HashSet<string>( capacity: sources.Length, comparer: StringComparer.OrdinalIgnoreCase );
        var parameterErrors = Chain<string>.Empty;

        foreach ( var s in sources )
        {
            existingParameters.Add( s.Name );
            if ( ! context.TryGetParameterType( s.Name, out var parameterType ) )
            {
                var error = ExceptionResources.UnexpectedStatementParameter( s.Name, s.Type.ActualType );
                parameterErrors = parameterErrors.Extend( error );
                continue;
            }

            if ( parameterType is null )
                continue;

            if ( (s.Type.IsNullable && ! parameterType.Value.IsNullable) ||
                ! s.Type.ActualType.IsAssignableTo( parameterType.Value.ActualType ) )
            {
                var error = ExceptionResources.IncompatibleStatementParameterType( s.Name, parameterType.Value, s.Type.ActualType );
                parameterErrors = parameterErrors.Extend( error );
                continue;
            }

            if ( s.Type.IsNullable && s.IgnoreWhenNull )
            {
                var error = ExceptionResources.RequiredStatementParameterIsIgnoredWhenNull( s.Name, s.Type.ActualType );
                parameterErrors = parameterErrors.Extend( error );
            }
        }

        foreach ( var p in context.Parameters )
        {
            if ( existingParameters.Contains( p.Key ) )
                continue;

            var error = ExceptionResources.MissingStatementParameter( p.Key, p.Value );
            parameterErrors = parameterErrors.Extend( error );
        }

        if ( parameterErrors.Count > 0 )
            throw new SqlCompilerException( Dialect, parameterErrors );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Action<IDbCommand, IEnumerable<KeyValuePair<string, object?>>> CreateDelegateWithReducedCollections(
        ISqlColumnTypeDefinitionProvider typeDefinitions,
        bool ignoreNullValues)
    {
        return (command, source) =>
        {
            using var enumerator = source.GetEnumerator();
            if ( ! enumerator.MoveNext() )
            {
                command.Parameters.Clear();
                return;
            }

            var index = 0;
            var parameters = command.Parameters;
            var originalCount = parameters.Count;
            do
            {
                var (name, value) = enumerator.Current;
                if ( value is null )
                {
                    if ( ! ignoreNullValues )
                        HandleNullValue( command, parameters, name, index++, originalCount, typeDefinitions );
                }
                else if ( value is IEnumerable enumerable and not string and not byte[] )
                {
                    var elementNo = 1;
                    foreach ( var element in enumerable )
                    {
                        if ( element is null )
                        {
                            if ( ! ignoreNullValues )
                                HandleNullValue( command, parameters, $"{name}{elementNo++}", index++, originalCount, typeDefinitions );
                        }
                        else
                            HandleValue( command, parameters, $"{name}{elementNo++}", element, index++, originalCount, typeDefinitions );
                    }
                }
                else
                    HandleValue( command, parameters, name, value, index++, originalCount, typeDefinitions );
            }
            while ( enumerator.MoveNext() );

            ClearExcessParameters( parameters, index, originalCount );
        };
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Action<IDbCommand, IEnumerable<KeyValuePair<string, object?>>> CreateDelegateWithoutReducedCollections(
        ISqlColumnTypeDefinitionProvider typeDefinitions,
        bool ignoreNullValues)
    {
        return (command, source) =>
        {
            using var enumerator = source.GetEnumerator();
            if ( ! enumerator.MoveNext() )
            {
                command.Parameters.Clear();
                return;
            }

            var index = 0;
            var parameters = command.Parameters;
            var originalCount = parameters.Count;
            do
            {
                var (name, value) = enumerator.Current;
                if ( value is null )
                {
                    if ( ! ignoreNullValues )
                        HandleNullValue( command, parameters, name, index++, originalCount, typeDefinitions );
                }
                else
                    HandleValue( command, parameters, name, value, index++, originalCount, typeDefinitions );
            }
            while ( enumerator.MoveNext() );

            ClearExcessParameters( parameters, index, originalCount );
        };
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void ClearExcessParameters(IDataParameterCollection parameters, int index, int originalCount)
    {
        if ( index == 0 )
            parameters.Clear();
        else
        {
            for ( var i = originalCount - 1; i >= index; --i )
                parameters.RemoveAt( i );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void HandleNullValue(
        IDbCommand command,
        IDataParameterCollection parameters,
        string name,
        int index,
        int originalCount,
        ISqlColumnTypeDefinitionProvider typeDefinitions)
    {
        var typeDef = typeDefinitions.GetByType( typeof( object ) );
        var parameter = GetOrCreateParameter( command, parameters, name, index, originalCount );
        typeDef.SetParameterInfo( parameter, isNullable: true );
        parameter.Value = DBNull.Value;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void HandleValue(
        IDbCommand command,
        IDataParameterCollection parameters,
        string name,
        object value,
        int index,
        int originalCount,
        ISqlColumnTypeDefinitionProvider typeDefinitions)
    {
        var typeDef = typeDefinitions.GetByType( value.GetType() );
        var parameter = GetOrCreateParameter( command, parameters, name, index, originalCount );
        typeDef.SetParameterInfo( parameter, isNullable: false );
        parameter.Value = typeDef.TryToParameterValue( value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static IDbDataParameter GetOrCreateParameter(
        IDbCommand command,
        IDataParameterCollection parameters,
        string name,
        int index,
        int originalCount)
    {
        IDbDataParameter? parameter;
        if ( index < originalCount )
        {
            parameter = (IDbDataParameter?)parameters[index];
            Ensure.IsNotNull( parameter );
        }
        else
        {
            parameter = command.CreateParameter();
            parameters.Add( parameter );
        }

        parameter.Direction = ParameterDirection.Input;
        parameter.ParameterName = name;
        return parameter;
    }

    private readonly struct Toolbox
    {
        internal readonly bool IsInitialized;
        internal readonly object Sync;
        internal readonly NullabilityInfoContext NullContext;
        internal readonly ParameterExpression AbstractCommand;
        internal readonly BinaryExpression CommandAssignment;
        internal readonly BinaryExpression CommandParametersAssignment;
        internal readonly BinaryExpression OriginalCountAssignment;
        internal readonly BinaryExpression IndexAssignment;
        internal readonly BinaryExpression ParameterDirectionAssignment;
        internal readonly MemberExpression ParameterNameAccess;
        internal readonly MemberExpression ParameterValueAccess;
        internal readonly BinaryExpression ParameterNullValueAssignment;
        internal readonly UnaryExpression IndexIncrement;
        internal readonly ConditionalExpression ClearExcessParametersConditional;
        internal readonly ParameterExpression[] ElementNoArray;
        internal readonly BinaryExpression ElementNoAssignment;
        internal readonly MethodCallExpression ElementNoToStringCall;
        internal readonly MethodInfo StringConcatMethod;
        internal readonly ConditionalExpression ParameterCreation;
        internal readonly ParameterExpression[] BlockParameters;

        private readonly Dictionary<Type, TypeDefinitionInfo> _typeDefinitions;

        private Toolbox(SqlDialect dialect, Type commandType, object sync)
        {
            IsInitialized = true;
            Sync = sync;
            _typeDefinitions = new Dictionary<Type, TypeDefinitionInfo>();
            NullContext = new NullabilityInfoContext();
            AbstractCommand = Expression.Parameter( typeof( IDbCommand ), "command" );
            var command = Expression.Variable( commandType, $"{dialect.Name.ToLower()}Command" );
            CommandAssignment = Expression.Assign( command, AbstractCommand.GetOrConvert( command.Type ) );

            var zero = Expression.Constant( 0 );
            var one = Expression.Constant( 1 );

            var parametersProperty = TypeHelpers.GetDbCommandParametersProperty( command.Type );
            Assume.IsNotNull( parametersProperty.DeclaringType );

            var commandParameters = Expression.Variable( parametersProperty.PropertyType, "parameters" );
            CommandParametersAssignment = Expression.Assign(
                commandParameters,
                Expression.MakeMemberAccess( command.GetOrConvert( parametersProperty.DeclaringType ), parametersProperty ) );

            var countProperty = TypeHelpers.GetDataParameterCollectionCountProperty( commandParameters.Type );
            Assume.IsNotNull( countProperty.DeclaringType );

            var originalCount = Expression.Variable( typeof( int ), "originalCount" );
            OriginalCountAssignment = Expression.Assign(
                originalCount,
                Expression.MakeMemberAccess( commandParameters.GetOrConvert( countProperty.DeclaringType ), countProperty ) );

            var index = Expression.Variable( typeof( int ), "index" );
            IndexAssignment = Expression.Assign( index, zero );

            var createParameterMethod = TypeHelpers.GetDbCommandCreateParameterMethod( command.Type );
            Assume.IsNotNull( createParameterMethod.DeclaringType );
            var parameter = Expression.Variable( createParameterMethod.ReturnType, "parameter" );

            var directionProperty = TypeHelpers.GetDataParameterDirectionProperty( parameter.Type );
            Assume.IsNotNull( directionProperty.DeclaringType );
            ParameterDirectionAssignment = Expression.Assign(
                Expression.MakeMemberAccess( parameter.GetOrConvert( directionProperty.DeclaringType ), directionProperty ),
                Expression.Constant( ParameterDirection.Input ) );

            var nameProperty = TypeHelpers.GetDataParameterNameProperty( parameter.Type );
            Assume.IsNotNull( nameProperty.DeclaringType );
            ParameterNameAccess = Expression.MakeMemberAccess( parameter.GetOrConvert( nameProperty.DeclaringType ), nameProperty );

            var valueProperty = TypeHelpers.GetDataParameterValueProperty( parameter.Type );
            Assume.IsNotNull( valueProperty.DeclaringType );
            ParameterValueAccess = Expression.MakeMemberAccess( parameter.GetOrConvert( valueProperty.DeclaringType ), valueProperty );
            ParameterNullValueAssignment = Expression.Assign( ParameterValueAccess, Expression.Constant( DBNull.Value ) );

            IndexIncrement = Expression.PreIncrementAssign( index );

            var parametersClearMethod = TypeHelpers.GetDataParameterCollectionClearMethod( commandParameters.Type );
            Assume.IsNotNull( parametersClearMethod.DeclaringType );
            var parametersClearCall = Expression.Call(
                commandParameters.GetOrConvert( parametersClearMethod.DeclaringType ),
                parametersClearMethod );

            var parametersRemoveAtMethod = TypeHelpers.GetDataParameterCollectionRemoveAtMethod( commandParameters.Type );
            Assume.IsNotNull( parametersRemoveAtMethod.DeclaringType );

            var removeExcessLoopIndex = Expression.Variable( typeof( int ), "i" );
            var removeExcessLoopIndexAssignment = Expression.Assign( removeExcessLoopIndex, Expression.Subtract( originalCount, one ) );
            var removeExcessLoopIndexTest = Expression.GreaterThanOrEqual( removeExcessLoopIndex, index );
            var removeExcessLoopIndexDecrement = Expression.PostDecrementAssign( removeExcessLoopIndex );

            var parametersClearLoopBreakLabel = Expression.Label( "ClearLoopEnd" );
            var parametersClearLoopBreak = Expression.Break( parametersClearLoopBreakLabel );
            var parametersClearLoop = Expression.Loop(
                Expression.IfThenElse(
                    removeExcessLoopIndexTest,
                    Expression.Call(
                        commandParameters.GetOrConvert( parametersRemoveAtMethod.DeclaringType ),
                        parametersRemoveAtMethod,
                        removeExcessLoopIndexDecrement ),
                    parametersClearLoopBreak ),
                parametersClearLoopBreakLabel );

            ClearExcessParametersConditional = Expression.IfThenElse(
                Expression.Equal( index, zero ),
                parametersClearCall,
                Expression.Block( new[] { removeExcessLoopIndex }, removeExcessLoopIndexAssignment, parametersClearLoop ) );

            var elementNo = Expression.Variable( typeof( int ), "elementNo" );
            ElementNoArray = new[] { elementNo };
            ElementNoAssignment = Expression.Assign( elementNo, one );

            StringConcatMethod = TypeHelpers.GetStringConcatMethod();
            var intToStringMethod = TypeHelpers.GetIntToStringMethod();
            ElementNoToStringCall = Expression.Call( Expression.PostIncrementAssign( elementNo ), intToStringMethod );

            var addParameterMethod = TypeHelpers.GetDataParameterCollectionAddMethod( commandParameters.Type, parameter.Type );
            Assume.IsNotNull( addParameterMethod.DeclaringType );
            var addParameterCall = Expression.Call(
                commandParameters.GetOrConvert( addParameterMethod.DeclaringType ),
                addParameterMethod,
                parameter.GetOrConvert( addParameterMethod.GetParameters()[0].ParameterType ) );

            var createParameterCall = Expression.Call( command.GetOrConvert( createParameterMethod.DeclaringType ), createParameterMethod );
            var parameterAssignmentFromCreateCall = Expression.Block(
                Expression.Assign( parameter, createParameterCall.GetOrConvert( parameter.Type ) ),
                addParameterCall );

            var parametersIndexer = TypeHelpers.GetDataParameterCollectionIndexer( commandParameters.Type, parameter.Type );
            Assume.IsNotNull( parametersIndexer.DeclaringType );

            var parameterCountTest = Expression.LessThan( index, originalCount );
            var parameterAssignmentFromIndexer = Expression.Assign(
                parameter,
                Expression.Property( commandParameters.GetOrConvert( parametersIndexer.DeclaringType ), parametersIndexer, index )
                    .GetOrConvert( parameter.Type ) );

            ParameterCreation = Expression.IfThenElse(
                parameterCountTest,
                parameterAssignmentFromIndexer,
                parameterAssignmentFromCreateCall );

            BlockParameters = new[] { command, commandParameters, parameter, originalCount, index };
        }

        internal ParameterExpression Parameter => BlockParameters[2];

        [Pure]
        internal static Toolbox Create(SqlDialect dialect, Type commandType, object sync)
        {
            return new Toolbox( dialect, commandType, sync );
        }

        [Pure]
        internal Expression CreateParameterAssignment(
            StatementParameterInfo parameter,
            ParameterExpression source,
            ISqlColumnTypeDefinitionProvider typeDefinitions)
        {
            var parameterSource = CreateExpression( parameter, source );

            if ( parameter.IsReducibleCollection )
            {
                return parameter.Type.IsNullable
                    ? CreateNullableCollectionParameterAssignment(
                        parameterSource,
                        parameter.Name,
                        parameter.IgnoreWhenNull,
                        typeDefinitions )
                    : CreateCollectionParameterAssignment( parameterSource, parameter.Name, parameter.IgnoreWhenNull, typeDefinitions );
            }

            return parameter.Type.IsNullable
                ? CreateNullableScalarParameterAssignment( parameterSource, parameter.Name, parameter.IgnoreWhenNull, typeDefinitions )
                : CreateScalarParameterAssignment( parameterSource, parameter.Name, typeDefinitions );
        }

        [Pure]
        private BlockExpression CreateNullableCollectionParameterAssignment(
            Expression parameterSource,
            string name,
            bool ignoreWhenNull,
            ISqlColumnTypeDefinitionProvider typeDefinitions)
        {
            var variable = Expression.Variable( parameterSource.Type, $"val_{name}" );
            var (notNullTest, value) = CreateNotNullExpressionTest( variable );

            var result = Expression.Block(
                new[] { variable },
                Expression.Assign( variable, parameterSource ),
                Expression.IfThen( notNullTest, CreateCollectionParameterAssignment( value, name, ignoreWhenNull, typeDefinitions ) ) );

            return result;
        }

        [Pure]
        private BlockExpression CreateCollectionParameterAssignment(
            Expression parameterSource,
            string name,
            bool ignoreWhenNull,
            ISqlColumnTypeDefinitionProvider typeDefinitions)
        {
            var elementName = CreateCollectionElementName( name );
            var forEachLoopCreator = parameterSource.ToForEachLoop( currentVariableName: "element" );
            var elementType = SqlStatementTypeInfo.Create( forEachLoopCreator.CurrentProperty, NullContext );

            BlockExpression forEachLoopBlock;
            if ( elementType.IsNullable )
            {
                var elementVariable = Expression.Variable( forEachLoopCreator.CurrentProperty.PropertyType, $"val_el_{name}" );
                Expression[] elementBuffer;

                if ( ignoreWhenNull )
                {
                    elementBuffer = new Expression[2];
                    AppendIgnoredNullableScalarParameterAssignmentExpressions(
                        elementBuffer.AsSpan( 1 ),
                        elementVariable,
                        elementName,
                        typeDefinitions );
                }
                else
                {
                    elementBuffer = new Expression[6];
                    AppendIncludedNullableScalarParameterAssignmentExpressions(
                        elementBuffer.AsSpan( 1 ),
                        elementVariable,
                        elementName,
                        typeDefinitions );
                }

                elementBuffer[0] = Expression.Assign( elementVariable, forEachLoopCreator.Current );
                forEachLoopBlock = Expression.Block(
                    forEachLoopCreator.CurrentAssignment,
                    Expression.Block( new[] { elementVariable }, elementBuffer ) );
            }
            else
            {
                var buffer = new Expression[7];
                buffer[0] = forEachLoopCreator.CurrentAssignment;
                AppendScalarParameterAssignmentExpressions( buffer.AsSpan( 1 ), forEachLoopCreator.Current, elementName, typeDefinitions );
                forEachLoopBlock = Expression.Block( buffer );
            }

            var forEachElementLoop = forEachLoopCreator.Create( forEachLoopBlock );
            var result = Expression.Block( ElementNoArray, ElementNoAssignment, forEachElementLoop );
            return result;
        }

        [Pure]
        private BlockExpression CreateNullableScalarParameterAssignment(
            Expression parameterSource,
            string name,
            bool ignoreWhenNull,
            ISqlColumnTypeDefinitionProvider typeDefinitions)
        {
            var nameConst = Expression.Constant( name );
            var variable = Expression.Variable( parameterSource.Type, $"val_{name}" );

            Expression[] buffer;
            if ( ignoreWhenNull )
            {
                buffer = new Expression[2];
                AppendIgnoredNullableScalarParameterAssignmentExpressions( buffer.AsSpan( 1 ), variable, nameConst, typeDefinitions );
            }
            else
            {
                buffer = new Expression[6];
                AppendIncludedNullableScalarParameterAssignmentExpressions( buffer.AsSpan( 1 ), variable, nameConst, typeDefinitions );
            }

            buffer[0] = Expression.Assign( variable, parameterSource );
            var result = Expression.Block( new[] { variable }, buffer );
            return result;
        }

        [Pure]
        private BlockExpression CreateScalarParameterAssignment(
            Expression parameterSource,
            string name,
            ISqlColumnTypeDefinitionProvider typeDefinitions)
        {
            var buffer = new Expression[6];
            AppendScalarParameterAssignmentExpressions( buffer, parameterSource, Expression.Constant( name ), typeDefinitions );
            var result = Expression.Block( buffer );
            return result;
        }

        private void AppendScalarParameterAssignmentExpressions(
            Span<Expression> buffer,
            Expression parameterSource,
            Expression name,
            ISqlColumnTypeDefinitionProvider typeDefinitions)
        {
            Assume.Equals( buffer.Length, 6 );
            var typeDef = GetOrAddTypeDefinition( parameterSource.Type, typeDefinitions );

            buffer[0] = ParameterCreation;
            buffer[1] = ParameterDirectionAssignment;
            buffer[2] = Expression.Assign( ParameterNameAccess, name );
            buffer[3] = typeDef.SetParameterInfoCall;
            buffer[4] = typeDef.CreateParameterValueAssignment( ParameterValueAccess, parameterSource );
            buffer[5] = IndexIncrement;
        }

        private void AppendIncludedNullableScalarParameterAssignmentExpressions(
            Span<Expression> buffer,
            ParameterExpression variable,
            Expression name,
            ISqlColumnTypeDefinitionProvider typeDefinitions)
        {
            Assume.Equals( buffer.Length, 5 );
            var (notNullTest, value) = CreateNotNullExpressionTest( variable );
            var typeDef = GetOrAddTypeDefinition( value.Type, typeDefinitions );

            buffer[0] = ParameterCreation;
            buffer[1] = ParameterDirectionAssignment;
            buffer[2] = Expression.Assign( ParameterNameAccess, name );
            buffer[3] = Expression.IfThenElse(
                notNullTest,
                Expression.Block( typeDef.SetParameterInfoCall, typeDef.CreateParameterValueAssignment( ParameterValueAccess, variable ) ),
                typeDef.SetNullParameterBlock );

            buffer[4] = IndexIncrement;
        }

        private void AppendIgnoredNullableScalarParameterAssignmentExpressions(
            Span<Expression> buffer,
            ParameterExpression variable,
            Expression name,
            ISqlColumnTypeDefinitionProvider typeDefinitions)
        {
            Assume.Equals( buffer.Length, 1 );
            var (notNullTest, value) = CreateNotNullExpressionTest( variable );

            var assignmentBuffer = new Expression[6];
            AppendScalarParameterAssignmentExpressions( assignmentBuffer, value, name, typeDefinitions );

            buffer[0] = Expression.IfThen( notNullTest, Expression.Block( assignmentBuffer ) );
        }

        [Pure]
        private MethodCallExpression CreateCollectionElementName(string collectionName)
        {
            return Expression.Call( StringConcatMethod, Expression.Constant( collectionName ), ElementNoToStringCall );
        }

        [Pure]
        private static Expression CreateExpression(StatementParameterInfo parameter, ParameterExpression source)
        {
            if ( parameter.Source is MemberInfo member )
                return Expression.MakeMemberAccess( source, member );

            var selector = ReinterpretCast.To<LambdaExpression>( parameter.Source );
            var selectorParameter = selector.Parameters[0];
            return selector.Body.ReplaceParameter( selectorParameter, source.GetOrConvert( selectorParameter.Type ) );
        }

        [Pure]
        private static (Expression NotNullTest, Expression Expression) CreateNotNullExpressionTest(Expression expression)
        {
            if ( ! expression.Type.IsValueType )
                return (expression.IsNotNullReference(), expression);

            var hasValue = TypeHelpers.GetNullableHasValueProperty( expression.Type );
            var value = TypeHelpers.GetNullableValueProperty( expression.Type );
            return (Expression.MakeMemberAccess( expression, hasValue ), Expression.MakeMemberAccess( expression, value ));
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private TypeDefinitionInfo GetOrAddTypeDefinition(Type type, ISqlColumnTypeDefinitionProvider typeDefinitions)
        {
            lock ( Sync )
            {
                ref var typeDef = ref CollectionsMarshal.GetValueRefOrAddDefault( _typeDefinitions, type, out var exists );
                if ( ! exists )
                    typeDef = new TypeDefinitionInfo( typeDefinitions.GetByType( type ), Parameter, ParameterNullValueAssignment );

                return typeDef;
            }
        }

        private readonly struct TypeDefinitionInfo
        {
            internal readonly ConstantExpression Definition;
            internal readonly MethodCallExpression SetParameterInfoCall;
            internal readonly BlockExpression SetNullParameterBlock;
            internal readonly MethodInfo GetParameterValueMethod;
            internal readonly Type GetParameterValueMethodValueType;
            internal readonly Expression GetParameterValueCallTarget;

            internal TypeDefinitionInfo(
                ISqlColumnTypeDefinition definition,
                ParameterExpression parameter,
                BinaryExpression parameterNullValueAssignment)
            {
                Definition = Expression.Constant( definition );
                var setParameterInfoMethod = TypeHelpers.GetColumnTypeDefinitionSetParameterInfoMethod( Definition.Type, parameter.Type );
                Assume.IsNotNull( setParameterInfoMethod.DeclaringType );

                var callTarget = Definition.GetOrConvert( setParameterInfoMethod.DeclaringType );
                var callParameter = parameter.GetOrConvert( setParameterInfoMethod.GetParameters()[0].ParameterType );

                SetParameterInfoCall = Expression.Call(
                    callTarget,
                    setParameterInfoMethod,
                    callParameter,
                    Expression.Constant( Boxed.False ) );

                SetNullParameterBlock = Expression.Block(
                    Expression.Call(
                        callTarget,
                        setParameterInfoMethod,
                        callParameter,
                        Expression.Constant( Boxed.True ) ),
                    parameterNullValueAssignment );

                GetParameterValueMethod = TypeHelpers.GetColumnTypeDefinitionToParameterValueMethod(
                    Definition.Type,
                    definition.RuntimeType );

                Assume.IsNotNull( GetParameterValueMethod.DeclaringType );
                GetParameterValueMethodValueType = GetParameterValueMethod.GetParameters()[0].ParameterType;
                GetParameterValueCallTarget = Definition.GetOrConvert( GetParameterValueMethod.DeclaringType );
            }

            [Pure]
            internal BinaryExpression CreateParameterValueAssignment(MemberExpression parameterValue, Expression value)
            {
                var getParameterValueCall = Expression.Call(
                    GetParameterValueCallTarget,
                    GetParameterValueMethod,
                    value.GetOrConvert( GetParameterValueMethodValueType ) );

                return Expression.Assign( parameterValue, getParameterValueCall );
            }
        }
    }
}

public class SqlParameterBinderFactory<TCommand> : SqlParameterBinderFactory
    where TCommand : IDbCommand
{
    protected SqlParameterBinderFactory(SqlDialect dialect, ISqlColumnTypeDefinitionProvider columnTypeDefinitions)
        : base( typeof( TCommand ), dialect, columnTypeDefinitions ) { }
}
