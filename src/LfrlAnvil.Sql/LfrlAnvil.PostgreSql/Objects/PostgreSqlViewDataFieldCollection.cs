using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.PostgreSql.Objects;

public sealed class PostgreSqlViewDataFieldCollection : SqlViewDataFieldCollection
{
    internal PostgreSqlViewDataFieldCollection(SqlQueryExpressionNode source)
        : base( source ) { }

    public new PostgreSqlView View => ReinterpretCast.To<PostgreSqlView>( base.View );

    [Pure]
    public new PostgreSqlViewDataField Get(string name)
    {
        return ReinterpretCast.To<PostgreSqlViewDataField>( base.Get( name ) );
    }

    [Pure]
    public new PostgreSqlViewDataField? TryGet(string name)
    {
        return ReinterpretCast.To<PostgreSqlViewDataField>( base.TryGet( name ) );
    }

    [Pure]
    public new SqlObjectEnumerator<SqlViewDataField, PostgreSqlViewDataField> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<PostgreSqlViewDataField>();
    }

    protected override PostgreSqlViewDataField CreateDataField(string name)
    {
        return new PostgreSqlViewDataField( View, name );
    }
}
