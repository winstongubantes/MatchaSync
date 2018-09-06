# Queries

IMobileServiceCrudTable has CreateQuery method which will create an instance of IMobileServiceTableQuery<T\>  which is quite similar to IQueryable.

## Take

```csharp
//create a query
var query = _crudTodotTable.CreateQuery().Take(10);
 ```

full version below

 ```csharp
//get the client instance
var client = MobileServiceClient.Instance;

//get the synctable
var todoTable = client.GetSyncTable<TodoItem>();

//create a query
var query = _crudTodotTable.CreateQuery().Take(10);

await client.PullAsync("takequery", query);
var data = client.ToList("takequery");
 ```

## Skip

```csharp
//create a query
var query = _crudTodotTable.CreateQuery().Skip(1);
 ```

full version below

 ```csharp
//get the client instance
var client = MobileServiceClient.Instance;

//get the synctable
var todoTable = client.GetSyncTable<TodoItem>();

//create a query
var query = _crudTodotTable.CreateQuery().Skip(1);

await client.PullAsync("skipquery", query);
var data = client.ToList("skipquery");
 ```

## Order By

```csharp
//create a query
var query = _crudTodotTable.CreateQuery().OrderBy(e=> e.Name);
 ```

full version below

 ```csharp
//get the client instance
var client = MobileServiceClient.Instance;

//get the synctable
var todoTable = client.GetSyncTable<TodoItem>();

//create a query
var query = _crudTodotTable.CreateQuery().OrderBy(e=> e.Name);

await client.PullAsync("skipquery", query);
var data = client.ToList("skipquery");
 ```

## Order By Desc

```csharp
//create a query
var query = _crudTodotTable.CreateQuery().OrderByDescending(e=> e.Name);
 ```

full version below

 ```csharp
//get the client instance
var client = MobileServiceClient.Instance;

//get the synctable
var todoTable = client.GetSyncTable<TodoItem>();

//create a query
var query = _crudTodotTable.CreateQuery().OrderByDescending(e=> e.Name);

await client.PullAsync("skipquery", query);
var data = client.ToList("skipquery");
 ```

## Where

Where method contains Expression which can be complex at times, however there where some cases that the expression maybe not supported as to the limit of odata query, I have provided a generic sample below.

```csharp
//create a query
var query = _crudTodotTable.CreateQuery().Where(e=> e.IsComplete || e.Name.Contains("T"));
 ```

full version below

 ```csharp
//get the client instance
var client = MobileServiceClient.Instance;

//get the synctable
var todoTable = client.GetSyncTable<TodoItem>();

//create a query
var query = _crudTodotTable.CreateQuery().Where(e=> e.IsComplete || e.Name.Contains("T"));

await client.PullAsync("testquerycomplete", query);
var data = client.ToList("testquerycomplete");
 ```

## Where 'Contains'
```csharp
//create a query
var query = _crudTodotTable.CreateQuery().Where(e=> e.Name.Contains("Task"));
 ```

full version below

 ```csharp
//get the client instance
var client = MobileServiceClient.Instance;

//get the synctable
var todoTable = client.GetSyncTable<TodoItem>();

//create a query
var query = _crudTodotTable.CreateQuery().Where(e=> e.Name.Contains("Task"));

await client.PullAsync("testquerycomplete", query);
var data = client.ToList("testquerycomplete");
 ```

## Where 'StartsWith'
```csharp
//create a query
var query = _crudTodotTable.CreateQuery().Where(e=> e.Name.StartsWith("Task"));
 ```

full version below

 ```csharp
//get the client instance
var client = MobileServiceClient.Instance;

//get the synctable
var todoTable = client.GetSyncTable<TodoItem>();

//create a query
var query = _crudTodotTable.CreateQuery().Where(e=> e.Name.StartsWith("Task"));

await client.PullAsync("testquerycomplete", query);
var data = client.ToList("testquerycomplete");
 ```

## Where 'EndsWith'
```csharp
//create a query
var query = _crudTodotTable.CreateQuery().Where(e=> e.Name.EndsWith("Task"));
 ```

full version below

 ```csharp
//get the client instance
var client = MobileServiceClient.Instance;

//get the synctable
var todoTable = client.GetSyncTable<TodoItem>();

//create a query
var query = _crudTodotTable.CreateQuery().Where(e=> e.Name.EndsWith("Task"));

await client.PullAsync("testquerycomplete", query);
var data = client.ToList("testquerycomplete");
 ```

## Where Combinations
```csharp
//create a query
var query = _crudTodotTable.CreateQuery()
				.Where(e=> e.Name.StartsWith("Task"))
				.Where(e=> e.Name.Contains("Task") || e.Name.Contains("a"))
				.Where(e=> e.IsComplete);
 ```

## Query Combinations
```csharp
//create a query
var query = _crudTodotTable.CreateQuery()
				.Where(e=> e.Name.StartsWith("Task") || e.Name.Contains("T"))
				.Where(e=> e.Name.Contains("Task"))
				.Where(e=> e.IsComplete)
				.Take(20)
				.Skip(1)
				.OrderBy(e=> e.Name);
 ```

Note: Skip and Take is a very useful combinations that we can take advantage for creating a pagination process, which is in this [LINK HERE](Pagination.md)

<= Back to [Table of Contents](README.md)