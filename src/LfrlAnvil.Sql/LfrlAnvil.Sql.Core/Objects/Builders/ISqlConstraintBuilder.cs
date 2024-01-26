namespace LfrlAnvil.Sql.Objects.Builders;

public interface ISqlConstraintBuilder : ISqlObjectBuilder
{
    ISqlTableBuilder Table { get; }

    new ISqlConstraintBuilder SetName(string name);
    ISqlConstraintBuilder SetDefaultName();
}
