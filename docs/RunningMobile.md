# Running Sample Project (Mobile)

When you are running the Mobile project, make sure to select "SampleMobile.Android" or the "SampleMobile.iOS" as the starting project. You might encounter issue with android project make sure your android SDK updated, for more info about android SDK update please click [HERE](https://docs.microsoft.com/en-us/xamarin/android/get-started/installation/android-sdk?tabs=vswin). 

If you are new to Xamarin mobile development here is a [LINK](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/get-started/hello-xamarin-forms/quickstart?tabs=vswin) to get started. 

## Setup
The sample project has everything setup for you except the Api Address, You will need to get the api endpoint path, Here is the link on how to get the end point [LINK HERE](RunningApi.md).  Then assign the endpoint to the ApiUrl property of AppConstants under the Constants folder.

 ```csharp
  public static class AppConstants
  {
      public const string ApiUrl = "https://YOUR_API_ENDPOINT_HERE/api";
  }
 ```

That's it, run your mobile and your are ready to go

<= Back to [Table of Contents](README.md)