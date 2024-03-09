using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlViewDataFieldCollection : SqlViewDataFieldCollection
{
    internal MySqlViewDataFieldCollection(SqlQueryExpressionNode source)
        : base( source ) { }

    public new MySqlView View => ReinterpretCast.To<MySqlView>( base.View );

    [Pure]
    public new MySqlViewDataField Get(string name)
    {
        return ReinterpretCast.To<MySqlViewDataField>( base.Get( name ) );
    }

    [Pure]
    public new MySqlViewDataField? TryGet(string name)
    {
        return ReinterpretCast.To<MySqlViewDataField>( base.TryGet( name ) );
    }

    [Pure]
    public new SqlObjectEnumerator<SqlViewDataField, MySqlViewDataField> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<MySqlViewDataField>();
    }

    protected override MySqlViewDataField CreateDataField(string name)
    {
        return new MySqlViewDataField( View, name );
    }
}
