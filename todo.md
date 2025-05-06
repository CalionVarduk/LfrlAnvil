# TODO

|       Project       |              Title               |                        Details                         |                    Requirements                    |
|:-------------------:|:--------------------------------:|:------------------------------------------------------:|:--------------------------------------------------:|
|   MessageBroker.*   |             Messages             |            [link](#messagebroker-messages)             |                                                    |
|   MessageBroker.*   |   Max Packet Length & Batching   |   [link](#messagebroker-max-packet-length--batching)   |          [link](#messagebroker-messages)           |
|   MessageBroker.*   |            Permanence            |           [link](#messagebroker-permanence)            | [link](#messagebroker-max-packet-length--batching) |
|   MessageBroker.*   |      Request-Reply Channel       |      [link](#messagebroker-request-reply-channel)      |         [link](#messagebroker-permanence)          |
|   MessageBroker.*   |        Subscriber Filter         |        [link](#messagebroker-subscriber-filter)        |    [link](#messagebroker-request-reply-channel)    |
|   MessageBroker.*   |        Server Maintenance        |       [link](#messagebroker-server-maintenance)        |      [link](#messagebroker-subscriber-filter)      |
|    Dependencies     |     Generic dependency types     |     [link](#dependencies-generic-dependency-types)     |                         -                          |
|   Dependencies.*    |  Dependencies.ServiceProviders   |            [link](#dependencies-aspnetcore)            |   [link](#dependencies-generic-dependency-types)   |
|    Computable.*     |       Math/Physics structs       |        [link](#computable-mathphysics-structs)         |                         -                          |
|        Sql.*        |     Add support for triggers     |               [link](#sqlcore-triggers)                |                         -                          |
|          -          |             Terminal             |                   [link](#terminal)                    |                         -                          |
| Computable.Automata |     Add Context-free grammar     |                           -                            |                         -                          |
|        Sql.*        |    Add Microsoft SQL support     |                           -                            |                         -                          |
|     Collections     |           Add SkipList           |                           -                            |                         -                          |
|   Reactive.State    | Async validator & change tracker | [link](#reactivestate-async-validator--change-tracker) |                         -                          |
|        Sql.*        |         DbBatch support          |            [link](#sqlcore-dbbatch-support)            |                         -                          |
|      Sql.Core       |          Add JSON nodes          |            [link](#sqlcore-add-json-nodes)             |                         -                          |

### Scribbles:

Other:

- SparseSet/SparseDictionary?
  - probably not, SparseListSlim already sort of works like that
- SegmentedSparseListSlim?
  - segments of constant 2^n length
  - segmentIndex = index >> n
  - elementIndex = index & (n-1)
  - segments are created on-demand
  - may require cache of segments
- BitArraySlim (changing bit at index N ensures length of at least N+1)
- InlineLinkedList
  - linked list struct optimized for 0 or 1 elements (no-alloc)
- MemoryPool:
  - add Split method to token, which splits node in two & returns a new token with elements belonging to the first part
  - add trimSide parameter to token's SetLength method, which allows to specify which part (start or end)
    - of the buffer should be returned to the pool when length is decreased, equal to 'end' by default (current behavior)

Sql:

- source generators for queries/statements?
- for parameter binding too, maybe?
- IncludedColumns for IXs? simple blocking link to SqlColumnBuilder (low priority)
    - from implemented dialects only postgresql supports this

MessageBroker:

- redo logging
- dedicated IMessageBroker*Logger interfaces (?)
- dedicated methods for each possible event (?)
    - skip interface maybe, allow to define multiple callbacks
    - however, there can't be too many of them...
    - define some event groups, that could be handled by the same callback
    - for example: network events, obj creation/removal events, message processing events
    - it should be possible to link chain of events together via context-id, if necessary
    - some events may have to be emitted under active locks
        - misuse of handlers may lead to deadlocks - make sure to warn about it
        - this mostly concerns obj removal (queue/channel) from within the handler on the server side
        - with new events (like creating-publisher etc.) the order of events and some overlap/swapping may not matter that much
        - since the actual order may be verified by checking timestamp-of-first-event-in-context
        - so event emitting under active lock might be possible to be kept to bare minimum
- refactor packet read/write & payload error emitting? there's a lot of copy-pasta (wait for packet batching)
- change endpoint values to be as sequential as possible (minor switch optimization)
- separate synchronous enqueue-write operation from asynchronous write-and-wait-for-response operation
    - consumption example: client.Enqueue(...).WaitForResponseAsync()
    - make sure that Enqueue already starts the underlying process
    - so that fire-and-forget misuse won't cause the whole write queue to be blocked
    - or, explain that this is a low-level API, and any issues caused by misuse are on the consumer
    - in that case, there may have to exist some sort of TryEnqueue method
    - since all operations at first validate e.g. client state and may throw
    - or Enqueue itself returns a Result<EnqueuedOperation>, which would allow the consumer to easily react to such a scenario
- add some basic memory pool tracking & possibility to trim excess
- rename channel binding to MessageBrokerChannelPublisherBinding
- rename subscription to MessageBrokerChannelListenerBinding
- apply renaming to collections, requests, responses etc. (Publishers/Listeners will suffice)
  - e.g. BindPublisher/BindListener
- refactor MessageContextQueue on both client and server side
  - they outlived their usefulness, and are in need of some refactoring/splitting into different structs
  - since now I have a better grasp on how they're used
- add message's sender/stream name synchronization via notifications to the client
  - probably requires 64bit GlobalId for each such object
  - GlobalId must be sent with each relevant packet header (what about channels?)
  - each remote-client on server side will cache sent names
  - if name has not been sent, then send it in separate notification, before the actual message
  - but make writer source acquisition is part of the same client lock
  - so that nothing else can interject itself in between, or that the name can't be sent twice
  - cache will be a segmented sparse list slim of inline linked lists
    - cache key is object's id
    - actual obj will then be found in the linked list by its global id
    - cache may either have limited number of entries or entry lifetime, or both
    - this will simplify element removal, since there won't be any need for listening to some additional events
    - especially since sender clients can be disposed before messages are sent to receiver clients
- message ack/nack, followed by redelivery (with config via listener), followed by retry (with config via listener)
- custom (network) stream interface, that is capable of leveraging Socket's send method with multiple buffer segments
  - server-side only, for now
  - must support: read to single buffer, write single buffer, write two buffers
  - custom decorators can be defined to extend the behavior
  - add some sort of system.io.stream compatibility adapter, but this will force using 'write single buffer' method
    - by copying two buffers into one
  - this is to combat message copying from stream to subscribers
    - since this may cause impractically large memory usage for large messages + lots of subscribers

Reactive:

- scheduler requires DelayValueTaskSource usage, with async MRE
  - also, add task container pooling
- timer: remove async start methods
  - simplifies interface
  - signals to the consumer that the underlying mechanism is not async & requires dedicated thread

### Terminal

project idea:

- contains extensions to System.Console
- write colored fore/back-ground (with temp IDisposable swapper)
- write table
- prompt, switch etc. for user interaction

### MessageBroker: Messages

- Add possibility for a client to publish a message to a channel
    - messages must be propagated to all channel subscribers
        - each message header contains a msg-id (unique within channel), a timestamp (moved-to-channel), client(publisher)-id
    - it must be relatively safe, no loss of data etc.:
        - client sends message
            - \[stage 2\] client can limit target clients by providing their ids, by default all subscribed clients are notified
        - server moves message to channel's storage
        - server immediately responds with ack/nack (ack contains message id - uint64)
        - server propagates message from channel to all subscribers (channel's task loop)
        - \[stage 2\] subscriber moves message to un-acked messages collection
        - subscriber notifies remote client's task loop that there is message to send
        - remote client sends message to client
        - client receives message and is notified of it through its local subscription instance
        - \[stage 2\] client needs to respond with ack/nack (not immediately)
            - message headers get additional re-send-no (starts from 0) & re-delivery-no (starts from 0) fields
            - ack notifies the server that message was processed & removes it from waiting-for-ack messages collection
            - nack notifies the serve that message processing failed in a controlled manner
                - subscribers can configure max re-send count & max re-delivery count & default re-send delay
                - nack can specify whether or not to re-send the message (if subscriber allows), true by default
                - nack can specify custom re-send delay
                - \[stage 3\] nack can specify to skip dead letter
        - \[stage 2\] if client doesn't respond in time (5 minutes by default? configurable):
            - server attempts to re-deliver the message, as long as subscriber's max re-delivery allows
        - \[stage 3\] messages that reached max re-send or re-delivery count are moved by the server to subscriber's dead letter collection
            - it can be queried by client-side subscriber instances (fetching dequeues from dead-letter permanently)
            - should be possible to configure max number of dead-letter entries (either channel or subscriber config)
    - note on server's lack of ack/nack to client's message send:
        - if this is due to client's disposal, then client could instead wait for server's responses before fully disposing
            - in this state, client must be seen as disposed to new consumers (no sending of messages etc.)
        - otherwise, it's up to the client to handle this properly
            - there is a chance that server handled the message correctly & just failed to respond to client in time
            - client won't know about it, which may lead to it either rolling back some sort of transaction
            - or sending the same message more than once
            - IT'S ON THE CLIENT TO HANDLE THIS SCENARIO, IF IT CAN EVEN OCCUR
                - it would be a sign of server/client misconfiguration or overwhelmed server
                - response times can be tracked (& would probably have a real impact on client's performance, if very long)
                - and client should be able to react accordingly before a disaster happens
            - it's more important to NOT LOSE messages rather than making sure they are unique
- server should be allowed to send messages to any channel or subscriber
- \[stage 4\] channel synchronization groups (queues):
    - each client-channel link can define a custom synchronization group key (string)
    - each such group acts as a separate synced queue of messages that were received from the client
    - and are yet to be propagated to subscribers
    - each channel by default uses a queue with the same name

### MessageBroker: Max Packet Length & Batching

- Add internal limit to single TCP packet's length (configurable, min 16KB, server defines acceptable range)
    - data that exceeds the limit will be chunked accordingly
    - data must be handled atomically by both client and server (impacts ack/nack)
        - (server-side) store in some sort of temp file with managed lifetime
        - (client-side) if client is configured to ignore large packet buffering:
            - then read all chunks from stream and move them to a single large buffer, then notify
            - otherwise, notify on the fly, with additional chunk-no & chunk-count & total-size info in subscriber event header
                - basically, moves the responsibility to the consumer, but becomes more complex to handle (every msg needs to be checked)
                - only the last chunk can be acked?
                - any chunk can be nacked?
                    - client may have to locally store msg ids
                    - since server may continue sending next chunks before registering the nack and stopping
                    - client will then continue reading re-send once server sends chunk 0 again
                    - anything else will be discarded
                    - or, each chunk is treated as a separate msg?
                        - so, nack would only apply to that particular chunk instead of the whole thing
                        - then, its on the client's consumer to correctly put all chunks together if it nacked any of them
                - 'ignore large packet buffering' could be configurable per subscriber
- packets queued up for sending will be batched together, as long as batch's length does not exceed packed size limit
    - max messages-in-single-batch-count can be configured (unlimited by default) or even disabled, send only, reading does not care
    - message readers must be prepared to handle batch packets (preserve message order)
        - listeners will hold batch packet data in a single large buffer
        - each msg in batch could have its own new buffer made and data could be copied from the large buffer
        - however, that's not optimal, due to more memory usage & copying
        - one easy solution would be to allow memory-pool to 'split' a buffer token in two, at the specified position
        - this way, the large buffer could be split as many times as necessary
        - no copying would be done and no additional mem-alloc would happen
        - and each smaller split buffer would be returned to the pool when it's done
        - which avoids the issue of ref-counting in order to return the whole large buffer
- server's handshake request handling must verify incoming packet length
    - the same applies to create channel, subscriber etc. requests

### MessageBroker: Permanence

- Clients, channels & subscribers can be configured as permanent
  - or, more like they can be configured as transient, false by default
- uses sqlite under the hood (no other option, for now)
- Permanent subscribers:
    - preserve id
    - on-dispose, any unsent messages are persisted in db
    - on-dispose, any un-acked messages are persisted in db before unsent, with re-delivery count increased (if allowed)
    - on-dispose, dead-letter are persisted in db
    - subscriber can configure max in-memory message queue size (min 2?)
        - if that size is exceeded, then all following messages will be persisted in db immediately
        - this will lower memory usage but will negatively impact performance
        - when half(?) of entries get dequeued, then db-persisted entries will be moved to memory (with respect to max size)
    - possibility to configure TTL for dead-letter
    - on-reactivate, data should be loaded from db
- Permanent channels:
    - preserve id
    - on-dispose, any non-propagated messages to subscribers are persisted in db
        - should messages published by clients that specify this channel as non-permanent be persisted? I'm leaning towards: no, they should
          not
    - on-dispose, all info about permanent subscribers should be persisted in db
    - similar to subscribers, configurable max in-memory queue size
    - on-reactivate, subscribers & data should be loaded from db
    - messages pushed to non-active permanent subscribers should be stored in their db (with some form of batching)
    - large object temp files need to be handled correctly
- Permanent clients:
    - preserve id
- channels need to track their permanence with relation to all linked clients & subscribers
    - different clients may specify different permanence for the same channel
    - if at least one client specifies the channel as permanent, then it stays permanent
    - if at least one subscriber is permanent, then it also stays permanent
- non-permanent client can only create non-permanent channel
- non-permanent client can be linked to existing permanent channel
- non-permanent client can only create non-permanent subscriber
- non-permanent client can detach from channel
    - may cause the channel to be deleted when no other client or subscriber is linked to it
- permanent client can create any channel
- permanent client can make existing non-permanent channel permanent
- permanent client can detach from channel
    - channel will be completely removed only when all permanent clients linked to it detach with that flag enabled
    - also, all subscribers must be deleted as well
- permanent client can create any subscriber
- any client can explicitly detach from any channel
    - severs the link between the client and the channel
    - if channel is no longer linked to any other channel & subscriber, then it will be removed
    - if channel is linked to clients that only specify it as non-permanent, then it will become non-permanent, if it wasn't already (all
      subscribers must also be non-permanent)
- any client can explicitly delete any subscriber
- permanent clients can create permanent subscribers to non-permanent channels
    - this will convert the channel to permanent
- reactivating permanent client as non-permanent will make it non-permanent
    - this will also modify all channel links and subscribers related to that client to non-permanent
    - however, all persisted data will be processed as if permanent (until the connection is dropped, then data will also be dropped)

### MessageBroker: Request-Reply Channel

- Add support
- can have multiple subscribers, which all must respond
- cannot be made permanent
- its subscribers also cannot be permanent

### MessageBroker: Subscriber Filter

- allow clients to configure a filter on a subscriber
- filter will be ran on the server-side
- only applies to pub-sub channel subscriptions
- filter is provided in string format
- server compiles the filter into a delegate
    - delegate only accepts a single parameter of some sort of context type
    - which contains all the necessary info about the message
    - it should also allow to read binary message data

### MessageBroker: Server Maintenance

- allow the server to disconnect clients (already implemented)
- delete channels
- delete subscribers
- delete client-channel links
- deleting sends a notification request to the relevant client and doesn't wait for a response
    - it's on the client to handle this correctly
    - however, try to make it so that any 'racing' due to async doesn't cause the client to be auto-disposed
    - e.g. server drops channel
    - at the same time, client sends msg through that channel
    - server discards msg, since client is no longer linked to that channel
        - this should not drop server-client connection
        - server should simply nack msg sent by client
- memory pool usage, access to internal queues etc., statistics and clean-up

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
