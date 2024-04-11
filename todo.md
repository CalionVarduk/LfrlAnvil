# TODO
|       Project       |                  Title                   |                          Details                          | Requirements |
|:-------------------:|:----------------------------------------:|:---------------------------------------------------------:|:------------:|
|    Dependencies     |            Reader/Writer Lock            |          [link](#dependencies-readerwriter-lock)          |      -       |
|          -          |           Reactive.Scheduling            |                [link](#reactivescheduling)                |      -       |
|   Dependencies.*    |         Dependencies.AspNetCore          |             [link](#dependencies-aspnetcore)              |      -       |
|        Sql.*        |          Positional parameters           |          [link](#sqlcore-positional-parameters)           |      -       |
|          -          |                 Terminal                 |                     [link](#terminal)                     |      -       |
|          -          |               Diagnostics                |                   [link](#diagnostics)                    |      -       |
| Computable.Automata |         Add Context-free grammar         |                             -                             |      -       |
|    Computable.*     |           Math/Physics structs           |          [link](#computable-mathphysics-structs)          |      -       |
|        Sql.*        |         Add support for triggers         |                 [link](#sqlcore-triggers)                 |      -       |
|        Sql.*        |        Add Microsoft SQL support         |                             -                             |      -       |
|     Collections     |               Add SkipList               |                             -                             |      -       |
|   Reactive.State    |     Async validator & change tracker     |  [link](#reactivestate-async-validator--change-tracker)   |      -       |
|        Sql.*        |             DbBatch support              |             [link](#sqlcore-dbbatch-support)              |      -       |
|      Sql.Core       |              Add JSON nodes              |              [link](#sqlcore-add-json-nodes)              |      -       |
|    Dependencies     |         Generic dependency types         |      [link](#dependencies-generic-dependency-types)       |      -       |

### Scribbles:
Sql:
- source generators for queries/statements?
- for parameter binding too, maybe?


### Reactive.Scheduling
project idea:
- combines Reactive.Chrono & Reactive.Queues
- contains sth like IReactiveTimerProvider, caches timers by some sort of key
- contains TimedCache, which runs on a timer & uses an underlying reorderable queue,
each entry can have a different lifetime
- contains schedulers, that run on a timer & use an underlying queue,
can they delegate event handling to a different thread?
- may have an additional reference to the Caching project
- its implementation may drive what Reactive.Scheduling actually consists of

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
- this could use a floating-point time & memory structs for tracking (related to Chrono?)
- also, macro benchmark can track zero-elapsed-time samples
- benchmark itself can have a 'title' property, that describes it and it can have a RunFiltered method + base IBenchmark interface
- Micro benchmark may be split into a separate step, if done at all, focus on extracting things from Core.Diagnostics

### Dependencies: Reader/Writer Lock
- instead of 'lock', there should be a lot more reading (resolving dependencies) then writing (creating scopes)
- this may be tricky for singleton, scoped singleton or scoped dependencies
  - since their creation & caching needs to be thread-safe & that's a write operation

### Dependencies: Generic dependency types
- add support for open generic constructable dependency types
- can use only auto-discovered ctor or explicit ctor from that open generic type or shared open generic implementor
- each generic dependency could provide partial generic parameters resolution
- e.g. Implementor<T, U> : IDependency<T>, T will be filled in automatically, but U cannot be resolved based on the interface alone
- the functionality could allow to provide a concrete type to use as a substitution for the U type

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

### Reactive.State: Async validator & change tracker
- extract VariableValidator & VariableChangeTracker(?) to separate interfaces
- this could potentially allow for more flexibility & for easier async validation?
- or add a separate IAsyncVariable stack, since CancellationToken should be supported

### Sql.Core: Triggers
- add support for basic(?) triggers
- they would accept a batch node as their body
- body needs to be scanned for referenced db objects & proper references need to be registered
- there would be no support for branching statements or local variables
  - this could be worked around with raw statement nodes, but that's unsafe
- limitation is mostly due to sqlite, that doesn't support branching
  - however, they can have a condition, so maybe that's the way to do it?
  - do other sql providers support conditional triggers?
    - I guess a simple IF could be added in their case
- sqlite has another limitation: CTEs inside triggers are not supported
  - but that can't be helped
- only FOR EACH ROW triggers would be supported, with BEFORE/AFTER + INSERT/UPDATE/DELETE
- triggers could be registered as table constraints

### Sql.Core: Add JSON nodes
Add nodes for JSON column manipulation (read value, set value etc.)
- this heavily depends on all sql providers' support for json

### Sql.Core: Add Visitor for node CLR type extraction
Add sql node visitor that allows to extract node's type (+ add Type to ViewDataField)
- this is a LOT of work, since every dialect needs to define its own visitor
- and those visitors must handle all node types
- not sure how necessary it is to have this functionality, since any visitor usage is expensive
- and it requires intimate knowledge of each db type system
  - type systems have to be basically re-implemented in those visitors which feels redundant

### Dependencies: AspNetCore
- Add adapter layer between lfrlanvil & aspnetcore service providers
- so that lfrlanvil provider can be used as service container in aspentcore apps

### Sql.Core: Positional parameters
- Add support for positional parameters in parameter binder factory & in sql nodes
- each sql parameter node will probably still require some sort of name
- in order to identify the parameter & extract its optional position
- parameter binder factory, along with context instance, will be able to translate the nodes
- parameter binder factory will also contain type-erased versions of binders
- that just accept an array of System.Object as parameter values

### Sql.Core: DbBatch support
- Add support for DbBatch and its commands
- Awkward to implement, since there is no shared interface between DbCommand & DbBatchCommand
- statement/query execution can be done already, with a trivial bit of boiler-plate
- same situation with multi reading: DbBatch allows to create a reader, which can be linked to multi reader
- DbBatchCommand parameters are an issue:
  - param binder factory spews out delegates that accept IDbCommand instances
  - support for DbBatch would require either accepting IDataParameterCollection instance as a binder argument
    - which leads to an issue: how to create an actual parameter instance?
    - methods for parameter creation can only be found in commands, not parameter collections
    - an alternative would be to somehow scan the generic DbConnection type
    - in order to find correct DbParameter type? seems finicky, but there may not be a better way to do this
  - or a separate factory method would need to be created, that supports DbBatchCommand instances
- another minor issue: how to share parameter instances between multiple DbBatchCommand instances
- that belong to the same DbBatch? dealing with that may be a micro-optimization, but it may "add up"
- maybe a separate binder factory method for the whole DbBatch...?
- this would need some sort of way to define to which db batch commands all of the parameters should be attached to
- either way, quite a bit of work
