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
         MobileServiceClient.Init("http://YOUR_IP_HERE/SampleApi/odata");
         
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
 ```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
 {
     if (env.IsDevelopment())
     {
         app.UseDeveloperExceptionPage();
     }

     app.UseMvc(b =>
     {
         //call MapODataServiceRouteBase
         b.MapODataServiceRouteBase("api", "api");
     });
 }
 ```

## Setup OData supported queries
We will not gonna cover it all here since this is very broad topic, I will just gonna link you here to get you started with OData Asp.Net Core [LINK HERE](https://blogs.msdn.microsoft.com/odatateam/2018/07/03/asp-net-core-odata-now-available/)

You can take a look at our sample Asp.Net Core project for more reference [LINK HERE](https://github.com/winstongubantes/matchasync/tree/master/Sample/SampleApi)

<= Back to [Table of Contents](README.md)