using System.Data.Common;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlColumnTypeDefinitionMock<T> : SqlColumnTypeDefinition<T, DbDataRecord, DbParameter>
    where T : notnull
{
    public SqlColumnTypeDefinitionMock(SqlDataTypeMock dataType, T defaultValue)
        : base( dataType, defaultValue, static (r, i) => (T)r.GetValue( i ) ) { }

    public new SqlDataTypeMock DataType => ReinterpretCast.To<SqlDataTypeMock>( base.DataType );

    [Pure]
    public override string ToDbLiteral(T value)
    {
        return value.ToString() ?? string.Empty;
    }

    public override object ToParameterValue(T value)
    {
        return value;
    }

    public override void SetParameterInfo(DbParameter parameter, bool isNullable)
    {
        base.SetParameterInfo( parameter, isNullable );
        parameter.IsNullable = isNullable;
    }
}
