﻿using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.MySql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlView : MySqlObject, ISqlView
{
    private SqlRecordSetInfo? _info;
    private SqlViewNode? _recordSet;

    internal MySqlView(MySqlSchema schema, MySqlViewBuilder builder)
        : base( builder )
    {
        Schema = schema;
        FullName = builder.FullName;
        DataFields = new MySqlViewDataFieldCollection( this, builder.Source );
        _info = builder.GetCachedInfo();
        _recordSet = null;
    }

    public MySqlSchema Schema { get; }
    public MySqlViewDataFieldCollection DataFields { get; }
    public override string FullName { get; }
    public SqlRecordSetInfo Info => _info ??= SqlRecordSetInfo.Create( Schema.Name, Name );
    public SqlViewNode RecordSet => _recordSet ??= SqlNode.View( this );
    public override MySqlDatabase Database => Schema.Database;

    ISqlSchema ISqlView.Schema => Schema;
    ISqlViewDataFieldCollection ISqlView.DataFields => DataFields;
}
