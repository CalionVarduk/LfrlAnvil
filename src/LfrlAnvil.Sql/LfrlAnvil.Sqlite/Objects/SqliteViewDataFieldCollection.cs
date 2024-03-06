using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteViewDataFieldCollection : SqlViewDataFieldCollection
{
    internal SqliteViewDataFieldCollection(SqlQueryExpressionNode source)
        : base( source ) { }

    public new SqliteView View => ReinterpretCast.To<SqliteView>( base.View );

    [Pure]
    public new SqliteViewDataField Get(string name)
    {
        return ReinterpretCast.To<SqliteViewDataField>( base.Get( name ) );
    }

    [Pure]
    public new SqliteViewDataField? TryGet(string name)
    {
        return ReinterpretCast.To<SqliteViewDataField>( base.TryGet( name ) );
    }

    [Pure]
    public new SqlObjectEnumerator<SqlViewDataField, SqliteViewDataField> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<SqliteViewDataField>();
    }

    protected override SqliteViewDataField CreateDataField(string name)
    {
        return new SqliteViewDataField( View, name );
    }
}
