# Getting Started

## Setup Mobile
* NuGet: [Matcha.Sync.Mobile](http://www.nuget.org/packages/Matcha.Sync.Mobile) [![NuGet](https://img.shields.io/nuget/v/Matcha.Sync.Mobile.svg?label=NuGet)](https://www.nuget.org/packages/Matcha.Sync.Mobile/)
* `PM> Install-Package Matcha.Sync.Mobile`
* Install into ALL of your projects, include client projects.

## For Android
 
You call the init after all libraries initialization in MainActivity class.
 
 ```csharp
 
public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
 {
     protected override void OnCreate(Bundle bundle)
     {
	     MobileServiceClient.Init("http://YOUR_API_ADDRESS_HERE");
	     
	     base.OnCreate(bundle);
         ....// Code for init was here
     }
 }
 
 ```

## For iOS
 
You call the init after all libraries initialization in FinishedLaunching method in FormsApplicationDelegate class.
 
 ```csharp
 
public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
 {
     public override bool FinishedLaunching(UIApplication app, NSDictionary options)
     {
         MobileServiceClient.Init("http://YOUR_API_ADDRESS_HERE");
         
           ....// Code for init was here
         return base.FinishedLaunching(app, options);
     }
 }
 
 ```

## Setup Asp.Net Core
* NuGet: [Matcha.Sync.Api](http://www.nuget.org/packages/Matcha.Sync.Api) [![NuGet](https://img.shields.io/nuget/v/Matcha.Sync.Api.svg?label=NuGet)](https://www.nuget.org/packages/Xam.Plugins.Settings/)
* `PM> Install-Package Matcha.Sync.Api`
* Install to your  asp.net core project

## Create TodoItem model and Inherit Synchronizable

 ```csharp
 public class TodoItem : Synchronizable
 {
     public string Name { get; set; }
     public bool IsComplete { get; set; }
 }
 ```

## Create Controller and Inherit BaseController

BaseController is based on ODataController from Microsoft.AspNetCore.OData nuget library, It holds a generic parameter which is of type ISynchronizable in our case it is TodoItem.

 ```csharp
 
public class TodoItemsController : BaseController<TodoItem>
 {
     public override IActionResult Get() => Ok(_db.TodoItems);

     protected override async Task Delete(TodoItem data)
     {
         //Your code here
     }

     protected override async Task Insert(TodoItem data)
     {
         //Your code here
     }

     protected override async Task Update(TodoItem data)
     {
         //Your code here
     }
 }
 
 ```

## Setup OData Configure method

MapODataServiceRouteBase method will automatically register all Controller derived from "BaseController".

 ```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
 {
     //... Code here
     
     app.UseMvc(b =>
     {
         //This will automatically register all OData derived from "BaseController"
         b.MapODataServiceRouteBase("api", "api");
     });
 }
 ```

Note: In our sample project we are using the In-Memory Database for our TodoItemContext, you can change it to use SqlServer or any other options available if you wanted to, If you want to know more about OData and Asp.Net Core [LINK HERE](https://blogs.msdn.microsoft.com/odatateam/2018/07/03/asp-net-core-odata-now-available/)

## Getting Started with the sample project

There is a guide on how to run the sample project ([Mobile Guide](RunningMobile.md) and [Api Guide](RunningApi.md))  and the full source is also provided [LINK HERE](https://github.com/winstongubantes/matchasync/tree/master/Sample).

<= Back to [Table of Contents](README.md)