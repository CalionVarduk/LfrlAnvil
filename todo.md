# TODO

|        Project         |              Title               |                        Details                         |                  Requirements                  |
|:----------------------:|:--------------------------------:|:------------------------------------------------------:|:----------------------------------------------:|
|         Sql.*          |           Refinements            |                [link](#sql-refinements)                |                       -                        |
|       Reactive.*       |           Refinements            |             [link](#reactive-refinements)              |                       -                        |
|    MessageBroker.*     |           Refinements            |           [link](#messagebroker-refinements)           |                       -                        |
|      Dependencies      |           Refinements            |           [link](#dependencies-refinements)            |                       -                        |
|      Dependencies      |     Generic dependency types     |     [link](#dependencies-generic-dependency-types)     |                       -                        |
|     Dependencies.*     |  Dependencies.ServiceProviders   |            [link](#dependencies-aspnetcore)            | [link](#dependencies-generic-dependency-types) |
| Computable.Expressions |           Refinements            |       [link](#computableexpressions-refinements)       |                       -                        |
|      Computable.*      |       Math/Physics structs       |        [link](#computable-mathphysics-structs)         |                       -                        |
|           -            |             Terminal             |                   [link](#terminal)                    |                       -                        |
|  Computable.Automata   |     Add Context-free grammar     |                           -                            |                       -                        |
|         Sql.*          |     Add support for triggers     |               [link](#sqlcore-triggers)                |                       -                        |
|         Sql.*          |    Add Microsoft SQL support     |                           -                            |                       -                        |
|      Collections       |           Add SkipList           |                           -                            |                       -                        |
|     Reactive.State     | Async validator & change tracker | [link](#reactivestate-async-validator--change-tracker) |                       -                        |
|         Sql.*          |         DbBatch support          |            [link](#sqlcore-dbbatch-support)            |                       -                        |
|        Sql.Core        |          Add JSON nodes          |            [link](#sqlcore-add-json-nodes)             |                       -                        |

### Scribbles:

Core:

- could add expression extensions for generating a lambda from a collection of members and a target
  - would have to be null-aware
- migrate to dotnet10 once most refinements are implemented
  - swap to System.Threading.Lock instead of ExclusiveLock
    - ExclusiveLock may stay for legacy Monitor usage
  - use directory.packages.props

### Terminal

project idea:

- contains extensions to System.Console
- write colored fore/back-ground (with temp IDisposable swapper)
- write table
- prompt, switch etc. for user interaction
- may add some sort of terminal script runner?
  - handle output
  - handle args
  - handle exit code
  - handle graceful cancellation
  - handle start/end/elapsed time tracking

### Sql: Refinements

- add extension methods for table node:
  - create parameterized insert statement
    - inserts a row from parameters using column names
    - optional filter delegate, which can exclude columns
    - optional parameter settings for mapping
  - create parameterized delete statement
    - deletes a row by PK parameters
    - optional parameter settings for mapping
  - create parameterized update statement
    - updates a row by PK parameters with non-PK columns being updated from parameters
    - optional filter delegate, which can exclude assignments
    - optional parameter settings for mapping
  - create parameterized upsert statement
    - upserts a row from parameters using column names
    - optional filter delegates, which can exclude insert columns and/or update assignments
    - optional parameter settings for mapping
  - create temp table
    - copies table's schema
    - requires explicit name
    - optional filter delegate, which can exclude columns
    - optional setting if PK should be copied or ignored

Low priority/probably won't do:
- source generators for parameter bindings and queries/statements
- IncludedColumns for IXs
  - for now, only postgresql can use those
  - requires a blocking collection of referenced columns
- support for virtual generated columns in postgresql
  - alter column drop expression doesn't work on them
  - would probably have to drop column => create column with new storage type
  - impacts new identity columns with old virtual storage

### Reactive: Refinements

- use ValueTaskDelaySource in ReactiveTimer
- refactor schedulers to use ValueTaskDelaySource
  - also, their disposal should wait for disposal of all currently running tasks
  - with some timeout
  - add task container pooling?
  - add worker task pool, for full async and efficient memory usage?

Might do:
- extension methods for converting between event sources and async enumerables
- after upgrade to dotnet10, take a look at Source vs ConcurrentSource etc.
  - lock everything? decorators may be a bit of an issue

### Computable.Expressions: Refinements

- Add block expressions, with their own local variable scopes, starts with {, ends with }
  - last statement is the result
  - macros stay global
- Add enum support, maybe?
  - constants can be registered manually
  - maybe an extension method which registers specific enum-related helpers?
    - register all members as [EnumName]:[MemberName] constants
    - register [EnumName]:Contains, [EnumName]:ContainsName
    - register eq/cmp operators
    - for flag enums, register binary operators and [EnumName]:HasAnyFlag and [EnumName]:HasAllFlags

### Dependencies: Refinements

- consider adding some sort of Decorate method/extension
- add scope factory
- async scope disposal/support for IAsyncDisposable
  - scope will implement both IDisposable and IAsyncDisposable
  - default should be async usage
  - when registering dependency, async disposal delegate may be provided
- better disposable scope instances tracking
  - may have to track disposables in a hashset as well, for by-ref comparison
  - disposal should still happen in reverse order, based on first resolution
  - e.g. register Foo as disposable implementor, IBar as dependency using Foo
    - and IQux as dependency using factory method which resolves IBar
- hosted services registration?
- options registration?

### MessageBroker: Refinements

- Add possibility to change publisher's stream and listener's queue during rebinding
  - include client-defined flag which states if this is desired or not
  - publishers are straightforward
  - listeners will need a collection of queue-binding objects
    - in order to not lose any messages
    - queues and their messages will be linked to queue-bindings
    - queue-bindings will store a copy of listener settings
      - may either be static or be updated on every listener rebind
      - NOTE: listeners may be refactored to no longer use interlocked counters
        - and just use a lock for simplicity
        - also, currently, expired dead letter for inactive listeners
        - will cause the queue processor to essentially spin-wait, so fix that too
    - queue-bindings will need to track how many messages reference them
    - and get disposed automatically when the counter reaches 0
    - message notifications will need an additional queue-id in their header
      - important for ack/nack
- Replace server-side public getters using lock with a single
  - property/method which returns all values using a single lock
  - this might further be converted into stats fetcher/maintenance public surface
- Expose some public methods for fetching memory usage stats etc.
  - should allow to trim unused memory
- Expose better message querying for streams and queues
  - could be useful for some sort of metrics/management app
  - paging, maybe? or expect a buffer and starting position?
- Allow server to send messages
  - stream class would have to expose a public method, with SenderId = 0
  - SenderId = 0 would have to be supported everywhere, including storage
  - include possibility to route messages only to chosen client(s)
    - along with the message, provide channel ref if not routing
    - otherwise, provide a collection of listener bindings
      - they must all be linked to the same channel
    - a special method with a single listener might be added for easier routing to a single client

Low priority/probably won't do:
- Storage could be more dynamic, not just invoked on server dispose
  - would improve resilience against unexpected errors, power outages etc.
- in-memory queue messages could have an upper bound
  - exceeding that limit would cause further messages to be stored on the disk
  - append-only log files could be used, probably
- in-memory stream messages could have an upper bound
  - they might have to be remade into some sort of LRU cache with random access
  - upper limit might either be the number of messages or total size of cached messages
  - if message data does not exist in-memory, then load it from the disk
  - messages would have to be stored in multiple files, fixed size
    - so that none of them becomes too large
  - each storeKey would have to always deterministically resolve to the same file
    - may require some additional metadata file which stores message positions in a file
    - entries in that file would have to be of fixed size
    - so that only one such file can exist, and position for each storeKey is easily computable
    - might add a separate LRU cache for message metadata which can store more entries
      - each entry would be pretty small, so a larger cache should be fine
      - this would allow to somewhat optimize message metadata access
- inactive clients shouldn't store their queue data in-memory
  - requires some sort of swap of underlying queue tasks during disconnect/reconnect
  - may require async lock usage for the swap
- request-reply channels:
  - may have 0 or more listeners
  - all listeners will have to respond for the requestor to continue, with timeouts
  - always ephemeral

### Dependencies: Generic dependency types

- add support for open generic constructable dependency types
- can use only auto-discovered ctor or explicit ctor from that open generic type or shared open generic implementor
- each generic dependency could provide partial generic parameters resolution
- e.g. Implementor\<T, U\> : IDependency\<T\>, T will be filled in automatically, but U cannot be resolved based on the interface alone
- the functionality could allow to provide a concrete type to use as a substitution for the U type
- also, add methods to get type-erased keyed locators (important for ServiceProviders project)
- also, check if open generics based on factories should be allowed

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
- requires possibility to register open generics

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
