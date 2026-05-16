# TODO

|        Project         |              Title               |                        Details                         |
|:----------------------:|:--------------------------------:|:------------------------------------------------------:|
|    MessageBroker.*     |           Refinements            |           [link](#messagebroker-refinements)           |
| Computable.Expressions |           Refinements            |       [link](#computableexpressions-refinements)       |
|      Computable.*      |       Math/Physics structs       |        [link](#computable-mathphysics-structs)         |
|           -            |             Terminal             |                   [link](#terminal)                    |
|  Computable.Automata   |     Add Context-free grammar     |                           -                            |
|    DistributedCache    |      Add distributed cache       |               [link](#distributed-cache)               |
|         Sql.*          |           Refinements            |                [link](#sql-refinements)                |
|       Reactive.*       |           Refinements            |             [link](#reactive-refinements)              |
|         Sql.*          |    Add Microsoft SQL support     |                           -                            |
|      Collections       |           Add SkipList           |                           -                            |
|      Dependencies      |           Refinements            |           [link](#dependencies-refinements)            |
|     Reactive.State     | Async validator & change tracker | [link](#reactivestate-async-validator--change-tracker) |
|         Sql.*          |         DbBatch support          |            [link](#sqlcore-dbbatch-support)            |
|        Sql.Core        |          Add JSON nodes          |            [link](#sqlcore-add-json-nodes)             |

### Scribbles:

Core:

- could add expression extensions for generating a lambda from a collection of members and a target
  - would have to be null-aware
- migrate to dotnet10 once most refinements are implemented
  - swap to System.Threading.Lock instead of ExclusiveLock
    - ExclusiveLock may stay for legacy Monitor usage
  - use directory.packages.props + slnx

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

Low priority/probably won't do:
- add CreateQuery, CreateInsertInto, CreateDeleteFrom, CreateUpdate, CreateUpsert extension methods for tables
  - where the source is not a set of parameters but rather another table with matching identity
- add support for some sort of BulkCopy operation
  - targets a single table
  - DataTable as the source...?
  - fallback to normal one-by-one insert if dialect doesn't support bulk copying
  - might be added as a method to ISqlDatabase
- source generators for parameter bindings and queries/statements
- IncludedColumns for IXs
  - for now, only postgresql can use those
  - requires a blocking collection of referenced columns
- support for virtual generated columns in postgresql
  - alter column drop expression doesn't work on them
  - would probably have to drop column => create column with new storage type
  - impacts new identity columns with old virtual storage
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

### Reactive: Refinements

probably won't do:
- Reactive.State, variables aren't thread-safe
  - and naive locking on them could lead to deadlocks due to their event callbacks
  - can probably be resolved by making variables implement their own event sources, sort of inlined, using the same lock etc.

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

Low priority/probably won't do:
- add possibility to disable captive dependency checks for specific implementors
  - might be useful for registrations marked as global
- log warning when global registration overrides another registration of the same type
- factory-based resolutions should have an internal Create method, used for invoking from within an expression resolver
  - currently, each invocation verifies Type.IsInstanceOfType result
  - this is redundant for expression-based resolvers, and it could be optimized away
- AnyKey support:
  - requires custom AnyKey singleton object, so that there are no references to external packages
  - works as a fallback for resolving keyed dependencies
    - dependencies registered under AnyKey will be used, in case the desired one doesn't exist
    - this part is relatively straightforward, complexity similar to FromKeyedServices support
      - impacts closed generic specializations, since they too should verify AnyKey registrations as fallback
  - direct request for dependencies with AnyKey key throws
    - except for IEnumerable<> requests, which should return ALL keyed dependencies of that type
      - except for those registered under AnyKey key
    - this part is more complex due to IEnumerable<>
      - every non-AnyKey keyed registration should be automatically added to relevant AnyKey range of the same dependency type
      - however, dependencies can be registered with AnyKey key, so those non-AnyKey ranges would have to be tracked separately
- better decorator registration support
  - currently, the goto way is to define a keyed locator and register base implementations there
  - while the decorator registration resolves ctor-param/member from that keyed locator
  - instead, range index from the range dependency builder could be used and decorated registration could be marked as excluded from range
  - this, however, would complicate resolver factories extraction etc.
- custom ctor-param/member resolutions using factories don't enable circular dependency checks; they should
- add possibility to disable circular dependency checks in resolvers
- add possibility to track captive dependencies during manual open generic resolver closure and potentially treat them as errors
  - mostly range resolvers are the issue, they currently don't store underlying resolvers, only the expression/delegate
  - this only impacts dynamic parameter/member generic specializations
  - alternative is to do it during container building, since generic dependency builders will know about their open versions etc.
- open generics:
  - support factory-based resolution
    - would require an additional 'Type requestedType' parameter
  - add validation of additional notnull generic constrains on implementors
    - complex, requires working with compiler-generated attributes

### MessageBroker: Refinements

- Allow server to send messages
  - stream class would have to expose a public method, with SenderId = 0
  - SenderId = 0 would have to be supported everywhere, including storage
  - include possibility to route messages only to chosen client(s)
    - along with the message, provide channel ref if not routing
    - otherwise, provide a collection of listener bindings
      - they must all be linked to the same channel
    - a special method with a single listener might be added for easier routing to a single client

Low priority/probably won't do:
- add delete to secondary queue binding
  - fail for primary
- Replace server-side public getters using lock with a single
  - property/method which returns all values using a single lock
  - this might further be converted into stats fetcher/maintenance public surface
- Expose some public methods for fetching memory usage stats etc.
  - should allow to trim unused memory
- Expose better message querying for streams and queues
  - could be useful for some sort of metrics/management app
  - paging, maybe? or expect a buffer and starting position?
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

### Distributed Cache:

- simple, TCP-socket-based distributed cache implementation
- stores byte-array entries by string keys
- allows to configure entry lifetime and if it should be sliding or not
- allows to add/query/check-if-exists/remove/exchange entries
  - compare-exchange, where entry's version is compared?
- supports distributed locks, with timeout
- supports signed int64 counters
- no permanent storage
- probably async/await? or single-threaded processor?
- static configuration which allows to configure default entry lifetime and max cache size
  - probably LRU + TTL cache type
- probably needs to support request/response batching and ping scheduler (see message broker)
- key-related notifications (key-removed, added, etc)
  - probably as the last feature

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

### Sql.Core: Add JSON nodes

Add nodes for JSON column manipulation (read value, set value etc.)
- this heavily depends on all sql providers' support for json

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
