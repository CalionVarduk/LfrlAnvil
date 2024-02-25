using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.TestExtensions.Sql.Mocks.System;

public sealed class DbParameterMock : DbParameter
{
    private bool _isNullable;
    private DbType _dbType;
    private string? _name;

    public DbParameterMock()
    {
        Id = -1;
    }

    public DbParameterMock(DbCommandMock command)
    {
        Command = command;
        Id = Command.GetNextParameterId();
    }

    public int Id { get; }
    public DbCommandMock? Command { get; }

    public override DbType DbType
    {
        get => _dbType;
        set
        {
            Command?.AddAudit( this, $"{nameof( DbType )} = {value}" );
            _dbType = value;
        }
    }

    public override ParameterDirection Direction { get; set; }

    public override bool IsNullable
    {
        get => _isNullable;
        set
        {
            Command?.AddAudit( this, $"{nameof( IsNullable )} = {value}" );
            _isNullable = value;
        }
    }

    [AllowNull]
    public override string ParameterName
    {
        get => _name!;
        set
        {
            Command?.AddAudit( this, $"{nameof( ParameterName )} = {value}" );
            _name = value;
        }
    }

    [AllowNull]
    public override string SourceColumn { get; set; }

    public override bool SourceColumnNullMapping { get; set; }
    public override object? Value { get; set; }
    public override int Size { get; set; }

    public override void ResetDbType()
    {
        IsNullable = false;
        DbType = DbType.Object;
    }

    [Pure]
    public override string ToString()
    {
        return $"DbParameter[{Id}]";
    }
}
