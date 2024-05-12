using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <inheritdoc cref="ISqlTableBuilder" />
public abstract class SqlTableBuilder : SqlObjectBuilder, ISqlTableBuilder
{
    private SqlRecordSetInfo? _info;
    private SqlTableBuilderNode? _node;

    /// <summary>
    /// Creates a new <see cref="SqlTableBuilder"/> instance.
    /// </summary>
    /// <param name="schema">Schema that this table belongs to.</param>
    /// <param name="name">Object's name.</param>
    /// <param name="columns">Collection of columns that belong to this table.</param>
    /// <param name="constraints">Collection of constraints that belong to this table.</param>
    protected SqlTableBuilder(
        SqlSchemaBuilder schema,
        string name,
        SqlColumnBuilderCollection columns,
        SqlConstraintBuilderCollection constraints)
        : base( schema.Database, SqlObjectType.Table, name )
    {
        _node = null;
        _info = null;
        Schema = schema;
        Columns = columns;
        Constraints = constraints;
        Columns.SetTable( this );
        Constraints.SetTable( this );
    }

    /// <inheritdoc cref="ISqlTableBuilder.Schema" />
    public SqlSchemaBuilder Schema { get; }

    /// <inheritdoc cref="ISqlTableBuilder.Columns" />
    public SqlColumnBuilderCollection Columns { get; }

    /// <inheritdoc cref="ISqlTableBuilder.Constraints" />
    public SqlConstraintBuilderCollection Constraints { get; }

    /// <inheritdoc />
    public SqlRecordSetInfo Info => _info ??= SqlRecordSetInfo.Create( Schema.Name, Name );

    /// <inheritdoc />
    public SqlTableBuilderNode Node => _node ??= SqlNode.Table( this );

    ISqlSchemaBuilder ISqlTableBuilder.Schema => Schema;
    ISqlColumnBuilderCollection ISqlTableBuilder.Columns => Columns;
    ISqlConstraintBuilderCollection ISqlTableBuilder.Constraints => Constraints;

    /// <summary>
    /// Returns a string representation of this <see cref="SqlTableBuilder"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqlHelpers.GetFullName( Schema.Name, Name )}";
    }

    /// <inheritdoc cref="SqlObjectBuilder.SetName(string)" />
    public new SqlTableBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc />
    protected override SqlPropertyChange<string> BeforeNameChange(string newValue)
    {
        var change = base.BeforeNameChange( newValue );
        if ( change.IsCancelled )
            return change;

        ChangeNameInCollection( Schema.Objects, this, newValue );
        return change;
    }

    /// <inheritdoc />
    protected override void AfterNameChange(string originalValue)
    {
        ResetInfoCache();
        AddNameChange( this, this, originalValue );
    }

    /// <inheritdoc />
    protected override void BeforeRemove()
    {
        base.BeforeRemove();
        QuickRemoveConstraints();
        QuickRemoveColumns();
        RemoveFromCollection( Schema.Objects, this );
    }

    /// <inheritdoc />
    protected override void AfterRemove()
    {
        AddRemoval( this, this );
    }

    /// <inheritdoc />
    protected override void QuickRemoveCore()
    {
        base.QuickRemoveCore();
        ClearCollection( Constraints );
        QuickRemoveColumns();
    }

    /// <summary>
    /// Removes all <see cref="Columns"/> from this table. This is a version for the <see cref="QuickRemoveCore()"/> method.
    /// </summary>
    protected void QuickRemoveColumns()
    {
        foreach ( var column in Columns )
            QuickRemove( column );

        ClearCollection( Columns );
    }

    /// <summary>
    /// Removes all <see cref="Constraints"/> from this table. This is a version for the <see cref="QuickRemoveCore()"/> method.
    /// </summary>
    protected void QuickRemoveConstraints()
    {
        foreach ( var constraint in Constraints )
        {
            QuickRemove( constraint );
            RemoveFromCollection( Schema.Objects, constraint );
        }

        ClearCollection( Constraints );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void ResetInfoCache()
    {
        _info = null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal SqlRecordSetInfo? GetCachedInfo()
    {
        return _info;
    }

    ISqlTableBuilder ISqlTableBuilder.SetName(string name)
    {
        return SetName( name );
    }
}
