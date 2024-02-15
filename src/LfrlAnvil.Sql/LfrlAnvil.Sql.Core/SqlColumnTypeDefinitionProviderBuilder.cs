using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql;

public abstract class SqlColumnTypeDefinitionProviderBuilder : ISqlColumnTypeDefinitionProviderBuilder
{
    internal readonly Dictionary<Type, SqlColumnTypeDefinition> Definitions;

    protected SqlColumnTypeDefinitionProviderBuilder(SqlDialect dialect)
    {
        Definitions = new Dictionary<Type, SqlColumnTypeDefinition>();
        Dialect = dialect;
    }

    public SqlDialect Dialect { get; }

    [Pure]
    public bool Contains(Type type)
    {
        return Definitions.ContainsKey( type );
    }

    public SqlColumnTypeDefinitionProviderBuilder Register(SqlColumnTypeDefinition definition)
    {
        Ensure.Equals( definition.DataType.Dialect, Dialect );
        Definitions[definition.RuntimeType] = definition;
        return this;
    }

    [Pure]
    public abstract SqlColumnTypeDefinitionProvider Build();

    protected void AddOrUpdate(SqlColumnTypeDefinition definition)
    {
        Assume.Equals( definition.DataType.Dialect, Dialect );
        Definitions[definition.RuntimeType] = definition;
    }

    ISqlColumnTypeDefinitionProviderBuilder ISqlColumnTypeDefinitionProviderBuilder.Register(ISqlColumnTypeDefinition definition)
    {
        return Register( SqlHelpers.CastOrThrow<SqlColumnTypeDefinition>( Dialect, definition ) );
    }

    [Pure]
    ISqlColumnTypeDefinitionProvider ISqlColumnTypeDefinitionProviderBuilder.Build()
    {
        return Build();
    }
}
