﻿// Copyright 2024 Łukasz Furlepa
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
using System.Data.Common;
using System.Globalization;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.MySql.Internal;

internal sealed class MySqlDatabaseCommitVersionsContext : SqlDatabaseCommitVersionsContext
{
    protected override void SetInsertVersionHistoryRecordCommandParameters(
        DbParameterCollection parameters,
        int ordinal,
        Version version,
        string description)
    {
        Assume.Equals( parameters.Count, 7 );
        var pOrdinal = parameters[0];
        var pVersionMajor = parameters[1];
        var pVersionMinor = parameters[2];
        var pVersionBuild = parameters[3];
        var pVersionRevision = parameters[4];
        var pDescription = parameters[5];
        var pCommitDateUtc = parameters[6];

        var pOrdinalType = GetVersionHistoryColumnType( pOrdinal.ParameterName );
        var pVersionMajorType = GetVersionHistoryColumnType( pVersionMajor.ParameterName );
        var pVersionMinorType = GetVersionHistoryColumnType( pVersionMinor.ParameterName );
        var pVersionBuildType = GetVersionHistoryColumnType( pVersionBuild.ParameterName );
        var pVersionRevisionType = GetVersionHistoryColumnType( pVersionRevision.ParameterName );
        var pDescriptionType = GetVersionHistoryColumnType( pDescription.ParameterName );
        var pCommitDateUtcType = GetVersionHistoryColumnType( pCommitDateUtc.ParameterName );

        pOrdinal.Value = pOrdinalType.TryToParameterValue( ordinal );
        pVersionMajor.Value = pVersionMajorType.TryToParameterValue( version.Major );
        pVersionMinor.Value = pVersionMinorType.TryToParameterValue( version.Minor );
        pVersionBuild.Value = version.Build >= 0 ? pVersionBuildType.TryToParameterValue( version.Build ) : DBNull.Value;
        pVersionRevision.Value = version.Revision >= 0 ? pVersionRevisionType.TryToParameterValue( version.Revision ) : DBNull.Value;
        pDescription.Value = pDescriptionType.TryToParameterValue( description );
        pCommitDateUtc.Value = pCommitDateUtcType.TryToParameterValue(
            DateTime.UtcNow.ToString( SqlHelpers.DateTimeFormatTick, CultureInfo.InvariantCulture ) );
    }
}
