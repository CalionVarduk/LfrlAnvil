using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using NpgsqlTypes;

namespace LfrlAnvil.PostgreSql;

public sealed class PostgreSqlDataType : ISqlDataType, IEquatable<PostgreSqlDataType>, IComparable<PostgreSqlDataType>, IComparable
{
    public static readonly PostgreSqlDataType Boolean = new PostgreSqlDataType( "BOOLEAN", NpgsqlDbType.Boolean, DbType.Boolean );
    public static readonly PostgreSqlDataType Int2 = new PostgreSqlDataType( "INT2", NpgsqlDbType.Smallint, DbType.Int16 );
    public static readonly PostgreSqlDataType Int4 = new PostgreSqlDataType( "INT4", NpgsqlDbType.Integer, DbType.Int32 );
    public static readonly PostgreSqlDataType Int8 = new PostgreSqlDataType( "INT8", NpgsqlDbType.Bigint, DbType.Int64 );
    public static readonly PostgreSqlDataType Float4 = new PostgreSqlDataType( "FLOAT4", NpgsqlDbType.Real, DbType.Single );
    public static readonly PostgreSqlDataType Float8 = new PostgreSqlDataType( "FLOAT8", NpgsqlDbType.Double, DbType.Double );

    public static readonly PostgreSqlDataType Decimal = new PostgreSqlDataType(
        "DECIMAL(29, 10)",
        NpgsqlDbType.Numeric,
        DbType.Decimal,
        new[] { 29, 10 },
        new[]
        {
            new SqlDataTypeParameter( "PRECISION", Bounds.Create( 0, 1000 ) ),
            new SqlDataTypeParameter( "SCALE", Bounds.Create( -1000, 1000 ) )
        } );

    public static readonly PostgreSqlDataType VarChar = new PostgreSqlDataType(
        "VARCHAR",
        NpgsqlDbType.Varchar,
        DbType.String,
        new[] { 10485760 },
        new[] { new SqlDataTypeParameter( "MAX_LENGTH", Bounds.Create( 0, 10485760 ) ) } );

    public static readonly PostgreSqlDataType VarBit = new PostgreSqlDataType(
        "VARBIT",
        NpgsqlDbType.Varbit,
        DbType.Binary,
        new[] { 10485760 },
        new[] { new SqlDataTypeParameter( "MAX_LENGTH", Bounds.Create( 0, 10485760 ) ) } );

    public static readonly PostgreSqlDataType Uuid = new PostgreSqlDataType( "UUID", NpgsqlDbType.Uuid, DbType.Guid );
    public static readonly PostgreSqlDataType Bytea = new PostgreSqlDataType( "BYTEA", NpgsqlDbType.Bytea, DbType.Binary );
    public static readonly PostgreSqlDataType Date = new PostgreSqlDataType( "DATE", NpgsqlDbType.Date, DbType.Date );
    public static readonly PostgreSqlDataType Time = new PostgreSqlDataType( "TIME", NpgsqlDbType.Time, DbType.Time );
    public static readonly PostgreSqlDataType Timestamp = new PostgreSqlDataType( "TIMESTAMP", NpgsqlDbType.Timestamp, DbType.DateTime2 );

    public static readonly PostgreSqlDataType TimestampTz = new PostgreSqlDataType(
        "TIMESTAMPTZ",
        NpgsqlDbType.TimestampTz,
        DbType.DateTime );

    private readonly int[] _parameters;
    private readonly SqlDataTypeParameter[] _parameterDefinitions;

    private PostgreSqlDataType(string name, NpgsqlDbType value, DbType dbType)
        : this( name, value, dbType, Array.Empty<int>(), Array.Empty<SqlDataTypeParameter>() ) { }

    private PostgreSqlDataType(
        string name,
        NpgsqlDbType value,
        DbType dbType,
        int[] parameters,
        SqlDataTypeParameter[] parameterDefinitions)
    {
        Assume.ContainsExactly( parameters, parameterDefinitions.Length );
        Assume.True( parameters.Zip( parameterDefinitions ).All( x => x.Second.Bounds.Contains( x.First ) ) );
        Name = name;
        Value = value;
        DbType = dbType;
        _parameters = parameters;
        _parameterDefinitions = parameterDefinitions;
    }

    public string Name { get; }
    public NpgsqlDbType Value { get; }
    public DbType DbType { get; }
    public SqlDialect Dialect => PostgreSqlDialect.Instance;
    public ReadOnlySpan<int> Parameters => _parameters;
    public ReadOnlySpan<SqlDataTypeParameter> ParameterDefinitions => _parameterDefinitions;

    [Pure]
    public static PostgreSqlDataType CreateDecimal(int precision, int scale)
    {
        if ( precision == Decimal._parameters[0] && scale == Decimal._parameters[1] )
            return Decimal;

        var parameters = Decimal._parameterDefinitions;
        var errors = Chain<Pair<SqlDataTypeParameter, int>>.Empty;

        if ( ! parameters[0].Bounds.Contains( precision ) )
            errors = errors.Extend( Pair.Create( parameters[0], precision ) );

        if ( ! parameters[1].Bounds.Contains( scale ) )
            errors = errors.Extend( Pair.Create( parameters[1], scale ) );

        if ( errors.Count > 0 )
            throw new SqlDataTypeException( errors );

        return new PostgreSqlDataType(
            $"DECIMAL({precision}, {scale})",
            Decimal.Value,
            Decimal.DbType,
            new[] { precision, scale },
            parameters );
    }

    [Pure]
    public static PostgreSqlDataType CreateVarChar(int maxLength)
    {
        var parameters = VarChar._parameterDefinitions;
        if ( parameters[0].Bounds.Contains( maxLength ) )
            return new PostgreSqlDataType( $"VARCHAR({maxLength})", VarChar.Value, VarChar.DbType, new[] { maxLength }, parameters );

        if ( maxLength < parameters[0].Bounds.Min )
            throw new SqlDataTypeException( Chain.Create( Pair.Create( parameters[0], maxLength ) ) );

        return VarChar;
    }

    [Pure]
    public static PostgreSqlDataType CreateVarBit(int maxLength)
    {
        var parameters = VarBit._parameterDefinitions;
        if ( parameters[0].Bounds.Contains( maxLength ) )
            return new PostgreSqlDataType( $"VARBIT({maxLength})", VarBit.Value, VarBit.DbType, new[] { maxLength }, parameters );

        if ( maxLength < parameters[0].Bounds.Min )
            throw new SqlDataTypeException( Chain.Create( Pair.Create( parameters[0], maxLength ) ) );

        return VarBit;
    }

    [Pure]
    public static PostgreSqlDataType Custom(string name, NpgsqlDbType value, DbType dbType)
    {
        return new PostgreSqlDataType( name, value, dbType );
    }

    [Pure]
    public override string ToString()
    {
        return $"'{Name}' ({Value})";
    }

    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine( Value, Name );
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is PostgreSqlDataType t && Equals( t );
    }

    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is PostgreSqlDataType t ? CompareTo( t ) : 1;
    }

    [Pure]
    public bool Equals(PostgreSqlDataType? other)
    {
        return EqualsBase( other );
    }

    [Pure]
    public int CompareTo(PostgreSqlDataType? other)
    {
        return CompareToBase( other );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator NpgsqlDbType(PostgreSqlDataType t)
    {
        return t.Value;
    }

    [Pure]
    public static bool operator ==(PostgreSqlDataType? a, PostgreSqlDataType? b)
    {
        return a?.EqualsBase( b ) ?? b is null;
    }

    [Pure]
    public static bool operator !=(PostgreSqlDataType? a, PostgreSqlDataType? b)
    {
        return ! (a?.EqualsBase( b ) ?? b is null);
    }

    [Pure]
    public static bool operator >=(PostgreSqlDataType? a, PostgreSqlDataType? b)
    {
        return a is null ? b is null : a.CompareToBase( b ) >= 0;
    }

    [Pure]
    public static bool operator <(PostgreSqlDataType? a, PostgreSqlDataType? b)
    {
        return a is null ? b is not null : a.CompareToBase( b ) < 0;
    }

    [Pure]
    public static bool operator <=(PostgreSqlDataType? a, PostgreSqlDataType? b)
    {
        return a is null || a.CompareToBase( b ) <= 0;
    }

    [Pure]
    public static bool operator >(PostgreSqlDataType? a, PostgreSqlDataType? b)
    {
        return a is not null && a.CompareToBase( b ) > 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool EqualsBase(PostgreSqlDataType? other)
    {
        return other is not null && Value == other.Value && string.Equals( Name, other.Name, StringComparison.Ordinal );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private int CompareToBase(PostgreSqlDataType? other)
    {
        if ( other is null )
            return 1;

        var cmp = Comparer<NpgsqlDbType>.Default.Compare( Value, other.Value );
        return cmp != 0 ? cmp : string.Compare( Name, other.Name, StringComparison.Ordinal );
    }
}
