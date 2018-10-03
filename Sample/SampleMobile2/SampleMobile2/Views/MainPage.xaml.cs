using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SampleMobile2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void Button_OnClicked(object sender, EventArgs e)
        {
            if (!(sender is Button button)) return;

            switch (button.Text)
            {
                case "Todo List (OData)":
                    Navigation.PushAsync(new ODataQuerySyncPage());
                    break;
                case "Todo-Complete (OData)":
                    Navigation.PushAsync(new ODataQueryStatusPage());
                    break;
                case "Todo List (WebApi)":
                    Navigation.PushAsync(new WebApiSyncPage());
                    break;
                case "OData Function Call":
                    Navigation.PushAsync(new ODataFunctionPage());
                    break;
                case "WebApi Method Call":
                    Navigation.PushAsync(new WebApiMethodPage());
                    break;
                case "Pagination":
                    Navigation.PushAsync(new PaginationPage());
                    break;
                case "Infinite Scroll":
                    Navigation.PushAsync(new InfiniteScrollPage());
                    break;
            }
        }
    }
}