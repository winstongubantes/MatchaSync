using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Matcha.Sync.Mobile;
using Plugin.Connectivity.Abstractions;
using SampleMobile2.Models;
using SampleMobile2.ViewModels.Base;
using Xamarin.Forms;

namespace SampleMobile2.ViewModels
{
    public class WebApiSyncPageViewModel : BindableBase
    {
        #region Fields
        private readonly IMobileServiceClient _mobileServiceClient;
        private readonly IMobileServiceCrudTable<CustomItem> _crudTodotTable;
        private readonly IConnectivity _connectivity;
        #endregion

        #region Ctor
        public WebApiSyncPageViewModel()
        {
            _mobileServiceClient = MobileServiceClient.Instance;
            _crudTodotTable = _mobileServiceClient.GetSyncTable<CustomItem>();

            Device.BeginInvokeOnMainThread(async () =>
            {
                await LoadTasks();
            });
        }
        #endregion

        #region Commands
        private ICommand _syncCommand;
        public ICommand SyncCommand => _syncCommand ?? (_syncCommand = new Command(async () => await SyncToServer()));

        private ICommand _addTaskCommand;
        public ICommand AddTaskCommand => _addTaskCommand ?? (_addTaskCommand = new Command(async () => await CreateNewTask()));

        #endregion

        #region Private Methods
        private async Task LoadTasks()
        {
            if (!_connectivity.IsConnected)
            {
                await Application.Current.MainPage.DisplayAlert("", "No Internet!", "Ok");
                return;
            }

            IsBusy = true;

            try
            {
                await _crudTodotTable.PullAsync("apiquery", "GetComplete");
                var data = _crudTodotTable.ToList("apiquery");
                TodoItems = new ObservableCollection<CustomItem>(data);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("", "No Internet!", "Ok");
            }


            IsBusy = false;
        }

        private async Task CreateNewTask()
        {
            var lastData = TodoItems.LastOrDefault();

            if (_connectivity.IsConnected && !string.IsNullOrWhiteSpace(NewTaskValue))
            {
                _crudTodotTable.InsertOrUpdate(new CustomItem
                {
                    Id = (lastData?.Id ?? 0) + 1,
                    Name = NewTaskValue,
                    LastUpdated = DateTime.Now
                });

                NewTaskValue = string.Empty;
                IsBusy = true;

                try
                {
                    await _crudTodotTable.PushAsync();
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("", "No Internet!", "Ok");
                }

                IsBusy = false;
            }

            await LoadTasks();
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
                    await Application.Current.MainPage.DisplayAlert("", "No Internet!", "Ok");
                }
            }

            //refresh locally
            var data = _crudTodotTable.ToList("apiquery");
            TodoItems = new ObservableCollection<CustomItem>(data);

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

        private ObservableCollection<CustomItem> _todoItems;

        public ObservableCollection<CustomItem> TodoItems
        {
            get => _todoItems;
            set => SetProperty(ref _todoItems, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
        #endregion
    }
}
