using System.Diagnostics.Contracts;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlConstraintCollection : SqlConstraintCollection
{
    internal MySqlConstraintCollection(MySqlConstraintBuilderCollection source)
        : base( source ) { }

    /// <inheritdoc cref="SqlConstraintCollection.PrimaryKey" />
    public new MySqlPrimaryKey PrimaryKey => ReinterpretCast.To<MySqlPrimaryKey>( base.PrimaryKey );

    /// <inheritdoc cref="SqlConstraintCollection.Table" />
    public new MySqlTable Table => ReinterpretCast.To<MySqlTable>( base.Table );

    /// <inheritdoc cref="SqlConstraintCollection.GetIndex(string)" />
    [Pure]
    public new MySqlIndex GetIndex(string name)
    {
        return ReinterpretCast.To<MySqlIndex>( base.GetIndex( name ) );
    }

    /// <inheritdoc cref="SqlConstraintCollection.TryGetIndex(string)" />
    [Pure]
    public new MySqlIndex? TryGetIndex(string name)
    {
        return ReinterpretCast.To<MySqlIndex>( base.TryGetIndex( name ) );
    }

    /// <inheritdoc cref="SqlConstraintCollection.GetForeignKey(string)" />
    [Pure]
    public new MySqlForeignKey GetForeignKey(string name)
    {
        return ReinterpretCast.To<MySqlForeignKey>( base.GetForeignKey( name ) );
    }

    /// <inheritdoc cref="SqlConstraintCollection.TryGetForeignKey(string)" />
    [Pure]
    public new MySqlForeignKey? TryGetForeignKey(string name)
    {
        return ReinterpretCast.To<MySqlForeignKey>( base.TryGetForeignKey( name ) );
    }

    /// <inheritdoc cref="SqlConstraintCollection.GetCheck(string)" />
    [Pure]
    public new MySqlCheck GetCheck(string name)
    {
        return ReinterpretCast.To<MySqlCheck>( base.GetCheck( name ) );
    }

    /// <inheritdoc cref="SqlConstraintCollection.TryGetCheck(string)" />
    [Pure]
    public new MySqlCheck? TryGetCheck(string name)
    {
        return ReinterpretCast.To<MySqlCheck>( base.TryGetCheck( name ) );
    }
}
