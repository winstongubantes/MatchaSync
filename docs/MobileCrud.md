# Mobile CRUD Operations

IMobileServiceCrudTable the one who manage the operations, He also acts as the intermediary between your api and your local data.
   
## First things first
* Dont forget to install the nuget [NuGet](http://www.nuget.org/packages/Matcha.Sync.Mobile) [![NuGet](https://img.shields.io/nuget/v/Matcha.Sync.Mobile.svg?label=NuGet)](https://www.nuget.org/packages/Xam.Plugins.Settings/)
* You must install into ALL of your projects, including client projects.
* You must create a model that inherit Synchronizable or ISynchronizable 

 ```csharp
 public class TodoItem : Synchronizable
 {
     public string Name { get; set; }
     public bool IsComplete { get; set; }
 }
 ```

## Inserting and Updating Data

IMobileServiceCrudTable manages and checks if the data is already existing or not (through field LocalId), It will be stored first on the local storage to be synced later when the connection is available.

 ```csharp
//get the client instance
var client = MobileServiceClient.Instance;

//get the synctable
var todoTable = client.GetSyncTable<TodoItem>();

todoTable .InsertOrUpdate(new TodoItem
 {
     Id = 1,
     Name = "NewTaskValue",
     LastUpdated = DateTime.Now
 });
 ```
 
 If you are using MVVM framework MobileServiceClient instance can be registered on your IOC container should be mockable for test since it is derive from IMobileServiceClient.

## Deleting Data

Data is not physically deleted but it is Tagged as "IsDeleted" to true (Soft deletion). So it is really up to the server to delete the data or it can be for soft deletion.

 ```csharp
//get the client instance
var client = MobileServiceClient.Instance;

//get the synctable
var todoTable = client.GetSyncTable<TodoItem>();

//find data for deletion
var item = TodoItems.FirstOrDefault(e => e.Id == 1);

todoTable.Delete(item );

 ```

Note: when you are deleting or inserting a data, it does not automatically sync. You will need to call PushAsync method which will be futher discussed [here](PushAndPullSync.md)

<= Back to [Table of Contents](README.md)