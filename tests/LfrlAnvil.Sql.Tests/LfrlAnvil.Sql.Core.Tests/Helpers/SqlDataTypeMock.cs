using System.Data;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlDataTypeMock : ISqlDataType, IEquatable<SqlDataTypeMock>
{
    public static readonly SqlDataTypeMock Integer = new SqlDataTypeMock( DbType.Int32 );
    public static readonly SqlDataTypeMock Boolean = new SqlDataTypeMock( DbType.Boolean );
    public static readonly SqlDataTypeMock Real = new SqlDataTypeMock( DbType.Double );
    public static readonly SqlDataTypeMock Text = new SqlDataTypeMock( DbType.String );
    public static readonly SqlDataTypeMock Binary = new SqlDataTypeMock( DbType.Binary );
    public static readonly SqlDataTypeMock Object = new SqlDataTypeMock( DbType.Object );

    private SqlDataTypeMock(DbType dbType)
    {
        DbType = dbType;
    }

    public DbType DbType { get; }
    public SqlDialect Dialect => SqlDialectMock.Instance;
    public string Name => DbType.ToString().ToUpperInvariant();
    public ReadOnlySpan<int> Parameters => ReadOnlySpan<int>.Empty;
    public ReadOnlySpan<SqlDataTypeParameter> ParameterDefinitions => ReadOnlySpan<SqlDataTypeParameter>.Empty;

    [Pure]
    public override string ToString()
    {
        return Name;
    }

    [Pure]
    public override int GetHashCode()
    {
        return DbType.GetHashCode();
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is SqlDataTypeMock t && Equals( t );
    }

    [Pure]
    public bool Equals(SqlDataTypeMock? other)
    {
        return other is not null && DbType == other.DbType;
    }
}
