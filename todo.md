# TODO
| Priority |       Project       |                            Title                            |                                   Details                                   |                    Requirements                    |
|:--------:|:-------------------:|:-----------------------------------------------------------:|:---------------------------------------------------------------------------:|:--------------------------------------------------:|
|    1     |        Sql.*        |                         PostgreSql                          |                           [link](#sql-postgresql)                           |                         -                          |
|    2     |      Sql.Core       |                       Add JSON nodes                        |                       [link](#sqlcore-add-json-nodes)                       |                         -                          |
|    3     |      Sql.Core       |                 Add Attach/Detach DB nodes                  |                 [link](#sqlcore-add-attachdetach-db-nodes)                  |                         -                          |
|    4     |          -          |                     Reactive.Scheduling                     |                         [link](#reactivescheduling)                         |                         -                          |
|    5     |   Dependencies.*    |                   Dependencies.AspNetCore                   |                      [link](#dependencies-aspnetcore)                       |                         -                          |
|    6     |          -          |                          Terminal                           |                              [link](#terminal)                              |                         -                          |
|    7     |          -          |                         Diagnostics                         |                            [link](#diagnostics)                             |                         -                          |
|    8     |          -          |                    Diagnostics.Terminal                     |                        [link](#diagnosticsterminal)                         | [Terminal](#terminal), [Diagnostics](#diagnostics) |
|    9     |    Dependencies     |                     Reader/Writer Lock                      |                   [link](#dependencies-readerwriter-lock)                   |                         -                          |
|    10    |    Computable.*     |                    Math/Physics structs                     |                   [link](#computable-mathphysics-structs)                   |                         -                          |
|    11    | Computable.Automata |                  Add Context-free grammar                   |                                      -                                      |                         -                          |
|    12    |     Collections     |                        Add SkipList                         |                                      -                                      |                         -                          |
|    13    |      Sql.Core       |          Add Visitor for node CLR type extraction           |          [link](#sqlcore-add-visitor-for-node-clr-type-extraction)          |                         -                          |
|    14    |      Sql.Core       |               Add Custom node tree validators               |              [link](#sqlcore-add-custom-node-tree-validators)               |                         -                          |
|    15    |    Dependencies     |                  Generic dependency types                   |               [link](#dependencies-generic-dependency-types)                |                         -                          |
|    16    |      Sql.Core       |                  Pin/Unpin builder methods                  |                  [link](#sqlcore-pinunpin-builder-methods)                  |                         -                          |
|    17    |    Dependencies     |                    Generic builder types                    |                 [link](#dependencies-generic-builder-types)                 |                         -                          |
|    18    |   Reactive.State    |                       Extension ideas                       |                   [link](#reactivestate-extension-ideas)                    |                         -                          |

### Scribbles:
- Chrono:
  - TimedCache here? old entries would be removed when new entry is added, so no timers required
  - may be better to create a new Chrono.Collections project?


- Dependencies:
  - scopes & their attachment to thread may be an issue
  - when dealing with async code that after await doesn't go back to the original context (e.g. default aspnetcore)
  - and then a child scope spawn attempt is made, now all of a sudden from a parent scope attached to another thread
  - it should always be possible to create a child scope
  - parent scopes should be able to support multiple child scopes, as an implicit linked list
  - ^ child scopes will have a pointer to the 'next sibling' scope ('first' node is the last created child scope?)
  - ^ this shouldn't require a doubly linked list
  - parent scope disposal cleans up all child scopes
  - the only thing to 'work out' is attachment of created scope as the active thread scope
  - ^ this may also have to be a linked list...


- PostgreSql:
  - https://www.postgresql.org/docs/current/runtime-config-compatible.html#GUC-STANDARD-CONFORMING-STRINGS
  - ^ standard_conforming_strings, must be 'on'
  - ^ also, backslash_quote, must be 'off'


- Sql.Core:
  - extract type definition constants (format etc.) to dialect XHelpers static classes
  - support for positional parameters?
    - node interpreter context would have to track it
    - this would also require changes to parameter binder factory
  - RETURNING for INSERT/DELETE/UPDATE:
    - postgresql: ok, RETURNING is always the last 'thing' in statement
    - sqlite: ok, however DELETE/UPDATE with ORDER-BY/LIMIT require RETURNING directly after WHERE
      - also, sqlite supports this since 2021, so this would require an option for node interpreter
      - also, RETURNING cannot be used in CTEs (postgresql supports this)
      - this could still be useful
    - mysql: NO SUPPORT! this is probably a deal breaker then
  - take a look at ado.net dbbatch & its commands
  - node replacer: visitor that allows to 'modify' nodes (like linq expression visitor)
    - low priority
    - could be fun to implement
    - requires a lot of testing


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
- instead of 'lock', there should be a lot more reading (resolving dependencies) then writing (creating scopes)

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

### Sql: PostgreSql
Implement Sql.Core for PostgreSql

### Sql.Core: Add JSON nodes
Add nodes for JSON column manipulation (read value, set value etc.)
- this heavily depends on all sql providers' support for json

### Sql.Core: Add Visitor for node CLR type extraction
- Add sql node visitor that allows to extract node's type (+ add Type to ViewDataField)

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
