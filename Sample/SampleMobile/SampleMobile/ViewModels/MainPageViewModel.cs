using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Matcha.Sync.Mobile;
using Plugin.Connectivity.Abstractions;
using Prism.Services;
using SampleMobile.Models;
using SampleMobile.Views;

namespace SampleMobile.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;


        public MainPageViewModel(
            INavigationService navigationService)
            : base(navigationService)
        {
            _navigationService = navigationService;
        } 

        private ICommand _showFullSyncSampleCommand;
        public ICommand ShowFullSyncSampleCommand => _showFullSyncSampleCommand ?? (_showFullSyncSampleCommand = new DelegateCommand(() =>
        {
            _navigationService.NavigateAsync(nameof(ODataQuerySyncPage));
        }));

        private ICommand _showStatusSyncSampleCommand;
        public ICommand ShowStatusSyncSampleCommand => _showStatusSyncSampleCommand ?? (_showStatusSyncSampleCommand = new DelegateCommand(() =>
        {
            _navigationService.NavigateAsync(nameof(ODataQueryStatusPage));
        }));

        private ICommand _showODataCallCommand;
        public ICommand ShowODataCallCommand => _showODataCallCommand ?? (_showODataCallCommand = new DelegateCommand(() =>
        {
            _navigationService.NavigateAsync(nameof(ODataFunctionPage));
        }));

        private ICommand _showWebApiCallCommand;
        public ICommand ShowWebApiCallCommand => _showWebApiCallCommand ?? (_showWebApiCallCommand = new DelegateCommand(() =>
        {
            _navigationService.NavigateAsync(nameof(WebApiMethodPage));
        }));
    }
}
