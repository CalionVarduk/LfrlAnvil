using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Expressions;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Statements.Compilers;

public class SqlQueryReaderFactory : ISqlQueryReaderFactory
{
    private readonly object _sync = new object();
    private Toolbox _toolbox;
    private ResultSetToolbox _resultSetToolbox;
    private MethodInfo? _createAsyncQueryReaderExpressionMethod;
    private MethodInfo? _createAsyncScalarQueryReaderExpressionMethod;

    internal SqlQueryReaderFactory(Type dataReaderType, SqlDialect dialect, ISqlColumnTypeDefinitionProvider columnTypeDefinitions)
    {
        Assume.True( dataReaderType.IsAssignableTo( typeof( IDataReader ) ) );
        DataReaderType = dataReaderType;
        Dialect = dialect;
        ColumnTypeDefinitions = columnTypeDefinitions;
        _toolbox = default;
        _resultSetToolbox = default;
        _createAsyncQueryReaderExpressionMethod = null;
        _createAsyncScalarQueryReaderExpressionMethod = null;
        SupportsAsync = DataReaderType.IsAssignableTo( typeof( DbDataReader ) );
    }

    public bool SupportsAsync { get; }
    public Type DataReaderType { get; }
    public SqlDialect Dialect { get; }
    public ISqlColumnTypeDefinitionProvider ColumnTypeDefinitions { get; }

    [Pure]
    public SqlQueryReader Create(SqlQueryReaderCreationOptions? options = null)
    {
        return new SqlQueryReader(
            Dialect,
            options?.ResultSetFieldsPersistenceMode == SqlQueryReaderResultSetFieldsPersistenceMode.PersistWithTypes
                ? ReadRowsWithFieldTypes
                : ReadRows );
    }

    [Pure]
    public SqlAsyncQueryReader CreateAsync(SqlQueryReaderCreationOptions? options = null)
    {
        return new SqlAsyncQueryReader(
            Dialect,
            options?.ResultSetFieldsPersistenceMode == SqlQueryReaderResultSetFieldsPersistenceMode.PersistWithTypes
                ? ReadRowsWithFieldTypesAsync
                : ReadRowsAsync );
    }

    [Pure]
    public SqlQueryReaderExpression CreateExpression(Type rowType, SqlQueryReaderCreationOptions? options = null)
    {
        var errors = Chain<string>.Empty;
        if ( rowType.IsAbstract )
            errors = errors.Extend( ExceptionResources.TypeCannotBeAbstract );

        if ( rowType.IsGenericTypeDefinition )
            errors = errors.Extend( ExceptionResources.TypeCannotBeOpenGeneric );

        if ( rowType.IsValueType && Nullable.GetUnderlyingType( rowType ) is not null )
            errors = errors.Extend( ExceptionResources.TypeCannotBeNullable );

        if ( errors.Count > 0 )
            throw new SqlCompilerException( Dialect, errors );

        var opt = options ?? SqlQueryReaderCreationOptions.Default;
        var body = CreateLambdaExpressionBody( rowType, in opt );

        var lambda = Expression.Lambda( body, _toolbox.AbstractReader, _toolbox.CreationOptions );
        return new SqlQueryReaderExpression( Dialect, rowType, lambda );
    }

    [Pure]
    public SqlAsyncQueryReaderExpression CreateAsyncExpression(Type rowType, SqlQueryReaderCreationOptions? options = null)
    {
        var errors = Chain<string>.Empty;
        if ( ! SupportsAsync )
            errors = errors.Extend( ExceptionResources.DataReaderDoesNotSupportAsyncQueries );

        if ( rowType.IsAbstract )
            errors = errors.Extend( ExceptionResources.TypeCannotBeAbstract );

        if ( rowType.IsGenericTypeDefinition )
            errors = errors.Extend( ExceptionResources.TypeCannotBeOpenGeneric );

        if ( rowType.IsValueType && Nullable.GetUnderlyingType( rowType ) is not null )
            errors = errors.Extend( ExceptionResources.TypeCannotBeNullable );

        if ( errors.Count > 0 )
            throw new SqlCompilerException( Dialect, errors );

        var expression = CreateAsyncQueryReaderExpression( rowType, options ?? SqlQueryReaderCreationOptions.Default );
        return new SqlAsyncQueryReaderExpression( Dialect, rowType, expression );
    }

    [Pure]
    public SqlScalarQueryReader CreateScalar()
    {
        return new SqlScalarQueryReader( Dialect, ReadScalar );
    }

    [Pure]
    public SqlAsyncScalarQueryReader CreateAsyncScalar()
    {
        return new SqlAsyncScalarQueryReader( Dialect, ReadScalarAsync );
    }

    [Pure]
    public SqlScalarQueryReaderExpression CreateScalarExpression(Type resultType, bool isNullable = false)
    {
        var errors = Chain<string>.Empty;
        if ( resultType.IsAbstract )
            errors = errors.Extend( ExceptionResources.TypeCannotBeAbstract );

        if ( resultType.IsGenericTypeDefinition )
            errors = errors.Extend( ExceptionResources.TypeCannotBeOpenGeneric );

        if ( errors.Count > 0 )
            throw new SqlCompilerException( Dialect, errors );

        var body = CreateScalarLambdaExpressionBody( resultType, isNullable );
        var lambda = Expression.Lambda( body, _toolbox.AbstractReader );
        return new SqlScalarQueryReaderExpression( Dialect, resultType, lambda );
    }

    [Pure]
    public SqlAsyncScalarQueryReaderExpression CreateAsyncScalarExpression(Type resultType, bool isNullable = false)
    {
        var errors = Chain<string>.Empty;
        if ( ! SupportsAsync )
            errors = errors.Extend( ExceptionResources.DataReaderDoesNotSupportAsyncQueries );

        if ( resultType.IsAbstract )
            errors = errors.Extend( ExceptionResources.TypeCannotBeAbstract );

        if ( resultType.IsGenericTypeDefinition )
            errors = errors.Extend( ExceptionResources.TypeCannotBeOpenGeneric );

        if ( errors.Count > 0 )
            throw new SqlCompilerException( Dialect, errors );

        var expression = CreateAsyncScalarQueryReaderExpression( resultType, isNullable );
        return new SqlAsyncScalarQueryReaderExpression( Dialect, resultType, expression );
    }

    private ISqlAsyncQueryLambdaExpression CreateAsyncQueryReaderExpression(Type rowType, SqlQueryReaderCreationOptions options)
    {
        if ( _createAsyncQueryReaderExpressionMethod is null )
        {
            var methods = typeof( SqlQueryReaderFactory ).GetMethods(
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly );

            foreach ( var m in methods )
            {
                if ( m.IsGenericMethod && m.Name == nameof( CreateAsyncQueryReaderExpression ) )
                {
                    _createAsyncQueryReaderExpressionMethod = m;
                    break;
                }
            }

            Assume.IsNotNull( _createAsyncQueryReaderExpressionMethod );
        }

        var closedAsyncQueryReaderExpressionMethod = _createAsyncQueryReaderExpressionMethod.MakeGenericMethod( DataReaderType, rowType );
        try
        {
            var result = closedAsyncQueryReaderExpressionMethod.Invoke( this, new object[] { options } );
            Assume.IsNotNull( result );
            return ReinterpretCast.To<ISqlAsyncQueryLambdaExpression>( result );
        }
        catch ( TargetInvocationException e )
        {
            e.InnerException?.Rethrow();
            throw;
        }
    }

    private SqlAsyncQueryLambdaExpression<TDataReader, TRow> CreateAsyncQueryReaderExpression<TDataReader, TRow>(
        SqlQueryReaderCreationOptions creationOptions)
        where TDataReader : DbDataReader
        where TRow : notnull
    {
        Assume.Equals( typeof( TDataReader ), DataReaderType );
        InitializeToolbox();

        var (ctor, ctorParameters) = FindRowTypeConstructor( typeof( TRow ), creationOptions.RowTypeConstructorPredicate );
        var queryFields = CreateQueryFields( typeof( TRow ), ctorParameters, in creationOptions );
        var ordinals = new AsyncOrdinalsCollection( this, queryFields.Length );
        var memberReadExpressions = CreateMemberReadExpressionsAndPopulateOrdinals(
            queryFields,
            ordinals,
            creationOptions.AlwaysTestForNull );

        var persistResultSetFields = creationOptions.ResultSetFieldsPersistenceMode != SqlQueryReaderResultSetFieldsPersistenceMode.Ignore;
        var persistResultSetFieldTypes = creationOptions.ResultSetFieldsPersistenceMode ==
            SqlQueryReaderResultSetFieldsPersistenceMode.PersistWithTypes;

        Expression<Func<TDataReader, SqlAsyncQueryReaderInitResult>> initExpression;
        if ( persistResultSetFields )
        {
            InitializeResultSetToolbox();

            initExpression = Expression.Lambda<Func<TDataReader, SqlAsyncQueryReaderInitResult>>(
                Expression.Block(
                    new[] { _resultSetToolbox.ResultSetFields },
                    _resultSetToolbox.CreateResultSetFieldsInitLoop( ordinals.GetUsedFieldNames(), persistResultSetFieldTypes ),
                    Expression.New(
                        _toolbox.AsyncReaderInitResultCtor,
                        ordinals.CreateOrdinalsArray(),
                        _resultSetToolbox.ResultSetFields ) ),
                _toolbox.Reader );
        }
        else
        {
            initExpression = Expression.Lambda<Func<TDataReader, SqlAsyncQueryReaderInitResult>>(
                Expression.New( _toolbox.AsyncReaderInitResultCtor, ordinals.CreateOrdinalsArray(), _toolbox.DefaultResultSetArray ),
                _toolbox.Reader );
        }

        var createRow = CreateRowInitExpression( ctor, ctorParameters.Length == 0, queryFields, memberReadExpressions );
        if ( persistResultSetFieldTypes )
        {
            var createRowExpression = Expression.Lambda<Func<TDataReader, int[], SqlResultSetField[], TRow>>(
                Expression.Block( _resultSetToolbox.ResultSetFieldsTypeAssignmentBlock, createRow ),
                _toolbox.Reader,
                _toolbox.AsyncOrdinals,
                _resultSetToolbox.ResultSetFields );

            return SqlAsyncQueryLambdaExpression<TDataReader, TRow>.Create( initExpression, createRowExpression );
        }
        else
        {
            var createRowExpression = Expression.Lambda<Func<TDataReader, int[], TRow>>(
                createRow,
                _toolbox.Reader,
                _toolbox.AsyncOrdinals );

            return SqlAsyncQueryLambdaExpression<TDataReader, TRow>.Create( initExpression, createRowExpression );
        }
    }

    private ISqlAsyncScalarQueryLambdaExpression CreateAsyncScalarQueryReaderExpression(Type resultType, bool isNullable)
    {
        if ( _createAsyncScalarQueryReaderExpressionMethod is null )
        {
            var methods = typeof( SqlQueryReaderFactory ).GetMethods(
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly );

            foreach ( var m in methods )
            {
                if ( m.IsGenericMethod && m.Name == nameof( CreateAsyncScalarQueryReaderExpression ) )
                {
                    _createAsyncScalarQueryReaderExpressionMethod = m;
                    break;
                }
            }

            Assume.IsNotNull( _createAsyncScalarQueryReaderExpressionMethod );
        }

        var closedAsyncScalarQueryReaderExpressionMethod = _createAsyncScalarQueryReaderExpressionMethod
            .MakeGenericMethod( DataReaderType, resultType );

        try
        {
            var result = closedAsyncScalarQueryReaderExpressionMethod.Invoke( this, new object[] { isNullable } );
            Assume.IsNotNull( result );
            return ReinterpretCast.To<ISqlAsyncScalarQueryLambdaExpression>( result );
        }
        catch ( TargetInvocationException e )
        {
            e.InnerException?.Rethrow();
            throw;
        }
    }

    private SqlAsyncScalarQueryLambdaExpression<TDataReader, T> CreateAsyncScalarQueryReaderExpression<TDataReader, T>(bool isNullable)
        where TDataReader : DbDataReader
    {
        Assume.Equals( typeof( TDataReader ), DataReaderType );
        InitializeToolbox();

        var underlyingType = typeof( T ).IsValueType ? Nullable.GetUnderlyingType( typeof( T ) ) ?? typeof( T ) : typeof( T );
        if ( typeof( T ) != underlyingType )
            isNullable = true;

        var typeDefinition = ColumnTypeDefinitions.GetByType( underlyingType );
        var scalarToolbox = new ScalarToolbox( typeof( T ) );
        var valueReadCall = scalarToolbox.CreateValueReadCall( typeDefinition, isNullable, in _toolbox );
        var body = scalarToolbox.CreateResultCtorCall( valueReadCall );

        var readRowExpression = Expression.Lambda<Func<TDataReader, SqlScalarQueryResult<T>>>( body, _toolbox.Reader );
        return SqlAsyncScalarQueryLambdaExpression<TDataReader, T>.Create( readRowExpression );
    }

    [Pure]
    private Expression CreateLambdaExpressionBody(Type rowType, in SqlQueryReaderCreationOptions creationOptions)
    {
        InitializeToolbox();

        var (ctor, ctorParameters) = FindRowTypeConstructor( rowType, creationOptions.RowTypeConstructorPredicate );
        var queryFields = CreateQueryFields( rowType, ctorParameters, in creationOptions );
        var ordinals = new SyncOrdinalsCollection( queryFields.Length );
        var memberReadExpressions = CreateMemberReadExpressionsAndPopulateOrdinals(
            queryFields,
            ordinals,
            creationOptions.AlwaysTestForNull );

        var persistResultSetFields = creationOptions.ResultSetFieldsPersistenceMode != SqlQueryReaderResultSetFieldsPersistenceMode.Ignore;
        var persistResultSetFieldTypes = creationOptions.ResultSetFieldsPersistenceMode ==
            SqlQueryReaderResultSetFieldsPersistenceMode.PersistWithTypes;

        var rowTypeToolbox = new RowTypeToolbox( rowType, in _toolbox );
        var readVariables = new ParameterExpression[ordinals.Source.Count + (persistResultSetFields ? 1 : 0) + 1];
        var readBlock = new Expression[readVariables.Length + (persistResultSetFieldTypes ? 1 : 0) + 4];
        readBlock[0] = rowTypeToolbox.RowsAssignment;

        if ( persistResultSetFields )
        {
            InitializeResultSetToolbox();
            readVariables[^2] = _resultSetToolbox.ResultSetFields;
        }

        readVariables[^1] = rowTypeToolbox.Rows;

        var index = 0;
        foreach ( var (name, ordinal) in ordinals.Source )
        {
            readVariables[index++] = ordinal;
            var getOrdinalCall = _toolbox.CreateGetOrdinalCall( name );
            readBlock[index] = Expression.Assign( ordinal, getOrdinalCall );
        }

        if ( persistResultSetFields )
            readBlock[++index] = _resultSetToolbox.CreateResultSetFieldsInitLoop(
                ordinals.GetUsedFieldNames(),
                persistResultSetFieldTypes );

        var addRow = rowTypeToolbox.CreateAddRow(
            CreateRowInitExpression( ctor, ctorParameters.Length == 0, queryFields, memberReadExpressions ) );

        readBlock[++index] = _toolbox.ReadRowLabel;
        readBlock[++index] = addRow;

        if ( persistResultSetFieldTypes )
            readBlock[++index] = _resultSetToolbox.ResultSetFieldsTypeAssignmentBlock;

        readBlock[++index] = _toolbox.GotoReadRowLabelTest;
        readBlock[++index] = Expression.Assign(
            rowTypeToolbox.QueryResult,
            Expression.New(
                rowTypeToolbox.QueryResultCtor,
                persistResultSetFields ? _resultSetToolbox.ResultSetFields : _toolbox.DefaultResultSetArray,
                rowTypeToolbox.Rows ) );

        var block = new Expression[]
        {
            _toolbox.ReaderAssignment,
            Expression.IfThen( _toolbox.ReadTest, Expression.Block( readVariables, readBlock ) ),
            rowTypeToolbox.QueryResult
        };

        return Expression.Block( new[] { _toolbox.Reader, rowTypeToolbox.QueryResult }, block );
    }

    [Pure]
    private Expression CreateScalarLambdaExpressionBody(Type resultType, bool isNullable)
    {
        InitializeToolbox();

        var underlyingType = resultType.IsValueType ? Nullable.GetUnderlyingType( resultType ) ?? resultType : resultType;
        if ( resultType != underlyingType )
            isNullable = true;

        var typeDefinition = ColumnTypeDefinitions.GetByType( underlyingType );
        var scalarToolbox = new ScalarToolbox( resultType );
        var valueReadCall = scalarToolbox.CreateValueReadCall( typeDefinition, isNullable, in _toolbox );

        var block = new Expression[]
        {
            _toolbox.ReaderAssignment,
            Expression.Condition(
                _toolbox.ReadTest,
                scalarToolbox.CreateResultCtorCall( valueReadCall ),
                scalarToolbox.CreateEmptyResultAccess() )
        };

        return Expression.Block( new[] { _toolbox.Reader }, block );
    }

    [Pure]
    private static Expression CreateRowInitExpression(
        ConstructorInfo ctor,
        bool isCtorParameterless,
        QueryFieldInfo[] queryFields,
        Expression[] memberReadExpressions)
    {
        if ( ! isCtorParameterless )
            return Expression.New( ctor, memberReadExpressions );

        var memberBindings = new MemberBinding[queryFields.Length];
        for ( var i = 0; i < queryFields.Length; ++i )
            memberBindings[i] = Expression.Bind( ReinterpretCast.To<MemberInfo>( queryFields[i].Target ), memberReadExpressions[i] );

        return Expression.MemberInit( Expression.New( ctor ), memberBindings );
    }

    [Pure]
    private (ConstructorInfo Ctor, ParameterInfo[] Parameters) FindRowTypeConstructor(
        Type rowType,
        Func<ConstructorInfo, bool>? predicate)
    {
        ConstructorInfo? current = null;
        var currentParameters = Array.Empty<ParameterInfo>();

        var constructors = rowType.GetConstructors( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
        foreach ( var ctor in constructors )
        {
            var parameters = ctor.GetParameters();
            if ( parameters.Any( static p => p.Name is null ) || (predicate is not null && ! predicate( ctor )) )
                continue;

            if ( current is not null && parameters.Length <= currentParameters.Length )
                continue;

            current = ctor;
            currentParameters = parameters;
        }

        if ( current is null )
            throw new SqlCompilerException( Dialect, ExceptionResources.RowTypeDoesNotHaveValidCtor );

        return (current, currentParameters);
    }

    [Pure]
    private QueryFieldInfo[] CreateQueryFields(
        Type rowType,
        ReadOnlySpan<ParameterInfo> ctorParameters,
        in SqlQueryReaderCreationOptions creationOptions)
    {
        QueryFieldInfo[] result;
        var memberConfigurationsByName = creationOptions.CreateMemberConfigurationByNameLookup( DataReaderType );

        if ( ctorParameters.Length > 0 )
        {
            result = new QueryFieldInfo[ctorParameters.Length];
            for ( var i = 0; i < ctorParameters.Length; ++i )
                result[i] = CreateQueryField( ctorParameters[i], memberConfigurationsByName );
        }
        else
        {
            var fields = rowType.GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
            var properties = rowType.GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );

            var memberCount = fields.Length + properties.Length;
            if ( memberCount == 0 )
                throw new SqlCompilerException( Dialect, ExceptionResources.RowTypeDoesNotHaveAnyValidMembers );

            var i = 0;
            var members = new (MemberInfo Member, string Name)[memberCount];
            var predicate = creationOptions.RowTypeMemberPredicate;

            foreach ( var f in fields )
            {
                if ( f.GetBackedProperty() is null && (predicate is null || predicate( f )) )
                    members[i++] = (f, f.Name);
            }

            foreach ( var p in properties )
            {
                var setMethod = p.GetSetMethod( nonPublic: true );
                if ( setMethod is not null )
                {
                    if ( setMethod.GetParameters().Length == 1 && (predicate is null || predicate( p )) )
                        members[i++] = (p, p.Name);

                    continue;
                }

                var backingField = p.GetBackingField();
                if ( backingField is not null && (predicate is null || predicate( p )) )
                    members[i++] = (backingField, p.Name);
            }

            if ( i == 0 )
                throw new SqlCompilerException( Dialect, ExceptionResources.RowTypeDoesNotHaveAnyValidMembers );

            result = new QueryFieldInfo[i];
            for ( i = 0; i < result.Length; ++i )
            {
                var (member, name) = members[i];
                result[i] = CreateQueryField( member, name, memberConfigurationsByName );
            }
        }

        return result;
    }

    [Pure]
    private Expression[] CreateMemberReadExpressionsAndPopulateOrdinals(
        ReadOnlySpan<QueryFieldInfo> dataFields,
        IOrdinalsCollection ordinals,
        bool alwaysTestForNull)
    {
        var readerCallsToolbox = new ReaderCallsToolbox( this, ordinals, dataFields.Length );
        for ( var i = 0; i < dataFields.Length; ++i )
        {
            var field = dataFields[i];
            if ( field.IsIgnored )
            {
                readerCallsToolbox.SetReadExpression( i, Expression.Default( field.Type.ActualType ) );
                continue;
            }

            var mapping = field.Mapping;
            if ( field.HasCustomMapping )
            {
                var customRead = readerCallsToolbox.ApplyCustomMapping( mapping, field.Type.ActualType );
                readerCallsToolbox.SetReadExpression( i, customRead );
                continue;
            }

            var ordinal = ordinals.GetOrAddOrdinal( field.Name );
            Assume.True( mapping.Body.Type.IsAssignableTo( field.Type.UnderlyingType ) );
            var columnRead = readerCallsToolbox.ApplyTypeDefMapping( mapping, ordinal, field.Type.ActualType );

            if ( alwaysTestForNull || field.Type.IsNullable )
                columnRead = _toolbox.CreateNullableRead( ordinal, Expression.Default( field.Type.ActualType ), columnRead );

            readerCallsToolbox.SetReadExpression( i, columnRead );
        }

        return readerCallsToolbox.GetMemberExpressions();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private QueryFieldInfo CreateQueryField(ParameterInfo parameter, Dictionary<string, SqlQueryMemberConfiguration>? memberConfigurations)
    {
        Assume.IsNotNull( parameter.Name );
        var type = _toolbox.NullContext.GetTypeNullability( parameter );
        return CreateQueryField( parameter.Name, parameter, type, memberConfigurations );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private QueryFieldInfo CreateQueryField(
        MemberInfo member,
        string name,
        Dictionary<string, SqlQueryMemberConfiguration>? memberConfigurations)
    {
        var type = member.MemberType == MemberTypes.Field
            ? _toolbox.NullContext.GetTypeNullability( ReinterpretCast.To<FieldInfo>( member ) )
            : _toolbox.NullContext.GetTypeNullability( ReinterpretCast.To<PropertyInfo>( member ) );

        return CreateQueryField( name, member, type, memberConfigurations );
    }

    [Pure]
    private QueryFieldInfo CreateQueryField(
        string name,
        object target,
        TypeNullability type,
        Dictionary<string, SqlQueryMemberConfiguration>? memberConfigurations)
    {
        if ( memberConfigurations is null || ! memberConfigurations.TryGetValue( name, out var cfg ) )
        {
            var typeDef = ColumnTypeDefinitions.GetByType( type.UnderlyingType );
            return new QueryFieldInfo( name, target, type, HasCustomMapping: false, typeDef.OutputMapping );
        }

        var customMappingMemberType = cfg.CustomMappingMemberType;
        if ( customMappingMemberType is not null && customMappingMemberType.IsAssignableTo( type.ActualType ) )
            return new QueryFieldInfo( name, target, type, HasCustomMapping: true, cfg.CustomMapping );

        if ( ! cfg.IsIgnored )
        {
            var typeDef = ColumnTypeDefinitions.GetByType( type.UnderlyingType );
            return new QueryFieldInfo( cfg.SourceFieldName ?? name, target, type, HasCustomMapping: false, typeDef.OutputMapping );
        }

        return new QueryFieldInfo( name, target, type, HasCustomMapping: false, Mapping: null );
    }

    [Pure]
    private static SqlResultSetField[] CreateResultSetFields(IDataRecord record, bool includeTypeNames)
    {
        var result = new SqlResultSetField[record.FieldCount];
        for ( var i = 0; i < result.Length; ++i )
            result[i] = new SqlResultSetField( i, record.GetName( i ), isUsed: true, includeTypeNames );

        return result;
    }

    [Pure]
    private static List<object?> CreateCellsBuffer(SqlQueryReaderOptions options, int fieldCount)
    {
        return options.InitialBufferCapacity is not null
            ? new List<object?>( capacity: options.InitialBufferCapacity.Value * fieldCount )
            : new List<object?>();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void InitializeToolbox()
    {
        lock ( _sync )
        {
            if ( ! _toolbox.IsInitialized )
                _toolbox = Toolbox.Create( DataReaderType, Dialect );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void InitializeResultSetToolbox()
    {
        lock ( _sync )
        {
            if ( ! _resultSetToolbox.IsInitialized )
                _resultSetToolbox = ResultSetToolbox.Create( in _toolbox );
        }
    }

    [Pure]
    private static SqlQueryReaderResult ReadRows(IDataReader reader, SqlQueryReaderOptions options)
    {
        if ( ! reader.Read() )
            return SqlQueryReaderResult.Empty;

        var fields = CreateResultSetFields( reader, includeTypeNames: false );
        var cells = CreateCellsBuffer( options, fields.Length );

        do
        {
            for ( var i = 0; i < fields.Length; ++i )
            {
                var value = reader.GetValue( i );
                cells.Add( ReferenceEquals( value, DBNull.Value ) ? null : value );
            }
        }
        while ( reader.Read() );

        return new SqlQueryReaderResult( fields, cells );
    }

    [Pure]
    private static SqlQueryReaderResult ReadRowsWithFieldTypes(IDataReader reader, SqlQueryReaderOptions options)
    {
        if ( ! reader.Read() )
            return SqlQueryReaderResult.Empty;

        var fields = CreateResultSetFields( reader, includeTypeNames: true );
        var cells = CreateCellsBuffer( options, fields.Length );

        do
        {
            for ( var i = 0; i < fields.Length; ++i )
            {
                var isDbNull = reader.IsDBNull( i );
                cells.Add( isDbNull ? null : reader.GetValue( i ) );
                fields[i].TryAddTypeName( isDbNull ? "NULL" : reader.GetDataTypeName( i ) );
            }
        }
        while ( reader.Read() );

        return new SqlQueryReaderResult( fields, cells );
    }

    [Pure]
    private static async ValueTask<SqlQueryReaderResult> ReadRowsAsync(
        IDataReader reader,
        SqlQueryReaderOptions options,
        CancellationToken cancellationToken)
    {
        var dbReader = (DbDataReader)reader;
        if ( ! await dbReader.ReadAsync( cancellationToken ).ConfigureAwait( false ) )
            return SqlQueryReaderResult.Empty;

        var fields = CreateResultSetFields( dbReader, includeTypeNames: false );
        var cells = CreateCellsBuffer( options, fields.Length );

        do
        {
            for ( var i = 0; i < fields.Length; ++i )
            {
                var value = dbReader.GetValue( i );
                cells.Add( ReferenceEquals( value, DBNull.Value ) ? null : value );
            }
        }
        while ( await dbReader.ReadAsync( cancellationToken ).ConfigureAwait( false ) );

        return new SqlQueryReaderResult( fields, cells );
    }

    [Pure]
    private static async ValueTask<SqlQueryReaderResult> ReadRowsWithFieldTypesAsync(
        IDataReader reader,
        SqlQueryReaderOptions options,
        CancellationToken cancellationToken)
    {
        var dbReader = (DbDataReader)reader;
        if ( ! await dbReader.ReadAsync( cancellationToken ).ConfigureAwait( false ) )
            return SqlQueryReaderResult.Empty;

        var fields = CreateResultSetFields( dbReader, includeTypeNames: true );
        var cells = CreateCellsBuffer( options, fields.Length );

        do
        {
            for ( var i = 0; i < fields.Length; ++i )
            {
                var isDbNull = dbReader.IsDBNull( i );
                cells.Add( isDbNull ? null : dbReader.GetValue( i ) );
                fields[i].TryAddTypeName( isDbNull ? "NULL" : dbReader.GetDataTypeName( i ) );
            }
        }
        while ( await dbReader.ReadAsync( cancellationToken ).ConfigureAwait( false ) );

        return new SqlQueryReaderResult( fields, cells );
    }

    [Pure]
    private static SqlScalarQueryResult ReadScalar(IDataReader reader)
    {
        if ( ! reader.Read() )
            return SqlScalarQueryResult.Empty;

        var value = reader.GetValue( 0 );
        return new SqlScalarQueryResult( ReferenceEquals( value, DBNull.Value ) ? null : value );
    }

    [Pure]
    private static async ValueTask<SqlScalarQueryResult> ReadScalarAsync(IDataReader reader, CancellationToken cancellationToken)
    {
        var dbReader = (DbDataReader)reader;
        if ( ! await dbReader.ReadAsync( cancellationToken ).ConfigureAwait( false ) )
            return SqlScalarQueryResult.Empty;

        var value = dbReader.GetValue( 0 );
        return new SqlScalarQueryResult( ReferenceEquals( value, DBNull.Value ) ? null : value );
    }

    private interface IOrdinalsCollection
    {
        Expression GetOrAddOrdinal(string name);
    }

    private sealed class SyncOrdinalsCollection : IOrdinalsCollection
    {
        internal readonly Dictionary<string, ParameterExpression> Source;

        internal SyncOrdinalsCollection(int count)
        {
            Source = new Dictionary<string, ParameterExpression>( capacity: count, comparer: SqlHelpers.NameComparer );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ParameterExpression GetOrAddOrdinal(string name)
        {
            ref var result = ref CollectionsMarshal.GetValueRefOrAddDefault( Source, name, out var exists )!;
            if ( ! exists )
                result = Expression.Variable( typeof( int ), $"ord_{name}" );

            return result;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public HashSet<string> GetUsedFieldNames()
        {
            return new HashSet<string>( Source.Keys, Source.Comparer );
        }

        Expression IOrdinalsCollection.GetOrAddOrdinal(string name)
        {
            return GetOrAddOrdinal( name );
        }
    }

    private sealed class AsyncOrdinalsCollection : IOrdinalsCollection
    {
        private readonly SqlQueryReaderFactory _factory;
        private readonly Dictionary<string, Expression> _source;
        private readonly List<Expression> _getOrdinalCalls;

        internal AsyncOrdinalsCollection(SqlQueryReaderFactory factory, int count)
        {
            _factory = factory;
            _source = new Dictionary<string, Expression>( capacity: count, comparer: SqlHelpers.NameComparer );
            _getOrdinalCalls = new List<Expression>( capacity: count );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Expression GetOrAddOrdinal(string name)
        {
            ref var result = ref CollectionsMarshal.GetValueRefOrAddDefault( _source, name, out var exists )!;
            if ( ! exists )
            {
                result = Expression.ArrayAccess( _factory._toolbox.AsyncOrdinals, Expression.Constant( _getOrdinalCalls.Count ) );
                var getOrdinalCall = _factory._toolbox.CreateGetOrdinalCall( name );
                _getOrdinalCalls.Add( getOrdinalCall );
            }

            return result;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public HashSet<string> GetUsedFieldNames()
        {
            return new HashSet<string>( _source.Keys, _source.Comparer );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public NewArrayExpression CreateOrdinalsArray()
        {
            return Expression.NewArrayInit( typeof( int ), _getOrdinalCalls );
        }
    }

    private readonly record struct QueryFieldInfo(
        string Name,
        object Target,
        TypeNullability Type,
        bool HasCustomMapping,
        LambdaExpression? Mapping)
    {
        [MemberNotNullWhen( false, nameof( Mapping ) )]
        internal bool IsIgnored => Mapping is null;
    }

    private readonly struct ResultSetToolbox
    {
        internal readonly bool IsInitialized;
        internal readonly ParameterExpression ResultSetFields;
        internal readonly ConstructorInfo ResultSetFieldCtor;
        internal readonly MethodInfo StringHashSetContainsMethod;
        internal readonly ParameterExpression[] FieldLoopIndexArray;
        internal readonly ParameterExpression[] FieldNameArray;
        internal readonly BinaryExpression FieldLoopTest;
        internal readonly BinaryExpression FieldNameAssignment;
        internal readonly IndexExpression ResultSetFieldsIndexerAccess;
        internal readonly UnaryExpression FieldLoopIndexIncrement;
        internal readonly BinaryExpression ResultSetFieldsAssignment;
        internal readonly BinaryExpression FieldLoopIndexZeroAssignment;
        internal readonly LabelTarget ResultSetInitLoopEndTarget;
        internal readonly GotoExpression ResultSetInitLoopBreak;
        internal readonly ConstantExpression TrueConst;
        internal readonly ConstantExpression FalseConst;
        internal readonly BlockExpression ResultSetFieldsTypeAssignmentBlock;

        private ResultSetToolbox(in Toolbox toolbox)
        {
            IsInitialized = true;
            ResultSetFields = Expression.Variable( toolbox.ResultSetFieldArrayType, "resultSetFields" );
            ResultSetFieldCtor = TypeHelpers.GetResultSetFieldCtor( toolbox.ResultSetFieldType );
            StringHashSetContainsMethod = TypeHelpers.GetStringHashSetContainsMethod();

            var fieldLoopIndex = Expression.Variable( typeof( int ), "i" );
            FieldLoopIndexArray = new[] { fieldLoopIndex };
            var fieldName = Expression.Variable( typeof( string ), "fieldName" );
            FieldNameArray = new[] { fieldName };
            ResultSetFieldsIndexerAccess = Expression.ArrayAccess( ResultSetFields, fieldLoopIndex );
            FieldLoopIndexIncrement = Expression.PreIncrementAssign( fieldLoopIndex );

            var fieldArrayLengthProperty = TypeHelpers.GetArrayLengthProperty( toolbox.ResultSetFieldArrayType );
            FieldLoopTest = Expression.LessThan( fieldLoopIndex, Expression.MakeMemberAccess( ResultSetFields, fieldArrayLengthProperty ) );

            var getNameMethod = TypeHelpers.GetDataRecordGetNameMethod( toolbox.Reader.Type );
            Assume.IsNotNull( getNameMethod.DeclaringType );

            FieldNameAssignment = Expression.Assign(
                fieldName,
                Expression.Call( toolbox.Reader.GetOrConvert( getNameMethod.DeclaringType ), getNameMethod, fieldLoopIndex ) );

            var fieldCountProperty = TypeHelpers.GetDataRecordFieldCountProperty( toolbox.Reader.Type );
            Assume.IsNotNull( fieldCountProperty.DeclaringType );

            var resultSetFieldArrayCtor = TypeHelpers.GetArrayCtor( toolbox.ResultSetFieldArrayType );
            ResultSetFieldsAssignment = Expression.Assign(
                ResultSetFields,
                Expression.New(
                    resultSetFieldArrayCtor,
                    Expression.MakeMemberAccess( toolbox.Reader.GetOrConvert( fieldCountProperty.DeclaringType ), fieldCountProperty ) ) );

            FieldLoopIndexZeroAssignment = Expression.Assign( fieldLoopIndex, Expression.Constant( 0 ) );
            ResultSetInitLoopEndTarget = Expression.Label( "ResultSetInitLoopEnd" );
            ResultSetInitLoopBreak = Expression.Break( ResultSetInitLoopEndTarget );
            TrueConst = Expression.Constant( Boxed.True );
            FalseConst = Expression.Constant( Boxed.False );

            var resultSetFieldTryAddTypeNameMethod = TypeHelpers.GetResultSetFieldTryAddTypeNameMethod( toolbox.ResultSetFieldType );
            var getDataTypeNameMethod = TypeHelpers.GetDataRecordGetDataTypeNameMethod( toolbox.Reader.Type );
            Assume.IsNotNull( getDataTypeNameMethod.DeclaringType );

            var resultSetTypeLoopEndTarget = Expression.Label( "ResultSetTypeLoopEnd" );
            var resultSetTypeLoopBreak = Expression.Break( resultSetTypeLoopEndTarget );

            var resultSetFieldsTypeLoop = Expression.Loop(
                Expression.IfThenElse(
                    FieldLoopTest,
                    Expression.Block(
                        Expression.Call(
                            ResultSetFieldsIndexerAccess,
                            resultSetFieldTryAddTypeNameMethod,
                            Expression.Condition(
                                toolbox.CreateIsDbNullCall( fieldLoopIndex ),
                                Expression.Constant( "NULL" ),
                                Expression.Call(
                                    toolbox.Reader.GetOrConvert( getDataTypeNameMethod.DeclaringType ),
                                    getDataTypeNameMethod,
                                    fieldLoopIndex ) ) ),
                        FieldLoopIndexIncrement ),
                    resultSetTypeLoopBreak ),
                resultSetTypeLoopEndTarget );

            ResultSetFieldsTypeAssignmentBlock = Expression.Block(
                FieldLoopIndexArray,
                FieldLoopIndexZeroAssignment,
                resultSetFieldsTypeLoop );
        }

        [Pure]
        internal static ResultSetToolbox Create(in Toolbox toolbox)
        {
            Assume.True( toolbox.IsInitialized );
            return new ResultSetToolbox( in toolbox );
        }

        [Pure]
        internal BlockExpression CreateResultSetFieldsInitLoop(HashSet<string> usedColumnNames, bool includeTypes)
        {
            Assume.True( IsInitialized );

            var fieldName = FieldNameArray[0];
            var loop = Expression.Loop(
                Expression.IfThenElse(
                    FieldLoopTest,
                    Expression.Block(
                        FieldNameArray,
                        FieldNameAssignment,
                        Expression.Assign(
                            ResultSetFieldsIndexerAccess,
                            Expression.New(
                                ResultSetFieldCtor,
                                FieldLoopIndexArray[0],
                                fieldName,
                                Expression.Call( Expression.Constant( usedColumnNames ), StringHashSetContainsMethod, fieldName ),
                                includeTypes ? TrueConst : FalseConst ) ),
                        FieldLoopIndexIncrement ),
                    ResultSetInitLoopBreak ),
                ResultSetInitLoopEndTarget );

            var result = Expression.Block( FieldLoopIndexArray, ResultSetFieldsAssignment, FieldLoopIndexZeroAssignment, loop );
            return result;
        }
    }

    private readonly struct RowTypeToolbox
    {
        internal readonly ParameterExpression QueryResult;
        internal readonly ParameterExpression Rows;
        internal readonly BinaryExpression RowsAssignment;
        internal readonly MethodInfo RowsAddMethod;
        internal readonly ConstructorInfo QueryResultCtor;

        internal RowTypeToolbox(Type rowType, in Toolbox toolbox)
        {
            Assume.True( toolbox.IsInitialized );

            var rowListType = typeof( List<> ).MakeGenericType( rowType );
            var parameterlessRowListCtor = TypeHelpers.GetListDefaultCtor( rowListType );
            var rowListCtorWithCapacity = TypeHelpers.GetListCtorWithCapacity( rowListType );
            RowsAddMethod = TypeHelpers.GetListAddMethod( rowListType, rowType );

            var queryResultType = typeof( SqlQueryReaderResult<> ).MakeGenericType( rowType );
            QueryResult = Expression.Variable( queryResultType, "result" );
            QueryResultCtor = TypeHelpers.GetQueryReaderResultCtor( queryResultType, toolbox.ResultSetFieldArrayType, rowListType );

            Rows = Expression.Variable( rowListType, "rows" );
            RowsAssignment = Expression.Assign(
                Rows,
                Expression.Condition(
                    toolbox.InitBufferCapacityNullTest,
                    Expression.New( rowListCtorWithCapacity, toolbox.InitBufferCapacityValueAccess ),
                    Expression.New( parameterlessRowListCtor ) ) );
        }

        [Pure]
        internal MethodCallExpression CreateAddRow(Expression row)
        {
            return Expression.Call( Rows, RowsAddMethod, row );
        }
    }

    private readonly struct ScalarToolbox
    {
        internal readonly ConstructorInfo ResultCtor;
        internal readonly DefaultExpression DefaultResult;

        internal ScalarToolbox(Type resultType)
        {
            ResultCtor = TypeHelpers.GetScalarQueryResultCtor( resultType );
            DefaultResult = Expression.Default( resultType );
        }

        [Pure]
        internal MemberExpression CreateEmptyResultAccess()
        {
            Assume.IsNotNull( ResultCtor.DeclaringType );
            var field = TypeHelpers.GetScalarQueryResultEmptyField( ResultCtor.DeclaringType );
            return Expression.MakeMemberAccess( null, field );
        }

        [Pure]
        internal NewExpression CreateResultCtorCall(Expression value)
        {
            return Expression.New( ResultCtor, value );
        }

        [Pure]
        internal Expression CreateValueReadCall(ISqlColumnTypeDefinition typeDefinition, bool isNullable, in Toolbox toolbox)
        {
            var mapping = typeDefinition.OutputMapping;
            var readerParameter = mapping.Parameters[0];

            var result = mapping.Body.ReplaceParameters(
                    new[] { readerParameter, mapping.Parameters[1] },
                    new[] { toolbox.Reader.GetOrConvert( readerParameter.Type ), toolbox.IntZero } )
                .GetOrConvert( DefaultResult.Type );

            if ( isNullable )
                result = Expression.Condition( toolbox.ReaderIsDbNullCallAtZero, DefaultResult, result );

            return result;
        }
    }

    private readonly struct Toolbox
    {
        internal readonly bool IsInitialized;
        internal readonly NullabilityInfoContext NullContext;
        internal readonly ParameterExpression AbstractReader;
        internal readonly ParameterExpression CreationOptions;
        internal readonly ParameterExpression Reader;
        internal readonly BinaryExpression ReaderAssignment;
        internal readonly MethodCallExpression ReadTest;
        internal readonly Expression ReaderForGetOrdinal;
        internal readonly MethodInfo ReaderGetOrdinalMethod;
        internal readonly Expression ReaderForIsDbNull;
        internal readonly MethodInfo ReaderIsDbNullMethod;
        internal readonly MemberExpression InitBufferCapacityNullTest;
        internal readonly MemberExpression InitBufferCapacityValueAccess;
        internal readonly LabelExpression ReadRowLabel;
        internal readonly ConditionalExpression GotoReadRowLabelTest;
        internal readonly Type ResultSetFieldType;
        internal readonly Type ResultSetFieldArrayType;
        internal readonly DefaultExpression DefaultResultSetArray;
        internal readonly ParameterExpression AsyncOrdinals;
        internal readonly ConstructorInfo AsyncReaderInitResultCtor;
        internal readonly ConstantExpression IntZero;
        internal readonly MethodCallExpression ReaderIsDbNullCallAtZero;

        private Toolbox(Type dataReaderType, SqlDialect dialect)
        {
            IsInitialized = true;
            NullContext = new NullabilityInfoContext();
            AbstractReader = Expression.Parameter( typeof( IDataReader ), "reader" );
            CreationOptions = Expression.Parameter( typeof( SqlQueryReaderOptions ), "options" );
            Reader = Expression.Variable( dataReaderType, $"{dialect.Name.ToLower()}Reader" );
            ReaderAssignment = Expression.Assign( Reader, AbstractReader.GetOrConvert( Reader.Type ) );

            var readMethod = TypeHelpers.GetDataReaderReadMethod( dataReaderType );
            Assume.IsNotNull( readMethod.DeclaringType );

            ReadTest = Expression.Call( Reader.GetOrConvert( readMethod.DeclaringType ), readMethod );

            ReaderGetOrdinalMethod = TypeHelpers.GetDataRecordGetOrdinalMethod( dataReaderType );
            Assume.IsNotNull( ReaderGetOrdinalMethod.DeclaringType );
            ReaderForGetOrdinal = Reader.GetOrConvert( ReaderGetOrdinalMethod.DeclaringType );

            ReaderIsDbNullMethod = TypeHelpers.GetDataRecordIsDbNullMethod( dataReaderType );
            Assume.IsNotNull( ReaderIsDbNullMethod.DeclaringType );
            ReaderForIsDbNull = Reader.GetOrConvert( ReaderIsDbNullMethod.DeclaringType );

            var initBufferCapacityProperty = TypeHelpers.GetQueryReaderOptionsInitialBufferCapacityProperty();
            var initBufferCapacityAccess = Expression.MakeMemberAccess( CreationOptions, initBufferCapacityProperty );

            var nullableHasValueProperty = TypeHelpers.GetNullableHasValueProperty( typeof( int? ) );
            var nullableValueProperty = TypeHelpers.GetNullableValueProperty( typeof( int? ) );
            InitBufferCapacityNullTest = Expression.MakeMemberAccess( initBufferCapacityAccess, nullableHasValueProperty );
            InitBufferCapacityValueAccess = Expression.MakeMemberAccess( initBufferCapacityAccess, nullableValueProperty );

            var readRowLabelTarget = Expression.Label( "ReadRow" );
            var gotoReadRowLabel = Expression.Goto( readRowLabelTarget );
            ReadRowLabel = Expression.Label( readRowLabelTarget );
            GotoReadRowLabelTest = Expression.IfThen( ReadTest, gotoReadRowLabel );

            ResultSetFieldType = typeof( SqlResultSetField );
            ResultSetFieldArrayType = ResultSetFieldType.MakeArrayType();
            DefaultResultSetArray = Expression.Default( ResultSetFieldArrayType );

            AsyncOrdinals = Expression.Variable( typeof( int[] ), "ordinals" );
            AsyncReaderInitResultCtor = TypeHelpers.GetAsyncReaderInitResultCtor();

            IntZero = Expression.Constant( 0 );
            ReaderIsDbNullCallAtZero = CreateIsDbNullCall( IntZero );
        }

        [Pure]
        internal static Toolbox Create(Type dataReaderType, SqlDialect dialect)
        {
            return new Toolbox( dataReaderType, dialect );
        }

        [Pure]
        internal MethodCallExpression CreateGetOrdinalCall(string name)
        {
            return Expression.Call( ReaderForGetOrdinal, ReaderGetOrdinalMethod, Expression.Constant( name ) );
        }

        [Pure]
        internal MethodCallExpression CreateIsDbNullCall(Expression ordinal)
        {
            return Expression.Call( ReaderForIsDbNull, ReaderIsDbNullMethod, ordinal );
        }

        [Pure]
        internal ConditionalExpression CreateNullableRead(Expression ordinal, Expression ifNull, Expression ifNotNull)
        {
            var nullTest = CreateIsDbNullCall( ordinal );
            return Expression.Condition( nullTest, ifNull, ifNotNull );
        }
    }

    private struct ReaderCallsToolbox
    {
        internal readonly SqlQueryReaderFactory Factory;
        internal readonly IOrdinalsCollection Ordinals;
        private readonly Expression[] _memberReadExpressions;
        private ExpressionParameterReplacer? _parameterReplacer;
        private ParameterExpression[]? _parametersToReplace;
        private Expression[]? _parameterReplacements;
        private ReaderFacadeTransformer? _readerFacadeTransformer;

        internal ReaderCallsToolbox(SqlQueryReaderFactory factory, IOrdinalsCollection ordinals, int fieldCount)
        {
            Factory = factory;
            _parameterReplacer = null;
            _parametersToReplace = null;
            _parameterReplacements = null;
            _readerFacadeTransformer = null;
            _memberReadExpressions = new Expression[fieldCount];
            Ordinals = ordinals;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Expression[] GetMemberExpressions()
        {
            return _memberReadExpressions;
        }

        [Pure]
        internal Expression ApplyTypeDefMapping(LambdaExpression mapping, Expression ordinal, Type expectedType)
        {
            Assume.ContainsExactly( mapping.Parameters, 2 );
            var readerParameter = mapping.Parameters[0];
            var ordinalParameter = mapping.Parameters[1];
            var reader = Factory._toolbox.Reader;
            Assume.True( reader.Type.IsAssignableTo( readerParameter.Type ) );
            Assume.Equals( ordinalParameter.Type, typeof( int ) );

            if ( _parameterReplacer is null )
            {
                _parametersToReplace = new ParameterExpression[2];
                _parameterReplacements = new Expression[2];
                _parameterReplacer = new ExpressionParameterReplacer( _parametersToReplace, _parameterReplacements );
            }

            Assume.IsNotNull( _parametersToReplace );
            Assume.IsNotNull( _parameterReplacements );
            _parametersToReplace[0] = readerParameter;
            _parametersToReplace[1] = ordinalParameter;
            _parameterReplacements[0] = reader.GetOrConvert( readerParameter.Type );
            _parameterReplacements[1] = ordinal;

            var result = _parameterReplacer.Visit( mapping.Body ).GetOrConvert( expectedType );
            return result;
        }

        [Pure]
        internal Expression ApplyCustomMapping(LambdaExpression mapping, Type expectedType)
        {
            Assume.ContainsExactly( mapping.Parameters, 1 );
            _readerFacadeTransformer ??= new ReaderFacadeTransformer( this );
            _readerFacadeTransformer.SetParameter( mapping.Parameters[0] );
            var result = _readerFacadeTransformer.Visit( mapping.Body ).GetOrConvert( expectedType );
            return result;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void SetReadExpression(int index, Expression expression)
        {
            _memberReadExpressions[index] = expression;
        }
    }

    private sealed class ReaderFacadeTransformer : ExpressionVisitor
    {
        private readonly ReaderCallsToolbox _readerCallsToolbox;
        private ParameterExpression? _parameter;
        private Expression? _readerReplacement;

        internal ReaderFacadeTransformer(ReaderCallsToolbox readerCallsToolbox)
        {
            _readerCallsToolbox = readerCallsToolbox;
            _parameter = null;
            _readerReplacement = null;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void SetParameter(ParameterExpression parameter)
        {
            var reader = _readerCallsToolbox.Factory._toolbox.Reader;
            _parameter = parameter;
            Assume.True( parameter.Type.IsGenericType );
            Assume.Equals( parameter.Type.GetGenericTypeDefinition(), typeof( ISqlDataRecordFacade<> ) );
            var facadeReaderType = parameter.Type.GetGenericArguments()[0];
            Assume.True( reader.Type.IsAssignableTo( facadeReaderType ) );
            _readerReplacement = reader.GetOrConvert( facadeReaderType );
        }

        [Pure]
        [return: NotNullIfNotNull( "node" )]
        public override Expression? Visit(Expression? node)
        {
            if ( node is not null )
            {
                switch ( node.NodeType )
                {
                    case ExpressionType.MemberAccess:
                        if ( node is MemberExpression member && ReferenceEquals( member.Expression, _parameter ) )
                            return VisitReaderFacadeMemberAccess( member );

                        break;

                    case ExpressionType.Call:
                        if ( node is MethodCallExpression call && ReferenceEquals( call.Object, _parameter ) )
                            return VisitReaderFacadeMethodCall( call );

                        break;
                }
            }

            return base.Visit( node );
        }

        [Pure]
        private Expression VisitReaderFacadeMemberAccess(MemberExpression node)
        {
            Assume.IsNotNull( _readerReplacement );
            Assume.Equals( node.Member.MemberType, MemberTypes.Property );
            Assume.Equals( node.Member.Name, nameof( ISqlDataRecordFacade<IDataReader>.Record ) );
            return _readerReplacement;
        }

        [Pure]
        private Expression VisitReaderFacadeMethodCall(MethodCallExpression node)
        {
            Assume.ContainsInRange( node.Arguments, 1, 2 );
            var nameArgument = ReinterpretCast.To<ConstantExpression>( node.Arguments[0] );
            Assume.Equals( nameArgument.Type, typeof( string ) );

            var fieldName = ReinterpretCast.To<string>( nameArgument.Value );
            Assume.IsNotNull( fieldName );
            var ordinal = _readerCallsToolbox.Ordinals.GetOrAddOrdinal( fieldName );

            if ( node.Method.Name == nameof( ISqlDataRecordFacade<IDataReader>.GetOrdinal ) )
            {
                Assume.ContainsExactly( node.Arguments, 1 );
                Assume.False( node.Method.IsGenericMethod );
                return ordinal;
            }

            var factory = _readerCallsToolbox.Factory;
            if ( node.Method.Name == nameof( ISqlDataRecordFacade<IDataReader>.IsNull ) )
            {
                Assume.ContainsExactly( node.Arguments, 1 );
                Assume.False( node.Method.IsGenericMethod );
                return factory._toolbox.CreateIsDbNullCall( ordinal );
            }

            var valueType = node.Type;
            Assume.True( node.Method.IsGenericMethod );
            Assume.Equals( valueType, node.Method.GetGenericArguments()[0] );

            var mapping = factory.ColumnTypeDefinitions.GetByType( valueType ).OutputMapping;
            var body = _readerCallsToolbox.ApplyTypeDefMapping( mapping, ordinal, valueType );

            if ( node.Method.Name == nameof( ISqlDataRecordFacade<IDataReader>.Get ) )
            {
                Assume.ContainsExactly( node.Arguments, 1 );
                return body;
            }

            Assume.Equals( node.Method.Name, nameof( ISqlDataRecordFacade<IDataReader>.GetNullable ) );
            var ifNull = node.Arguments.Count == 1 ? Expression.Default( valueType ) : base.Visit( node.Arguments[1] );
            Assume.Equals( ifNull.Type, valueType );
            return factory._toolbox.CreateNullableRead( ordinal, ifNull, body );
        }
    }
}

public class SqlQueryReaderFactory<TDataReader> : SqlQueryReaderFactory
    where TDataReader : IDataReader
{
    protected SqlQueryReaderFactory(SqlDialect dialect, ISqlColumnTypeDefinitionProvider columnTypeDefinitions)
        : base( typeof( TDataReader ), dialect, columnTypeDefinitions ) { }
}
