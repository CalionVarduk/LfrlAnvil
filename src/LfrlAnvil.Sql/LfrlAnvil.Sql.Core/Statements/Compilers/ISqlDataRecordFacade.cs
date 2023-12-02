using System.Data;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Statements.Compilers;

public interface ISqlDataRecordFacade<out TDataRecord>
    where TDataRecord : IDataRecord
{
    TDataRecord Record { get; }

    [Pure]
    T Get<T>(string name)
        where T : notnull;

    [Pure]
    T? GetNullable<T>(string name);

    [Pure]
    T GetNullable<T>(string name, T @default)
        where T : notnull;

    [Pure]
    bool IsNull(string name);

    [Pure]
    int GetOrdinal(string name);
}
