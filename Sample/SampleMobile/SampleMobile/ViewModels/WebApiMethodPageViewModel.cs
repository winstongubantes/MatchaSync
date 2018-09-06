using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Matcha.Sync.Mobile;
using Prism.Services;
using SampleMobile.Models;

namespace SampleMobile.ViewModels
{
	public class WebApiMethodPageViewModel : BindableBase
	{
	    private readonly IMobileServiceClient _mobileServiceClient;
	    private readonly IDeviceService _deviceService;
	    private readonly IPageDialogService _dialogService;
	    private readonly IMobileServiceCrudTable<TodoItem> _crudTodotTable;

	    public WebApiMethodPageViewModel(
	        IMobileServiceClient mobileServiceClient,
	        IDeviceService deviceService,
	        IPageDialogService dialogService)
	    {
	        _mobileServiceClient = mobileServiceClient;
	        _deviceService = deviceService;
	        _dialogService = dialogService;
	        _crudTodotTable = mobileServiceClient.GetSyncTable<TodoItem>();
	    }

	    private ICommand _calculateCommand;
	    public ICommand CalculateCommand => _calculateCommand ?? (_calculateCommand = new DelegateCommand(async () =>
	    {
	        try
	        {
	            var valueResult = await _crudTodotTable.PostWebDataAsync<string>(10, $"CustomItems/GetSalesTaxRate");
	            ResultValue = valueResult;
	        }
	        catch (Exception e)
	        {
	            await _dialogService.DisplayAlertAsync("", "No Internet!", "Ok");
	        }
	    }));


	    private string _resultValue;

	    public string ResultValue
	    {
	        get => _resultValue;
	        set => SetProperty(ref _resultValue, value);
	    }

	    private string _postalCode;

	    public string PostalCode
	    {
	        get => _postalCode;
	        set => SetProperty(ref _postalCode, value);
	    }
    }
}
