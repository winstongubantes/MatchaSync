using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    public class PaginationPageViewModel : BindableBase
    {
        #region Fields
        private readonly IMobileServiceClient _mobileServiceClient;
        private readonly IMobileServiceCrudTable<TodoItem> _crudTodotTable;
        private readonly IConnectivity _connectivity;
        #endregion

        #region Ctor
        public PaginationPageViewModel()
        {
            _mobileServiceClient = MobileServiceClient.Instance;
            _connectivity = CrossConnectivity.Current;
            _crudTodotTable = _mobileServiceClient.GetSyncTable<TodoItem>();

            Device.BeginInvokeOnMainThread(async () =>
            {
                await LoadTasks();
            });
        }
        #endregion

        #region Commands
        private ICommand _prevCommand;
        public ICommand PrevCommand => _prevCommand ?? (_prevCommand = new Command(async () =>
        {
            if (Page <= 0) return;
            Page--;
            await LoadTasks();
        }));


        private ICommand _nextCommand;
        public ICommand NextCommand => _nextCommand ?? (_nextCommand = new Command(async () =>
        {
            if (Page >= NumberPages) return;
            Page++;
            await LoadTasks();
        }));

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

            var query = _crudTodotTable.CreateQuery()
                                .Skip(((Page - 1) * RecordPerPage))
                                .Take(RecordPerPage);

            try
            {
                await _crudTodotTable.PullAsync($"getinfoquery{Page}", query);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            var data = _crudTodotTable.ToList($"getinfoquery{Page}");
            var recordNumber = _crudTodotTable.RecordCount($"getinfoquery{Page}");

            NumberPages = (int)Math.Ceiling((double)recordNumber / RecordPerPage);

            TodoItems = new ObservableCollection<TodoItem>(data);


            IsBusy = false;
        }
        #endregion

        #region Properties
        private int _recordPerPage = 10;

        public int RecordPerPage
        {
            get => _recordPerPage;
            set => SetProperty(ref _recordPerPage, value);
        }

        private int _page = 1;

        public int Page
        {
            get => _page;
            set => SetProperty(ref _page, value);
        }

        private int _numberPages = 0;

        public int NumberPages
        {
            get => _numberPages;
            set => SetProperty(ref _numberPages, value);
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
