using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;
using LfrlAnvil.PostgreSql.Internal;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using ExceptionResources = LfrlAnvil.Sql.Exceptions.ExceptionResources;

namespace LfrlAnvil.PostgreSql.Objects.Builders;

public sealed class PostgreSqlDatabaseChangeTracker : SqlDatabaseChangeTracker
{
    internal PostgreSqlDatabaseChangeTracker() { }

    public new PostgreSqlDatabaseBuilder Database => ReinterpretCast.To<PostgreSqlDatabaseBuilder>( base.Database );

    protected override void AddNameChange(SqlObjectBuilder activeObject, SqlObjectBuilder target, string originalValue)
    {
        if ( target.Type != SqlObjectType.Schema )
        {
            base.AddNameChange( activeObject, target, originalValue );
            return;
        }

        CompletePendingChanges();

        var interpreter = CreateNodeInterpreter();
        PostgreSqlHelpers.AppendRenameSchema( interpreter, originalValue, target.Name );
        var sql = GetSqlAndClearContext( interpreter );
        AddSqlAction( sql );
    }

    protected override void AddIsRemovedChange(SqlObjectBuilder activeObject, SqlObjectBuilder target)
    {
        if ( target.Type != SqlObjectType.Schema )
        {
            base.AddIsRemovedChange( activeObject, target );
            return;
        }

        CompletePendingChanges();

        var schema = ReinterpretCast.To<PostgreSqlSchemaBuilder>( target );
        if ( target.IsRemoved )
            AddRemoveSchemaAction( schema.Name );
        else
            AddCreateSchemaAction( schema.Name );
    }

    protected override void CompletePendingCreateObjectChanges(SqlObjectBuilder obj)
    {
        switch ( obj.Type )
        {
            case SqlObjectType.Table:
                AddCreateTableAction( ReinterpretCast.To<PostgreSqlTableBuilder>( obj ) );
                return;
            case SqlObjectType.View:
                AddCreateViewAction( ReinterpretCast.To<PostgreSqlViewBuilder>( obj ) );
                return;
        }

        Assume.Unreachable();
    }

    protected override void CompletePendingRemoveObjectChanges(SqlObjectBuilder obj)
    {
        switch ( obj.Type )
        {
            case SqlObjectType.Table:
                AddRemoveTableAction( ReinterpretCast.To<PostgreSqlTableBuilder>( obj ) );
                return;
            case SqlObjectType.View:
                AddRemoveViewAction( ReinterpretCast.To<PostgreSqlViewBuilder>( obj ) );
                return;
        }

        Assume.Unreachable();
    }

    protected override void CompletePendingAlterObjectChanges(SqlObjectBuilder obj, SqlDatabaseChangeAggregator changeAggregator)
    {
        var aggregator = ReinterpretCast.To<PostgreSqlDatabaseChangeAggregator>( changeAggregator );
        switch ( obj.Type )
        {
            case SqlObjectType.Table:
                AddAlterTableActions( ReinterpretCast.To<PostgreSqlTableBuilder>( obj ), aggregator );
                return;
            case SqlObjectType.View:
                AddAlterViewAction( ReinterpretCast.To<PostgreSqlViewBuilder>( obj ), aggregator );
                return;
        }

        Assume.Unreachable();
    }

    [Pure]
    protected override SqlDatabaseChangeAggregator CreateAlterObjectChangeAggregator()
    {
        return new PostgreSqlDatabaseChangeAggregator( this );
    }

    internal new void SetModeAndAttach(SqlDatabaseCreateMode mode)
    {
        base.SetModeAndAttach( mode );
    }

    internal void AddCreateSchemaAction(string name)
    {
        var interpreter = CreateNodeInterpreter();
        PostgreSqlHelpers.AppendCreateSchema( interpreter, name );
        var sql = GetSqlAndClearContext( interpreter );
        AddSqlAction( sql );
    }

    private void AddRemoveSchemaAction(string name)
    {
        var interpreter = CreateNodeInterpreter();
        PostgreSqlHelpers.AppendDropSchema( interpreter, name );
        var sql = GetSqlAndClearContext( interpreter );
        AddSqlAction( sql );
    }

    private void AddCreateTableAction(PostgreSqlTableBuilder table)
    {
        ValidateTable( table );

        var interpreter = CreateNodeInterpreter();
        var createTable = table.ToCreateNode();
        interpreter.VisitCreateTable( createTable );
        AppendSqlCommandEnd( interpreter );

        foreach ( var constraint in table.Constraints )
        {
            if ( constraint.Type != SqlObjectType.Index )
                continue;

            var index = ReinterpretCast.To<PostgreSqlIndexBuilder>( constraint );
            if ( index.IsVirtual )
                continue;

            interpreter.VisitCreateIndex( index.ToCreateNode() );
            AppendSqlCommandEnd( interpreter );
        }

        var sql = interpreter.Context.Sql.ToString();
        interpreter.Context.Clear();
        AddSqlAction( sql );
    }

    private void AddCreateViewAction(PostgreSqlViewBuilder view)
    {
        var interpreter = CreateNodeInterpreter();
        interpreter.VisitCreateView( view.ToCreateNode() );
        var sql = GetSqlAndClearContext( interpreter );
        AddSqlAction( sql );
    }

    private void AddRemoveTableAction(PostgreSqlTableBuilder table)
    {
        var interpreter = CreateNodeInterpreter();
        var name = this.GetOriginalValue( table, SqlObjectChangeDescriptor.Name ).GetValueOrDefault( table.Name );
        interpreter.VisitDropTable( SqlNode.DropTable( SqlRecordSetInfo.Create( table.Schema.Name, name ) ) );
        var sql = GetSqlAndClearContext( interpreter );
        AddSqlAction( sql );
    }

    private void AddRemoveViewAction(PostgreSqlViewBuilder view)
    {
        var interpreter = CreateNodeInterpreter();
        var name = this.GetOriginalValue( view, SqlObjectChangeDescriptor.Name ).GetValueOrDefault( view.Name );
        interpreter.VisitDropView( SqlNode.DropView( SqlRecordSetInfo.Create( view.Schema.Name, name ) ) );
        var sql = GetSqlAndClearContext( interpreter );
        AddSqlAction( sql );
    }

    private void AddAlterTableActions(PostgreSqlTableBuilder table, PostgreSqlDatabaseChangeAggregator changeAggregator)
    {
        if ( ! changeAggregator.HasChanged )
            return;

        ValidateTable( table );
        changeAggregator.PrepareColumnsForAlteration( table );

        var interpreter = CreateNodeInterpreter();
        if ( changeAggregator.IsRenamed )
        {
            var name = changeAggregator.OriginalName.GetValueOrDefault( table.Name );
            AddRenameTableAction( table, SqlRecordSetInfo.Create( table.Schema.Name, name ), interpreter );
        }

        if ( changeAggregator.RemovedForeignKeys.Count > 0 || changeAggregator.RemovedChecks.Count > 0 )
        {
            PostgreSqlHelpers.AppendAlterTableHeader( interpreter, table.Info );
            using ( interpreter.Context.TempIndentIncrease() )
            {
                foreach ( var (name, _) in changeAggregator.RemovedForeignKeys )
                    PostgreSqlHelpers.AppendAlterTableDropConstraint( interpreter, name );

                foreach ( var (name, _) in changeAggregator.RemovedChecks )
                    PostgreSqlHelpers.AppendAlterTableDropConstraint( interpreter, name );
            }

            interpreter.Context.Sql.ShrinkBy( 1 );
            AppendSqlCommandEnd( interpreter );
        }

        foreach ( var (name, _) in changeAggregator.RemovedIndexes )
        {
            interpreter.VisitDropIndex( SqlNode.DropIndex( table.Info, SqlSchemaObjectName.Create( table.Info.Name.Schema, name ) ) );
            AppendSqlCommandEnd( interpreter );
        }

        if ( changeAggregator.DroppedPrimaryKey is not null )
        {
            var originalName = this.GetOriginalValue( changeAggregator.DroppedPrimaryKey, SqlObjectChangeDescriptor.Name )
                .GetValueOrDefault( changeAggregator.DroppedPrimaryKey.Name );

            PostgreSqlHelpers.AppendAlterTableHeader( interpreter, table.Info );
            using ( interpreter.Context.TempIndentIncrease() )
                PostgreSqlHelpers.AppendAlterTableDropConstraint( interpreter, originalName );

            interpreter.Context.Sql.ShrinkBy( 1 );
            AppendSqlCommandEnd( interpreter );
        }

        if ( changeAggregator.ModifiedColumns.Count > 0 || changeAggregator.RemovedColumns.Count > 0 )
        {
            var isAltered = false;
            if ( changeAggregator.RemovedColumns.Count > 0 )
            {
                isAltered = true;
                PostgreSqlHelpers.AppendAlterTableHeader( interpreter, table.Info );
            }

            foreach ( var modification in changeAggregator.ModifiedColumns )
            {
                var originalComputation = this.GetOriginalValue( modification.Source, SqlObjectChangeDescriptor.Computation )
                    .GetValueOrDefault( modification.Source.Computation );

                if ( originalComputation is null || modification.Column.Computation is not null )
                    continue;

                if ( ! isAltered )
                {
                    isAltered = true;
                    PostgreSqlHelpers.AppendAlterTableHeader( interpreter, table.Info );
                }

                var originalName = this.GetOriginalValue( modification.Source, SqlObjectChangeDescriptor.Name )
                    .GetValueOrDefault( modification.Source.Name );

                using ( interpreter.Context.TempIndentIncrease() )
                    PostgreSqlHelpers.AppendAlterTableDropColumnExpression( interpreter, originalName );
            }

            using ( interpreter.Context.TempIndentIncrease() )
            {
                foreach ( var (name, _) in changeAggregator.RemovedColumns )
                    PostgreSqlHelpers.AppendAlterTableDropColumn( interpreter, name );
            }

            if ( isAltered )
            {
                interpreter.Context.Sql.ShrinkBy( 1 );
                AppendSqlCommandEnd( interpreter );
            }
        }

        if ( changeAggregator.ConstraintRenames.Count > 0 )
        {
            changeAggregator.PopulateConstraintsByOriginalName();
            HandleObjectCollectionRenaming(
                interpreter,
                table,
                changeAggregator.ConstraintRenames,
                changeAggregator.ObjectsByOriginalName );
        }

        if ( changeAggregator.ColumnRenames.Count > 0 )
        {
            changeAggregator.PopulateColumnsByOriginalName();
            HandleObjectCollectionRenaming(
                interpreter,
                table,
                changeAggregator.ColumnRenames,
                changeAggregator.ObjectsByOriginalName );
        }

        if ( changeAggregator.ModifiedColumns.Count > 0
            || changeAggregator.CreatedColumns.Count > 0
            || changeAggregator.CreatedPrimaryKey is not null )
        {
            var isAltered = false;
            if ( changeAggregator.CreatedColumns.Count > 0 || changeAggregator.CreatedPrimaryKey is not null )
            {
                isAltered = true;
                PostgreSqlHelpers.AppendAlterTableHeader( interpreter, table.Info );
            }

            foreach ( var modification in changeAggregator.ModifiedColumns )
            {
                var originalIsNullable = this.GetOriginalValue( modification.Source, SqlObjectChangeDescriptor.IsNullable )
                    .GetValueOrDefault( modification.Source.IsNullable );

                var originalDataType = this.GetOriginalValue( modification.Source, SqlObjectChangeDescriptor.DataType )
                    .GetValueOrDefault( modification.Source.TypeDefinition.DataType );

                var originalDefaultValue = this.GetOriginalValue( modification.Source, SqlObjectChangeDescriptor.DefaultValue )
                    .GetValueOrDefault( modification.Source.DefaultValue );

                var isNullableChanged = originalIsNullable != modification.Column.IsNullable;
                var isDataTypeChanged = ! originalDataType.Equals( modification.Column.TypeDefinition.DataType );
                var isDefaultValueChanged = ! ReferenceEquals( originalDefaultValue, modification.Column.DefaultValue );

                if ( ! isAltered && (isNullableChanged || isDataTypeChanged || isDefaultValueChanged) )
                {
                    isAltered = true;
                    PostgreSqlHelpers.AppendAlterTableHeader( interpreter, table.Info );
                }

                if ( isDefaultValueChanged && originalDefaultValue is not null )
                {
                    using ( interpreter.Context.TempIndentIncrease() )
                        PostgreSqlHelpers.AppendAlterTableDropColumnDefault( interpreter, modification.Column.Name );
                }

                if ( isNullableChanged )
                {
                    using ( interpreter.Context.TempIndentIncrease() )
                    {
                        if ( modification.Column.IsNullable )
                            PostgreSqlHelpers.AppendAlterTableDropColumnNotNull( interpreter, modification.Column.Name );
                        else
                            PostgreSqlHelpers.AppendAlterTableSetColumnNotNull( interpreter, modification.Column.Name );
                    }
                }

                if ( isDataTypeChanged )
                {
                    using ( interpreter.Context.TempIndentIncrease() )
                        PostgreSqlHelpers.AppendAlterTableSetColumnDataType(
                            interpreter,
                            modification.Column.Name,
                            modification.Column.TypeDefinition.DataType );
                }

                if ( isDefaultValueChanged && modification.Column.DefaultValue is not null )
                {
                    using ( interpreter.TempIgnoreAllRecordSets() )
                    using ( interpreter.Context.TempIndentIncrease() )
                        PostgreSqlHelpers.AppendAlterTableSetColumnDefault(
                            interpreter,
                            modification.Column.Name,
                            modification.Column.DefaultValue );
                }
            }

            var containsGeneratedColumn = false;
            foreach ( var column in changeAggregator.CreatedColumns )
            {
                if ( column.Computation is not null )
                {
                    containsGeneratedColumn = true;
                    continue;
                }

                if ( column.DefaultValue is null && ! column.IsNullable )
                    column.UpdateDefaultValueBasedOnDataType();

                using ( interpreter.TempIgnoreAllRecordSets() )
                using ( interpreter.Context.TempIndentIncrease() )
                    PostgreSqlHelpers.AppendAlterTableAddColumn( interpreter, column.ToDefinitionNode() );
            }

            if ( containsGeneratedColumn )
            {
                foreach ( var column in changeAggregator.CreatedColumns )
                {
                    if ( column.Computation is null )
                        continue;

                    using ( interpreter.TempIgnoreAllRecordSets() )
                    using ( interpreter.Context.TempIndentIncrease() )
                        PostgreSqlHelpers.AppendAlterTableAddColumn( interpreter, column.ToDefinitionNode() );
                }
            }

            if ( changeAggregator.CreatedPrimaryKey is not null )
            {
                using ( interpreter.TempIgnoreAllRecordSets() )
                using ( interpreter.Context.TempIndentIncrease() )
                    PostgreSqlHelpers.AppendAlterTableAddPrimaryKey( interpreter, changeAggregator.CreatedPrimaryKey.ToDefinitionNode() );
            }

            if ( isAltered )
            {
                interpreter.Context.Sql.ShrinkBy( 1 );
                AppendSqlCommandEnd( interpreter );
            }
        }

        foreach ( var index in changeAggregator.CreatedIndexes )
        {
            interpreter.VisitCreateIndex( index.ToCreateNode() );
            AppendSqlCommandEnd( interpreter );
        }

        if ( changeAggregator.CreatedForeignKeys.Count > 0 || changeAggregator.CreatedChecks.Count > 0 )
        {
            PostgreSqlHelpers.AppendAlterTableHeader( interpreter, table.Info );
            using ( interpreter.TempIgnoreAllRecordSets() )
            using ( interpreter.Context.TempIndentIncrease() )
            {
                foreach ( var check in changeAggregator.CreatedChecks )
                    PostgreSqlHelpers.AppendAlterTableAddCheck( interpreter, check.ToDefinitionNode() );

                foreach ( var foreignKey in changeAggregator.CreatedForeignKeys )
                    PostgreSqlHelpers.AppendAlterTableAddForeignKey( interpreter, foreignKey.ToDefinitionNode( table.Node ) );
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

    private static void HandleObjectCollectionRenaming(
        SqlNodeInterpreter interpreter,
        PostgreSqlTableBuilder table,
        Dictionary<ulong, PostgreSqlObjectRename> renames,
        SqlDatabaseNamedObjectsSet<SqlObjectBuilder> objectsByOriginalName)
    {
        foreach ( var rename in renames.Values )
        {
            if ( ! rename.IsPending )
                continue;

            ref var renameRef = ref CollectionsMarshal.GetValueRefOrNullRef( renames, rename.Object.Id );
            Assume.False( Unsafe.IsNullRef( ref renameRef ) );

            renameRef = renameRef.Complete();
            HandleObjectRename( interpreter, table, objectsByOriginalName, renames, ref renameRef );
        }

        static void HandleObjectRename(
            SqlNodeInterpreter interpreter,
            PostgreSqlTableBuilder table,
            SqlDatabaseNamedObjectsSet<SqlObjectBuilder> objectsByOriginalName,
            Dictionary<ulong, PostgreSqlObjectRename> objectRenames,
            ref PostgreSqlObjectRename rename)
        {
            Assume.False( rename.IsPending );

            var conflictingColumn = objectsByOriginalName.TryGetObject( rename.Name );
            if ( conflictingColumn is not null )
            {
                ref var conflictingRename = ref CollectionsMarshal.GetValueRefOrNullRef( objectRenames, conflictingColumn.Id );
                Assume.False( Unsafe.IsNullRef( ref conflictingRename ) );

                if ( conflictingRename.IsPending )
                {
                    conflictingRename = conflictingRename.Complete();
                    HandleObjectRename( interpreter, table, objectsByOriginalName, objectRenames, ref conflictingRename );
                }
                else
                {
                    var tempName = CreateTemporaryName( conflictingRename.OriginalName );
                    AppendObjectRename( interpreter, table, conflictingRename.Object, conflictingRename.OriginalName, tempName );

                    objectsByOriginalName.Remove( conflictingRename.OriginalName );
                    objectsByOriginalName.Add( tempName, conflictingRename.Object );
                    conflictingRename = PostgreSqlObjectRename.CreateTemporary( conflictingRename, tempName );
                }
            }

            AppendObjectRename( interpreter, table, rename.Object, rename.OriginalName, rename.Name );
            objectsByOriginalName.Remove( rename.OriginalName );
            objectsByOriginalName.Add( rename.Name, rename.Object );
        }

        static void AppendObjectRename(
            SqlNodeInterpreter interpreter,
            PostgreSqlTableBuilder table,
            SqlObjectBuilder obj,
            string oldName,
            string newName)
        {
            switch ( obj.Type )
            {
                case SqlObjectType.Column:
                    interpreter.VisitRenameColumn( SqlNode.RenameColumn( table.Info, oldName, newName ) );
                    break;

                case SqlObjectType.Index:
                    PostgreSqlHelpers.AppendRenameIndex(
                        interpreter,
                        SqlSchemaObjectName.Create( table.Info.Name.Schema, oldName ),
                        newName );

                    break;

                default:
                    PostgreSqlHelpers.AppendRenameConstraint( interpreter, table.Info, oldName, newName );
                    break;
            }

            AppendSqlCommandEnd( interpreter );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static string CreateTemporaryName(string name)
        {
            return $"__{name}__{Guid.NewGuid():N}__";
        }
    }

    private void AddAlterViewAction(PostgreSqlViewBuilder view, PostgreSqlDatabaseChangeAggregator changeAggregator)
    {
        if ( ! changeAggregator.IsRenamed )
            return;

        var interpreter = CreateNodeInterpreter();
        var originalName = SqlSchemaObjectName.Create( view.Schema.Name, changeAggregator.OriginalName.GetValueOrDefault( view.Name ) );
        PostgreSqlHelpers.AppendRenameView( interpreter, originalName, view.Name );
        var sql = GetSqlAndClearContext( interpreter );
        AddSqlAction( sql );
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

    private void AddRenameTableAction(PostgreSqlTableBuilder table, SqlRecordSetInfo originalInfo, SqlNodeInterpreter interpreter)
    {
        interpreter.VisitRenameTable( SqlNode.RenameTable( originalInfo, table.Info.Name ) );
        var sql = GetSqlAndClearContext( interpreter );
        AddSqlAction( sql );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void ValidateTable(PostgreSqlTableBuilder table)
    {
        if ( table.Constraints.TryGetPrimaryKey() is null )
            ExceptionThrower.Throw(
                SqlHelpers.CreateObjectBuilderException( table.Database, ExceptionResources.PrimaryKeyIsMissing( table ) ) );
    }
}
