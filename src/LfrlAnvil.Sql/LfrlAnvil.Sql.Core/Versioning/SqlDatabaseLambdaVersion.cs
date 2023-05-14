using System;
using LfrlAnvil.Sql.Builders;

namespace LfrlAnvil.Sql.Versioning;

internal sealed class SqlDatabaseLambdaVersion : SqlDatabaseVersion
{
    private readonly Action<ISqlDatabaseBuilder> _apply;

    internal SqlDatabaseLambdaVersion(Version value, string description, Action<ISqlDatabaseBuilder> apply)
        : base( value, description )
    {
        _apply = apply;
    }

    public override void Apply(ISqlDatabaseBuilder database)
    {
        _apply( database );
    }
}
