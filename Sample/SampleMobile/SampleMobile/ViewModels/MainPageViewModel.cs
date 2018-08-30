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

            table.PullAsync("test", table.CreateQuery().Where(e=> e.Name == "test"));
        }
    }
}
