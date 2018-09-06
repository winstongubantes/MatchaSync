# Sync Your Changes (OData)

IMobileServiceCrudTable the one who manage the operations, He also acts as the intermediary between your api and your local data, There are two methods to push the changes you have made and pull the newest data, We only support OData v4 endpoints.

## PullAsync

PullAsync is IMobileServiceCrudTable method to get the data from the server, two overload methods available and is very straightforward.

 ```csharp
//get the client instance
var client = MobileServiceClient.Instance;

//get the synctable
var todoTable = client.GetSyncTable<TodoItem>();

//create a query
var query = todoTable.CreateQuery()
                            .Where(e=> e.IsComplete);

await todoTable.PullAsync("testquerycomplete", query);
var data = todoTable.ToList("testquerycomplete");
 ```
 
Note: you can compose a complex query in CreateQuery method which is similar to IQueryable but only limited methods. To know more about CreateQuery please check the [LINK HERE](Query.md)

## PushAsync

PullAsync is IMobileServiceCrudTable method to push the changes made in your local db and push it to the server, You can take a look how the handling was made [LINK HERE](https://github.com/winstongubantes/matchasync/blob/master/Sample/SampleApi/Controllers/TodoItemsController.cs).

We made a deletion of data and send it to server

 ```csharp
//get the client instance
var client = MobileServiceClient.Instance;

//get the synctable
var todoTable = client.GetSyncTable<TodoItem>();

//find data for deletion
var item = TodoItems.FirstOrDefault(e => e.Id == 1);

todoTable.Delete(item );

await todoTable.PushAsync();
 ```

Please take a look at the sample project for reference or you can check the UnitTest Project it contains integration test for syncing data

<= Back to [Table of Contents](README.md)