using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
	    private ICommand _prevCommand;
	    public ICommand PrevCommand => _prevCommand ?? (_prevCommand = new DelegateCommand(async () =>
	    {
            if(Page <= 0) return;
	        Page--;
            await LoadTasks();
	    }));


	    private ICommand _nextCommand;
	    public ICommand NextCommand => _nextCommand ?? (_nextCommand = new DelegateCommand(async () =>
	    {
	        if (Page >= NumberPages) return;
	        Page++;
	        await LoadTasks();
        }));

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
        #endregion
    }
}
