using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo( "LfrlAnvil.Core.Tests" )]

// TODO:
// - Add possibility to ignore record set name in data field node
//   ^ may be important for partial indexes & updates in the form of UPDATE T SET X = X + 1 (instead of UPDATE T SET X = T.X + 1)
//   ^ seems to work for Sqlite, at least => leave it for now, let's see how other db providers handle that
// - Add check constraints? as expression trees <= NEXT NEXT
// - ISqlDatabaseBuilder.AddRawStatement should accept an expression tree (array of, internally will create a batch node) + parameters
// - Add column script can be handled with ALTER TABLE
//   ^ however, situation where column with this name was also removed must be treated as a full reconstruction
//   ^ this may need to be handled anyway for MySQL and/or PostgreSQL - leave it as-is, for now
// - SqliteColumnTypeDefinition is a candidate for moving to Sql.Core, as a generic class, where T is an sql data type
// - SqliteIndexColumnBuilder is a candidate for moving to Sql.Core, as a generic class, where T is a column builder type
//   ^ honestly, pretty much everything could be moved to Sql.Core, as generic abstract classes, that allow to override default behaviors
//   ^ and define new ones
// - Support for Sqlite COLLATION in added columns? let's see how other SQL providers handle that
// - ON CONFLICT ROLLBACK instead of ABORT? https://www.sqlite.org/lang_conflict.html
// - STRICT table option is supported in v3.37.0 (2021-11-27), disabled for now
// - more ReferenceBehavior options?
// - WITHOUT ROWID is supported in v3.8.2 (2013-12-06), this is the oldest supported version
// - update: v3.8.3 (2014-02-03) is now the oldest supported version, unless CTEs aren't used (WITH clause)
// - Add upsert node? or sth more akin to insert or update on duplicate key
//   ^ this could be an extension to insert node, rather than sth completely new
//   ^ this, however, would move the oldest supported version to 3.24.0 (2018-06-04), unless upsert isn't used
// - Add possibility to register custom view/default-value/index-filter validators in db builders
// - Add window function nodes <= NEXT
// - Add datetime related functions?
// - Add attach/detach DB nodes
// - Add optional explicit DB name to table/view record set nodes (cross-DB queries)
// - Make DB builder use SQL node tree? This could unify a lot of SQL statement building by moving it all to interpreters <= NOW
//   ^ it will cause a bit of an overhead, due to having to create a graph and then interpret it
//   ^ but it happens only once per version, when it's actually applied to the DB, so it should be fine
// - Add sql node visitor that allows to extract node's type (+ add Type to ViewDataField)
// - Add statement 'compilers', that can work with ado.net objects and prepare a delegate that reads/changes db data
//   ^ based on sql tree metadata (from the interpreter context)
//   ^ supported statements: select, select many (single batch), insert into, update, delete from
//   ^ queries will allow to specify an output row type & optional input parameters type
//   ^ insert into, update & delete from will allow to specify an optional input parameters type
//   ^ this can later be extended to work with table db objects, that could automatically verify input parameters type's properties
//   ^ and link them with correct table columns
//   ^ + update could accept any subset of columns to allow multiple 'variations' for the same table
// - Add sqlite interpreter options, that can be enabled e.g. based on server version
//   ^ these options can e.g. allow to enable STRICT tables, UPDATE FROM handling,
//   ^ row value comparison in complex DELETE/UPDATE with multiple column PKs etc.
