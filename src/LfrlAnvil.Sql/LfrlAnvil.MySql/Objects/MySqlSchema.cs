using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Memory;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.MySql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlSchema : MySqlObject, ISqlSchema
{
    private MySqlSchema(MySqlDatabase database, MySqlSchemaBuilder builder)
        : base( builder )
    {
        Database = database;
        Objects = new MySqlObjectCollection( this, builder.Objects.Count );
    }

    public MySqlObjectCollection Objects { get; }
    public override MySqlDatabase Database { get; }
    public override string FullName => Name;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MySqlSchema Create(
        MySqlDatabase database,
        MySqlSchemaBuilder builder,
        RentedMemorySequence<MySqlObjectBuilder> tables)
    {
        var result = new MySqlSchema( database, builder );
        result.Objects.Populate( builder.Objects, tables );
        return result;
    }

    ISqlObjectCollection ISqlSchema.Objects => Objects;
}
