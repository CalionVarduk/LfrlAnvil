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
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Internal;
using ExceptionResources = LfrlAnvil.Sql.Exceptions.ExceptionResources;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <inheritdoc cref="ISqlSchemaBuilder" />
public abstract class SqlSchemaBuilder : SqlObjectBuilder, ISqlSchemaBuilder
{
    /// <summary>
    /// Creates a new <see cref="SqlSchemaBuilder"/> instance.
    /// </summary>
    /// <param name="database">Database that this schema belongs to.</param>
    /// <param name="name">Object's name.</param>
    /// <param name="objects">Collection of objects that belong to this schema.</param>
    protected SqlSchemaBuilder(SqlDatabaseBuilder database, string name, SqlObjectBuilderCollection objects)
        : base( database, SqlObjectType.Schema, name )
    {
        Objects = objects;
        Objects.SetSchema( this );
    }

    /// <inheritdoc cref="ISqlSchemaBuilder.Objects" />
    public SqlObjectBuilderCollection Objects { get; }

    /// <inheritdoc />
    public override bool CanRemove => ! ReferenceEquals( this, Database.Schemas.Default ) && base.CanRemove;

    ISqlObjectBuilderCollection ISqlSchemaBuilder.Objects => Objects;

    /// <inheritdoc cref="SqlObjectBuilder.SetName(string)" />
    public new SqlSchemaBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <summary>
    /// Throws an exception when this schema is the <see cref="ISqlSchemaBuilderCollection.Default"/> schema.
    /// </summary>
    /// <exception cref="SqlObjectBuilderException">
    /// When this schema is the <see cref="ISqlSchemaBuilderCollection.Default"/> schema.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected void ThrowIfDefault()
    {
        if ( ReferenceEquals( this, Database.Schemas.Default ) )
            ExceptionThrower.Throw( SqlHelpers.CreateObjectBuilderException( Database, ExceptionResources.DefaultSchemaCannotBeRemoved ) );
    }

    /// <inheritdoc />
    protected override SqlPropertyChange<string> BeforeNameChange(string newValue)
    {
        var change = base.BeforeNameChange( newValue );
        if ( change.IsCancelled )
            return change;

        ChangeNameInCollection( Database.Schemas, this, newValue );
        return change;
    }

    /// <inheritdoc />
    protected override void AfterNameChange(string originalValue)
    {
        ResetAllTableAndViewInfoCache();
        AddNameChange( this, this, originalValue );
    }

    /// <inheritdoc />
    protected override void BeforeRemove()
    {
        ThrowIfDefault();
        base.BeforeRemove();
        QuickRemoveObjects();
        RemoveFromCollection( Database.Schemas, this );
    }

    /// <inheritdoc />
    protected override void AfterRemove()
    {
        AddRemoval( this, this );
    }

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">Quick removal of schemas is not supported by default.</exception>
    protected override void QuickRemoveCore()
    {
        throw new NotSupportedException( ExceptionResources.SchemaQuickRemovalIsUnsupported );
    }

    /// <summary>
    /// Quick-removes all <see cref="Objects"/>.
    /// </summary>
    protected void QuickRemoveObjects()
    {
        foreach ( var obj in Objects )
            QuickRemove( obj );

        ClearCollection( Objects );
    }

    /// <summary>
    /// Resets <see cref="SqlRecordSetInfo"/> of all tables and views.
    /// </summary>
    /// <remarks>
    /// See <see cref="SqlBuilderApi.ResetInfo(SqlTableBuilder)"/> and <see cref="SqlBuilderApi.ResetInfo(SqlViewBuilder)"/>
    /// for more information.
    /// </remarks>
    protected void ResetAllTableAndViewInfoCache()
    {
        foreach ( var obj in Objects )
        {
            switch ( obj.Type )
            {
                case SqlObjectType.Table:
                    ResetInfo( ReinterpretCast.To<SqlTableBuilder>( obj ) );
                    break;

                case SqlObjectType.View:
                    ResetInfo( ReinterpretCast.To<SqlViewBuilder>( obj ) );
                    break;
            }
        }
    }

    ISqlSchemaBuilder ISqlSchemaBuilder.SetName(string name)
    {
        return SetName( name );
    }
}
