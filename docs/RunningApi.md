# Running Sample Project (Api)

To get you getting started fast and running the api project, you need to expose your local endpoint so that your mobile client can communicate, there are many ways on how to expose your local endpoint to the mobile client, list below are the popular one.

* running api project in [IIS launch mode](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/development-time-iis-support?view=aspnetcore-2.1)
* exposing your local endpoint to public using [ngrok](https://ngrok.com/product)
* Publish in [local IIS](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/?view=aspnetcore-2.1&tabs=aspnetcore2x)

When running your project with either IIS launch mode or publish local IIS, you will need to get your local IP via command prompt [How To](https://www.whatismyip.com/how-to-get-your-local-ip-address-on-windows-10/).  Your local api endpoint should be something like this:

[http://YOU_LOCAL_IP/SampleApi/api](http://localhost/SampleApi/api)

But When you are using ngrok it would similar to this endpoint:

https://myapp.ngrok.io/api 


## Testing via web browser

The has a default get method, which you can call directly via web browser, In our case we have TodoItemsController, and we can call directly like the sample path below. 

[http://YOU_LOCAL_IP/SampleApi/api/TodoItems](http://localhost/SampleApi/api/TodoItems)

It will return a json result of the default query

```json
{
    "@odata.context": "https://sampleapisync.azurewebsites.net/api/$metadata#TodoItems",
    "value": [
        {
            "Name": "Task 1",
            "IsComplete": false,
            "Id": 1,
            "LocalId": "9f8bd152-8039-4b88-be7c-231244931fe4",
            "IsSynced": false,
            "IsDeleted": false,
            "LastUpdated": "2018-09-09T00:17:14.7205038Z"
        }]
}
```

## Use Postman to test your api

Postman is a very handy tool to test your api, may it be local, staging or live. If you are new to Postman here is a [LINK](https://learning.getpostman.com/getting-started/) to get you started. 

<= Back to [Table of Contents](README.md)