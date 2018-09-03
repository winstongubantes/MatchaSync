using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Matcha.Sync.Mobile;
using Plugin.Connectivity.Abstractions;
using Prism.Services;
using SampleMobile.Models;

namespace SampleMobile.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly IMobileServiceClient _mobileServiceClient;
        private readonly IDeviceService _deviceService;
        private readonly IMobileServiceCrudTable<TodoItem> _crudTodotTable;
        private readonly IConnectivity _connectivity;
        private readonly IPageDialogService _dialogService;

        public MainPageViewModel(
            INavigationService navigationService, 
            IMobileServiceClient mobileServiceClient, 
            IDeviceService deviceService, 
            IConnectivity connectivity, 
            IPageDialogService dialogService) 
            : base (navigationService)
        {
            _mobileServiceClient = mobileServiceClient;
            _deviceService = deviceService;
            _connectivity = connectivity;
            _dialogService = dialogService;
            _crudTodotTable = _mobileServiceClient.GetSyncTable<TodoItem>();
        }

        public override void OnNavigatedTo(NavigationParameters parameters)
        {
            _deviceService.BeginInvokeOnMainThread(async () =>
            {
                await LoadTasks();
            });
        }

        private async Task LoadTasks()
        {
            if (!_connectivity.IsConnected)
            {
                await _dialogService.DisplayAlertAsync("", "No Internet!", "Ok");
                return;
            }

            IsBusy = true;

            var query = _crudTodotTable.CreateQuery();
                //.Where(e => !e.IsComplete)
                //.Take(20);

            await _crudTodotTable.PullAsync("testquery", query);
            var data = _crudTodotTable.ToList("testquery");
            TodoItems = new ObservableCollection<TodoItem>(data);

            IsBusy = false;
        }

        private ICommand _toggleCompleteCommand;
        public ICommand ToggleCompleteCommand => _toggleCompleteCommand ?? (_toggleCompleteCommand = new DelegateCommand<TodoItem>(ToggleComplete));

        private ICommand _syncCommand;
        public ICommand SyncCommand => _syncCommand ?? (_syncCommand = new DelegateCommand(async () => await SyncToServer()));

        private ICommand _addTaskCommand;
        public ICommand AddTaskCommand => _addTaskCommand ?? (_addTaskCommand = new DelegateCommand(async ()=> await CreateNewTask()));

        private async Task CreateNewTask()
        {
            var lastData = TodoItems.LastOrDefault();

            if (_connectivity.IsConnected && !string.IsNullOrWhiteSpace(NewTaskValue))
            {
                _crudTodotTable.InsertOrUpdate(new TodoItem
                {
                    Id = (lastData?.Id ?? 0) + 1,
                    Name = NewTaskValue,
                    LastUpdated = DateTime.Now
                });

                NewTaskValue = string.Empty;
                IsBusy = true;
                await _crudTodotTable.PushAsync();
                IsBusy = false;
            }

           
            await LoadTasks();
        }

        private async Task SyncToServer()
        {
            IsBusy = true;
            await _mobileServiceClient.SyncAllData();
            IsBusy = false;
        }

        private void ToggleComplete(TodoItem item)
        {
            item.IsComplete = !item.IsComplete;
            _crudTodotTable.InsertOrUpdate(item);

            //refresh locally
            var data = _crudTodotTable.ToList("testquery");
            TodoItems = new ObservableCollection<TodoItem>(data);
        }

        private string _newTaskValue;

        public string NewTaskValue
        {
            get => _newTaskValue;
            set => SetProperty(ref _newTaskValue, value);
        }

        private ObservableCollection<TodoItem> _todoItems;

        public ObservableCollection<TodoItem> TodoItems
        {
            get => _todoItems;
            set => SetProperty(ref _todoItems, value);
        }
    }
}
