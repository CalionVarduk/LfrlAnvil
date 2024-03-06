using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sqlite.Internal.TypeDefinitions;

namespace LfrlAnvil.Sqlite;

public sealed class SqliteColumnTypeDefinitionProvider : SqlColumnTypeDefinitionProvider
{
    private readonly ReadOnlyArray<SqlColumnTypeDefinition> _defaultDefinitions;

    internal SqliteColumnTypeDefinitionProvider(SqliteColumnTypeDefinitionProviderBuilder builder)
        : base( builder )
    {
        var defaultAny = new SqliteColumnTypeDefinitionObject( this, builder.DefaultBlob );
        _defaultDefinitions = new SqlColumnTypeDefinition[]
        {
            defaultAny,
            builder.DefaultInteger,
            builder.DefaultReal,
            builder.DefaultText,
            builder.DefaultBlob
        };

        TryAddDefinition( defaultAny );
    }

    [Pure]
    public override IReadOnlyCollection<SqlColumnTypeDefinition> GetDataTypeDefinitions()
    {
        return _defaultDefinitions.GetUnderlyingArray();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlColumnTypeDefinition GetByDataType(SqliteDataType type)
    {
        var index = (int)type.Value;
        return _defaultDefinitions[index];
    }

    [Pure]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public override SqlColumnTypeDefinition GetByDataType(ISqlDataType type)
    {
        return GetByDataType( SqlHelpers.CastOrThrow<SqliteDataType>( Dialect, type ) );
    }

    [Pure]
    protected override SqlColumnTypeDefinition<TEnum> CreateEnumTypeDefinition<TEnum, TUnderlying>(
        SqlColumnTypeDefinition<TUnderlying> underlyingTypeDefinition)
    {
        return new SqliteColumnTypeEnumDefinition<TEnum, TUnderlying>(
            ReinterpretCast.To<SqliteColumnTypeDefinition<TUnderlying>>( underlyingTypeDefinition ) );
    }
}
