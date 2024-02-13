using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Sql.Internal;
using ExceptionResources = LfrlAnvil.Sql.Exceptions.ExceptionResources;

namespace LfrlAnvil.Sql.Objects.Builders;

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

    public ulong Id { get; }
    public SqlDatabaseBuilder Database { get; }
    public SqlObjectType Type { get; }
    public string Name { get; private set; }
    public bool IsRemoved { get; private set; }

    public SqlObjectBuilderReferenceCollection<SqlObjectBuilder> ReferencingObjects =>
        new SqlObjectBuilderReferenceCollection<SqlObjectBuilder>( this );

    public virtual bool CanRemove => ReferencedTargets is null || ReferencedTargets.Count == 0;

    ISqlDatabaseBuilder ISqlObjectBuilder.Database => Database;
    SqlObjectBuilderReferenceCollection<ISqlObjectBuilder> ISqlObjectBuilder.ReferencingObjects => ReferencingObjects;

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

    public void Remove()
    {
        if ( IsRemoved )
            return;

        BeforeRemove();
        Assume.Equals( CanRemove, true );
        IsRemoved = true;
        AfterRemove();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void ThrowIfRemoved()
    {
        if ( IsRemoved )
            ExceptionThrower.Throw( SqlHelpers.CreateObjectBuilderException( Database, ExceptionResources.ObjectHasBeenRemoved( this ) ) );
    }

    public void ThrowIfReferenced()
    {
        if ( ReferencedTargets is null || ReferencedTargets.Count == 0 )
            return;

        var errors = Chain<string>.Empty;
        foreach ( var reference in ReferencingObjects )
            errors = errors.Extend( ExceptionResources.ReferenceExists( reference ) );

        throw SqlHelpers.CreateObjectBuilderException( Database, errors );
    }

    protected virtual SqlPropertyChange<string> BeforeNameChange(string newValue)
    {
        return Name == newValue ? SqlPropertyChange.Cancel<string>() : newValue;
    }

    protected abstract void AfterNameChange(string originalValue);

    protected virtual void BeforeRemove()
    {
        ThrowIfReferenced();
    }

    protected abstract void AfterRemove();

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
