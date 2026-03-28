using System.Linq;
using System.Text;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public class SqlNodeInterpreterContextTests : TestsBase
{
    [Fact]
    public void Create_ShouldCreateWithStringBuilderWithDefaultCapacity()
    {
        var sut = SqlNodeInterpreterContext.Create();

        Assertion.All(
                sut.Indent.TestEquals( 0 ),
                sut.ChildDepth.TestEquals( 0 ),
                sut.Parameters.TestEmpty(),
                sut.Sql.Length.TestEquals( 0 ),
                sut.Sql.Capacity.TestEquals( 1024 ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldCreateWithExplicitStringBuilder()
    {
        var builder = new StringBuilder();
        var sut = SqlNodeInterpreterContext.Create( builder );

        Assertion.All(
                sut.Indent.TestEquals( 0 ),
                sut.ChildDepth.TestEquals( 0 ),
                sut.Parameters.TestEmpty(),
                sut.Sql.TestRefEquals( builder ) )
            .Go();
    }

    [Fact]
    public void IncreaseIndent_ShouldSetIndentationToTwoSpaces_WhenCurrentIndentIsEmpty()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.IncreaseIndent();
        sut.Indent.TestEquals( 2 ).Go();
    }

    [Fact]
    public void IncreaseIndent_ShouldAddTwoSpacesToCurrentNonEmptyIndent()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.IncreaseIndent();

        sut.IncreaseIndent();

        sut.Indent.TestEquals( 4 ).Go();
    }

    [Fact]
    public void DecreaseIndent_ShouldDoNothing_WhenCurrentIndentIsEmpty()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.DecreaseIndent();
        sut.Indent.TestEquals( 0 ).Go();
    }

    [Fact]
    public void DecreaseIndent_ShouldSetIndentToEmpty_WhenCurrentIndentIsEqualToTwoSpaces()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.IncreaseIndent();

        sut.DecreaseIndent();

        sut.Indent.TestEquals( 0 ).Go();
    }

    [Fact]
    public void DecreaseIndent_ShouldRemoveTwoSpacesFromCurrentIndentation_WhenCurrentIndentIsGreaterThanTwoSpaces()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.IncreaseIndent();
        sut.IncreaseIndent();

        sut.DecreaseIndent();

        sut.Indent.TestEquals( 2 ).Go();
    }

    [Fact]
    public void TempIndentIncrease_ShouldIncreaseIndentByTwoSpacesAndReturnObjectThatReducesIndentByTwoSpacesWhenDisposed()
    {
        var sut = SqlNodeInterpreterContext.Create();

        int indent;
        using ( sut.TempIndentIncrease() )
            indent = sut.Indent;

        Assertion.All(
                sut.Indent.TestEquals( 0 ),
                indent.TestEquals( 2 ) )
            .Go();
    }

    [Fact]
    public void IncreaseChildDepth_ShouldSetChildDepthToOne_WhenCurrentChildDepthIsZero()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.IncreaseChildDepth();
        sut.ChildDepth.TestEquals( 1 ).Go();
    }

    [Fact]
    public void IncreaseChildDepth_ShouldIncrementCurrentChildDepth()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.IncreaseChildDepth();

        sut.IncreaseChildDepth();

        sut.ChildDepth.TestEquals( 2 ).Go();
    }

    [Fact]
    public void DecreaseChildDepth_ShouldDoNothing_WhenCurrentChildDepthIsZero()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.DecreaseChildDepth();
        sut.ChildDepth.TestEquals( 0 ).Go();
    }

    [Fact]
    public void DecreaseChildDepth_ShouldSetChildDepthToZero_WhenCurrentChildDepthIsEqualToOn()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.IncreaseChildDepth();

        sut.DecreaseChildDepth();

        sut.ChildDepth.TestEquals( 0 ).Go();
    }

    [Fact]
    public void DecreaseChildDepth_ShouldRemoveDecrementCurrentChildDepth_WhenCurrentChildDepthIsGreaterThanZero()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.IncreaseChildDepth();
        sut.IncreaseChildDepth();

        sut.DecreaseChildDepth();

        sut.ChildDepth.TestEquals( 1 ).Go();
    }

    [Fact]
    public void TempChildDepthIncrease_ShouldIncrementChildDepthAndReturnObjectThatDecrementsChildDepthWhenDisposed()
    {
        var sut = SqlNodeInterpreterContext.Create();

        int depth;
        using ( sut.TempChildDepthIncrease() )
            depth = sut.ChildDepth;

        Assertion.All(
                sut.ChildDepth.TestEquals( 0 ),
                depth.TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void AppendIndent_ShouldAppendNewLineWithCurrentIndentToStringBuilder()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.IncreaseIndent();

        var result = sut.AppendIndent();

        Assertion.All(
                result.TestRefEquals( sut.Sql ),
                result.ToString().TestEquals( $"{Environment.NewLine}  " ) )
            .Go();
    }

    [Fact]
    public void AppendShortIndent_ShouldAppendNewLineWithCurrentIndentReducedByTwoSpacesToStringBuilder()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.IncreaseIndent();
        sut.IncreaseIndent();

        var result = sut.AppendShortIndent();

        Assertion.All(
                result.TestRefEquals( sut.Sql ),
                result.ToString().TestEquals( $"{Environment.NewLine}  " ) )
            .Go();
    }

    [Fact]
    public void AppendShortIndent_ShouldAppendNewLineToStringBuilder_WhenCurrentIndentIsNotGreaterThanTwoSpaces()
    {
        var sut = SqlNodeInterpreterContext.Create();

        var result = sut.AppendShortIndent();

        Assertion.All(
                result.TestRefEquals( sut.Sql ),
                result.ToString().TestEquals( Environment.NewLine ) )
            .Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( null )]
    public void AddParameter_ShouldAddNewParameter(int? index)
    {
        var name = "foo";
        var type = TypeNullability.Create<int>();
        var sut = SqlNodeInterpreterContext.Create();

        sut.AddParameter( name, type, index );

        sut.Parameters.TestSequence( [ new SqlNodeInterpreterContextParameter( name, type, index ) ] ).Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( null )]
    public void AddParameter_ShouldDoNothing_WhenParameterWithExactlyTheSameTypeAndIndexAlreadyExists(int? index)
    {
        var name = "foo";
        var type = TypeNullability.Create<int>();
        var sut = SqlNodeInterpreterContext.Create();
        sut.AddParameter( name, type, index );

        sut.AddParameter( name, type, index );

        sut.Parameters.TestSequence( [ new SqlNodeInterpreterContextParameter( name, type, index ) ] ).Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( null )]
    public void AddParameter_ShouldChangeTypeToNull_WhenParameterAlreadyExistsAndHasDifferentType(int? index)
    {
        var name = "foo";
        var originalType = TypeNullability.Create<int>();
        var newType = TypeNullability.Create<int>( isNullable: true );
        var sut = SqlNodeInterpreterContext.Create();
        sut.AddParameter( name, originalType, index );

        sut.AddParameter( name, newType, index );

        sut.Parameters.TestSequence( [ new SqlNodeInterpreterContextParameter( name, null, index ) ] ).Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( null )]
    public void AddParameter_ShouldChangeIndexToNull_WhenParameterAlreadyExistsAndHasDifferentIndex(int? index)
    {
        var name = "foo";
        var type = TypeNullability.Create<int>();
        var sut = SqlNodeInterpreterContext.Create();
        sut.AddParameter( name, type, index );

        sut.AddParameter( name, type, index: 2 );

        sut.Parameters.TestSequence( [ new SqlNodeInterpreterContextParameter( name, type, null ) ] ).Go();
    }

    [Fact]
    public void AddParameter_ShouldAddSecondParameterCorrectly()
    {
        var firstName = "foo";
        var firstType = TypeNullability.Create<int>();
        var secondName = "bar";
        var secondType = TypeNullability.Create<string>();
        var sut = SqlNodeInterpreterContext.Create();
        sut.AddParameter( firstName, firstType, null );

        sut.AddParameter( secondName, secondType, null );

        sut.Parameters.TestSequence(
            [
                new SqlNodeInterpreterContextParameter( firstName, firstType, null ),
                new SqlNodeInterpreterContextParameter( secondName, secondType, null )
            ] )
            .Go();
    }

    [Fact]
    public void TryGetParameter_ShouldReturnFalse_WhenParametersAreEmpty()
    {
        var sut = SqlNodeInterpreterContext.Create();

        var result = sut.TryGetParameter( "foo", out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void TryGetParameter_ShouldReturnFalse_WhenParameterDoesNotExist()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.AddParameter( "foo", TypeNullability.Create<int>(), index: 1 );

        var result = sut.TryGetParameter( "bar", out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void TryGetParameter_ShouldReturnTrue_WhenParameterExists()
    {
        var name = "foo";
        var type = TypeNullability.Create<int>();
        var sut = SqlNodeInterpreterContext.Create();
        sut.AddParameter( name, type, index: 2 );

        var result = sut.TryGetParameter( name, out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( new SqlNodeInterpreterContextParameter( name, type, Index: 2 ) ) )
            .Go();
    }

    [Fact]
    public void Clear_ShouldResetAllContextData()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.IncreaseIndent();
        sut.IncreaseChildDepth();
        sut.AddParameter( "foo", TypeNullability.Create<int>(), null );
        sut.Sql.Append( "SELECT * FROM bar" );

        sut.Clear();

        Assertion.All(
                sut.Indent.TestEquals( 0 ),
                sut.ChildDepth.TestEquals( 0 ),
                sut.Sql.Length.TestEquals( 0 ),
                sut.Parameters.TestEmpty() )
            .Go();
    }

    [Fact]
    public void ToSnapshot_ShouldCreateReadonlySnapshotOfCurrentContextState()
    {
        var sut = SqlNodeInterpreterContext.Create();
        sut.AddParameter( "a", TypeNullability.Create<int>(), null );
        sut.AddParameter( "b", TypeNullability.Create<string>( isNullable: true ), index: 1 );
        sut.AddParameter( "c", null, index: 2 );
        sut.Sql.Append( "SELECT * FROM bar" );

        var result = sut.ToSnapshot();

        Assertion.All(
                result.Sql.TestEquals( "SELECT * FROM bar" ),
                result.Parameters.TestCount( count => count.TestEquals( 3 ) )
                    .Then( p => Assertion.All(
                        p[0].Name.TestEquals( "a" ),
                        p[0].Type.TestEquals( TypeNullability.Create<int>() ),
                        p[0].Index.TestNull(),
                        p[1].Name.TestEquals( "b" ),
                        p[1].Type.TestEquals( TypeNullability.Create<string>( isNullable: true ) ),
                        p[1].Index.TestEquals( 1 ),
                        p[2].Name.TestEquals( "c" ),
                        p[2].Type.TestNull(),
                        p[2].Index.TestEquals( 2 ) ) ),
                result.ToString().TestEquals( result.Sql ),
                sut.Sql.ToString().TestEquals( result.Sql ),
                sut.Parameters.Count.TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public void Snapshot_ToExpression_ShouldCreateRawExpressionNode()
    {
        var context = SqlNodeInterpreterContext.Create();
        context.AddParameter( "a", TypeNullability.Create<int>(), null );
        context.Sql.Append( "SELECT * FROM bar" );
        var sut = context.ToSnapshot();

        var result = sut.ToExpression();

        Assertion.All(
                result.Sql.TestEquals( "SELECT * FROM bar" ),
                result.Parameters.TestSequence( sut.Parameters.ToArray() ),
                result.Type.TestNull() )
            .Go();
    }

    [Fact]
    public void Snapshot_ToExpression_ShouldCreateRawExpressionNode_WithType()
    {
        var context = SqlNodeInterpreterContext.Create();
        context.AddParameter( "a", TypeNullability.Create<int>(), null );
        context.Sql.Append( "SELECT * FROM bar" );
        var sut = context.ToSnapshot();

        var result = sut.ToExpression( TypeNullability.Create<string>() );

        Assertion.All(
                result.Sql.TestEquals( "SELECT * FROM bar" ),
                result.Parameters.TestSequence( sut.Parameters.ToArray() ),
                result.Type.TestEquals( TypeNullability.Create<string>() ) )
            .Go();
    }

    [Fact]
    public void Snapshot_ToCondition_ShouldCreateRawConditionNode()
    {
        var context = SqlNodeInterpreterContext.Create();
        context.AddParameter( "a", TypeNullability.Create<int>(), null );
        context.Sql.Append( "SELECT * FROM bar" );
        var sut = context.ToSnapshot();

        var result = sut.ToCondition();

        Assertion.All(
                result.Sql.TestEquals( "SELECT * FROM bar" ),
                result.Parameters.TestSequence( sut.Parameters.ToArray() ) )
            .Go();
    }

    [Fact]
    public void Snapshot_ToStatement_ShouldCreateRawStatementNode()
    {
        var context = SqlNodeInterpreterContext.Create();
        context.AddParameter( "a", TypeNullability.Create<int>(), null );
        context.Sql.Append( "SELECT * FROM bar" );
        var sut = context.ToSnapshot();

        var result = sut.ToStatement();

        Assertion.All(
                result.Sql.TestEquals( "SELECT * FROM bar" ),
                result.Parameters.TestSequence( sut.Parameters.ToArray() ) )
            .Go();
    }

    [Fact]
    public void Snapshot_ToQuery_ShouldCreateRawQueryNode()
    {
        var context = SqlNodeInterpreterContext.Create();
        context.AddParameter( "a", TypeNullability.Create<int>(), null );
        context.Sql.Append( "SELECT * FROM bar" );
        var sut = context.ToSnapshot();

        var result = sut.ToQuery();

        Assertion.All(
                result.Sql.TestEquals( "SELECT * FROM bar" ),
                result.Parameters.TestSequence( sut.Parameters.ToArray() ) )
            .Go();
    }

    [Fact]
    public void Snapshot_Restore_ShouldCreateNewContext()
    {
        var context = SqlNodeInterpreterContext.Create();
        context.AddParameter( "a", TypeNullability.Create<int>(), null );
        context.Sql.Append( "SELECT * FROM bar" );
        var sut = context.ToSnapshot();

        var result = sut.Restore();

        Assertion.All(
                result.Sql.ToString().TestEquals( "SELECT * FROM bar" ),
                result.Parameters.TestSequence(
                    sut.Parameters.ToArray().Select( p => new SqlNodeInterpreterContextParameter( p.Name, p.Type, p.Index ) ) ) )
            .Go();
    }

    [Fact]
    public void Snapshot_Replace_ExpressionWithString_ShouldCreateNewSnapshot()
    {
        var placeholder = SqlNode.Placeholders.Expression( wrapInParentheses: false );
        var context = new SqlNodeDebugInterpreter().Interpret( SqlNode.Parameter<int>( "a" ) + placeholder );
        var sut = context.ToSnapshot();

        var result = sut.Replace( placeholder, "1" );

        Assertion.All(
                result.Sql.TestEquals( "(@a : System.Int32) + (1)" ),
                result.Parameters.TestSequence( sut.Parameters.ToArray() ) )
            .Go();
    }

    [Fact]
    public void Snapshot_Replace_ExpressionWithSnapshot_ShouldCreateNewSnapshot_WhenReplacementDoesNotHaveParameters()
    {
        var placeholder = SqlNode.Placeholders.Expression( wrapInParentheses: false );
        var context = new SqlNodeDebugInterpreter().Interpret( SqlNode.Parameter<int>( "a" ) + placeholder );
        var snapshot = new SqlNodeDebugInterpreter().Interpret( SqlNode.Literal( 1 ) ).ToSnapshot();
        var sut = context.ToSnapshot();

        var result = sut.Replace( placeholder, snapshot );

        Assertion.All(
                result.Sql.TestEquals( "(@a : System.Int32) + (\"1\" : System.Int32)" ),
                result.Parameters.TestSequence( sut.Parameters.ToArray() ) )
            .Go();
    }

    [Fact]
    public void Snapshot_Replace_ExpressionWithSnapshot_ShouldCreateNewSnapshot_WhenSourceDoesNotHaveParameters()
    {
        var placeholder = SqlNode.Placeholders.Expression( wrapInParentheses: false );
        var context = new SqlNodeDebugInterpreter().Interpret( SqlNode.Literal( 1 ) + placeholder );
        var snapshot = new SqlNodeDebugInterpreter().Interpret( SqlNode.Parameter<int>( "a" ) ).ToSnapshot();
        var sut = context.ToSnapshot();

        var result = sut.Replace( placeholder, snapshot );

        Assertion.All(
                result.Sql.TestEquals( "(\"1\" : System.Int32) + (@a : System.Int32)" ),
                result.Parameters.TestSequence( snapshot.Parameters.ToArray() ) )
            .Go();
    }

    [Fact]
    public void Snapshot_Replace_ExpressionWithSnapshot_ShouldCreateNewSnapshot_WhenBothContextsHaveParameters()
    {
        var placeholder = SqlNode.Placeholders.Expression( wrapInParentheses: false );
        var context = new SqlNodeDebugInterpreter().Interpret( SqlNode.Parameter<int>( "a" ) + placeholder );
        var snapshot = new SqlNodeDebugInterpreter().Interpret( SqlNode.Parameter<int>( "b" ) ).ToSnapshot();
        var sut = context.ToSnapshot();

        var result = sut.Replace( placeholder, snapshot );

        Assertion.All(
                result.Sql.TestEquals( "(@a : System.Int32) + (@b : System.Int32)" ),
                result.Parameters.ToArray()
                    .Select( p => p.ToString() )
                    .TestSetEqual( sut.Parameters.ToArray().Concat( snapshot.Parameters.ToArray() ).Select( p => p.ToString() ) ) )
            .Go();
    }

    [Fact]
    public void Snapshot_Replace_ExpressionWithContext_ShouldCreateNewSnapshot_WhenReplacementDoesNotHaveParameters()
    {
        var placeholder = SqlNode.Placeholders.Expression( wrapInParentheses: false );
        var context = new SqlNodeDebugInterpreter().Interpret( SqlNode.Parameter<int>( "a" ) + placeholder );
        var other = new SqlNodeDebugInterpreter().Interpret( SqlNode.Literal( 1 ) );
        var sut = context.ToSnapshot();

        var result = sut.Replace( placeholder, other );

        Assertion.All(
                result.Sql.TestEquals( "(@a : System.Int32) + (\"1\" : System.Int32)" ),
                result.Parameters.TestSequence( sut.Parameters.ToArray() ) )
            .Go();
    }

    [Fact]
    public void Snapshot_Replace_ExpressionWithContext_ShouldCreateNewSnapshot_WhenSourceDoesNotHaveParameters()
    {
        var placeholder = SqlNode.Placeholders.Expression( wrapInParentheses: false );
        var context = new SqlNodeDebugInterpreter().Interpret( SqlNode.Literal( 1 ) + placeholder );
        var other = new SqlNodeDebugInterpreter().Interpret( SqlNode.Parameter<int>( "a" ) );
        var sut = context.ToSnapshot();

        var result = sut.Replace( placeholder, other );

        Assertion.All(
                result.Sql.TestEquals( "(\"1\" : System.Int32) + (@a : System.Int32)" ),
                result.Parameters.ToArray()
                    .Select( p => p.ToString() )
                    .TestSequence( other.Parameters.ToArray().Select( p => SqlNode.Parameter( p.Name, p.Type, p.Index ).ToString() ) ) )
            .Go();
    }

    [Fact]
    public void Snapshot_Replace_ExpressionWithContext_ShouldCreateNewSnapshot_WhenBothContextsHaveParameters()
    {
        var placeholder = SqlNode.Placeholders.Expression( wrapInParentheses: false );
        var context = new SqlNodeDebugInterpreter().Interpret( SqlNode.Parameter<int>( "a" ) + placeholder );
        var other = new SqlNodeDebugInterpreter().Interpret( SqlNode.Parameter<int>( "b" ) );
        var sut = context.ToSnapshot();

        var result = sut.Replace( placeholder, other );

        Assertion.All(
                result.Sql.TestEquals( "(@a : System.Int32) + (@b : System.Int32)" ),
                result.Parameters.ToArray()
                    .Select( p => p.ToString() )
                    .TestSetEqual(
                        sut.Parameters.ToArray()
                            .Concat( other.Parameters.ToArray().Select( p => SqlNode.Parameter( p.Name, p.Type, p.Index ) ) )
                            .Select( p => p.ToString() ) ) )
            .Go();
    }

    [Fact]
    public void Snapshot_Replace_ConditionWithString_ShouldCreateNewSnapshot()
    {
        var placeholder = SqlNode.Placeholders.Condition( wrapInParentheses: false );
        var context = new SqlNodeDebugInterpreter().Interpret( (SqlNode.Parameter<int>( "a" ) == SqlNode.Literal( 0 )).Or( placeholder ) );

        var sut = context.ToSnapshot();

        var result = sut.Replace( placeholder, "b > 0" );

        Assertion.All(
                result.Sql.TestEquals( "((@a : System.Int32) == (\"0\" : System.Int32)) OR (b > 0)" ),
                result.Parameters.TestSequence( sut.Parameters.ToArray() ) )
            .Go();
    }

    [Fact]
    public void Snapshot_Replace_ConditionWithSnapshot_ShouldCreateNewSnapshot_WhenReplacementDoesNotHaveParameters()
    {
        var placeholder = SqlNode.Placeholders.Condition( wrapInParentheses: false );
        var context = new SqlNodeDebugInterpreter().Interpret( (SqlNode.Parameter<int>( "a" ) == SqlNode.Literal( 0 )).Or( placeholder ) );
        var snapshot = new SqlNodeDebugInterpreter()
            .Interpret( SqlNode.RawDataField( SqlNode.RawRecordSet( "r" ), "val" ) == SqlNode.Literal( 1 ) )
            .ToSnapshot();

        var sut = context.ToSnapshot();

        var result = sut.Replace( placeholder, snapshot );

        Assertion.All(
                result.Sql.TestEquals( "((@a : System.Int32) == (\"0\" : System.Int32)) OR (([r].[val] : ?) == (\"1\" : System.Int32))" ),
                result.Parameters.TestSequence( sut.Parameters.ToArray() ) )
            .Go();
    }

    [Fact]
    public void Snapshot_Replace_ConditionWithSnapshot_ShouldCreateNewSnapshot_WhenSourceDoesNotHaveParameters()
    {
        var placeholder = SqlNode.Placeholders.Condition( wrapInParentheses: false );
        var context = new SqlNodeDebugInterpreter().Interpret(
            (SqlNode.RawDataField( SqlNode.RawRecordSet( "r" ), "val" ) == SqlNode.Literal( 1 )).Or( placeholder ) );

        var snapshot = new SqlNodeDebugInterpreter().Interpret( SqlNode.Parameter<int>( "b" ) == SqlNode.Literal( 1 ) ).ToSnapshot();
        var sut = context.ToSnapshot();

        var result = sut.Replace( placeholder, snapshot );

        Assertion.All(
                result.Sql.TestEquals( "(([r].[val] : ?) == (\"1\" : System.Int32)) OR ((@b : System.Int32) == (\"1\" : System.Int32))" ),
                result.Parameters.TestSequence( snapshot.Parameters.ToArray() ) )
            .Go();
    }

    [Fact]
    public void Snapshot_Replace_ConditionWithSnapshot_ShouldCreateNewSnapshot_WhenBothContextsHaveParameters()
    {
        var placeholder = SqlNode.Placeholders.Condition( wrapInParentheses: false );
        var context = new SqlNodeDebugInterpreter().Interpret( (SqlNode.Parameter<int>( "a" ) == SqlNode.Literal( 0 )).Or( placeholder ) );
        var snapshot = new SqlNodeDebugInterpreter().Interpret( SqlNode.Parameter<int>( "b" ) == SqlNode.Literal( 1 ) ).ToSnapshot();
        var sut = context.ToSnapshot();

        var result = sut.Replace( placeholder, snapshot );

        Assertion.All(
                result.Sql.TestEquals(
                    "((@a : System.Int32) == (\"0\" : System.Int32)) OR ((@b : System.Int32) == (\"1\" : System.Int32))" ),
                result.Parameters.ToArray()
                    .Select( p => p.ToString() )
                    .TestSetEqual( sut.Parameters.ToArray().Concat( snapshot.Parameters.ToArray() ).Select( p => p.ToString() ) ) )
            .Go();
    }

    [Fact]
    public void Snapshot_Replace_ConditionWithContext_ShouldCreateNewSnapshot_WhenReplacementDoesNotHaveParameters()
    {
        var placeholder = SqlNode.Placeholders.Condition( wrapInParentheses: false );
        var context = new SqlNodeDebugInterpreter().Interpret( (SqlNode.Parameter<int>( "a" ) == SqlNode.Literal( 0 )).Or( placeholder ) );
        var other = new SqlNodeDebugInterpreter().Interpret(
            SqlNode.RawDataField( SqlNode.RawRecordSet( "r" ), "val" ) == SqlNode.Literal( 1 ) );

        var sut = context.ToSnapshot();

        var result = sut.Replace( placeholder, other );

        Assertion.All(
                result.Sql.TestEquals( "((@a : System.Int32) == (\"0\" : System.Int32)) OR (([r].[val] : ?) == (\"1\" : System.Int32))" ),
                result.Parameters.TestSequence( sut.Parameters.ToArray() ) )
            .Go();
    }

    [Fact]
    public void Snapshot_Replace_ConditionWithContext_ShouldCreateNewSnapshot_WhenSourceDoesNotHaveParameters()
    {
        var placeholder = SqlNode.Placeholders.Condition( wrapInParentheses: false );
        var context = new SqlNodeDebugInterpreter().Interpret(
            (SqlNode.RawDataField( SqlNode.RawRecordSet( "r" ), "val" ) == SqlNode.Literal( 1 )).Or( placeholder ) );

        var other = new SqlNodeDebugInterpreter().Interpret( SqlNode.Parameter<int>( "b" ) == SqlNode.Literal( 1 ) );
        var sut = context.ToSnapshot();

        var result = sut.Replace( placeholder, other );

        Assertion.All(
                result.Sql.TestEquals( "(([r].[val] : ?) == (\"1\" : System.Int32)) OR ((@b : System.Int32) == (\"1\" : System.Int32))" ),
                result.Parameters.ToArray()
                    .Select( p => p.ToString() )
                    .TestSequence( other.Parameters.ToArray().Select( p => SqlNode.Parameter( p.Name, p.Type, p.Index ).ToString() ) ) )
            .Go();
    }

    [Fact]
    public void Snapshot_Replace_ConditionWithContext_ShouldCreateNewSnapshot_WhenBothContextsHaveParameters()
    {
        var placeholder = SqlNode.Placeholders.Condition( wrapInParentheses: false );
        var context = new SqlNodeDebugInterpreter().Interpret( (SqlNode.Parameter<int>( "a" ) == SqlNode.Literal( 0 )).Or( placeholder ) );
        var other = new SqlNodeDebugInterpreter().Interpret( SqlNode.Parameter<int>( "b" ) == SqlNode.Literal( 1 ) );
        var sut = context.ToSnapshot();

        var result = sut.Replace( placeholder, other );

        Assertion.All(
                result.Sql.TestEquals(
                    "((@a : System.Int32) == (\"0\" : System.Int32)) OR ((@b : System.Int32) == (\"1\" : System.Int32))" ),
                result.Parameters.ToArray()
                    .Select( p => p.ToString() )
                    .TestSetEqual(
                        sut.Parameters.ToArray()
                            .Concat( other.Parameters.ToArray().Select( p => SqlNode.Parameter( p.Name, p.Type, p.Index ) ) )
                            .Select( p => p.ToString() ) ) )
            .Go();
    }
}
