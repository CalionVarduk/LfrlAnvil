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

/// <inheritdoc cref="ISqlDataType" />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlDataType : ISqlDataType, IEquatable<PostgreSqlDataType>, IComparable<PostgreSqlDataType>, IComparable
{
    /// <summary>
    /// Represents the <b>BOOLEAN</b> type.
    /// </summary>
    public static readonly PostgreSqlDataType Boolean = new PostgreSqlDataType( "BOOLEAN", NpgsqlDbType.Boolean, DbType.Boolean );

    /// <summary>
    /// Represents the <b>INT2</b> type.
    /// </summary>
    public static readonly PostgreSqlDataType Int2 = new PostgreSqlDataType( "INT2", NpgsqlDbType.Smallint, DbType.Int16 );

    /// <summary>
    /// Represents the <b>INT4</b> type.
    /// </summary>
    public static readonly PostgreSqlDataType Int4 = new PostgreSqlDataType( "INT4", NpgsqlDbType.Integer, DbType.Int32 );

    /// <summary>
    /// Represents the <b>INT8</b> type.
    /// </summary>
    public static readonly PostgreSqlDataType Int8 = new PostgreSqlDataType( "INT8", NpgsqlDbType.Bigint, DbType.Int64 );

    /// <summary>
    /// Represents the <b>FLOAT4</b> type.
    /// </summary>
    public static readonly PostgreSqlDataType Float4 = new PostgreSqlDataType( "FLOAT4", NpgsqlDbType.Real, DbType.Single );

    /// <summary>
    /// Represents the <b>FLOAT8</b> type.
    /// </summary>
    public static readonly PostgreSqlDataType Float8 = new PostgreSqlDataType( "FLOAT8", NpgsqlDbType.Double, DbType.Double );

    /// <summary>
    /// Represents the <b>DECIMAL</b> type with default <b>29</b> scale and <b>10</b> precision.
    /// </summary>
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

    /// <summary>
    /// Represents the <b>VARCHAR</b> type with default <b>10485760</b> maximum length.
    /// </summary>
    public static readonly PostgreSqlDataType VarChar = new PostgreSqlDataType(
        "VARCHAR",
        NpgsqlDbType.Varchar,
        DbType.String,
        new[] { 10485760 },
        new[] { new SqlDataTypeParameter( "MAX_LENGTH", Bounds.Create( 0, 10485760 ) ) } );

    /// <summary>
    /// Represents the <b>UUID</b> type.
    /// </summary>
    public static readonly PostgreSqlDataType Uuid = new PostgreSqlDataType( "UUID", NpgsqlDbType.Uuid, DbType.Guid );

    /// <summary>
    /// Represents the <b>BYTEA</b> type.
    /// </summary>
    public static readonly PostgreSqlDataType Bytea = new PostgreSqlDataType( "BYTEA", NpgsqlDbType.Bytea, DbType.Binary );

    /// <summary>
    /// Represents the <b>DATE</b> type.
    /// </summary>
    public static readonly PostgreSqlDataType Date = new PostgreSqlDataType( "DATE", NpgsqlDbType.Date, DbType.Date );

    /// <summary>
    /// Represents the <b>TIME</b> type.
    /// </summary>
    public static readonly PostgreSqlDataType Time = new PostgreSqlDataType( "TIME", NpgsqlDbType.Time, DbType.Time );

    /// <summary>
    /// Represents the <b>TIMESTAMP</b> type.
    /// </summary>
    public static readonly PostgreSqlDataType Timestamp = new PostgreSqlDataType( "TIMESTAMP", NpgsqlDbType.Timestamp, DbType.DateTime2 );

    /// <summary>
    /// Represents the <b>TIMESTAMPTZ</b> type.
    /// </summary>
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

    /// <inheritdoc />
    public string Name { get; }

    /// <summary>
    /// Underlying value.
    /// </summary>
    public NpgsqlDbType Value { get; }

    /// <inheritdoc />
    public DbType DbType { get; }

    /// <inheritdoc />
    public SqlDialect Dialect => PostgreSqlDialect.Instance;

    /// <inheritdoc />
    public ReadOnlySpan<int> Parameters => _parameters;

    /// <inheritdoc />
    public ReadOnlySpan<SqlDataTypeParameter> ParameterDefinitions => _parameterDefinitions;

    /// <summary>
    /// Creates a new <b>DECIMAL</b> type.
    /// </summary>
    /// <param name="precision">Desired precision.</param>
    /// <param name="scale">Desired scale.</param>
    /// <returns><see cref="PostgreSqlDataType"/> instance that represents the desired <b>DECIMAL</b> type.</returns>
    /// <exception cref="SqlDataTypeException">
    /// When <paramref name="precision"/> is not in <b>[0, 1000]</b> range
    /// or when <paramref name="scale"/> is not in <b>[-1000, 1000]</b> range.
    /// </exception>
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

    /// <summary>
    /// Creates a new <b>VARCHAR</b> type.
    /// </summary>
    /// <param name="maxLength">Desired maximum length.</param>
    /// <returns><see cref="PostgreSqlDataType"/> instance that represents the desired <b>VARCHAR</b> type.</returns>
    /// <exception cref="SqlDataTypeException">When <paramref name="maxLength"/> is less than <b>0</b>.</exception>
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

    /// <summary>
    /// Creates a new custom type.
    /// </summary>
    /// <param name="name">DB name of this data type.</param>
    /// <param name="value">Underlying value.</param>
    /// <param name="dbType"><see cref="System.Data.DbType"/> of this data type.</param>
    /// <returns>New <see cref="PostgreSqlDataType"/> instance.</returns>
    [Pure]
    public static PostgreSqlDataType Custom(string name, NpgsqlDbType value, DbType dbType)
    {
        return new PostgreSqlDataType( name, value, dbType );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="PostgreSqlDataType"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"'{Name}' ({Value})";
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine( Value, Name );
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is PostgreSqlDataType t && Equals( t );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is PostgreSqlDataType t
            ? CompareTo( t )
            : throw new ArgumentException( LfrlAnvil.Exceptions.ExceptionResources.InvalidType, nameof( obj ) );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(PostgreSqlDataType? other)
    {
        return EqualsBase( other );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(PostgreSqlDataType? other)
    {
        return CompareToBase( other );
    }

    /// <summary>
    /// Converts this instance to the underlying value type. Returns <see cref="Value"/> of <paramref name="t"/>.
    /// </summary>
    /// <param name="t">Data type to convert.</param>
    /// <returns><see cref="Value"/> of <paramref name="t"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator NpgsqlDbType(PostgreSqlDataType t)
    {
        return t.Value;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator ==(PostgreSqlDataType? a, PostgreSqlDataType? b)
    {
        return a?.EqualsBase( b ) ?? b is null;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is not equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are not equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator !=(PostgreSqlDataType? a, PostgreSqlDataType? b)
    {
        return ! (a?.EqualsBase( b ) ?? b is null);
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is greater than or equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is greater than or equal to <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator >=(PostgreSqlDataType? a, PostgreSqlDataType? b)
    {
        return a is null ? b is null : a.CompareToBase( b ) >= 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is less than <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is less than <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator <(PostgreSqlDataType? a, PostgreSqlDataType? b)
    {
        return a is null ? b is not null : a.CompareToBase( b ) < 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is less than or equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is less than or equal to <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator <=(PostgreSqlDataType? a, PostgreSqlDataType? b)
    {
        return a is null || a.CompareToBase( b ) <= 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is greater than <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is greater than <paramref name="b"/>, otherwise <b>false</b>.</returns>
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
