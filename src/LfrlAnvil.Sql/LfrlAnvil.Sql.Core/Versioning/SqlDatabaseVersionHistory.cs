using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sql.Versioning;

public class SqlDatabaseVersionHistory
{
    public static readonly Version InitialVersion = new Version();

    private readonly SqlDatabaseVersion[] _versions;

    public SqlDatabaseVersionHistory(IEnumerable<SqlDatabaseVersion> versions)
        : this( versions.ToArray() ) { }

    public SqlDatabaseVersionHistory(params SqlDatabaseVersion[] versions)
    {
        _versions = versions;

        var span = versions.AsSpan();
        if ( span.Length == 0 )
            return;

        var errors = Chain<string>.Empty;
        if ( span[0].Value == InitialVersion )
            errors = errors.Extend( ExceptionResources.FirstVersionHasValueEqualToInitialValue( span[0] ) );

        for ( var i = 1; i < span.Length; ++i )
        {
            var prev = span[i - 1];
            var current = span[i];

            if ( current.Value <= prev.Value )
                errors = errors.Extend( ExceptionResources.VersionIsPrecededByVersionWithGreaterOrEqualValue( i, prev, current ) );
        }

        if ( errors.Count > 0 )
            throw new SqlDatabaseVersionHistoryException( errors );
    }

    public ReadOnlySpan<SqlDatabaseVersion> Versions => _versions;

    [Pure]
    public DatabaseComparisonResult CompareToDatabase(ReadOnlySpan<SqlDatabaseVersionRecord> records)
    {
        if ( records.Length == 0 )
            return new DatabaseComparisonResult( _versions, 0, InitialVersion, 1 );

        var allVersions = Versions;
        var errors = Chain<string>.Empty;

        var ordinalOffset = records[0].Ordinal - 1;
        Assume.IsGreaterThanOrEqualTo( ordinalOffset, 0 );
        var dbVersion = records[^1].Version;
        var lastOrdinal = records[^1].Ordinal;
        Assume.IsGreaterThanOrEqualTo( lastOrdinal, records[0].Ordinal );

        var committedVersionCount = GetVersionCountUpTo( allVersions, dbVersion );
        if ( committedVersionCount == 0 )
            errors = errors.Extend( ExceptionResources.DatabaseVersionDoesNotExistInHistory( dbVersion ) );

        if ( lastOrdinal != committedVersionCount )
            errors = errors.Extend( ExceptionResources.VersionCountDoesNotMatch( lastOrdinal, committedVersionCount, dbVersion ) );

        var persistedCommittedVersions = ordinalOffset < committedVersionCount
            ? allVersions.Slice( ordinalOffset, committedVersionCount - ordinalOffset )
            : ReadOnlySpan<SqlDatabaseVersion>.Empty;

        if ( ordinalOffset > 0 && records.Length != persistedCommittedVersions.Length )
        {
            var error = ExceptionResources.PersistedVersionCountDoesNotMatch(
                records.Length,
                persistedCommittedVersions.Length,
                dbVersion );

            errors = errors.Extend( error );
        }

        var length = Math.Min( records.Length, persistedCommittedVersions.Length );
        for ( var i = 0; i < length; ++i )
        {
            var record = records[i];
            var historyVersion = persistedCommittedVersions[i];

            if ( record.Version != historyVersion.Value )
                errors = errors.Extend( ExceptionResources.DatabaseAndHistoryVersionDoNotMatch( record, historyVersion.Value ) );
        }

        if ( errors.Count > 0 )
            throw new SqlDatabaseVersionHistoryException( errors );

        return new DatabaseComparisonResult( _versions, committedVersionCount, dbVersion, lastOrdinal + 1 );
    }

    public readonly struct DatabaseComparisonResult
    {
        private readonly SqlDatabaseVersion[] _allVersions;
        private readonly int _committedVersionCount;

        internal DatabaseComparisonResult(
            SqlDatabaseVersion[] allVersions,
            int committedVersionCount,
            Version current,
            int nextOrdinal)
        {
            Assume.IsGreaterThanOrEqualTo( committedVersionCount, 0 );
            _allVersions = allVersions;
            _committedVersionCount = committedVersionCount;
            Current = current;
            NextOrdinal = nextOrdinal;
        }

        public ReadOnlySpan<SqlDatabaseVersion> Committed => _allVersions.AsSpan( 0, _committedVersionCount );
        public ReadOnlySpan<SqlDatabaseVersion> Uncommitted => _allVersions.AsSpan( _committedVersionCount );
        public Version Current { get; }
        public int NextOrdinal { get; }
    }

    [Pure]
    private static int GetVersionCountUpTo(ReadOnlySpan<SqlDatabaseVersion> versions, Version target)
    {
        var lo = 0;
        var hi = versions.Length - 1;

        while ( lo <= hi )
        {
            var mid = unchecked( (int)(((uint)hi + (uint)lo) >> 1) );
            var cmp = target.CompareTo( versions[mid].Value );

            if ( cmp == 0 )
                return mid + 1;

            if ( cmp > 0 )
                lo = mid + 1;
            else
                hi = mid - 1;
        }

        return 0;
    }
}
