# Custom Functions

IMobileServiceCrudTable has also the capability to call custom functions both for WebApi and OData for versatility (same for Sync Methods which supports both WebApi and OData) .

## Call a Function from OData

PostWebDataAsync is a generic method to call odata or webapi methods, in this case we are calling odata method which is "GetSalesTaxRate", we have this on our sample Asp.Net Core project.

 ```csharp
//get the client instance
var client = MobileServiceClient.Instance;

//get the synctable
var todoTable = client.GetSyncTable<TodoItem>();

var valueResult = await todoTable.PostWebDataAsync<Dictionary<string, string>>(null, $"GetSalesTaxRate(PostalCode={PostalCode})");
var resultValue = valueResult["value"];
 ```

Keep in mind that we need to do some extra steps in exposing odata custom functions, for more info you can check the complete documentation in this [LINK HERE](https://docs.microsoft.com/en-us/aspnet/web-api/overview/odata-support-in-aspnet-web-api/odata-v4/odata-actions-and-functions)

## Call a Function from WebApi

In Webapi version it is very straightforward, This is your typical and plain old way. We are calling "GetSalesTaxRate" method.

 ```csharp
//get the client instance
var client = MobileServiceClient.Instance;

//get the synctable
var todoTable = client.GetSyncTable<TodoItem>();

var valueResult = await todoTable.PostWebDataAsync<string>(10, $"Custom/GetSalesTaxRate");
var resultValue = valueResult;
 ```

<= Back to [Table of Contents](README.md)