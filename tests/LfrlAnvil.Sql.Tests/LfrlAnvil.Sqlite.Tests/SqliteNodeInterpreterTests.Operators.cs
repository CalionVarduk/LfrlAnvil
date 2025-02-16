using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.Sqlite.Tests;

public partial class SqliteNodeInterpreterTests
{
    public class Operators : TestsBase
    {
        [Fact]
        public void Visit_ShouldInterpretNegate_WithValueWrappedInParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( -25 ).Negate() );
            sut.Context.Sql.ToString().TestEquals( "-(-25)" ).Go();
        }

        [Fact]
        public void VisitChild_ShouldInterpretNegateWithParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.Literal( 25 ).Negate() );
            sut.Context.Sql.ToString().TestEquals( "(-(25))" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretAdd_WhenValuesRequireParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ) + SqlNode.RawExpression( "foo.b" ) );
            sut.Context.Sql.ToString().TestEquals( "(foo.a) + (foo.b)" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretAdd_WhenValuesDoNotRequireParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 25 ) + SqlNode.Literal( 35 ) );
            sut.Context.Sql.ToString().TestEquals( "25 + 35" ).Go();
        }

        [Fact]
        public void VisitChild_ShouldInterpretAddWithParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.Literal( 25 ) + SqlNode.Literal( 35 ) );
            sut.Context.Sql.ToString().TestEquals( "(25 + 35)" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretConcat_WhenValuesRequireParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ).Concat( SqlNode.RawExpression( "foo.b" ) ) );
            sut.Context.Sql.ToString().TestEquals( "(foo.a) || (foo.b)" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretConcat_WhenValuesDoNotRequireParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( "foo" ).Concat( SqlNode.Literal( "bar" ) ) );
            sut.Context.Sql.ToString().TestEquals( "'foo' || 'bar'" ).Go();
        }

        [Fact]
        public void VisitChild_ShouldInterpretConcatWithParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.Literal( "foo" ).Concat( SqlNode.Literal( "bar" ) ) );
            sut.Context.Sql.ToString().TestEquals( "('foo' || 'bar')" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretSubtract_WhenValuesRequireParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ) - SqlNode.RawExpression( "foo.b" ) );
            sut.Context.Sql.ToString().TestEquals( "(foo.a) - (foo.b)" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretSubtract_WhenValuesDoNotRequireParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 25 ) - SqlNode.Literal( 35 ) );
            sut.Context.Sql.ToString().TestEquals( "25 - 35" ).Go();
        }

        [Fact]
        public void VisitChild_ShouldInterpretSubtractWithParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.Literal( 25 ) - SqlNode.Literal( 35 ) );
            sut.Context.Sql.ToString().TestEquals( "(25 - 35)" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretMultiply_WhenValuesRequireParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ) * SqlNode.RawExpression( "foo.b" ) );
            sut.Context.Sql.ToString().TestEquals( "(foo.a) * (foo.b)" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretMultiply_WhenValuesDoNotRequireParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 25 ) * SqlNode.Literal( 35 ) );
            sut.Context.Sql.ToString().TestEquals( "25 * 35" ).Go();
        }

        [Fact]
        public void VisitChild_ShouldInterpretMultiplyWithParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.Literal( 25 ) * SqlNode.Literal( 35 ) );
            sut.Context.Sql.ToString().TestEquals( "(25 * 35)" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretDivide_WhenValuesRequireParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ) / SqlNode.RawExpression( "foo.b" ) );
            sut.Context.Sql.ToString().TestEquals( "(foo.a) / (foo.b)" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretDivide_WhenValuesDoNotRequireParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 25 ) / SqlNode.Literal( 35 ) );
            sut.Context.Sql.ToString().TestEquals( "25 / 35" ).Go();
        }

        [Fact]
        public void VisitChild_ShouldInterpretDivideWithParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.Literal( 25 ) / SqlNode.Literal( 35 ) );
            sut.Context.Sql.ToString().TestEquals( "(25 / 35)" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretModulo_WhenValuesRequireParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ) % SqlNode.RawExpression( "foo.b" ) );
            sut.Context.Sql.ToString().TestEquals( "MOD((foo.a), (foo.b))" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretModulo_WhenValuesDoNotRequireParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 25 ) % SqlNode.Literal( 35 ) );
            sut.Context.Sql.ToString().TestEquals( "MOD(25, 35)" ).Go();
        }

        [Fact]
        public void VisitChild_ShouldInterpretModuloWithoutParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.Literal( 25 ) % SqlNode.Literal( 35 ) );
            sut.Context.Sql.ToString().TestEquals( "MOD(25, 35)" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretBitwiseNot_WithValueWrappedInParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( -25 ).BitwiseNot() );
            sut.Context.Sql.ToString().TestEquals( "~(-25)" ).Go();
        }

        [Fact]
        public void VisitChild_ShouldInterpretBitwiseNotWithParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.Literal( 25 ).BitwiseNot() );
            sut.Context.Sql.ToString().TestEquals( "(~(25))" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretBitwiseAnd_WhenValuesRequireParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ) & SqlNode.RawExpression( "foo.b" ) );
            sut.Context.Sql.ToString().TestEquals( "(foo.a) & (foo.b)" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretBitwiseAnd_WhenValuesDoNotRequireParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 25 ) & SqlNode.Literal( 35 ) );
            sut.Context.Sql.ToString().TestEquals( "25 & 35" ).Go();
        }

        [Fact]
        public void VisitChild_ShouldInterpretBitwiseAndWithParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.Literal( 25 ) & SqlNode.Literal( 35 ) );
            sut.Context.Sql.ToString().TestEquals( "(25 & 35)" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretBitwiseOr_WhenValuesRequireParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ) | SqlNode.RawExpression( "foo.b" ) );
            sut.Context.Sql.ToString().TestEquals( "(foo.a) | (foo.b)" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretBitwiseOr_WhenValuesDoNotRequireParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 25 ) | SqlNode.Literal( 35 ) );
            sut.Context.Sql.ToString().TestEquals( "25 | 35" ).Go();
        }

        [Fact]
        public void VisitChild_ShouldInterpretBitwiseOrWithParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.Literal( 25 ) | SqlNode.Literal( 35 ) );
            sut.Context.Sql.ToString().TestEquals( "(25 | 35)" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretBitwiseXor_WhenValuesRequireParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ) ^ SqlNode.RawExpression( "foo.b" ) );
            sut.Context.Sql.ToString().TestEquals( "((foo.a) | (foo.b)) & ~((foo.a) & (foo.b))" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretBitwiseXor_WhenValuesDoNotRequireParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 25 ) ^ SqlNode.Literal( 35 ) );
            sut.Context.Sql.ToString().TestEquals( "(25 | 35) & ~(25 & 35)" ).Go();
        }

        [Fact]
        public void VisitChild_ShouldInterpretBitwiseXorWithParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.Literal( 25 ) ^ SqlNode.Literal( 35 ) );
            sut.Context.Sql.ToString().TestEquals( "((25 | 35) & ~(25 & 35))" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretBitwiseLeftShift_WhenValuesRequireParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ).BitwiseLeftShift( SqlNode.RawExpression( "foo.b" ) ) );
            sut.Context.Sql.ToString().TestEquals( "(foo.a) << (foo.b)" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretBitwiseLeftShift_WhenValuesDoNotRequireParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 25 ).BitwiseLeftShift( SqlNode.Literal( 35 ) ) );
            sut.Context.Sql.ToString().TestEquals( "25 << 35" ).Go();
        }

        [Fact]
        public void VisitChild_ShouldInterpretBitwiseLeftShiftWithParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.Literal( 25 ).BitwiseLeftShift( SqlNode.Literal( 35 ) ) );
            sut.Context.Sql.ToString().TestEquals( "(25 << 35)" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretBitwiseRightShift_WhenValuesRequireParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ).BitwiseRightShift( SqlNode.RawExpression( "foo.b" ) ) );
            sut.Context.Sql.ToString().TestEquals( "(foo.a) >> (foo.b)" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretBitwiseRightShift_WhenValuesDoNotRequireParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 25 ).BitwiseRightShift( SqlNode.Literal( 35 ) ) );
            sut.Context.Sql.ToString().TestEquals( "25 >> 35" ).Go();
        }

        [Fact]
        public void VisitChild_ShouldInterpretBitwiseRightShiftWithParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.Literal( 25 ).BitwiseRightShift( SqlNode.Literal( 35 ) ) );
            sut.Context.Sql.ToString().TestEquals( "(25 >> 35)" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretEqualTo()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ) == SqlNode.Literal( 10 ) );
            sut.Context.Sql.ToString().TestEquals( "(foo.a) = 10" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretEqualTo_WhenRightOperandIsNull()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ) == SqlNode.Null() );
            sut.Context.Sql.ToString().TestEquals( "(foo.a) IS NULL" ).Go();
        }

        [Fact]
        public void VisitChild_ShouldInterpretEqualToWithParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.RawExpression( "foo.a" ) == SqlNode.Literal( 10 ) );
            sut.Context.Sql.ToString().TestEquals( "((foo.a) = 10)" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretNotEqualTo()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ) != SqlNode.Literal( 10 ) );
            sut.Context.Sql.ToString().TestEquals( "(foo.a) <> 10" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretNotEqualTo_WhenRightOperandIsNull()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ) != SqlNode.Null() );
            sut.Context.Sql.ToString().TestEquals( "(foo.a) IS NOT NULL" ).Go();
        }

        [Fact]
        public void VisitChild_ShouldInterpretNotEqualToWithParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.RawExpression( "foo.a" ) != SqlNode.Literal( 10 ) );
            sut.Context.Sql.ToString().TestEquals( "((foo.a) <> 10)" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretGreaterThan()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ) > SqlNode.Literal( 10 ) );
            sut.Context.Sql.ToString().TestEquals( "(foo.a) > 10" ).Go();
        }

        [Fact]
        public void VisitChild_ShouldInterpretGreaterThanWithParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.RawExpression( "foo.a" ) > SqlNode.Literal( 10 ) );
            sut.Context.Sql.ToString().TestEquals( "((foo.a) > 10)" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretLessThan()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ) < SqlNode.Literal( 10 ) );
            sut.Context.Sql.ToString().TestEquals( "(foo.a) < 10" ).Go();
        }

        [Fact]
        public void VisitChild_ShouldInterpretLessThanWithParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.RawExpression( "foo.a" ) < SqlNode.Literal( 10 ) );
            sut.Context.Sql.ToString().TestEquals( "((foo.a) < 10)" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretGreaterThanOrEqualTo()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ) >= SqlNode.Literal( 10 ) );
            sut.Context.Sql.ToString().TestEquals( "(foo.a) >= 10" ).Go();
        }

        [Fact]
        public void VisitChild_ShouldInterpretGreaterThanOrEqualToWithParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.RawExpression( "foo.a" ) >= SqlNode.Literal( 10 ) );
            sut.Context.Sql.ToString().TestEquals( "((foo.a) >= 10)" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretLessThanOrEqualTo()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ) <= SqlNode.Literal( 10 ) );
            sut.Context.Sql.ToString().TestEquals( "(foo.a) <= 10" ).Go();
        }

        [Fact]
        public void VisitChild_ShouldInterpretLessThanOrEqualToWithParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.RawExpression( "foo.a" ) <= SqlNode.Literal( 10 ) );
            sut.Context.Sql.ToString().TestEquals( "((foo.a) <= 10)" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretAnd()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawCondition( "foo.a > 10" ).And( SqlNode.RawCondition( "foo.b < 20" ) ) );
            sut.Context.Sql.ToString().TestEquals( "(foo.a > 10) AND (foo.b < 20)" ).Go();
        }

        [Fact]
        public void VisitChild_ShouldInterpretAndWithParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.RawCondition( "foo.a > 10" ).And( SqlNode.RawCondition( "foo.b < 20" ) ) );
            sut.Context.Sql.ToString().TestEquals( "((foo.a > 10) AND (foo.b < 20))" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretOr()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawCondition( "foo.a > 10" ).Or( SqlNode.RawCondition( "foo.b < 20" ) ) );
            sut.Context.Sql.ToString().TestEquals( "(foo.a > 10) OR (foo.b < 20)" ).Go();
        }

        [Fact]
        public void VisitChild_ShouldInterpretOrWithParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.RawCondition( "foo.a > 10" ).Or( SqlNode.RawCondition( "foo.b < 20" ) ) );
            sut.Context.Sql.ToString().TestEquals( "((foo.a > 10) OR (foo.b < 20))" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretBetween()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ).IsBetween( SqlNode.Literal( 10 ), SqlNode.Literal( 20 ) ) );
            sut.Context.Sql.ToString().TestEquals( "(foo.a) BETWEEN 10 AND 20" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretNotBetween()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ).IsNotBetween( SqlNode.Literal( 10 ), SqlNode.Literal( 20 ) ) );
            sut.Context.Sql.ToString().TestEquals( "(foo.a) NOT BETWEEN 10 AND 20" ).Go();
        }

        [Fact]
        public void VisitChild_ShouldInterpretBetweenWithParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.RawExpression( "foo.a" ).IsBetween( SqlNode.Literal( 10 ), SqlNode.Literal( 20 ) ) );
            sut.Context.Sql.ToString().TestEquals( "((foo.a) BETWEEN 10 AND 20)" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretExists()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawQuery( "SELECT * FROM foo" ).Exists() );
            sut.Context.Sql.ToString()
                .TestEquals(
                    """
                    EXISTS (
                      SELECT * FROM foo
                    )
                    """ )
                .Go();
        }

        [Fact]
        public void Visit_ShouldInterpretNotExists()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawQuery( "SELECT * FROM foo" ).NotExists() );
            sut.Context.Sql.ToString()
                .TestEquals(
                    """
                    NOT EXISTS (
                      SELECT * FROM foo
                    )
                    """ )
                .Go();
        }

        [Fact]
        public void VisitChild_ShouldInterpretExistsWithParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.RawQuery( "SELECT * FROM foo" ).Exists() );
            sut.Context.Sql.ToString()
                .TestEquals(
                    """
                    (EXISTS (
                        SELECT * FROM foo
                      ))
                    """ )
                .Go();
        }

        [Fact]
        public void Visit_ShouldInterpretLike()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ).Like( SqlNode.Literal( "\\%bar%" ), SqlNode.Literal( "\\" ) ) );
            sut.Context.Sql.ToString().TestEquals( @"(foo.a) LIKE '\%bar%' ESCAPE '\'" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretNotLike()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ).NotLike( SqlNode.Literal( "%bar%" ) ) );
            sut.Context.Sql.ToString().TestEquals( "(foo.a) NOT LIKE '%bar%'" ).Go();
        }

        [Fact]
        public void VisitChild_ShouldInterpretLikeWithParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.RawExpression( "foo.a" ).Like( SqlNode.Literal( "%bar%" ) ) );
            sut.Context.Sql.ToString().TestEquals( "((foo.a) LIKE '%bar%')" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretIn()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ).In( SqlNode.Literal( "foo" ), SqlNode.Literal( "bar" ) ) );
            sut.Context.Sql.ToString().TestEquals( "(foo.a) IN ('foo', 'bar')" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretNotIn()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ).NotIn( SqlNode.Literal( "foo" ), SqlNode.Literal( "bar" ) ) );
            sut.Context.Sql.ToString().TestEquals( "(foo.a) NOT IN ('foo', 'bar')" ).Go();
        }

        [Fact]
        public void VisitChild_ShouldInterpretInWithParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.RawExpression( "foo.a" ).In( SqlNode.Literal( "foo" ), SqlNode.Literal( "bar" ) ) );
            sut.Context.Sql.ToString().TestEquals( "((foo.a) IN ('foo', 'bar'))" ).Go();
        }

        [Fact]
        public void Visit_ShouldInterpretInQuery()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ).InQuery( SqlNode.RawQuery( "SELECT qux FROM bar" ) ) );
            sut.Context.Sql.ToString()
                .TestEquals(
                    """
                    (foo.a) IN (
                      SELECT qux FROM bar
                    )
                    """ )
                .Go();
        }

        [Fact]
        public void Visit_ShouldInterpretNotInQuery()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ).NotInQuery( SqlNode.RawQuery( "SELECT qux FROM bar" ) ) );
            sut.Context.Sql.ToString()
                .TestEquals(
                    """
                    (foo.a) NOT IN (
                      SELECT qux FROM bar
                    )
                    """ )
                .Go();
        }

        [Fact]
        public void VisitChild_ShouldInterpretInQueryWithParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.RawExpression( "foo.a" ).InQuery( SqlNode.RawQuery( "SELECT qux FROM bar" ) ) );
            sut.Context.Sql.ToString()
                .TestEquals(
                    """
                    ((foo.a) IN (
                        SELECT qux FROM bar
                      ))
                    """ )
                .Go();
        }
    }
}
