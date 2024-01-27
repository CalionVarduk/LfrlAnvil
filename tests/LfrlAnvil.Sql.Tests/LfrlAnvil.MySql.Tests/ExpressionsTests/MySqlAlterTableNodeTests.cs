using System.Collections.Generic;
using LfrlAnvil.MySql.Internal.Expressions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.MySql.Tests.ExpressionsTests;

public class MySqlAlterTableNodeTests : TestsBase
{
    [Fact]
    public void AlterTableNode_ShouldHaveCorrectProperties()
    {
        var table = SqlNode.RawRecordSet( "foo.bar" );
        var other = SqlNode.RawRecordSet( "foo.qux" );
        var info = SqlRecordSetInfo.Create( "foo", "bar" );
        var oldColumns = new[] { "a", "b" };
        var oldForeignKeys = new[] { "FK_1", "FK_2" };
        var oldChecks = new[] { "CHK_1", "CHK_2" };
        var renamedIndexes = new[] { KeyValuePair.Create( "IX_A", "IX_1" ), KeyValuePair.Create( "IX_B", "IX_2" ) };
        var changedColumns = new[]
        {
            KeyValuePair.Create( "CA", SqlNode.Column<int>( "C1" ) ),
            KeyValuePair.Create( "CB", SqlNode.Column<int>( "C2", isNullable: true ) )
        };

        var newColumns = new[] { SqlNode.Column<long>( "C3" ), SqlNode.Column<long>( "C4", isNullable: true ) };
        var newPrimaryKey = SqlNode.PrimaryKey( SqlSchemaObjectName.Create( "PK" ), SqlNode.OrderByAsc( table["C5"] ) );
        var newForeignKeys = new[]
        {
            SqlNode.ForeignKey(
                SqlSchemaObjectName.Create( "FK_3" ),
                new SqlDataFieldNode[] { table["C6"] },
                other,
                new SqlDataFieldNode[] { other["C1"] } ),
            SqlNode.ForeignKey(
                SqlSchemaObjectName.Create( "FK_4" ),
                new SqlDataFieldNode[] { table["C7"] },
                other,
                new SqlDataFieldNode[] { other["C2"] } )
        };

        var newChecks = new[]
        {
            SqlNode.Check( SqlSchemaObjectName.Create( "CHK_3" ), SqlNode.True() ),
            SqlNode.Check( SqlSchemaObjectName.Create( "CHK_4" ), SqlNode.False() )
        };

        var sut = new MySqlAlterTableNode(
            info: info,
            oldColumns: oldColumns,
            oldForeignKeys: oldForeignKeys,
            oldChecks: oldChecks,
            renamedIndexes: renamedIndexes,
            changedColumns: changedColumns,
            newColumns: newColumns,
            newPrimaryKey: newPrimaryKey,
            newForeignKeys: newForeignKeys,
            newChecks: newChecks );

        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Unknown );
            sut.Info.Should().Be( info );
            sut.OldColumns.ToArray().Should().BeSequentiallyEqualTo( oldColumns );
            sut.OldForeignKeys.ToArray().Should().BeSequentiallyEqualTo( oldForeignKeys );
            sut.OldChecks.ToArray().Should().BeSequentiallyEqualTo( oldChecks );
            sut.RenamedIndexes.ToArray().Should().BeSequentiallyEqualTo( renamedIndexes );
            sut.ChangedColumns.ToArray().Should().BeSequentiallyEqualTo( changedColumns );
            sut.NewColumns.ToArray().Should().BeSequentiallyEqualTo( newColumns );
            sut.NewPrimaryKey.Should().BeSameAs( newPrimaryKey );
            sut.NewForeignKeys.ToArray().Should().BeSequentiallyEqualTo( newForeignKeys );
            sut.NewChecks.ToArray().Should().BeSequentiallyEqualTo( newChecks );
            ((ISqlStatementNode)sut).Node.Should().BeSameAs( sut );
            ((ISqlStatementNode)sut).QueryCount.Should().Be( 0 );
            text.Should()
                .Be(
                    @"ALTER TABLE [foo].[bar]
  DROP FOREIGN KEY [FK_1]
  DROP FOREIGN KEY [FK_2]
  DROP CHECK [CHK_1]
  DROP CHECK [CHK_2]
  RENAME INDEX [IX_A] TO [IX_1]
  RENAME INDEX [IX_B] TO [IX_2]
  DROP COLUMN [a]
  DROP COLUMN [b]
  CHANGE COLUMN [CA] TO [C1] : System.Int32
  CHANGE COLUMN [CB] TO [C2] : Nullable<System.Int32>
  ADD COLUMN [C3] : System.Int64
  ADD COLUMN [C4] : Nullable<System.Int64>
  SET PRIMARY KEY [PK] (([foo.bar].[C5] : ?) ASC)
  ADD CHECK [CHK_3] (TRUE)
  ADD CHECK [CHK_4] (FALSE)
  ADD FOREIGN KEY [FK_3] (([foo.bar].[C6] : ?)) REFERENCES [foo.qux] (([foo.qux].[C1] : ?)) ON DELETE RESTRICT ON UPDATE RESTRICT
  ADD FOREIGN KEY [FK_4] (([foo.bar].[C7] : ?)) REFERENCES [foo.qux] (([foo.qux].[C2] : ?)) ON DELETE RESTRICT ON UPDATE RESTRICT" );
        }
    }
}
