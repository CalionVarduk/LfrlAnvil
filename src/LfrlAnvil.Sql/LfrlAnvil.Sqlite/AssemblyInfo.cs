using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo( "LfrlAnvil.Core.Tests" )]

// TODO:
// - ISqlColumnBuilder.DefaultValue should be an expression tree <= NEXT
// - Add partial indexes with filter as an expression tree <= NEXT NEXT
// - Add check constraints? as expression trees
// - ISqlDatabaseBuilder.AddRawStatement should accept an expression tree (array of, internally will create a batch node) + parameters
// - Add column script can be handled with ALTER TABLE
//   ^ however, situation where column with this name was also removed must be treated as a full reconstruction
//   ^ this may need to be handled anyway for MySQL and/or PostgreSQL - leave it as-is, for now
// - SqliteColumnTypeDefinition is a candidate for moving to Sql.Core, as a generic class, where T is an sql data type
// - SqliteIndexColumnBuilder is a candidate for moving to Sql.Core, as a generic class, where T is a column builder type
//   ^ honestly, pretty much everything could be moved to Sql.Core, as generic abstract classes, that allow to override default behaviors
//   ^ and define new ones
// - Add some events to database creation process, e.g. version statement begin, version statement end
//   ^ so that e.g. loggers could be easily added
// - For Sqlite, add possibility to setup connection with e.g. custom functions
//   ^ just add an optional delegate for on connection created/connected
// - For Sqlite, add possibility to set connection to permanent in factory - works like the current in-memory version
// - Support for Sqlite COLLATION in added columns? let's see how other SQL providers handle that
// - ON CONFLICT ROLLBACK instead of ABORT? https://www.sqlite.org/lang_conflict.html
// - STRICT table option is supported in v3.37.0 (2021-11-27), disabled for now
// - more ReferenceBehavior options?
// - WITHOUT ROWID is supported in v3.8.2 (2013-12-06), this is the oldest supported version
// - update: v3.8.3 (2014-02-03) is now the oldest supported version, unless CTEs aren't used (WITH clause)
// - Add CREATE/DROP (TEMP)INDEX nodes
// - Add window function nodes
// - Add TemporaryTable & TableBuilder as valid target for complex Update/DeleteFrom nodes <= NEXT
// - Add attach/detach DB nodes
// - Add optional explicit DB name to table/view record set nodes (cross-DB queries)
// - Make DB builder use SQL node tree? This could unify a lot of SQL statement building by moving it all to interpreters
//   ^ it will cause a bit of an overhead, due to having to create a graph and then interpret it
//   ^ but it happens only once per version, when it's actually applied to the DB, so it should be fine
// - Add sql node visitor that allows to extract node's type (+ add Type to ViewDataField)
