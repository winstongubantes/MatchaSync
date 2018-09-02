# Matcha Sync Plugin for Xamarin.Forms
A plugin library for offline data sync, provides an SDK library for both Client & Server that makes it easy for developers to create apps that are functional without a network connection. But is also versatile that it can handle custom webapi calls (non-Odata webserver).
 
## Preview


 
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

 
### Setup for Mobile 
 
 ### For iOS
 
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

 ### For Android
 
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
 
 ### Inserting Data
 
The plain and old way in adding data using MobileServiceClient is by using its static instance .
 
 ```csharp
var todoItemTable = MobileServiceClient.Instance.GetSyncTable<TodoItem>();

//Inserts or Update a Data
todoItemTable.InsertOrUpdate(new TodoItem
{
    Name = "Task Name"
    ... You Data here
});
 ```
 
In our sample project since we are using PRISM, we register our instance in Container and therefore make it more testable and meaningful.

 ```csharp
containerRegistry.RegisterInstance(MobileServiceClient.Instance);
 ```

Then we are able to use it anywhere, in this case in our viewmodel.
 
 ```csharp
 public class MainPageViewModel : ViewModelBase
{
    private readonly IMobileServiceClient _mobileServiceClient;
	
	public MainPageViewModel(IMobileServiceClient mobileServiceClient)
	{
		_mobileServiceClient = mobileServiceClient;
	}
	
	public void InsertData(Todo Item)
	{
		var todoItemTable = _mobileServiceClient.GetSyncTable<TodoItem>();

		//Inserts or Update a Data
		todoItemTable.InsertOrUpdate(new TodoItem
		{
			Name = "Task Name"
			... You Data here
		});
	}
}
 ```
 
 ### Beautiful!
 
 To be continued....

