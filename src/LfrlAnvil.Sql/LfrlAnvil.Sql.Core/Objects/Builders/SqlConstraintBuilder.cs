using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Objects.Builders;

public abstract class SqlConstraintBuilder : SqlObjectBuilder, ISqlConstraintBuilder
{
    internal SqlConstraintBuilder(SqlTableBuilder table, SqlObjectType type, string name)
        : base( table.Database, type, name )
    {
        Table = table;
    }

    public SqlTableBuilder Table { get; }
    ISqlTableBuilder ISqlConstraintBuilder.Table => Table;

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqlHelpers.GetFullName( Table.Schema.Name, Name )}";
    }

    public new SqlConstraintBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    public SqlConstraintBuilder SetDefaultName()
    {
        return SetName( GetDefaultName() );
    }

    [Pure]
    protected abstract string GetDefaultName();

    protected override SqlPropertyChange<string> BeforeNameChange(string newValue)
    {
        var change = base.BeforeNameChange( newValue );
        if ( change.IsCancelled )
            return change;

        ChangeNameInCollection( Table.Constraints, this, newValue );
        return change;
    }

    protected override void AfterNameChange(string originalValue)
    {
        AddNameChange( Table, this, originalValue );
    }

    protected override void BeforeRemove()
    {
        base.BeforeRemove();
        RemoveFromCollection( Table.Constraints, this );
    }

    protected override void AfterRemove()
    {
        AddRemoval( Table, this );
    }

    ISqlConstraintBuilder ISqlConstraintBuilder.SetName(string name)
    {
        return SetName( name );
    }

    ISqlConstraintBuilder ISqlConstraintBuilder.SetDefaultName()
    {
        return SetDefaultName();
    }
}
