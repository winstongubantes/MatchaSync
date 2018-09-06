# Sync Your Changes (WebApi)

IMobileServiceCrudTable the one who manage the operations, He also acts as the intermediary between your api and your local data, There are two methods to push the changes you have made and pull the newest data.

## PullAsync

PullAsync is IMobileServiceCrudTable method to get the data from the server, two overload methods available and is very straightforward.

 ```csharp
//get the client instance
var client = MobileServiceClient.Instance;

//get the synctable
var todoTable = client.GetSyncTable<CustomItem>();

//'GetComplete' is a custom method from webapi
await todoTable.PullAsync("apiquerycomplete", "GetComplete?isComplete=false");
var data = todoTable.ToList("apiquerycomplete");
 ```
 
Note: CustomItem is just derive from TodoItem class, we do this to match the controller name in webapi.

## PushAsync

PullAsync is IMobileServiceCrudTable method to push the changes made in your local db and push it to the server.

We made a deletion of data and send it to server

 ```csharp
//get the client instance
var client = MobileServiceClient.Instance;

//get the synctable
var todoTable = client.GetSyncTable<CustomItem>();

//find data for deletion
var item = TodoItems.FirstOrDefault(e => e.Id == 1);

todoTable.Delete(item );

await todoTable.PushAsync();
 ```

Please take a look at the sample project for reference or you can check the UnitTest Project it contains integration test for syncing data

<= Back to [Table of Contents](README.md)