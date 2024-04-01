using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Exceptions;

internal static class Resources
{
    internal const string ConnectionStringMustIncludeDatabase = "PostgreSql connection string must include a database.";

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string GeneratedColumnsWithVirtualStorageAreForbidden(PostgreSqlColumnBuilder column, SqlColumnComputation computation)
    {
        return
            $"Cannot set '{computation.Expression}' with virtual storage as computation of '{column}' because generated columns with virtual storage are forbidden.";
    }
}
