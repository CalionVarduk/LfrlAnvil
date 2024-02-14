using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlUnknownObjectBuilderMock : SqlConstraintBuilder
{
    public SqlUnknownObjectBuilderMock(SqlTableBuilderMock table, string name, bool useDefaultImplementation, bool deferCreation)
        : base( table, name )
    {
        UseDefaultImplementation = useDefaultImplementation;
        DeferCreation = deferCreation;
    }

    public bool UseDefaultImplementation { get; }
    public bool DeferCreation { get; }

    protected override void AfterNameChange(string originalValue)
    {
        AddNameChange( Table, this, originalValue );
    }

    protected override void AfterRemove()
    {
        AddRemoval( Table, this );
    }

    [Pure]
    protected override string GetDefaultName()
    {
        return "UNK";
    }
}
