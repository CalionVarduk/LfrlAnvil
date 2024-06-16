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
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using MySqlConnector;

namespace LfrlAnvil.MySql;

/// <inheritdoc cref="ISqlDataType" />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlDataType : ISqlDataType, IEquatable<MySqlDataType>, IComparable<MySqlDataType>, IComparable
{
    /// <summary>
    /// Represents the <b>BOOL</b> type.
    /// </summary>
    public static readonly MySqlDataType Bool = new MySqlDataType( "BOOL", MySqlDbType.Bool, DbType.Boolean );

    /// <summary>
    /// Represents the <b>TINYINT</b> type.
    /// </summary>
    public static readonly MySqlDataType TinyInt = new MySqlDataType( "TINYINT", MySqlDbType.Byte, DbType.SByte );

    /// <summary>
    /// Represents the <b>TINYINT UNSIGNED</b> type.
    /// </summary>
    public static readonly MySqlDataType UnsignedTinyInt = new MySqlDataType( "TINYINT UNSIGNED", MySqlDbType.UByte, DbType.Byte );

    /// <summary>
    /// Represents the <b>SMALLINT</b> type.
    /// </summary>
    public static readonly MySqlDataType SmallInt = new MySqlDataType( "SMALLINT", MySqlDbType.Int16, DbType.Int16 );

    /// <summary>
    /// Represents the <b>SMALLINT UNSIGNED</b> type.
    /// </summary>
    public static readonly MySqlDataType UnsignedSmallInt = new MySqlDataType( "SMALLINT UNSIGNED", MySqlDbType.UInt16, DbType.UInt16 );

    /// <summary>
    /// Represents the <b>INT</b> type.
    /// </summary>
    public static readonly MySqlDataType Int = new MySqlDataType( "INT", MySqlDbType.Int32, DbType.Int32 );

    /// <summary>
    /// Represents the <b>INT UNSIGNED</b> type.
    /// </summary>
    public static readonly MySqlDataType UnsignedInt = new MySqlDataType( "INT UNSIGNED", MySqlDbType.UInt32, DbType.UInt32 );

    /// <summary>
    /// Represents the <b>BIGINT</b> type.
    /// </summary>
    public static readonly MySqlDataType BigInt = new MySqlDataType( "BIGINT", MySqlDbType.Int64, DbType.Int64 );

    /// <summary>
    /// Represents the <b>BIGINT UNSIGNED</b> type.
    /// </summary>
    public static readonly MySqlDataType UnsignedBigInt = new MySqlDataType( "BIGINT UNSIGNED", MySqlDbType.UInt64, DbType.UInt64 );

    /// <summary>
    /// Represents the <b>FLOAT</b> type.
    /// </summary>
    public static readonly MySqlDataType Float = new MySqlDataType( "FLOAT", MySqlDbType.Float, DbType.Single );

    /// <summary>
    /// Represents the <b>DOUBLE</b> type.
    /// </summary>
    public static readonly MySqlDataType Double = new MySqlDataType( "DOUBLE", MySqlDbType.Double, DbType.Double );

    /// <summary>
    /// Represents the <b>DECIMAL</b> type with default <b>29</b> scale and <b>10</b> precision.
    /// </summary>
    public static readonly MySqlDataType Decimal = new MySqlDataType(
        "DECIMAL(29, 10)",
        MySqlDbType.NewDecimal,
        DbType.Decimal,
        new[] { 29, 10 },
        new[]
        {
            new SqlDataTypeParameter( "PRECISION", Bounds.Create( 0, 65 ) ), new SqlDataTypeParameter( "SCALE", Bounds.Create( 0, 30 ) )
        } );

    /// <summary>
    /// Represents the <b>CHAR</b> type with default <b>255</b> length.
    /// </summary>
    public static readonly MySqlDataType Char = new MySqlDataType(
        "CHAR(255)",
        MySqlDbType.String,
        DbType.StringFixedLength,
        new[] { 255 },
        new[] { new SqlDataTypeParameter( "LENGTH", Bounds.Create( 0, 255 ) ) } );

    /// <summary>
    /// Represents the <b>BINARY</b> type with default <b>255</b> length.
    /// </summary>
    public static readonly MySqlDataType Binary = new MySqlDataType(
        "BINARY(255)",
        MySqlDbType.Binary,
        DbType.Binary,
        new[] { 255 },
        new[] { new SqlDataTypeParameter( "LENGTH", Bounds.Create( 0, 255 ) ) } );

    /// <summary>
    /// Represents the <b>VARCHAR</b> type with default <b>65535</b> maximum length.
    /// </summary>
    public static readonly MySqlDataType VarChar = new MySqlDataType(
        "VARCHAR(65535)",
        MySqlDbType.VarChar,
        DbType.String,
        new[] { 65535 },
        new[] { new SqlDataTypeParameter( "MAX_LENGTH", Bounds.Create( 0, 65535 ) ) } );

    /// <summary>
    /// Represents the <b>VARBINARY</b> type with default <b>65535</b> maximum length.
    /// </summary>
    public static readonly MySqlDataType VarBinary = new MySqlDataType(
        "VARBINARY(65535)",
        MySqlDbType.VarBinary,
        DbType.Binary,
        new[] { 65535 },
        new[] { new SqlDataTypeParameter( "MAX_LENGTH", Bounds.Create( 0, 65535 ) ) } );

    /// <summary>
    /// Represents the <b>LONGBLOB</b> type.
    /// </summary>
    public static readonly MySqlDataType Blob = new MySqlDataType( "LONGBLOB", MySqlDbType.LongBlob, DbType.Binary );

    /// <summary>
    /// Represents the <b>LONGTEXT</b> type.
    /// </summary>
    public static readonly MySqlDataType Text = new MySqlDataType( "LONGTEXT", MySqlDbType.LongText, DbType.String );

    /// <summary>
    /// Represents the <b>DATE</b> type.
    /// </summary>
    public static readonly MySqlDataType Date = new MySqlDataType( "DATE", MySqlDbType.Newdate, DbType.Date );

    /// <summary>
    /// Represents the <b>TIME(6)</b> type.
    /// </summary>
    public static readonly MySqlDataType Time = new MySqlDataType( "TIME(6)", MySqlDbType.Time, DbType.Time );

    /// <summary>
    /// Represents the <b>DATETIME(6)</b> type.
    /// </summary>
    public static readonly MySqlDataType DateTime = new MySqlDataType( "DATETIME(6)", MySqlDbType.DateTime, DbType.DateTime );

    private readonly int[] _parameters;
    private readonly SqlDataTypeParameter[] _parameterDefinitions;

    private MySqlDataType(string name, MySqlDbType value, DbType dbType)
        : this( name, value, dbType, Array.Empty<int>(), Array.Empty<SqlDataTypeParameter>() ) { }

    private MySqlDataType(string name, MySqlDbType value, DbType dbType, int[] parameters, SqlDataTypeParameter[] parameterDefinitions)
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
    public MySqlDbType Value { get; }

    /// <inheritdoc />
    public DbType DbType { get; }

    /// <inheritdoc />
    public SqlDialect Dialect => MySqlDialect.Instance;

    /// <inheritdoc />
    public ReadOnlySpan<int> Parameters => _parameters;

    /// <inheritdoc />
    public ReadOnlySpan<SqlDataTypeParameter> ParameterDefinitions => _parameterDefinitions;

    /// <summary>
    /// Creates a new <b>DECIMAL</b> type.
    /// </summary>
    /// <param name="precision">Desired precision.</param>
    /// <param name="scale">Desired scale.</param>
    /// <returns><see cref="MySqlDataType"/> instance that represents the desired <b>DECIMAL</b> type.</returns>
    /// <exception cref="SqlDataTypeException">
    /// When <paramref name="precision"/> is not in <b>[0, 65]</b> range or when <paramref name="scale"/> is not in <b>[0, 30]</b> range.
    /// </exception>
    [Pure]
    public static MySqlDataType CreateDecimal(int precision, int scale)
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

        return new MySqlDataType( $"DECIMAL({precision}, {scale})", Decimal.Value, Decimal.DbType, new[] { precision, scale }, parameters );
    }

    /// <summary>
    /// Creates a new <b>CHAR</b> type.
    /// </summary>
    /// <param name="length">Desired length.</param>
    /// <returns><see cref="MySqlDataType"/> instance that represents the desired <b>CHAR</b> type.</returns>
    /// <exception cref="SqlDataTypeException">When <paramref name="length"/> is not in <b>[0, 255]</b> range.</exception>
    [Pure]
    public static MySqlDataType CreateChar(int length)
    {
        if ( length == Char._parameters[0] )
            return Char;

        var parameters = Char._parameterDefinitions;
        if ( ! parameters[0].Bounds.Contains( length ) )
            throw new SqlDataTypeException( Chain.Create( Pair.Create( parameters[0], length ) ) );

        return new MySqlDataType( $"CHAR({length})", Char.Value, Char.DbType, new[] { length }, parameters );
    }

    /// <summary>
    /// Creates a new <b>BINARY</b> type.
    /// </summary>
    /// <param name="length">Desired length.</param>
    /// <returns><see cref="MySqlDataType"/> instance that represents the desired <b>BINARY</b> type.</returns>
    /// <exception cref="SqlDataTypeException">When <paramref name="length"/> is not in <b>[0, 255]</b> range.</exception>
    [Pure]
    public static MySqlDataType CreateBinary(int length)
    {
        if ( length == Binary._parameters[0] )
            return Binary;

        var parameters = Binary._parameterDefinitions;
        if ( ! parameters[0].Bounds.Contains( length ) )
            throw new SqlDataTypeException( Chain.Create( Pair.Create( parameters[0], length ) ) );

        return new MySqlDataType( $"BINARY({length})", Binary.Value, Binary.DbType, new[] { length }, parameters );
    }

    /// <summary>
    /// Creates a new <b>VARCHAR</b> type.
    /// </summary>
    /// <param name="maxLength">Desired maximum length.</param>
    /// <returns><see cref="MySqlDataType"/> instance that represents the desired <b>VARCHAR</b> type.</returns>
    /// <exception cref="SqlDataTypeException">When <paramref name="maxLength"/> is not in <b>[0, 65535]</b> range.</exception>
    [Pure]
    public static MySqlDataType CreateVarChar(int maxLength)
    {
        if ( maxLength == VarChar._parameters[0] )
            return VarChar;

        var parameters = VarChar._parameterDefinitions;
        if ( ! parameters[0].Bounds.Contains( maxLength ) )
            throw new SqlDataTypeException( Chain.Create( Pair.Create( parameters[0], maxLength ) ) );

        return new MySqlDataType( $"VARCHAR({maxLength})", VarChar.Value, VarChar.DbType, new[] { maxLength }, parameters );
    }

    /// <summary>
    /// Creates a new <b>VARBINARY</b> type.
    /// </summary>
    /// <param name="maxLength">Desired maximum length.</param>
    /// <returns><see cref="MySqlDataType"/> instance that represents the desired <b>VARBINARY</b> type.</returns>
    /// <exception cref="SqlDataTypeException">When <paramref name="maxLength"/> is not in <b>[0, 65535]</b> range.</exception>
    [Pure]
    public static MySqlDataType CreateVarBinary(int maxLength)
    {
        if ( maxLength == VarBinary._parameters[0] )
            return VarBinary;

        var parameters = VarBinary._parameterDefinitions;
        if ( ! parameters[0].Bounds.Contains( maxLength ) )
            throw new SqlDataTypeException( Chain.Create( Pair.Create( parameters[0], maxLength ) ) );

        return new MySqlDataType( $"VARBINARY({maxLength})", VarBinary.Value, VarBinary.DbType, new[] { maxLength }, parameters );
    }

    /// <summary>
    /// Creates a new custom type.
    /// </summary>
    /// <param name="name">DB name of this data type.</param>
    /// <param name="value">Underlying value.</param>
    /// <param name="dbType"><see cref="System.Data.DbType"/> of this data type.</param>
    /// <returns>New <see cref="MySqlDataType"/> instance.</returns>
    [Pure]
    public static MySqlDataType Custom(string name, MySqlDbType value, DbType dbType)
    {
        return new MySqlDataType( name, value, dbType );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="MySqlDataType"/> instance.
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
        return obj is MySqlDataType t && Equals( t );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is MySqlDataType t
            ? CompareTo( t )
            : throw new ArgumentException( LfrlAnvil.Exceptions.ExceptionResources.InvalidType, nameof( obj ) );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(MySqlDataType? other)
    {
        return EqualsBase( other );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(MySqlDataType? other)
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
    public static implicit operator MySqlDbType(MySqlDataType t)
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
    public static bool operator ==(MySqlDataType? a, MySqlDataType? b)
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
    public static bool operator !=(MySqlDataType? a, MySqlDataType? b)
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
    public static bool operator >=(MySqlDataType? a, MySqlDataType? b)
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
    public static bool operator <(MySqlDataType? a, MySqlDataType? b)
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
    public static bool operator <=(MySqlDataType? a, MySqlDataType? b)
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
    public static bool operator >(MySqlDataType? a, MySqlDataType? b)
    {
        return a is not null && a.CompareToBase( b ) > 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool EqualsBase(MySqlDataType? other)
    {
        return other is not null && Value == other.Value && string.Equals( Name, other.Name, StringComparison.Ordinal );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private int CompareToBase(MySqlDataType? other)
    {
        if ( other is null )
            return 1;

        var cmp = Comparer<MySqlDbType>.Default.Compare( Value, other.Value );
        return cmp != 0 ? cmp : string.Compare( Name, other.Name, StringComparison.Ordinal );
    }
}
