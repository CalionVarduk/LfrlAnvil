using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlConstraintCollectionMock : SqlConstraintCollection
{
    public SqlConstraintCollectionMock(SqlConstraintBuilderCollection source)
        : base( source ) { }
}
