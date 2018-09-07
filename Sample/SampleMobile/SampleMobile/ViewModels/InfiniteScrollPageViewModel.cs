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
	public class InfiniteScrollPageViewModel : ViewModelBase
	{
        #region Fields
        private readonly IMobileServiceClient _mobileServiceClient;
        private readonly IDeviceService _deviceService;
        private readonly IMobileServiceCrudTable<TodoItem> _crudTodotTable;
        private readonly IConnectivity _connectivity;
        private readonly IPageDialogService _dialogService;
        #endregion

        #region Ctor
        public InfiniteScrollPageViewModel(
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

        #region Public Methods
        public override void OnNavigatedTo(NavigationParameters parameters)
        {
            _deviceService.BeginInvokeOnMainThread(async () =>
            {
                TodoItems = new ObservableCollection<TodoItem>();
                await LoadTasks();
            });
        }

	    public async Task LoadTasks()
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
	            await _dialogService.DisplayAlertAsync("", ex.Message, "Ok");
	        }


	        IsBusy = false;
	    }
        #endregion

        #region Private Methods

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
        #endregion
    }
}
