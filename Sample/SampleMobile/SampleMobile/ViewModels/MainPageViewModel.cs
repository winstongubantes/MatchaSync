using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Matcha.Sync.Mobile;
using Prism.Services;
using SampleMobile.Models;

namespace SampleMobile.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly IMobileServiceClient _mobileServiceClient;
        private readonly IDeviceService _deviceService;

        public MainPageViewModel(
            INavigationService navigationService, 
            IMobileServiceClient mobileServiceClient, 
            IDeviceService deviceService) 
            : base (navigationService)
        {
            _mobileServiceClient = mobileServiceClient;
            _deviceService = deviceService;
            var table = _mobileServiceClient.GetSyncTable<TodoItem>();

            var t = table.CreateQuery().Where(e => e.Name == "Task" || e.Name == "1").Query;
            var t1 = table.CreateQuery()
                .Where(e => e.Name.Contains("Task") || e.Name == "1")
                .Where(e => e.Id == 1)
                .Where(e => !e.IsComplete);


            var t2 = table.CreateQuery().Where(e => e.Id == 1).Query;
            var t3 = table.CreateQuery().Where(e => !e.IsComplete).Query;
            var t4 = table.CreateQuery().Where(e => e.Name.Contains("Task")).Query;
            var t5 = table.CreateQuery().Where(e => e.Name.StartsWith("Task")).Query;
            var t6 = table.CreateQuery().Where(e => e.Name.EndsWith("Task")).Query;
            var t7 = table.CreateQuery().Where(e => e.Name.Contains("Task") && e.Name.EndsWith("Task")).Query;

            Debug.WriteLine(t);
           // Debug.WriteLine(t1);
            Debug.WriteLine(t2);
            Debug.WriteLine(t3);
            Debug.WriteLine(t4);
            Debug.WriteLine(t5);
            Debug.WriteLine(t6);
            Debug.WriteLine(t7);

                //[0:] Name eq 'Task' or Name eq '1'&$count=true
                //[0:] Name eq 'Task' or Name eq '1' and Id eq 1 and not(IsComplete eq true)&$count=true
                //[0:] Id eq 1&$count=true
                //[0:] not(IsComplete eq true)&$count=true
                //[0:] contains(Name , 'Task')&$count=true
                //[0:] startswith(Name , 'Task')&$count=true
                //[0:] endswith(Name , 'Task')&$count=true
                //[0:] contains(Name , 'Task') and endswith(Name , 'Task')&$count=true

           _deviceService.BeginInvokeOnMainThread(async () =>
           {
               await table.PullAsync("test", t1);
               var data = table.ToList("test");
               var lastData = data.LastOrDefault();

               table.InsertOrUpdate( new TodoItem
               {
                   Id = (lastData?.Id ?? 0) + 1,
                   Name = "New Test Task",
                   LastUpdated = DateTime.Now
               });

               await table.PushAsync();
               await table.PullAsync("test", t1);
               data = table.ToList("test");
           });
        }
    }
}
