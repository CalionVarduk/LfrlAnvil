using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.Sql.Exceptions;

public static class ExceptionResources
{
    public const string DefaultSchemaCannotBeRemoved = "Default schema cannot be removed.";
    public const string IndexMustHaveAtLeastOneColumn = "Index must have at least one column.";
    public const string PrimaryKeyIndexMustRemainUnique = "Primary key index must remain unique.";
    public const string ForeignKeyIndexAndReferencedIndexAreTheSame = "Foreign key index and referenced index are the same.";

    public const string ForeignKeyIndexAndReferencedIndexMustHaveTheSameAmountOfColumns =
        "Foreign key index and referenced index must have the same amount of columns.";

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
    public static string IndexAlreadyExists(ReadOnlyMemory<ISqlIndexColumnBuilder> columns)
    {
        var builder = new StringBuilder( 48 );
        builder.Append( "Index with columns (" );

        if ( columns.Length > 0 )
        {
            foreach ( var c in columns.Span )
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
        return $"Type {type} of column '{column.FullName}' is incompatible with type {otherType} of column '{otherColumn.FullName}'.";
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
    public static string ColumnIsReferencedByIndex(ISqlIndexBuilder index)
    {
        return $"Column is referenced by {index}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string DefaultTypeDefinitionCannotBeOverriden(Type type)
    {
        return $"Default registration for type '{type.GetDebugString()}' cannot be overriden.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ValueCannotBeConvertedToDbLiteral(Type type)
    {
        return $"Value cannot be converted to database literal through definition of '{type.GetDebugString()}' type.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ValueCannotBeUsedInParameter(Type type)
    {
        return $"Value cannot be used in database parameter through definition of '{type.GetDebugString()}' type.";
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
        if ( errors.Count == 0 )
            return $"An unexpected error has occurred for {dialect} object builder.";

        var headerText = $"Encountered {errors.Count} error(s) for {dialect} object builder:";
        var errorsText = string.Join( Environment.NewLine, errors.Select( (e, i) => $"{i + 1}. {e}" ) );
        return $"{headerText}{Environment.NewLine}{errorsText}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string GetObjectCastMessage(SqlDialect dialect, Type expected, Type actual)
    {
        return $"Expected {dialect} object of type {expected.GetDebugString()} but found object of type {actual.GetDebugString()}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string GetVersionHistoryErrors(Chain<string> errors)
    {
        if ( errors.Count == 0 )
            return $"An unexpected error has occurred during version history validation.";

        var headerText = $"Encountered {errors.Count} version history validation error(s):";
        var errorsText = string.Join( Environment.NewLine, errors.Select( (e, i) => $"{i + 1}. {e}" ) );
        return $"{headerText}{Environment.NewLine}{errorsText}";
    }
}
