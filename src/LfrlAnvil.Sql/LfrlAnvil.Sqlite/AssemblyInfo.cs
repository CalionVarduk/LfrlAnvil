using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo( "LfrlAnvil.Core.Tests" )]

// TODO:
// - ISqlColumnBuilder.DefaultValue should be an expression tree
// - Add partial indexes with filter as an expression tree
// - Add check constraints? as expression trees
// - Add views
// - ISqlDatabaseBuilder.AddRawStatement should accept an expression tree + parameters
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
// - Support for Sqlite COLLATION in added columns? let's see how other SQL providers handle that
// - ON CONFLICT ROLLBACK instead of ABORT? https://www.sqlite.org/lang_conflict.html
// - STRICT table option is supported in v3.37.0 (2021-11-27), disabled for now
// - more ReferenceBehavior options?
// - WITHOUT ROWID is supported in v3.8.2 (2013-12-06), this is the oldest supported version
// - update: v3.8.3 (2014-02-03) is now the oldest supported version, unless CTEs aren't used (WITH clause)
// - Add With data source extension (CTEs) (separate thing for recursive?) <= NEXT
// - get rid of Type requirement in SqlExpression/SqlSelect nodes? let the interpreter actually manage that?
//   ^ stuff like parameter, literal, data field etc. can still have it, as a hint for the interpreter
// - Add function nodes (base => understood by every interpreter + possibility to register custom)
// - Add node tree interpreter
//   ^ this interpreter must be customizable through DB builder
// - Add CREATE/DROP (TEMP)TABLE/INDEX nodes
// - Add INSERT/UPDATE/DELETE nodes
