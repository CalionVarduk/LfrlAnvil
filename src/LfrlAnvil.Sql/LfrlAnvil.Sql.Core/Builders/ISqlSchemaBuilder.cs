namespace LfrlAnvil.Sql.Builders;

public interface ISqlSchemaBuilder : ISqlObjectBuilder
{
    ISqlObjectBuilderCollection Objects { get; }

    new ISqlSchemaBuilder SetName(string name);
}
