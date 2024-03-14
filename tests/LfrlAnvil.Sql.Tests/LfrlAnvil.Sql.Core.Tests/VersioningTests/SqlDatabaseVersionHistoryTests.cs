using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests.VersioningTests;

public class SqlDatabaseVersionHistoryTests : TestsBase
{
    [Fact]
    public void InitialVersion_ShouldBeEqualToZeroVersion()
    {
        var sut = SqlDatabaseVersionHistory.InitialVersion;
        sut.Should().Be( Version.Parse( "0.0" ) );
    }

    [Fact]
    public void Ctor_ShouldCreateCorrectHistory_WhenVersionsAreEmpty()
    {
        var sut = new SqlDatabaseVersionHistory( Enumerable.Empty<ISqlDatabaseVersion>() );
        sut.Versions.Length.Should().Be( 0 );
    }

    [Fact]
    public void Ctor_ShouldCreateCorrectHistory_WhenThereIsOnlyOneVersionWithValueGreaterThanInitial()
    {
        var version = SqlDatabaseVersion.Create( Version.Parse( "0.0.0.1" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var sut = new SqlDatabaseVersionHistory( version );
        sut.Versions.ToArray().Should().BeSequentiallyEqualTo( version );
    }

    [Fact]
    public void Ctor_ShouldThrowSqlDatabaseVersionHistoryException_WhenThereIsOnlyOneVersionWithValueEqualToInitial()
    {
        var version = SqlDatabaseVersion.Create( Version.Parse( "0.0" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var action = Lambda.Of( () => new SqlDatabaseVersionHistory( version ) );
        action.Should().ThrowExactly<SqlDatabaseVersionHistoryException>();
    }

    [Fact]
    public void Ctor_ShouldCreateCorrectHistory_WhenThereAreManyVersionsWithIncreasingValues()
    {
        var version1 = SqlDatabaseVersion.Create( Version.Parse( "0.1" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version2 = SqlDatabaseVersion.Create( Version.Parse( "0.2" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version3 = SqlDatabaseVersion.Create( Version.Parse( "0.3" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );

        var sut = new SqlDatabaseVersionHistory( version1, version2, version3 );

        sut.Versions.ToArray().Should().BeSequentiallyEqualTo( version1, version2, version3 );
    }

    [Fact]
    public void Ctor_ShouldThrowSqlDatabaseVersionHistoryException_WhenThereAreManyVersionsAndSuccessorHasValueEqualToPredecessorValue()
    {
        var version1 = SqlDatabaseVersion.Create( Version.Parse( "0.1" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version2 = SqlDatabaseVersion.Create( Version.Parse( "0.2" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version3 = SqlDatabaseVersion.Create( Version.Parse( "0.2" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );

        var action = Lambda.Of( () => new SqlDatabaseVersionHistory( version1, version2, version3 ) );

        action.Should().ThrowExactly<SqlDatabaseVersionHistoryException>();
    }

    [Fact]
    public void Ctor_ShouldThrowSqlDatabaseVersionHistoryException_WhenThereAreManyVersionsAndSuccessorHasValueLessThenPredecessorValue()
    {
        var version1 = SqlDatabaseVersion.Create( Version.Parse( "0.1" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version2 = SqlDatabaseVersion.Create( Version.Parse( "0.3" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version3 = SqlDatabaseVersion.Create( Version.Parse( "0.2" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );

        var action = Lambda.Of( () => new SqlDatabaseVersionHistory( version1, version2, version3 ) );

        action.Should().ThrowExactly<SqlDatabaseVersionHistoryException>();
    }

    [Fact]
    public void CompareToDatabase_ShouldReturnCorrectResult_WhenHistoryAndRecordsAreEmpty()
    {
        var records = Array.Empty<SqlDatabaseVersionRecord>();
        var sut = new SqlDatabaseVersionHistory();

        var result = sut.CompareToDatabase( records );

        using ( new AssertionScope() )
        {
            result.Uncommitted.Length.Should().Be( 0 );
            result.Committed.Length.Should().Be( 0 );
            result.Current.Should().BeSameAs( SqlDatabaseVersionHistory.InitialVersion );
            result.NextOrdinal.Should().Be( 1 );
        }
    }

    [Fact]
    public void CompareToDatabase_ShouldReturnCorrectResult_WhenRecordsAreEmpty()
    {
        var version1 = SqlDatabaseVersion.Create( Version.Parse( "0.1" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version2 = SqlDatabaseVersion.Create( Version.Parse( "0.2" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version3 = SqlDatabaseVersion.Create( Version.Parse( "0.3" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var records = Array.Empty<SqlDatabaseVersionRecord>();

        var sut = new SqlDatabaseVersionHistory( version1, version2, version3 );

        var result = sut.CompareToDatabase( records );

        using ( new AssertionScope() )
        {
            result.Uncommitted.ToArray().Should().BeSequentiallyEqualTo( version1, version2, version3 );
            result.Committed.Length.Should().Be( 0 );
            result.Current.Should().BeSameAs( SqlDatabaseVersionHistory.InitialVersion );
            result.NextOrdinal.Should().Be( 1 );
        }
    }

    [Fact]
    public void CompareToDatabase_ShouldReturnCorrectResult_WhenHistoryAndRecordsAreEquivalent()
    {
        var version1 = SqlDatabaseVersion.Create( Version.Parse( "0.1" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version2 = SqlDatabaseVersion.Create( Version.Parse( "0.2" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version3 = SqlDatabaseVersion.Create( Version.Parse( "0.3" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var records = new[]
        {
            new SqlDatabaseVersionRecord( 1, Version.Parse( "0.1" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero ),
            new SqlDatabaseVersionRecord( 2, Version.Parse( "0.2" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero ),
            new SqlDatabaseVersionRecord( 3, Version.Parse( "0.3" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero )
        };

        var sut = new SqlDatabaseVersionHistory( version1, version2, version3 );

        var result = sut.CompareToDatabase( records );

        using ( new AssertionScope() )
        {
            result.Uncommitted.Length.Should().Be( 0 );
            result.Committed.ToArray().Should().BeSequentiallyEqualTo( version1, version2, version3 );
            result.Current.Should().Be( Version.Parse( "0.3" ) );
            result.NextOrdinal.Should().Be( 4 );
        }
    }

    [Fact]
    public void CompareToDatabase_ShouldReturnCorrectResult_WhenHistoryAndRecordsAreEquivalentAndRecordsOnlyContainLastVersion()
    {
        var version1 = SqlDatabaseVersion.Create( Version.Parse( "0.1" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version2 = SqlDatabaseVersion.Create( Version.Parse( "0.2" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version3 = SqlDatabaseVersion.Create( Version.Parse( "0.3" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var records = new[]
        {
            new SqlDatabaseVersionRecord( 3, Version.Parse( "0.3" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero )
        };

        var sut = new SqlDatabaseVersionHistory( version1, version2, version3 );

        var result = sut.CompareToDatabase( records );

        using ( new AssertionScope() )
        {
            result.Uncommitted.Length.Should().Be( 0 );
            result.Committed.ToArray().Should().BeSequentiallyEqualTo( version1, version2, version3 );
            result.Current.Should().Be( Version.Parse( "0.3" ) );
            result.NextOrdinal.Should().Be( 4 );
        }
    }

    [Fact]
    public void CompareToDatabase_ShouldReturnCorrectResult_WhenHistoryAndRecordsAreDifferentWithAlignedCommittedVersions()
    {
        var version1 = SqlDatabaseVersion.Create( Version.Parse( "0.1" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version2 = SqlDatabaseVersion.Create( Version.Parse( "0.2" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version3 = SqlDatabaseVersion.Create( Version.Parse( "0.3" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version4 = SqlDatabaseVersion.Create( Version.Parse( "0.4" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version5 = SqlDatabaseVersion.Create( Version.Parse( "0.5" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var records = new[]
        {
            new SqlDatabaseVersionRecord( 1, Version.Parse( "0.1" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero ),
            new SqlDatabaseVersionRecord( 2, Version.Parse( "0.2" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero ),
            new SqlDatabaseVersionRecord( 3, Version.Parse( "0.3" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero )
        };

        var sut = new SqlDatabaseVersionHistory( version1, version2, version3, version4, version5 );

        var result = sut.CompareToDatabase( records );

        using ( new AssertionScope() )
        {
            result.Uncommitted.ToArray().Should().BeSequentiallyEqualTo( version4, version5 );
            result.Committed.ToArray().Should().BeSequentiallyEqualTo( version1, version2, version3 );
            result.Current.Should().Be( Version.Parse( "0.3" ) );
            result.NextOrdinal.Should().Be( 4 );
        }
    }

    [Fact]
    public void CompareToDatabase_ShouldReturnCorrectResult_WhenHistoryAndRecordsAreEquivalentAndRecordsContainPartialVersions()
    {
        var version1 = SqlDatabaseVersion.Create( Version.Parse( "0.1" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version2 = SqlDatabaseVersion.Create( Version.Parse( "0.2" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version3 = SqlDatabaseVersion.Create( Version.Parse( "0.3" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var records = new[]
        {
            new SqlDatabaseVersionRecord( 2, Version.Parse( "0.2" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero ),
            new SqlDatabaseVersionRecord( 3, Version.Parse( "0.3" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero )
        };

        var sut = new SqlDatabaseVersionHistory( version1, version2, version3 );

        var result = sut.CompareToDatabase( records );

        using ( new AssertionScope() )
        {
            result.Uncommitted.Length.Should().Be( 0 );
            result.Committed.ToArray().Should().BeSequentiallyEqualTo( version1, version2, version3 );
            result.Current.Should().Be( Version.Parse( "0.3" ) );
            result.NextOrdinal.Should().Be( 4 );
        }
    }

    [Fact]
    public void CompareToDatabase_ShouldThrowSqlDatabaseVersionHistoryException_WhenDatabaseVersionDoesNotExist()
    {
        var version1 = SqlDatabaseVersion.Create( Version.Parse( "0.1" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version2 = SqlDatabaseVersion.Create( Version.Parse( "0.2" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version3 = SqlDatabaseVersion.Create( Version.Parse( "0.3" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var records = new[]
        {
            new SqlDatabaseVersionRecord( 1, Version.Parse( "0.1" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero ),
            new SqlDatabaseVersionRecord( 2, Version.Parse( "0.1.1" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero )
        };

        var sut = new SqlDatabaseVersionHistory( version1, version2, version3 );

        var action = Lambda.Of(
            () => { _ = sut.CompareToDatabase( records ); } );

        action.Should().ThrowExactly<SqlDatabaseVersionHistoryException>();
    }

    [Fact]
    public void CompareToDatabase_ShouldThrowSqlDatabaseVersionHistoryException_WhenHistoryIsEmpty()
    {
        var records = new[]
        {
            new SqlDatabaseVersionRecord( 1, Version.Parse( "0.1" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero )
        };

        var sut = new SqlDatabaseVersionHistory();

        var action = Lambda.Of(
            () => { _ = sut.CompareToDatabase( records ); } );

        action.Should().ThrowExactly<SqlDatabaseVersionHistoryException>();
    }

    [Fact]
    public void CompareToDatabase_ShouldThrowSqlDatabaseVersionHistoryException_WhenHistoryAndRecordsCountDoNotMatchWithMoreHistory()
    {
        var version1 = SqlDatabaseVersion.Create( Version.Parse( "0.1" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version2 = SqlDatabaseVersion.Create( Version.Parse( "0.2" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version3 = SqlDatabaseVersion.Create( Version.Parse( "0.2.1" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version4 = SqlDatabaseVersion.Create( Version.Parse( "0.3" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var records = new[]
        {
            new SqlDatabaseVersionRecord( 1, Version.Parse( "0.1" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero ),
            new SqlDatabaseVersionRecord( 2, Version.Parse( "0.2" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero ),
            new SqlDatabaseVersionRecord( 3, Version.Parse( "0.3" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero )
        };

        var sut = new SqlDatabaseVersionHistory( version1, version2, version3, version4 );

        var action = Lambda.Of(
            () => { _ = sut.CompareToDatabase( records ); } );

        action.Should().ThrowExactly<SqlDatabaseVersionHistoryException>();
    }

    [Fact]
    public void CompareToDatabase_ShouldThrowSqlDatabaseVersionHistoryException_WhenHistoryAndRecordsCountDoNotMatchWithMoreRecords()
    {
        var version1 = SqlDatabaseVersion.Create( Version.Parse( "0.1" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version2 = SqlDatabaseVersion.Create( Version.Parse( "0.3" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version3 = SqlDatabaseVersion.Create( Version.Parse( "0.4" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var records = new[]
        {
            new SqlDatabaseVersionRecord( 1, Version.Parse( "0.1" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero ),
            new SqlDatabaseVersionRecord( 2, Version.Parse( "0.2" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero ),
            new SqlDatabaseVersionRecord( 3, Version.Parse( "0.3" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero )
        };

        var sut = new SqlDatabaseVersionHistory( version1, version2, version3 );

        var action = Lambda.Of(
            () => { _ = sut.CompareToDatabase( records ); } );

        action.Should().ThrowExactly<SqlDatabaseVersionHistoryException>();
    }

    [Fact]
    public void CompareToDatabase_ShouldThrowSqlDatabaseVersionHistoryException_WhenFirstRecordOrdinalExceedsCommittedHistoryCount()
    {
        var version1 = SqlDatabaseVersion.Create( Version.Parse( "0.1" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version2 = SqlDatabaseVersion.Create( Version.Parse( "0.2" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version3 = SqlDatabaseVersion.Create( Version.Parse( "0.3" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version4 = SqlDatabaseVersion.Create( Version.Parse( "0.4" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version5 = SqlDatabaseVersion.Create( Version.Parse( "0.5" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var records = new[]
        {
            new SqlDatabaseVersionRecord( 4, Version.Parse( "0.2" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero )
        };

        var sut = new SqlDatabaseVersionHistory( version1, version2, version3, version4, version5 );

        var action = Lambda.Of(
            () => { _ = sut.CompareToDatabase( records ); } );

        action.Should().ThrowExactly<SqlDatabaseVersionHistoryException>();
    }

    [Fact]
    public void CompareToDatabase_ShouldThrowSqlDatabaseVersionHistoryException_WhenHistoryAndPartialRecordsCountDoNotMatchWithMoreHistory()
    {
        var version1 = SqlDatabaseVersion.Create( Version.Parse( "0.1" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version2 = SqlDatabaseVersion.Create( Version.Parse( "0.1.1" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version3 = SqlDatabaseVersion.Create( Version.Parse( "0.2" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version4 = SqlDatabaseVersion.Create( Version.Parse( "0.3" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var records = new[]
        {
            new SqlDatabaseVersionRecord( 2, Version.Parse( "0.2" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero ),
            new SqlDatabaseVersionRecord( 3, Version.Parse( "0.3" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero )
        };

        var sut = new SqlDatabaseVersionHistory( version1, version2, version3, version4 );

        var action = Lambda.Of(
            () => { _ = sut.CompareToDatabase( records ); } );

        action.Should().ThrowExactly<SqlDatabaseVersionHistoryException>();
    }

    [Fact]
    public void CompareToDatabase_ShouldThrowSqlDatabaseVersionHistoryException_WhenHistoryAndPartialRecordsCountDoNotMatchWithMoreRecords()
    {
        var version1 = SqlDatabaseVersion.Create( Version.Parse( "0.3" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version2 = SqlDatabaseVersion.Create( Version.Parse( "0.4" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var records = new[]
        {
            new SqlDatabaseVersionRecord( 2, Version.Parse( "0.2" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero ),
            new SqlDatabaseVersionRecord( 3, Version.Parse( "0.3" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero )
        };

        var sut = new SqlDatabaseVersionHistory( version1, version2 );

        var action = Lambda.Of(
            () => { _ = sut.CompareToDatabase( records ); } );

        action.Should().ThrowExactly<SqlDatabaseVersionHistoryException>();
    }

    [Fact]
    public void CompareToDatabase_ShouldThrowSqlDatabaseVersionHistoryException_WhenSomeVersionsAtTheSamePositionDoNotMatch()
    {
        var version1 = SqlDatabaseVersion.Create( Version.Parse( "0.1.1" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version2 = SqlDatabaseVersion.Create( Version.Parse( "0.2.2" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version3 = SqlDatabaseVersion.Create( Version.Parse( "0.3" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var records = new[]
        {
            new SqlDatabaseVersionRecord( 1, Version.Parse( "0.1" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero ),
            new SqlDatabaseVersionRecord( 2, Version.Parse( "0.2" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero ),
            new SqlDatabaseVersionRecord( 3, Version.Parse( "0.3" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero )
        };

        var sut = new SqlDatabaseVersionHistory( version1, version2, version3 );

        var action = Lambda.Of(
            () => { _ = sut.CompareToDatabase( records ); } );

        action.Should().ThrowExactly<SqlDatabaseVersionHistoryException>();
    }

    [Fact]
    public void
        CompareToDatabase_ShouldThrowSqlDatabaseVersionHistoryException_WhenSomeVersionsAtTheSamePositionDoNotMatchWithPartialRecords()
    {
        var version1 = SqlDatabaseVersion.Create( Version.Parse( "0.1" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version2 = SqlDatabaseVersion.Create( Version.Parse( "0.2.1" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var version3 = SqlDatabaseVersion.Create( Version.Parse( "0.3" ), Substitute.For<Action<ISqlDatabaseBuilder>>() );
        var records = new[]
        {
            new SqlDatabaseVersionRecord( 2, Version.Parse( "0.2" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero ),
            new SqlDatabaseVersionRecord( 3, Version.Parse( "0.3" ), string.Empty, DateTime.UnixEpoch, TimeSpan.Zero )
        };

        var sut = new SqlDatabaseVersionHistory( version1, version2, version3 );

        var action = Lambda.Of(
            () => { _ = sut.CompareToDatabase( records ); } );

        action.Should().ThrowExactly<SqlDatabaseVersionHistoryException>();
    }
}
