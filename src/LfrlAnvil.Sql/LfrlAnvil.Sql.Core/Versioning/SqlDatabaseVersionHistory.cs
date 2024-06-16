// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sql.Versioning;

/// <summary>
/// Represents a sequential history of all database versions.
/// </summary>
public class SqlDatabaseVersionHistory
{
    /// <summary>
    /// Represents an identifier of the initial <b>0.0</b> version.
    /// </summary>
    public static readonly Version InitialVersion = new Version();

    private readonly ISqlDatabaseVersion[] _versions;

    /// <summary>
    /// Creates a new <see cref="SqlDatabaseVersionHistory"/> instance.
    /// </summary>
    /// <param name="versions">Sequential collection of all database versions.</param>
    /// <exception cref="SqlDatabaseVersionHistoryException">
    /// When first version's identifier is equal to <see cref="InitialVersion"/>
    /// or when versions are not ordered in ascending order by their <see cref="ISqlDatabaseVersion.Value"/>.
    /// </exception>
    public SqlDatabaseVersionHistory(IEnumerable<ISqlDatabaseVersion> versions)
        : this( versions.ToArray() ) { }

    /// <summary>
    /// Creates a new <see cref="SqlDatabaseVersionHistory"/> instance.
    /// </summary>
    /// <param name="versions">Sequential collection of all database versions.</param>
    /// <exception cref="SqlDatabaseVersionHistoryException">
    /// When first version's identifier is equal to <see cref="InitialVersion"/>
    /// or when versions are not ordered in ascending order by their <see cref="ISqlDatabaseVersion.Value"/>
    /// or when version identifiers are not unique.
    /// </exception>
    public SqlDatabaseVersionHistory(params ISqlDatabaseVersion[] versions)
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

    /// <summary>
    /// Sequential collection of all database versions.
    /// </summary>
    public ReadOnlySpan<ISqlDatabaseVersion> Versions => _versions;

    /// <summary>
    /// Compares this DB version history to a sequence of versions applied to the database.
    /// </summary>
    /// <param name="records">Sequence of versions applied to the database.</param>
    /// <returns>New <see cref="DatabaseComparisonResult"/> instance.</returns>
    /// <exception cref="SqlDatabaseVersionHistoryException">
    /// When there is any discrepancy between this DB version history and the provided sequence of versions applied to the database.
    /// </exception>
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
            : ReadOnlySpan<ISqlDatabaseVersion>.Empty;

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

    /// <summary>
    /// Represents a result of comparison of an <see cref="SqlDatabaseVersionHistory"/> instance
    /// with a sequence of versions already applied to the database.
    /// </summary>
    public readonly struct DatabaseComparisonResult
    {
        private readonly ISqlDatabaseVersion[] _allVersions;
        private readonly int _committedVersionCount;

        internal DatabaseComparisonResult(
            ISqlDatabaseVersion[] allVersions,
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

        /// <summary>
        /// Collection of all versions applied to the database.
        /// </summary>
        public ReadOnlySpan<ISqlDatabaseVersion> Committed => _allVersions.AsSpan( 0, _committedVersionCount );

        /// <summary>
        /// Collection of all versions that haven't been applied to the database yet.
        /// </summary>
        public ReadOnlySpan<ISqlDatabaseVersion> Uncommitted => _allVersions.AsSpan( _committedVersionCount );

        /// <summary>
        /// Specifies the identifier of the last applied version to the database.
        /// </summary>
        public Version Current { get; }

        /// <summary>
        /// Specifies the <see cref="SqlDatabaseVersionRecord.Ordinal"/> value of the next version to be applied to the database.
        /// </summary>
        public int NextOrdinal { get; }
    }

    [Pure]
    private static int GetVersionCountUpTo(ReadOnlySpan<ISqlDatabaseVersion> versions, Version target)
    {
        var lo = 0;
        var hi = versions.Length - 1;

        while ( lo <= hi )
        {
            var mid = unchecked( ( int )((( uint )hi + ( uint )lo) >> 1) );
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
