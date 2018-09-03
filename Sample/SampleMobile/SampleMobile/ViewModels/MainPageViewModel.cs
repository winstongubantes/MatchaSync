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
        #region Fields
        private readonly IMobileServiceClient _mobileServiceClient;
        private readonly IDeviceService _deviceService;
        private readonly IMobileServiceCrudTable<TodoItem> _crudTodotTable;
        private readonly IConnectivity _connectivity;
        private readonly IPageDialogService _dialogService;
        #endregion

        #region Ctor
        public MainPageViewModel(
            INavigationService navigationService,
            IMobileServiceClient mobileServiceClient,
            IDeviceService deviceService,
            IConnectivity connectivity,
            IPageDialogService dialogService)
            : base(navigationService)
        {
            _mobileServiceClient = mobileServiceClient;
            _deviceService = deviceService;
            _connectivity = connectivity;
            _dialogService = dialogService;
            _crudTodotTable = _mobileServiceClient.GetSyncTable<TodoItem>();
        } 
        #endregion

        #region Commands
        private ICommand _toggleCompleteCommand;
        public ICommand ToggleCompleteCommand => _toggleCompleteCommand ?? (_toggleCompleteCommand = new DelegateCommand<TodoItem>(ToggleComplete));

        private ICommand _syncCommand;
        public ICommand SyncCommand => _syncCommand ?? (_syncCommand = new DelegateCommand(async () => await SyncToServer()));

        private ICommand _addTaskCommand;
        public ICommand AddTaskCommand => _addTaskCommand ?? (_addTaskCommand = new DelegateCommand(async () => await CreateNewTask()));

        #endregion

        #region Public Methods
        public override void OnNavigatedTo(NavigationParameters parameters)
        {
            _deviceService.BeginInvokeOnMainThread(async () =>
            {
                await LoadTasks();
            });
        } 
        #endregion

        #region Private Methods
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

            var notSynced = _crudTodotTable.ToList("testquery").Where(e=> !e.IsSynced);

            if (_connectivity.IsConnected)
            {
                await _mobileServiceClient.SyncAllData();
            }

            var notSynced2 = _crudTodotTable.ToList("testquery").Where(e => !e.IsSynced);

            //refresh locally
            var data = _crudTodotTable.ToList("testquery");
            TodoItems = new ObservableCollection<TodoItem>(data);

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
        #endregion

        #region Properties
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
        #endregion
    }
}
