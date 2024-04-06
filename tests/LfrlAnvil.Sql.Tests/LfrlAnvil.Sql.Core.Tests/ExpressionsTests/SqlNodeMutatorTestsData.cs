using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public class SqlNodeMutatorTestsData
{
    public static TheoryData<SqlNodeBase, bool, string> GetReplacementData(IFixture fixture)
    {
        var leafResult = "(\"1\" : System.Int32) + (\"2\" : System.Int32)";
        var nodeResult = "(\"1\" : System.Int32) + (\"foo\" : System.String)";
        var arg1 = SqlNode.Parameter( "a" );
        var arg2 = SqlNode.Parameter( "b" );
        var arg3 = SqlNode.Parameter( "c" );

        var expressions = new SqlNodeBase[]
        {
            SqlNode.RawExpression( "1 + 2" ),
            SqlNode.RawDataField( SqlNode.RawRecordSet( "a" ), "b" ),
            SqlNode.Null(),
            SqlNode.Literal( 1 ),
            SqlNode.Parameter<int>( "a" ),
            SqlTableMock.Create<int>( "a", new[] { "b" } ).Node["b"],
            SqlTableBuilderMock.Create<int>( "a", new[] { "b" } ).Node["b"],
            SqlNode.RawRecordSet( "a" ).ToDataSource().Select( r => new[] { r.From["b"].AsSelf() } ).AsSet( "c" )["b"],
            SqlViewMock.Create( "a", SqlNode.RawRecordSet( "c" ).ToDataSource().Select( r => new[] { r.From["b"].AsSelf() } ) ).Node["b"],
            -arg1,
            arg1 + arg2,
            arg1.Concat( arg2 ),
            arg1 - arg2,
            arg1 * arg2,
            arg1 / arg2,
            arg1 % arg2,
            ~arg1,
            arg1 & arg2,
            arg1 | arg2,
            arg1 ^ arg2,
            arg1.BitwiseLeftShift( arg2 ),
            arg1.BitwiseRightShift( arg2 ),
            SqlNode.SwitchCase( SqlNode.RawCondition( "a > 0" ), arg1 ),
            SqlNode.Switch( new[] { SqlNode.SwitchCase( SqlNode.RawCondition( "a > 0" ), arg1 ) }, arg2 ),
            SqlNode.Functions.Named( SqlSchemaObjectName.Create( "a" ) ),
            arg1.Coalesce(),
            SqlNode.Functions.CurrentDate(),
            SqlNode.Functions.CurrentTime(),
            SqlNode.Functions.CurrentDateTime(),
            SqlNode.Functions.CurrentTimestamp(),
            SqlNode.Functions.ExtractDate( arg1 ),
            SqlNode.Functions.ExtractTimeOfDay( arg1 ),
            SqlNode.Functions.ExtractDayOfYear( arg1 ),
            SqlNode.Functions.ExtractTemporalUnit( arg1, SqlTemporalUnit.Hour ),
            SqlNode.Functions.TemporalAdd( arg1, arg2, SqlTemporalUnit.Minute ),
            SqlNode.Functions.TemporalDiff( arg1, arg2, SqlTemporalUnit.Second ),
            SqlNode.Functions.NewGuid(),
            arg1.Length(),
            arg1.ByteLength(),
            arg1.ToLower(),
            arg1.ToUpper(),
            arg1.TrimStart(),
            arg1.TrimEnd(),
            arg1.Trim(),
            arg1.Substring( arg2 ),
            arg1.Replace( arg2, arg3 ),
            arg1.Reverse(),
            arg1.IndexOf( arg2 ),
            arg1.LastIndexOf( arg2 ),
            arg1.Sign(),
            arg1.Abs(),
            arg1.Ceiling(),
            arg1.Floor(),
            arg1.Truncate(),
            arg1.Round( arg2 ),
            arg1.Power( arg2 ),
            arg1.SquareRoot(),
            arg1.Min( arg2 ),
            arg1.Max( arg2 ),
            new SqlFunctionNodeMock(),
            SqlNode.AggregateFunctions.Named( SqlSchemaObjectName.Create( "a" ) ),
            arg1.Min(),
            arg1.Max(),
            arg1.Average(),
            arg1.Sum(),
            arg1.Count(),
            arg1.StringConcat(),
            SqlNode.WindowFunctions.RowNumber(),
            SqlNode.WindowFunctions.Rank(),
            SqlNode.WindowFunctions.DenseRank(),
            SqlNode.WindowFunctions.CumulativeDistribution(),
            arg1.NTile(),
            arg1.Lag(),
            arg1.Lead(),
            arg1.FirstValue(),
            arg1.LastValue(),
            arg1.NthValue( arg2 ),
            new SqlAggregateFunctionNodeMock(),
            SqlNode.RawCondition( "a > 0" ),
            SqlNode.True(),
            SqlNode.False(),
            arg1 == arg2,
            arg1 != arg2,
            arg1 > arg2,
            arg1 < arg2,
            arg1 >= arg2,
            arg1 <= arg2,
            SqlNode.True().And( SqlNode.True() ),
            SqlNode.True().Or( SqlNode.True() ),
            SqlNode.True().ToValue(),
            arg1.IsBetween( arg2, arg3 ),
            SqlNode.RawQuery( "SELECT * FROM foo" ).Exists(),
            arg1.Like( arg2 ),
            arg1.In( arg2 ),
            arg1.InQuery( SqlNode.RawQuery( "SELECT * FROM foo" ) ),
            SqlNode.RawRecordSet( "a" ),
            SqlNode.Functions.Named( SqlSchemaObjectName.Create( "a" ) ).AsSet( "b" ),
            SqlTableMock.Create<int>( "a", new[] { "b" } ).Node,
            SqlTableBuilderMock.Create<int>( "a", new[] { "b" } ).Node,
            SqlViewMock.Create( "a" ).Node,
            SqlViewBuilderMock.Create( "a" ).Node,
            SqlNode.RawQuery( "SELECT * FROM foo" ).AsSet( "a" ),
            SqlNode.RawQuery( "SELECT * FROM foo" ).ToCte( "a" ).RecordSet,
            SqlNode.CreateTable( SqlRecordSetInfo.Create( "a" ), Array.Empty<SqlColumnDefinitionNode>() ).RecordSet,
            SqlNode.CreateView( SqlRecordSetInfo.Create( "a" ), SqlNode.RawQuery( "SELECT * FROM foo" ) ).AsSet(),
            SqlNode.RawRecordSet( "a" ).InnerOn( SqlNode.True() ),
            SqlNode.DummyDataSource(),
            SqlNode.RawRecordSet( "a" ).ToDataSource(),
            SqlNode.RawRecordSet( "a" ).Join( SqlNode.RawRecordSet( "b" ).InnerOn( SqlNode.True() ) ),
            arg1.As( "b" ),
            SqlTableMock.Create<int>( "a", new[] { "b" } )
                .Node.ToDataSource()
                .Select( t => new[] { t.From["b"].AsSelf() } )
                .CompoundWith( SqlNode.RawQuery( "SELECT * FROM foo" ).ToCompound( SqlCompoundQueryOperator.UnionAll ) )
                .Selection[0],
            SqlNode.RawRecordSet( "a" ).GetAll(),
            SqlNode.RawRecordSet( "a" ).ToDataSource().GetAll(),
            arg1.As( "b" ).ToExpression(),
            SqlNode.RawQuery( "SELECT * FROM foo" ),
            SqlNode.RawRecordSet( "a" ).ToDataSource().Select( r => new[] { r.GetAll() } ),
            SqlNode.RawQuery( "SELECT * FROM foo" )
                .CompoundWith( SqlNode.RawQuery( "SELECT * FROM foo" ).ToCompound( SqlCompoundQueryOperator.UnionAll ) ),
            SqlNode.RawQuery( "SELECT * FROM foo" ).ToCompound( SqlCompoundQueryOperator.UnionAll ),
            SqlNode.DistinctTrait(),
            SqlNode.FilterTrait( SqlNode.True(), true ),
            SqlNode.AggregationTrait(),
            SqlNode.AggregationFilterTrait( SqlNode.True(), true ),
            SqlNode.SortTrait(),
            SqlNode.LimitTrait( arg1 ),
            SqlNode.OffsetTrait( arg1 ),
            SqlNode.CommonTableExpressionTrait(),
            SqlNode.WindowDefinitionTrait(),
            SqlNode.WindowTrait( SqlNode.WindowDefinition( "a", Array.Empty<SqlOrderByNode>() ) ),
            arg1.Asc(),
            SqlNode.RawQuery( "SELECT * FROM foo" ).ToCte( "a" ),
            SqlNode.WindowDefinition( "a", Array.Empty<SqlOrderByNode>() ),
            SqlNode.RowsWindowFrame( SqlWindowFrameBoundary.CurrentRow, SqlWindowFrameBoundary.CurrentRow ),
            arg1.CastTo<int>(),
            SqlNode.Values( arg1 ),
            SqlNode.Values( new SqlExpressionNode[,] { { arg1 }, { arg2 } } ),
            SqlNode.RawStatement( "COMMIT", SqlNode.Parameter<int>( "a" ) ),
            SqlNode.Values( arg1 ).ToInsertInto( SqlNode.RawRecordSet( "a" ) ),
            SqlNode.RawQuery( "SELECT * FROM foo" ).ToInsertInto( SqlNode.RawRecordSet( "b" ) ),
            SqlNode.RawRecordSet( "a" ).ToDataSource().ToUpdate(),
            SqlNode.Values( arg1 )
                .ToUpsert(
                    SqlNode.RawRecordSet( "a" ),
                    ReadOnlyArray<SqlDataFieldNode>.Empty,
                    (_, _) => Array.Empty<SqlValueAssignmentNode>() ),
            SqlNode.RawQuery( "SELECT * FROM foo" )
                .ToUpsert(
                    SqlNode.RawRecordSet( "a" ),
                    ReadOnlyArray<SqlDataFieldNode>.Empty,
                    (_, _) => Array.Empty<SqlValueAssignmentNode>() ),
            SqlNode.RawRecordSet( "a" )["b"].Assign( arg1 ),
            SqlNode.RawRecordSet( "a" ).ToDataSource().ToDeleteFrom(),
            SqlNode.RawRecordSet( "a" ).ToTruncate(),
            SqlNode.Column<int>( "a" ),
            SqlNode.PrimaryKey( SqlSchemaObjectName.Create( "a" ), ReadOnlyArray<SqlOrderByNode>.Empty ),
            SqlNode.ForeignKey(
                SqlSchemaObjectName.Create( "a" ),
                Array.Empty<SqlDataFieldNode>(),
                SqlNode.RawRecordSet( "b" ),
                Array.Empty<SqlDataFieldNode>() ),
            SqlNode.Check( SqlSchemaObjectName.Create( "a" ), SqlNode.True() ),
            SqlNode.CreateTable( SqlRecordSetInfo.Create( "a" ), Array.Empty<SqlColumnDefinitionNode>() ),
            SqlNode.CreateView( SqlRecordSetInfo.Create( "a" ), SqlNode.RawQuery( "SELECT * FROM foo" ) ),
            SqlNode.CreateIndex(
                SqlSchemaObjectName.Create( "a" ),
                false,
                SqlNode.RawRecordSet( "a" ),
                ReadOnlyArray<SqlOrderByNode>.Empty ),
            SqlNode.RenameTable( SqlRecordSetInfo.Create( "a" ), SqlSchemaObjectName.Create( "b" ) ),
            SqlNode.RenameColumn( SqlRecordSetInfo.Create( "a" ), "b", "c" ),
            SqlNode.AddColumn( SqlRecordSetInfo.Create( "a" ), SqlNode.Column<int>( "b" ) ),
            SqlNode.DropColumn( SqlRecordSetInfo.Create( "a" ), "b" ),
            SqlNode.DropTable( SqlRecordSetInfo.Create( "a" ) ),
            SqlNode.DropView( SqlRecordSetInfo.Create( "a" ) ),
            SqlNode.DropIndex( SqlRecordSetInfo.Create( "a" ), SqlSchemaObjectName.Create( "b" ) ),
            SqlNode.Batch(),
            SqlNode.BeginTransaction( IsolationLevel.Serializable ),
            SqlNode.CommitTransaction(),
            SqlNode.RollbackTransaction(),
            new SqlNodeMock()
        };

        var result = new TheoryData<SqlNodeBase, bool, string>();
        foreach ( var e in expressions )
        {
            result.Add( e, true, leafResult );
            result.Add( e, false, nodeResult );
        }

        return result;
    }

    public static TheoryData<SqlNodeBase, SqlNodeBase?, bool> GetLeafNodesData(IFixture fixture)
    {
        var arg = SqlNode.Parameter( "a" );
        var trait = SqlNode.DistinctTrait();
        var data = new (SqlNodeBase Node, SqlNodeBase? Child)[]
        {
            (SqlNode.RawExpression( "@a + 1", arg ), arg),
            (SqlNode.Null(), null),
            (SqlNode.Literal( 1 ), null),
            (SqlNode.Parameter<int>( "a" ), null),
            (SqlNode.Functions.CurrentDate(), null),
            (SqlNode.Functions.CurrentTime(), null),
            (SqlNode.Functions.CurrentDateTime(), null),
            (SqlNode.Functions.CurrentUtcDateTime(), null),
            (SqlNode.Functions.CurrentTimestamp(), null),
            (SqlNode.Functions.NewGuid(), null),
            (new SqlFunctionNodeMock( new SqlExpressionNode[] { arg } ), arg),
            (new SqlAggregateFunctionNodeMock( new SqlExpressionNode[] { arg } ), arg),
            (new SqlAggregateFunctionNodeMock( ReadOnlyArray<SqlExpressionNode>.Empty, Chain.Create<SqlTraitNode>( trait ) ), trait),
            (SqlNode.RawCondition( "@a > 0", arg ), arg),
            (SqlNode.True(), null),
            (SqlNode.False(), null),
            (SqlNode.RawRecordSet( "a" ), null),
            (SqlNode.Functions.Named( SqlSchemaObjectName.Create( "a" ), arg ).AsSet( "b" ), arg),
            (SqlTableMock.Create<int>( "a", new[] { "b" } ).Node, null),
            (SqlTableBuilderMock.Create<int>( "a", new[] { "b" } ).Node, null),
            (SqlViewMock.Create( "a" ).Node, null),
            (SqlViewBuilderMock.Create( "a" ).Node, null),
            (SqlNode.RawQuery( "SELECT * FROM foo" ).AsSet( "a" ), null),
            (SqlNode.RawQuery( "SELECT * FROM foo" ).ToCte( "a" ), null),
            (SqlNode.CreateTable( SqlRecordSetInfo.Create( "a" ), Array.Empty<SqlColumnDefinitionNode>() ).RecordSet, null),
            (SqlNode.CreateView( SqlRecordSetInfo.Create( "a" ), SqlNode.RawQuery( "SELECT * FROM foo" ) ).AsSet(), null),
            (SqlTableMock.Create<int>( "a", new[] { "b" } )
                 .Node.ToDataSource()
                 .Select( t => new[] { t.From["b"].AsSelf() } )
                 .CompoundWith( SqlNode.RawQuery( "SELECT * FROM foo" ).ToCompound( SqlCompoundQueryOperator.UnionAll ) )
                 .Selection[0], null),
            (SqlNode.RawQuery( "SELECT * FROM foo" ), null),
            (SqlNode.DistinctTrait(), null),
            (SqlNode.RowsWindowFrame( SqlWindowFrameBoundary.CurrentRow, SqlWindowFrameBoundary.CurrentRow ), null),
            (SqlNode.RawStatement( "EXEC x(@a)", arg ), arg),
            (SqlNode.RenameTable( SqlRecordSetInfo.Create( "a" ), SqlSchemaObjectName.Create( "b" ) ), null),
            (SqlNode.RenameColumn( SqlRecordSetInfo.Create( "a" ), "b", "c" ), null),
            (SqlNode.DropColumn( SqlRecordSetInfo.Create( "a" ), "b" ), null),
            (SqlNode.DropTable( SqlRecordSetInfo.Create( "a" ) ), null),
            (SqlNode.DropView( SqlRecordSetInfo.Create( "a" ) ), null),
            (SqlNode.DropIndex( SqlRecordSetInfo.Create( "a" ), SqlSchemaObjectName.Create( "b" ) ), null),
            (SqlNode.BeginTransaction( IsolationLevel.Serializable ), null),
            (SqlNode.CommitTransaction(), null),
            (SqlNode.RollbackTransaction(), null),
            (new SqlNodeMock(), null)
        };

        var result = new TheoryData<SqlNodeBase, SqlNodeBase?, bool>();
        foreach ( var (node, child) in data )
        {
            result.Add( node, child, true );
            result.Add( node, child, false );
        }

        return result;
    }

    public static TheoryData<SqlDataFieldNode, SqlRecordSetNode?, bool, string> GetDataFieldsData(IFixture fixture)
    {
        var replacement = SqlNode.RawRecordSet( "replaced" );
        var data = new (SqlDataFieldNode, string)[]
        {
            (SqlNode.RawDataField( SqlNode.RawRecordSet( "a" ), "b" ), "[replaced].[b] : ?"),
            (SqlTableMock.Create<int>( "a", new[] { "b" } ).Node["b"], "[replaced].[b] : System.Int32"),
            (SqlTableBuilderMock.Create<int>( "a", new[] { "b" } ).Node["b"], "[replaced].[b] : System.Int32"),
            (SqlNode.RawRecordSet( "a" ).ToDataSource().Select( r => new[] { r.From["b"].AsSelf() } ).AsSet( "c" )["b"], "[replaced].[b]"),
            (SqlViewMock.Create( "a", SqlNode.RawRecordSet( "c" ).ToDataSource().Select( r => new[] { r.From["b"].AsSelf() } ) ).Node["b"],
             "[replaced].[b]")
        };

        var result = new TheoryData<SqlDataFieldNode, SqlRecordSetNode?, bool, string>();
        foreach ( var (field, changed) in data )
        {
            var unchanged = field.ToString();
            result.Add( field, null, true, unchanged );
            result.Add( field, null, false, unchanged );
            result.Add( field, replacement, false, changed );
        }

        return result;
    }

    public static TheoryData<SqlNodeBase, (SqlNodeBase, SqlNodeBase)?, bool, string> GetOperatorsData(
        IFixture fixture)
    {
        var arg1 = SqlNode.Parameter( "a" );
        var arg2 = SqlNode.Parameter( "b" );
        var arg3 = SqlNode.Parameter( "c" );
        var cArg1 = SqlNode.RawCondition( "a > 0" );
        var cArg2 = SqlNode.RawCondition( "b > 0" );
        var replacement = SqlNode.Literal( "replaced" );
        var cReplacement = SqlNode.True();
        var query = SqlNode.RawQuery( "SELECT * FROM foo" );
        var queryReplacement = SqlNode.RawQuery( "SELECT * FROM replaced" );

        var data = new (SqlNodeBase, (SqlNodeBase, string)[])[]
        {
            (-arg1, new[] { Arg( arg1, $"-({replacement})" ) }),
            (arg1 + arg2, new[] { Arg( arg1, $"({replacement}) + ({arg2})" ), Arg( arg2, $"({arg1}) + ({replacement})" ) }),
            (arg1.Concat( arg2 ), new[] { Arg( arg1, $"({replacement}) || ({arg2})" ), Arg( arg2, $"({arg1}) || ({replacement})" ) }),
            (arg1 - arg2, new[] { Arg( arg1, $"({replacement}) - ({arg2})" ), Arg( arg2, $"({arg1}) - ({replacement})" ) }),
            (arg1 * arg2, new[] { Arg( arg1, $"({replacement}) * ({arg2})" ), Arg( arg2, $"({arg1}) * ({replacement})" ) }),
            (arg1 / arg2, new[] { Arg( arg1, $"({replacement}) / ({arg2})" ), Arg( arg2, $"({arg1}) / ({replacement})" ) }),
            (arg1 % arg2, new[] { Arg( arg1, $"({replacement}) % ({arg2})" ), Arg( arg2, $"({arg1}) % ({replacement})" ) }),
            (~arg1, new[] { Arg( arg1, $"~({replacement})" ) }),
            (arg1 & arg2, new[] { Arg( arg1, $"({replacement}) & ({arg2})" ), Arg( arg2, $"({arg1}) & ({replacement})" ) }),
            (arg1 | arg2, new[] { Arg( arg1, $"({replacement}) | ({arg2})" ), Arg( arg2, $"({arg1}) | ({replacement})" ) }),
            (arg1 ^ arg2, new[] { Arg( arg1, $"({replacement}) ^ ({arg2})" ), Arg( arg2, $"({arg1}) ^ ({replacement})" ) }),
            (arg1.BitwiseLeftShift( arg2 ),
             new[] { Arg( arg1, $"({replacement}) << ({arg2})" ), Arg( arg2, $"({arg1}) << ({replacement})" ) }),
            (arg1.BitwiseRightShift( arg2 ),
             new[] { Arg( arg1, $"({replacement}) >> ({arg2})" ), Arg( arg2, $"({arg1}) >> ({replacement})" ) }),
            (arg1 == arg2, new[] { Arg( arg1, $"({replacement}) == ({arg2})" ), Arg( arg2, $"({arg1}) == ({replacement})" ) }),
            (arg1 != arg2, new[] { Arg( arg1, $"({replacement}) <> ({arg2})" ), Arg( arg2, $"({arg1}) <> ({replacement})" ) }),
            (arg1 > arg2, new[] { Arg( arg1, $"({replacement}) > ({arg2})" ), Arg( arg2, $"({arg1}) > ({replacement})" ) }),
            (arg1 < arg2, new[] { Arg( arg1, $"({replacement}) < ({arg2})" ), Arg( arg2, $"({arg1}) < ({replacement})" ) }),
            (arg1 >= arg2, new[] { Arg( arg1, $"({replacement}) >= ({arg2})" ), Arg( arg2, $"({arg1}) >= ({replacement})" ) }),
            (arg1 <= arg2, new[] { Arg( arg1, $"({replacement}) <= ({arg2})" ), Arg( arg2, $"({arg1}) <= ({replacement})" ) }),
            (arg1 <= arg2, new[] { Arg( arg1, $"({replacement}) <= ({arg2})" ), Arg( arg2, $"({arg1}) <= ({replacement})" ) }),
            (arg1 <= arg2, new[] { Arg( arg1, $"({replacement}) <= ({arg2})" ), Arg( arg2, $"({arg1}) <= ({replacement})" ) }),
            (cArg1.And( cArg2 ),
             new[] { Arg( cArg1, $"({cReplacement}) AND ({cArg2})" ), Arg( cArg2, $"({cArg1}) AND ({cReplacement})" ) }),
            (cArg1.Or( cArg2 ), new[] { Arg( cArg1, $"({cReplacement}) OR ({cArg2})" ), Arg( cArg2, $"({cArg1}) OR ({cReplacement})" ) }),
            (arg1.IsBetween( arg2, arg3 ),
             new[]
             {
                 Arg( arg1, $"({replacement}) BETWEEN ({arg2}) AND ({arg3})" ),
                 Arg( arg2, $"({arg1}) BETWEEN ({replacement}) AND ({arg3})" ),
                 Arg( arg3, $"({arg1}) BETWEEN ({arg2}) AND ({replacement})" )
             }),
            (arg1.IsNotBetween( arg2, arg3 ),
             new[]
             {
                 Arg( arg1, $"({replacement}) NOT BETWEEN ({arg2}) AND ({arg3})" ),
                 Arg( arg2, $"({arg1}) NOT BETWEEN ({replacement}) AND ({arg3})" ),
                 Arg( arg3, $"({arg1}) NOT BETWEEN ({arg2}) AND ({replacement})" )
             }),
            (query.Exists(), new[] { Arg( query, $"EXISTS ({Environment.NewLine}  {queryReplacement}{Environment.NewLine})" ) }),
            (query.NotExists(), new[] { Arg( query, $"NOT EXISTS ({Environment.NewLine}  {queryReplacement}{Environment.NewLine})" ) }),
            (arg1.Like( arg2 ), new[] { Arg( arg1, $"({replacement}) LIKE ({arg2})" ), Arg( arg2, $"({arg1}) LIKE ({replacement})" ) }),
            (arg1.NotLike( arg2 ),
             new[]
             {
                 Arg( arg1, $"({replacement}) NOT LIKE ({arg2})" ),
                 Arg( arg2, $"({arg1}) NOT LIKE ({replacement})" )
             }),
            (arg1.Like( arg2, arg3 ),
             new[]
             {
                 Arg( arg1, $"({replacement}) LIKE ({arg2}) ESCAPE ({arg3})" ),
                 Arg( arg2, $"({arg1}) LIKE ({replacement}) ESCAPE ({arg3})" ),
                 Arg( arg3, $"({arg1}) LIKE ({arg2}) ESCAPE ({replacement})" )
             }),
            (arg1.NotLike( arg2, arg3 ),
             new[]
             {
                 Arg( arg1, $"({replacement}) NOT LIKE ({arg2}) ESCAPE ({arg3})" ),
                 Arg( arg2, $"({arg1}) NOT LIKE ({replacement}) ESCAPE ({arg3})" ),
                 Arg( arg3, $"({arg1}) NOT LIKE ({arg2}) ESCAPE ({replacement})" )
             }),
            (arg1.In( arg2, arg3 ),
             new[]
             {
                 Arg( arg1, $"({replacement}) IN (({arg2}), ({arg3}))" ),
                 Arg( arg2, $"({arg1}) IN (({replacement}), ({arg3}))" ),
                 Arg( arg3, $"({arg1}) IN (({arg2}), ({replacement}))" )
             }),
            (arg1.NotIn( arg2, arg3 ),
             new[]
             {
                 Arg( arg1, $"({replacement}) NOT IN (({arg2}), ({arg3}))" ),
                 Arg( arg2, $"({arg1}) NOT IN (({replacement}), ({arg3}))" ),
                 Arg( arg3, $"({arg1}) NOT IN (({arg2}), ({replacement}))" )
             }),
            (arg1.InQuery( query ),
             new[]
             {
                 Arg( arg1, $"({replacement}) IN ({Environment.NewLine}  {query}{Environment.NewLine})" ),
                 Arg( query, $"({arg1}) IN ({Environment.NewLine}  {queryReplacement}{Environment.NewLine})" )
             }),
            (arg1.NotInQuery( query ),
             new[]
             {
                 Arg( arg1, $"({replacement}) NOT IN ({Environment.NewLine}  {query}{Environment.NewLine})" ),
                 Arg( query, $"({arg1}) NOT IN ({Environment.NewLine}  {queryReplacement}{Environment.NewLine})" )
             }),
            (arg1.CastTo<int>(), new[] { Arg( arg1, $"CAST(({replacement}) AS System.Int32)" ) })
        };

        var result = new TheoryData<SqlNodeBase, (SqlNodeBase, SqlNodeBase)?, bool, string>();
        foreach ( var (op, args) in data )
        {
            var unchanged = op.ToString();
            result.Add( op, null, true, unchanged );
            result.Add( op, null, false, unchanged );
            foreach ( var (arg, changed) in args )
                result.Add(
                    op,
                    (arg, arg switch
                    {
                        SqlQueryExpressionNode => queryReplacement,
                        SqlConditionNode => cReplacement,
                        _ => replacement
                    }),
                    false,
                    changed );
        }

        return result;

        [Pure]
        static (SqlNodeBase, string) Arg(SqlNodeBase arg, string expected)
        {
            return (arg, expected);
        }
    }

    public static TheoryData<SqlExpressionNode, (SqlExpressionNode, SqlExpressionNode)?, bool, string> GetFunctionsData(IFixture fixture)
    {
        var arg1 = SqlNode.Parameter( "a" );
        var arg2 = SqlNode.Parameter( "b" );
        var arg3 = SqlNode.Parameter( "c" );
        var trait1 = SqlNode.DistinctTrait();
        var trait2 = SqlNode.FilterTrait( SqlNode.True(), true );
        var traits = Chain.Create( new SqlTraitNode[] { trait1, trait2 }.AsEnumerable() );
        var traitsText = $"{Environment.NewLine}  {trait1}{Environment.NewLine}  {trait2}";
        var replacement = SqlNode.Literal( "replaced" );

        var data = new (SqlExpressionNode, (SqlExpressionNode, string)[])[]
        {
            (SqlNode.Functions.Named( SqlSchemaObjectName.Create( "f" ), arg1, arg2 ),
             new[] { Arg( arg1, $"[f](({replacement}), ({arg2}))" ), Arg( arg2, $"[f](({arg1}), ({replacement}))" ) }),
            (arg1.Coalesce( arg2 ),
             new[] { Arg( arg1, $"COALESCE(({replacement}), ({arg2}))" ), Arg( arg2, $"COALESCE(({arg1}), ({replacement}))" ) }),
            (arg1.ExtractDate(), new[] { Arg( arg1, $"EXTRACT_DATE(({replacement}))" ) }),
            (arg1.ExtractTimeOfDay(), new[] { Arg( arg1, $"EXTRACT_TIME_OF_DAY(({replacement}))" ) }),
            (arg1.ExtractDayOfYear(), new[] { Arg( arg1, $"EXTRACT_DAY_OF_YEAR(({replacement}))" ) }),
            (arg1.ExtractDayOfMonth(), new[] { Arg( arg1, $"EXTRACT_DAY_OF_MONTH(({replacement}))" ) }),
            (arg1.ExtractDayOfWeek(), new[] { Arg( arg1, $"EXTRACT_DAY_OF_WEEK(({replacement}))" ) }),
            (arg1.ExtractTemporalUnit( SqlTemporalUnit.Month ), new[] { Arg( arg1, $"EXTRACT_TEMPORAL_MONTH(({replacement}))" ) }),
            (arg1.ExtractTemporalUnit( SqlTemporalUnit.Hour ), new[] { Arg( arg1, $"EXTRACT_TEMPORAL_HOUR(({replacement}))" ) }),
            (arg1.TemporalAdd( arg2, SqlTemporalUnit.Month ),
             new[]
             {
                 Arg( arg1, $"TEMPORAL_ADD_MONTH(({replacement}), ({arg2}))" ),
                 Arg( arg2, $"TEMPORAL_ADD_MONTH(({arg1}), ({replacement}))" )
             }),
            (arg1.TemporalAdd( arg2, SqlTemporalUnit.Hour ),
             new[]
             {
                 Arg( arg1, $"TEMPORAL_ADD_HOUR(({replacement}), ({arg2}))" ),
                 Arg( arg2, $"TEMPORAL_ADD_HOUR(({arg1}), ({replacement}))" )
             }),
            (arg1.TemporalDiff( arg2, SqlTemporalUnit.Month ),
             new[]
             {
                 Arg( arg1, $"TEMPORAL_DIFF_MONTH(({replacement}), ({arg2}))" ),
                 Arg( arg2, $"TEMPORAL_DIFF_MONTH(({arg1}), ({replacement}))" )
             }),
            (arg1.TemporalDiff( arg2, SqlTemporalUnit.Hour ),
             new[]
             {
                 Arg( arg1, $"TEMPORAL_DIFF_HOUR(({replacement}), ({arg2}))" ),
                 Arg( arg2, $"TEMPORAL_DIFF_HOUR(({arg1}), ({replacement}))" )
             }),
            (arg1.Length(), new[] { Arg( arg1, $"LENGTH(({replacement}))" ) }),
            (arg1.ByteLength(), new[] { Arg( arg1, $"BYTE_LENGTH(({replacement}))" ) }),
            (arg1.ToLower(), new[] { Arg( arg1, $"TO_LOWER(({replacement}))" ) }),
            (arg1.ToUpper(), new[] { Arg( arg1, $"TO_UPPER(({replacement}))" ) }),
            (arg1.TrimStart(), new[] { Arg( arg1, $"TRIM_START(({replacement}))" ) }),
            (arg1.TrimStart( arg2 ),
             new[] { Arg( arg1, $"TRIM_START(({replacement}), ({arg2}))" ), Arg( arg2, $"TRIM_START(({arg1}), ({replacement}))" ) }),
            (arg1.TrimEnd(), new[] { Arg( arg1, $"TRIM_END(({replacement}))" ) }),
            (arg1.TrimEnd( arg2 ),
             new[] { Arg( arg1, $"TRIM_END(({replacement}), ({arg2}))" ), Arg( arg2, $"TRIM_END(({arg1}), ({replacement}))" ) }),
            (arg1.Trim(), new[] { Arg( arg1, $"TRIM(({replacement}))" ) }),
            (arg1.Trim( arg2 ), new[] { Arg( arg1, $"TRIM(({replacement}), ({arg2}))" ), Arg( arg2, $"TRIM(({arg1}), ({replacement}))" ) }),
            (arg1.Substring( arg2 ),
             new[] { Arg( arg1, $"SUBSTRING(({replacement}), ({arg2}))" ), Arg( arg2, $"SUBSTRING(({arg1}), ({replacement}))" ) }),
            (arg1.Substring( arg2, arg3 ),
             new[]
             {
                 Arg( arg1, $"SUBSTRING(({replacement}), ({arg2}), ({arg3}))" ),
                 Arg( arg2, $"SUBSTRING(({arg1}), ({replacement}), ({arg3}))" ),
                 Arg( arg3, $"SUBSTRING(({arg1}), ({arg2}), ({replacement}))" )
             }),
            (arg1.Replace( arg2, arg3 ),
             new[]
             {
                 Arg( arg1, $"REPLACE(({replacement}), ({arg2}), ({arg3}))" ),
                 Arg( arg2, $"REPLACE(({arg1}), ({replacement}), ({arg3}))" ),
                 Arg( arg3, $"REPLACE(({arg1}), ({arg2}), ({replacement}))" )
             }),
            (arg1.Reverse(), new[] { Arg( arg1, $"REVERSE(({replacement}))" ) }),
            (arg1.IndexOf( arg2 ),
             new[] { Arg( arg1, $"INDEX_OF(({replacement}), ({arg2}))" ), Arg( arg2, $"INDEX_OF(({arg1}), ({replacement}))" ) }),
            (arg1.LastIndexOf( arg2 ),
             new[] { Arg( arg1, $"LAST_INDEX_OF(({replacement}), ({arg2}))" ), Arg( arg2, $"LAST_INDEX_OF(({arg1}), ({replacement}))" ) }),
            (arg1.Sign(), new[] { Arg( arg1, $"SIGN(({replacement}))" ) }),
            (arg1.Abs(), new[] { Arg( arg1, $"ABS(({replacement}))" ) }),
            (arg1.Ceiling(), new[] { Arg( arg1, $"CEILING(({replacement}))" ) }),
            (arg1.Floor(), new[] { Arg( arg1, $"FLOOR(({replacement}))" ) }),
            (arg1.Truncate(), new[] { Arg( arg1, $"TRUNCATE(({replacement}))" ) }),
            (arg1.Truncate( arg2 ),
             new[] { Arg( arg1, $"TRUNCATE(({replacement}), ({arg2}))" ), Arg( arg2, $"TRUNCATE(({arg1}), ({replacement}))" ) }),
            (arg1.Round( arg2 ),
             new[] { Arg( arg1, $"ROUND(({replacement}), ({arg2}))" ), Arg( arg2, $"ROUND(({arg1}), ({replacement}))" ) }),
            (arg1.Power( arg2 ),
             new[] { Arg( arg1, $"POWER(({replacement}), ({arg2}))" ), Arg( arg2, $"POWER(({arg1}), ({replacement}))" ) }),
            (arg1.SquareRoot(), new[] { Arg( arg1, $"SQUARE_ROOT(({replacement}))" ) }),
            (arg1.Min( arg2 ), new[] { Arg( arg1, $"MIN(({replacement}), ({arg2}))" ), Arg( arg2, $"MIN(({arg1}), ({replacement}))" ) }),
            (arg1.Max( arg2 ), new[] { Arg( arg1, $"MAX(({replacement}), ({arg2}))" ), Arg( arg2, $"MAX(({arg1}), ({replacement}))" ) }),
            (SqlNode.AggregateFunctions.Named( SqlSchemaObjectName.Create( "f" ), arg1, arg2 ).SetTraits( traits ),
             new[]
             {
                 Arg( arg1, $"AGG_[f](({replacement}), ({arg2})){traitsText}" ),
                 Arg( arg2, $"AGG_[f](({arg1}), ({replacement})){traitsText}" )
             }),
            (arg1.Min().SetTraits( traits ), new[] { Arg( arg1, $"AGG_MIN(({replacement})){traitsText}" ) }),
            (arg1.Max().SetTraits( traits ), new[] { Arg( arg1, $"AGG_MAX(({replacement})){traitsText}" ) }),
            (arg1.Average().SetTraits( traits ), new[] { Arg( arg1, $"AGG_AVERAGE(({replacement})){traitsText}" ) }),
            (arg1.Sum().SetTraits( traits ), new[] { Arg( arg1, $"AGG_SUM(({replacement})){traitsText}" ) }),
            (arg1.Count().SetTraits( traits ), new[] { Arg( arg1, $"AGG_COUNT(({replacement})){traitsText}" ) }),
            (arg1.StringConcat().SetTraits( traits ), new[] { Arg( arg1, $"AGG_STRING_CONCAT(({replacement})){traitsText}" ) }),
            (arg1.StringConcat( arg2 ).SetTraits( traits ),
             new[]
             {
                 Arg( arg1, $"AGG_STRING_CONCAT(({replacement}), ({arg2})){traitsText}" ),
                 Arg( arg2, $"AGG_STRING_CONCAT(({arg1}), ({replacement})){traitsText}" )
             }),
            (arg1.NTile().SetTraits( traits ), new[] { Arg( arg1, $"WND_N_TILE(({replacement})){traitsText}" ) }),
            (arg1.Lag( arg2 ).SetTraits( traits ),
             new[]
             {
                 Arg( arg1, $"WND_LAG(({replacement}), ({arg2})){traitsText}" ),
                 Arg( arg2, $"WND_LAG(({arg1}), ({replacement})){traitsText}" )
             }),
            (arg1.Lag( arg2, arg3 ).SetTraits( traits ),
             new[]
             {
                 Arg( arg1, $"WND_LAG(({replacement}), ({arg2}), ({arg3})){traitsText}" ),
                 Arg( arg2, $"WND_LAG(({arg1}), ({replacement}), ({arg3})){traitsText}" ),
                 Arg( arg3, $"WND_LAG(({arg1}), ({arg2}), ({replacement})){traitsText}" )
             }),
            (arg1.Lead( arg2 ).SetTraits( traits ),
             new[]
             {
                 Arg( arg1, $"WND_LEAD(({replacement}), ({arg2})){traitsText}" ),
                 Arg( arg2, $"WND_LEAD(({arg1}), ({replacement})){traitsText}" )
             }),
            (arg1.Lead( arg2, arg3 ).SetTraits( traits ),
             new[]
             {
                 Arg( arg1, $"WND_LEAD(({replacement}), ({arg2}), ({arg3})){traitsText}" ),
                 Arg( arg2, $"WND_LEAD(({arg1}), ({replacement}), ({arg3})){traitsText}" ),
                 Arg( arg3, $"WND_LEAD(({arg1}), ({arg2}), ({replacement})){traitsText}" )
             }),
            (arg1.FirstValue().SetTraits( traits ), new[] { Arg( arg1, $"WND_FIRST_VALUE(({replacement})){traitsText}" ) }),
            (arg1.LastValue().SetTraits( traits ), new[] { Arg( arg1, $"WND_LAST_VALUE(({replacement})){traitsText}" ) }),
            (arg1.NthValue( arg2 ).SetTraits( traits ),
             new[]
             {
                 Arg( arg1, $"WND_NTH_VALUE(({replacement}), ({arg2})){traitsText}" ),
                 Arg( arg2, $"WND_NTH_VALUE(({arg1}), ({replacement})){traitsText}" )
             }),
        };

        var result = new TheoryData<SqlExpressionNode, (SqlExpressionNode, SqlExpressionNode)?, bool, string>();
        foreach ( var (f, args) in data )
        {
            var unchanged = f.ToString();
            result.Add( f, null, true, unchanged );
            result.Add( f, null, false, unchanged );
            foreach ( var (arg, changed) in args )
                result.Add( f, (arg, replacement), false, changed );
        }

        return result;

        [Pure]
        static (SqlExpressionNode, string) Arg(SqlExpressionNode arg, string expected)
        {
            return (arg, expected);
        }
    }

    public static TheoryData<SqlAggregateFunctionExpressionNode, (SqlTraitNode, SqlTraitNode)?, bool, string>
        GetAggregateFunctionTraitsData(IFixture fixture)
    {
        var arg1 = SqlNode.Parameter( "a" );
        var arg2 = SqlNode.Parameter( "b" );
        var trait1 = SqlNode.DistinctTrait();
        var trait2 = SqlNode.FilterTrait( SqlNode.True(), true );
        var traits = Chain.Create( new SqlTraitNode[] { trait1, trait2 }.AsEnumerable() );
        var replacement = SqlNode.SortTrait();
        var replacementText1 = $"{Environment.NewLine}  {replacement}{Environment.NewLine}  {trait2}";
        var replacementText2 = $"{Environment.NewLine}  {trait1}{Environment.NewLine}  {replacement}";

        var data = new (SqlAggregateFunctionExpressionNode, (SqlTraitNode, string)[])[]
        {
            (SqlNode.AggregateFunctions.Named( SqlSchemaObjectName.Create( "f" ), arg1 ).SetTraits( traits ),
             new[]
             {
                 Arg( trait1, $"AGG_[f](({arg1})){replacementText1}" ),
                 Arg( trait2, $"AGG_[f](({arg1})){replacementText2}" )
             }),
            (arg1.Min().SetTraits( traits ),
             new[] { Arg( trait1, $"AGG_MIN(({arg1})){replacementText1}" ), Arg( trait2, $"AGG_MIN(({arg1})){replacementText2}" ) }),
            (arg1.Max().SetTraits( traits ),
             new[] { Arg( trait1, $"AGG_MAX(({arg1})){replacementText1}" ), Arg( trait2, $"AGG_MAX(({arg1})){replacementText2}" ) }),
            (arg1.Average().SetTraits( traits ),
             new[]
             {
                 Arg( trait1, $"AGG_AVERAGE(({arg1})){replacementText1}" ),
                 Arg( trait2, $"AGG_AVERAGE(({arg1})){replacementText2}" )
             }),
            (arg1.Sum().SetTraits( traits ),
             new[] { Arg( trait1, $"AGG_SUM(({arg1})){replacementText1}" ), Arg( trait2, $"AGG_SUM(({arg1})){replacementText2}" ) }),
            (arg1.Count().SetTraits( traits ),
             new[] { Arg( trait1, $"AGG_COUNT(({arg1})){replacementText1}" ), Arg( trait2, $"AGG_COUNT(({arg1})){replacementText2}" ) }),
            (arg1.StringConcat().SetTraits( traits ), new[]
            {
                Arg( trait1, $"AGG_STRING_CONCAT(({arg1})){replacementText1}" ),
                Arg( trait2, $"AGG_STRING_CONCAT(({arg1})){replacementText2}" )
            }),
            (SqlNode.WindowFunctions.RowNumber().SetTraits( traits ), new[]
            {
                Arg( trait1, $"WND_ROW_NUMBER(){replacementText1}" ),
                Arg( trait2, $"WND_ROW_NUMBER(){replacementText2}" )
            }),
            (SqlNode.WindowFunctions.Rank().SetTraits( traits ), new[]
            {
                Arg( trait1, $"WND_RANK(){replacementText1}" ),
                Arg( trait2, $"WND_RANK(){replacementText2}" )
            }),
            (SqlNode.WindowFunctions.DenseRank().SetTraits( traits ), new[]
            {
                Arg( trait1, $"WND_DENSE_RANK(){replacementText1}" ),
                Arg( trait2, $"WND_DENSE_RANK(){replacementText2}" )
            }),
            (SqlNode.WindowFunctions.CumulativeDistribution().SetTraits( traits ), new[]
            {
                Arg( trait1, $"WND_CUMULATIVE_DISTRIBUTION(){replacementText1}" ),
                Arg( trait2, $"WND_CUMULATIVE_DISTRIBUTION(){replacementText2}" )
            }),
            (arg1.NTile().SetTraits( traits ), new[]
            {
                Arg( trait1, $"WND_N_TILE(({arg1})){replacementText1}" ),
                Arg( trait2, $"WND_N_TILE(({arg1})){replacementText2}" )
            }),
            (arg1.Lag( arg2 ).SetTraits( traits ),
             new[]
             {
                 Arg( trait1, $"WND_LAG(({arg1}), ({arg2})){replacementText1}" ),
                 Arg( trait2, $"WND_LAG(({arg1}), ({arg2})){replacementText2}" )
             }),
            (arg1.Lead( arg2 ).SetTraits( traits ),
             new[]
             {
                 Arg( trait1, $"WND_LEAD(({arg1}), ({arg2})){replacementText1}" ),
                 Arg( trait2, $"WND_LEAD(({arg1}), ({arg2})){replacementText2}" )
             }),
            (arg1.FirstValue().SetTraits( traits ), new[]
            {
                Arg( trait1, $"WND_FIRST_VALUE(({arg1})){replacementText1}" ),
                Arg( trait2, $"WND_FIRST_VALUE(({arg1})){replacementText2}" )
            }),
            (arg1.LastValue().SetTraits( traits ), new[]
            {
                Arg( trait1, $"WND_LAST_VALUE(({arg1})){replacementText1}" ),
                Arg( trait2, $"WND_LAST_VALUE(({arg1})){replacementText2}" )
            }),
            (arg1.NthValue( arg2 ).SetTraits( traits ),
             new[]
             {
                 Arg( trait1, $"WND_NTH_VALUE(({arg1}), ({arg2})){replacementText1}" ),
                 Arg( trait2, $"WND_NTH_VALUE(({arg1}), ({arg2})){replacementText2}" )
             }),
        };

        var result = new TheoryData<SqlAggregateFunctionExpressionNode, (SqlTraitNode, SqlTraitNode)?, bool, string>();
        foreach ( var (f, args) in data )
        {
            var unchanged = f.ToString();
            result.Add( f, null, true, unchanged );
            result.Add( f, null, false, unchanged );
            foreach ( var (arg, changed) in args )
                result.Add( f, (arg, replacement), false, changed );
        }

        return result;

        [Pure]
        static (SqlTraitNode, string) Arg(SqlTraitNode arg, string expected)
        {
            return (arg, expected);
        }
    }

    public static TheoryData<SqlDataSourceNode, (SqlNodeBase, SqlNodeBase)?, bool, string> GetDataSourcesData(IFixture fixture)
    {
        var set1 = SqlNode.RawRecordSet( "a" );
        var set2 = SqlNode.RawRecordSet( "b" );
        var set3 = SqlNode.RawRecordSet( "c" );
        var trait1 = SqlNode.DistinctTrait();
        var trait2 = SqlNode.FilterTrait( SqlNode.True(), true );
        var traits = Chain.Create( new SqlTraitNode[] { trait1, trait2 }.AsEnumerable() );

        var data = new (SqlDataSourceNode, (SqlNodeBase, SqlNodeBase, string)[])[]
        {
            (SqlNode.DummyDataSource().SetTraits( traits ), new[]
            {
                Arg(
                    trait1,
                    SqlNode.SortTrait(),
                    """
                    FROM <DUMMY>
                    ORDER BY
                    AND WHERE TRUE
                    """
                ),
                Arg(
                    trait2,
                    SqlNode.SortTrait(),
                    """
                    FROM <DUMMY>
                    DISTINCT
                    ORDER BY
                    """
                )
            }),
            (set1.ToDataSource().SetTraits( traits ), new[]
            {
                Arg(
                    set1,
                    SqlNode.RawRecordSet( "foo" ),
                    """
                    FROM [foo]
                    DISTINCT
                    AND WHERE TRUE
                    """
                ),
                Arg(
                    trait1,
                    SqlNode.SortTrait(),
                    """
                    FROM [a]
                    ORDER BY
                    AND WHERE TRUE
                    """
                ),
                Arg(
                    trait2,
                    SqlNode.SortTrait(),
                    """
                    FROM [a]
                    DISTINCT
                    ORDER BY
                    """
                )
            }),
            (set1.Join( set2.InnerOn( SqlNode.True() ), set3.LeftOn( SqlNode.True() ) ).SetTraits( traits ), new[]
            {
                Arg(
                    set1,
                    SqlNode.RawRecordSet( "foo" ),
                    """
                    FROM [foo]
                    INNER JOIN [b] ON TRUE
                    LEFT JOIN [c] ON TRUE
                    DISTINCT
                    AND WHERE TRUE
                    """
                ),
                Arg(
                    set2,
                    SqlNode.RawRecordSet( "foo" ),
                    """
                    FROM [a]
                    INNER JOIN [foo] ON TRUE
                    LEFT JOIN [c] ON TRUE
                    DISTINCT
                    AND WHERE TRUE
                    """
                ),
                Arg(
                    set3,
                    SqlNode.RawRecordSet( "foo" ),
                    """
                    FROM [a]
                    INNER JOIN [b] ON TRUE
                    LEFT JOIN [foo] ON TRUE
                    DISTINCT
                    AND WHERE TRUE
                    """
                ),
                Arg(
                    trait1,
                    SqlNode.SortTrait(),
                    """
                    FROM [a]
                    INNER JOIN [b] ON TRUE
                    LEFT JOIN [c] ON TRUE
                    ORDER BY
                    AND WHERE TRUE
                    """
                ),
                Arg(
                    trait2,
                    SqlNode.SortTrait(),
                    """
                    FROM [a]
                    INNER JOIN [b] ON TRUE
                    LEFT JOIN [c] ON TRUE
                    DISTINCT
                    ORDER BY
                    """
                )
            })
        };

        var result = new TheoryData<SqlDataSourceNode, (SqlNodeBase, SqlNodeBase)?, bool, string>();
        foreach ( var (d, args) in data )
        {
            var unchanged = d.ToString();
            result.Add( d, null, true, unchanged );
            result.Add( d, null, false, unchanged );
            foreach ( var (arg, replacement, changed) in args )
                result.Add( d, (arg, replacement), false, changed );
        }

        return result;

        [Pure]
        static (SqlNodeBase, SqlNodeBase, string) Arg(SqlNodeBase arg, SqlNodeBase replacement, string expected)
        {
            return (arg, replacement, expected);
        }
    }

    public static TheoryData<SqlQueryExpressionNode, (SqlNodeBase, SqlNodeBase)?, bool, string> GetQueriesData(IFixture fixture)
    {
        var dataSource = SqlNode.RawRecordSet( "a" ).ToDataSource();
        var field1 = dataSource.From["x"].AsSelf();
        var field2 = dataSource.From["y"].AsSelf();
        var query1 = SqlNode.RawQuery( "SELECT * FROM foo" );
        var query2 = SqlNode.RawQuery( "SELECT * FROM bar" );
        var query3 = SqlNode.RawQuery( "SELECT * FROM qux" );
        var trait1 = SqlNode.DistinctTrait();
        var trait2 = SqlNode.FilterTrait( SqlNode.True(), true );
        var traits = Chain.Create( new SqlTraitNode[] { trait1, trait2 }.AsEnumerable() );

        var data = new (SqlQueryExpressionNode, (SqlNodeBase, SqlNodeBase, string)[])[]
        {
            (dataSource.Select( field1, field2 ).SetTraits( traits ), new[]
            {
                Arg(
                    dataSource,
                    SqlNode.DummyDataSource(),
                    """
                    FROM <DUMMY>
                    DISTINCT
                    AND WHERE TRUE
                    SELECT
                      ([a].[x] : ?),
                      ([a].[y] : ?)
                    """
                ),
                Arg(
                    field1,
                    SqlNode.Literal( 1 ).As( "x" ),
                    """
                    FROM [a]
                    DISTINCT
                    AND WHERE TRUE
                    SELECT
                      ("1" : System.Int32) AS [x],
                      ([a].[y] : ?)
                    """
                ),
                Arg(
                    field2,
                    SqlNode.Literal( 1 ).As( "y" ),
                    """
                    FROM [a]
                    DISTINCT
                    AND WHERE TRUE
                    SELECT
                      ([a].[x] : ?),
                      ("1" : System.Int32) AS [y]
                    """
                ),
                Arg(
                    trait1,
                    SqlNode.SortTrait(),
                    """
                    FROM [a]
                    ORDER BY
                    AND WHERE TRUE
                    SELECT
                      ([a].[x] : ?),
                      ([a].[y] : ?)
                    """
                ),
                Arg(
                    trait2,
                    SqlNode.SortTrait(),
                    """
                    FROM [a]
                    DISTINCT
                    ORDER BY
                    SELECT
                      ([a].[x] : ?),
                      ([a].[y] : ?)
                    """
                )
            }),
            (query1.CompoundWith(
                     query2.ToCompound( SqlCompoundQueryOperator.UnionAll ),
                     query3.ToCompound( SqlCompoundQueryOperator.Intersect ) )
                 .SetTraits( traits ),
             new[]
             {
                 Arg(
                     query1,
                     SqlNode.RawQuery( "SELECT * FROM lorem" ),
                     """
                     SELECT * FROM lorem
                     UNION ALL
                     SELECT * FROM bar
                     INTERSECT
                     SELECT * FROM qux
                     DISTINCT
                     AND WHERE TRUE
                     """
                 ),
                 Arg(
                     query2,
                     SqlNode.RawQuery( "SELECT * FROM lorem" ),
                     """
                     SELECT * FROM foo
                     UNION ALL
                     SELECT * FROM lorem
                     INTERSECT
                     SELECT * FROM qux
                     DISTINCT
                     AND WHERE TRUE
                     """
                 ),
                 Arg(
                     query3,
                     SqlNode.RawQuery( "SELECT * FROM lorem" ),
                     """
                     SELECT * FROM foo
                     UNION ALL
                     SELECT * FROM bar
                     INTERSECT
                     SELECT * FROM lorem
                     DISTINCT
                     AND WHERE TRUE
                     """
                 ),
                 Arg(
                     trait1,
                     SqlNode.SortTrait(),
                     """
                     SELECT * FROM foo
                     UNION ALL
                     SELECT * FROM bar
                     INTERSECT
                     SELECT * FROM qux
                     ORDER BY
                     AND WHERE TRUE
                     """
                 ),
                 Arg(
                     trait2,
                     SqlNode.SortTrait(),
                     """
                     SELECT * FROM foo
                     UNION ALL
                     SELECT * FROM bar
                     INTERSECT
                     SELECT * FROM qux
                     DISTINCT
                     ORDER BY
                     """
                 )
             })
        };

        var result = new TheoryData<SqlQueryExpressionNode, (SqlNodeBase, SqlNodeBase)?, bool, string>();
        foreach ( var (d, args) in data )
        {
            var unchanged = d.ToString();
            result.Add( d, null, true, unchanged );
            result.Add( d, null, false, unchanged );
            foreach ( var (arg, replacement, changed) in args )
                result.Add( d, (arg, replacement), false, changed );
        }

        return result;

        [Pure]
        static (SqlNodeBase, SqlNodeBase, string) Arg(SqlNodeBase arg, SqlNodeBase replacement, string expected)
        {
            return (arg, replacement, expected);
        }
    }

    public static TheoryData<SqlNodeBase, (SqlNodeBase, SqlNodeBase)?, bool, string> GetDataModificationsData(IFixture fixture)
    {
        var values = SqlNode.Values( SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) );
        var query = SqlNode.RawQuery( "SELECT * FROM foo" );
        var set = SqlNode.RawRecordSet( "bar" );
        var field1 = set["x"];
        var field2 = set["y"];
        var field3 = set["w"];
        var field4 = set["v"];

        var data = new (SqlNodeBase, (SqlNodeBase, SqlNodeBase, string)[])[]
        {
            (values.ToInsertInto( set, field1, field2 ), new[]
            {
                Arg(
                    values,
                    query,
                    """
                    INSERT INTO [bar] ([bar].[x] : ?, [bar].[y] : ?)
                    SELECT * FROM foo
                    """
                ),
                Arg(
                    set,
                    SqlNode.RawRecordSet( "qux" ),
                    """
                    INSERT INTO [qux] ([qux].[x] : ?, [qux].[y] : ?)
                    VALUES
                    ((@a : ?), (@b : ?))
                    """
                ),
                Arg(
                    field1,
                    set["z"],
                    """
                    INSERT INTO [bar] ([bar].[z] : ?, [bar].[y] : ?)
                    VALUES
                    ((@a : ?), (@b : ?))
                    """
                ),
                Arg(
                    field2,
                    set["z"],
                    """
                    INSERT INTO [bar] ([bar].[x] : ?, [bar].[z] : ?)
                    VALUES
                    ((@a : ?), (@b : ?))
                    """
                )
            }),
            (query.ToInsertInto( set, field1, field2 ), new[]
            {
                Arg(
                    query,
                    values,
                    """
                    INSERT INTO [bar] ([bar].[x] : ?, [bar].[y] : ?)
                    VALUES
                    ((@a : ?), (@b : ?))
                    """
                ),
                Arg(
                    set,
                    SqlNode.RawRecordSet( "qux" ),
                    """
                    INSERT INTO [qux] ([qux].[x] : ?, [qux].[y] : ?)
                    SELECT * FROM foo
                    """
                ),
                Arg(
                    field1,
                    set["z"],
                    """
                    INSERT INTO [bar] ([bar].[z] : ?, [bar].[y] : ?)
                    SELECT * FROM foo
                    """
                ),
                Arg(
                    field2,
                    set["z"],
                    """
                    INSERT INTO [bar] ([bar].[x] : ?, [bar].[z] : ?)
                    SELECT * FROM foo
                    """
                )
            }),
            (set.ToDataSource().ToUpdate( field1.Assign( SqlNode.Literal( 1 ) ), field2.Assign( SqlNode.Literal( 2 ) ) ), new[]
            {
                Arg(
                    set,
                    SqlNode.RawRecordSet( "qux" ),
                    """
                    UPDATE FROM [qux]
                    SET
                      ([qux].[x] : ?) = ("1" : System.Int32),
                      ([qux].[y] : ?) = ("2" : System.Int32)
                    """
                ),
                Arg(
                    field1,
                    set["z"],
                    """
                    UPDATE FROM [bar]
                    SET
                      ([bar].[z] : ?) = ("1" : System.Int32),
                      ([bar].[y] : ?) = ("2" : System.Int32)
                    """
                ),
                Arg(
                    field2,
                    set["z"],
                    """
                    UPDATE FROM [bar]
                    SET
                      ([bar].[x] : ?) = ("1" : System.Int32),
                      ([bar].[z] : ?) = ("2" : System.Int32)
                    """
                )
            }),
            (values.ToUpsert(
                 set,
                 new[] { field1, field2 },
                 (_, i) => new[] { field1.Assign( i["x"] ), field2.Assign( i["y"] ) },
                 new[] { field3, field4 } ),
             new[]
             {
                 Arg(
                     values,
                     query,
                     """
                     UPSERT [bar] USING
                     SELECT * FROM foo
                     WITH CONFLICT TARGET ([bar].[w] : ?, [bar].[v] : ?)
                     INSERT ([bar].[x] : ?, [bar].[y] : ?)
                     ON CONFLICT SET
                       ([bar].[x] : ?) = ([<internal>].[x] : ?),
                       ([bar].[y] : ?) = ([<internal>].[y] : ?)
                     """
                 ),
                 Arg(
                     set,
                     SqlNode.RawRecordSet( "qux" ),
                     """
                     UPSERT [qux] USING
                     VALUES
                     ((@a : ?), (@b : ?))
                     WITH CONFLICT TARGET ([qux].[w] : ?, [qux].[v] : ?)
                     INSERT ([qux].[x] : ?, [qux].[y] : ?)
                     ON CONFLICT SET
                       ([qux].[x] : ?) = ([<internal>].[x] : ?),
                       ([qux].[y] : ?) = ([<internal>].[y] : ?)
                     """
                 ),
                 Arg(
                     field1,
                     set["z"],
                     """
                     UPSERT [bar] USING
                     VALUES
                     ((@a : ?), (@b : ?))
                     WITH CONFLICT TARGET ([bar].[w] : ?, [bar].[v] : ?)
                     INSERT ([bar].[z] : ?, [bar].[y] : ?)
                     ON CONFLICT SET
                       ([bar].[z] : ?) = ([<internal>].[x] : ?),
                       ([bar].[y] : ?) = ([<internal>].[y] : ?)
                     """
                 ),
                 Arg(
                     field2,
                     set["z"],
                     """
                     UPSERT [bar] USING
                     VALUES
                     ((@a : ?), (@b : ?))
                     WITH CONFLICT TARGET ([bar].[w] : ?, [bar].[v] : ?)
                     INSERT ([bar].[x] : ?, [bar].[z] : ?)
                     ON CONFLICT SET
                       ([bar].[x] : ?) = ([<internal>].[x] : ?),
                       ([bar].[z] : ?) = ([<internal>].[y] : ?)
                     """
                 ),
                 Arg(
                     field3,
                     set["z"],
                     """
                     UPSERT [bar] USING
                     VALUES
                     ((@a : ?), (@b : ?))
                     WITH CONFLICT TARGET ([bar].[z] : ?, [bar].[v] : ?)
                     INSERT ([bar].[x] : ?, [bar].[y] : ?)
                     ON CONFLICT SET
                       ([bar].[x] : ?) = ([<internal>].[x] : ?),
                       ([bar].[y] : ?) = ([<internal>].[y] : ?)
                     """
                 ),
                 Arg(
                     field4,
                     set["z"],
                     """
                     UPSERT [bar] USING
                     VALUES
                     ((@a : ?), (@b : ?))
                     WITH CONFLICT TARGET ([bar].[w] : ?, [bar].[z] : ?)
                     INSERT ([bar].[x] : ?, [bar].[y] : ?)
                     ON CONFLICT SET
                       ([bar].[x] : ?) = ([<internal>].[x] : ?),
                       ([bar].[y] : ?) = ([<internal>].[y] : ?)
                     """
                 )
             }),
            (query.ToUpsert(
                 set,
                 new[] { field1, field2 },
                 (_, i) => new[] { field1.Assign( i["x"] ), field2.Assign( i["y"] ) },
                 new[] { field3, field4 } ),
             new[]
             {
                 Arg(
                     query,
                     values,
                     """
                     UPSERT [bar] USING
                     VALUES
                     ((@a : ?), (@b : ?))
                     WITH CONFLICT TARGET ([bar].[w] : ?, [bar].[v] : ?)
                     INSERT ([bar].[x] : ?, [bar].[y] : ?)
                     ON CONFLICT SET
                       ([bar].[x] : ?) = ([<internal>].[x] : ?),
                       ([bar].[y] : ?) = ([<internal>].[y] : ?)
                     """
                 ),
                 Arg(
                     set,
                     SqlNode.RawRecordSet( "qux" ),
                     """
                     UPSERT [qux] USING
                     SELECT * FROM foo
                     WITH CONFLICT TARGET ([qux].[w] : ?, [qux].[v] : ?)
                     INSERT ([qux].[x] : ?, [qux].[y] : ?)
                     ON CONFLICT SET
                       ([qux].[x] : ?) = ([<internal>].[x] : ?),
                       ([qux].[y] : ?) = ([<internal>].[y] : ?)
                     """
                 ),
                 Arg(
                     field1,
                     set["z"],
                     """
                     UPSERT [bar] USING
                     SELECT * FROM foo
                     WITH CONFLICT TARGET ([bar].[w] : ?, [bar].[v] : ?)
                     INSERT ([bar].[z] : ?, [bar].[y] : ?)
                     ON CONFLICT SET
                       ([bar].[z] : ?) = ([<internal>].[x] : ?),
                       ([bar].[y] : ?) = ([<internal>].[y] : ?)
                     """
                 ),
                 Arg(
                     field2,
                     set["z"],
                     """
                     UPSERT [bar] USING
                     SELECT * FROM foo
                     WITH CONFLICT TARGET ([bar].[w] : ?, [bar].[v] : ?)
                     INSERT ([bar].[x] : ?, [bar].[z] : ?)
                     ON CONFLICT SET
                       ([bar].[x] : ?) = ([<internal>].[x] : ?),
                       ([bar].[z] : ?) = ([<internal>].[y] : ?)
                     """
                 ),
                 Arg(
                     field3,
                     set["z"],
                     """
                     UPSERT [bar] USING
                     SELECT * FROM foo
                     WITH CONFLICT TARGET ([bar].[z] : ?, [bar].[v] : ?)
                     INSERT ([bar].[x] : ?, [bar].[y] : ?)
                     ON CONFLICT SET
                       ([bar].[x] : ?) = ([<internal>].[x] : ?),
                       ([bar].[y] : ?) = ([<internal>].[y] : ?)
                     """
                 ),
                 Arg(
                     field4,
                     set["z"],
                     """
                     UPSERT [bar] USING
                     SELECT * FROM foo
                     WITH CONFLICT TARGET ([bar].[w] : ?, [bar].[z] : ?)
                     INSERT ([bar].[x] : ?, [bar].[y] : ?)
                     ON CONFLICT SET
                       ([bar].[x] : ?) = ([<internal>].[x] : ?),
                       ([bar].[y] : ?) = ([<internal>].[y] : ?)
                     """
                 )
             }),
            (set.ToDataSource().ToDeleteFrom(), new[] { Arg( set, SqlNode.RawRecordSet( "qux" ), "DELETE FROM [qux]" ) }),
            (set.ToTruncate(), new[] { Arg( set, SqlNode.RawRecordSet( "qux" ), "TRUNCATE [qux]" ) })
        };

        var result = new TheoryData<SqlNodeBase, (SqlNodeBase, SqlNodeBase)?, bool, string>();
        foreach ( var (node, args) in data )
        {
            var unchanged = node.ToString();
            result.Add( node, null, true, unchanged );
            result.Add( node, null, false, unchanged );
            foreach ( var (arg, replacement, changed) in args )
                result.Add( node, (arg, replacement), false, changed );
        }

        return result;

        [Pure]
        static (SqlNodeBase, SqlNodeBase, string) Arg(SqlNodeBase arg, SqlNodeBase replacement, string expected)
        {
            return (arg, replacement, expected);
        }
    }

    public static TheoryData<SqlNodeBase, (SqlNodeBase, SqlNodeBase)?, bool, string> GetSchemaModificationsData(IFixture fixture)
    {
        var expr1 = SqlNode.Parameter( "a" );
        var expr2 = SqlNode.Parameter( "b" );
        var cond = SqlNode.RawCondition( "a > 0" );
        var set1 = SqlNode.RawRecordSet( "foo" );
        var set2 = SqlNode.RawRecordSet( "bar" );
        var field1 = set1["x"];
        var field2 = set1["y"];
        var field3 = set2["v"];
        var field4 = set2["w"];
        var query = SqlNode.RawQuery( "SELECT * FROM foo" );
        var createTable = SqlNode.CreateTable(
            SqlRecordSetInfo.Create( "a" ),
            new[] { SqlNode.Column<int>( "b", defaultValue: expr1 ), SqlNode.Column<string>( "c", defaultValue: expr2 ) },
            constraintsProvider: t => SqlCreateTableConstraints.Empty.WithPrimaryKey(
                    SqlNode.PrimaryKey( SqlSchemaObjectName.Create( "pk" ), new[] { t["b"].Asc() } ) )
                .WithForeignKeys(
                    SqlNode.ForeignKey(
                        SqlSchemaObjectName.Create( "fk" ),
                        new SqlDataFieldNode[] { t["b"] },
                        set2,
                        new SqlDataFieldNode[] { field3 } ) )
                .WithChecks( SqlNode.Check( SqlSchemaObjectName.Create( "chk" ), cond ) ) );

        var data = new (SqlNodeBase, (SqlNodeBase, SqlNodeBase, string)[])[]
        {
            (SqlNode.Column<int>( "a", defaultValue: expr1 ),
             new[] { Arg( expr1, SqlNode.Literal( 1 ), "[a] : System.Int32 DEFAULT (\"1\" : System.Int32)" ) }),
            (SqlNode.Column<int>( "a", computation: SqlColumnComputation.Stored( expr1 ) ),
             new[] { Arg( expr1, SqlNode.Literal( 1 ), "[a] : System.Int32 GENERATED (\"1\" : System.Int32) STORED" ) }),
            (SqlNode.Column<int>( "a", defaultValue: expr1, computation: SqlColumnComputation.Virtual( expr2 ) ),
             new[]
             {
                 Arg( expr1, SqlNode.Literal( 1 ), "[a] : System.Int32 DEFAULT (\"1\" : System.Int32) GENERATED (@b : ?) VIRTUAL" ),
                 Arg( expr2, SqlNode.Literal( 1 ), "[a] : System.Int32 DEFAULT (@a : ?) GENERATED (\"1\" : System.Int32) VIRTUAL" )
             }),
            (SqlNode.PrimaryKey( SqlSchemaObjectName.Create( "a" ), new[] { expr1.Asc(), expr2.Desc() } ), new[]
            {
                Arg( expr1, SqlNode.Literal( 1 ), "PRIMARY KEY [a] ((\"1\" : System.Int32) ASC, (@b : ?) DESC)" ),
                Arg( expr2, SqlNode.Literal( 1 ), "PRIMARY KEY [a] ((@a : ?) ASC, (\"1\" : System.Int32) DESC)" ),
            }),
            (SqlNode.ForeignKey(
                 SqlSchemaObjectName.Create( "a" ),
                 new SqlDataFieldNode[] { field1, field2 },
                 set2,
                 new SqlDataFieldNode[] { field3, field4 },
                 ReferenceBehavior.SetNull,
                 ReferenceBehavior.NoAction ),
             new[]
             {
                 Arg(
                     field1,
                     set1["z"],
                     "FOREIGN KEY [a] (([foo].[z] : ?), ([foo].[y] : ?)) REFERENCES [bar] (([bar].[v] : ?), ([bar].[w] : ?)) ON DELETE SET NULL ON UPDATE NO ACTION" ),
                 Arg(
                     field2,
                     set1["z"],
                     "FOREIGN KEY [a] (([foo].[x] : ?), ([foo].[z] : ?)) REFERENCES [bar] (([bar].[v] : ?), ([bar].[w] : ?)) ON DELETE SET NULL ON UPDATE NO ACTION" ),
                 Arg(
                     set2,
                     SqlNode.RawRecordSet( "qux" ),
                     "FOREIGN KEY [a] (([foo].[x] : ?), ([foo].[y] : ?)) REFERENCES [qux] (([qux].[v] : ?), ([qux].[w] : ?)) ON DELETE SET NULL ON UPDATE NO ACTION" ),
                 Arg(
                     field3,
                     set2["z"],
                     "FOREIGN KEY [a] (([foo].[x] : ?), ([foo].[y] : ?)) REFERENCES [bar] (([bar].[z] : ?), ([bar].[w] : ?)) ON DELETE SET NULL ON UPDATE NO ACTION" ),
                 Arg(
                     field4,
                     set2["z"],
                     "FOREIGN KEY [a] (([foo].[x] : ?), ([foo].[y] : ?)) REFERENCES [bar] (([bar].[v] : ?), ([bar].[z] : ?)) ON DELETE SET NULL ON UPDATE NO ACTION" ),
             }),
            (SqlNode.Check( SqlSchemaObjectName.Create( "a" ), cond ), new[] { Arg( cond, SqlNode.True(), "CHECK [a] (TRUE)" ) }),
            (SqlNode.CreateTable(
                 SqlRecordSetInfo.Create( "a" ),
                 new[] { SqlNode.Column<int>( "b", defaultValue: expr1 ) },
                 ifNotExists: true ),
             new[]
             {
                 Arg(
                     expr1,
                     SqlNode.Literal( 1 ),
                     """
                     CREATE TABLE IF NOT EXISTS [a] (
                       [b] : System.Int32 DEFAULT ("1" : System.Int32)
                     )
                     """
                 )
             }),
            (createTable,
             new[]
             {
                 Arg(
                     expr1,
                     SqlNode.Literal( 1 ),
                     """
                     CREATE TABLE [a] (
                       [b] : System.Int32 DEFAULT ("1" : System.Int32),
                       [c] : System.String DEFAULT (@b : ?),
                       PRIMARY KEY [pk] (([a].[b] : System.Int32) ASC),
                       FOREIGN KEY [fk] (([a].[b] : System.Int32)) REFERENCES [bar] (([bar].[v] : ?)) ON DELETE RESTRICT ON UPDATE RESTRICT,
                       CHECK [chk] (a > 0)
                     )
                     """
                 ),
                 Arg(
                     expr2,
                     SqlNode.Literal( 1 ),
                     """
                     CREATE TABLE [a] (
                       [b] : System.Int32 DEFAULT (@a : ?),
                       [c] : System.String DEFAULT ("1" : System.Int32),
                       PRIMARY KEY [pk] (([a].[b] : System.Int32) ASC),
                       FOREIGN KEY [fk] (([a].[b] : System.Int32)) REFERENCES [bar] (([bar].[v] : ?)) ON DELETE RESTRICT ON UPDATE RESTRICT,
                       CHECK [chk] (a > 0)
                     )
                     """
                 ),
                 Arg(
                     createTable.PrimaryKey!.Columns[0].Expression,
                     createTable.RecordSet["b"],
                     """
                     CREATE TABLE [a] (
                       [b] : System.Int32 DEFAULT (@a : ?),
                       [c] : System.String DEFAULT (@b : ?),
                       PRIMARY KEY [pk] (([a].[b] : System.Int32) ASC),
                       FOREIGN KEY [fk] (([a].[b] : System.Int32)) REFERENCES [bar] (([bar].[v] : ?)) ON DELETE RESTRICT ON UPDATE RESTRICT,
                       CHECK [chk] (a > 0)
                     )
                     """
                 ),
                 Arg(
                     createTable.ForeignKeys[0].ReferencedTable,
                     set1,
                     """
                     CREATE TABLE [a] (
                       [b] : System.Int32 DEFAULT (@a : ?),
                       [c] : System.String DEFAULT (@b : ?),
                       PRIMARY KEY [pk] (([a].[b] : System.Int32) ASC),
                       FOREIGN KEY [fk] (([a].[b] : System.Int32)) REFERENCES [foo] (([foo].[v] : ?)) ON DELETE RESTRICT ON UPDATE RESTRICT,
                       CHECK [chk] (a > 0)
                     )
                     """
                 ),
                 Arg(
                     createTable.Checks[0].Condition,
                     SqlNode.True(),
                     """
                     CREATE TABLE [a] (
                       [b] : System.Int32 DEFAULT (@a : ?),
                       [c] : System.String DEFAULT (@b : ?),
                       PRIMARY KEY [pk] (([a].[b] : System.Int32) ASC),
                       FOREIGN KEY [fk] (([a].[b] : System.Int32)) REFERENCES [bar] (([bar].[v] : ?)) ON DELETE RESTRICT ON UPDATE RESTRICT,
                       CHECK [chk] (TRUE)
                     )
                     """
                 )
             }),
            (SqlNode.CreateView( SqlRecordSetInfo.Create( "a" ), query ), new[]
            {
                Arg(
                    query,
                    SqlNode.RawQuery( "SELECT * FROM bar" ),
                    """
                    CREATE VIEW [a] AS
                    SELECT * FROM bar
                    """
                )
            }),
            (SqlNode.CreateIndex(
                 SqlSchemaObjectName.Create( "a" ),
                 isUnique: false,
                 set1,
                 new[] { field1.Asc(), field2.Desc() },
                 replaceIfExists: true ),
             new[]
             {
                 Arg(
                     set1,
                     set2,
                     "CREATE OR REPLACE INDEX [a] ON [bar] (([bar].[x] : ?) ASC, ([bar].[y] : ?) DESC)"
                 ),
                 Arg(
                     field1,
                     field2,
                     "CREATE OR REPLACE INDEX [a] ON [foo] (([foo].[y] : ?) ASC, ([foo].[y] : ?) DESC)"
                 ),
                 Arg(
                     field2,
                     field1,
                     "CREATE OR REPLACE INDEX [a] ON [foo] (([foo].[x] : ?) ASC, ([foo].[x] : ?) DESC)"
                 )
             }),
            (SqlNode.CreateIndex( SqlSchemaObjectName.Create( "a" ), isUnique: true, set1, new[] { field1.Asc() }, filter: cond ),
             new[] { Arg( cond, SqlNode.True(), "CREATE UNIQUE INDEX [a] ON [foo] (([foo].[x] : ?) ASC) WHERE (TRUE)" ) }),
            (SqlNode.AddColumn( SqlRecordSetInfo.Create( "a" ), SqlNode.Column<int>( "b", defaultValue: expr1 ) ),
             new[] { Arg( expr1, SqlNode.Literal( 1 ), "ADD COLUMN [a].[b] : System.Int32 DEFAULT (\"1\" : System.Int32)" ) })
        };

        var result = new TheoryData<SqlNodeBase, (SqlNodeBase, SqlNodeBase)?, bool, string>();
        foreach ( var (node, args) in data )
        {
            var unchanged = node.ToString();
            result.Add( node, null, true, unchanged );
            result.Add( node, null, false, unchanged );
            foreach ( var (arg, replacement, changed) in args )
                result.Add( node, (arg, replacement), false, changed );
        }

        return result;

        [Pure]
        static (SqlNodeBase, SqlNodeBase, string) Arg(SqlNodeBase arg, SqlNodeBase replacement, string expected)
        {
            return (arg, replacement, expected);
        }
    }

    public static TheoryData<SqlNodeBase, (SqlNodeBase, SqlNodeBase)?, bool, string> GetMiscData(IFixture fixture)
    {
        var expr1 = SqlNode.Parameter( "a" );
        var expr2 = SqlNode.Parameter( "b" );
        var expr3 = SqlNode.Parameter( "c" );
        var expr4 = SqlNode.Parameter( "d" );
        var cond = SqlNode.RawCondition( "x > 0" );
        var set = SqlNode.RawRecordSet( "foo" );
        var case1 = SqlNode.SwitchCase( cond, expr1 );
        var case2 = SqlNode.SwitchCase( SqlNode.RawCondition( "y < 0" ), SqlNode.Parameter( "e" ) );
        var query1 = SqlNode.RawQuery( "SELECT * FROM foo" );
        var query2 = SqlNode.RawQuery( "SELECT * FROM bar" );
        var recursiveCte = query1.ToCte( "u" ).ToRecursive( query2.ToCompound( SqlCompoundQueryOperator.UnionAll ) );
        var frame = SqlNode.RowsWindowFrame( SqlWindowFrameBoundary.CurrentRow, SqlWindowFrameBoundary.CurrentRow );

        var data = new (SqlNodeBase, (SqlNodeBase, SqlNodeBase, string)[])[]
        {
            (SqlNode.SwitchCase( cond, expr1 ), new[]
            {
                Arg(
                    cond,
                    SqlNode.True(),
                    $"""
                     WHEN TRUE
                       THEN ({expr1})
                     """ ),
                Arg(
                    expr1,
                    SqlNode.Literal( 1 ),
                    $"""
                     WHEN {cond}
                       THEN ("1" : System.Int32)
                     """ )
            }),
            (SqlNode.Switch( new[] { case1, case2 }, expr2 ), new[]
            {
                Arg(
                    case1,
                    SqlNode.SwitchCase( SqlNode.True(), SqlNode.Literal( 1 ) ),
                    $"""
                     CASE
                       WHEN TRUE
                         THEN ("1" : System.Int32)
                       WHEN y < 0
                         THEN (@e : ?)
                       ELSE ({expr2})
                     END
                     """
                ),
                Arg(
                    case2,
                    SqlNode.SwitchCase( SqlNode.True(), SqlNode.Literal( 1 ) ),
                    $"""
                     CASE
                       WHEN x > 0
                         THEN (@a : ?)
                       WHEN TRUE
                         THEN ("1" : System.Int32)
                       ELSE ({expr2})
                     END
                     """
                ),
                Arg(
                    expr2,
                    SqlNode.Literal( 1 ),
                    """
                    CASE
                      WHEN x > 0
                        THEN (@a : ?)
                      WHEN y < 0
                        THEN (@e : ?)
                      ELSE ("1" : System.Int32)
                    END
                    """
                )
            }),
            (cond.ToValue(), new[] { Arg( cond, SqlNode.True(), "CONDITION_VALUE(TRUE)" ) }),
            (SqlNode.InnerJoinOn( set, cond ), new[]
            {
                Arg( set, SqlNode.RawRecordSet( "bar" ), $"INNER JOIN [bar] ON {cond}" ),
                Arg( cond, SqlNode.True(), $"INNER JOIN {set} ON TRUE" )
            }),
            (SqlNode.LeftJoinOn( set, cond ), new[]
            {
                Arg( set, SqlNode.RawRecordSet( "bar" ), $"LEFT JOIN [bar] ON {cond}" ),
                Arg( cond, SqlNode.True(), $"LEFT JOIN {set} ON TRUE" )
            }),
            (SqlNode.RightJoinOn( set, cond ), new[]
            {
                Arg( set, SqlNode.RawRecordSet( "bar" ), $"RIGHT JOIN [bar] ON {cond}" ),
                Arg( cond, SqlNode.True(), $"RIGHT JOIN {set} ON TRUE" )
            }),
            (SqlNode.FullJoinOn( set, cond ), new[]
            {
                Arg( set, SqlNode.RawRecordSet( "bar" ), $"FULL JOIN [bar] ON {cond}" ),
                Arg( cond, SqlNode.True(), $"FULL JOIN {set} ON TRUE" )
            }),
            (SqlNode.CrossJoin( set ), new[] { Arg( set, SqlNode.RawRecordSet( "bar" ), "CROSS JOIN [bar]" ) }),
            (expr1.As( "x" ), new[] { Arg( expr1, SqlNode.Literal( 1 ), "(\"1\" : System.Int32) AS [x]" ) }),
            (set["x"].AsSelf(), new[] { Arg( set, SqlNode.RawRecordSet( "bar" ), "([bar].[x] : ?)" ) }),
            (set.GetAll(), new[] { Arg( set, SqlNode.RawRecordSet( "bar" ), "[bar].*" ) }),
            (set.ToDataSource().GetAll(), new[] { Arg( set, SqlNode.RawRecordSet( "bar" ), "*" ) }),
            (set.GetAll().ToExpression(), new[] { Arg( set, SqlNode.RawRecordSet( "bar" ), "[bar].*" ) }),
            (query1.ToCompound( SqlCompoundQueryOperator.UnionAll ),
             new[]
             {
                 Arg(
                     query1,
                     SqlNode.RawQuery( "SELECT * FROM qux" ),
                     """
                     UNION ALL
                     SELECT * FROM qux
                     """
                 )
             }),
            (query1.ToCompound( SqlCompoundQueryOperator.Intersect ),
             new[]
             {
                 Arg(
                     query1,
                     SqlNode.RawQuery( "SELECT * FROM qux" ),
                     """
                     INTERSECT
                     SELECT * FROM qux
                     """
                 )
             }),
            (SqlNode.FilterTrait( cond, true ), new[] { Arg( cond, SqlNode.True(), "AND WHERE TRUE" ) }),
            (SqlNode.FilterTrait( cond, false ), new[] { Arg( cond, SqlNode.True(), "OR WHERE TRUE" ) }),
            (SqlNode.AggregationTrait( expr1, expr2 ), new[]
            {
                Arg( expr1, SqlNode.Literal( 1 ), $"GROUP BY (\"1\" : System.Int32), ({expr2})" ),
                Arg( expr2, SqlNode.Literal( 1 ), $"GROUP BY ({expr1}), (\"1\" : System.Int32)" )
            }),
            (SqlNode.AggregationFilterTrait( cond, true ), new[] { Arg( cond, SqlNode.True(), "AND HAVING TRUE" ) }),
            (SqlNode.AggregationFilterTrait( cond, false ), new[] { Arg( cond, SqlNode.True(), "OR HAVING TRUE" ) }),
            (SqlNode.SortTrait( expr1.Asc(), expr2.Desc() ), new[]
            {
                Arg( expr1, SqlNode.Literal( 1 ), $"ORDER BY (\"1\" : System.Int32) ASC, ({expr2}) DESC" ),
                Arg( expr2, SqlNode.Literal( 1 ), $"ORDER BY ({expr1}) ASC, (\"1\" : System.Int32) DESC" ),
            }),
            (SqlNode.LimitTrait( expr1 ), new[] { Arg( expr1, SqlNode.Literal( 1 ), "LIMIT (\"1\" : System.Int32)" ) }),
            (SqlNode.OffsetTrait( expr1 ), new[] { Arg( expr1, SqlNode.Literal( 1 ), "OFFSET (\"1\" : System.Int32)" ) }),
            (SqlNode.CommonTableExpressionTrait( query1.ToCte( "u" ), query2.ToCte( "v" ) ), new[]
            {
                Arg(
                    query1,
                    SqlNode.RawQuery( "SELECT * FROM qux" ),
                    """
                    WITH ORDINAL [u] (
                      SELECT * FROM qux
                    ),
                    ORDINAL [v] (
                      SELECT * FROM bar
                    )
                    """
                ),
                Arg(
                    query2,
                    SqlNode.RawQuery( "SELECT * FROM qux" ),
                    """
                    WITH ORDINAL [u] (
                      SELECT * FROM foo
                    ),
                    ORDINAL [v] (
                      SELECT * FROM qux
                    )
                    """
                )
            }),
            (SqlNode.WindowDefinitionTrait(
                 SqlNode.WindowDefinition( "u", new[] { expr1.Asc() } ),
                 SqlNode.WindowDefinition( "v", new[] { expr2.Desc() } ) ),
             new[]
             {
                 Arg(
                     expr1,
                     SqlNode.Literal( 1 ),
                     """
                     WINDOW [u] AS (ORDER BY ("1" : System.Int32) ASC),
                       [v] AS (ORDER BY (@b : ?) DESC)
                     """
                 ),
                 Arg(
                     expr2,
                     SqlNode.Literal( 1 ),
                     """
                     WINDOW [u] AS (ORDER BY (@a : ?) ASC),
                       [v] AS (ORDER BY ("1" : System.Int32) DESC)
                     """
                 )
             }),
            (SqlNode.WindowTrait( SqlNode.WindowDefinition( "u", new[] { expr1.Asc() } ) ),
             new[]
             {
                 Arg(
                     expr1,
                     SqlNode.Literal( 1 ),
                     """
                     OVER [u] AS (ORDER BY ("1" : System.Int32) ASC)
                     """
                 )
             }),
            (expr1.Asc(), new[] { Arg( expr1, SqlNode.Literal( 1 ), "(\"1\" : System.Int32) ASC" ) }),
            (expr1.Desc(), new[] { Arg( expr1, SqlNode.Literal( 1 ), "(\"1\" : System.Int32) DESC" ) }),
            (query1.ToCte( "u" ), new[]
            {
                Arg(
                    query1,
                    SqlNode.RawQuery( "SELECT * FROM qux" ),
                    """
                    ORDINAL [u] (
                      SELECT * FROM qux
                    )
                    """
                )
            }),
            (recursiveCte, new[]
            {
                Arg(
                    recursiveCte.Query,
                    SqlNode.RawQuery( "SELECT * FROM qux" )
                        .CompoundWith( SqlNode.RawQuery( "SELECT * FROM lorem" ).ToCompound( SqlCompoundQueryOperator.Union ) ),
                    """
                    RECURSIVE [u] (
                      
                      SELECT * FROM qux
                    
                      UNION
                      
                      SELECT * FROM lorem

                    )
                    """
                )
            }),
            (SqlNode.WindowDefinition( "w", new SqlExpressionNode[] { expr1, expr2 }, new[] { expr3.Asc(), expr4.Desc() } ), new[]
            {
                Arg(
                    expr1,
                    SqlNode.Literal( 1 ),
                    $"""
                     [w] AS (PARTITION BY ("1" : System.Int32), ({expr2}) ORDER BY ({expr3}) ASC, ({expr4}) DESC)
                     """
                ),
                Arg(
                    expr2,
                    SqlNode.Literal( 1 ),
                    $"""
                     [w] AS (PARTITION BY ({expr1}), ("1" : System.Int32) ORDER BY ({expr3}) ASC, ({expr4}) DESC)
                     """
                ),
                Arg(
                    expr3,
                    SqlNode.Literal( 1 ),
                    $"""
                     [w] AS (PARTITION BY ({expr1}), ({expr2}) ORDER BY ("1" : System.Int32) ASC, ({expr4}) DESC)
                     """
                ),
                Arg(
                    expr4,
                    SqlNode.Literal( 1 ),
                    $"""
                     [w] AS (PARTITION BY ({expr1}), ({expr2}) ORDER BY ({expr3}) ASC, ("1" : System.Int32) DESC)
                     """
                ),
            }),
            (SqlNode.WindowDefinition( "w", new SqlExpressionNode[] { expr1 }, new[] { expr2.Asc() }, frame ), new[]
            {
                Arg(
                    frame,
                    SqlNode.RangeWindowFrame( SqlWindowFrameBoundary.UnboundedPreceding, SqlWindowFrameBoundary.UnboundedFollowing ),
                    $"""
                     [w] AS (PARTITION BY ({expr1}) ORDER BY ({expr2}) ASC RANGE BETWEEN UNBOUNDED PRECEDING AND UNBOUNDED FOLLOWING)
                     """
                ),
            }),
            (SqlNode.Values( expr1, expr2 ), new[]
            {
                Arg(
                    expr1,
                    SqlNode.Literal( 1 ),
                    $"""
                     VALUES
                     (("1" : System.Int32), ({expr2}))
                     """
                ),
                Arg(
                    expr2,
                    SqlNode.Literal( 1 ),
                    $"""
                     VALUES
                     (({expr1}), ("1" : System.Int32))
                     """
                ),
            }),
            (SqlNode.Values( new SqlExpressionNode[,] { { expr1, expr2 }, { expr3, expr4 } } ), new[]
            {
                Arg(
                    expr1,
                    SqlNode.Literal( 1 ),
                    $"""
                     VALUES
                     (("1" : System.Int32), ({expr2})),
                     (({expr3}), ({expr4}))
                     """
                ),
                Arg(
                    expr2,
                    SqlNode.Literal( 1 ),
                    $"""
                     VALUES
                     (({expr1}), ("1" : System.Int32)),
                     (({expr3}), ({expr4}))
                     """
                ),
                Arg(
                    expr3,
                    SqlNode.Literal( 1 ),
                    $"""
                     VALUES
                     (({expr1}), ({expr2})),
                     (("1" : System.Int32), ({expr4}))
                     """
                ),
                Arg(
                    expr4,
                    SqlNode.Literal( 1 ),
                    $"""
                     VALUES
                     (({expr1}), ({expr2})),
                     (({expr3}), ("1" : System.Int32))
                     """
                ),
            }),
            (set["x"].Assign( expr1 ), new[]
            {
                Arg( set, SqlNode.RawRecordSet( "bar" ), $"([bar].[x] : ?) = ({expr1})" ),
                Arg( expr1, SqlNode.Literal( 1 ), "([foo].[x] : ?) = (\"1\" : System.Int32)" )
            }),
            (SqlNode.Batch( query1, query2 ), new[]
            {
                Arg(
                    query1,
                    SqlNode.RawQuery( "SELECT * FROM qux" ),
                    """
                    BATCH
                    (
                      SELECT * FROM qux;
                    
                      SELECT * FROM bar;
                    )
                    """
                ),
                Arg(
                    query2,
                    SqlNode.RawQuery( "SELECT * FROM qux" ),
                    """
                    BATCH
                    (
                      SELECT * FROM foo;
                    
                      SELECT * FROM qux;
                    )
                    """
                ),
            })
        };

        var result = new TheoryData<SqlNodeBase, (SqlNodeBase, SqlNodeBase)?, bool, string>();
        foreach ( var (node, args) in data )
        {
            var unchanged = node.ToString();
            result.Add( node, null, true, unchanged );
            result.Add( node, null, false, unchanged );
            foreach ( var (arg, replacement, changed) in args )
                result.Add( node, (arg, replacement), false, changed );
        }

        return result;

        [Pure]
        static (SqlNodeBase, SqlNodeBase, string) Arg(SqlNodeBase arg, SqlNodeBase replacement, string expected)
        {
            return (arg, replacement, expected);
        }
    }
}
