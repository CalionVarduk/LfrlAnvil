﻿namespace LfrlAnvil.Sql;

public interface ISqlDataType
{
    SqlDialect Dialect { get; }
    string Name { get; }
    ISqlDataType? ParentType { get; }
}