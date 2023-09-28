using System;
using System.Data;
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;
using LfrlAnvil.Internal;
using LfrlAnvil.Sql;

namespace LfrlAnvil.Sqlite;

public abstract class SqliteColumnTypeDefinition : ISqlColumnTypeDefinition
{
    internal SqliteColumnTypeDefinition(SqliteDataType dbType, object defaultValue)
    {
        DbType = dbType;
        DefaultValue = defaultValue;
    }

    public SqliteDataType DbType { get; }
    public object DefaultValue { get; }
    public abstract Type RuntimeType { get; }
    ISqlDataType ISqlColumnTypeDefinition.DbType => DbType;

    [Pure]
    public sealed override string ToString()
    {
        return $"{RuntimeType.GetDebugString()} <=> {DbType}, {nameof( DefaultValue )}: [{DefaultValue}]";
    }

    [Pure]
    public abstract string? TryToDbLiteral(object value);

    public abstract bool TrySetParameter(IDbDataParameter parameter, object value);
}

public abstract class SqliteColumnTypeDefinition<T> : SqliteColumnTypeDefinition, ISqlColumnTypeDefinition<T>
    where T : notnull
{
    protected SqliteColumnTypeDefinition(SqliteDataType dbType, T defaultValue)
        : base( dbType, defaultValue ) { }

    public new T DefaultValue => (T)base.DefaultValue;
    public sealed override Type RuntimeType => typeof( T );

    [Pure]
    public SqliteColumnTypeDefinition<TTarget> Extend<TTarget>(Func<TTarget, T> mapper, TTarget defaultValue)
        where TTarget : notnull
    {
        return new SqliteColumnTypeDefinitionLambda<TTarget, T>( this, defaultValue, mapper );
    }

    [Pure]
    public abstract string ToDbLiteral(T value);

    public abstract void SetParameter(IDbDataParameter parameter, T value);

    [Pure]
    public override string? TryToDbLiteral(object value)
    {
        return value is T t ? ToDbLiteral( t ) : null;
    }

    public override bool TrySetParameter(IDbDataParameter parameter, object value)
    {
        if ( value is not T t )
            return false;

        SetParameter( parameter, t );
        return true;
    }

    [Pure]
    ISqlColumnTypeDefinition<TTarget> ISqlColumnTypeDefinition<T>.Extend<TTarget>(Func<TTarget, T> mapper, TTarget defaultValue)
    {
        return Extend( mapper, defaultValue );
    }
}

public abstract class SqliteColumnTypeDefinition<T, TBase> : SqliteColumnTypeDefinition<T>
    where T : notnull
    where TBase : notnull
{
    protected SqliteColumnTypeDefinition(SqliteColumnTypeDefinition<TBase> @base, T defaultValue)
        : base( @base.DbType, defaultValue )
    {
        Base = @base;
    }

    protected SqliteColumnTypeDefinition<TBase> Base { get; }

    [Pure]
    public sealed override string ToDbLiteral(T value)
    {
        var baseValue = MapToBaseType( value );
        return Base.ToDbLiteral( baseValue );
    }

    public sealed override void SetParameter(IDbDataParameter parameter, T value)
    {
        var baseValue = MapToBaseType( value );
        Base.SetParameter( parameter, baseValue );
    }

    [Pure]
    protected abstract TBase MapToBaseType(T value);
}

internal sealed class SqliteColumnTypeDefinitionLambda<T, TBase> : SqliteColumnTypeDefinition<T, TBase>
    where T : notnull
    where TBase : notnull
{
    private readonly Func<T, TBase> _mapper;

    internal SqliteColumnTypeDefinitionLambda(SqliteColumnTypeDefinition<TBase> @base, T defaultValue, Func<T, TBase> mapper)
        : base( @base, defaultValue )
    {
        _mapper = mapper;
    }

    [Pure]
    protected override TBase MapToBaseType(T value)
    {
        return _mapper( value );
    }
}

internal sealed class SqliteColumnTypeEnumDefinition<TEnum, T> : SqliteColumnTypeDefinition<TEnum, T>
    where TEnum : struct, Enum
    where T : unmanaged, IEquatable<T>
{
    internal SqliteColumnTypeEnumDefinition(SqliteColumnTypeDefinition<T> @base)
        : base( @base, FindDefaultValue() ) { }

    [Pure]
    protected override T MapToBaseType(TEnum value)
    {
        return (T)(object)value;
    }

    [Pure]
    private static TEnum FindDefaultValue()
    {
        var values = Enum.GetValues<TEnum>();
        foreach ( var value in values )
        {
            if ( Generic<TEnum>.IsDefault( value ) )
                return value;
        }

        return values.Length > 0 ? values[0] : default;
    }
}
