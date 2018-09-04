# Matcha Sync Plugin for Xamarin.Forms
A plugin library for offline data sync, provides an SDK library for both Client & Server that makes it easy for developers to create apps that are functional without a network connection. But is also versatile that it can handle custom webapi calls (non-Odata webserver), The reason for creating a plugin for data synchronization it to provide developers other alternative which is FREE and customizable and drop dead simple. 
 
## Preview
 ![alt text](https://github.com/winstongubantes/matcha.sync/blob/master/Images/valid.gif "Sample In Action")

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

### For further steps
Below are the things needed to wireup all the layers from server to client setup, but since the Mobile.Sync is very versatile we can use not just Odata driven webapi, we can also tap plain old webapi itself.

### Setup for WebApi
 
#### Using BaseController<T\>
 
BaseController is based on ODataController from Microsoft.AspNetCore.OData nuget library, by using BaseController you have to install Matcha.Sync.Api [nuget](https://www.nuget.org/packages/Matcha.Sync.Api/), you will have to implement and provide a generic ISynchronizable class, in our sample we use TodoItem class. 
 
 ```csharp
 
  public class TodoItemsController : BaseController<TodoItem>
  {
	  private TodoItemContext _db;
      public TodoItemsController(TodoItemContext db){_db = db;}
   
      public override IActionResult Get() => Ok(_db.TodoItems);

      protected override Task Delete(TodoItem data)
      {//Implement you delete logic here
      }

      protected override Task Insert(TodoItem data)
      {//Implement you insert logic here
      }

      protected override Task Update(TodoItem data)
      {//Implement you update logic here
      }
  }
 ```
 
In our sample webapi project we uses InMemoryDatabase for simplicity sake, so in your live projects you can use SQL Server or any supported Context Provider. 

 ```csharp
	 services.AddDbContext<TodoItemContext>(opt => opt.UseInMemoryDatabase("TodoItems"));
 ```
 
 Please make sure to add supported OData Query on your startup configure method of your web api project, it is something like this.
  ```csharp
  public void Configure(IApplicationBuilder app, IHostingEnvironment env)
   {
       if (env.IsDevelopment())
       {
           app.UseDeveloperExceptionPage();
       }

       app.UseMvc(b =>
       {
           b.MapODataServiceRoute("odata", "odata", GetEdmModel());

           //TODO: Add the supported query
           b.Select()
               .MaxTop(100)
               .OrderBy()
               .Filter()
               .Count();
       });
   }
 ```

#### Synchronizable class

Synchronizable is a generic class for your DTO model it is inherited from ISynchronizable interface, In which would make our data synchronization process between server and our Mobile Client, In our sample project we uses TodoItem. 

 ```csharp
 public class TodoItem : Synchronizable
 {
     public string Name { get; set; }
     public bool IsComplete { get; set; }
 }
 ```

#### The Good thing!

Matcha.Sync.Mobile doesnt enforce you to use BaseController from Matcha.Sync.Api, you can create  WebApi  project from a scratch and you can still use Matcha.Sync.Mobile synchronization feature and start working with an offline data on your mobile. 

 
### Setup for Mobile 
 
#### For iOS
 
You call the init after all libraries initialization in FinishedLaunching method in FormsApplicationDelegate class.
 
 ```csharp
 
public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
 {
     public override bool FinishedLaunching(UIApplication app, NSDictionary options)
     {
         MobileServiceClient.Init("http://YOUR_IP_HERE/SampleApi/odata");
         
           ....// Code for init was here
         return base.FinishedLaunching(app, options);
     }
 }
 
 ```

#### For Android
 
You call the init after all libraries initialization in MainActivity class.
 
 ```csharp
 
public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
 {
     protected override void OnCreate(Bundle bundle)
     {
	     MobileServiceClient.Init("http://YOUR_IP_HERE/SampleApi/odata");
	     
	     base.OnCreate(bundle);
         ....// Code for init was here
     }
 }
 
 ```

### Things to Remember!

#### IMobileServiceClient

IMobileServiceClient is the central context of our online and offline data management, It holds the list of queued CRUD operations which would sync it later by batch to the server.

 ```csharp
public interface IMobileServiceClient
 {
     IMobileServiceCrudTable<T> GetSyncTable<T>() where T : ISynchronizable;
     void DefineSyncTable<T>() where T : ISynchronizable;
     Task SyncAllData();
 }
 ```

#### IMobileServiceCrudTable
 
IMobileServiceCrudTable is the intermediary for both offline and online data syncronization, The generic class is ISynchronizable which is the same class we used on the BaseController of our webapi project, It is also versatile that you can call any custom webapi calls with "PostWebDataAsync" method, we are using SQLite-based implementation.

 ```csharp
 public interface IMobileServiceCrudTable<T> : IMobileServiceSyncTable
 {
     IList<T> ToList();
     IList<T> ToList(string queryId);
     void InsertOrUpdate(T data);
     void Delete(T data);
     Task PullAsync(string queryId, IMobileServiceTableQuery<T> paramQuery);
     Task PullAsync(string queryId, string paramQuery);
     IMobileServiceTableQuery<T> CreateQuery();
     Task<ODataResult<T>> ExecuteQuery(string paramQuery);
     Task<ODataResult<T>> ExecuteQuery(IMobileServiceTableQuery<T> paramQuery);
     Task<TF> PostWebDataAsync<TF>(object obj, string methodName);
     long RecordCount(string queryId);
 }
 ```

 
### Beautiful!
 
 How about calling custom methods on Odata Controller or WebApi Controller or a mixed of both, We have also supported it. look how simple it is.

#### Calling Custom OData Function

First , on your Asp.Net Core project you have to  declare it under Configure method on your IEdmModel,  you can follow the Microsoft's official [documentation](https://docs.microsoft.com/en-us/aspnet/web-api/overview/odata-support-in-aspnet-web-api/odata-v4/odata-actions-and-functions), You can also check our sample Asp.Net core project.

Sample call:

 ```csharp
// we are calling ODataController's GetSalesTaxRate method
var valueResult = await todoTable.PostWebDataAsync<Dictionary<string, string>>(null, $"GetSalesTaxRate(PostalCode={PostalCode})");
 ResultValue = valueResult["value"];
 ```

#### Calling Custom WebApi Function

Sample call:

 ```csharp
// we are calling Controller's GetSalesTaxRate method (with Custom route)
var valueResult = await todoTable.PostWebDataAsync<string>(10, $"Custom/GetSalesTaxRate");
ResultValue = valueResult;
 ```


### Awesome!
 
Matcha sync allows users to interact with a mobile application, viewing, adding, or modifying data, even where there isn't a network connection. Changes are stored in a local database, and once the device is online, the changes can be synced.