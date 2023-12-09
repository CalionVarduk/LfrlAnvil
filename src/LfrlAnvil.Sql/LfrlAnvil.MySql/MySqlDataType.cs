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

public sealed class MySqlDataType : ISqlDataType, IEquatable<MySqlDataType>, IComparable<MySqlDataType>, IComparable
{
    public static readonly MySqlDataType Bool = new MySqlDataType( "BOOL", MySqlDbType.Bool, DbType.Boolean );
    public static readonly MySqlDataType TinyInt = new MySqlDataType( "TINYINT", MySqlDbType.Byte, DbType.SByte );
    public static readonly MySqlDataType UnsignedTinyInt = new MySqlDataType( "TINYINT UNSIGNED", MySqlDbType.UByte, DbType.Byte );
    public static readonly MySqlDataType SmallInt = new MySqlDataType( "SMALLINT", MySqlDbType.Int16, DbType.Int16 );
    public static readonly MySqlDataType UnsignedSmallInt = new MySqlDataType( "SMALLINT UNSIGNED", MySqlDbType.UInt16, DbType.UInt16 );
    public static readonly MySqlDataType Int = new MySqlDataType( "INT", MySqlDbType.Int32, DbType.Int32 );
    public static readonly MySqlDataType UnsignedInt = new MySqlDataType( "INT UNSIGNED", MySqlDbType.UInt32, DbType.UInt32 );
    public static readonly MySqlDataType BigInt = new MySqlDataType( "BIGINT", MySqlDbType.Int64, DbType.Int64 );
    public static readonly MySqlDataType UnsignedBigInt = new MySqlDataType( "BIGINT UNSIGNED", MySqlDbType.UInt64, DbType.UInt64 );
    public static readonly MySqlDataType Float = new MySqlDataType( "FLOAT", MySqlDbType.Float, DbType.Single );
    public static readonly MySqlDataType Double = new MySqlDataType( "DOUBLE", MySqlDbType.Double, DbType.Double );

    public static readonly MySqlDataType Decimal = new MySqlDataType(
        "DECIMAL(29, 10)",
        MySqlDbType.NewDecimal,
        DbType.Decimal,
        new[] { 29, 10 },
        new[]
        {
            new SqlDataTypeParameter( "PRECISION", Bounds.Create( 0, 65 ) ),
            new SqlDataTypeParameter( "SCALE", Bounds.Create( 0, 30 ) )
        } );

    public static readonly MySqlDataType Char = new MySqlDataType(
        "CHAR(255)",
        MySqlDbType.String,
        DbType.StringFixedLength,
        new[] { 255 },
        new[] { new SqlDataTypeParameter( "LENGTH", Bounds.Create( 0, 255 ) ) } );

    public static readonly MySqlDataType Binary = new MySqlDataType(
        "BINARY(255)",
        MySqlDbType.Binary,
        DbType.Binary,
        new[] { 255 },
        new[] { new SqlDataTypeParameter( "LENGTH", Bounds.Create( 0, 255 ) ) } );

    public static readonly MySqlDataType VarChar = new MySqlDataType(
        "VARCHAR(65535)",
        MySqlDbType.VarChar,
        DbType.String,
        new[] { 65535 },
        new[] { new SqlDataTypeParameter( "MAX_LENGTH", Bounds.Create( 0, 65535 ) ) } );

    public static readonly MySqlDataType VarBinary = new MySqlDataType(
        "VARBINARY(65535)",
        MySqlDbType.VarBinary,
        DbType.Binary,
        new[] { 65535 },
        new[] { new SqlDataTypeParameter( "MAX_LENGTH", Bounds.Create( 0, 65535 ) ) } );

    public static readonly MySqlDataType Blob = new MySqlDataType( "LONGBLOB", MySqlDbType.LongBlob, DbType.Binary );
    public static readonly MySqlDataType Text = new MySqlDataType( "LONGTEXT", MySqlDbType.LongText, DbType.String );
    public static readonly MySqlDataType Date = new MySqlDataType( "DATE", MySqlDbType.Newdate, DbType.Date );
    public static readonly MySqlDataType Time = new MySqlDataType( "TIME", MySqlDbType.Time, DbType.Time );
    public static readonly MySqlDataType DateTime = new MySqlDataType( "DATETIME", MySqlDbType.DateTime, DbType.DateTime );

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

    public string Name { get; }
    public MySqlDbType Value { get; }
    public DbType DbType { get; }
    public SqlDialect Dialect => MySqlDialect.Instance;
    public ReadOnlySpan<int> Parameters => _parameters;
    public ReadOnlySpan<SqlDataTypeParameter> ParameterDefinitions => _parameterDefinitions;

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

    [Pure]
    public static MySqlDataType Custom(string name, MySqlDbType value, DbType dbType)
    {
        return new MySqlDataType( name, value, dbType );
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
        return obj is MySqlDataType t && Equals( t );
    }

    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is MySqlDataType t ? CompareTo( t ) : 1;
    }

    [Pure]
    public bool Equals(MySqlDataType? other)
    {
        return EqualsBase( other );
    }

    [Pure]
    public int CompareTo(MySqlDataType? other)
    {
        return CompareToBase( other );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator MySqlDbType(MySqlDataType t)
    {
        return t.Value;
    }

    [Pure]
    public static bool operator ==(MySqlDataType? a, MySqlDataType? b)
    {
        return a?.EqualsBase( b ) ?? b is null;
    }

    [Pure]
    public static bool operator !=(MySqlDataType? a, MySqlDataType? b)
    {
        return ! (a?.EqualsBase( b ) ?? b is null);
    }

    [Pure]
    public static bool operator >=(MySqlDataType? a, MySqlDataType? b)
    {
        return a is null ? b is null : a.CompareToBase( b ) >= 0;
    }

    [Pure]
    public static bool operator <(MySqlDataType? a, MySqlDataType? b)
    {
        return a is null ? b is not null : a.CompareToBase( b ) < 0;
    }

    [Pure]
    public static bool operator <=(MySqlDataType? a, MySqlDataType? b)
    {
        return a is null || a.CompareToBase( b ) <= 0;
    }

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
