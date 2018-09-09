# Custom Functions

IMobileServiceCrudTable has also the capability to call custom functions both for WebApi and OData for versatility (same for Sync Methods which supports both WebApi and OData) .

## Configure OData Function in Api
Before you can define a function in an ODataController it needs to be configure first on Configure method under Startup class, In our example we create "GetSalesTaxRate" , which should look like this.

 ```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
 {
     app.UseMvc(b =>
     {
	     ... Other code here
	     
         b.MapODataServiceRouteBase("api", "api", builder =>
         {
             //custom functions should ne registered here
             builder.Function("GetSalesTaxRate")
                 .Returns<double>()
                 .Parameter<int>("PostalCode");
         });
     });
 }
 ```

Next thing is in your BaseController in our case is "TodoItemsController" we defined it as a regular post method but with a [ODataRoute] annotation, which should look something like below.

 ```csharp
[HttpPost]
[ODataRoute("GetSalesTaxRate(PostalCode={postalCode})")]
public IActionResult GetSalesTaxRate([FromODataUri] int postalCode)
{
    double rate = 5.6;  // Use a fake number for the sample.
    return Ok(rate);
}
 ```

For your reference please have a look at our example [LINK HERE](https://github.com/winstongubantes/matchasync/tree/master/Sample/SampleApi)

## Calling an OData Function in Mobile

PostWebDataAsync is a generic method to call odata or webapi methods, in this case we are calling odata method which is "GetSalesTaxRate", we have this on our sample Asp.Net Core project.

 ```csharp
//get the client instance
var client = MobileServiceClient.Instance;

//get the synctable
var todoTable = client.GetSyncTable<TodoItem>();

var valueResult = await todoTable.PostWebDataAsync<Dictionary<string, string>>(null, $"GetSalesTaxRate(PostalCode={PostalCode})");
var resultValue = valueResult["value"];
 ```

For more information about OData Functions check the complete documentation in this [LINK HERE](https://docs.microsoft.com/en-us/aspnet/web-api/overview/odata-support-in-aspnet-web-api/odata-v4/odata-actions-and-functions)


## Calling  WebApi Method

In Webapi there is no need for configuration, it is very straightforward, This is your typical and plain old way. We are calling "GetSalesTaxRate" method something like this.

 ```csharp
//get the client instance
var client = MobileServiceClient.Instance;

//get the synctable
var todoTable = client.GetSyncTable<TodoItem>();

var valueResult = await todoTable.PostWebDataAsync<string>(10, $"Custom/GetSalesTaxRate");
var resultValue = valueResult;
 ```

<= Back to [Table of Contents](README.md)