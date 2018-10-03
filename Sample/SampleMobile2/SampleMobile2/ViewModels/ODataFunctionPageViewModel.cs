using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Matcha.Sync.Mobile;
using SampleMobile2.Models;
using SampleMobile2.ViewModels.Base;
using Xamarin.Forms;

namespace SampleMobile2.ViewModels
{
    public class ODataFunctionPageViewModel : BindableBase
    {
        private readonly IMobileServiceClient _mobileServiceClient;
        private readonly IMobileServiceCrudTable<TodoItem> _crudTodotTable;

        public ODataFunctionPageViewModel()
        {
            _mobileServiceClient = MobileServiceClient.Instance;
            _crudTodotTable = _mobileServiceClient.GetSyncTable<TodoItem>();
        }

        private ICommand _calculateCommand;
        public ICommand CalculateCommand => _calculateCommand ?? (_calculateCommand = new Command(async () =>
        {
            try
            {
                var valueResult = await _crudTodotTable.PostWebDataAsync<Dictionary<string, string>>(null, $"GetSalesTaxRate(PostalCode={PostalCode})");
                ResultValue = valueResult["value"];
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("", "No Internet!", "Ok");
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
