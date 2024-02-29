﻿using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;
using ExceptionResources = LfrlAnvil.Sql.Exceptions.ExceptionResources;

namespace LfrlAnvil.MySql.Objects.Builders;

public abstract class MySqlObjectBuilder : ISqlObjectBuilder
{
    protected MySqlObjectBuilder(ulong id, string name, SqlObjectType type)
    {
        Assume.IsDefined( type );
        IsRemoved = false;
        Id = id;
        Name = name;
        Type = type;
    }

    public ulong Id { get; }
    public SqlObjectType Type { get; }
    public string Name { get; protected set; }
    public bool IsRemoved { get; protected set; }
    public abstract MySqlDatabaseBuilder Database { get; }
    public virtual bool CanRemove => true;

    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;
    SqlObjectBuilderReferenceCollection<ISqlObjectBuilder> ISqlObjectBuilder.ReferencingObjects => throw new NotImplementedException();

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {Name}";
    }

    [Pure]
    public sealed override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public void Remove()
    {
        if ( IsRemoved )
            return;

        AssertRemoval();
        IsRemoved = true;
        RemoveCore();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected internal void EnsureNotRemoved()
    {
        if ( IsRemoved )
            ExceptionThrower.Throw( new MySqlObjectBuilderException( ExceptionResources.ObjectHasBeenRemoved( this ) ) );
    }

    protected virtual void AssertRemoval() { }
    protected abstract void RemoveCore();
    protected abstract void SetNameCore(string name);

    ISqlObjectBuilder ISqlObjectBuilder.SetName(string name)
    {
        EnsureNotRemoved();
        SetNameCore( name );
        return this;
    }
}