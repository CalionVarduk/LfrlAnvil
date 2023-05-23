using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests;

public class SqlDatabaseFactoryProviderTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldReturnEmptyProvider()
    {
        var sut = new SqlDatabaseFactoryProvider();
        sut.SupportedDialects.Should().BeEmpty();
    }

    [Fact]
    public void RegisterFactory_ShouldAddFactoryForDialect_WhenOneDoesNotExist()
    {
        var dialect = new SqlDialect( "FooSql" );
        var factory = Substitute.For<ISqlDatabaseFactory>();
        factory.Dialect.Returns( dialect );

        var sut = new SqlDatabaseFactoryProvider();

        var result = sut.RegisterFactory( factory );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.SupportedDialects.Should().BeSequentiallyEqualTo( dialect );
            result.GetFor( dialect ).Should().BeSameAs( factory );
        }
    }

    [Fact]
    public void RegisterFactory_ShouldAddFactoryForDialect_WhenOneDoesNotExistAndProviderIsNotEmpty()
    {
        var otherDialect = new SqlDialect( "BarSql" );
        var otherFactory = Substitute.For<ISqlDatabaseFactory>();
        otherFactory.Dialect.Returns( otherDialect );

        var dialect = new SqlDialect( "FooSql" );
        var factory = Substitute.For<ISqlDatabaseFactory>();
        factory.Dialect.Returns( dialect );

        var sut = new SqlDatabaseFactoryProvider();
        sut.RegisterFactory( otherFactory );

        var result = sut.RegisterFactory( factory );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.SupportedDialects.Should().HaveCount( 2 );
            result.SupportedDialects.Should().BeEquivalentTo( otherDialect, dialect );
            result.GetFor( dialect ).Should().BeSameAs( factory );
            result.GetFor( otherDialect ).Should().BeSameAs( otherFactory );
        }
    }

    [Fact]
    public void RegisterFactory_ShouldOverrideFactoryForDialect_WhenOneExists()
    {
        var dialect = new SqlDialect( "FooSql" );
        var oldFactory = Substitute.For<ISqlDatabaseFactory>();
        oldFactory.Dialect.Returns( dialect );

        var factory = Substitute.For<ISqlDatabaseFactory>();
        factory.Dialect.Returns( dialect );

        var sut = new SqlDatabaseFactoryProvider();
        sut.RegisterFactory( oldFactory );

        var result = sut.RegisterFactory( factory );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.SupportedDialects.Should().BeSequentiallyEqualTo( dialect );
            result.GetFor( dialect ).Should().BeSameAs( factory );
        }
    }

    [Fact]
    public void GetFor_ShouldThrowKeyNotFoundException_WhenDialectIsNotRegistered()
    {
        var dialect = new SqlDialect( "FooSql" );
        var sut = new SqlDatabaseFactoryProvider();

        var action = Lambda.Of( () => sut.GetFor( dialect ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }
}
