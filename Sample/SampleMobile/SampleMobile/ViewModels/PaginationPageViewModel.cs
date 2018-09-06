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
	public class PaginationPageViewModel : ViewModelBase
	{
        #region Fields
        private readonly IMobileServiceClient _mobileServiceClient;
        private readonly IDeviceService _deviceService;
        private readonly IMobileServiceCrudTable<TodoItem> _crudTodotTable;
        private readonly IConnectivity _connectivity;
        private readonly IPageDialogService _dialogService;
        #endregion

        #region Ctor
        public PaginationPageViewModel(
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
        private ICommand _getinfoCommand;
        public ICommand GetInfoCommand => _getinfoCommand ?? (_getinfoCommand = new DelegateCommand(async () => await GetInfo()));

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

            var query = _crudTodotTable.CreateQuery()
                                .Skip(((Page - 1) * RecordPerPage))
                                .Take(RecordPerPage);

            try
            {
                await _crudTodotTable.PullAsync("getinfoquery", query);
                var data = _crudTodotTable.ToList("getinfoquery");
                var recordNumber = _crudTodotTable.RecordCount("getinfoquery");

                NumberPages = (int) Math.Ceiling((double)recordNumber / RecordPerPage);

                TodoItems = new ObservableCollection<TodoItem>(data);
            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlertAsync("", ex.Message, "Ok");
            }


            IsBusy = false;
        }

        private async Task GetInfo()
        {
            IsBusy = true;

            if (_connectivity.IsConnected)
            {
                try
                {
                    //await _mobileServiceClient.SyncAllData();
                    await LoadTasks();
                }
                catch (Exception ex)
                {
                    await _dialogService.DisplayAlertAsync("", "No Internet!", "Ok");
                }
            }

            //refresh locally
            var data = _crudTodotTable.ToList("getinfoquery");
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
        #endregion
    }
}
