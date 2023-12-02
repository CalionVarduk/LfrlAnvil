using System.Linq;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class BaseExpressionsTests
{
    public class CompoundQuery : TestsBase
    {
        [Fact]
        public void Selection_ShouldContainTypedSelections_WhenAllQueriesHaveSimilarSelections()
        {
            var set1 = SqlNode.RawRecordSet( "foo" ).ToDataSource();
            var a1 = set1.From.GetRawField( "a", TypeNullability.Create<int>() );
            var a1Select = a1.AsSelf();
            var b1 = set1.From.GetRawField( "b", TypeNullability.Create<string>() );
            var b1Select = b1.AsSelf();
            var query1 = set1.Select( a1Select, b1Select );

            var set2 = SqlNode.RawRecordSet( "bar" ).ToDataSource();
            var a2 = set2.From.GetRawField( "a", TypeNullability.Create<int>( isNullable: true ) );
            var a2Select = a2.AsSelf();
            var b2 = set2.From.GetRawField( "b", TypeNullability.Create<string>() );
            var b2Select = b2.AsSelf();
            var query2 = set2.Select( a2Select, b2Select );

            var sut = query1.CompoundWith( query2.ToUnion() );

            var result = sut.Selection.ToArray();

            using ( new AssertionScope() )
            {
                result.Should().HaveCount( 2 );

                var element1 = result.ElementAtOrDefault( 0 ) as SqlSelectCompoundFieldNode;
                (element1?.ToString()).Should().Be( "[a]" );
                (element1?.NodeType).Should().Be( SqlNodeType.SelectCompoundField );
                (element1?.Name).Should().Be( "a" );
                (element1?.Origins.Length).Should().Be( 2 );

                var origin1 = element1?.Origins.Span[0];
                (origin1?.QueryIndex).Should().Be( 0 );
                (origin1?.Selection).Should().BeSameAs( a1Select );
                (origin1?.Expression).Should().BeSameAs( a1 );
                var origin2 = element1?.Origins.Span[1];
                (origin2?.QueryIndex).Should().Be( 1 );
                (origin2?.Selection).Should().BeSameAs( a2Select );
                (origin2?.Expression).Should().BeSameAs( a2 );

                var element2 = result.ElementAtOrDefault( 1 ) as SqlSelectCompoundFieldNode;
                (element2?.ToString()).Should().Be( "[b]" );
                (element2?.NodeType).Should().Be( SqlNodeType.SelectCompoundField );
                (element2?.Name).Should().Be( "b" );
                (element2?.Origins.Length).Should().Be( 2 );

                var origin3 = element2?.Origins.Span[0];
                (origin3?.QueryIndex).Should().Be( 0 );
                (origin3?.Selection).Should().BeSameAs( b1Select );
                (origin3?.Expression).Should().BeSameAs( b1 );
                var origin4 = element2?.Origins.Span[1];
                (origin4?.QueryIndex).Should().Be( 1 );
                (origin4?.Selection).Should().BeSameAs( b2Select );
                (origin4?.Expression).Should().BeSameAs( b2 );
            }
        }

        [Fact]
        public void Selection_ShouldFlattenSelectAllNodesToKnownFields()
        {
            var set1 = TableMock.Create( "T1", ColumnMock.CreateMany<int>( areNullable: false, "a", "b" ) ).ToRecordSet().ToDataSource();
            var t2 = TableMock.Create( "T2", ColumnMock.CreateMany<int>( areNullable: false, "a", "b" ) ).ToRecordSet();
            var t3 = TableMock.Create( "T3", ColumnMock.Create<int>( "c" ) ).ToRecordSet();
            var set2 = t2.Join( t3.InnerOn( t2["a"] == t3["c"] ) );

            var select1 = set1.GetAll();
            var select2 = set2["T2"].GetAll();

            var query1 = set1.Select( select1 );
            var query2 = set2.Select( select2 );

            var sut = query1.CompoundWith( query2.ToUnion() );

            var result = sut.Selection.ToArray();

            using ( new AssertionScope() )
            {
                result.Should().HaveCount( 2 );

                var element1 = result.ElementAtOrDefault( 0 ) as SqlSelectCompoundFieldNode;
                (element1?.NodeType).Should().Be( SqlNodeType.SelectCompoundField );
                (element1?.Name).Should().Be( "a" );
                (element1?.Origins.ToArray()).Should()
                    .BeSequentiallyEqualTo(
                        new SqlSelectCompoundFieldNode.Origin( QueryIndex: 0, Selection: select1, Expression: set1["T1"]["a"] ),
                        new SqlSelectCompoundFieldNode.Origin( QueryIndex: 1, Selection: select2, Expression: set2["T2"]["a"] ) );

                var element2 = result.ElementAtOrDefault( 1 ) as SqlSelectCompoundFieldNode;
                (element2?.NodeType).Should().Be( SqlNodeType.SelectCompoundField );
                (element2?.Name).Should().Be( "b" );
                (element2?.Origins.ToArray()).Should()
                    .BeSequentiallyEqualTo(
                        new SqlSelectCompoundFieldNode.Origin( QueryIndex: 0, Selection: select1, Expression: set1["T1"]["b"] ),
                        new SqlSelectCompoundFieldNode.Origin( QueryIndex: 1, Selection: select2, Expression: set2["T2"]["b"] ) );
            }
        }

        [Fact]
        public void AddTrait_ShouldCreateCompoundQueryWithTrait_WhenCalledForTheFirstTime()
        {
            var sut = SqlNode.RawQuery( "SELECT * FROM foo" ).CompoundWith( SqlNode.RawQuery( "SELECT * FROM bar" ).ToUnion() );
            var trait = SqlNode.LimitTrait( SqlNode.Literal( 10 ) );
            var result = sut.AddTrait( trait );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Traits.Should().BeSequentiallyEqualTo( trait );
                text.Should()
                    .Be(
                        @"(
  SELECT * FROM foo
)
UNION
(
  SELECT * FROM bar
)
LIMIT (""10"" : System.Int32)" );
            }
        }

        [Fact]
        public void AddTrait_ShouldCreateCompoundQueryWithTraits_WhenCalledForTheSecondTime()
        {
            var firstTrait = SqlNode.LimitTrait( SqlNode.Literal( 10 ) );
            var sut = SqlNode.RawQuery( "SELECT * FROM foo" )
                .CompoundWith( SqlNode.RawQuery( "SELECT * FROM bar" ).ToUnion() )
                .AddTrait( firstTrait );

            var secondTrait = SqlNode.OffsetTrait( SqlNode.Literal( 15 ) );
            var result = sut.AddTrait( secondTrait );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Traits.Should().BeSequentiallyEqualTo( firstTrait, secondTrait );
                text.Should()
                    .Be(
                        @"(
  SELECT * FROM foo
)
UNION
(
  SELECT * FROM bar
)
LIMIT (""10"" : System.Int32)
OFFSET (""15"" : System.Int32)" );
            }
        }

        [Fact]
        public void SetTraits_ShouldCreateCompoundQueryWithOverriddenTraits()
        {
            var sut = SqlNode.RawQuery( "SELECT * FROM foo" )
                .CompoundWith( SqlNode.RawQuery( "SELECT * FROM bar" ).ToUnion() )
                .AddTrait( SqlNode.DistinctTrait() );

            var traits = Chain.Create<SqlTraitNode>( SqlNode.LimitTrait( SqlNode.Literal( 10 ) ) )
                .Extend( SqlNode.OffsetTrait( SqlNode.Literal( 15 ) ) );

            var result = sut.SetTraits( traits );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Traits.Should().BeSequentiallyEqualTo( traits );
                text.Should()
                    .Be(
                        @"(
  SELECT * FROM foo
)
UNION
(
  SELECT * FROM bar
)
LIMIT (""10"" : System.Int32)
OFFSET (""15"" : System.Int32)" );
            }
        }
    }
}
