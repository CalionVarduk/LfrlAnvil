using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlConstraintCollectionMock : SqlConstraintCollection
{
    public SqlConstraintCollectionMock(SqlConstraintBuilderCollection source)
        : base( source ) { }
}
