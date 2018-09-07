using SampleMobile.Models;
using SampleMobile.ViewModels;
using Xamarin.Forms;

namespace SampleMobile.Views
{
    public partial class InfiniteScrollPage : ContentPage
    {
        private InfiniteScrollPageViewModel _vm;
        private int _lastItemAppearedIdx;

        public InfiniteScrollPage()
        {
            InitializeComponent();

            _vm = (InfiniteScrollPageViewModel)BindingContext;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            TodoListView.ItemAppearing += TodoListView_ItemAppearing;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            TodoListView.ItemAppearing -= TodoListView_ItemAppearing;
        }

        private async void TodoListView_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            var currentIdx = _vm.TodoItems.IndexOf(e.Item as TodoItem);
            
            if(currentIdx == _vm.TodoItems.Count -1)
                await _vm.LoadTasks();

            //if (!IsBusy)
            //    await _vm.LoadTasks();
        }
    }
}
