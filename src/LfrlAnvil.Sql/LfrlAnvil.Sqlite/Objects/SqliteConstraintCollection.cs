using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteConstraintCollection : SqlConstraintCollection
{
    internal SqliteConstraintCollection(SqliteConstraintBuilderCollection source)
        : base( source ) { }

    public new SqlitePrimaryKey PrimaryKey => ReinterpretCast.To<SqlitePrimaryKey>( base.PrimaryKey );
    public new SqliteTable Table => ReinterpretCast.To<SqliteTable>( base.Table );

    [Pure]
    public new SqliteIndex GetIndex(string name)
    {
        return ReinterpretCast.To<SqliteIndex>( base.GetIndex( name ) );
    }

    [Pure]
    public new SqliteIndex? TryGetIndex(string name)
    {
        return ReinterpretCast.To<SqliteIndex>( base.TryGetIndex( name ) );
    }

    [Pure]
    public new SqliteForeignKey GetForeignKey(string name)
    {
        return ReinterpretCast.To<SqliteForeignKey>( base.GetForeignKey( name ) );
    }

    [Pure]
    public new SqliteForeignKey? TryGetForeignKey(string name)
    {
        return ReinterpretCast.To<SqliteForeignKey>( base.TryGetForeignKey( name ) );
    }

    [Pure]
    public new SqliteCheck GetCheck(string name)
    {
        return ReinterpretCast.To<SqliteCheck>( base.GetCheck( name ) );
    }

    [Pure]
    public new SqliteCheck? TryGetCheck(string name)
    {
        return ReinterpretCast.To<SqliteCheck>( base.TryGetCheck( name ) );
    }
}
