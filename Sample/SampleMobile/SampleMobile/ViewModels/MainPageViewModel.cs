using System.Windows.Input;
using Prism.Commands;
using Prism.Navigation;
using SampleMobile.Views;

namespace SampleMobile.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;

        private ICommand _showFullSyncSampleCommand;

        private ICommand _showFullSyncWebApiSampleCommand;

        private ICommand _showInfiniteScrollPageCommand;

        private ICommand _showODataCallCommand;

        private ICommand _showPaginationPageCommand;

        private ICommand _showStatusSyncSampleCommand;

        private ICommand _showWebApiCallCommand;


        public MainPageViewModel(
            INavigationService navigationService)
            : base(navigationService)
        {
            _navigationService = navigationService;
        }

        public ICommand ShowFullSyncSampleCommand => _showFullSyncSampleCommand ?? (_showFullSyncSampleCommand =
                                                         new DelegateCommand(() =>
                                                         {
                                                             _navigationService.NavigateAsync(
                                                                 nameof(ODataQuerySyncPage));
                                                         }));

        public ICommand ShowFullSyncWebApiSampleCommand => _showFullSyncWebApiSampleCommand ??
                                                           (_showFullSyncWebApiSampleCommand = new DelegateCommand(() =>
                                                           {
                                                               _navigationService.NavigateAsync(
                                                                   nameof(WebApiSyncPage));
                                                           }));

        public ICommand ShowStatusSyncSampleCommand => _showStatusSyncSampleCommand ?? (_showStatusSyncSampleCommand =
                                                           new DelegateCommand(() =>
                                                           {
                                                               _navigationService.NavigateAsync(
                                                                   nameof(ODataQueryStatusPage));
                                                           }));

        public ICommand ShowODataCallCommand => _showODataCallCommand ?? (_showODataCallCommand =
                                                    new DelegateCommand(() =>
                                                    {
                                                        _navigationService.NavigateAsync(nameof(ODataFunctionPage));
                                                    }));

        public ICommand ShowWebApiCallCommand => _showWebApiCallCommand ?? (_showWebApiCallCommand =
                                                     new DelegateCommand(() =>
                                                     {
                                                         _navigationService.NavigateAsync(nameof(WebApiMethodPage));
                                                     }));

        public ICommand ShowPaginationPageCommand => _showPaginationPageCommand ?? (_showPaginationPageCommand =
                                                         new DelegateCommand(() =>
                                                         {
                                                             _navigationService.NavigateAsync(
                                                                 nameof(PaginationPage));
                                                         }));

        public ICommand ShowInfiniteScrollPageCommand => _showInfiniteScrollPageCommand ??
                                                         (_showInfiniteScrollPageCommand = new DelegateCommand(() =>
                                                         {
                                                             _navigationService.NavigateAsync(
                                                                 nameof(InfiniteScrollPage));
                                                         }));
    }
}