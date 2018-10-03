using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Matcha.Sync.Mobile;
using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;
using SampleMobile2.Models;
using SampleMobile2.ViewModels.Base;
using Xamarin.Forms;

namespace SampleMobile2.ViewModels
{
    public class ODataQueryStatusPageViewModel : BindableBase
    {
        #region Fields
        private readonly IMobileServiceClient _mobileServiceClient;
        private readonly IMobileServiceCrudTable<TodoItem> _crudTodotTable;
        #endregion

        #region Ctor
        public ODataQueryStatusPageViewModel()
        {
            _mobileServiceClient = MobileServiceClient.Instance;
            _crudTodotTable = _mobileServiceClient.GetSyncTable<TodoItem>();

            Device.BeginInvokeOnMainThread(async () =>
            {
                await LoadTasks();
            });
        }
        #endregion

        #region Commands
        private ICommand _syncCommand;
        public ICommand SyncCommand => _syncCommand ?? (_syncCommand = new Command(async () => await SyncToServer()));
        #endregion

        #region Private Methods
        private async Task LoadTasks()
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await Application.Current.MainPage.DisplayAlert("", "No Internet!", "Ok");
                return;
            }

            IsBusy = true;

            //LOADS ONLY COMPLETED TODOITEM
            var query = _crudTodotTable.CreateQuery()
                                       .Where(e => e.IsComplete);

            try
            {
                await _crudTodotTable.PullAsync("testquerycomplete", query);
                var data = _crudTodotTable.ToList("testquerycomplete");
                TodoItems = new ObservableCollection<TodoItem>(data);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("", "No Internet!", "Ok");
            }


            IsBusy = false;
        }

        private async Task SyncToServer()
        {
            IsBusy = true;

            //this is done due to , no trigger bound for changing status
            UpdateUnsyncData();

            if (CrossConnectivity.Current.IsConnected)
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

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
        #endregion
    }
}
