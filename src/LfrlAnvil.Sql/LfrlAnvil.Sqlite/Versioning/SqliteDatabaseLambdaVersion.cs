using System;
using LfrlAnvil.Sqlite.Builders;

namespace LfrlAnvil.Sqlite.Versioning;

internal sealed class SqliteDatabaseLambdaVersion : SqliteDatabaseVersion
{
    private readonly Action<SqliteDatabaseBuilder> _apply;

    internal SqliteDatabaseLambdaVersion(Version value, string description, Action<SqliteDatabaseBuilder> apply)
        : base( value, description )
    {
        _apply = apply;
    }

    protected override void Apply(SqliteDatabaseBuilder database)
    {
        _apply( database );
    }
}
