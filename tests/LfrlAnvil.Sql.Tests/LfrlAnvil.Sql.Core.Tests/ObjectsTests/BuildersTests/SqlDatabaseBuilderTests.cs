using System.Globalization;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ObjectsTests.BuildersTests;

public partial class SqlDatabaseBuilderTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateBuilderWithDefaultSchema()
    {
        var sut = SqlDatabaseBuilderMockFactory.Create();

        using ( new AssertionScope() )
        {
            sut.Schemas.Count.Should().Be( 1 );
            sut.Schemas.Database.Should().BeSameAs( sut );
            sut.Schemas.Should().BeSequentiallyEqualTo( sut.Schemas.Default );

            sut.Schemas.Default.Database.Should().BeSameAs( sut );
            sut.Schemas.Default.Name.Should().Be( "common" );
            sut.Schemas.Default.Objects.Should().BeEmpty();
            sut.Schemas.Default.Objects.Schema.Should().BeSameAs( sut.Schemas.Default );

            sut.Dialect.Should().BeSameAs( SqlDialectMock.Instance );
            sut.ServerVersion.Should().Be( "0.0.0" );

            sut.Changes.Database.Should().BeSameAs( sut );
            sut.Changes.Mode.Should().Be( SqlDatabaseCreateMode.DryRun );
            sut.Changes.IsAttached.Should().BeTrue();
            sut.Changes.ActiveObject.Should().BeNull();
            sut.Changes.ActiveObjectExistenceState.Should().Be( default( SqlObjectExistenceState ) );
            sut.Changes.IsActive.Should().BeTrue();
            sut.Changes.GetPendingActions().ToArray().Should().BeEmpty();

            ((ISqlDatabaseBuilder)sut).DataTypes.Should().BeSameAs( sut.DataTypes );
            ((ISqlDatabaseBuilder)sut).TypeDefinitions.Should().BeSameAs( sut.TypeDefinitions );
            ((ISqlDatabaseBuilder)sut).NodeInterpreters.Should().BeSameAs( sut.NodeInterpreters );
            ((ISqlDatabaseBuilder)sut).QueryReaders.Should().BeSameAs( sut.QueryReaders );
            ((ISqlDatabaseBuilder)sut).ParameterBinders.Should().BeSameAs( sut.ParameterBinders );
            ((ISqlDatabaseBuilder)sut).Schemas.Should().BeSameAs( sut.Schemas );
            ((ISqlDatabaseBuilder)sut).Changes.Should().BeSameAs( sut.Changes );
            ((ISqlSchemaBuilderCollection)sut.Schemas).Default.Should().BeSameAs( sut.Schemas.Default );
            ((ISqlSchemaBuilderCollection)sut.Schemas).Database.Should().BeSameAs( sut.Schemas.Database );
            ((ISqlDatabaseChangeTracker)sut.Changes).Database.Should().BeSameAs( sut.Changes.Database );
            ((ISqlDatabaseChangeTracker)sut.Changes).ActiveObject.Should().BeSameAs( sut.Changes.ActiveObject );
        }
    }

    [Fact]
    public void AddConnectionChangeCallback_ShouldNotThrow()
    {
        ISqlDatabaseBuilder sut = SqlDatabaseBuilderMockFactory.Create();
        var result = sut.AddConnectionChangeCallback( _ => { } );
        result.Should().BeSameAs( sut );
    }

    [Theory]
    [InlineData( true, "1" )]
    [InlineData( false, "0" )]
    public void SqlHelpers_GetDbLiteral_ShouldReturnCorrectResult_ForBool(bool value, string expected)
    {
        var result = SqlHelpers.GetDbLiteral( value );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 1234567, "1234567" )]
    [InlineData( 0, "0" )]
    [InlineData( -1234567, "-1234567" )]
    public void SqlHelpers_GetDbLiteral_ShouldReturnCorrectResult_ForInt64(long value, string expected)
    {
        var result = SqlHelpers.GetDbLiteral( value );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 1234567, "1234567" )]
    [InlineData( 0, "0" )]
    public void SqlHelpers_GetDbLiteral_ShouldReturnCorrectResult_ForUInt64(ulong value, string expected)
    {
        var result = SqlHelpers.GetDbLiteral( value );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 123.625, "123.625" )]
    [InlineData( 123, "123.0" )]
    [InlineData( 0, "0.0" )]
    [InlineData( -123.625, "-123.625" )]
    [InlineData( -123, "-123.0" )]
    [InlineData( double.Epsilon, "4.9406564584124654E-324" )]
    [InlineData( 1234567890987654321, "1.2345678909876544E+18" )]
    public void SqlHelpers_GetDbLiteral_ShouldReturnCorrectResult_ForDouble(double value, string expected)
    {
        var result = SqlHelpers.GetDbLiteral( value );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 123.625, "123.625" )]
    [InlineData( 123, "123.0" )]
    [InlineData( 0, "0.0" )]
    [InlineData( -123.625, "-123.625" )]
    [InlineData( -123, "-123.0" )]
    [InlineData( float.Epsilon, "1.40129846E-45" )]
    public void SqlHelpers_GetDbLiteral_ShouldReturnCorrectResult_ForFloat(float value, string expected)
    {
        var result = SqlHelpers.GetDbLiteral( value );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "123.625" )]
    [InlineData( "123.0" )]
    [InlineData( "0.0" )]
    [InlineData( "-123.625" )]
    [InlineData( "-123.0" )]
    [InlineData( "1.2345678901234567890123456789" )]
    [InlineData( "-1.2345678901234567890123456789" )]
    public void SqlHelpers_GetDbLiteral_ShouldReturnCorrectResult_ForDecimal(string text)
    {
        var value = decimal.Parse( text, CultureInfo.InvariantCulture );
        var result = SqlHelpers.GetDbLiteral( value );
        result.Should().Be( text );
    }

    [Theory]
    [InlineData( "foo", "'foo'" )]
    [InlineData( "", "''" )]
    [InlineData( "FOOBAR", "'FOOBAR'" )]
    [InlineData( "'", "''''" )]
    [InlineData( "f'oo'bar'", "'f''oo''bar'''" )]
    [InlineData( "'FOO'BAR", "'''FOO''BAR'" )]
    public void SqlHelpers_GetDbLiteral_ShouldReturnCorrectResult_ForString(string value, string expected)
    {
        var result = SqlHelpers.GetDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void SqlHelpers_GetDbLiteral_ShouldReturnCorrectResult_ForEmptyBinary()
    {
        var result = SqlHelpers.GetDbLiteral( Array.Empty<byte>() );
        result.Should().Be( "X''" );
    }

    [Fact]
    public void SqlHelpers_GetDbLiteral_ShouldReturnCorrectResult_ForNonEmptyBinary()
    {
        var value = new byte[] { 0, 10, 21, 31, 42, 58, 73, 89, 104, 129, 155, 181, 206, 233, 255 };
        var result = SqlHelpers.GetDbLiteral( value );
        result.Should().Be( "X'000A151F2A3A495968819BB5CEE9FF'" );
    }

    [Theory]
    [InlineData( "foo", "bar", "foo.bar" )]
    [InlineData( "", "bar", "bar" )]
    public void SqlHelpers_GetFullName_ShouldReturnCorrectResult_ForSchemaObject(string schemaName, string name, string expected)
    {
        var result = SqlHelpers.GetFullName( schemaName, name );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "foo", "bar", "qux", "foo.bar.qux" )]
    [InlineData( "", "bar", "qux", "bar.qux" )]
    public void SqlHelpers_GetFullName_ShouldReturnCorrectResult_ForRecordSetObject(
        string schemaName,
        string recordSetName,
        string name,
        string expected)
    {
        var result = SqlHelpers.GetFullName( schemaName, recordSetName, name );
        result.Should().Be( expected );
    }

    [Fact]
    public void SqlHelpers_GetReferencingObjectsInOrderOfCreation_ShouldReturnCorrectResult_WhenObjectIsNotReferenced()
    {
        var obj = SqlDatabaseBuilderMockFactory.Create().Schemas.Default;
        var result = SqlHelpers.GetReferencingObjectsInOrderOfCreation( obj );
        result.Should().BeEmpty();
    }

    [Fact]
    public void SqlHelpers_GetReferencingObjectsInOrderOfCreation_ShouldReturnCorrectResult_WhenAllReferencesAreFilteredOut()
    {
        var schema = SqlDatabaseBuilderMockFactory.Create().Schemas.Default;
        var obj = schema.Objects.CreateTable( "T" );
        SqlDatabaseBuilderMock.AddReference( obj, SqlObjectBuilderReferenceSource.Create( schema ) );

        var result = SqlHelpers.GetReferencingObjectsInOrderOfCreation( obj, _ => false );

        result.Should().BeEmpty();
    }

    [Fact]
    public void SqlHelpers_GetReferencingObjectsInOrderOfCreation_ShouldReturnCorrectResult_WhenAllObjectsAreIncluded()
    {
        var schema = SqlDatabaseBuilderMockFactory.Create().Schemas.Default;
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C1" );
        var obj = table.Columns.Create( "C2" );
        SqlDatabaseBuilderMock.AddReference( obj, SqlObjectBuilderReferenceSource.Create( column ) );
        SqlDatabaseBuilderMock.AddReference( obj, SqlObjectBuilderReferenceSource.Create( schema ) );
        SqlDatabaseBuilderMock.AddReference( obj, SqlObjectBuilderReferenceSource.Create( table ) );

        var result = SqlHelpers.GetReferencingObjectsInOrderOfCreation( obj );

        result.Should().BeSequentiallyEqualTo( schema, table, column );
    }

    [Fact]
    public void SqlHelpers_GetReferencingObjectsInOrderOfCreation_ShouldReturnCorrectResult_WhenSomeObjectsAreFilteredOut()
    {
        var schema = SqlDatabaseBuilderMockFactory.Create().Schemas.Default;
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C1" );
        var obj = table.Columns.Create( "C2" );
        SqlDatabaseBuilderMock.AddReference( obj, SqlObjectBuilderReferenceSource.Create( column ) );
        SqlDatabaseBuilderMock.AddReference( obj, SqlObjectBuilderReferenceSource.Create( schema ) );
        SqlDatabaseBuilderMock.AddReference( obj, SqlObjectBuilderReferenceSource.Create( table ) );

        var result = SqlHelpers.GetReferencingObjectsInOrderOfCreation( obj, o => o.Source.Object.Type != SqlObjectType.Schema );

        result.Should().BeSequentiallyEqualTo( table, column );
    }
}
