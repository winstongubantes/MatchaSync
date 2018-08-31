using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Matcha.Sync.Mobile;
using SampleMobile.Models;

namespace SampleMobile.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly IMobileServiceClient _mobileServiceClient;

        public MainPageViewModel(
            INavigationService navigationService, 
            IMobileServiceClient mobileServiceClient) 
            : base (navigationService)
        {
            _mobileServiceClient = mobileServiceClient;
            var table = _mobileServiceClient.GetSyncTable<TodoItem>();

            //var t = table.CreateQuery().Where(e => e.Name == "Task" || e.Name == "1").Query;
            var t4 = table.CreateQuery()
                .Where(e => e.Name == "Task" || e.Name == "1")
                .Where(e => e.Id == 1)
                .Where(e => !e.IsComplete).Query;
            //var t2 = table.CreateQuery().Where(e => e.Id == 1).Query;
            //var t3 = table.CreateQuery().Where(e => !e.IsComplete).Query;
            //var t4 = table.CreateQuery().Where(e => e.Name.Contains("Task")).Query;
            //var t5 = table.CreateQuery().Where(e => e.Name.StartsWith("Task")).Query;
            //var t6 = table.CreateQuery().Where(e => e.Name.EndsWith("Task")).Query;
            //var t7 = table.CreateQuery().Where(e => e.Name.Contains("Task") && e.Name.EndsWith("Task")).Query;

            //table.PullAsync("test", table.CreateQuery().Where(e=> e.Name == "test"));
        }
    }
}
