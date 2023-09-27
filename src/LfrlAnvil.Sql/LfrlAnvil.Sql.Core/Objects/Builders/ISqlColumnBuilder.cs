using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlColumnBuilder : ISqlObjectBuilder
{
    ISqlTableBuilder Table { get; }
    ISqlColumnTypeDefinition TypeDefinition { get; }
    bool IsNullable { get; }
    object? DefaultValue { get; }
    IReadOnlyCollection<ISqlIndexBuilder> Indexes { get; }
    IReadOnlyCollection<ISqlViewBuilder> ReferencingViews { get; }

    new ISqlColumnBuilder SetName(string name);
    ISqlColumnBuilder MarkAsNullable(bool enabled = true);
    ISqlColumnBuilder SetType(ISqlColumnTypeDefinition definition);
    ISqlColumnBuilder SetDefaultValue(object? value);

    [Pure]
    ISqlIndexColumnBuilder Asc();

    [Pure]
    ISqlIndexColumnBuilder Desc();
}
