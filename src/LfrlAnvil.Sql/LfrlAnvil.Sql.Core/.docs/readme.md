([root](https://github.com/CalionVarduk/LfrlAnvil/blob/main/readme.md))
[![NuGet Badge](https://buildstats.info/nuget/LfrlAnvil.Sql.Core)](https://www.nuget.org/packages/LfrlAnvil.Sql.Core/)

# [LfrlAnvil.Sql.Core](https://github.com/CalionVarduk/LfrlAnvil/tree/main/src/LfrlAnvil.Sql/LfrlAnvil.Sql.Core)

This project contains core functionalities related to SQL, such as:

- SQL statement syntax trees,
- [Expression](https://learn.microsoft.com/en-us/dotnet/api/system.linq.expressions.expression?view=net-7.0)-based SQL parameter binders,
- [Expression](https://learn.microsoft.com/en-us/dotnet/api/system.linq.expressions.expression?view=net-7.0)-based SQL query readers,
- Database schema versioning,
- Support for multiple SQL dialect implementations.

### Examples

Following is an example of an SQL statement syntax tree creation:
```csharp
// SQL node interpreter instance to use
SqlNodeInterpreter interpreter = ...;

var a = SqlNode.RawRecordSet( SqlRecordSetInfo.Create( "dbo", "a" ) );
var b = SqlNode.RawRecordSet( SqlRecordSetInfo.Create( "dbo", "b" ) ).As( "b" );

var dataSource = a.Join( b.InnerOn( a["x"] == b["x"] ) );
var decoratedDataSource = dataSource
    .AndWhere( a["y"] > SqlNode.Literal( 42 ) )
    .AndWhere( b["y"] != SqlNode.Parameter( "y" ) )
    .OrderBy( a["x"].Asc() );

// syntax tree node that represents an SQL query similar to the following:
// SELECT
//   dbo.a.x AS ax,
//   dbo.a.y AS ay,
//   b.y AS by,
//   b.z
// FROM dbo.a
// INNER JOIN dbo.b AS b ON dbo.a.x = b.x
// WHERE (dbo.a.y > 42) AND (b.y <> @y)
// ORDER BY dbo.a.x ASC
var query = decoratedDataSource
    .Select( a["x"].As( "ax" ), a["y"].As( "ay" ), b["y"].As( "by" ), b["z"] );

// the actual SQL query depends on the interpreter's implementation
var context = interpreter.Interpret( query );
var sql = context.Sql.ToString();

// ----------
var target = SqlNode.RawRecordSet( SqlRecordSetInfo.Create( "dbo", "a" ) );
var values = SqlNode.Values(
    new[,]
    {
        { SqlNode.Literal( 42 ), SqlNode.Literal( "foo" ) },
        { SqlNode.Literal( 123 ), SqlNode.Literal( "bar" ) },
        { SqlNode.Literal( -1 ), SqlNode.Literal( "qux" ) }
    } );

// syntax tree node that represents an SQL statement similar to the following:
// INSERT INTO dbo.a (x, y)
// VALUES
// (42, 'foo'),
// (123, 'bar'),
// (-1, 'qux')
var statement = values
    .ToInsertInto( target, target["x"], target["y"] );

// the actual SQL statement depends on the interpreter's implementation
var context = interpreter.Interpret( statement );
var sql = context.Sql.ToString();
```

Following is an example of creation of parameter binders and query readers:
```csharp
// type that defines available parameters and their values
public class ParameterSource
{
    ...
}

// SQL parameter binder factory to use
ISqlParameterBinderFactory factory = ...;

// creates a new parameter binder expression,
// where instances of ParameterSource type are used to initialize
// collections of bound SQL parameters
var expression = factory.CreateExpression<ParameterSource>();

// expression can be compiled into an actual parameter binder
var binder = expression.Compile();

// there exists a possibility to bind an instance of ParameterSource type
// to the parameter binder
var executor = binder.Bind( new ParameterSource() );

// DB command to use as the target of parameter binding
IDbCommand command = ...;

// initializes DB command's parameters based on the compiled binder and a ParameterSource instance
binder.Bind( command, new ParameterSource() );

// or based on prepared parameter binder's executor
executor.Execute( command );

// ----------

// type that defines data fields of a single row to be read
public class Row
{
    ...
}

// SQL query reader factory to use
ISqlQueryReaderFactory factory = ...;

// creates a new query reader expression,
// where Row type defines available data fields and their types
var expression = factory.CreateExpression<Row>();

// expression can be compiled into an actual query reader
var reader = expression.Compile();

// there exists a possibility to bind an SQL query to the query reader
var executor = reader.Bind( "SELECT * FROM foo" );

// data reader to use as the source of rows
using var r = command.ExecuteReader();

// reads all rows from the data reader's current result set
var rows = reader.Read( r );

// or reads all rows from the DB command by using the prepared query reader's executor
rows = executor.Execute( command );
```

Following is an example of how to define and apply DB versions to the specific database:
```csharp
// SQL DB factory to use
ISqlDatabaseFactory factory = ...;

// defines the first DB version
var version1 = SqlDatabaseVersion.Create(
    new Version( "0.1" ),
    db =>
    {
        // creates a new 'T1' table
        var t1 = db.Schemas.Default.Objects.CreateTable( "T1" );

        // creates a new non-nullable 'T1.C1' column of 'int' type
        var c1 = t1.Columns.Create( "C1" ).SetType<int>();

        // creates a new nullable 'T1.C2' column of 'string' type
        var c2 = t1.Columns.Create( "C2" ).SetType<string>().MarkAsNullable();

        // creates a new primary key constraint on the 'T1' table, using ('C1' ASC) columns
        var pk1 = t1.Constraints.SetPrimaryKey( c1.Asc() );
    } );

// defines the second DB version
var version2 = SqlDatabaseVersion.Create(
    new Version( "0.2" ),
    db =>
    {
        // gets the 'T1' table created in the previous version
        var t1 = db.Schemas.Default.Objects.GetTable( "T1" );

        // gets the primary key constraint from the 'T1' table
        var pk1 = t1.Constraints.GetPrimaryKey();

        // creates a new 'T2' table
        var t2 = db.Schemas.Default.Objects.CreateTable( "T2" );

        // creates a new non-nullable 'T2.C3' column of 'int' type
        var c3 = t2.Columns.Create( "C3" ).SetType<int>();

        // creates a new primary key constraint on the 'T2' table, using ('C3' ASC) columns
        var pk2 = t2.Constraints.SetPrimaryKey( c3.Asc() );

        // creates a new foreign key constraint on the 'T2' table,
        // whose 'C3' column references 'C1' column from the 'T1' table
        var fk = t2.Constraints.CreateForeignKey( pk2.Index, pk1.Index );
    } );

// creates a history of DB versions
var versionHistory = new SqlDatabaseVersionHistory( version1, version2 );

// connection string to the DB
string connectionString = ...;

// applies version history to the DB
var result = factory.Create(
    connectionString,
    versionHistory,
    SqlCreateDatabaseOptions.Default.SetMode( SqlDatabaseCreateMode.Commit ) );
```
