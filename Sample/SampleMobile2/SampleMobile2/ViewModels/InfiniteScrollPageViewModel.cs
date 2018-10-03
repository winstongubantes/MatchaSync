using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Matcha.Sync.Mobile;
using Plugin.Connectivity;
using SampleMobile2.Models;
using SampleMobile2.ViewModels.Base;
using Xamarin.Forms;

namespace SampleMobile2.ViewModels
{
    public class InfiniteScrollPageViewModel : BindableBase
    {
        private readonly IMobileServiceClient _mobileServiceClient;
        private readonly IMobileServiceCrudTable<TodoItem> _crudTodotTable;

        #region Public Methods

        public InfiniteScrollPageViewModel()
        {
            _mobileServiceClient = MobileServiceClient.Instance;
            _crudTodotTable = _mobileServiceClient.GetSyncTable<TodoItem>();

            Device.BeginInvokeOnMainThread(async () =>
            {
                TodoItems = new ObservableCollection<TodoItem>();
                await LoadTasks();
            });
        }

        public async Task LoadTasks()
        {
            if (!CrossConnectivity.Current.IsConnected)
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
                if (Page > NumberPages)
                {
                    IsBusy = false;
                    return;
                }

                await _crudTodotTable.PullAsync("getinfoquery", query);
                var data = _crudTodotTable.ToList("getinfoquery");
                var recordNumber = _crudTodotTable.RecordCount("getinfoquery");

                NumberPages = (int)Math.Ceiling((double)recordNumber / RecordPerPage);

                foreach (var todoItem in data)
                {
                    TodoItems.Add(todoItem);
                }

                Page++;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("", ex.Message, "Ok");
            }


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

        private int _numberPages = 1;

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
