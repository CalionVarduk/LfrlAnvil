([root](https://github.com/CalionVarduk/LfrlAnvil/blob/main/readme.md))
[![NuGet Badge](https://buildstats.info/nuget/LfrlAnvil.Collections)](https://www.nuget.org/packages/LfrlAnvil.Collections/)

# [LfrlAnvil.Collections](https://github.com/CalionVarduk/LfrlAnvil/tree/main/src/LfrlAnvil.Collections)

This project contains a few additional collections and data structures.
Some of them include different dictionary and set variations (e.g. multi-dictionary and multi-set) as well as heaps and graphs.

### Examples

This section contains examples of some of the more interesting data structures.

Following is an example of a dictionary heap data structure,
which is a heap data structure with the ability to identify contained elements by associated keys:
```csharp
// creates a new empty dictionary heap, with 'string' as entry key type and 'int' as entry value type
var heap = new DictionaryHeap<string, int>();

// adds a few entries to the heap
// expected order of extraction: ('qux', -1), ('foo', 42), ('bar', 123)
heap.Add( "foo", 42 );
heap.Add( "bar", 123 );
heap.Add( "qux", -1 );

// replaces existing entry's value and returns the old value, while respecting the heap's invariant
// result should be equal to -1
// expected order of extraction after replacement: ('foo', 42), ('bar', 123), ('qux', 456)
var oldValue = heap.Replace( "qux", 456 );

// removes the entry at the top of the heap and returns its value
// result should be equal to 42
var top = heap.Extract();

// returns value of the entry at the top of the heap, without removing it
// result should be equal to 123
var nextTop = heap.Peek();

// gets the value of an entry associated with the provided key
// result should be equal to 456
var qux = heap.GetValue( "qux" );
```

Following is an example of a directed graph data structure:
```csharp
// creates a new empty directed graph
// with 'string' as node key type, 'int' as node value type and 'double' as edge value type
var graph = new DirectedGraph<string, int, double>();

// adds a few nodes to the graph, each node has a key and a value
// so far, there are no edges in the graph
var fooNode = graph.AddNode( "foo", 42 );
var barNode = graph.AddNode( "bar", 123 );
var quxNode = graph.AddNode( "qux", -1 );

// adds a 'foo' => 'bar' edge, each edge also has a value
var fooBarEdge = fooNode.AddEdgeTo( "bar", 1.5 );

// adds a 'bar' <=> 'qux' edge
var barQuxEdge = graph.AddEdge( "bar", "qux", 2.25, GraphDirection.Both );

// adds a 'qux' <=> 'qux' edge
var quxSelfEdge = quxNode.AddEdgeTo( quxNode, -0.5 );

// changes the direction of the 'foo' => 'bar' edge to 'foo' <= 'bar'
fooBarEdge.ChangeDirection( GraphDirection.In );

// removes 'foo' <= 'bar' edge
graph.RemoveEdge( "foo", "bar" );

// removes 'qux' node and all associated edges, which includes
// the 'bar' <=> 'qux' edge and the 'qux' <=> 'qux' edge
quxNode.Remove();
```
