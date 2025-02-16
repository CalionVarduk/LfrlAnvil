using System.Globalization;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ObjectsTests.BuildersTests;

public partial class SqlDatabaseBuilderTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateBuilderWithDefaultSchema()
    {
        var sut = SqlDatabaseBuilderMock.Create();

        Assertion.All(
                sut.Schemas.Count.TestEquals( 1 ),
                sut.Schemas.Database.TestRefEquals( sut ),
                sut.Schemas.TestSequence( [ sut.Schemas.Default ] ),
                sut.Schemas.Default.Database.TestRefEquals( sut ),
                sut.Schemas.Default.Name.TestEquals( "common" ),
                sut.Schemas.Default.Objects.TestEmpty(),
                sut.Schemas.Default.Objects.Schema.TestRefEquals( sut.Schemas.Default ),
                sut.Dialect.TestRefEquals( SqlDialectMock.Instance ),
                sut.ServerVersion.TestEquals( "0.0.0" ),
                sut.Changes.Database.TestRefEquals( sut ),
                sut.Changes.Mode.TestEquals( SqlDatabaseCreateMode.DryRun ),
                sut.Changes.IsAttached.TestTrue(),
                sut.Changes.ActiveObject.TestNull(),
                sut.Changes.ActiveObjectExistenceState.TestEquals( default ),
                sut.Changes.IsActive.TestTrue(),
                sut.Changes.GetPendingActions().ToArray().TestEmpty(),
                (( ISqlDatabaseBuilder )sut).DataTypes.TestRefEquals( sut.DataTypes ),
                (( ISqlDatabaseBuilder )sut).TypeDefinitions.TestRefEquals( sut.TypeDefinitions ),
                (( ISqlDatabaseBuilder )sut).NodeInterpreters.TestRefEquals( sut.NodeInterpreters ),
                (( ISqlDatabaseBuilder )sut).QueryReaders.TestRefEquals( sut.QueryReaders ),
                (( ISqlDatabaseBuilder )sut).ParameterBinders.TestRefEquals( sut.ParameterBinders ),
                (( ISqlDatabaseBuilder )sut).DefaultNames.TestRefEquals( sut.DefaultNames ),
                (( ISqlDatabaseBuilder )sut).Schemas.TestRefEquals( sut.Schemas ),
                (( ISqlDatabaseBuilder )sut).Changes.TestRefEquals( sut.Changes ),
                (( ISqlSchemaBuilderCollection )sut.Schemas).Default.TestRefEquals( sut.Schemas.Default ),
                (( ISqlSchemaBuilderCollection )sut.Schemas).Database.TestRefEquals( sut.Schemas.Database ),
                (( ISqlDatabaseChangeTracker )sut.Changes).Database.TestRefEquals( sut.Changes.Database ),
                (( ISqlDatabaseChangeTracker )sut.Changes).ActiveObject.TestRefEquals( sut.Changes.ActiveObject ) )
            .Go();
    }

    [Fact]
    public void AddConnectionChangeCallback_ShouldNotThrow()
    {
        ISqlDatabaseBuilder sut = SqlDatabaseBuilderMock.Create();
        var result = sut.AddConnectionChangeCallback( _ => { } );
        result.TestRefEquals( sut ).Go();
    }

    [Theory]
    [InlineData( true, "1" )]
    [InlineData( false, "0" )]
    public void SqlHelpers_GetDbLiteral_ShouldReturnCorrectResult_ForBool(bool value, string expected)
    {
        var result = SqlHelpers.GetDbLiteral( value );
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 1234567, "1234567" )]
    [InlineData( 0, "0" )]
    [InlineData( -1234567, "-1234567" )]
    public void SqlHelpers_GetDbLiteral_ShouldReturnCorrectResult_ForInt64(long value, string expected)
    {
        var result = SqlHelpers.GetDbLiteral( value );
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 1234567, "1234567" )]
    [InlineData( 0, "0" )]
    public void SqlHelpers_GetDbLiteral_ShouldReturnCorrectResult_ForUInt64(ulong value, string expected)
    {
        var result = SqlHelpers.GetDbLiteral( value );
        result.TestEquals( expected ).Go();
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
        result.TestEquals( expected ).Go();
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
        result.TestEquals( expected ).Go();
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
        result.TestEquals( text ).Go();
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
        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void SqlHelpers_GetDbLiteral_ShouldReturnCorrectResult_ForEmptyBinary()
    {
        var result = SqlHelpers.GetDbLiteral( Array.Empty<byte>() );
        result.TestEquals( "X''" ).Go();
    }

    [Fact]
    public void SqlHelpers_GetDbLiteral_ShouldReturnCorrectResult_ForNonEmptyBinary()
    {
        var value = new byte[] { 0, 10, 21, 31, 42, 58, 73, 89, 104, 129, 155, 181, 206, 233, 255 };
        var result = SqlHelpers.GetDbLiteral( value );
        result.TestEquals( "X'000A151F2A3A495968819BB5CEE9FF'" ).Go();
    }

    [Theory]
    [InlineData( "foo", "bar", "foo.bar" )]
    [InlineData( "", "bar", "bar" )]
    public void SqlHelpers_GetFullName_ShouldReturnCorrectResult_ForSchemaObject(string schemaName, string name, string expected)
    {
        var result = SqlHelpers.GetFullName( schemaName, name );
        result.TestEquals( expected ).Go();
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
        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void SqlHelpers_GetReferencingObjectsInOrderOfCreation_ShouldReturnCorrectResult_WhenObjectIsNotReferenced()
    {
        var obj = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var result = SqlHelpers.GetReferencingObjectsInOrderOfCreation( obj );
        result.TestEmpty().Go();
    }

    [Fact]
    public void SqlHelpers_GetReferencingObjectsInOrderOfCreation_ShouldReturnCorrectResult_WhenAllReferencesAreFilteredOut()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var obj = schema.Objects.CreateTable( "T" );
        SqlDatabaseBuilderMock.AddReference( obj, SqlObjectBuilderReferenceSource.Create( schema ) );

        var result = SqlHelpers.GetReferencingObjectsInOrderOfCreation( obj, _ => false );

        result.TestEmpty().Go();
    }

    [Fact]
    public void SqlHelpers_GetReferencingObjectsInOrderOfCreation_ShouldReturnCorrectResult_WhenAllObjectsAreIncluded()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C1" );
        var obj = table.Columns.Create( "C2" );
        SqlDatabaseBuilderMock.AddReference( obj, SqlObjectBuilderReferenceSource.Create( column ) );
        SqlDatabaseBuilderMock.AddReference( obj, SqlObjectBuilderReferenceSource.Create( schema ) );
        SqlDatabaseBuilderMock.AddReference( obj, SqlObjectBuilderReferenceSource.Create( table ) );

        var result = SqlHelpers.GetReferencingObjectsInOrderOfCreation( obj );

        result.TestSequence( [ schema, table, column ] ).Go();
    }

    [Fact]
    public void SqlHelpers_GetReferencingObjectsInOrderOfCreation_ShouldReturnCorrectResult_WhenSomeObjectsAreFilteredOut()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C1" );
        var obj = table.Columns.Create( "C2" );
        SqlDatabaseBuilderMock.AddReference( obj, SqlObjectBuilderReferenceSource.Create( column ) );
        SqlDatabaseBuilderMock.AddReference( obj, SqlObjectBuilderReferenceSource.Create( schema ) );
        SqlDatabaseBuilderMock.AddReference( obj, SqlObjectBuilderReferenceSource.Create( table ) );

        var result = SqlHelpers.GetReferencingObjectsInOrderOfCreation( obj, o => o.Source.Object.Type != SqlObjectType.Schema );

        result.TestSequence( [ table, column ] ).Go();
    }
}
