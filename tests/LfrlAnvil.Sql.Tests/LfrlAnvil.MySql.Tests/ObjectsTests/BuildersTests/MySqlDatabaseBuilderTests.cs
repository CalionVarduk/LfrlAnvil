using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.MySql.Tests.ObjectsTests.BuildersTests;

public partial class MySqlDatabaseBuilderTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateBuilderWithDefaultSchema()
    {
        var sut = MySqlDatabaseBuilderMock.Create();

        using ( new AssertionScope() )
        {
            sut.Schemas.Count.Should().Be( 1 );
            sut.Schemas.Database.Should().BeSameAs( sut );
            sut.Schemas.Should().BeEquivalentTo( sut.Schemas.Default );
            ((ISqlSchemaBuilderCollection)sut.Schemas).Default.Should().BeSameAs( sut.Schemas.Default );
            ((ISqlSchemaBuilderCollection)sut.Schemas).Database.Should().BeSameAs( sut.Schemas.Database );

            sut.Schemas.Default.Database.Should().BeSameAs( sut );
            sut.Schemas.Default.Name.Should().Be( "common" );
            ((ISqlSchemaBuilder)sut.Schemas.Default).Database.Should().BeSameAs( sut.Schemas.Default.Database );
            ((ISqlSchemaBuilder)sut.Schemas.Default).Objects.Should().BeSameAs( sut.Schemas.Default.Objects );

            sut.Schemas.Default.Objects.Count.Should().Be( 0 );
            sut.Schemas.Default.Objects.Should().BeEmpty();
            sut.Schemas.Default.Objects.Schema.Should().BeSameAs( sut.Schemas.Default );
            ((ISqlObjectBuilderCollection)sut.Schemas.Default.Objects).Schema.Should().BeSameAs( sut.Schemas.Default.Objects.Schema );

            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.IsAttached.Should().BeTrue();
            sut.GetPendingStatements().ToArray().Should().BeEmpty();
            sut.ServerVersion.Should().NotBeEmpty();

            ((ISqlDatabaseBuilder)sut).DataTypes.Should().BeSameAs( sut.DataTypes );
            ((ISqlDatabaseBuilder)sut).TypeDefinitions.Should().BeSameAs( sut.TypeDefinitions );
            ((ISqlDatabaseBuilder)sut).NodeInterpreters.Should().BeSameAs( sut.NodeInterpreters );
            ((ISqlDatabaseBuilder)sut).QueryReaders.Should().BeSameAs( sut.QueryReaders );
            ((ISqlDatabaseBuilder)sut).ParameterBinders.Should().BeSameAs( sut.ParameterBinders );
            ((ISqlDatabaseBuilder)sut).Schemas.Should().BeSameAs( sut.Schemas );
        }
    }

    [Fact]
    public void AddStatement_ShouldAddNewStatement_WhenThereAreNoPendingChanges()
    {
        var statement = Fixture.Create<string>();
        var sut = MySqlDatabaseBuilderMock.Create();

        sut.AddStatement( statement );
        var result = sut.GetPendingStatements().ToArray();

        result.Select( s => s.Sql ).Should().BeSequentiallyEqualTo( $"{statement}{Environment.NewLine}" );
    }

    [Fact]
    public void AddStatement_ShouldAddNewStatement_WhenThereAreSomePendingChanges()
    {
        var statement = Fixture.Create<string>();
        var sut = MySqlDatabaseBuilderMock.Create();
        sut.ChangeTracker.SetMode( SqlDatabaseCreateMode.Commit );

        var table = sut.Schemas.Default.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        table.Constraints.SetPrimaryKey( column.Asc() );

        sut.AddStatement( statement );
        var result = sut.GetPendingStatements().ToArray();

        using ( new AssertionScope() )
        {
            result.Should().HaveCount( 2 );
            result[^1].Sql.Should().Be( $"{statement}{Environment.NewLine}" );
        }
    }

    [Fact]
    public void AddStatement_ShouldDoNothing_WhenBuilderIsDetached()
    {
        var statement = Fixture.Create<string>();
        var sut = MySqlDatabaseBuilderMock.Create().SetDetachedMode();

        sut.AddStatement( statement );

        sut.GetPendingStatements().Length.Should().Be( 0 );
    }

    [Fact]
    public void AddStatement_ShouldDoNothing_WhenBuilderIsInNoChangesMode()
    {
        var statement = Fixture.Create<string>();
        var sut = MySqlDatabaseBuilderMock.Create();
        sut.ChangeTracker.SetMode( SqlDatabaseCreateMode.NoChanges );

        sut.AddStatement( statement );

        sut.GetPendingStatements().Length.Should().Be( 0 );
    }

    [Fact]
    public void AddStatement_ShouldThrowMySqlObjectBuilderException_WhenStatementContainsParameters()
    {
        var sut = MySqlDatabaseBuilderMock.Create();
        var action = Lambda.Of( () => sut.AddStatement( SqlNode.RawStatement( Fixture.Create<string>(), SqlNode.Parameter( "a" ) ) ) );
        action.Should().ThrowExactly<MySqlObjectBuilderException>();
    }

    [Fact]
    public void AddParameterizedStatement_TypeErased_ShouldAddNewStatement_WhenThereAreNoPendingChanges()
    {
        var statement = Fixture.Create<string>();
        var sut = MySqlDatabaseBuilderMock.Create();

        sut.AddParameterizedStatement(
            SqlNode.RawStatement( statement, SqlNode.Parameter( "a" ) ),
            new[] { KeyValuePair.Create( "a", (object?)1 ) }.AsEnumerable() );

        var result = sut.GetPendingStatements().ToArray();

        using ( new AssertionScope() )
        {
            result.Select( s => s.Sql ).Should().BeSequentiallyEqualTo( $"{statement}{Environment.NewLine}" );
            result.ElementAtOrDefault( 0 ).BeforeCallback.Should().NotBeNull();
        }
    }

    [Fact]
    public void AddParameterizedStatement_TypeErased_ShouldAddNewStatement_WhenThereAreSomePendingChanges()
    {
        var statement = Fixture.Create<string>();
        var sut = MySqlDatabaseBuilderMock.Create();
        sut.ChangeTracker.SetMode( SqlDatabaseCreateMode.Commit );

        var table = sut.Schemas.Default.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        table.Constraints.SetPrimaryKey( column.Asc() );

        sut.AddParameterizedStatement(
            SqlNode.RawStatement( statement, SqlNode.Parameter( "a" ) ),
            new[] { KeyValuePair.Create( "a", (object?)1 ) }.AsEnumerable() );

        var result = sut.GetPendingStatements().ToArray();

        using ( new AssertionScope() )
        {
            result.Should().HaveCount( 2 );
            result[^1].Sql.Should().Be( $"{statement}{Environment.NewLine}" );
            result[^1].BeforeCallback.Should().NotBeNull();
        }
    }

    [Fact]
    public void AddParameterizedStatement_TypeErased_ShouldDoNothing_WhenBuilderIsDetached()
    {
        var statement = Fixture.Create<string>();
        var sut = MySqlDatabaseBuilderMock.Create().SetDetachedMode();

        sut.AddParameterizedStatement(
            SqlNode.RawStatement( statement, SqlNode.Parameter( "a" ) ),
            new[] { KeyValuePair.Create( "a", (object?)1 ) }.AsEnumerable() );

        sut.GetPendingStatements().Length.Should().Be( 0 );
    }

    [Fact]
    public void AddParameterizedStatement_TypeErased_ShouldDoNothing_WhenBuilderIsInNoChangesMode()
    {
        var statement = Fixture.Create<string>();
        var sut = MySqlDatabaseBuilderMock.Create();
        sut.ChangeTracker.SetMode( SqlDatabaseCreateMode.NoChanges );

        sut.AddParameterizedStatement(
            SqlNode.RawStatement( statement, SqlNode.Parameter( "a" ) ),
            new[] { KeyValuePair.Create( "a", (object?)1 ) }.AsEnumerable() );

        sut.GetPendingStatements().Length.Should().Be( 0 );
    }

    [Fact]
    public void AddParameterizedStatement_Generic_ShouldAddNewStatement_WhenThereAreNoPendingChanges()
    {
        var statement = Fixture.Create<string>();
        var sut = MySqlDatabaseBuilderMock.Create();

        sut.AddParameterizedStatement( SqlNode.RawStatement( statement, SqlNode.Parameter<int>( "a" ) ), new Source { A = 1 } );
        var result = sut.GetPendingStatements().ToArray();

        using ( new AssertionScope() )
        {
            result.Select( s => s.Sql ).Should().BeSequentiallyEqualTo( $"{statement}{Environment.NewLine}" );
            result.ElementAtOrDefault( 0 ).BeforeCallback.Should().NotBeNull();
        }
    }

    [Fact]
    public void AddParameterizedStatement_Generic_ShouldAddNewStatement_WhenThereAreSomePendingChanges()
    {
        var statement = Fixture.Create<string>();
        var sut = MySqlDatabaseBuilderMock.Create();
        sut.ChangeTracker.SetMode( SqlDatabaseCreateMode.Commit );

        var table = sut.Schemas.Default.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        table.Constraints.SetPrimaryKey( column.Asc() );

        sut.AddParameterizedStatement( SqlNode.RawStatement( statement, SqlNode.Parameter<int>( "a" ) ), new Source { A = 1 } );
        var result = sut.GetPendingStatements().ToArray();

        using ( new AssertionScope() )
        {
            result.Should().HaveCount( 2 );
            result[^1].Sql.Should().Be( $"{statement}{Environment.NewLine}" );
            result[^1].BeforeCallback.Should().NotBeNull();
        }
    }

    [Fact]
    public void AddParameterizedStatement_Generic_ShouldDoNothing_WhenBuilderIsDetached()
    {
        var statement = Fixture.Create<string>();
        var sut = MySqlDatabaseBuilderMock.Create().SetDetachedMode();

        sut.AddParameterizedStatement( SqlNode.RawStatement( statement, SqlNode.Parameter<int>( "a" ) ), new Source { A = 1 } );

        sut.GetPendingStatements().Length.Should().Be( 0 );
    }

    [Fact]
    public void AddParameterizedStatement_Generic_ShouldDoNothing_WhenBuilderIsInNoChangesMode()
    {
        var statement = Fixture.Create<string>();
        var sut = MySqlDatabaseBuilderMock.Create();
        sut.ChangeTracker.SetMode( SqlDatabaseCreateMode.NoChanges );

        sut.AddParameterizedStatement( SqlNode.RawStatement( statement, SqlNode.Parameter<int>( "a" ) ), new Source { A = 1 } );

        sut.GetPendingStatements().Length.Should().Be( 0 );
    }

    [Fact]
    public void AddParameterizedStatement_ShouldThrowSqlCompilerException_WhenParametersAreInvalid()
    {
        var sut = MySqlDatabaseBuilderMock.Create();
        var action = Lambda.Of(
            () => sut.AddParameterizedStatement(
                SqlNode.RawStatement( Fixture.Create<string>(), SqlNode.Parameter<string>( "a" ) ),
                new Source { A = 1 } ) );

        action.Should().ThrowExactly<SqlCompilerException>();
    }

    [Fact]
    public void ObjectChanges_ShouldDoNothing_WhenBuilderIsDetached()
    {
        var sut = MySqlDatabaseBuilderMock.Create().SetDetachedMode();

        sut.Schemas.Default.SetName( "s" );
        var table = sut.Schemas.Default.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "D" ).Asc() ).MarkAsUnique().SetFilter( SqlNode.True() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;
        var fk = table.Constraints.CreateForeignKey( ix1, ix2 );
        table.Constraints.CreateCheck( table.RecordSet["C"] != SqlNode.Literal( 0 ) );
        fk.SetOnDeleteBehavior( ReferenceBehavior.Cascade ).SetOnUpdateBehavior( ReferenceBehavior.Cascade );
        var column = table.Columns.Create( "E" );
        column.SetName( "F" ).MarkAsNullable().SetType<int>().SetDefaultValue( 123 );
        table.SetName( "U" );
        column.Remove();

        sut.GetPendingStatements().Length.Should().Be( 0 );
    }

    [Fact]
    public void ObjectCreation_ShouldDoNothing_WhenBuilderIsInNoChangesMode()
    {
        var sut = MySqlDatabaseBuilderMock.Create();
        sut.ChangeTracker.SetMode( SqlDatabaseCreateMode.NoChanges );

        sut.Schemas.Default.SetName( "s" );
        var table = sut.Schemas.Default.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "D" ).Asc() ).MarkAsUnique().SetFilter( SqlNode.True() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;
        var fk = table.Constraints.CreateForeignKey( ix1, ix2 );
        table.Constraints.CreateCheck( table.RecordSet["C"] != SqlNode.Literal( 0 ) );
        fk.SetOnDeleteBehavior( ReferenceBehavior.Cascade ).SetOnUpdateBehavior( ReferenceBehavior.Cascade );
        var column = table.Columns.Create( "E" );
        column.SetName( "F" ).MarkAsNullable().SetType<string>().SetDefaultValue( "123" );
        table.SetName( "U" );
        column.Remove();

        sut.GetPendingStatements().Length.Should().Be( 0 );
    }

    [Fact]
    public void SetNodeInterpreterFactory_ShouldUpdateNodeInterpreterFactory()
    {
        var sut = MySqlDatabaseBuilderMock.Create();
        var expected = new MySqlNodeInterpreterFactory( sut.TypeDefinitions, "common" );

        var result = ((ISqlDatabaseBuilder)sut).SetNodeInterpreterFactory( expected );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.NodeInterpreters.Should().BeSameAs( expected );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetAttachedMode_ShouldUpdateIsAttached(bool enabled)
    {
        var sut = MySqlDatabaseBuilderMock.Create().SetAttachedMode( ! enabled );

        var result = ((ISqlDatabaseBuilder)sut).SetAttachedMode( enabled );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.IsAttached.Should().Be( enabled );
        }
    }

    [Fact]
    public void SetAttachedMode_ShouldDoNothing_WhenBuilderIsAlreadyAttached()
    {
        var sut = MySqlDatabaseBuilderMock.Create();

        var result = ((ISqlDatabaseBuilder)sut).SetAttachedMode();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.IsAttached.Should().BeTrue();
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetDetachedMode_ShouldUpdateIsAttached(bool enabled)
    {
        var sut = MySqlDatabaseBuilderMock.Create().SetDetachedMode( ! enabled );

        var result = ((ISqlDatabaseBuilder)sut).SetDetachedMode( enabled );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.IsAttached.Should().Be( ! enabled );
        }
    }

    [Fact]
    public void SetDetachedMode_ShouldDoNothing_WhenBuilderIsAlreadyDetached()
    {
        var sut = MySqlDatabaseBuilderMock.Create().SetDetachedMode();

        var result = ((ISqlDatabaseBuilder)sut).SetDetachedMode();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.IsAttached.Should().BeFalse();
        }
    }

    [Fact]
    public void DetachingBuilder_ShouldCompletePendingChanges()
    {
        var sut = MySqlDatabaseBuilderMock.Create();

        var table = sut.Schemas.Default.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        table.Constraints.SetPrimaryKey( column.Asc() );

        sut.SetDetachedMode();
        var result = sut.GetPendingStatements().ToArray();

        result.Should().HaveCount( 1 );
    }

    [Fact]
    public void ForMySql_ShouldInvokeAction_WhenDatabaseIsMySql()
    {
        var action = Substitute.For<Action<MySqlDatabaseBuilder>>();
        var sut = MySqlDatabaseBuilderMock.Create();

        var result = sut.ForMySql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void ForMySql_ShouldNotInvokeAction_WhenDatabaseIsNotMySql()
    {
        var action = Substitute.For<Action<MySqlDatabaseBuilder>>();
        var sut = Substitute.For<ISqlDatabaseBuilder>();

        var result = sut.ForMySql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallCount.Should().Be( 0 );
    }

    [Fact]
    public void DefaultNodeInterpreterFactory_ShouldCreateCorrectInterpreter()
    {
        var sut = MySqlDatabaseBuilderMock.Create().NodeInterpreters;
        var result = sut.Create();

        using ( new AssertionScope() )
        {
            result.Context.Sql.Capacity.Should().Be( 1024 );
            result.Context.Indent.Should().Be( 0 );
            result.Context.ChildDepth.Should().Be( 0 );
            result.Context.Parameters.Should().BeEmpty();
            result.Should().BeOfType( typeof( MySqlNodeInterpreter ) );
        }
    }

    public sealed class Source
    {
        public int A { get; init; }
    }
}
