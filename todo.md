# TODO
| Priority |       Project       |                             Title                             |                                   Details                                   |                       Requirements                        |
|:--------:|:-------------------:|:-------------------------------------------------------------:|:---------------------------------------------------------------------------:|:---------------------------------------------------------:|
|    1     |      Sql.Core       |                    Add Statement compilers                    |                  [link](#sqlcore-add-statement-compilers)                   |                             -                             |
|    2     |      Sql.Core       |        ISqlDatabaseBuilder.AddRawStatement improvement        |       [link](#sqlcore-isqldatabasebuilderaddrawstatement-improvement)       | [AddStatementCompilers](#sqlcore-add-statement-compilers) |
|    3     |        Sql.*        |                             MySql                             |                             [link](#sql-mysql)                              |                             -                             |
|    4     |      Sql.Core       |  Create generic abstractions based on Sqlite & MySql objects  | [link](#sqlcore-create-generic-abstractions-based-on-sqlite--mysql-objects) |                    [MySql](#sql-mysql)                    |
|    5     |      Sql.Core       |                Add DateTime related functions                 |               [link](#sqlcore-add-datetime-related-functions)               |                             -                             |
|    6     |       Sqlite        |                 Add node interpreter options                  |                [link](#sqlite-add-node-interpreter-options)                 |                             -                             |
|    7     |      Sql.Core       |                        Add Upsert node                        |                      [link](#sqlcore-add-upsert-node)                       |                             -                             |
|    8     |        Sql.*        |                          PostgreSql                           |                           [link](#sql-postgresql)                           |                             -                             |
|    9     |      Sql.Core       |                  Add Attach/Detach DB nodes                   |                 [link](#sqlcore-add-attachdetach-db-nodes)                  |                             -                             |
|    10    |       Sqlite        |                       Improvement ideas                       |                      [link](#sqlite-improvement-ideas)                      |                             -                             |
|    11    |          -          |                      Reactive.Scheduling                      |                         [link](#reactivescheduling)                         |                             -                             |
|    12    |   Dependencies.*    |                    Dependencies.AspNetCore                    |                      [link](#dependencies-aspnetcore)                       |                             -                             |
|    13    |          -          |                           Terminal                            |                              [link](#terminal)                              |                             -                             |
|    14    |          -          |                          Diagnostics                          |                            [link](#diagnostics)                             |                             -                             |
|    15    |          -          |                     Diagnostics.Terminal                      |                        [link](#diagnosticsterminal)                         |    [Terminal](#terminal), [Diagnostics](#diagnostics)     |
|    16    |    Dependencies     |                      Reader/Writer Lock                       |                   [link](#dependencies-readerwriter-lock)                   |                             -                             |
|    17    |    Computable.*     |                     Math/Physics structs                      |                   [link](#computable-mathphysics-structs)                   |                             -                             |
|    18    | Computable.Automata |                   Add Context-free grammar                    |                                      -                                      |                             -                             |
|    19    |     Collections     |                         Add SkipList                          |                                      -                                      |                             -                             |
|    20    |      Sql.Core       |           Add Visitor for node CLR type extraction            |          [link](#sqlcore-add-visitor-for-node-clr-type-extraction)          |                             -                             |
|    21    |      Sql.Core       |                Add Custom node tree validators                |              [link](#sqlcore-add-custom-node-tree-validators)               |                             -                             |
|    22    |    Dependencies     |                   Generic dependency types                    |               [link](#dependencies-generic-dependency-types)                |                             -                             |
|    23    |      Sql.Core       |                   Pin/Unpin builder methods                   |                  [link](#sqlcore-pinunpin-builder-methods)                  |                             -                             |
|    24    |    Dependencies     |                     Generic builder types                     |                 [link](#dependencies-generic-builder-types)                 |                             -                             |
|    25    |   Reactive.State    |                        Extension ideas                        |                   [link](#reactivestate-extension-ideas)                    |                             -                             |

### Reactive.Scheduling
project idea:
- combines Reactive.Chrono & Reactive.Queues
- contains sth like IReactiveTimerProvider, caches timers by some sort of key
- contains TimedCache, which runs on a timer & uses an underlying reorderable queue,
each entry can have a different lifetime
- contains schedulers, that run on a timer & use an underlying queue,
can they delegate event handling to a different thread?

### Terminal
project idea:
- contains extensions to System.Console
- write colored fore/back-ground (with temp IDisposable swapper)
- write table
- prompt, switch etc. for user interaction

### Diagnostics
project idea:
- move most of Core.Diagnostics (except for stopwatch-related stuff & memory size) to that project
- separate Benchmark into Macro & Micro benchmarks, macro stays pretty much the same as it is now
- micro will attempt to calculate during warmup how many operations can it perform in order to be done in X amount of time
- this could use a floating-point time & memory structs for tracking
- also, macro benchmark can track zero-elapsed-time samples
- benchmark itself can have a 'title' property, that describes it and it can have a RunFiltered method + base IBenchmark interface

### Diagnostics.Terminal
project idea:
- extends Benchmarking project with Terminal project capabilities (writes benchmark "events" to console)

### Dependencies: Reader/Writer Lock
instead of 'lock', there should be a lot more reading (resolving dependencies) then writing (creating scopes)

### Dependencies: Generic dependency types
- add support for open generic constructable dependency types
- can use only auto-discovered ctor or explicit ctor from that open generic type or shared open generic implementor
- each generic dependency could provide partial generic parameters resolution
- e.g. Implementor<T, U> : IDependency<T>, T will be filled in automatically, but U cannot be resolved based on the interface alone
- the functionality could allow to provide a concrete type to use as a substitution for the U type

### Dependencies: Generic builder types
dedicated generic builder types for generic extension methods:
- this would allow to provide a factory that returns T instead of object
- also, this would possibly allow to set callback for disposal strategy that accepts T instead of object
- simple QoL & DevExp improvements
- LOW PRIORITY, needs a lot of annoying changes for little gain, setup can still go wrong
- since, e.g. providing an explicit ctor should still be available, which can't really be done 'generically'
- at least not without complete interface rework
- e.g. FromCtor<T>(Func<ConstructorInfo, bool> selector), where selector is fed constructors from the provided T type
- however, this would have its own drawbacks & annoyances
- e.g. why need to filter at all if desired ctor info ref is already assigned to a variable? feels inefficient

### Computable: Math/Physics structs
ideas for other LfrlAnvil.Computable projects:
- Complex struct
- Structures representing values with units
  - values can be generic (.NET7 static interface members would be very useful, so that the structures can have math operators)
  - units can be convertible if they belong to the same 'category'
  - e.g. weight units ('t', 'kg', 'g' etc.)
  - there could be a base interface for units IUnitCategory with properties 'Symbol' & 'ConversionRatio'
  - WeightUnit class would implement IUnitCategory & declare static readonly fields like Ton, Kilogram, Gram etc. (basically a smart enum)
  - this could actually be implemented for decimal type, for now
  - and when the decision is made to move to .NET7, then add support for generic value types

### Reactive.State: Extension ideas
- extract VariableValidator & VariableChangeTracker to separate interfaces
- this could potentially allow for more flexibility & for easier async validation?

### Sql.Core: Add Attach/Detach DB nodes
- Add attach/detach DB nodes
- Add optional explicit DB name to table/view record set nodes (cross-DB queries), in SqlSchemaObjectName?
- postgresql will be awkward, since dblink would have to be used,
  - which requires that all result set column names & types are defined explicitly
  - https://www.postgresql.org/docs/current/postgres-fdw.html

### Sql.Core: Add statement compilers
Add statement 'compilers', that can work with ado.net objects and prepare a delegate that reads/changes db data
- based on sql tree metadata (from the interpreter context)
- supported statements: select, select many (single batch), insert into, update, delete from
- queries will allow to specify an output row type & optional input parameters type
- insert into, update & delete from will allow to specify an optional input parameters type
- this can later be extended to work with table db objects, that could automatically verify input parameters type's properties
- and link them with correct table columns
- & update could accept any subset of columns to allow multiple 'variations' for the same table
- also, add generic 'repositories'? these would allow to easily link a db table (or view) with an entity CLR type

### Sql.Core: ISqlDatabaseBuilder.AddRawStatement improvement
ISqlDatabaseBuilder.AddRawStatement should accept an expression tree (array of, internally will create a batch node) + parameters

### Sql: MySql
Implement Sql.Core for MySql

### Sql: PostgreSql
Implement Sql.Core for PostgreSql

### Sql.Core: Create generic abstractions based on Sqlite & MySql objects
- SqliteColumnTypeDefinition is a candidate for moving to Sql.Core, as a generic class, where T is an sql data type
- SqliteIndexColumnBuilder is a candidate for moving to Sql.Core, as a generic class, where T is a column builder type
  - honestly, pretty much everything could be moved to Sql.Core, as generic abstract classes, that allow to override default behaviors
  - and define new ones

### Sql.Core: Add DateTime related functions
Add datetime related functions? kind of depends on other sql dialects
- things like Add or GetMonth etc.

### Sqlite: Add node interpreter options
Add sqlite interpreter options, that can be enabled e.g. based on server version
- these options can e.g. allow to enable STRICT tables, UPDATE FROM handling,
- row value comparison in complex DELETE/UPDATE with multiple column PKs etc.

### Sql.Core: Add Upsert node
Add upsert node? or sth more akin to insert or update on duplicate key
- this could be an extension to insert node, rather than sth completely new?
- this, however, would move the oldest supported version for Sqlite to 3.24.0 (2018-06-04), unless upsert isn't used
- or an alternative statement can be constructed, that may not be as efficient, but will work pretty much like an upsert

### Sqlite: Improvement ideas
- Add column script can be handled with ALTER TABLE
  - however, situation where column with this name was also removed must be treated as a full reconstruction
  - this may need to be handled anyway for MySQL and/or PostgreSQL - leave it as-is, for now
- Support for Sqlite COLLATION in added columns? let's see how other SQL providers handle that
- ON CONFLICT ROLLBACK instead of ABORT? https://www.sqlite.org/lang_conflict.html
- STRICT table option is supported in v3.37.0 (2021-11-27), disabled for now
- more ReferenceBehavior options?
- WITHOUT ROWID is supported in v3.8.2 (2013-12-06), this is the oldest supported version
- update: v3.8.3 (2014-02-03) is now the oldest supported version, unless CTEs aren't used (WITH clause)

### Sql.Core: Add Visitor for node CLR type extraction
Add sql node visitor that allows to extract node's type (+ add Type to ViewDataField)

### Sql.Core: Add Custom node tree validators
- Add possibility to register custom view/default-value/index-filter validators in db builders

### Dependencies: AspNetCore
- Add adapter layer between lfrlanvil & aspnetcore service providers
- so that lfrlanvil provider can be used as service container in aspentcore apps

### Sql.Core: Pin/Unpin builder methods
- a loose idea
- allow to Pin/Unpin db object builders for some reason
- Pin could be associated with object removal and/or its modification
- this would allow to manually mark a db object as read-only, pretty much
- e.g. if a custom trigger is created & consumer wants to be extra safe that referenced tables/columns/views etc. won't be removed/modified
- this could also replace current Referencing* properties
- however, sometimes even though an object is 'locked' for removal, it can still be removed when its parent is removed
- so, keep that in mind
