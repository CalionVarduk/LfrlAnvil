using System;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Versioning;

internal sealed class SqlLambdaDatabaseVersion<TDatabaseBuilder> : SqlDatabaseVersion<TDatabaseBuilder>
    where TDatabaseBuilder : class, ISqlDatabaseBuilder
{
    private readonly Action<TDatabaseBuilder> _apply;

    internal SqlLambdaDatabaseVersion(Action<TDatabaseBuilder> apply, Version value, string? description)
        : base( value, description )
    {
        _apply = apply;
    }

    public override void Apply(TDatabaseBuilder database)
    {
        _apply( database );
    }
}
