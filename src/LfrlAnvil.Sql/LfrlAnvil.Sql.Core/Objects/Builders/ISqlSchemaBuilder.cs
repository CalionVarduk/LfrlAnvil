namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlSchemaBuilder : ISqlObjectBuilder
{
    ISqlObjectBuilderCollection Objects { get; }

    new ISqlSchemaBuilder SetName(string name);
}
