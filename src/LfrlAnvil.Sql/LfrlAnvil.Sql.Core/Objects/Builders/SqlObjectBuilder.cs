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

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Internal;
using ExceptionResources = LfrlAnvil.Sql.Exceptions.ExceptionResources;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <inheritdoc cref="ISqlObjectBuilder" />
public abstract class SqlObjectBuilder : SqlBuilderApi, ISqlObjectBuilder
{
    internal Dictionary<SqlObjectBuilderReferenceSource<SqlObjectBuilder>, SqlObjectBuilder>? ReferencedTargets;

    internal SqlObjectBuilder(SqlDatabaseBuilder database, SqlObjectType type, string name)
    {
        Assume.IsDefined( type );
        Id = database.GetNextId();
        Database = database;
        Type = type;
        Name = name;
        IsRemoved = false;
        ReferencedTargets = null;
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectBuilder"/> instance.
    /// </summary>
    /// <param name="database">Database that this object belongs to.</param>
    /// <param name="name">Object's name.</param>
    protected SqlObjectBuilder(SqlDatabaseBuilder database, string name)
        : this( database, SqlObjectType.Unknown, name ) { }

    /// <summary>
    /// Unique identifier of this object builder within its <see cref="Database"/>.
    /// </summary>
    public ulong Id { get; }

    /// <inheritdoc cref="ISqlObjectBuilder.Database" />
    public SqlDatabaseBuilder Database { get; }

    /// <inheritdoc />
    public SqlObjectType Type { get; }

    /// <inheritdoc />
    public string Name { get; private set; }

    /// <inheritdoc />
    public bool IsRemoved { get; private set; }

    /// <inheritdoc cref="ISqlObjectBuilder.ReferencingObjects" />
    public SqlObjectBuilderReferenceCollection<SqlObjectBuilder> ReferencingObjects =>
        new SqlObjectBuilderReferenceCollection<SqlObjectBuilder>( this );

    /// <inheritdoc />
    public virtual bool CanRemove => ReferencedTargets is null || ReferencedTargets.Count == 0;

    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;
    SqlObjectBuilderReferenceCollection<ISqlObjectBuilder> ISqlObjectBuilder.ReferencingObjects => ReferencingObjects;

    /// <summary>
    /// Returns a string representation of this <see cref="SqlObjectBuilder"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {Name}";
    }

    /// <inheritdoc />
    [Pure]
    public sealed override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <inheritdoc cref="ISqlObjectBuilder.SetName(string)" />
    public SqlObjectBuilder SetName(string name)
    {
        ThrowIfRemoved();
        var change = BeforeNameChange( name );
        if ( change.IsCancelled )
            return this;

        var originalValue = Name;
        Name = change.NewValue;
        AfterNameChange( originalValue );
        return this;
    }

    /// <inheritdoc />
    public void Remove()
    {
        if ( IsRemoved )
            return;

        BeforeRemove();
        Assume.True( CanRemove );
        IsRemoved = true;
        AfterRemove();
    }

    /// <summary>
    /// Throws an exception when <see cref="IsRemoved"/> is equal to <b>true</b>.
    /// </summary>
    /// <exception cref="SqlObjectBuilderException">When <see cref="IsRemoved"/> is equal to <b>true</b>.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void ThrowIfRemoved()
    {
        if ( IsRemoved )
            ExceptionThrower.Throw( SqlHelpers.CreateObjectBuilderException( Database, ExceptionResources.ObjectHasBeenRemoved( this ) ) );
    }

    /// <summary>
    /// Throws an exception when <see cref="ReferencingObjects"/> is not empty.
    /// </summary>
    /// <exception cref="SqlObjectBuilderException">When <see cref="ReferencingObjects"/> is not empty.</exception>
    public void ThrowIfReferenced()
    {
        if ( ReferencedTargets is null || ReferencedTargets.Count == 0 )
            return;

        var errors = Chain<string>.Empty;
        foreach ( var reference in ReferencingObjects )
            errors = errors.Extend( ExceptionResources.ReferenceExists( reference ) );

        throw SqlHelpers.CreateObjectBuilderException( Database, errors );
    }

    /// <summary>
    /// Callback invoked just before <see cref="Name"/> change is processed.
    /// </summary>
    /// <param name="newValue">Value to set.</param>
    /// <returns><see cref="SqlPropertyChange{T}"/> instance associated with <see cref="Name"/> change attempt.</returns>
    /// <exception cref="SqlObjectBuilderException">When <see cref="Name"/> of this object cannot be changed.</exception>
    protected virtual SqlPropertyChange<string> BeforeNameChange(string newValue)
    {
        return Name == newValue ? SqlPropertyChange.Cancel<string>() : newValue;
    }

    /// <summary>
    /// Callback invoked just after <see cref="Name"/> change has been processed.
    /// </summary>
    /// <param name="originalValue">Original value.</param>
    protected abstract void AfterNameChange(string originalValue);

    /// <summary>
    /// Callback invoked just before the removal is processed.
    /// </summary>
    /// <exception cref="SqlObjectBuilderException">When this object cannot be removed.</exception>
    protected virtual void BeforeRemove()
    {
        ThrowIfReferenced();
    }

    /// <summary>
    /// Callback invoked just after the removal has been processed.
    /// </summary>
    protected abstract void AfterRemove();

    /// <summary>
    /// Performs a quick removal of this object.
    /// </summary>
    /// <remarks>See <see cref="SqlBuilderApi.QuickRemove(SqlObjectBuilder)"/> for more information.</remarks>
    protected virtual void QuickRemoveCore()
    {
        ClearReferences( this );
    }

    internal void QuickRemove()
    {
        if ( IsRemoved )
            return;

        QuickRemoveCore();
        IsRemoved = true;
    }

    ISqlObjectBuilder ISqlObjectBuilder.SetName(string name)
    {
        return SetName( name );
    }
}
