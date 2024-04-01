using System.Diagnostics.Contracts;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.PostgreSql.Objects;

public sealed class PostgreSqlConstraintCollection : SqlConstraintCollection
{
    internal PostgreSqlConstraintCollection(PostgreSqlConstraintBuilderCollection source)
        : base( source ) { }

    public new PostgreSqlPrimaryKey PrimaryKey => ReinterpretCast.To<PostgreSqlPrimaryKey>( base.PrimaryKey );
    public new PostgreSqlTable Table => ReinterpretCast.To<PostgreSqlTable>( base.Table );

    [Pure]
    public new PostgreSqlIndex GetIndex(string name)
    {
        return ReinterpretCast.To<PostgreSqlIndex>( base.GetIndex( name ) );
    }

    [Pure]
    public new PostgreSqlIndex? TryGetIndex(string name)
    {
        return ReinterpretCast.To<PostgreSqlIndex>( base.TryGetIndex( name ) );
    }

    [Pure]
    public new PostgreSqlForeignKey GetForeignKey(string name)
    {
        return ReinterpretCast.To<PostgreSqlForeignKey>( base.GetForeignKey( name ) );
    }

    [Pure]
    public new PostgreSqlForeignKey? TryGetForeignKey(string name)
    {
        return ReinterpretCast.To<PostgreSqlForeignKey>( base.TryGetForeignKey( name ) );
    }

    [Pure]
    public new PostgreSqlCheck GetCheck(string name)
    {
        return ReinterpretCast.To<PostgreSqlCheck>( base.GetCheck( name ) );
    }

    [Pure]
    public new PostgreSqlCheck? TryGetCheck(string name)
    {
        return ReinterpretCast.To<PostgreSqlCheck>( base.TryGetCheck( name ) );
    }
}
