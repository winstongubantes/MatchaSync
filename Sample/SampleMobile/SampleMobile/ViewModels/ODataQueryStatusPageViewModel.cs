using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Matcha.Sync.Mobile;
using Plugin.Connectivity.Abstractions;
using Prism.Navigation;
using Prism.Services;
using SampleMobile.Models;

namespace SampleMobile.ViewModels
{
	public class ODataQueryStatusPageViewModel : ViewModelBase
    {
        #region Fields
        private readonly IMobileServiceClient _mobileServiceClient;
        private readonly IDeviceService _deviceService;
        private readonly IMobileServiceCrudTable<TodoItem> _crudTodotTable;
        private readonly IConnectivity _connectivity;
        private readonly IPageDialogService _dialogService;
        #endregion

        #region Ctor
        public ODataQueryStatusPageViewModel(
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
        private ICommand _syncCommand;
        public ICommand SyncCommand => _syncCommand ?? (_syncCommand = new DelegateCommand(async () => await SyncToServer()));
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

            //LOADS ONLY COMPLETED TODOITEM
            var query = _crudTodotTable.CreateQuery()
                                       .Where(e=> e.IsComplete);

            try
            {
                await _crudTodotTable.PullAsync("testquerycomplete", query);
                var data = _crudTodotTable.ToList("testquerycomplete");
                TodoItems = new ObservableCollection<TodoItem>(data);
            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlertAsync("", "No Internet!", "Ok");
            }


            IsBusy = false;
        }

        private async Task SyncToServer()
        {
            IsBusy = true;

            //this is done due to , no trigger bound for changing status
            UpdateUnsyncData();

            if (_connectivity.IsConnected)
            {
                try
                {
                    await _mobileServiceClient.SyncAllData();
                }
                catch (Exception ex)
                {
                    await _dialogService.DisplayAlertAsync("", "No Internet!", "Ok");
                }
            }

            //refresh locally
            var data = _crudTodotTable.ToList("testquerycomplete");
            TodoItems = new ObservableCollection<TodoItem>(data);

            IsBusy = false;
        }

        private void UpdateUnsyncData()
        {
            var unSyncList = TodoItems.Where(e => !e.IsSynced);

            foreach (var todoItem in unSyncList)
            {
                _crudTodotTable.InsertOrUpdate(todoItem);
            }
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
