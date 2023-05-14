using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteColumnBuilder : SqliteObjectBuilder, ISqlColumnBuilder
{
    private Dictionary<ulong, SqliteIndexBuilder>? _indexes;
    private string? _fullName;

    internal SqliteColumnBuilder(SqliteTableBuilder table, string name, SqliteColumnTypeDefinition typeDefinition)
        : base( table.Database.GetNextId(), name, SqlObjectType.Column )
    {
        Table = table;
        Name = name;
        TypeDefinition = typeDefinition;
        IsNullable = false;
        DefaultValue = null;
        _fullName = null;
        _indexes = null;
    }

    public SqliteTableBuilder Table { get; }
    public SqliteColumnTypeDefinition TypeDefinition { get; private set; }
    public bool IsNullable { get; private set; }
    public object? DefaultValue { get; private set; }
    public override SqliteDatabaseBuilder Database => Table.Database;
    public override string FullName => _fullName ??= $"{Table.FullName}.{Name}";
    public IReadOnlyCollection<SqliteIndexBuilder> Indexes => (_indexes?.Values).EmptyIfNull();
    internal override bool CanRemove => _indexes is null || _indexes.Count == 0;

    ISqlTableBuilder ISqlColumnBuilder.Table => Table;
    IReadOnlyCollection<ISqlIndexBuilder> ISqlColumnBuilder.Indexes => Indexes;
    ISqlColumnTypeDefinition ISqlColumnBuilder.TypeDefinition => TypeDefinition;
    object? ISqlColumnBuilder.DefaultValue => DefaultValue;
    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;

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

            if ( ! ReferenceEquals( Database.TypeDefinitions.TryGetByType( definition.RuntimeType ), definition ) )
                throw new SqliteObjectBuilderException( ExceptionResources.UnrecognizedTypeDefinition( definition ) );

            SetDefaultValue( null );
            var oldDefinition = TypeDefinition;
            TypeDefinition = definition;
            Database.ChangeTracker.TypeDefinitionUpdated( this, oldDefinition );
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
            Database.ChangeTracker.IsNullableUpdated( this );
        }

        return this;
    }

    public SqliteColumnBuilder SetDefaultValue(object? value)
    {
        EnsureNotRemoved();

        if ( ! Equals( DefaultValue, value ) )
        {
            var oldValue = DefaultValue;
            DefaultValue = value;
            Database.ChangeTracker.DefaultValueUpdated( this, oldValue );
        }

        return this;
    }

    [Pure]
    public SqliteIndexColumnBuilder Asc()
    {
        return SqliteIndexColumnBuilder.Asc( this );
    }

    [Pure]
    public SqliteIndexColumnBuilder Desc()
    {
        return SqliteIndexColumnBuilder.Desc( this );
    }

    internal void AddIndex(SqliteIndexBuilder index)
    {
        _indexes ??= new Dictionary<ulong, SqliteIndexBuilder>();
        _indexes.Add( index.Id, index );
    }

    internal void RemoveIndex(SqliteIndexBuilder index)
    {
        _indexes?.Remove( index.Id );
    }

    internal void UpdateDefaultValueBasedOnDataType()
    {
        Assume.IsNull( DefaultValue, nameof( DefaultValue ) );
        DefaultValue = TypeDefinition.DefaultValue;
    }

    protected override void AssertRemoval()
    {
        EnsureMutable();
    }

    protected override void RemoveCore()
    {
        Assume.Equals( CanRemove, true, nameof( CanRemove ) );

        _indexes = null;

        Table.Columns.Remove( Name );
        Database.ChangeTracker.ObjectRemoved( Table, this );
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
        ResetFullName();
        Database.ChangeTracker.NameUpdated( Table, this, oldName );
    }

    internal void ResetFullName()
    {
        _fullName = null;
    }

    private void EnsureMutable()
    {
        var errors = Chain<string>.Empty;

        if ( _indexes is not null )
        {
            foreach ( var index in _indexes.Values )
                errors = errors.Extend( ExceptionResources.ColumnIsReferencedByIndex( index ) );
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

    ISqlColumnBuilder ISqlColumnBuilder.SetDefaultValue(object? value)
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
