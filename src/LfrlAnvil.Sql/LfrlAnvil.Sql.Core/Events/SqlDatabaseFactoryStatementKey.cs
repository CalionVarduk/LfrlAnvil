using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Events;

public readonly record struct SqlDatabaseFactoryStatementKey(Version Version, int Ordinal)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDatabaseFactoryStatementKey Create(Version version)
    {
        return new SqlDatabaseFactoryStatementKey( version, Ordinal: 0 );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlDatabaseFactoryStatementKey NextOrdinal()
    {
        return new SqlDatabaseFactoryStatementKey( Version, Ordinal + 1 );
    }
}
