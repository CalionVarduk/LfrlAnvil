using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Internal;
using ExceptionResources = LfrlAnvil.Sql.Exceptions.ExceptionResources;

namespace LfrlAnvil.Sqlite.Objects.Builders;

public sealed class SqliteDatabaseChangeTracker : SqlDatabaseChangeTracker
{
    private Dictionary<ulong, SqliteTableBuilder>? _modifiedTables;

    internal SqliteDatabaseChangeTracker()
    {
        _modifiedTables = null;
    }

    public new SqliteDatabaseBuilder Database => ReinterpretCast.To<SqliteDatabaseBuilder>( base.Database );
    public IReadOnlyCollection<SqliteTableBuilder> ModifiedTables => (_modifiedTables?.Values).EmptyIfNull();

    protected override void AddNameChange(SqlObjectBuilder activeObject, SqlObjectBuilder target, string originalValue)
    {
        if ( target.Type != SqlObjectType.Schema )
        {
            base.AddNameChange( activeObject, target, originalValue );
            return;
        }

        CompletePendingChanges();

        var schema = ReinterpretCast.To<SqliteSchemaBuilder>( target );
        if ( schema.Objects.Count > 0 )
            AddRenameSchemaActions( schema, originalValue );
    }

    protected override void AddIsRemovedChange(SqlObjectBuilder activeObject, SqlObjectBuilder target)
    {
        if ( target.Type != SqlObjectType.Schema )
        {
            base.AddIsRemovedChange( activeObject, target );
            return;
        }

        if ( ! target.IsRemoved )
            return;

        CompletePendingChanges();
        AddRemoveSchemaActions( ReinterpretCast.To<SqliteSchemaBuilder>( target ) );
    }

    protected override void CompletePendingCreateObjectChanges(SqlObjectBuilder obj)
    {
        switch ( obj.Type )
        {
            case SqlObjectType.Table:
                AddCreateTableAction( ReinterpretCast.To<SqliteTableBuilder>( obj ) );
                return;
            case SqlObjectType.View:
                AddCreateViewAction( ReinterpretCast.To<SqliteViewBuilder>( obj ) );
                return;
        }

        Assume.Unreachable();
    }

    protected override void CompletePendingRemoveObjectChanges(SqlObjectBuilder obj)
    {
        switch ( obj.Type )
        {
            case SqlObjectType.Table:
                AddRemoveTableAction( ReinterpretCast.To<SqliteTableBuilder>( obj ) );
                return;
            case SqlObjectType.View:
                AddRemoveViewAction( ReinterpretCast.To<SqliteViewBuilder>( obj ) );
                return;
        }

        Assume.Unreachable();
    }

    protected override void CompletePendingAlterObjectChanges(SqlObjectBuilder obj, SqlDatabaseChangeAggregator changeAggregator)
    {
        var aggregator = ReinterpretCast.To<SqliteDatabaseChangeAggregator>( changeAggregator );
        switch ( obj.Type )
        {
            case SqlObjectType.Table:
                AddAlterTableActions( ReinterpretCast.To<SqliteTableBuilder>( obj ), aggregator );
                return;
            case SqlObjectType.View:
                AddAlterViewActions( ReinterpretCast.To<SqliteViewBuilder>( obj ), aggregator );
                return;
        }

        Assume.Unreachable();
    }

    [Pure]
    protected override SqlDatabaseChangeAggregator CreateAlterObjectChangeAggregator()
    {
        return new SqliteDatabaseChangeAggregator( this );
    }

    internal new void SetModeAndAttach(SqlDatabaseCreateMode mode)
    {
        base.SetModeAndAttach( mode );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void ClearModifiedTables()
    {
        _modifiedTables?.Clear();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void MarkTableAsModified(SqliteTableBuilder table)
    {
        _modifiedTables ??= new Dictionary<ulong, SqliteTableBuilder>();
        _modifiedTables.TryAdd( table.Id, table );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void RemoveTableFromModified(SqliteTableBuilder table)
    {
        _modifiedTables?.Remove( table.Id );
    }

    private void AddRenameSchemaActions(SqlSchemaBuilder schema, string originalName)
    {
        var interpreter = CreateNodeInterpreter();
        var viewsToRename = new List<SqliteViewBuilder>();
        var objectsToReconstruct = SqlDatabaseObjectsSet<SqlObjectBuilder>.Create();

        foreach ( var obj in schema.Objects )
        {
            switch ( obj.Type )
            {
                case SqlObjectType.Table:
                {
                    var table = ReinterpretCast.To<SqliteTableBuilder>( obj );
                    AddRenameTableAction( table, SqlRecordSetInfo.Create( originalName, table.Name ), interpreter );

                    objectsToReconstruct.Add( obj );
                    foreach ( var reference in obj.ReferencingObjects )
                    {
                        if ( reference.Source.Property is not null )
                            continue;

                        switch ( reference.Source.Object.Type )
                        {
                            case SqlObjectType.ForeignKey:
                            {
                                var originTable = ReinterpretCast.To<SqliteForeignKeyBuilder>( reference.Source.Object ).OriginIndex.Table;
                                objectsToReconstruct.Add( originTable );
                                break;
                            }
                            case SqlObjectType.View:
                            {
                                var view = ReinterpretCast.To<SqliteViewBuilder>( reference.Source.Object );
                                if ( ! ReferenceEquals( schema, view.Schema ) )
                                    objectsToReconstruct.Add( view );

                                break;
                            }
                        }
                    }

                    break;
                }
                case SqlObjectType.View:
                {
                    viewsToRename.Add( ReinterpretCast.To<SqliteViewBuilder>( obj ) );
                    foreach ( var reference in obj.ReferencingObjects )
                    {
                        if ( reference.Source.Object.Type == SqlObjectType.View && reference.Source.Property is null )
                        {
                            var view = ReinterpretCast.To<SqliteViewBuilder>( reference.Source.Object );
                            if ( ! ReferenceEquals( schema, view.Schema ) )
                                objectsToReconstruct.Add( view );
                        }
                    }

                    break;
                }
            }
        }

        viewsToRename.Sort( static (a, b) => a.Id.CompareTo( b.Id ) );
        foreach ( var view in viewsToRename )
            AddReconstructViewAction( view, SqlRecordSetInfo.Create( originalName, view.Name ), interpreter );

        foreach ( var obj in objectsToReconstruct )
        {
            switch ( obj.Type )
            {
                case SqlObjectType.Table:
                {
                    var table = ReinterpretCast.To<SqliteTableBuilder>( obj );
                    var originalSchemaName = ReferenceEquals( schema, table.Schema ) ? originalName : table.Schema.Name;
                    AddReconstructTableAction( table, originalSchemaName, interpreter );
                    break;
                }
                case SqlObjectType.View:
                {
                    var view = ReinterpretCast.To<SqliteViewBuilder>( obj );
                    AddReconstructViewAction( view, view.Info, interpreter );
                    break;
                }
            }
        }
    }

    private void AddRemoveSchemaActions(SqlSchemaBuilder schema)
    {
        foreach ( var obj in schema.Objects )
        {
            switch ( obj.Type )
            {
                case SqlObjectType.Table:
                {
                    AddRemoveTableAction( ReinterpretCast.To<SqliteTableBuilder>( obj ) );
                    break;
                }
                case SqlObjectType.View:
                {
                    AddRemoveViewAction( ReinterpretCast.To<SqliteViewBuilder>( obj ) );
                    break;
                }
            }
        }
    }

    private void AddCreateTableAction(SqliteTableBuilder table)
    {
        ValidateTable( table );

        var interpreter = CreateNodeInterpreter();
        interpreter.VisitCreateTable( table.ToCreateNode() );
        AppendSqlCommandEnd( interpreter );

        AppendCreateAllIndexes( interpreter, table.Constraints );
        var sql = interpreter.Context.Sql.ToString();
        interpreter.Context.Clear();

        MarkTableAsModified( table );
        AddSqlAction( sql );
    }

    private void AddCreateViewAction(SqliteViewBuilder view)
    {
        var interpreter = CreateNodeInterpreter();
        interpreter.VisitCreateView( view.ToCreateNode() );
        var sql = GetSqlAndClearContext( interpreter );
        AddSqlAction( sql );
    }

    private void AddRemoveTableAction(SqliteTableBuilder table)
    {
        var interpreter = CreateNodeInterpreter();
        var name = this.GetOriginalValue( table, SqlObjectChangeDescriptor.Name ).GetValueOrDefault( table.Name );
        interpreter.VisitDropTable( SqlNode.DropTable( SqlRecordSetInfo.Create( table.Schema.Name, name ) ) );
        var sql = GetSqlAndClearContext( interpreter );

        RemoveTableFromModified( table );
        AddSqlAction( sql );
    }

    private void AddRemoveViewAction(SqliteViewBuilder view)
    {
        var interpreter = CreateNodeInterpreter();
        var name = this.GetOriginalValue( view, SqlObjectChangeDescriptor.Name ).GetValueOrDefault( view.Name );
        interpreter.VisitDropView( SqlNode.DropView( SqlRecordSetInfo.Create( view.Schema.Name, name ) ) );
        var sql = GetSqlAndClearContext( interpreter );
        AddSqlAction( sql );
    }

    private void AddAlterTableActions(SqliteTableBuilder table, SqliteDatabaseChangeAggregator changeAggregator)
    {
        if ( ! changeAggregator.HasChanged )
            return;

        ValidateTable( table );
        changeAggregator.UpdateReconstructionRequirement( table );

        var interpreter = CreateNodeInterpreter();
        if ( changeAggregator.IsRenamed )
        {
            var name = changeAggregator.OriginalName.GetValueOrDefault( table.Name );
            AddRenameTableAction( table, SqlRecordSetInfo.Create( table.Schema.Name, name ), interpreter );

            HashSet<ulong>? reconstructedTables = null;
            foreach ( var reference in table.ReferencingObjects )
            {
                if ( reference.Source.Property is not null )
                    continue;

                switch ( reference.Source.Object.Type )
                {
                    case SqlObjectType.ForeignKey:
                    {
                        var referencingTable = ReinterpretCast.To<SqliteForeignKeyBuilder>( reference.Source.Object ).Table;

                        reconstructedTables ??= new HashSet<ulong>();
                        if ( reconstructedTables.Add( referencingTable.Id ) )
                            AddReconstructTableAction( referencingTable, referencingTable.Schema.Name, interpreter );

                        break;
                    }
                    case SqlObjectType.View:
                    {
                        var view = ReinterpretCast.To<SqliteViewBuilder>( reference.Source.Object );
                        AddReconstructViewAction( view, view.Info, interpreter );
                        break;
                    }
                }
            }
        }

        if ( changeAggregator.RequiresReconstruction )
            AddReconstructTableAction( table, changeAggregator, interpreter );
        else
            TryAddSimpleAlterTableAction( table, changeAggregator, interpreter );
    }

    private void AddReconstructTableAction(
        SqliteTableBuilder table,
        SqliteDatabaseChangeAggregator changeAggregator,
        SqlNodeInterpreter interpreter)
    {
        changeAggregator.PrepareColumnsForReconstruction( table );
        AppendDropRemovedIndexes( interpreter, table, changeAggregator.RemovedIndexes );

        foreach ( var constraint in table.Constraints )
        {
            if ( constraint.Type != SqlObjectType.Index )
                continue;

            var index = ReinterpretCast.To<SqliteIndexBuilder>( constraint );
            if ( index.IsVirtual || changeAggregator.CreatedIndexes.Contains( index ) )
                continue;

            var name = this.GetOriginalValue( index, SqlObjectChangeDescriptor.Name ).GetValueOrDefault( index.Name );
            interpreter.VisitDropIndex( SqlNode.DropIndex( table.Info, SqlSchemaObjectName.Create( index.Table.Schema.Name, name ) ) );
            AppendSqlCommandEnd( interpreter );
        }

        foreach ( var column in changeAggregator.CreatedColumns )
        {
            if ( column.DefaultValue is null && ! column.IsNullable )
                column.UpdateDefaultValueBasedOnDataType();
        }

        var temporaryTable = AppendTemporaryTable( interpreter, table );

        var i = 0;
        var selections = new SqlSelectNode[table.Columns.Count];
        var tableColumns = new SqlDataFieldNode[table.Columns.Count];

        foreach ( var column in table.Columns )
        {
            tableColumns[i] = temporaryTable[column.Name];

            if ( changeAggregator.CreatedColumns.Contains( column ) )
            {
                selections[i++] = (column.DefaultValue ?? SqlNode.Null()).As( column.Name );
                continue;
            }

            var modification = changeAggregator.ModifiedColumns.TryGetSource( column );
            if ( modification is null )
            {
                selections[i++] = column.Node;
                continue;
            }

            var originalName = this.GetOriginalValue( modification.Value.Source, SqlObjectChangeDescriptor.Name )
                .GetValueOrDefault( modification.Value.Source.Name );

            var originalIsNullable = this.GetOriginalValue( modification.Value.Source, SqlObjectChangeDescriptor.IsNullable )
                .GetValueOrDefault( modification.Value.Source.IsNullable );

            var originalDataType = ReinterpretCast.To<SqliteDataType>(
                this.GetOriginalValue( modification.Value.Source, SqlObjectChangeDescriptor.DataType )
                    .GetValueOrDefault( modification.Value.Source.TypeDefinition.DataType ) );

            SqlExpressionNode oldDataField = originalDataType == ReinterpretCast.To<SqliteDataType>( column.TypeDefinition.DataType )
                ? column.Node
                : table.Node.GetRawField(
                        originalName,
                        TypeNullability.Create(
                            Database.TypeDefinitions.GetByDataType( originalDataType ).RuntimeType,
                            originalIsNullable ) )
                    .CastTo( column.TypeDefinition );

            if ( originalIsNullable && ! column.IsNullable )
                oldDataField = oldDataField.Coalesce( column.DefaultValue ?? column.TypeDefinition.DefaultValue );

            selections[i++] = oldDataField.As( column.Name );
        }

        AppendTableReconstructionWrapUp( interpreter, table, temporaryTable, selections, tableColumns );
        AppendCreateAllIndexes( interpreter, table.Constraints );
        var sql = interpreter.Context.Sql.ToString();
        interpreter.Context.Clear();

        MarkTableAsModified( table );
        AddSqlAction( sql );
    }

    private void TryAddSimpleAlterTableAction(
        SqliteTableBuilder table,
        SqliteDatabaseChangeAggregator changeAggregator,
        SqlNodeInterpreter interpreter)
    {
        changeAggregator.PopulateColumnsByOriginalName();
        AppendDropRemovedIndexes( interpreter, table, changeAggregator.RemovedIndexes );

        foreach ( var (name, _) in changeAggregator.RemovedColumns )
        {
            interpreter.VisitDropColumn( SqlNode.DropColumn( table.Info, name ) );
            AppendSqlCommandEnd( interpreter );
        }

        foreach ( var rename in changeAggregator.ColumnRenames.Values )
        {
            if ( ! rename.IsPending )
                continue;

            ref var renameRef = ref CollectionsMarshal.GetValueRefOrNullRef( changeAggregator.ColumnRenames, rename.Column.Id );
            Assume.False( Unsafe.IsNullRef( ref renameRef ) );

            renameRef = renameRef.Complete();
            HandleColumnRename( interpreter, table, changeAggregator, ref renameRef );
        }

        foreach ( var ix in changeAggregator.CreatedIndexes )
        {
            interpreter.VisitCreateIndex( ix.ToCreateNode() );
            AppendSqlCommandEnd( interpreter );
        }

        if ( interpreter.Context.Sql.Length == 0 )
            return;

        var sql = interpreter.Context.Sql.ToString();
        interpreter.Context.Clear();
        AddSqlAction( sql );

        static void HandleColumnRename(
            SqlNodeInterpreter interpreter,
            SqliteTableBuilder table,
            SqliteDatabaseChangeAggregator aggregator,
            ref SqliteColumnRename rename)
        {
            Assume.False( rename.IsPending );

            var conflictingColumn = aggregator.ColumnsByOriginalName.TryGetObject( rename.Name );
            if ( conflictingColumn is not null )
            {
                ref var conflictingRename = ref CollectionsMarshal.GetValueRefOrNullRef( aggregator.ColumnRenames, conflictingColumn.Id );
                Assume.False( Unsafe.IsNullRef( ref conflictingRename ) );

                if ( conflictingRename.IsPending )
                {
                    conflictingRename = conflictingRename.Complete();
                    HandleColumnRename( interpreter, table, aggregator, ref conflictingRename );
                }
                else
                {
                    var tempName = CreateTemporaryName( conflictingRename.OriginalName );
                    interpreter.VisitRenameColumn( SqlNode.RenameColumn( table.Info, conflictingRename.OriginalName, tempName ) );
                    AppendSqlCommandEnd( interpreter );

                    aggregator.ColumnsByOriginalName.Remove( conflictingRename.OriginalName );
                    aggregator.ColumnsByOriginalName.Add( tempName, conflictingRename.Column );
                    conflictingRename = SqliteColumnRename.CreateTemporary( conflictingRename, tempName );
                }
            }

            interpreter.VisitRenameColumn( SqlNode.RenameColumn( table.Info, rename.OriginalName, rename.Name ) );
            AppendSqlCommandEnd( interpreter );
            aggregator.ColumnsByOriginalName.Remove( rename.OriginalName );
            aggregator.ColumnsByOriginalName.Add( rename.Name, rename.Column );
        }
    }

    private void AddAlterViewActions(SqliteViewBuilder view, SqliteDatabaseChangeAggregator changeAggregator)
    {
        if ( ! changeAggregator.IsRenamed )
            return;

        var interpreter = CreateNodeInterpreter();
        var originalInfo = SqlRecordSetInfo.Create( view.Schema.Name, changeAggregator.OriginalName.GetValueOrDefault( view.Name ) );
        AddReconstructViewAction( view, originalInfo, interpreter );

        foreach ( var reference in view.ReferencingObjects )
        {
            if ( reference.Source.Object.Type != SqlObjectType.View || reference.Source.Property is not null )
                continue;

            var refView = ReinterpretCast.To<SqliteViewBuilder>( reference.Source.Object );
            AddReconstructViewAction( refView, refView.Info, interpreter );
        }
    }

    private void AddReconstructTableAction(SqliteTableBuilder table, string originalSchemaName, SqlNodeInterpreter interpreter)
    {
        foreach ( var constraint in table.Constraints )
        {
            if ( constraint.Type != SqlObjectType.Index )
                continue;

            var index = ReinterpretCast.To<SqliteIndexBuilder>( constraint );
            if ( index.IsVirtual )
                continue;

            var name = this.GetOriginalValue( index, SqlObjectChangeDescriptor.Name ).GetValueOrDefault( index.Name );
            interpreter.VisitDropIndex( SqlNode.DropIndex( table.Info, SqlSchemaObjectName.Create( originalSchemaName, name ) ) );
            AppendSqlCommandEnd( interpreter );
        }

        var temporaryTable = AppendTemporaryTable( interpreter, table );

        var i = 0;
        var selections = new SqlSelectNode[table.Columns.Count];
        var tableColumns = new SqlDataFieldNode[table.Columns.Count];

        foreach ( var column in table.Columns )
        {
            tableColumns[i] = temporaryTable[column.Name];
            selections[i++] = column.Node;
        }

        AppendTableReconstructionWrapUp( interpreter, table, temporaryTable, selections, tableColumns );
        AppendCreateAllIndexes( interpreter, table.Constraints );
        var sql = interpreter.Context.Sql.ToString();
        interpreter.Context.Clear();

        MarkTableAsModified( table );
        AddSqlAction( sql );
    }

    private void AddRenameTableAction(SqliteTableBuilder table, SqlRecordSetInfo originalInfo, SqlNodeInterpreter interpreter)
    {
        interpreter.VisitRenameTable( SqlNode.RenameTable( originalInfo, table.Info.Name ) );
        var sql = GetSqlAndClearContext( interpreter );
        AddSqlAction( sql );
    }

    private void AddReconstructViewAction(SqliteViewBuilder view, SqlRecordSetInfo originalInfo, SqlNodeInterpreter interpreter)
    {
        interpreter.VisitDropView( SqlNode.DropView( originalInfo ) );
        AppendSqlCommandEnd( interpreter );
        interpreter.VisitCreateView( view.ToCreateNode() );
        var sql = GetSqlAndClearContext( interpreter );
        AddSqlAction( sql );
    }

    private static void AppendCreateAllIndexes(SqlNodeInterpreter interpreter, SqliteConstraintBuilderCollection constraints)
    {
        foreach ( var constraint in constraints )
        {
            if ( constraint.Type != SqlObjectType.Index )
                continue;

            var ix = ReinterpretCast.To<SqliteIndexBuilder>( constraint );
            if ( ix.IsVirtual )
                continue;

            interpreter.VisitCreateIndex( ix.ToCreateNode() );
            AppendSqlCommandEnd( interpreter );
        }
    }

    private static void AppendDropRemovedIndexes(
        SqlNodeInterpreter interpreter,
        SqliteTableBuilder table,
        SqlDatabaseNamedObjectsSet<SqliteIndexBuilder> indexes)
    {
        foreach ( var (name, _) in indexes )
        {
            interpreter.VisitDropIndex( SqlNode.DropIndex( table.Info, SqlSchemaObjectName.Create( table.Schema.Name, name ) ) );
            AppendSqlCommandEnd( interpreter );
        }
    }

    private static SqlNewTableNode AppendTemporaryTable(SqlNodeInterpreter interpreter, SqliteTableBuilder table)
    {
        var info = SqlRecordSetInfo.Create( CreateTemporaryName( SqliteHelpers.GetFullName( table.Schema.Name, table.Name ) ) );
        var createTemporaryTable = table.ToCreateNode( customInfo: info );
        interpreter.VisitCreateTable( createTemporaryTable );
        AppendSqlCommandEnd( interpreter );
        return createTemporaryTable.RecordSet;
    }

    private static void AppendTableReconstructionWrapUp(
        SqlNodeInterpreter interpreter,
        SqliteTableBuilder table,
        SqlNewTableNode temporaryTable,
        SqlSelectNode[] insertIntoSelections,
        SqlDataFieldNode[] insertIntoFields)
    {
        var insertInto = table.Node.ToDataSource().Select( insertIntoSelections ).ToInsertInto( temporaryTable, insertIntoFields );
        interpreter.VisitInsertInto( insertInto );
        AppendSqlCommandEnd( interpreter );
        interpreter.VisitDropTable( SqlNode.DropTable( table.Info ) );
        AppendSqlCommandEnd( interpreter );
        interpreter.VisitRenameTable( SqlNode.RenameTable( temporaryTable.Info, table.Info.Name ) );
        AppendSqlCommandEnd( interpreter );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void ValidateTable(SqliteTableBuilder table)
    {
        if ( table.Constraints.TryGetPrimaryKey() is null )
            ExceptionThrower.Throw(
                SqlHelpers.CreateObjectBuilderException( table.Database, ExceptionResources.PrimaryKeyIsMissing( table ) ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static string CreateTemporaryName(string name)
    {
        return $"__{name}__{Guid.NewGuid():N}__";
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static StringBuilder AppendSqlCommandEnd(SqlNodeInterpreter interpreter)
    {
        return interpreter.Context.Sql.AppendSemicolon().AppendLine();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static string GetSqlAndClearContext(SqlNodeInterpreter interpreter)
    {
        var result = AppendSqlCommandEnd( interpreter ).ToString();
        interpreter.Context.Clear();
        return result;
    }
}
