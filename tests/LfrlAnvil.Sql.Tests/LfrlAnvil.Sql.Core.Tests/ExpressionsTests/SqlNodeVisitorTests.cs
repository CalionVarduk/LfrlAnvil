using System.Collections.Generic;
using System.Data;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public class SqlNodeVisitorTests : TestsBase
{
    [Fact]
    public void VisitRawExpression_ShouldVisitParameters()
    {
        var sut = new VisitorMock();
        var parameters = new[] { SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) };

        sut.VisitRawExpression( SqlNode.RawExpression( "@a + @b", parameters ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( parameters[0], parameters[1] );
    }

    [Fact]
    public void VisitRawDataField_ShouldVisitRecordSet()
    {
        var sut = new VisitorMock();
        var recordSet = SqlNode.RawRecordSet( "foo" );

        sut.VisitRawDataField( SqlNode.RawDataField( recordSet, "bar" ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( recordSet );
    }

    [Fact]
    public void VisitNull_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitNull( SqlNode.Null() ) );
        action.Should().NotThrow();
    }

    [Fact]
    public void VisitLiteral_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitLiteral( (SqlLiteralNode)SqlNode.Literal( 20 ) ) );
        action.Should().NotThrow();
    }

    [Fact]
    public void VisitParameter_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitParameter( SqlNode.Parameter( "a" ) ) );
        action.Should().NotThrow();
    }

    [Fact]
    public void VisitColumn_ShouldVisitRecordSet()
    {
        var sut = new VisitorMock();
        var recordSet = TableMock.Create( "foo", ColumnMock.Create<int>( "bar" ) ).ToRecordSet();

        sut.VisitColumn( recordSet["bar"] );

        sut.Nodes.Should().BeSequentiallyEqualTo( recordSet );
    }

    [Fact]
    public void VisitColumnBuilder_ShouldVisitRecordSet()
    {
        var sut = new VisitorMock();
        var recordSet = TableMock.CreateBuilder( "foo", ColumnMock.CreateBuilder<int>( "bar" ) ).ToRecordSet();

        sut.VisitColumnBuilder( recordSet["bar"] );

        sut.Nodes.Should().BeSequentiallyEqualTo( recordSet );
    }

    [Fact]
    public void VisitQueryDataField_ShouldVisitRecordSet()
    {
        var sut = new VisitorMock();
        var recordSet = ViewMock.CreateBuilder(
                "qux",
                SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( x => new[] { x.From.GetUnsafeField( "bar" ).AsSelf() } ) )
            .ToRecordSet();

        sut.VisitQueryDataField( recordSet["bar"] );

        sut.Nodes.Should().BeSequentiallyEqualTo( recordSet );
    }

    [Fact]
    public void VisitViewDataField_ShouldVisitRecordSet()
    {
        var sut = new VisitorMock();
        var recordSet = ViewMock.Create(
                "qux",
                SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( x => new[] { x.From.GetUnsafeField( "bar" ).AsSelf() } ) )
            .ToRecordSet();

        sut.VisitViewDataField( recordSet["bar"] );

        sut.Nodes.Should().BeSequentiallyEqualTo( recordSet );
    }

    [Fact]
    public void VisitNegate_ShouldVisitValue()
    {
        var sut = new VisitorMock();
        var value = SqlNode.Literal( 20 );

        sut.VisitNegate( value.Negate() );

        sut.Nodes.Should().BeSequentiallyEqualTo( value );
    }

    [Fact]
    public void VisitAdd_ShouldVisitOperands()
    {
        var sut = new VisitorMock();
        var operands = new[] { SqlNode.Literal( 20 ), SqlNode.Literal( 30 ) };

        sut.VisitAdd( operands[0] + operands[1] );

        sut.Nodes.Should().BeSequentiallyEqualTo( operands[0], operands[1] );
    }

    [Fact]
    public void VisitConcat_ShouldVisitOperands()
    {
        var sut = new VisitorMock();
        var operands = new[] { SqlNode.Literal( 20 ), SqlNode.Literal( 30 ) };

        sut.VisitConcat( operands[0].Concat( operands[1] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( operands[0], operands[1] );
    }

    [Fact]
    public void VisitSubtract_ShouldVisitOperands()
    {
        var sut = new VisitorMock();
        var operands = new[] { SqlNode.Literal( 20 ), SqlNode.Literal( 30 ) };

        sut.VisitSubtract( operands[0] - operands[1] );

        sut.Nodes.Should().BeSequentiallyEqualTo( operands[0], operands[1] );
    }

    [Fact]
    public void VisitMultiply_ShouldVisitOperands()
    {
        var sut = new VisitorMock();
        var operands = new[] { SqlNode.Literal( 20 ), SqlNode.Literal( 30 ) };

        sut.VisitMultiply( operands[0] * operands[1] );

        sut.Nodes.Should().BeSequentiallyEqualTo( operands[0], operands[1] );
    }

    [Fact]
    public void VisitDivide_ShouldVisitOperands()
    {
        var sut = new VisitorMock();
        var operands = new[] { SqlNode.Literal( 20 ), SqlNode.Literal( 30 ) };

        sut.VisitDivide( operands[0] / operands[1] );

        sut.Nodes.Should().BeSequentiallyEqualTo( operands[0], operands[1] );
    }

    [Fact]
    public void VisitModulo_ShouldVisitOperands()
    {
        var sut = new VisitorMock();
        var operands = new[] { SqlNode.Literal( 20 ), SqlNode.Literal( 30 ) };

        sut.VisitModulo( operands[0] % operands[1] );

        sut.Nodes.Should().BeSequentiallyEqualTo( operands[0], operands[1] );
    }

    [Fact]
    public void VisitBitwiseNot_ShouldVisitValue()
    {
        var sut = new VisitorMock();
        var value = SqlNode.Literal( 20 );

        sut.VisitBitwiseNot( value.BitwiseNot() );

        sut.Nodes.Should().BeSequentiallyEqualTo( value );
    }

    [Fact]
    public void VisitBitwiseAnd_ShouldVisitOperands()
    {
        var sut = new VisitorMock();
        var operands = new[] { SqlNode.Literal( 20 ), SqlNode.Literal( 30 ) };

        sut.VisitBitwiseAnd( operands[0] & operands[1] );

        sut.Nodes.Should().BeSequentiallyEqualTo( operands[0], operands[1] );
    }

    [Fact]
    public void VisitBitwiseOr_ShouldVisitOperands()
    {
        var sut = new VisitorMock();
        var operands = new[] { SqlNode.Literal( 20 ), SqlNode.Literal( 30 ) };

        sut.VisitBitwiseOr( operands[0] | operands[1] );

        sut.Nodes.Should().BeSequentiallyEqualTo( operands[0], operands[1] );
    }

    [Fact]
    public void VisitBitwiseXor_ShouldVisitOperands()
    {
        var sut = new VisitorMock();
        var operands = new[] { SqlNode.Literal( 20 ), SqlNode.Literal( 30 ) };

        sut.VisitBitwiseXor( operands[0] ^ operands[1] );

        sut.Nodes.Should().BeSequentiallyEqualTo( operands[0], operands[1] );
    }

    [Fact]
    public void VisitBitwiseLeftShift_ShouldVisitOperands()
    {
        var sut = new VisitorMock();
        var operands = new[] { SqlNode.Literal( 20 ), SqlNode.Literal( 30 ) };

        sut.VisitBitwiseLeftShift( operands[0].BitwiseLeftShift( operands[1] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( operands[0], operands[1] );
    }

    [Fact]
    public void VisitBitwiseRightShift_ShouldVisitOperands()
    {
        var sut = new VisitorMock();
        var operands = new[] { SqlNode.Literal( 20 ), SqlNode.Literal( 30 ) };

        sut.VisitBitwiseRightShift( operands[0].BitwiseRightShift( operands[1] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( operands[0], operands[1] );
    }

    [Fact]
    public void VisitSwitchCase_ShouldVisitConditionAndExpression()
    {
        var sut = new VisitorMock();
        var condition = SqlNode.True();
        var expression = SqlNode.Literal( 20 );

        sut.VisitSwitchCase( condition.Then( expression ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( condition, expression );
    }

    [Fact]
    public void VisitSwitch_ShouldVisitCasesAndDefault()
    {
        var sut = new VisitorMock();
        var cases = new[] { SqlNode.True().Then( SqlNode.Literal( 20 ) ), SqlNode.False().Then( SqlNode.Literal( 30 ) ) };
        var @default = SqlNode.Literal( 40 );

        sut.VisitSwitch( SqlNode.Switch( cases, @default ) );

        sut.Nodes.Should()
            .BeSequentiallyEqualTo( cases[0].Condition, cases[0].Expression, cases[1].Condition, cases[1].Expression, @default );
    }

    [Fact]
    public void VisitRecordsAffectedFunction_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitRecordsAffectedFunction( SqlNode.Functions.RecordsAffected() ) );
        action.Should().NotThrow();
    }

    [Fact]
    public void VisitCoalesceFunction_ShouldVisitArguments()
    {
        var sut = new VisitorMock();
        var arguments = new[] { SqlNode.Literal( 10 ), SqlNode.Literal( 20 ) };

        sut.VisitCoalesceFunction( arguments[0].Coalesce( arguments[1] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( arguments[0], arguments[1] );
    }

    [Fact]
    public void VisitCurrentDateFunction_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitCurrentDateFunction( SqlNode.Functions.CurrentDate() ) );
        action.Should().NotThrow();
    }

    [Fact]
    public void VisitCurrentTimeFunction_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitCurrentTimeFunction( SqlNode.Functions.CurrentTime() ) );
        action.Should().NotThrow();
    }

    [Fact]
    public void VisitCurrentDateTimeFunction_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitCurrentDateTimeFunction( SqlNode.Functions.CurrentDateTime() ) );
        action.Should().NotThrow();
    }

    [Fact]
    public void VisitCurrentTimestampFunction_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitCurrentTimestampFunction( SqlNode.Functions.CurrentTimestamp() ) );
        action.Should().NotThrow();
    }

    [Fact]
    public void VisitNewGuidFunction_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitNewGuidFunction( SqlNode.Functions.NewGuid() ) );
        action.Should().NotThrow();
    }

    [Fact]
    public void VisitLengthFunction_ShouldVisitArgument()
    {
        var sut = new VisitorMock();
        var argument = SqlNode.Literal( "foo" );

        sut.VisitLengthFunction( argument.Length() );

        sut.Nodes.Should().BeSequentiallyEqualTo( argument );
    }

    [Fact]
    public void VisitToLowerFunction_ShouldVisitArgument()
    {
        var sut = new VisitorMock();
        var argument = SqlNode.Literal( "foo" );

        sut.VisitToLowerFunction( argument.ToLower() );

        sut.Nodes.Should().BeSequentiallyEqualTo( argument );
    }

    [Fact]
    public void VisitToUpperFunction_ShouldVisitArgument()
    {
        var sut = new VisitorMock();
        var argument = SqlNode.Literal( "foo" );

        sut.VisitToUpperFunction( argument.ToUpper() );

        sut.Nodes.Should().BeSequentiallyEqualTo( argument );
    }

    [Fact]
    public void VisitTrimStartFunction_ShouldVisitArguments()
    {
        var sut = new VisitorMock();
        var arguments = new[] { SqlNode.Literal( "foo" ), SqlNode.Literal( "f" ) };

        sut.VisitTrimStartFunction( arguments[0].TrimStart( arguments[1] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( arguments[0], arguments[1] );
    }

    [Fact]
    public void VisitTrimEndFunction_ShouldVisitArguments()
    {
        var sut = new VisitorMock();
        var arguments = new[] { SqlNode.Literal( "foo" ), SqlNode.Literal( "f" ) };

        sut.VisitTrimEndFunction( arguments[0].TrimEnd( arguments[1] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( arguments[0], arguments[1] );
    }

    [Fact]
    public void VisitTrimFunction_ShouldVisitArguments()
    {
        var sut = new VisitorMock();
        var arguments = new[] { SqlNode.Literal( "foo" ), SqlNode.Literal( "f" ) };

        sut.VisitTrimFunction( arguments[0].Trim( arguments[1] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( arguments[0], arguments[1] );
    }

    [Fact]
    public void VisitSubstringFunction_ShouldVisitArguments()
    {
        var sut = new VisitorMock();
        var arguments = new[] { SqlNode.Literal( "foo" ), SqlNode.Literal( 1 ), SqlNode.Literal( 2 ) };

        sut.VisitSubstringFunction( arguments[0].Substring( arguments[1], arguments[2] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( arguments[0], arguments[1], arguments[2] );
    }

    [Fact]
    public void VisitReplaceFunction_ShouldVisitArguments()
    {
        var sut = new VisitorMock();
        var arguments = new[] { SqlNode.Literal( "foo" ), SqlNode.Literal( "f" ), SqlNode.Literal( "e" ) };

        sut.VisitReplaceFunction( arguments[0].Replace( arguments[1], arguments[2] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( arguments[0], arguments[1], arguments[2] );
    }

    [Fact]
    public void VisitIndexOfFunction_ShouldVisitArguments()
    {
        var sut = new VisitorMock();
        var arguments = new[] { SqlNode.Literal( "foo" ), SqlNode.Literal( "o" ) };

        sut.VisitIndexOfFunction( arguments[0].IndexOf( arguments[1] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( arguments[0], arguments[1] );
    }

    [Fact]
    public void VisitLastIndexOfFunction_ShouldVisitArguments()
    {
        var sut = new VisitorMock();
        var arguments = new[] { SqlNode.Literal( "foo" ), SqlNode.Literal( "o" ) };

        sut.VisitLastIndexOfFunction( arguments[0].LastIndexOf( arguments[1] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( arguments[0], arguments[1] );
    }

    [Fact]
    public void VisitSignFunction_ShouldVisitArgument()
    {
        var sut = new VisitorMock();
        var argument = SqlNode.Literal( 10 );

        sut.VisitSignFunction( argument.Sign() );

        sut.Nodes.Should().BeSequentiallyEqualTo( argument );
    }

    [Fact]
    public void VisitAbsFunction_ShouldVisitArgument()
    {
        var sut = new VisitorMock();
        var argument = SqlNode.Literal( 10 );

        sut.VisitAbsFunction( argument.Abs() );

        sut.Nodes.Should().BeSequentiallyEqualTo( argument );
    }

    [Fact]
    public void VisitCeilingFunction_ShouldVisitArgument()
    {
        var sut = new VisitorMock();
        var argument = SqlNode.Literal( 10 );

        sut.VisitCeilingFunction( argument.Ceiling() );

        sut.Nodes.Should().BeSequentiallyEqualTo( argument );
    }

    [Fact]
    public void VisitFloorFunction_ShouldVisitArgument()
    {
        var sut = new VisitorMock();
        var argument = SqlNode.Literal( 10 );

        sut.VisitFloorFunction( argument.Floor() );

        sut.Nodes.Should().BeSequentiallyEqualTo( argument );
    }

    [Fact]
    public void VisitTruncateFunction_ShouldVisitArgument()
    {
        var sut = new VisitorMock();
        var argument = SqlNode.Literal( 10 );

        sut.VisitTruncateFunction( argument.Truncate() );

        sut.Nodes.Should().BeSequentiallyEqualTo( argument );
    }

    [Fact]
    public void VisitPowerFunction_ShouldVisitArguments()
    {
        var sut = new VisitorMock();
        var arguments = new[] { SqlNode.Literal( 10 ), SqlNode.Literal( 20 ) };

        sut.VisitPowerFunction( arguments[0].Power( arguments[1] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( arguments[0], arguments[1] );
    }

    [Fact]
    public void VisitSquareRootFunction_ShouldVisitArgument()
    {
        var sut = new VisitorMock();
        var argument = SqlNode.Literal( 10 );

        sut.VisitSquareRootFunction( argument.SquareRoot() );

        sut.Nodes.Should().BeSequentiallyEqualTo( argument );
    }

    [Fact]
    public void VisitMinFunction_ShouldVisitArguments()
    {
        var sut = new VisitorMock();
        var arguments = new[] { SqlNode.Literal( 10 ), SqlNode.Literal( 20 ) };

        sut.VisitMinFunction( arguments[0].Min( arguments[1] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( arguments[0], arguments[1] );
    }

    [Fact]
    public void VisitMaxFunction_ShouldVisitArguments()
    {
        var sut = new VisitorMock();
        var arguments = new[] { SqlNode.Literal( 10 ), SqlNode.Literal( 20 ) };

        sut.VisitMaxFunction( arguments[0].Max( arguments[1] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( arguments[0], arguments[1] );
    }

    [Fact]
    public void VisitCustomFunction_ShouldVisitArguments()
    {
        var sut = new VisitorMock();
        var arguments = new[] { SqlNode.Literal( 10 ), SqlNode.Literal( 20 ) };

        sut.VisitCustomFunction( arguments[0].Coalesce( arguments[1] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( arguments[0], arguments[1] );
    }

    [Fact]
    public void VisitMinAggregateFunction_ShouldVisitArgumentAndTraits()
    {
        var sut = new VisitorMock();
        var argument = SqlNode.Literal( 10 );
        var trait = SqlNode.DistinctTrait();

        sut.VisitMinAggregateFunction( argument.Min().AddTrait( trait ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( argument, trait );
    }

    [Fact]
    public void VisitMaxAggregateFunction_ShouldVisitArgumentAndTraits()
    {
        var sut = new VisitorMock();
        var argument = SqlNode.Literal( 10 );
        var trait = SqlNode.DistinctTrait();

        sut.VisitMaxAggregateFunction( argument.Max().AddTrait( trait ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( argument, trait );
    }

    [Fact]
    public void VisitAverageAggregateFunction_ShouldVisitArgumentAndTraits()
    {
        var sut = new VisitorMock();
        var argument = SqlNode.Literal( 10 );
        var trait = SqlNode.DistinctTrait();

        sut.VisitAverageAggregateFunction( argument.Average().AddTrait( trait ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( argument, trait );
    }

    [Fact]
    public void VisitSumAggregateFunction_ShouldVisitArgumentAndTraits()
    {
        var sut = new VisitorMock();
        var argument = SqlNode.Literal( 10 );
        var trait = SqlNode.DistinctTrait();

        sut.VisitSumAggregateFunction( argument.Sum().AddTrait( trait ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( argument, trait );
    }

    [Fact]
    public void VisitCountAggregateFunction_ShouldVisitArgumentAndTraits()
    {
        var sut = new VisitorMock();
        var argument = SqlNode.Literal( 10 );
        var trait = SqlNode.DistinctTrait();

        sut.VisitCountAggregateFunction( argument.Count().AddTrait( trait ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( argument, trait );
    }

    [Fact]
    public void VisitStringConcatAggregateFunction_ShouldVisitArgumentsAndTraits()
    {
        var sut = new VisitorMock();
        var arguments = new[] { SqlNode.Literal( "foo" ), SqlNode.Literal( ";" ) };
        var trait = SqlNode.DistinctTrait();

        sut.VisitStringConcatAggregateFunction( arguments[0].StringConcat( arguments[1] ).AddTrait( trait ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( arguments[0], arguments[1], trait );
    }

    [Fact]
    public void VisitCustomAggregateFunction_ShouldVisitArgumentsAndTraits()
    {
        var sut = new VisitorMock();
        var arguments = new[] { SqlNode.Literal( "foo" ), SqlNode.Literal( ";" ) };
        var trait = SqlNode.DistinctTrait();

        sut.VisitCustomAggregateFunction( arguments[0].StringConcat( arguments[1] ).AddTrait( trait ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( arguments[0], arguments[1], trait );
    }

    [Fact]
    public void VisitRawCondition_ShouldVisitParameters()
    {
        var sut = new VisitorMock();
        var parameters = new[] { SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) };

        sut.VisitRawCondition( SqlNode.RawCondition( "@a > @b", parameters ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( parameters[0], parameters[1] );
    }

    [Fact]
    public void VisitTrue_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitTrue( SqlNode.True() ) );
        action.Should().NotThrow();
    }

    [Fact]
    public void VisitFalse_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitFalse( SqlNode.False() ) );
        action.Should().NotThrow();
    }

    [Fact]
    public void VisitEqualTo_ShouldVisitOperands()
    {
        var sut = new VisitorMock();
        var operands = new[] { SqlNode.Literal( 20 ), SqlNode.Literal( 30 ) };

        sut.VisitEqualTo( operands[0].IsEqualTo( operands[1] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( operands[0], operands[1] );
    }

    [Fact]
    public void VisitNotEqualTo_ShouldVisitOperands()
    {
        var sut = new VisitorMock();
        var operands = new[] { SqlNode.Literal( 20 ), SqlNode.Literal( 30 ) };

        sut.VisitNotEqualTo( operands[0].IsNotEqualTo( operands[1] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( operands[0], operands[1] );
    }

    [Fact]
    public void VisitGreaterThan_ShouldVisitOperands()
    {
        var sut = new VisitorMock();
        var operands = new[] { SqlNode.Literal( 20 ), SqlNode.Literal( 30 ) };

        sut.VisitGreaterThan( operands[0].IsGreaterThan( operands[1] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( operands[0], operands[1] );
    }

    [Fact]
    public void VisitLessThan_ShouldVisitOperands()
    {
        var sut = new VisitorMock();
        var operands = new[] { SqlNode.Literal( 20 ), SqlNode.Literal( 30 ) };

        sut.VisitLessThan( operands[0].IsLessThan( operands[1] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( operands[0], operands[1] );
    }

    [Fact]
    public void VisitGreaterThanOrEqualTo_ShouldVisitOperands()
    {
        var sut = new VisitorMock();
        var operands = new[] { SqlNode.Literal( 20 ), SqlNode.Literal( 30 ) };

        sut.VisitGreaterThanOrEqualTo( operands[0].IsGreaterThanOrEqualTo( operands[1] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( operands[0], operands[1] );
    }

    [Fact]
    public void VisitLessThanOrEqualTo_ShouldVisitOperands()
    {
        var sut = new VisitorMock();
        var operands = new[] { SqlNode.Literal( 20 ), SqlNode.Literal( 30 ) };

        sut.VisitLessThanOrEqualTo( operands[0].IsLessThanOrEqualTo( operands[1] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( operands[0], operands[1] );
    }

    [Fact]
    public void VisitAnd_ShouldVisitOperands()
    {
        var sut = new VisitorMock();
        var operands = new SqlConditionNode[] { SqlNode.True(), SqlNode.False() };

        sut.VisitAnd( operands[0].And( operands[1] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( operands[0], operands[1] );
    }

    [Fact]
    public void VisitOr_ShouldVisitOperands()
    {
        var sut = new VisitorMock();
        var operands = new SqlConditionNode[] { SqlNode.True(), SqlNode.False() };

        sut.VisitOr( operands[0].Or( operands[1] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( operands[0], operands[1] );
    }

    [Fact]
    public void VisitConditionValue_ShouldVisitCondition()
    {
        var sut = new VisitorMock();
        var condition = SqlNode.True();

        sut.VisitConditionValue( condition.ToValue() );

        sut.Nodes.Should().BeSequentiallyEqualTo( condition );
    }

    [Fact]
    public void VisitBetween_ShouldVisitValueAndMinAndMax()
    {
        var sut = new VisitorMock();
        var operands = new[] { SqlNode.Literal( 30 ), SqlNode.Literal( 20 ), SqlNode.Literal( 40 ) };

        sut.VisitBetween( operands[0].IsBetween( operands[1], operands[2] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( operands[0], operands[1], operands[2] );
    }

    [Fact]
    public void VisitExists_ShouldVisitQuery()
    {
        var sut = new VisitorMock();
        var query = SqlNode.RawQuery( "SELECT * FROM foo WHERE foo.a > @a", SqlNode.Parameter( "a" ) );

        sut.VisitExists( query.Exists() );

        sut.Nodes.Should().BeSequentiallyEqualTo( query.Parameters.Span[0] );
    }

    [Fact]
    public void VisitLike_ShouldVisitValueAndPatternAndEscape()
    {
        var sut = new VisitorMock();
        var operands = new[] { SqlNode.Literal( "foo" ), SqlNode.Literal( "f" ), SqlNode.Literal( "\\" ) };

        sut.VisitLike( operands[0].Like( operands[1], operands[2] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( operands[0], operands[1], operands[2] );
    }

    [Fact]
    public void VisitIn_ShouldVisitValueAndExpressions()
    {
        var sut = new VisitorMock();
        var operands = new[] { SqlNode.Literal( 10 ), SqlNode.Literal( 20 ), SqlNode.Literal( 30 ) };

        sut.VisitIn( (SqlInConditionNode)operands[0].In( operands[1], operands[2] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( operands[0], operands[1], operands[2] );
    }

    [Fact]
    public void VisitInQuery_ShouldVisitValueAndQuery()
    {
        var sut = new VisitorMock();
        var value = SqlNode.Literal( 10 );
        var query = SqlNode.RawQuery( "SELECT * FROM foo WHERE foo.a > @a", SqlNode.Parameter( "a" ) );

        sut.VisitInQuery( value.InQuery( query ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( value, query.Parameters.Span[0] );
    }

    [Fact]
    public void VisitRawRecordSet_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitRawRecordSet( SqlNode.RawRecordSet( "foo" ) ) );
        action.Should().NotThrow();
    }

    [Fact]
    public void VisitTable_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitTable( TableMock.Create( "foo" ).ToRecordSet() ) );
        action.Should().NotThrow();
    }

    [Fact]
    public void VisitTableBuilder_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitTableBuilder( TableMock.CreateBuilder( "foo" ).ToRecordSet() ) );
        action.Should().NotThrow();
    }

    [Fact]
    public void VisitView_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitView( ViewMock.Create( "foo" ).ToRecordSet() ) );
        action.Should().NotThrow();
    }

    [Fact]
    public void VisitViewBuilder_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitViewBuilder( ViewMock.CreateBuilder( "foo" ).ToRecordSet() ) );
        action.Should().NotThrow();
    }

    [Fact]
    public void VisitQueryRecordSet_ShouldVisitQuery()
    {
        var sut = new VisitorMock();
        var query = SqlNode.RawQuery( "SELECT * FROM foo WHERE foo.a > @a", SqlNode.Parameter( "a" ) );

        sut.VisitQueryRecordSet( query.AsSet( "bar" ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( query.Parameters.Span[0] );
    }

    [Fact]
    public void VisitCommonTableExpressionRecordSet_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of(
            () => sut.VisitCommonTableExpressionRecordSet( SqlNode.RawQuery( "SELECT * FROM foo" ).ToCte( "bar" ).RecordSet ) );

        action.Should().NotThrow();
    }

    [Fact]
    public void VisitNewTable_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of(
            () => sut.VisitNewTable(
                SqlNode.CreateTable(
                        SqlRecordSetInfo.Create( "foo" ),
                        new[] { SqlNode.Column<int>( "a", defaultValue: SqlNode.Parameter( "a" ) ) } )
                    .AsSet( "bar" ) ) );

        action.Should().NotThrow();
    }

    [Fact]
    public void VisitNewView_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of(
            () => sut.VisitNewView( SqlNode.RawQuery( "SELECT * FROM foo" ).ToCreateView( SqlRecordSetInfo.Create( "foo" ) ).AsSet() ) );

        action.Should().NotThrow();
    }

    [Fact]
    public void VisitJoinOn_ShouldVisitInnerRecordSetAndOnExpression()
    {
        var sut = new VisitorMock();
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var condition = SqlNode.True();

        sut.VisitJoinOn( recordSet.InnerOn( condition ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( recordSet, condition );
    }

    [Fact]
    public void VisitDataSource_ShouldVisitFromAndJoinsAndTraits()
    {
        var sut = new VisitorMock();
        var from = SqlNode.RawRecordSet( "foo" );
        var join = SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() );
        var trait = SqlNode.DistinctTrait();

        sut.VisitDataSource( from.Join( join ).AddTrait( trait ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( from, join.InnerRecordSet, join.OnExpression, trait );
    }

    [Fact]
    public void VisitDataSource_ShouldVisitTraits_WhenDataSourceIsDummy()
    {
        var sut = new VisitorMock();
        var trait = SqlNode.DistinctTrait();

        sut.VisitDataSource( SqlNode.DummyDataSource().AddTrait( trait ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( trait );
    }

    [Fact]
    public void VisitSelectField_ShouldVisitExpression()
    {
        var sut = new VisitorMock();
        var expression = SqlNode.Literal( 10 );

        sut.VisitSelectField( expression.As( "x" ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( expression );
    }

    [Fact]
    public void VisitSelectCompoundField_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of(
            () => sut.VisitSelectCompoundField(
                (SqlSelectCompoundFieldNode)SqlNode.RawRecordSet( "foo" )
                    .ToDataSource()
                    .Select( x => new[] { x.From["x"].AsSelf() } )
                    .CompoundWith(
                        SqlNode.RawRecordSet( "bar" ).ToDataSource().Select( x => new[] { x.From["x"].AsSelf() } ).ToUnion() )
                    .Selection.Span[0] ) );

        action.Should().NotThrow();
    }

    [Fact]
    public void VisitSelectRecordSet_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitSelectRecordSet( SqlNode.RawRecordSet( "foo" ).GetAll() ) );
        action.Should().NotThrow();
    }

    [Fact]
    public void VisitSelectAll_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitSelectAll( SqlNode.RawRecordSet( "foo" ).ToDataSource().GetAll() ) );
        action.Should().NotThrow();
    }

    [Fact]
    public void VisitSelectExpression_ShouldVisitSelection()
    {
        var sut = new VisitorMock();
        var selection = SqlNode.RawRecordSet( "foo" ).ToDataSource().GetAll();

        sut.VisitSelectExpression( selection.ToExpression() );

        sut.Nodes.Should().BeSequentiallyEqualTo( selection );
    }

    [Fact]
    public void VisitRawQuery_ShouldVisitParameters()
    {
        var sut = new VisitorMock();
        var parameters = new[] { SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) };

        sut.VisitRawQuery( SqlNode.RawQuery( "SELECT * FROM foo WHERE @a > @b", parameters ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( parameters[0], parameters[1] );
    }

    [Fact]
    public void VisitDataSourceQuery_ShouldVisitSelectionAndDataSourceAndTraits()
    {
        var sut = new VisitorMock();
        var from = SqlNode.RawRecordSet( "foo" );
        var join = SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() );
        var trait = SqlNode.DistinctTrait();
        var dataSource = from.Join( join );
        var selection = dataSource.GetAll();

        sut.VisitDataSourceQuery( dataSource.Select( selection ).AddTrait( trait ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( selection, from, join.InnerRecordSet, join.OnExpression, trait );
    }

    [Fact]
    public void VisitCompoundQuery_ShouldVisitSelectionAndFirstQueryAndFollowingQueriesAndTraits()
    {
        var sut = new VisitorMock();
        var firstRecordSet = SqlNode.RawRecordSet( "foo" );
        var secondRecordSet = SqlNode.RawRecordSet( "bar" );
        var firstSelection = firstRecordSet["x"].AsSelf();
        var secondSelection = secondRecordSet["x"].AsSelf();
        var trait = SqlNode.DistinctTrait();
        var query = firstRecordSet.ToDataSource()
            .Select( firstSelection )
            .CompoundWith( secondRecordSet.ToDataSource().Select( secondSelection ).ToUnion() )
            .AddTrait( trait );

        sut.VisitCompoundQuery( query );

        sut.Nodes.Should()
            .BeSequentiallyEqualTo( query.Selection.Span[0], firstRecordSet, firstRecordSet, secondRecordSet, secondRecordSet, trait );
    }

    [Fact]
    public void VisitCompoundQueryComponent_ShouldVisitQuery()
    {
        var sut = new VisitorMock();
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var selection = recordSet.GetAll();
        var trait = SqlNode.DistinctTrait();

        sut.VisitCompoundQueryComponent( recordSet.ToDataSource().Select( selection ).AddTrait( trait ).ToUnion() );

        sut.Nodes.Should().BeSequentiallyEqualTo( selection, recordSet, trait );
    }

    [Fact]
    public void VisitDistinctTrait_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitDistinctTrait( SqlNode.DistinctTrait() ) );
        action.Should().NotThrow();
    }

    [Fact]
    public void VisitFilterTrait_ShouldVisitFilter()
    {
        var sut = new VisitorMock();
        var filter = SqlNode.True();

        sut.VisitFilterTrait( SqlNode.FilterTrait( filter, isConjunction: true ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( filter );
    }

    [Fact]
    public void VisitAggregationTrait_ShouldVisitExpressions()
    {
        var sut = new VisitorMock();
        var expressions = new[] { SqlNode.Literal( 10 ), SqlNode.Literal( 20 ) };

        sut.VisitAggregationTrait( SqlNode.AggregationTrait( expressions ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( expressions[0], expressions[1] );
    }

    [Fact]
    public void VisitAggregationFilterTrait_ShouldVisitFilter()
    {
        var sut = new VisitorMock();
        var filter = SqlNode.True();

        sut.VisitAggregationFilterTrait( SqlNode.AggregationFilterTrait( filter, isConjunction: true ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( filter );
    }

    [Fact]
    public void VisitSortTrait_ShouldVisitOrdering()
    {
        var sut = new VisitorMock();
        var ordering = new[] { SqlNode.Literal( 10 ).Asc(), SqlNode.Literal( 20 ).Desc() };

        sut.VisitSortTrait( SqlNode.SortTrait( ordering ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( ordering[0].Expression, ordering[1].Expression );
    }

    [Fact]
    public void VisitLimitTrait_ShouldVisitValue()
    {
        var sut = new VisitorMock();
        var value = SqlNode.Literal( 20 );

        sut.VisitLimitTrait( SqlNode.LimitTrait( value ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( value );
    }

    [Fact]
    public void VisitOffsetTrait_ShouldVisitValue()
    {
        var sut = new VisitorMock();
        var value = SqlNode.Literal( 20 );

        sut.VisitOffsetTrait( SqlNode.OffsetTrait( value ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( value );
    }

    [Fact]
    public void VisitCommonTableExpressionTrait_ShouldVisitCommonTableExpressions()
    {
        var sut = new VisitorMock();
        var queries = new[]
        {
            SqlNode.RawQuery( "SELECT * FROM foo", SqlNode.Parameter( "a" ) ),
            SqlNode.RawQuery( "SELECT * FROM bar", SqlNode.Parameter( "b" ) )
        };

        sut.VisitCommonTableExpressionTrait( SqlNode.CommonTableExpressionTrait( queries[0].ToCte( "X" ), queries[1].ToCte( "Y" ) ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( queries[0].Parameters.Span[0], queries[1].Parameters.Span[0] );
    }

    [Fact]
    public void VisitOrderBy_ShouldVisitExpression()
    {
        var sut = new VisitorMock();
        var expression = SqlNode.Literal( 20 );

        sut.VisitOrderBy( expression.Asc() );

        sut.Nodes.Should().BeSequentiallyEqualTo( expression );
    }

    [Fact]
    public void VisitCommonTableExpression_ShouldVisitQuery()
    {
        var sut = new VisitorMock();
        var query = SqlNode.RawQuery( "SELECT * FROM foo", SqlNode.Parameter( "a" ) );

        sut.VisitCommonTableExpression( query.ToCte( "X" ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( query.Parameters.Span[0] );
    }

    [Fact]
    public void VisitTypeCast_ShouldVisitValue()
    {
        var sut = new VisitorMock();
        var expression = SqlNode.Literal( 20 );

        sut.VisitTypeCast( expression.CastTo<int>() );

        sut.Nodes.Should().BeSequentiallyEqualTo( expression );
    }

    [Fact]
    public void VisitValues_ShouldVisitExpressions()
    {
        var sut = new VisitorMock();
        var expressions = new[,] { { SqlNode.Literal( 10 ), SqlNode.Literal( 20 ) }, { SqlNode.Literal( 30 ), SqlNode.Literal( 40 ) } };

        sut.VisitValues( SqlNode.Values( expressions ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( expressions[0, 0], expressions[0, 1], expressions[1, 0], expressions[1, 1] );
    }

    [Fact]
    public void VisitRawStatement_ShouldVisitParameters()
    {
        var sut = new VisitorMock();
        var parameters = new[] { SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) };

        sut.VisitRawStatement( SqlNode.RawStatement( "INSERT INTO foo (a, b) VALUES (@a, @b)", parameters ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( parameters[0], parameters[1] );
    }

    [Fact]
    public void VisitInsertInto_ShouldVisitRecordSetAndDataFieldsAndSource()
    {
        var sut = new VisitorMock();
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var dataFields = new[] { recordSet["x"], recordSet["y"] };
        var values = new[,] { { SqlNode.Literal( 10 ), SqlNode.Literal( 20 ) }, { SqlNode.Literal( 30 ), SqlNode.Literal( 40 ) } };

        sut.VisitInsertInto( SqlNode.Values( values ).ToInsertInto( recordSet, dataFields[0], dataFields[1] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( recordSet, recordSet, recordSet, values[0, 0], values[0, 1], values[1, 0], values[1, 1] );
    }

    [Fact]
    public void VisitUpdate_ShouldVisitAssignmentsAndDataSource()
    {
        var sut = new VisitorMock();
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var values = new[] { SqlNode.Literal( 10 ), SqlNode.Literal( 20 ) };

        sut.VisitUpdate( recordSet.ToDataSource().ToUpdate( recordSet["x"].Assign( values[0] ), recordSet["y"].Assign( values[1] ) ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( recordSet, values[0], recordSet, values[1], recordSet );
    }

    [Fact]
    public void VisitValueAssignment_ShouldVisitDataFieldAndValue()
    {
        var sut = new VisitorMock();
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var value = SqlNode.Literal( 10 );

        sut.VisitValueAssignment( recordSet["x"].Assign( value ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( recordSet, value );
    }

    [Fact]
    public void VisitDeleteFrom_ShouldVisitDataSource()
    {
        var sut = new VisitorMock();
        var recordSet = SqlNode.RawRecordSet( "foo" );

        sut.VisitDeleteFrom( recordSet.ToDataSource().ToDeleteFrom() );

        sut.Nodes.Should().BeSequentiallyEqualTo( recordSet );
    }

    [Fact]
    public void VisitTruncate_ShouldVisitTable()
    {
        var sut = new VisitorMock();
        var recordSet = SqlNode.RawRecordSet( "foo" );

        sut.VisitTruncate( recordSet.ToTruncate() );

        sut.Nodes.Should().BeSequentiallyEqualTo( recordSet );
    }

    [Fact]
    public void VisitColumnDefinition_ShouldVisitDefaultValue()
    {
        var sut = new VisitorMock();
        var parameter = SqlNode.Parameter( "a" );
        var column = SqlNode.Column<int>( "a", defaultValue: parameter );

        sut.VisitColumnDefinition( column );

        sut.Nodes.Should().BeSequentiallyEqualTo( parameter );
    }

    [Fact]
    public void VisitPrimaryKeyDefinition_ShouldVisitColumns()
    {
        var sut = new VisitorMock();
        var table = SqlNode.RawRecordSet( "foo" );
        var columns = new[] { table["a"], table["b"] };

        sut.VisitPrimaryKeyDefinition( SqlNode.PrimaryKey( "PK", columns[0].Asc(), columns[1].Desc() ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( table, table );
    }

    [Fact]
    public void VisitForeignKeyDefinition_ShouldVisitColumnsAndReferencedObjects()
    {
        var sut = new VisitorMock();
        var table = SqlNode.RawRecordSet( "foo" );
        var columns = new SqlDataFieldNode[] { table["a"], table["b"] };
        var otherTable = SqlNode.RawRecordSet( "bar" );
        var otherColumns = new SqlDataFieldNode[] { otherTable["a"], otherTable["b"] };

        sut.VisitForeignKeyDefinition( SqlNode.ForeignKey( "FK", columns, otherTable, otherColumns ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( table, table, otherTable, otherTable, otherTable );
    }

    [Fact]
    public void VisitCheckDefinition_ShouldVisitPredicate()
    {
        var sut = new VisitorMock();
        var parameter = SqlNode.Parameter( "a" );

        sut.VisitCheckDefinition( SqlNode.Check( "CHK", SqlNode.RawCondition( "a > @a", parameter ) ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( parameter );
    }

    [Fact]
    public void VisitCreateTable_ShouldVisitColumnsAndConstraints()
    {
        var sut = new VisitorMock();
        var otherTable = SqlNode.RawRecordSet( "bar" );
        var referencedColumn = otherTable["b"];
        var parameters = new[] { SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ), SqlNode.Parameter( "c" ) };

        var table = SqlNode.CreateTable(
            SqlRecordSetInfo.Create( "foo" ),
            new[]
            {
                SqlNode.Column<int>( "a", defaultValue: parameters[0] ),
                SqlNode.Column<int>( "b", defaultValue: parameters[1] )
            },
            constraintsProvider: t =>
                SqlCreateTableConstraints.Empty
                    .WithPrimaryKey( SqlNode.PrimaryKey( "PK", t["a"].Asc() ) )
                    .WithForeignKeys(
                        SqlNode.ForeignKey(
                            "FK",
                            new SqlDataFieldNode[] { t["b"] },
                            otherTable,
                            new SqlDataFieldNode[] { referencedColumn } ) )
                    .WithChecks( SqlNode.Check( "CHK", SqlNode.RawCondition( "a > @a", parameters[2] ) ) ) );

        sut.VisitCreateTable( table );

        sut.Nodes.Should()
            .BeSequentiallyEqualTo(
                parameters[0],
                parameters[1],
                table.RecordSet,
                table.RecordSet,
                otherTable,
                otherTable,
                parameters[2] );
    }

    [Fact]
    public void VisitCreateView_ShouldVisitSource()
    {
        var sut = new VisitorMock();
        var parameter = SqlNode.Parameter( "a" );
        var view = SqlNode.CreateView( SqlRecordSetInfo.Create( "V" ), SqlNode.RawQuery( "SELECT * FROM foo WHERE a > @a", parameter ) );

        sut.VisitCreateView( view );

        sut.Nodes.Should().BeSequentiallyEqualTo( parameter );
    }

    [Fact]
    public void VisitCreateIndex_ShouldVisitTableAndColumnsAndFilter()
    {
        var sut = new VisitorMock();
        var table = SqlNode.RawRecordSet( "foo" );
        var parameter = SqlNode.Parameter( "a" );

        var index = SqlNode.CreateIndex(
            SqlSchemaObjectName.Create( "IX" ),
            isUnique: Fixture.Create<bool>(),
            table,
            new[] { table["a"].Asc(), table["b"].Asc() },
            filter: SqlNode.RawCondition( "a > @a", parameter ) );

        sut.VisitCreateIndex( index );

        sut.Nodes.Should().BeSequentiallyEqualTo( table, table, table, parameter );
    }

    [Fact]
    public void VisitRenameTable_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of(
            () => sut.VisitRenameTable( SqlNode.RenameTable( SqlRecordSetInfo.Create( "a" ), SqlSchemaObjectName.Create( "b" ) ) ) );

        action.Should().NotThrow();
    }

    [Fact]
    public void VisitRenameColumn_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitRenameColumn( SqlNode.RenameColumn( SqlRecordSetInfo.Create( "a" ), "b", "c" ) ) );
        action.Should().NotThrow();
    }

    [Fact]
    public void VisitAddColumn_ShouldVisitDefinition()
    {
        var sut = new VisitorMock();
        var defaultValue = SqlNode.Literal( 10 );

        sut.VisitAddColumn( SqlNode.AddColumn( SqlRecordSetInfo.Create( "a" ), SqlNode.Column<int>( "b", defaultValue: defaultValue ) ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( defaultValue );
    }

    [Fact]
    public void VisitDropColumn_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitDropColumn( SqlNode.DropColumn( SqlRecordSetInfo.Create( "a" ), "b" ) ) );
        action.Should().NotThrow();
    }

    [Fact]
    public void VisitDropTable_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitDropTable( SqlNode.DropTable( SqlRecordSetInfo.Create( "a" ) ) ) );
        action.Should().NotThrow();
    }

    [Fact]
    public void VisitDropView_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitDropView( SqlNode.DropView( SqlRecordSetInfo.Create( "a" ) ) ) );
        action.Should().NotThrow();
    }

    [Fact]
    public void VisitDropIndex_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitDropIndex( SqlNode.DropIndex( SqlSchemaObjectName.Create( "a" ) ) ) );
        action.Should().NotThrow();
    }

    [Fact]
    public void VisitStatementBatch_ShouldVisitStatements()
    {
        var sut = new VisitorMock();
        var parameters = new[] { SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) };
        var statements = new[]
        {
            SqlNode.RawQuery( "SELECT * FROM foo WHERE a > @a", parameters[0] ),
            SqlNode.RawQuery( "SELECT * FROM bar WHERE b < @b", parameters[1] )
        };

        sut.VisitStatementBatch( SqlNode.Batch( statements[0], statements[1] ) );

        sut.Nodes.Should().BeSequentiallyEqualTo( parameters[0], parameters[1] );
    }

    [Fact]
    public void VisitBeginTransaction_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitBeginTransaction( SqlNode.BeginTransaction( IsolationLevel.Serializable ) ) );
        action.Should().NotThrow();
    }

    [Fact]
    public void VisitCommitTransaction_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitCommitTransaction( SqlNode.CommitTransaction() ) );
        action.Should().NotThrow();
    }

    [Fact]
    public void VisitRollbackTransaction_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitRollbackTransaction( SqlNode.RollbackTransaction() ) );
        action.Should().NotThrow();
    }

    [Fact]
    public void VisitCustom_ShouldDoNothing()
    {
        var sut = new Visitor();
        var action = Lambda.Of( () => sut.VisitCustom( SqlNode.RollbackTransaction() ) );
        action.Should().NotThrow();
    }

    private sealed class Visitor : SqlNodeVisitor { }

    private sealed class VisitorMock : SqlNodeVisitor
    {
        public readonly List<SqlNodeBase> Nodes = new List<SqlNodeBase>();

        public override void VisitNull(SqlNullNode node)
        {
            base.VisitNull( node );
            Nodes.Add( node );
        }

        public override void VisitLiteral(SqlLiteralNode node)
        {
            base.VisitLiteral( node );
            Nodes.Add( node );
        }

        public override void VisitParameter(SqlParameterNode node)
        {
            base.VisitParameter( node );
            Nodes.Add( node );
        }

        public override void VisitRecordsAffectedFunction(SqlRecordsAffectedFunctionExpressionNode node)
        {
            base.VisitRecordsAffectedFunction( node );
            Nodes.Add( node );
        }

        public override void VisitCurrentDateFunction(SqlCurrentDateFunctionExpressionNode node)
        {
            base.VisitCurrentDateFunction( node );
            Nodes.Add( node );
        }

        public override void VisitCurrentTimeFunction(SqlCurrentTimeFunctionExpressionNode node)
        {
            base.VisitCurrentTimeFunction( node );
            Nodes.Add( node );
        }

        public override void VisitCurrentDateTimeFunction(SqlCurrentDateTimeFunctionExpressionNode node)
        {
            base.VisitCurrentDateTimeFunction( node );
            Nodes.Add( node );
        }

        public override void VisitCurrentTimestampFunction(SqlCurrentTimestampFunctionExpressionNode node)
        {
            base.VisitCurrentTimestampFunction( node );
            Nodes.Add( node );
        }

        public override void VisitNewGuidFunction(SqlNewGuidFunctionExpressionNode node)
        {
            base.VisitNewGuidFunction( node );
            Nodes.Add( node );
        }

        public override void VisitTrue(SqlTrueNode node)
        {
            base.VisitTrue( node );
            Nodes.Add( node );
        }

        public override void VisitFalse(SqlFalseNode node)
        {
            base.VisitFalse( node );
            Nodes.Add( node );
        }

        public override void VisitRawRecordSet(SqlRawRecordSetNode node)
        {
            base.VisitRawRecordSet( node );
            Nodes.Add( node );
        }

        public override void VisitTable(SqlTableNode node)
        {
            base.VisitTable( node );
            Nodes.Add( node );
        }

        public override void VisitTableBuilder(SqlTableBuilderNode node)
        {
            base.VisitTableBuilder( node );
            Nodes.Add( node );
        }

        public override void VisitView(SqlViewNode node)
        {
            base.VisitView( node );
            Nodes.Add( node );
        }

        public override void VisitViewBuilder(SqlViewBuilderNode node)
        {
            base.VisitViewBuilder( node );
            Nodes.Add( node );
        }

        public override void VisitCommonTableExpressionRecordSet(SqlCommonTableExpressionRecordSetNode node)
        {
            base.VisitCommonTableExpressionRecordSet( node );
            Nodes.Add( node );
        }

        public override void VisitNewTable(SqlNewTableNode node)
        {
            base.VisitNewTable( node );
            Nodes.Add( node );
        }

        public override void VisitSelectCompoundField(SqlSelectCompoundFieldNode node)
        {
            base.VisitSelectCompoundField( node );
            Nodes.Add( node );
        }

        public override void VisitSelectRecordSet(SqlSelectRecordSetNode node)
        {
            base.VisitSelectRecordSet( node );
            Nodes.Add( node );
        }

        public override void VisitSelectAll(SqlSelectAllNode node)
        {
            base.VisitSelectAll( node );
            Nodes.Add( node );
        }

        public override void VisitDistinctTrait(SqlDistinctTraitNode node)
        {
            base.VisitDistinctTrait( node );
            Nodes.Add( node );
        }

        public override void VisitDropTable(SqlDropTableNode node)
        {
            base.VisitDropTable( node );
            Nodes.Add( node );
        }

        public override void VisitDropView(SqlDropViewNode node)
        {
            base.VisitDropView( node );
            Nodes.Add( node );
        }

        public override void VisitDropIndex(SqlDropIndexNode node)
        {
            base.VisitDropIndex( node );
            Nodes.Add( node );
        }

        public override void VisitBeginTransaction(SqlBeginTransactionNode node)
        {
            base.VisitBeginTransaction( node );
            Nodes.Add( node );
        }

        public override void VisitCommitTransaction(SqlCommitTransactionNode node)
        {
            base.VisitCommitTransaction( node );
            Nodes.Add( node );
        }

        public override void VisitRollbackTransaction(SqlRollbackTransactionNode node)
        {
            base.VisitRollbackTransaction( node );
            Nodes.Add( node );
        }

        public override void VisitCustom(SqlNodeBase node)
        {
            base.VisitCustom( node );
            Nodes.Add( node );
        }
    }
}
