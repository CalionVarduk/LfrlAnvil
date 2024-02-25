using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlColumnTypeDefinitionMock<T> : SqlColumnTypeDefinition<T, DbDataReaderMock, DbParameterMock>
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

    public override void SetParameterInfo(DbParameterMock parameter, bool isNullable)
    {
        base.SetParameterInfo( parameter, isNullable );
        parameter.IsNullable = isNullable;
    }
}
