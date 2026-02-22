using System.Linq;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.TestExtensions.Sql.Mocks;

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

            result.TestCount( count => count.TestEquals( 2 ) )
                .Then( r => Assertion.All(
                    r[0]
                        .TestType()
                        .AssignableTo<SqlSelectCompoundFieldNode>( n => Assertion.All(
                            "firstNode",
                            n.ToString().TestEquals( "[a]" ),
                            n.NodeType.TestEquals( SqlNodeType.SelectCompoundField ),
                            n.Name.TestEquals( "a" ),
                            n.Origins.TestCount( count => count.TestEquals( 2 ) )
                                .Then( o => Assertion.All(
                                    "origins",
                                    o[0].QueryIndex.TestEquals( 0 ),
                                    o[0].Selection.TestRefEquals( a1Select ),
                                    o[0].Expression.TestRefEquals( a1 ),
                                    o[1].QueryIndex.TestEquals( 1 ),
                                    o[1].Selection.TestRefEquals( a2Select ),
                                    o[1].Expression.TestRefEquals( a2 ) ) ) ) ),
                    r[1]
                        .TestType()
                        .AssignableTo<SqlSelectCompoundFieldNode>( n => Assertion.All(
                            "secondNode",
                            n.ToString().TestEquals( "[b]" ),
                            n.NodeType.TestEquals( SqlNodeType.SelectCompoundField ),
                            n.Name.TestEquals( "b" ),
                            n.Origins.TestCount( count => count.TestEquals( 2 ) )
                                .Then( o => Assertion.All(
                                    "origins",
                                    o[0].QueryIndex.TestEquals( 0 ),
                                    o[0].Selection.TestRefEquals( b1Select ),
                                    o[0].Expression.TestRefEquals( b1 ),
                                    o[1].QueryIndex.TestEquals( 1 ),
                                    o[1].Selection.TestRefEquals( b2Select ),
                                    o[1].Expression.TestRefEquals( b2 ) ) ) ) ) ) )
                .Go();
        }

        [Fact]
        public void Selection_ShouldFlattenSelectAllNodesToKnownFields()
        {
            var set1 = SqlTableMock.Create<int>( "T1", new[] { "a", "b" } ).Node.ToDataSource();
            var t2 = SqlTableMock.Create<int>( "T2", new[] { "a", "b" } ).Node;
            var t3 = SqlTableMock.Create<int>( "T3", new[] { "c" } ).Node;
            var set2 = t2.Join( t3.InnerOn( t2["a"] == t3["c"] ) );

            var select1 = set1.GetAll();
            var select2 = set2["common.T2"].GetAll();

            var query1 = set1.Select( select1 );
            var query2 = set2.Select( select2 );

            var sut = query1.CompoundWith( query2.ToUnion() );

            var result = sut.Selection.ToArray();

            result.TestCount( count => count.TestEquals( 2 ) )
                .Then( r => Assertion.All(
                    r[0]
                        .TestType()
                        .AssignableTo<SqlSelectCompoundFieldNode>( n =>
                            Assertion.All(
                                "firstNode",
                                n.NodeType.TestEquals( SqlNodeType.SelectCompoundField ),
                                n.Name.TestEquals( "a" ),
                                n.Origins.TestSequence(
                                [
                                    new SqlSelectCompoundFieldNode.Origin(
                                        QueryIndex: 0,
                                        Selection: select1,
                                        Expression: set1["common.T1"]["a"] ),
                                    new SqlSelectCompoundFieldNode.Origin(
                                        QueryIndex: 1,
                                        Selection: select2,
                                        Expression: set2["common.T2"]["a"] )
                                ] ) ) ),
                    r[1]
                        .TestType()
                        .AssignableTo<SqlSelectCompoundFieldNode>( n =>
                            Assertion.All(
                                "secondNode",
                                n.NodeType.TestEquals( SqlNodeType.SelectCompoundField ),
                                n.Name.TestEquals( "b" ),
                                n.Origins.TestSequence(
                                [
                                    new SqlSelectCompoundFieldNode.Origin(
                                        QueryIndex: 0,
                                        Selection: select1,
                                        Expression: set1["common.T1"]["b"] ),
                                    new SqlSelectCompoundFieldNode.Origin(
                                        QueryIndex: 1,
                                        Selection: select2,
                                        Expression: set2["common.T2"]["b"] )
                                ] ) ) ) ) )
                .Go();
        }

        [Fact]
        public void AddTrait_ShouldCreateCompoundQueryWithTrait_WhenCalledForTheFirstTime()
        {
            var sut = SqlNode.RawQuery( "SELECT * FROM foo" ).CompoundWith( SqlNode.RawQuery( "SELECT * FROM bar" ).ToUnion() );
            var trait = SqlNode.LimitTrait( SqlNode.Literal( 10 ) );
            var result = sut.AddTrait( trait );
            var text = result.ToString();

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.Traits.TestSequence( [ trait ] ),
                    text.TestEquals(
                        """
                        SELECT * FROM foo
                        UNION
                        SELECT * FROM bar
                        LIMIT ("10" : System.Int32)
                        """ ) )
                .Go();
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

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.Traits.TestSequence( [ firstTrait, secondTrait ] ),
                    text.TestEquals(
                        """
                        SELECT * FROM foo
                        UNION
                        SELECT * FROM bar
                        LIMIT ("10" : System.Int32)
                        OFFSET ("15" : System.Int32)
                        """ ) )
                .Go();
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

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.Traits.TestSequence( traits ),
                    text.TestEquals(
                        """
                        SELECT * FROM foo
                        UNION
                        SELECT * FROM bar
                        LIMIT ("10" : System.Int32)
                        OFFSET ("15" : System.Int32)
                        """ ) )
                .Go();
        }
    }
}
