using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteColumnBuilder : SqliteObjectBuilder, ISqlColumnBuilder
{
    private Dictionary<ulong, SqliteIndexBuilder>? _referencingIndexes;
    private Dictionary<ulong, SqliteIndexBuilder>? _referencingIndexFilters;
    private Dictionary<ulong, SqliteViewBuilder>? _referencingViews;
    private Dictionary<ulong, SqliteCheckBuilder>? _referencingChecks;
    private SqlColumnBuilderNode? _node;

    internal SqliteColumnBuilder(SqliteTableBuilder table, string name, SqliteColumnTypeDefinition typeDefinition)
        : base( table.Database.GetNextId(), name, SqlObjectType.Column )
    {
        Table = table;
        Name = name;
        TypeDefinition = typeDefinition;
        IsNullable = false;
        DefaultValue = null;
        _referencingIndexes = null;
        _referencingIndexFilters = null;
        _referencingViews = null;
        _referencingChecks = null;
        _node = null;
    }

    public SqliteTableBuilder Table { get; }
    public SqliteColumnTypeDefinition TypeDefinition { get; private set; }
    public bool IsNullable { get; private set; }
    public SqlExpressionNode? DefaultValue { get; private set; }
    public override SqliteDatabaseBuilder Database => Table.Database;
    public SqlColumnBuilderNode Node => _node ??= Table.Node[Name];
    public IReadOnlyCollection<SqliteIndexBuilder> ReferencingIndexes => (_referencingIndexes?.Values).EmptyIfNull();
    public IReadOnlyCollection<SqliteIndexBuilder> ReferencingIndexFilters => (_referencingIndexFilters?.Values).EmptyIfNull();
    public IReadOnlyCollection<SqliteViewBuilder> ReferencingViews => (_referencingViews?.Values).EmptyIfNull();
    public IReadOnlyCollection<SqliteCheckBuilder> ReferencingChecks => (_referencingChecks?.Values).EmptyIfNull();

    public override bool CanRemove =>
        (_referencingIndexes is null || _referencingIndexes.Count == 0) &&
        (_referencingIndexFilters is null || _referencingIndexFilters.Count == 0) &&
        (_referencingViews is null || _referencingViews.Count == 0) &&
        (_referencingChecks is null || _referencingChecks.Count == 0);

    ISqlTableBuilder ISqlColumnBuilder.Table => Table;
    ISqlColumnTypeDefinition ISqlColumnBuilder.TypeDefinition => TypeDefinition;
    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( Table.Schema.Name, Table.Name, Name )}";
    }

    public SqliteColumnBuilder SetName(string name)
    {
        EnsureNotRemoved();
        SetNameCore( name );
        return this;
    }

    public SqliteColumnBuilder SetType(SqliteColumnTypeDefinition definition)
    {
        EnsureNotRemoved();

        if ( ! ReferenceEquals( TypeDefinition, definition ) )
        {
            EnsureMutable();

            if ( ! ReferenceEquals( Database.TypeDefinitions.TryGetByType( definition.RuntimeType ), definition ) &&
                ! ReferenceEquals( Database.TypeDefinitions.GetByDataType( definition.DataType ), definition ) )
                throw new SqliteObjectBuilderException( ExceptionResources.UnrecognizedTypeDefinition( definition ) );

            SetDefaultValue( null );
            var oldDefinition = TypeDefinition;
            TypeDefinition = definition;
            Database.Changes.TypeDefinitionUpdated( this, oldDefinition );
        }

        return this;
    }

    public SqliteColumnBuilder MarkAsNullable(bool enabled = true)
    {
        EnsureNotRemoved();

        if ( IsNullable != enabled )
        {
            EnsureMutable();
            IsNullable = enabled;
            Database.Changes.IsNullableUpdated( this );
        }

        return this;
    }

    public SqliteColumnBuilder SetDefaultValue(SqlExpressionNode? value)
    {
        EnsureNotRemoved();

        if ( ! ReferenceEquals( DefaultValue, value ) )
        {
            if ( value is not null )
            {
                var validator = new SqliteConstantExpressionValidator();
                validator.Visit( value );

                var errors = validator.GetErrors();
                if ( errors.Count > 0 )
                    throw new SqliteObjectBuilderException( errors );
            }

            var oldValue = DefaultValue;
            DefaultValue = value;
            Database.Changes.DefaultValueUpdated( this, oldValue );
        }

        return this;
    }

    [Pure]
    public SqlIndexColumnBuilder<SqliteColumnBuilder> Asc()
    {
        return SqlIndexColumnBuilder.CreateAsc( this );
    }

    [Pure]
    public SqlIndexColumnBuilder<SqliteColumnBuilder> Desc()
    {
        return SqlIndexColumnBuilder.CreateDesc( this );
    }

    internal void AddReferencingIndex(SqliteIndexBuilder index)
    {
        _referencingIndexes ??= new Dictionary<ulong, SqliteIndexBuilder>();
        _referencingIndexes.Add( index.Id, index );
    }

    internal void RemoveReferencingIndex(SqliteIndexBuilder index)
    {
        _referencingIndexes?.Remove( index.Id );
    }

    internal void AddReferencingIndexFilter(SqliteIndexBuilder index)
    {
        _referencingIndexFilters ??= new Dictionary<ulong, SqliteIndexBuilder>();
        _referencingIndexFilters.Add( index.Id, index );
    }

    internal void RemoveReferencingIndexFilter(SqliteIndexBuilder index)
    {
        _referencingIndexFilters?.Remove( index.Id );
    }

    internal void AddReferencingView(SqliteViewBuilder view)
    {
        _referencingViews ??= new Dictionary<ulong, SqliteViewBuilder>();
        _referencingViews.Add( view.Id, view );
    }

    internal void RemoveReferencingView(SqliteViewBuilder view)
    {
        _referencingViews?.Remove( view.Id );
    }

    internal void AddReferencingCheck(SqliteCheckBuilder check)
    {
        _referencingChecks ??= new Dictionary<ulong, SqliteCheckBuilder>();
        _referencingChecks.Add( check.Id, check );
    }

    internal void RemoveReferencingCheck(SqliteCheckBuilder check)
    {
        _referencingChecks?.Remove( check.Id );
    }

    internal void UpdateDefaultValueBasedOnDataType()
    {
        Assume.IsNull( DefaultValue );
        DefaultValue = TypeDefinition.DefaultValue;
    }

    protected override void AssertRemoval()
    {
        EnsureMutable();
    }

    protected override void RemoveCore()
    {
        Assume.Equals( CanRemove, true );

        _referencingIndexes = null;
        _referencingIndexFilters = null;
        _referencingViews = null;
        _referencingChecks = null;

        Table.Columns.Remove( Name );
        Database.Changes.ObjectRemoved( Table, this );
    }

    protected override void SetNameCore(string name)
    {
        if ( Name == name )
            return;

        EnsureMutable();
        SqliteHelpers.AssertName( name );
        Table.Columns.ChangeName( this, name );

        var oldName = Name;
        Name = name;
        Database.Changes.NameUpdated( Table, this, oldName );
    }

    private void EnsureMutable()
    {
        var errors = Chain<string>.Empty;

        if ( _referencingIndexes is not null && _referencingIndexes.Count > 0 )
        {
            foreach ( var index in _referencingIndexes.Values )
                errors = errors.Extend( ExceptionResources.ColumnIsReferencedByObject( index ) );
        }

        if ( _referencingIndexFilters is not null && _referencingIndexFilters.Count > 0 )
        {
            foreach ( var index in _referencingIndexFilters.Values )
                errors = errors.Extend( ExceptionResources.ColumnIsReferencedByIndexFilter( index ) );
        }

        if ( _referencingViews is not null && _referencingViews.Count > 0 )
        {
            foreach ( var view in _referencingViews.Values )
                errors = errors.Extend( ExceptionResources.ColumnIsReferencedByObject( view ) );
        }

        if ( _referencingChecks is not null && _referencingChecks.Count > 0 )
        {
            foreach ( var check in _referencingChecks.Values )
                errors = errors.Extend( ExceptionResources.ColumnIsReferencedByObject( check ) );
        }

        if ( errors.Count > 0 )
            throw new SqliteObjectBuilderException( errors );
    }

    ISqlColumnBuilder ISqlColumnBuilder.SetName(string name)
    {
        return SetName( name );
    }

    ISqlColumnBuilder ISqlColumnBuilder.SetType(ISqlColumnTypeDefinition definition)
    {
        return SetType( SqliteHelpers.CastOrThrow<SqliteColumnTypeDefinition>( definition ) );
    }

    ISqlColumnBuilder ISqlColumnBuilder.MarkAsNullable(bool enabled)
    {
        return MarkAsNullable( enabled );
    }

    ISqlColumnBuilder ISqlColumnBuilder.SetDefaultValue(SqlExpressionNode? value)
    {
        return SetDefaultValue( value );
    }

    [Pure]
    SqlIndexColumnBuilder<ISqlColumnBuilder> ISqlColumnBuilder.Asc()
    {
        return Asc();
    }

    [Pure]
    SqlIndexColumnBuilder<ISqlColumnBuilder> ISqlColumnBuilder.Desc()
    {
        return Desc();
    }
}
