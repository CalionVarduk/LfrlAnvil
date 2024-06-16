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
using LfrlAnvil.Exceptions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using ExceptionResources = LfrlAnvil.Sql.Exceptions.ExceptionResources;

namespace LfrlAnvil.MySql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlSchemaBuilder : SqlSchemaBuilder
{
    internal MySqlSchemaBuilder(MySqlDatabaseBuilder database, string name)
        : base( database, name, new MySqlObjectBuilderCollection() ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new MySqlDatabaseBuilder Database => ReinterpretCast.To<MySqlDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlSchemaBuilder.Objects" />
    public new MySqlObjectBuilderCollection Objects => ReinterpretCast.To<MySqlObjectBuilderCollection>( base.Objects );

    /// <inheritdoc />
    public override bool CanRemove => base.CanRemove && ! Name.Equals( Database.CommonSchemaName, StringComparison.OrdinalIgnoreCase );

    /// <inheritdoc cref="SqlSchemaBuilder.SetName(string)" />
    public new MySqlSchemaBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc />
    protected override void BeforeRemove()
    {
        ThrowIfDefault();

        if ( Name.Equals( Database.CommonSchemaName, StringComparison.OrdinalIgnoreCase ) )
            ExceptionThrower.Throw( SqlHelpers.CreateObjectBuilderException( Database, ExceptionResources.CommonSchemaCannotBeRemoved ) );

        ThrowIfReferenced();
        QuickRemoveObjects();
        RemoveFromCollection( Database.Schemas, this );
    }
}
