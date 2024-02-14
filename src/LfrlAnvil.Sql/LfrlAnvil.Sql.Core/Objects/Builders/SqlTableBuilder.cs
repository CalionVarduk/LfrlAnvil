using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Objects.Builders;

public abstract class SqlTableBuilder : SqlObjectBuilder, ISqlTableBuilder
{
    private SqlRecordSetInfo? _info;
    private SqlTableBuilderNode? _node;

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

    public SqlSchemaBuilder Schema { get; }
    public SqlColumnBuilderCollection Columns { get; }
    public SqlConstraintBuilderCollection Constraints { get; }
    public SqlRecordSetInfo Info => _info ??= SqlRecordSetInfo.Create( Schema.Name, Name );
    public SqlTableBuilderNode Node => _node ??= SqlNode.Table( this );

    ISqlSchemaBuilder ISqlTableBuilder.Schema => Schema;
    ISqlColumnBuilderCollection ISqlTableBuilder.Columns => Columns;
    ISqlConstraintBuilderCollection ISqlTableBuilder.Constraints => Constraints;

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqlHelpers.GetFullName( Schema.Name, Name )}";
    }

    public new SqlTableBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    protected override SqlPropertyChange<string> BeforeNameChange(string newValue)
    {
        var change = base.BeforeNameChange( newValue );
        if ( change.IsCancelled )
            return change;

        ChangeNameInCollection( Schema.Objects, this, newValue );
        return change;
    }

    protected override void AfterNameChange(string originalValue)
    {
        ResetInfoCache();
        AddNameChange( this, this, originalValue );
    }

    protected override void BeforeRemove()
    {
        base.BeforeRemove();
        QuickRemoveConstraints();
        QuickRemoveColumns();
        RemoveFromCollection( Schema.Objects, this );
    }

    protected override void AfterRemove()
    {
        AddRemoval( this, this );
    }

    protected override void QuickRemoveCore()
    {
        base.QuickRemoveCore();
        ClearCollection( Constraints );
        QuickRemoveColumns();
    }

    protected void QuickRemoveColumns()
    {
        foreach ( var column in Columns )
            QuickRemove( column );

        ClearCollection( Columns );
    }

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
