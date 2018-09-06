using Matcha.Sync.Mobile;
using Plugin.Connectivity;
using Prism;
using Prism.Ioc;
using SampleMobile.ViewModels;
using SampleMobile.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Prism.DryIoc;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace SampleMobile
{
    public partial class App : PrismApplication
    {
        public App() : this(null) { }

        public App(IPlatformInitializer initializer) : base(initializer) { }

        protected override async void OnInitialized()
        {
            InitializeComponent();

            await NavigationService.NavigateAsync("NavigationPage/MainPage");
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //Services
            containerRegistry.RegisterInstance(MobileServiceClient.Instance);
            containerRegistry.RegisterInstance(CrossConnectivity.Current);

            //Pages
            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<MainPage>();
            containerRegistry.RegisterForNavigation<ODataQuerySyncPage>();
            containerRegistry.RegisterForNavigation<ODataQueryStatusPage>();
            containerRegistry.RegisterForNavigation<ODataFunctionPage>();
            containerRegistry.RegisterForNavigation<WebApiMethodPage>();
            containerRegistry.RegisterForNavigation<WebApiSyncPage>();
            containerRegistry.RegisterForNavigation<PaginationPage>();
        }
    }
}
