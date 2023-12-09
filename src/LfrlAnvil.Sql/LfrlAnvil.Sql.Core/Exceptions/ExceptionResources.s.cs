using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.Sql.Exceptions;

public static class ExceptionResources
{
    public const string DefaultSchemaCannotBeRemoved = "Default schema cannot be removed.";
    public const string IndexMustHaveAtLeastOneColumn = "Index must have at least one column.";
    public const string PrimaryKeyIndexMustRemainUnique = "Primary key index must remain unique.";
    public const string PrimaryKeyIndexCannotBePartial = "Primary key index cannot be partial.";
    public const string ForeignKeyOriginIndexAndReferencedIndexAreTheSame = "Foreign key origin index and referenced index are the same.";

    public const string ForeignKeyOriginIndexAndReferencedIndexMustHaveTheSameAmountOfColumns =
        "Foreign key origin index and referenced index must have the same amount of columns.";

    public const string DummyDataSourceDoesNotContainAnyRecordSets = "Dummy data source does not contain any record sets.";
    public const string RowTypeCannotBeAbstract = "Row type cannot be abstract.";
    public const string RowTypeCannotBeOpenGeneric = "Row type cannot be an open generic type.";
    public const string RowTypeCannotBeNullable = "Row type cannot be nullable.";
    public const string RowTypeDoesNotHaveValidCtor = "Row type doesn't have a valid constructor.";
    public const string RowTypeDoesNotHaveAnyValidMembers = "Row type doesn't have any valid members.";
    public const string SourceTypeCannotBeAbstract = "Source type cannot be abstract.";
    public const string SourceTypeCannotBeOpenGeneric = "Source type cannot be an open generic type.";
    public const string SourceTypeCannotBeNullable = "Source type cannot be nullable.";

    internal static readonly string DataReaderDoesNotSupportAsyncQueries =
        $"Only data readers of type '{typeof( DbDataReader ).GetDebugString()}' support asynchronous queries.";

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ObjectHasBeenRemoved(ISqlObjectBuilder obj)
    {
        return $"{obj} has been removed.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string NameIsAlreadyTaken(ISqlObjectBuilder obj, string name)
    {
        return $"'{name}' is already taken by {obj}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ForeignKeyAlreadyExists(ISqlIndexBuilder index, ISqlIndexBuilder referencingIndex)
    {
        return $"Foreign key '{index.FullName}' => '{referencingIndex.FullName}' already exists.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string CheckAlreadyExists(string name)
    {
        return $"Check '{name}' already exists.";
    }

    [Pure]
    public static string IndexAlreadyExists(ReadOnlyMemory<ISqlIndexColumnBuilder> columns)
    {
        var builder = new StringBuilder( 48 );
        builder.Append( "Index with columns (" );

        if ( columns.Length > 0 )
        {
            foreach ( var c in columns )
                builder.Append( c.Column.Name ).Append( ' ' ).Append( c.Ordering.Name );
        }

        builder.Append( ") already exists." );
        return builder.ToString();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string InvalidName(string name)
    {
        return $"'{name}' is not a valid name.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string DetectedExternalForeignKey(ISqlForeignKeyBuilder foreignKey)
    {
        return $"Detected an external '{foreignKey.FullName}' foreign key.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string DetectedExternalReferencingView(ISqlViewBuilder view, ISqlObjectBuilder obj)
    {
        return $"Detected an external '{view.FullName}' referencing view in {obj}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ObjectDoesNotBelongToTable(ISqlObjectBuilder obj, ISqlTableBuilder expectedTable)
    {
        return $"{obj} does not belong to table '{expectedTable.FullName}'.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ObjectBelongsToAnotherDatabase(ISqlObjectBuilder obj)
    {
        return $"{obj} belongs to another database.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ColumnIsNullable(ISqlColumnBuilder column)
    {
        return $"{column} is nullable.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ColumnIsDuplicated(ISqlColumnBuilder column)
    {
        return $"{column} is duplicated.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ColumnTypesAreIncompatible(ISqlColumnBuilder column, ISqlColumnBuilder otherColumn)
    {
        var type = column.TypeDefinition.RuntimeType.GetDebugString();
        var otherType = otherColumn.TypeDefinition.RuntimeType.GetDebugString();
        return $"Type '{type}' of column '{column.FullName}' is incompatible with type '{otherType}' of column '{otherColumn.FullName}'.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string IndexIsNotMarkedAsUnique(ISqlIndexBuilder index)
    {
        return $"{index} is not marked as unique.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string UnrecognizedTypeDefinition(ISqlColumnTypeDefinition definition)
    {
        return $"Column type definition {definition} is unrecognized in this database.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string PrimaryKeyIsMissing(ISqlTableBuilder table)
    {
        return $"{table} is missing a primary key.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string IndexMustRemainUniqueBecauseItIsReferencedByForeignKey(ISqlForeignKeyBuilder foreignKey)
    {
        return $"Index must remain unique because it is referenced by {foreignKey}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ColumnIsReferencedByObject(ISqlObjectBuilder obj)
    {
        return $"Column is referenced by {obj}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ColumnIsReferencedByIndexFilter(ISqlIndexBuilder index)
    {
        return $"Column is referenced by filter of {index}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string TableIsReferencedByObject(ISqlObjectBuilder obj)
    {
        return $"Table is referenced by {obj}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ViewIsReferencedByObject(ISqlObjectBuilder obj)
    {
        return $"View is referenced by {obj}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ValueCannotBeConvertedToDbLiteral(Type type)
    {
        return $"Value cannot be converted to database literal through definition of '{type.GetDebugString()}' type.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string UnrecognizedSqlNode(Type visitorType, SqlNodeBase node)
    {
        return $@"Visitor of '{visitorType.GetDebugString()}' type doesn't recognize the following node:
{node}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string FailedWhileVisitingNode(string reason, Type visitorType, SqlNodeBase node)
    {
        return $@"Visitor of '{visitorType.GetDebugString()}' type has failed because {reason} while visiting the following node:
{node}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ColumnBelongsToAnotherDatabase(SqlColumnBuilderNode node)
    {
        return $@"Column belongs to another database:
{node}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string TableBelongsToAnotherDatabase(SqlTableBuilderNode node)
    {
        return $@"Table belongs to another database:
{node}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ViewBelongsToAnotherDatabase(SqlViewBuilderNode node)
    {
        return $@"View belongs to another database:
{node}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ColumnBelongsToAnotherTable(SqlColumnBuilderNode node)
    {
        return $@"Column belongs to another table:
{node}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ColumnIsArchived(SqlColumnBuilderNode node)
    {
        return $@"Column is archived:
{node}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string TableIsArchived(SqlTableBuilderNode node)
    {
        return $@"Table is archived:
{node}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ViewIsArchived(SqlViewBuilderNode node)
    {
        return $@"View is archived:
{node}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string UnexpectedNode(SqlNodeBase node)
    {
        return $@"Unexpected node of type '{node.GetType().GetDebugString()}':
{node}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string MissingColumnTypeDefinition(Type type)
    {
        return $"Column type definition for type '{type.GetDebugString()}' is missing.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string UnexpectedStatementParameter(string name, Type type)
    {
        return $"Found unexpected statement parameter '{name}' of type '{type.GetDebugString()}'.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string IncompatibleStatementParameterType(string name, TypeNullability expectedType, Type actualType)
    {
        return
            $"Found statement parameter '{name}' with expected type '{expectedType}' but actual type is '{actualType.GetDebugString()}'.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string RequiredStatementParameterIsIgnoredWhenNull(string name, Type actualType)
    {
        return $"Found nullable statement parameter '{name}' of '{actualType.GetDebugString()}' type which is ignored when value is null.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string MissingStatementParameter(string name, TypeNullability? type)
    {
        return type is null
            ? $"Found missing statement parameter '{name}'."
            : $"Found missing statement parameter '{name}' of type '{type.Value}'.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ParameterAppearsMoreThanOnce(string name)
    {
        return $"Parameter '{name}' appears more than once.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string StatementIsParameterized(ISqlStatementNode statement, SqlNodeInterpreterContext context)
    {
        var parameters = context.Parameters;
        var parametersText = string.Join(
            Environment.NewLine,
            parameters.Select( (p, i) => $"{i + 1}. @{p.Key} : {p.Value?.ToString() ?? "?"}" ) );

        return $@"Statement
{statement.Node}
contains {parameters.Count} parameter(s):
{parametersText}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FieldDoesNotExist(string name)
    {
        return $"Field with name '{name}' does not exist.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FieldExistsMoreThanOnce(string name)
    {
        return $"Field with name '{name}' exists more than once.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string GivenRecordSetWasNotPresentInDataSource(string recordSetName)
    {
        return $"The given record set name '{recordSetName}' was not present in the data source.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string VersionIsPrecededByVersionWithGreaterOrEqualValue(
        int index,
        SqlDatabaseVersion previous,
        SqlDatabaseVersion current)
    {
        return $"Version {current} at position {index} is preceded by version {previous} with greater or equal value.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FirstVersionHasValueEqualToInitialValue(SqlDatabaseVersion version)
    {
        return $"First version {version} has value equal to the initial value.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string DatabaseVersionDoesNotExistInHistory(Version version)
    {
        return $"Database version {version} does not exist in the provided version history.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string VersionCountDoesNotMatch(int dbCount, int historyCount, Version dbVersion)
    {
        return
            $"Database version count ({dbCount}) & version count ({historyCount}) from the provided version history do not match for database version {dbVersion}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string PersistedVersionCountDoesNotMatch(int dbCount, int historyCount, Version dbVersion)
    {
        return
            $"Persisted database version count ({dbCount}) & version count ({historyCount}) from the provided version history do not match for database version {dbVersion}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string DatabaseAndHistoryVersionDoNotMatch(SqlDatabaseVersionRecord record, Version historyVersion)
    {
        return
            $"Database version {record.Version} with ordinal {record.Ordinal} & version {historyVersion} from the provided version history do not match.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string GetObjectBuilderErrors(SqlDialect dialect, Chain<string> errors)
    {
        return errors.Count == 0
            ? $"An unexpected error has occurred for {dialect} object builder."
            : MergeErrors( $"Encountered {errors.Count} error(s) for {dialect} object builder:", errors );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string GetObjectCastMessage(SqlDialect dialect, Type expected, Type actual)
    {
        return $"Expected {dialect} object of type '{expected.GetDebugString()}' but found object of type '{actual.GetDebugString()}'.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string GetVersionHistoryErrors(Chain<string> errors)
    {
        return errors.Count == 0
            ? "An unexpected error has occurred during version history validation."
            : MergeErrors( $"Encountered {errors.Count} version history validation error(s):", errors );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string CompilerErrorsHaveOccurred(SqlDialect dialect, Chain<string> errors)
    {
        return errors.Count == 0
            ? $"An unexpected error has occurred during {dialect} compilation."
            : MergeErrors( $"Encountered {errors.Count} error(s) during {dialect} compilation:", errors );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string CompilerConfigurationErrorsHaveOccurred(Chain<Pair<Expression, Exception>> errors)
    {
        return errors.Count == 0
            ? "An unexpected error has occurred during compiler configuration."
            : MergeErrors(
                $"Encountered {errors.Count} error(s) during compiler configuration:",
                errors.Select( GetCompilerConfigurationError ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidDataTypeParameters(Chain<Pair<SqlDataTypeParameter, int>> parameters)
    {
        return parameters.Count == 0
            ? "Some parameters are invalid."
            : MergeErrors( $"Encountered {parameters.Count} invalid parameter(s):", parameters.Select( GetInvalidParameterError ) );
    }

    [Pure]
    private static string MergeErrors(string header, IEnumerable<string> elements)
    {
        var errorsText = string.Join( Environment.NewLine, elements.Select( static (e, i) => $"{i + 1}. {e}" ) );
        return $"{header}{Environment.NewLine}{errorsText}";
    }

    [Pure]
    private static string GetCompilerConfigurationError(Pair<Expression, Exception> error)
    {
        return $"Extraction of '{error.First}' expression's value has failed with reason: {error.Second.Message}.";
    }

    [Pure]
    private static string GetInvalidParameterError(Pair<SqlDataTypeParameter, int> parameter)
    {
        return $"Invalid value {parameter.Second} for parameter {parameter.First}.";
    }
}
