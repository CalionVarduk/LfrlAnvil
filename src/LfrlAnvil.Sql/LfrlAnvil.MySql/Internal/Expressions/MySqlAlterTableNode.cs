using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.MySql.Internal.Expressions;

public sealed class MySqlAlterTableNode : SqlNodeBase, ISqlStatementNode
{
    public MySqlAlterTableNode(
        SqlRecordSetInfo info,
        string[] oldColumns,
        string[] oldForeignKeys,
        string[] oldChecks,
        KeyValuePair<string, string>[] renamedIndexes,
        KeyValuePair<string, SqlColumnDefinitionNode>[] changedColumns,
        SqlColumnDefinitionNode[] newColumns,
        SqlPrimaryKeyDefinitionNode? newPrimaryKey,
        SqlForeignKeyDefinitionNode[] newForeignKeys,
        SqlCheckDefinitionNode[] newChecks)
    {
        Info = info;
        OldColumns = oldColumns;
        OldForeignKeys = oldForeignKeys;
        OldChecks = oldChecks;
        RenamedIndexes = renamedIndexes;
        ChangedColumns = changedColumns;
        NewColumns = newColumns;
        NewPrimaryKey = newPrimaryKey;
        NewForeignKeys = newForeignKeys;
        NewChecks = newChecks;
    }

    public SqlRecordSetInfo Info { get; }
    public ReadOnlyMemory<string> OldColumns { get; }
    public ReadOnlyMemory<string> OldForeignKeys { get; }
    public ReadOnlyMemory<string> OldChecks { get; }
    public ReadOnlyMemory<KeyValuePair<string, string>> RenamedIndexes { get; }
    public ReadOnlyMemory<KeyValuePair<string, SqlColumnDefinitionNode>> ChangedColumns { get; }
    public ReadOnlyMemory<SqlColumnDefinitionNode> NewColumns { get; }
    public SqlPrimaryKeyDefinitionNode? NewPrimaryKey { get; }
    public ReadOnlyMemory<SqlForeignKeyDefinitionNode> NewForeignKeys { get; }
    public ReadOnlyMemory<SqlCheckDefinitionNode> NewChecks { get; }
    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;

    [Pure]
    public static MySqlAlterTableNode CreateDropForeignKeys(SqlRecordSetInfo info, params string[] foreignKeyNames)
    {
        return new MySqlAlterTableNode(
            info: info,
            oldColumns: Array.Empty<string>(),
            oldForeignKeys: foreignKeyNames,
            oldChecks: Array.Empty<string>(),
            renamedIndexes: Array.Empty<KeyValuePair<string, string>>(),
            changedColumns: Array.Empty<KeyValuePair<string, SqlColumnDefinitionNode>>(),
            newColumns: Array.Empty<SqlColumnDefinitionNode>(),
            newPrimaryKey: null,
            newForeignKeys: Array.Empty<SqlForeignKeyDefinitionNode>(),
            newChecks: Array.Empty<SqlCheckDefinitionNode>() );
    }

    [Pure]
    public static MySqlAlterTableNode CreateAddForeignKeys(SqlRecordSetInfo info, params SqlForeignKeyDefinitionNode[] foreignKeys)
    {
        return new MySqlAlterTableNode(
            info: info,
            oldColumns: Array.Empty<string>(),
            oldForeignKeys: Array.Empty<string>(),
            oldChecks: Array.Empty<string>(),
            renamedIndexes: Array.Empty<KeyValuePair<string, string>>(),
            changedColumns: Array.Empty<KeyValuePair<string, SqlColumnDefinitionNode>>(),
            newColumns: Array.Empty<SqlColumnDefinitionNode>(),
            newPrimaryKey: null,
            newForeignKeys: foreignKeys,
            newChecks: Array.Empty<SqlCheckDefinitionNode>() );
    }

    protected override void ToString(SqlNodeDebugInterpreter interpreter)
    {
        interpreter.Context.Sql.Append( "ALTER" ).AppendSpace().Append( "TABLE" ).AppendSpace();
        interpreter.AppendDelimitedRecordSetInfo( Info );

        using ( interpreter.Context.TempIndentIncrease() )
        {
            foreach ( var name in OldForeignKeys )
            {
                interpreter.Context.AppendIndent();
                interpreter.Context.Sql.Append( "DROP" ).AppendSpace().Append( "FOREIGN" ).AppendSpace().Append( "KEY" ).AppendSpace();
                interpreter.AppendDelimitedName( name );
            }

            foreach ( var name in OldChecks )
            {
                interpreter.Context.AppendIndent();
                interpreter.Context.Sql.Append( "DROP" ).AppendSpace().Append( "CHECK" ).AppendSpace();
                interpreter.AppendDelimitedName( name );
            }

            foreach ( var (oldName, newName) in RenamedIndexes )
            {
                interpreter.Context.AppendIndent();
                interpreter.Context.Sql.Append( "RENAME" ).AppendSpace().Append( "INDEX" ).AppendSpace();
                interpreter.AppendDelimitedName( oldName );
                interpreter.Context.Sql.AppendSpace().Append( "TO" ).AppendSpace();
                interpreter.AppendDelimitedName( newName );
            }

            foreach ( var name in OldColumns )
            {
                interpreter.Context.AppendIndent();
                interpreter.Context.Sql.Append( "DROP" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
                interpreter.AppendDelimitedName( name );
            }

            foreach ( var (oldName, column) in ChangedColumns )
            {
                interpreter.Context.AppendIndent().Append( "CHANGE" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
                interpreter.AppendDelimitedName( oldName );
                interpreter.Context.Sql.AppendSpace().Append( "TO" ).AppendSpace();
                interpreter.VisitColumnDefinition( column );
            }

            foreach ( var column in NewColumns )
            {
                interpreter.Context.AppendIndent().Append( "ADD" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
                interpreter.VisitColumnDefinition( column );
            }

            if ( NewPrimaryKey is not null )
            {
                interpreter.Context.AppendIndent().Append( "SET" ).AppendSpace();
                interpreter.VisitPrimaryKeyDefinition( NewPrimaryKey );
            }

            foreach ( var check in NewChecks )
            {
                interpreter.Context.AppendIndent().Append( "ADD" ).AppendSpace();
                interpreter.VisitCheckDefinition( check );
            }

            foreach ( var foreignKey in NewForeignKeys )
            {
                interpreter.Context.AppendIndent().Append( "ADD" ).AppendSpace();
                interpreter.VisitForeignKeyDefinition( foreignKey );
            }
        }
    }
}
