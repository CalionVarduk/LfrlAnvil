using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <inheritdoc cref="ISqlPrimaryKeyBuilder" />
public abstract class SqlPrimaryKeyBuilder : SqlConstraintBuilder, ISqlPrimaryKeyBuilder
{
    /// <summary>
    /// Creates a new <see cref="SqlPrimaryKeyBuilder"/> instance.
    /// </summary>
    /// <param name="index">Underlying index that defines this primary key.</param>
    /// <param name="name">Object's name.</param>
    protected SqlPrimaryKeyBuilder(SqlIndexBuilder index, string name)
        : base( index.Table, SqlObjectType.PrimaryKey, name )
    {
        Index = index;
    }

    /// <inheritdoc cref="ISqlPrimaryKeyBuilder.Index" />
    public SqlIndexBuilder Index { get; }

    /// <inheritdoc />
    public override bool CanRemove => Index.CanRemove;

    ISqlIndexBuilder ISqlPrimaryKeyBuilder.Index => Index;

    /// <inheritdoc cref="SqlConstraintBuilder.SetName(string)" />
    public new SqlPrimaryKeyBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc cref="SqlConstraintBuilder.SetDefaultName()" />
    public new SqlPrimaryKeyBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    /// <inheritdoc />
    [Pure]
    protected sealed override string GetDefaultName()
    {
        return Database.DefaultNames.GetForPrimaryKey( Table );
    }

    /// <inheritdoc />
    protected override void BeforeRemove()
    {
        Index.Remove();
    }

    /// <inheritdoc />
    protected override void AfterRemove() { }

    ISqlPrimaryKeyBuilder ISqlPrimaryKeyBuilder.SetName(string name)
    {
        return SetName( name );
    }

    ISqlPrimaryKeyBuilder ISqlPrimaryKeyBuilder.SetDefaultName()
    {
        return SetDefaultName();
    }
}
