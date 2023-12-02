using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Sql.Tests.Helpers.Data;

public sealed class DbDataParameter : IDbDataParameter
{
    public DbType DbType { get; set; }
    public ParameterDirection Direction { get; set; }
    public bool IsNullable { get; set; }

    [AllowNull]
    public string ParameterName { get; set; }

    [AllowNull]
    public string SourceColumn { get; set; }

    public DataRowVersion SourceVersion { get; set; }
    public object? Value { get; set; }
    public byte Precision { get; set; }
    public byte Scale { get; set; }
    public int Size { get; set; }
}
