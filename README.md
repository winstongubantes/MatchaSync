# Matcha Sync Plugin for Xamarin.Forms
A plugin library for offline data sync, provides an SDK library for both Client & Server that makes it easy for developers to create apps that are functional without a network connection. But is also versatile that it can handle custom webapi calls (non-Odata webserver), The reason for creating a plugin for data synchronization it to provide developers other alternative which is FREE and customizable and drop dead simple. 
 
## Nuget
[Matcha.Sync.Api](http://www.nuget.org/packages/Matcha.Sync.Api) [![NuGet](https://img.shields.io/nuget/v/Matcha.Sync.Api.svg?label=NuGet)](https://www.nuget.org/packages/Matcha.Sync.Api/)

[Matcha.Sync.Mobile](http://www.nuget.org/packages/Matcha.Sync.Mobile) [![NuGet](https://img.shields.io/nuget/v/Matcha.Sync.Mobile.svg?label=NuGet)](https://www.nuget.org/packages/Matcha.Sync.Mobile/)
 
## Preview

<img src="https://github.com/winstongubantes/matcha.sync/blob/master/Images/valid.gif" width="600" title="md">

### Simple as 1 - 2 - 3
#### Step 1

 ```csharp
 //call initialization in each platform
MobileServiceClient.Init("http://YOUR_IP_HERE/SampleApi/odata");
 ```
 #### Step 2

 ```csharp
 //get the client instance
var client = MobileServiceClient.Instance;
//define the synctable
var todoTable = client.DefineSyncTable<TodoItem>();
 ```
#### Step 3

 ```csharp
 //create a query
var query = todoTable.CreateQuery().Where(e=> e.Name.Contains("Task"));
//it will pull data based on the query
await todoTable.PullAsync("testquery", query);
 ``` 
 
#### CRUD
Since "todoTable" is a IMobileServiceCrudTable, you will be able to insert a record which can then be synchronized later on the server.

 ```csharp
//insert or update
todoTable.InsertOrUpdate(new TodoItem
{
    Id = (lastData?.Id ?? 0) + 1,
    Name = NewTaskValue,
    LastUpdated = DateTime.Now
});

//for deletion, get the item you want to delete
todoTable.Delete(item)
 ``` 

That was easy....

### Documentation

Get started by reading through the [Matcha Sync Documentation](https://winstongubantes.github.io/matchasync/)

### Awesome!
 
Matcha sync allows users to interact with a mobile application, viewing, adding, or modifying data, even where there isn't a network connection. Changes are stored in a local database, and once the device is online, the changes can be synced.
