using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ObjectsTests.BuildersTests;

public class SqlColumnBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Columns.Create( "C" );

        var result = sut.ToString();

        result.TestEquals( "[Column] foo.T.C" ).Go();
    }

    [Fact]
    public void Asc_ShouldReturnCorrectResult()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Columns.Create( "C" );

        var result = sut.Asc();

        Assertion.All(
                result.Expression.TestRefEquals( sut.Node ),
                result.Ordering.TestRefEquals( OrderBy.Asc ) )
            .Go();
    }

    [Fact]
    public void Asc_ShouldThrowSqlObjectBuilderException_WhenColumnIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Columns.Create( "C" );
        sut.Remove();

        var action = Lambda.Of( () => sut.Asc() );

        action.Test( exc => exc.TestType().Exact<SqlObjectBuilderException>() ).Go();
    }

    [Fact]
    public void Desc_ShouldReturnCorrectResult()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Columns.Create( "C" );

        var result = sut.Desc();

        Assertion.All(
                result.Expression.TestRefEquals( sut.Node ),
                result.Ordering.TestRefEquals( OrderBy.Desc ) )
            .Go();
    }

    [Fact]
    public void Desc_ShouldThrowSqlObjectBuilderException_WhenColumnIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Columns.Create( "C" );
        sut.Remove();

        var action = Lambda.Of( () => sut.Desc() );

        action.Test( exc => exc.TestType().Exact<SqlObjectBuilderException>() ).Go();
    }

    [Fact]
    public void Creation_ShouldMarkTableForAlteration()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Columns.Create( "C2" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                table.Columns.TryGet( sut.Name ).TestRefEquals( sut ),
                sut.Name.TestEquals( "C2" ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          CREATE [Column] foo.T.C2;
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void Creation_FollowedByRemove_ShouldDoNothing()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Columns.Create( "C2" );
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.TestEmpty().Go();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetName( sut.Name );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNameChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetName( "bar" );
        var result = sut.SetName( oldName );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        var node = sut.Node;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "bar" ),
                table.Columns.TryGet( "bar" ).TestRefEquals( sut ),
                table.Columns.TryGet( "C2" ).TestNull(),
                node.Name.TestEquals( "bar" ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.bar ([1] : 'Name' (System.String) FROM C2);
                        """
                    ] ) )
            .Go();
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( " " )]
    [InlineData( "'" )]
    [InlineData( "f\'oo" )]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenColumnIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetName( "bar" ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInTableColumns()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var action = Lambda.Of( () => sut.SetName( "C1" ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenColumnIsUsedInIndex()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        table.Constraints.CreateIndex( sut.Asc() );

        var action = Lambda.Of( () => sut.SetName( "C3" ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenColumnIsUsedInView()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        schema.Objects.CreateView( "V", table.Node.ToDataSource().Select( s => new[] { s.From["C2"].AsSelf() } ) );

        var action = Lambda.Of( () => sut.SetName( "C3" ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetType_ShouldDoNothing_WhenNewTypeEqualsOldType()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetType( schema.Database.TypeDefinitions.GetByType<object>() );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetType_ShouldDoNothing_WhenTypeChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetType( SqlDataTypeMock.Integer );
        var result = sut.SetType( schema.Database.TypeDefinitions.GetByType<object>() );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetType_ShouldUpdateTypeAndSetDefaultValueToNull_WhenNewTypeIsDifferentFromOldType()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetDefaultValue( 123 );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetType<int>();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.TypeDefinition.TestRefEquals( schema.Database.TypeDefinitions.GetByType<int>() ),
                sut.DefaultValue.TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C2 ([3] : 'DataType' (LfrlAnvil.Sql.ISqlDataType) FROM OBJECT)
                          ALTER [Column] foo.T.C2 ([4] : 'DefaultValue' (LfrlAnvil.Sql.Expressions.SqlExpressionNode) FROM "123" : System.Int32);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetType_ShouldDoNothing_WhenNewTypeIsDifferentFromOldTypeButSqlTypeRemainsTheSameAndDefaultValueIsNull()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetType<int>();

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetType<long>();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.TypeDefinition.TestRefEquals( schema.Database.TypeDefinitions.GetByType<long>() ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetType_ShouldThrowSqlObjectBuilderException_WhenColumnIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetType<int>() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetType_ShouldThrowSqlObjectBuilderException_WhenColumnIsUsedInIndex()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        table.Constraints.CreateIndex( sut.Asc() );

        var action = Lambda.Of( () => sut.SetType<int>() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetType_ShouldThrowSqlObjectBuilderException_WhenColumnIsUsedInView()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        schema.Objects.CreateView( "V", table.Node.ToDataSource().Select( s => new[] { s.From["C2"].AsSelf() } ) );

        var action = Lambda.Of( () => sut.SetType<int>() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetType_ShouldThrowSqlObjectBuilderException_WhenTypeDefinitionDoesNotBelongToDatabase()
    {
        var definition = SqlDatabaseBuilderMock.Create().TypeDefinitions.GetByType<int>();
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var action = Lambda.Of( () => sut.SetType( definition ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetType_ShouldThrowSqlObjectCastException_WhenTypeDefinitionIsOfInvalidType()
    {
        var definition = Substitute.For<ISqlColumnTypeDefinition>();
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var action = Lambda.Of( () => (( ISqlColumnBuilder )sut).SetType( definition ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectCastException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Expected.TestEquals( typeof( SqlColumnTypeDefinition ) ) ) ) )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void MarkAsNullable_ShouldDoNothing_WhenNewValueEqualsOldValue(bool value)
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable( value );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsNullable( value );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void MarkAsNullable_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal(bool value)
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable( value );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.MarkAsNullable( ! value );
        var result = sut.MarkAsNullable( value );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void MarkAsNullable_ShouldUpdateIsNullableToTrue_WhenOldValueIsFalse()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsNullable();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.IsNullable.TestTrue(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C2 ([2] : 'IsNullable' (System.Boolean) FROM False);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void MarkAsNullable_ShouldUpdateIsNullableToFalse_WhenOldValueIsTrue()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable();

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsNullable( false );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                result.IsNullable.TestFalse(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C2 ([2] : 'IsNullable' (System.Boolean) FROM True);
                        """
                    ] ) )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void MarkAsNullable_ShouldThrowSqlObjectBuilderException_WhenColumnIsRemoved(bool value)
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable( ! value );
        sut.Remove();

        var action = Lambda.Of( () => sut.MarkAsNullable( value ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void MarkAsNullable_ShouldThrowSqlObjectBuilderException_WhenColumnIsUsedInIndex(bool value)
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable( ! value );
        table.Constraints.CreateIndex( sut.Asc() );

        var action = Lambda.Of( () => sut.MarkAsNullable( value ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void MarkAsNullable_ShouldThrowSqlObjectBuilderException_WhenColumnIsUsedInView(bool value)
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable( ! value );
        schema.Objects.CreateView( "V", table.Node.ToDataSource().Select( s => new[] { s.From["C2"].AsSelf() } ) );

        var action = Lambda.Of( () => sut.MarkAsNullable( value ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void MarkAsNullable_ShouldThrowSqlObjectBuilderException_WhenColumnIsIdentity()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetIdentity( SqlColumnIdentity.Default );

        var action = Lambda.Of( () => sut.MarkAsNullable() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetDefaultValue_ShouldDoNothing_WhenNewValueEqualsOldValue()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetDefaultValue( 123 );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultValue( sut.DefaultValue );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetDefaultValue_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetDefaultValue( 123 );
        var originalDefaultValue = sut.DefaultValue;

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetDefaultValue( ( int? )42 );
        var result = sut.SetDefaultValue( originalDefaultValue );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetDefaultValue_ShouldUpdateDefaultValue_WhenNewValueIsDifferentFromOldValue()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetDefaultValue( 123 );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultValue( 42 );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.DefaultValue.TestType().AssignableTo<SqlLiteralNode<int>>( n => n.Value.TestEquals( 42 ) ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C2 ([4] : 'DefaultValue' (LfrlAnvil.Sql.Expressions.SqlExpressionNode) FROM "123" : System.Int32);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetDefaultValue_ShouldUpdateDefaultValue_WhenNewValueIsNull()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetDefaultValue( 123 );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultValue( null );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.DefaultValue.TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C2 ([4] : 'DefaultValue' (LfrlAnvil.Sql.Expressions.SqlExpressionNode) FROM "123" : System.Int32);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetDefaultValue_ShouldUpdateDefaultValue_WhenOldValueIsNull()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultValue( 123 );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.DefaultValue.TestType().AssignableTo<SqlLiteralNode<int>>( n => n.Value.TestEquals( 123 ) ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C2 ([4] : 'DefaultValue' (LfrlAnvil.Sql.Expressions.SqlExpressionNode) FROM <null>);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetDefaultValue_ShouldUpdateDefaultValue_WhenNewValueIsValidComplexExpression()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetType<long>();

        var actionCount = schema.Database.GetPendingActionCount();
        var defaultValue = SqlNode.Literal( 10 ) + SqlNode.Literal( 50 ) + SqlNode.Literal( 100 ).Max( SqlNode.Literal( 80 ) );
        var result = sut.SetDefaultValue( defaultValue );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.DefaultValue.TestRefEquals( defaultValue ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C2 ([4] : 'DefaultValue' (LfrlAnvil.Sql.Expressions.SqlExpressionNode) FROM <null>);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetDefaultValue_ShouldBePossible_WhenColumnIsUsedInIndex()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        table.Constraints.CreateIndex( sut.Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultValue( 123 );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.DefaultValue.TestType().AssignableTo<SqlLiteralNode<int>>( n => n.Value.TestEquals( 123 ) ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C2 ([4] : 'DefaultValue' (LfrlAnvil.Sql.Expressions.SqlExpressionNode) FROM <null>);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetDefaultValue_ShouldBePossible_WhenColumnIsUsedInView()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        schema.Objects.CreateView( "V", table.Node.ToDataSource().Select( s => new[] { s.From["C2"].AsSelf() } ) );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultValue( ( int? )123 );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.DefaultValue.TestType().AssignableTo<SqlLiteralNode<int>>( n => n.Value.TestEquals( 123 ) ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C2 ([4] : 'DefaultValue' (LfrlAnvil.Sql.Expressions.SqlExpressionNode) FROM <null>);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetDefaultValue_ShouldThrowSqlObjectBuilderException_WhenColumnIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetDefaultValue( 42 ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetDefaultValue_ShouldThrowSqlObjectBuilderException_WhenColumnIsGenerated()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetComputation( SqlColumnComputation.Virtual( SqlNode.Literal( 1 ) ) );

        var action = Lambda.Of( () => sut.SetDefaultValue( 42 ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetDefaultValue_ShouldThrowSqlObjectBuilderException_WhenExpressionIsInvalid()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var action = Lambda.Of( () => sut.SetDefaultValue( table.ToRecordSet().GetField( "C1" ) ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetDefaultValue_ShouldThrowSqlObjectBuilderException_WhenColumnIsIdentity()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetIdentity( SqlColumnIdentity.Default );

        var action = Lambda.Of( () => sut.SetDefaultValue( 42 ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetComputation_ShouldDoNothing_WhenNewNullValueEqualsOldValue()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetComputation( null );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetComputation_ShouldDoNothing_WhenNewNonNullValueEqualsOldValue()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetComputation( SqlColumnComputation.Virtual( SqlNode.Literal( 1 ) ) );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetComputation( sut.Computation );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetComputation_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetComputation( SqlColumnComputation.Virtual( SqlNode.Literal( 1 ) ) );
        var originalComputation = sut.Computation;

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetComputation( SqlColumnComputation.Stored( SqlNode.Literal( 42 ) ) );
        var result = sut.SetComputation( originalComputation );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetComputation_ShouldUpdateComputation_WhenNewNullValueIsDifferentFromOldStoredValue()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var other = table.Columns.Create( "C2" );
        var sut = table.Columns.Create( "C3" ).SetComputation( SqlColumnComputation.Stored( other.Node + SqlNode.Literal( 1 ) ) );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetComputation( null );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Computation.TestNull(),
                sut.ReferencedComputationColumns.TestEmpty(),
                other.ReferencingObjects.TestEmpty(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C3 ([5] : 'Computation' (System.Nullable`1[T is LfrlAnvil.Sql.Objects.Builders.SqlColumnComputation]) FROM SqlColumnComputation { Expression = ([foo].[T].[C2] : System.Object) + ("1" : System.Int32), Storage = Stored });
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetComputation_ShouldUpdateComputation_WhenNewNullValueIsDifferentFromOldVirtualValue()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var other = table.Columns.Create( "C2" );
        var sut = table.Columns.Create( "C3" ).SetComputation( SqlColumnComputation.Virtual( other.Node + SqlNode.Literal( 1 ) ) );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetComputation( null );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Computation.TestNull(),
                sut.ReferencedComputationColumns.TestEmpty(),
                other.ReferencingObjects.TestEmpty(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C3 ([5] : 'Computation' (System.Nullable`1[T is LfrlAnvil.Sql.Objects.Builders.SqlColumnComputation]) FROM SqlColumnComputation { Expression = ([foo].[T].[C2] : System.Object) + ("1" : System.Int32), Storage = Virtual });
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetComputation_ShouldUpdateComputation_WhenNewStoredValueIsDifferentFromOldNullValue()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var other = table.Columns.Create( "C2" );
        var sut = table.Columns.Create( "C3" );
        var computation = SqlColumnComputation.Stored( other.Node + SqlNode.Literal( 1 ) );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetComputation( computation );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Computation.TestEquals( computation ),
                sut.ReferencedComputationColumns.TestSequence( [ other ] ),
                other.ReferencingObjects.TestSequence(
                    [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut, property: "Computation" ), other ) ] ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C3 ([5] : 'Computation' (System.Nullable`1[T is LfrlAnvil.Sql.Objects.Builders.SqlColumnComputation]) FROM <null>);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetComputation_ShouldUpdateComputation_WhenNewVirtualValueIsDifferentFromOldNullValue()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var other = table.Columns.Create( "C2" );
        var sut = table.Columns.Create( "C3" );
        var computation = SqlColumnComputation.Virtual( other.Node + SqlNode.Literal( 1 ) );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetComputation( computation );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Computation.TestEquals( computation ),
                sut.ReferencedComputationColumns.TestSequence( [ other ] ),
                other.ReferencingObjects.TestSequence(
                    [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut, property: "Computation" ), other ) ] ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C3 ([5] : 'Computation' (System.Nullable`1[T is LfrlAnvil.Sql.Objects.Builders.SqlColumnComputation]) FROM <null>);
                        """
                    ] ) )
            .Go();
    }

    [Theory]
    [InlineData( SqlColumnComputationStorage.Virtual )]
    [InlineData( SqlColumnComputationStorage.Stored )]
    public void SetComputation_ShouldUpdateComputation_WhenNewExpressionIsDifferentFromOldExpression(SqlColumnComputationStorage storage)
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var other = table.Columns.Create( "C2" );
        var oldOther = table.Columns.Create( "C4" );
        var sut = table.Columns.Create( "C3" ).SetComputation( new SqlColumnComputation( oldOther.Node + SqlNode.Literal( 1 ), storage ) );
        var computation = new SqlColumnComputation( other.Node + SqlNode.Literal( 1 ), storage );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetComputation( computation );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Computation.TestEquals( computation ),
                sut.ReferencedComputationColumns.TestSequence( [ other ] ),
                oldOther.ReferencingObjects.TestEmpty(),
                other.ReferencingObjects.TestSequence(
                    [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut, property: "Computation" ), other ) ] ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        $$"""
                          ALTER [Table] foo.T
                            ALTER [Column] foo.T.C3 ([5] : 'Computation' (System.Nullable`1[T is LfrlAnvil.Sql.Objects.Builders.SqlColumnComputation]) FROM SqlColumnComputation { Expression = ([foo].[T].[C4] : System.Object) + ("1" : System.Int32), Storage = {{storage}} });
                          """
                    ] ) )
            .Go();
    }

    [Theory]
    [InlineData( SqlColumnComputationStorage.Virtual, SqlColumnComputationStorage.Stored )]
    [InlineData( SqlColumnComputationStorage.Stored, SqlColumnComputationStorage.Virtual )]
    public void SetComputation_ShouldUpdateComputation_WhenNewStorageIsDifferentFromOldStorage(
        SqlColumnComputationStorage oldStorage,
        SqlColumnComputationStorage newStorage)
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var other = table.Columns.Create( "C2" );
        var expression = other.Node + SqlNode.Literal( 1 );
        var sut = table.Columns.Create( "C3" ).SetComputation( new SqlColumnComputation( expression, oldStorage ) );
        table.Constraints.CreateIndex( sut.Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetComputation( new SqlColumnComputation( expression, newStorage ) );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Computation.TestEquals( new SqlColumnComputation( expression, newStorage ) ),
                sut.ReferencedComputationColumns.TestSequence( [ other ] ),
                other.ReferencingObjects.TestSequence(
                    [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut, property: "Computation" ), other ) ] ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        $$"""
                          ALTER [Table] foo.T
                            ALTER [Column] foo.T.C3 ([5] : 'Computation' (System.Nullable`1[T is LfrlAnvil.Sql.Objects.Builders.SqlColumnComputation]) FROM SqlColumnComputation { Expression = ([foo].[T].[C2] : System.Object) + ("1" : System.Int32), Storage = {{oldStorage}} });
                          """
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetComputation_ShouldSetDefaultValueToNull_WhenValueIsNotNull()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var other = table.Columns.Create( "C2" );
        var sut = table.Columns.Create( "C3" ).SetDefaultValue( 42 );
        var computation = SqlColumnComputation.Stored( other.Node + SqlNode.Literal( 1 ) );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetComputation( computation );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.DefaultValue.TestNull(),
                sut.Computation.TestEquals( computation ),
                sut.ReferencedComputationColumns.TestSequence( [ other ] ),
                other.ReferencingObjects.TestSequence(
                    [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut, property: "Computation" ), other ) ] ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C3 ([4] : 'DefaultValue' (LfrlAnvil.Sql.Expressions.SqlExpressionNode) FROM "42" : System.Int32)
                          ALTER [Column] foo.T.C3 ([5] : 'Computation' (System.Nullable`1[T is LfrlAnvil.Sql.Objects.Builders.SqlColumnComputation]) FROM <null>);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetComputation_ShouldThrowSqlObjectBuilderException_WhenColumnIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetComputation( SqlColumnComputation.Virtual( SqlNode.Literal( 1 ) ) ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetComputation_ShouldThrowSqlObjectBuilderException_WhenColumnIsReferencedAndOldValueIsNull()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        table.Constraints.CreateIndex( sut.Asc() );

        var action = Lambda.Of( () => sut.SetComputation( SqlColumnComputation.Virtual( SqlNode.Literal( 1 ) ) ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetComputation_ShouldThrowSqlObjectBuilderException_WhenExpressionIsInvalidAndOldValueIsNull()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var action = Lambda.Of( () => sut.SetComputation( SqlColumnComputation.Virtual( SqlNode.RawRecordSet( "bar" )["x"] ) ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetComputation_ShouldThrowSqlObjectBuilderException_WhenColumnIsReferencedAndOldValueIsNotNull()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetComputation( SqlColumnComputation.Stored( SqlNode.Literal( 42 ) ) );
        table.Constraints.CreateIndex( sut.Asc() );

        var action = Lambda.Of( () => sut.SetComputation( SqlColumnComputation.Virtual( SqlNode.Literal( 1 ) ) ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetComputation_ShouldThrowSqlObjectBuilderException_WhenExpressionIsInvalidAndOldValueIsNotNull()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetComputation( SqlColumnComputation.Stored( SqlNode.Literal( 42 ) ) );

        var action = Lambda.Of( () => sut.SetComputation( SqlColumnComputation.Virtual( SqlNode.RawRecordSet( "bar" )["x"] ) ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetComputation_ShouldThrowSqlObjectBuilderException_WhenColumnIsReferencedAndNewValueIsNull()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetComputation( SqlColumnComputation.Virtual( SqlNode.Literal( 1 ) ) );
        table.Constraints.CreateIndex( sut.Asc() );

        var action = Lambda.Of( () => sut.SetComputation( null ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetComputation_ShouldThrowSqlObjectBuilderException_WhenColumnReferencesSelf()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var action = Lambda.Of( () => sut.SetComputation( SqlColumnComputation.Virtual( sut.Node + SqlNode.Literal( 1 ) ) ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetComputation_ShouldThrowSqlObjectBuilderException_WhenColumnIsIdentity()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetIdentity( SqlColumnIdentity.Default );

        var action = Lambda.Of( () => sut.SetComputation( SqlColumnComputation.Virtual( SqlNode.Literal( 1 ) ) ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetIdentity_ShouldDoNothing_WhenNewValueEqualsOldValue()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetIdentity( SqlColumnIdentity.Default );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetIdentity( SqlColumnIdentity.Default );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetIdentity_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        var originalIdentity = sut.Identity;

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetIdentity( SqlColumnIdentity.Default );
        var result = sut.SetIdentity( originalIdentity );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetIdentity_ShouldUpdateIdentity_WhenNewValueIsDifferentFromOldValue()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetIdentity( SqlColumnIdentity.Default );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetIdentity( new SqlColumnIdentity( 123 ) );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Identity.TestEquals( new SqlColumnIdentity( 123 ) ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C2 ([6] : 'Identity' (System.Nullable`1[T is LfrlAnvil.Sql.Objects.Builders.SqlColumnIdentity]) FROM SqlColumnIdentity { AutoIncrementCache =  });
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetIdentity_ShouldUpdateIdentity_WhenNewValueIsNull()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetIdentity( SqlColumnIdentity.Default );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetIdentity( null );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Identity.TestNull(),
                table.Columns.Identity.TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C2 ([6] : 'Identity' (System.Nullable`1[T is LfrlAnvil.Sql.Objects.Builders.SqlColumnIdentity]) FROM SqlColumnIdentity { AutoIncrementCache =  });
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetIdentity_ShouldUpdateIdentity_WhenOldValueIsNull()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetIdentity( SqlColumnIdentity.Default );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Identity.TestEquals( SqlColumnIdentity.Default ),
                table.Columns.Identity.TestRefEquals( sut ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C2 ([6] : 'Identity' (System.Nullable`1[T is LfrlAnvil.Sql.Objects.Builders.SqlColumnIdentity]) FROM <null>);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetIdentity_ShouldResetDefaultValue_WhenNewValueIsNotNull()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetDefaultValue( 123 );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetIdentity( SqlColumnIdentity.Default );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Identity.TestEquals( SqlColumnIdentity.Default ),
                sut.DefaultValue.TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C2 ([4] : 'DefaultValue' (LfrlAnvil.Sql.Expressions.SqlExpressionNode) FROM "123" : System.Int32)
                          ALTER [Column] foo.T.C2 ([6] : 'Identity' (System.Nullable`1[T is LfrlAnvil.Sql.Objects.Builders.SqlColumnIdentity]) FROM <null>);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetIdentity_ShouldResetComputation_WhenNewValueIsNotNull()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetComputation( SqlColumnComputation.Virtual( SqlNode.Literal( 1 ) ) );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetIdentity( SqlColumnIdentity.Default );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Identity.TestEquals( SqlColumnIdentity.Default ),
                sut.Computation.TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C2 ([5] : 'Computation' (System.Nullable`1[T is LfrlAnvil.Sql.Objects.Builders.SqlColumnComputation]) FROM SqlColumnComputation { Expression = "1" : System.Int32, Storage = Virtual })
                          ALTER [Column] foo.T.C2 ([6] : 'Identity' (System.Nullable`1[T is LfrlAnvil.Sql.Objects.Builders.SqlColumnIdentity]) FROM <null>);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetIdentity_ShouldResetIsNullable_WhenNewValueIsNotNull()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable();

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetIdentity( SqlColumnIdentity.Default );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Identity.TestEquals( SqlColumnIdentity.Default ),
                sut.IsNullable.TestFalse(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C2 ([2] : 'IsNullable' (System.Boolean) FROM True)
                          ALTER [Column] foo.T.C2 ([6] : 'Identity' (System.Nullable`1[T is LfrlAnvil.Sql.Objects.Builders.SqlColumnIdentity]) FROM <null>);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetIdentity_ShouldBePossible_WhenColumnIsUsedInIndex()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        table.Constraints.CreateIndex( sut.Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetIdentity( SqlColumnIdentity.Default );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Identity.TestEquals( SqlColumnIdentity.Default ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C2 ([6] : 'Identity' (System.Nullable`1[T is LfrlAnvil.Sql.Objects.Builders.SqlColumnIdentity]) FROM <null>);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetIdentity_ShouldBePossible_WhenColumnIsUsedInView()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        schema.Objects.CreateView( "V", table.Node.ToDataSource().Select( s => new[] { s.From["C2"].AsSelf() } ) );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetIdentity( SqlColumnIdentity.Default );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Identity.TestEquals( SqlColumnIdentity.Default ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C2 ([6] : 'Identity' (System.Nullable`1[T is LfrlAnvil.Sql.Objects.Builders.SqlColumnIdentity]) FROM <null>);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetIdentity_ShouldThrowSqlObjectBuilderException_WhenColumnIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetIdentity( SqlColumnIdentity.Default ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetIdentity_ShouldThrowSqlObjectBuilderException_WhenTableAlreadyContainsIdentityColumn()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var other = table.Columns.Create( "C1" ).SetIdentity( SqlColumnIdentity.Default );
        table.Constraints.SetPrimaryKey( other.Asc() );
        var sut = table.Columns.Create( "C2" );

        var action = Lambda.Of( () => sut.SetIdentity( SqlColumnIdentity.Default ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveColumnAndClearReferencedComputationColumns()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var other = table.Columns.Create( "C1" );
        var pk = table.Constraints.SetPrimaryKey( other.Asc() );
        var sut = table.Columns.Create( "C2" ).SetComputation( SqlColumnComputation.Stored( other.Node + SqlNode.Literal( 1 ) ) );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                table.Columns.TryGet( sut.Name ).TestNull(),
                sut.ReferencedComputationColumns.TestEmpty(),
                sut.Computation.TestNull(),
                sut.IsRemoved.TestTrue(),
                other.ReferencingObjects.TestSequence(
                    [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( pk.Index ), other ) ] ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          REMOVE [Column] foo.T.C2;
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveIdentityColumn()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var other = table.Columns.Create( "C1" );
        var pk = table.Constraints.SetPrimaryKey( other.Asc() );
        var sut = table.Columns.Create( "C2" ).SetIdentity( SqlColumnIdentity.Default );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                table.Columns.TryGet( sut.Name ).TestNull(),
                table.Columns.Identity.TestNull(),
                sut.IsRemoved.TestTrue(),
                other.ReferencingObjects.TestSequence(
                    [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( pk.Index ), other ) ] ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          REMOVE [Column] foo.T.C2;
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenColumnIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        schema.Database.Changes.CompletePendingChanges();
        sut.Remove();

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.TestEmpty().Go();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenColumnIsReferencedByIndex()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        table.Constraints.CreateIndex( sut.Asc() );

        var action = Lambda.Of( () => sut.Remove() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenColumnIsReferencedByIndexFilter()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        table.Constraints.CreateIndex( table.Columns.Create( "C3" ).Asc() ).SetFilter( t => t["C2"] != null );

        var action = Lambda.Of( () => sut.Remove() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenColumnIsReferencedByView()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        schema.Objects.CreateView( "V", table.Node.ToDataSource().Select( s => new[] { s.From["C2"].AsSelf() } ) );

        var action = Lambda.Of( () => sut.Remove() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenColumnIsReferencedByCheck()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        table.Constraints.CreateCheck( table.Node["C2"] != SqlNode.Literal( 0 ) );

        var action = Lambda.Of( () => sut.Remove() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void QuickRemove_ShouldClearReferencingObjectsAndReferencedComputationColumns()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var other = table.Columns.Create( "C1" );
        var pk = table.Constraints.SetPrimaryKey( other.Asc() );
        var sut = table.Columns.Create( "C2" ).SetComputation( SqlColumnComputation.Stored( other.Node + SqlNode.Literal( 1 ) ) );
        var ixColumn = sut.Asc();
        var ix = table.Constraints.CreateIndex( ixColumn );
        var chk = table.Constraints.CreateCheck( sut.Node > SqlNode.Literal( 0 ) );

        var actionCount = schema.Database.GetPendingActionCount();
        SqlDatabaseBuilderMock.QuickRemove( sut );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                table.Columns.TryGet( sut.Name ).TestRefEquals( sut ),
                sut.ReferencedComputationColumns.TestEmpty(),
                sut.Computation.TestNull(),
                sut.IsRemoved.TestTrue(),
                sut.ReferencingObjects.TestEmpty(),
                ix.Columns.Expressions.TestSequence( [ ixColumn ] ),
                chk.ReferencedColumns.TestSequence( [ sut ] ),
                other.ReferencingObjects.Count.TestEquals( 2 ),
                other.ReferencingObjects.TestSetEqual(
                [
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( pk.Index ), other ),
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut, property: "Computation" ), other )
                ] ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void QuickRemove_ShouldDoNothing_WhenColumnIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        schema.Database.Changes.CompletePendingChanges();
        sut.Remove();

        var actionCount = schema.Database.GetPendingActionCount();
        SqlDatabaseBuilderMock.QuickRemove( sut );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.TestEmpty().Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void ToDefinitionNode_ShouldReturnCorrectNode(bool isNullable)
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" )
            .MarkAsNullable( isNullable )
            .SetComputation( SqlColumnComputation.Stored( SqlNode.Literal( 1 ) ) );

        var result = sut.ToDefinitionNode();

        Assertion.All(
                result.Name.TestRefEquals( sut.Name ),
                result.Type.TestEquals( TypeNullability.Create<object>( isNullable ) ),
                result.TypeDefinition.TestRefEquals( sut.TypeDefinition ),
                result.DefaultValue.TestRefEquals( sut.DefaultValue ),
                result.Computation.TestEquals( sut.Computation ) )
            .Go();
    }

    [Fact]
    public void ISqlColumnBuilder_SetName_ShouldBeEquivalentToSetName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        var node = sut.Node;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = (( ISqlColumnBuilder )sut).SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "bar" ),
                table.Columns.TryGet( "bar" ).TestRefEquals( sut ),
                table.Columns.TryGet( "C2" ).TestNull(),
                node.Name.TestEquals( "bar" ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.bar ([1] : 'Name' (System.String) FROM C2);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void ISqlObjectBuilder_SetName_ShouldBeEquivalentToSetName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        var node = sut.Node;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = (( ISqlObjectBuilder )sut).SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "bar" ),
                table.Columns.TryGet( "bar" ).TestRefEquals( sut ),
                table.Columns.TryGet( "C2" ).TestNull(),
                node.Name.TestEquals( "bar" ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.bar ([1] : 'Name' (System.String) FROM C2);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void ISqlColumnBuilder_SetType_ShouldBeEquivalentToSetType()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = (( ISqlColumnBuilder )sut).SetType<int>();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.TypeDefinition.TestRefEquals( schema.Database.TypeDefinitions.GetByType<int>() ),
                sut.DefaultValue.TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C2 ([3] : 'DataType' (LfrlAnvil.Sql.ISqlDataType) FROM OBJECT);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void ISqlColumnBuilder_SetType_ByDataType_ShouldBeEquivalentToSetType()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = (( ISqlColumnBuilder )sut).SetType( SqlDataTypeMock.Integer );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.TypeDefinition.TestRefEquals( schema.Database.TypeDefinitions.GetByDataType( SqlDataTypeMock.Integer ) ),
                sut.DefaultValue.TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C2 ([3] : 'DataType' (LfrlAnvil.Sql.ISqlDataType) FROM OBJECT);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void ISqlColumnBuilder_MarkAsNullable_ShouldBeEquivalentToMarkAsNullable()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = (( ISqlColumnBuilder )sut).MarkAsNullable();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.IsNullable.TestTrue(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C2 ([2] : 'IsNullable' (System.Boolean) FROM False);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void ISqlColumnBuilder_SetDefaultValue_ShouldBeEquivalentToSetDefaultValue()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = (( ISqlColumnBuilder )sut).SetDefaultValue( ( int? )123 ).SetDefaultValue( 42 );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.DefaultValue.TestType().AssignableTo<SqlLiteralNode<int>>( n => n.Value.TestEquals( 42 ) ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C2 ([4] : 'DefaultValue' (LfrlAnvil.Sql.Expressions.SqlExpressionNode) FROM <null>);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void ISqlColumnBuilder_SetComputation_ShouldBeEquivalentToSetComputation()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        var computation = SqlColumnComputation.Virtual( SqlNode.Literal( 1 ) );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = (( ISqlColumnBuilder )sut).SetComputation( computation );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Computation.TestEquals( computation ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C2 ([5] : 'Computation' (System.Nullable`1[T is LfrlAnvil.Sql.Objects.Builders.SqlColumnComputation]) FROM <null>);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void ISqlColumnBuilder_SetIdentity_ShouldBeEquivalentToSetIdentity()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = (( ISqlColumnBuilder )sut).SetIdentity( SqlColumnIdentity.Default );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Identity.TestEquals( SqlColumnIdentity.Default ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [Column] foo.T.C2 ([6] : 'Identity' (System.Nullable`1[T is LfrlAnvil.Sql.Objects.Builders.SqlColumnIdentity]) FROM <null>);
                        """
                    ] ) )
            .Go();
    }
}
