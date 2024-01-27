using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlColumnBuilder : MySqlObjectBuilder, ISqlColumnBuilder
{
    private Dictionary<ulong, MySqlIndexBuilder>? _referencingIndexes;
    private Dictionary<ulong, MySqlIndexBuilder>? _referencingIndexFilters;
    private Dictionary<ulong, MySqlViewBuilder>? _referencingViews;
    private Dictionary<ulong, MySqlCheckBuilder>? _referencingChecks;
    private SqlColumnBuilderNode? _node;

    internal MySqlColumnBuilder(MySqlTableBuilder table, string name, MySqlColumnTypeDefinition typeDefinition)
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

    public MySqlTableBuilder Table { get; }
    public MySqlColumnTypeDefinition TypeDefinition { get; private set; }
    public bool IsNullable { get; private set; }
    public SqlExpressionNode? DefaultValue { get; private set; }
    public override MySqlDatabaseBuilder Database => Table.Database;
    public SqlColumnBuilderNode Node => _node ??= Table.RecordSet[Name];
    public IReadOnlyCollection<MySqlIndexBuilder> ReferencingIndexes => (_referencingIndexes?.Values).EmptyIfNull();
    public IReadOnlyCollection<MySqlIndexBuilder> ReferencingIndexFilters => (_referencingIndexFilters?.Values).EmptyIfNull();
    public IReadOnlyCollection<MySqlViewBuilder> ReferencingViews => (_referencingViews?.Values).EmptyIfNull();
    public IReadOnlyCollection<MySqlCheckBuilder> ReferencingChecks => (_referencingChecks?.Values).EmptyIfNull();

    internal override bool CanRemove =>
        (_referencingIndexes is null || _referencingIndexes.Count == 0) &&
        (_referencingIndexFilters is null || _referencingIndexFilters.Count == 0) &&
        (_referencingViews is null || _referencingViews.Count == 0) &&
        (_referencingChecks is null || _referencingChecks.Count == 0);

    ISqlTableBuilder ISqlColumnBuilder.Table => Table;
    IReadOnlyCollection<ISqlIndexBuilder> ISqlColumnBuilder.ReferencingIndexes => ReferencingIndexes;
    IReadOnlyCollection<ISqlIndexBuilder> ISqlColumnBuilder.ReferencingIndexFilters => ReferencingIndexFilters;
    ISqlColumnTypeDefinition ISqlColumnBuilder.TypeDefinition => TypeDefinition;
    IReadOnlyCollection<ISqlViewBuilder> ISqlColumnBuilder.ReferencingViews => ReferencingViews;
    IReadOnlyCollection<ISqlCheckBuilder> ISqlColumnBuilder.ReferencingChecks => ReferencingChecks;
    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {MySqlHelpers.GetFullName( Table.Schema.Name, Table.Name, Name )}";
    }

    public MySqlColumnBuilder SetName(string name)
    {
        EnsureNotRemoved();
        SetNameCore( name );
        return this;
    }

    public MySqlColumnBuilder SetType(MySqlColumnTypeDefinition definition)
    {
        EnsureNotRemoved();

        if ( ! ReferenceEquals( TypeDefinition, definition ) )
        {
            EnsureMutable();

            if ( ! ReferenceEquals( Database.TypeDefinitions.TryGetByType( definition.RuntimeType ), definition ) &&
                ! ReferenceEquals( Database.TypeDefinitions.GetByDataType( definition.DataType ), definition ) )
                throw new MySqlObjectBuilderException( ExceptionResources.UnrecognizedTypeDefinition( definition ) );

            SetDefaultValue( null );
            var oldDefinition = TypeDefinition;
            TypeDefinition = definition;
            Database.ChangeTracker.TypeDefinitionUpdated( this, oldDefinition );
        }

        return this;
    }

    public MySqlColumnBuilder MarkAsNullable(bool enabled = true)
    {
        EnsureNotRemoved();

        if ( IsNullable != enabled )
        {
            EnsureMutable();
            IsNullable = enabled;
            Database.ChangeTracker.IsNullableUpdated( this );
        }

        return this;
    }

    public MySqlColumnBuilder SetDefaultValue(SqlExpressionNode? value)
    {
        EnsureNotRemoved();

        if ( ! ReferenceEquals( DefaultValue, value ) )
        {
            if ( value is not null )
            {
                var validator = new MySqlConstantExpressionValidator();
                validator.Visit( value );

                var errors = validator.GetErrors();
                if ( errors.Count > 0 )
                    throw new MySqlObjectBuilderException( errors );
            }

            var oldValue = DefaultValue;
            DefaultValue = value;
            Database.ChangeTracker.DefaultValueUpdated( this, oldValue );
        }

        return this;
    }

    [Pure]
    public MySqlIndexColumnBuilder Asc()
    {
        return MySqlIndexColumnBuilder.Asc( this );
    }

    [Pure]
    public MySqlIndexColumnBuilder Desc()
    {
        return MySqlIndexColumnBuilder.Desc( this );
    }

    internal void AddReferencingIndex(MySqlIndexBuilder index)
    {
        _referencingIndexes ??= new Dictionary<ulong, MySqlIndexBuilder>();
        _referencingIndexes.Add( index.Id, index );
    }

    internal void RemoveReferencingIndex(MySqlIndexBuilder index)
    {
        _referencingIndexes?.Remove( index.Id );
    }

    internal void AddReferencingIndexFilter(MySqlIndexBuilder index)
    {
        _referencingIndexFilters ??= new Dictionary<ulong, MySqlIndexBuilder>();
        _referencingIndexFilters.Add( index.Id, index );
    }

    internal void RemoveReferencingIndexFilter(MySqlIndexBuilder index)
    {
        _referencingIndexFilters?.Remove( index.Id );
    }

    internal void AddReferencingView(MySqlViewBuilder view)
    {
        _referencingViews ??= new Dictionary<ulong, MySqlViewBuilder>();
        _referencingViews.Add( view.Id, view );
    }

    internal void RemoveReferencingView(MySqlViewBuilder view)
    {
        _referencingViews?.Remove( view.Id );
    }

    internal void AddReferencingCheck(MySqlCheckBuilder check)
    {
        _referencingChecks ??= new Dictionary<ulong, MySqlCheckBuilder>();
        _referencingChecks.Add( check.Id, check );
    }

    internal void RemoveReferencingCheck(MySqlCheckBuilder check)
    {
        _referencingChecks?.Remove( check.Id );
    }

    internal void UpdateDefaultValueBasedOnDataType()
    {
        Assume.IsNull( DefaultValue );
        DefaultValue = TypeDefinition.DefaultValue;
    }

    internal void MarkAsRemoved()
    {
        Assume.Equals( IsRemoved, false );
        IsRemoved = true;

        _referencingIndexes = null;
        _referencingIndexFilters = null;
        _referencingViews = null;
        _referencingChecks = null;
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
        Database.ChangeTracker.ObjectRemoved( Table, this );
    }

    protected override void SetNameCore(string name)
    {
        if ( Name == name )
            return;

        EnsureMutable();
        MySqlHelpers.AssertName( name );
        Table.Columns.ChangeName( this, name );

        var oldName = Name;
        Name = name;
        Database.ChangeTracker.NameUpdated( Table, this, oldName );
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
            throw new MySqlObjectBuilderException( errors );
    }

    ISqlColumnBuilder ISqlColumnBuilder.SetName(string name)
    {
        return SetName( name );
    }

    ISqlColumnBuilder ISqlColumnBuilder.SetType(ISqlColumnTypeDefinition definition)
    {
        return SetType( MySqlHelpers.CastOrThrow<MySqlColumnTypeDefinition>( definition ) );
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
    ISqlIndexColumnBuilder ISqlColumnBuilder.Asc()
    {
        return Asc();
    }

    [Pure]
    ISqlIndexColumnBuilder ISqlColumnBuilder.Desc()
    {
        return Desc();
    }
}
