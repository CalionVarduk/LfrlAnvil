using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo( "LfrlAnvil.Core.Tests" )]

// TODO:
// - ISqlColumnBuilder.DefaultValue should be an expression tree
// - Add partial indexes with filter as an expression tree
// - Add check constraints? as expression trees
// - Add views
// - ISqlColumnTypeDefinitionProvider.RegisterDefinition & Extend method should allow to use a "derived" db type
//   ^ e.g. BarcodeString could be of type NVARCHAR(50), which extends string of type NVARCHAR(MAX)
// - ISqlDatabaseBuilder.AddRawStatement should accept an expression tree + parameters
// - Add internal? methods for efficiently finding out things like: can column be removed, can table be removed etc.
//   ^ right now, memory for enumerators is allocated on the heap
// - Add readonly Sqlite objects that represent fully built db
// - Add column script can be handled with ALTER TABLE
//   ^ however, situation where column with this name was also removed must be treated as a full reconstruction
//   ^ this may need to be handled anyway for MySQL and/or PostgreSQL - leave it as-is, for now
// - SqliteColumnTypeDefinition is a candidate for moving to Sql.Core, as a generic class, where T is an sql data type
// - SqliteIndexColumnBuilder is a candidate for moving to Sql.Core, as a generic class, where T is a column builder type
// - Support for Sqlite COLLATION in added columns? let's see how other SQL providers handle that
// - ON CONFLICT ROLLBACK instead of ABORT? https://www.sqlite.org/lang_conflict.html
// - STRICT table options is supported in v3.37.0 (2021-11-27), disabled for now
// - more ReferenceBehavior options?
// - WITHOUT ROWID is supported in v3.8.2 (2013-12-06), this is the oldest supported version
// - update: v3.8.3 (2014-02-03) is now the oldest supported version, unless CTEs aren't used (WITH clause)
