using System.Diagnostics.Contracts;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlConstraintCollection : SqlConstraintCollection
{
    internal MySqlConstraintCollection(MySqlConstraintBuilderCollection source)
        : base( source ) { }

    public new MySqlPrimaryKey PrimaryKey => ReinterpretCast.To<MySqlPrimaryKey>( base.PrimaryKey );
    public new MySqlTable Table => ReinterpretCast.To<MySqlTable>( base.Table );

    [Pure]
    public new MySqlIndex GetIndex(string name)
    {
        return ReinterpretCast.To<MySqlIndex>( base.GetIndex( name ) );
    }

    [Pure]
    public new MySqlIndex? TryGetIndex(string name)
    {
        return ReinterpretCast.To<MySqlIndex>( base.TryGetIndex( name ) );
    }

    [Pure]
    public new MySqlForeignKey GetForeignKey(string name)
    {
        return ReinterpretCast.To<MySqlForeignKey>( base.GetForeignKey( name ) );
    }

    [Pure]
    public new MySqlForeignKey? TryGetForeignKey(string name)
    {
        return ReinterpretCast.To<MySqlForeignKey>( base.TryGetForeignKey( name ) );
    }

    [Pure]
    public new MySqlCheck GetCheck(string name)
    {
        return ReinterpretCast.To<MySqlCheck>( base.GetCheck( name ) );
    }

    [Pure]
    public new MySqlCheck? TryGetCheck(string name)
    {
        return ReinterpretCast.To<MySqlCheck>( base.TryGetCheck( name ) );
    }
}
