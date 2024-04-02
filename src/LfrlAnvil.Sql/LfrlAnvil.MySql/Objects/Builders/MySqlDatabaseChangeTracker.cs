using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using ExceptionResources = LfrlAnvil.Sql.Exceptions.ExceptionResources;

namespace LfrlAnvil.MySql.Objects.Builders;

public sealed class MySqlDatabaseChangeTracker : SqlDatabaseChangeTracker
{
    internal MySqlDatabaseChangeTracker() { }

    public new MySqlDatabaseBuilder Database => ReinterpretCast.To<MySqlDatabaseBuilder>( base.Database );

    protected override void AddNameChange(SqlObjectBuilder activeObject, SqlObjectBuilder target, string originalValue)
    {
        if ( target.Type != SqlObjectType.Schema )
        {
            base.AddNameChange( activeObject, target, originalValue );
            return;
        }

        CompletePendingChanges();

        var schema = ReinterpretCast.To<MySqlSchemaBuilder>( target );
        AddRenameSchemaActions( schema, originalValue );
    }

    protected override void AddIsRemovedChange(SqlObjectBuilder activeObject, SqlObjectBuilder target)
    {
        if ( target.Type != SqlObjectType.Schema )
        {
            base.AddIsRemovedChange( activeObject, target );
            return;
        }

        CompletePendingChanges();

        var schema = ReinterpretCast.To<MySqlSchemaBuilder>( target );
        if ( target.IsRemoved )
            AddRemoveSchemaAction( schema.Name );
        else if ( ! schema.Name.Equals( Database.CommonSchemaName, StringComparison.OrdinalIgnoreCase ) )
            AddCreateSchemaAction( schema.Name );
    }

    protected override void CompletePendingCreateObjectChanges(SqlObjectBuilder obj)
    {
        switch ( obj.Type )
        {
            case SqlObjectType.Table:
                AddCreateTableAction( ReinterpretCast.To<MySqlTableBuilder>( obj ) );
                return;
            case SqlObjectType.View:
                AddCreateViewAction( ReinterpretCast.To<MySqlViewBuilder>( obj ) );
                return;
        }

        Assume.Unreachable();
    }

    protected override void CompletePendingRemoveObjectChanges(SqlObjectBuilder obj)
    {
        switch ( obj.Type )
        {
            case SqlObjectType.Table:
                AddRemoveTableAction( ReinterpretCast.To<MySqlTableBuilder>( obj ) );
                return;
            case SqlObjectType.View:
                AddRemoveViewAction( ReinterpretCast.To<MySqlViewBuilder>( obj ) );
                return;
        }

        Assume.Unreachable();
    }

    protected override void CompletePendingAlterObjectChanges(SqlObjectBuilder obj, SqlDatabaseChangeAggregator changeAggregator)
    {
        var aggregator = ReinterpretCast.To<MySqlDatabaseChangeAggregator>( changeAggregator );
        switch ( obj.Type )
        {
            case SqlObjectType.Table:
                AddAlterTableActions( ReinterpretCast.To<MySqlTableBuilder>( obj ), aggregator );
                return;
            case SqlObjectType.View:
                AddAlterViewActions( ReinterpretCast.To<MySqlViewBuilder>( obj ), aggregator );
                return;
        }

        Assume.Unreachable();
    }

    [Pure]
    protected override SqlDatabaseChangeAggregator CreateAlterObjectChangeAggregator()
    {
        return new MySqlDatabaseChangeAggregator( this );
    }

    internal new void SetModeAndAttach(SqlDatabaseCreateMode mode)
    {
        base.SetModeAndAttach( mode );
    }

    internal void AddCreateGuidFunctionAction()
    {
        var interpreter = CreateNodeInterpreter();
        MySqlHelpers.AppendCreateGuidFunction( interpreter, Database.CommonSchemaName );
        var sql = GetSqlAndClearContext( interpreter );
        AddSqlAction( sql );
    }

    internal void AddCreateDropIndexIfExistsProcedureAction()
    {
        var interpreter = CreateNodeInterpreter();
        MySqlHelpers.AppendCreateDropIndexIfExistsProcedure( interpreter, Database.CommonSchemaName );
        var sql = GetSqlAndClearContext( interpreter );
        AddSqlAction( sql );
    }

    internal void AddCreateSchemaAction(string name)
    {
        var interpreter = CreateNodeInterpreter();
        MySqlHelpers.AppendCreateSchema(
            interpreter,
            name,
            Database.CharacterSetName,
            Database.CollationName,
            Database.IsEncryptionEnabled );

        var sql = GetSqlAndClearContext( interpreter );
        AddSqlAction( sql );
    }

    private void AddRenameSchemaActions(MySqlSchemaBuilder schema, string originalName)
    {
        if ( ! schema.Name.Equals( Database.CommonSchemaName, StringComparison.OrdinalIgnoreCase ) )
            AddCreateSchemaAction( schema.Name );

        var interpreter = CreateNodeInterpreter();
        var viewsToRename = new List<MySqlViewBuilder>();
        var viewsToReconstruct = SqlDatabaseObjectsSet<MySqlViewBuilder>.Create();

        foreach ( var obj in schema.Objects )
        {
            switch ( obj.Type )
            {
                case SqlObjectType.Table:
                {
                    var table = ReinterpretCast.To<MySqlTableBuilder>( obj );
                    AddRenameTableAction( table, SqlRecordSetInfo.Create( originalName, table.Name ), interpreter );

                    foreach ( var reference in obj.ReferencingObjects )
                    {
                        if ( reference.Source.Object.Type == SqlObjectType.View && reference.Source.Property is null )
                        {
                            var view = ReinterpretCast.To<MySqlViewBuilder>( reference.Source.Object );
                            if ( ! ReferenceEquals( schema, view.Schema ) )
                                viewsToReconstruct.Add( view );
                        }
                    }

                    break;
                }
                case SqlObjectType.View:
                {
                    viewsToRename.Add( ReinterpretCast.To<MySqlViewBuilder>( obj ) );
                    foreach ( var reference in obj.ReferencingObjects )
                    {
                        if ( reference.Source.Object.Type == SqlObjectType.View && reference.Source.Property is null )
                        {
                            var view = ReinterpretCast.To<MySqlViewBuilder>( reference.Source.Object );
                            if ( ! ReferenceEquals( schema, view.Schema ) )
                                viewsToReconstruct.Add( view );
                        }
                    }

                    break;
                }
            }
        }

        viewsToRename.Sort( static (a, b) => a.Id.CompareTo( b.Id ) );
        foreach ( var view in viewsToRename )
            AddReconstructViewAction( view, SqlRecordSetInfo.Create( originalName, view.Name ), interpreter );

        foreach ( var view in viewsToReconstruct )
            AddReconstructViewAction( view, view.Info, interpreter );

        if ( ! originalName.Equals( Database.CommonSchemaName, StringComparison.OrdinalIgnoreCase ) )
            AddRemoveSchemaAction( originalName );
    }

    private void AddRemoveSchemaAction(string name)
    {
        var interpreter = CreateNodeInterpreter();
        MySqlHelpers.AppendDropSchema( interpreter, name );
        var sql = GetSqlAndClearContext( interpreter );
        AddSqlAction( sql );
    }

    private void AddCreateTableAction(MySqlTableBuilder table)
    {
        ValidateTable( table );

        var interpreter = CreateNodeInterpreter();
        var createTable = table.ToCreateNode( includeForeignKeys: false, sortGeneratedColumns: true );
        interpreter.VisitCreateTable( createTable );
        AppendSqlCommandEnd( interpreter );

        var hasForeignKeys = false;
        foreach ( var constraint in table.Constraints )
        {
            switch ( constraint.Type )
            {
                case SqlObjectType.ForeignKey:
                {
                    hasForeignKeys = true;
                    break;
                }
                case SqlObjectType.Index:
                {
                    var index = ReinterpretCast.To<MySqlIndexBuilder>( constraint );
                    if ( index.IsVirtual )
                        continue;

                    interpreter.VisitCreateIndex( index.ToCreateNode() );
                    AppendSqlCommandEnd( interpreter );
                    break;
                }
            }
        }

        if ( hasForeignKeys )
        {
            MySqlHelpers.AppendAlterTableHeader( interpreter, table.Info );

            using ( interpreter.TempIgnoreAllRecordSets() )
            using ( interpreter.Context.TempIndentIncrease() )
            {
                foreach ( var constraint in table.Constraints )
                {
                    if ( constraint.Type != SqlObjectType.ForeignKey )
                        continue;

                    var foreignKey = ReinterpretCast.To<MySqlForeignKeyBuilder>( constraint );
                    MySqlHelpers.AppendAlterTableAddForeignKey( interpreter, foreignKey.ToDefinitionNode( createTable.RecordSet ) );
                }

                interpreter.Context.Sql.ShrinkBy( 1 );
                AppendSqlCommandEnd( interpreter );
            }
        }

        var sql = interpreter.Context.Sql.ToString();
        interpreter.Context.Clear();
        AddSqlAction( sql );
    }

    private void AddCreateViewAction(MySqlViewBuilder view)
    {
        var interpreter = CreateNodeInterpreter();
        interpreter.VisitCreateView( view.ToCreateNode() );
        var sql = GetSqlAndClearContext( interpreter );
        AddSqlAction( sql );
    }

    private void AddRemoveTableAction(MySqlTableBuilder table)
    {
        var interpreter = CreateNodeInterpreter();
        var name = this.GetOriginalValue( table, SqlObjectChangeDescriptor.Name ).GetValueOrDefault( table.Name );
        interpreter.VisitDropTable( SqlNode.DropTable( SqlRecordSetInfo.Create( table.Schema.Name, name ) ) );
        var sql = GetSqlAndClearContext( interpreter );
        AddSqlAction( sql );
    }

    private void AddRemoveViewAction(MySqlViewBuilder view)
    {
        var interpreter = CreateNodeInterpreter();
        var name = this.GetOriginalValue( view, SqlObjectChangeDescriptor.Name ).GetValueOrDefault( view.Name );
        interpreter.VisitDropView( SqlNode.DropView( SqlRecordSetInfo.Create( view.Schema.Name, name ) ) );
        var sql = GetSqlAndClearContext( interpreter );
        AddSqlAction( sql );
    }

    private void AddAlterTableActions(MySqlTableBuilder table, MySqlDatabaseChangeAggregator changeAggregator)
    {
        if ( ! changeAggregator.HasChanged )
            return;

        ValidateTable( table );
        changeAggregator.PrepareColumnsForAlteration( table );
        changeAggregator.PrepareForeignKeysForAlteration();

        var interpreter = CreateNodeInterpreter();
        if ( changeAggregator.IsRenamed )
        {
            var name = changeAggregator.OriginalName.GetValueOrDefault( table.Name );
            AddRenameTableAction( table, SqlRecordSetInfo.Create( table.Schema.Name, name ), interpreter );

            foreach ( var reference in table.ReferencingObjects )
            {
                if ( reference.Source.Object.Type != SqlObjectType.View || reference.Source.Property is not null )
                    continue;

                var view = ReinterpretCast.To<MySqlViewBuilder>( reference.Source.Object );
                AddReconstructViewAction( view, view.Info, interpreter );
            }
        }

        if ( changeAggregator.RemovedForeignKeys.Count > 0 )
        {
            MySqlHelpers.AppendAlterTableHeader( interpreter, table.Info );

            using ( interpreter.TempIgnoreAllRecordSets() )
            using ( interpreter.Context.TempIndentIncrease() )
            {
                foreach ( var (name, _) in changeAggregator.RemovedForeignKeys )
                    MySqlHelpers.AppendAlterTableDropForeignKey( interpreter, name );
            }

            interpreter.Context.Sql.ShrinkBy( 1 );
            AppendSqlCommandEnd( interpreter );
        }

        foreach ( var (name, _) in changeAggregator.RemovedIndexes )
        {
            interpreter.VisitDropIndex( SqlNode.DropIndex( table.Info, SqlSchemaObjectName.Create( table.Info.Name.Schema, name ) ) );
            AppendSqlCommandEnd( interpreter );
        }

        if ( changeAggregator.RequiresNonForeignKeyAlteration )
        {
            MySqlHelpers.AppendAlterTableHeader( interpreter, table.Info );

            using ( interpreter.TempIgnoreAllRecordSets() )
            using ( interpreter.Context.TempIndentIncrease() )
            {
                if ( changeAggregator.IsPrimaryKeyChanged )
                    MySqlHelpers.AppendAlterTableDropPrimaryKey( interpreter );

                foreach ( var (name, _) in changeAggregator.RemovedChecks )
                    MySqlHelpers.AppendAlterTableDropCheck( interpreter, name );

                foreach ( var (originalName, index) in changeAggregator.RenamedIndexes )
                    MySqlHelpers.AppendAlterTableRenameIndex( interpreter, originalName, index.Name );

                foreach ( var (name, _) in changeAggregator.RemovedColumns )
                    MySqlHelpers.AppendAlterTableDropColumn( interpreter, name );

                foreach ( var modification in changeAggregator.ModifiedColumns )
                {
                    var originalName = this.GetOriginalValue( modification.Source, SqlObjectChangeDescriptor.Name )
                        .GetValueOrDefault( modification.Source.Name );

                    MySqlHelpers.AppendAlterTableChangeColumn( interpreter, originalName, modification.Column.ToDefinitionNode() );
                }

                foreach ( var column in changeAggregator.CreatedColumns )
                {
                    if ( column.DefaultValue is null && ! column.IsNullable && column.Computation is null )
                        column.UpdateDefaultValueBasedOnDataType();

                    MySqlHelpers.AppendAlterTableAddColumn( interpreter, column.ToDefinitionNode() );
                }

                if ( changeAggregator.IsPrimaryKeyChanged )
                {
                    var primaryKey = table.Constraints.GetPrimaryKey();
                    MySqlHelpers.AppendAlterTableAddPrimaryKey( interpreter, primaryKey.ToDefinitionNode() );
                }

                foreach ( var check in changeAggregator.CreatedChecks )
                    MySqlHelpers.AppendAlterTableAddCheck( interpreter, check.ToDefinitionNode() );
            }

            interpreter.Context.Sql.ShrinkBy( 1 );
            AppendSqlCommandEnd( interpreter );
        }

        foreach ( var index in changeAggregator.CreatedIndexes )
        {
            interpreter.VisitCreateIndex( index.ToCreateNode() );
            AppendSqlCommandEnd( interpreter );
        }

        if ( changeAggregator.CreatedForeignKeys.Count > 0 )
        {
            MySqlHelpers.AppendAlterTableHeader( interpreter, table.Info );

            using ( interpreter.TempIgnoreAllRecordSets() )
            using ( interpreter.Context.TempIndentIncrease() )
            {
                foreach ( var foreignKey in changeAggregator.CreatedForeignKeys )
                    MySqlHelpers.AppendAlterTableAddForeignKey( interpreter, foreignKey.ToDefinitionNode( table.Node ) );
            }

            interpreter.Context.Sql.ShrinkBy( 1 );
            AppendSqlCommandEnd( interpreter );
        }

        if ( interpreter.Context.Sql.Length == 0 )
            return;

        var sql = interpreter.Context.Sql.AppendLine().ToString();
        interpreter.Context.Clear();
        AddSqlAction( sql );
    }

    private void AddAlterViewActions(MySqlViewBuilder view, MySqlDatabaseChangeAggregator changeAggregator)
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

            var refView = ReinterpretCast.To<MySqlViewBuilder>( reference.Source.Object );
            AddReconstructViewAction( refView, refView.Info, interpreter );
        }
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

    private void AddRenameTableAction(MySqlTableBuilder table, SqlRecordSetInfo originalInfo, SqlNodeInterpreter interpreter)
    {
        interpreter.VisitRenameTable( SqlNode.RenameTable( originalInfo, table.Info.Name ) );
        var sql = GetSqlAndClearContext( interpreter );
        AddSqlAction( sql );
    }

    private void AddReconstructViewAction(MySqlViewBuilder view, SqlRecordSetInfo originalInfo, SqlNodeInterpreter interpreter)
    {
        interpreter.VisitDropView( SqlNode.DropView( originalInfo ) );
        AppendSqlCommandEnd( interpreter );
        interpreter.VisitCreateView( view.ToCreateNode() );
        var sql = GetSqlAndClearContext( interpreter );
        AddSqlAction( sql );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void ValidateTable(MySqlTableBuilder table)
    {
        if ( table.Constraints.TryGetPrimaryKey() is null )
            ExceptionThrower.Throw(
                SqlHelpers.CreateObjectBuilderException( table.Database, ExceptionResources.PrimaryKeyIsMissing( table ) ) );
    }
}
