# Infinite Scroll

Implementing your Infinite Scroll is now easy and straightforward with Matcha Sync, it is very similar to what we have with Pagination.

## In Viewmodel's 
This is the viewmodel's point of view, in our case we put this on LoadTask method
 ```csharp
//get the client instance
var client = MobileServiceClient.Instance;

//get the synctable
var todoTable = client.GetSyncTable<TodoItem>();


var query = _crudTodotTable.CreateQuery()
                                .Skip(((Page - 1) * RecordPerPage))
                                .Take(RecordPerPage);

await _crudTodotTable.PullAsync("getinfoquery", query);
var data = _crudTodotTable.ToList("getinfoquery");
var recordNumber = _crudTodotTable.RecordCount("getinfoquery");

NumberPages = (int) Math.Ceiling(recordNumber / RecordPerPage);

foreach (var todoItem in data)
{
    TodoItems.Add(todoItem);
}

Page++; //Increment Page when scrolling bottom
 ```

## In View's backend code 

 ```csharp
 public partial class InfiniteScrollPage : ContentPage
 {
     private InfiniteScrollPageViewModel _vm;
     private int _lastItemAppearedIdx;

     public InfiniteScrollPage()
     {
         InitializeComponent();

         _vm = (InfiniteScrollPageViewModel)BindingContext;
     }


	 ....Code here subscribing and unsubscribing to event
	 

     private async void TodoListView_ItemAppearing(object sender, ItemVisibilityEventArgs e)
     {
         var currentIdx = _vm.TodoItems.IndexOf(e.Item as TodoItem);
         
         //this is where the magic happens
         if(currentIdx == _vm.TodoItems.Count -1)
             await _vm.LoadTasks();
     }
 }
 ```

For a complete reference, we have this on our Mobile sample, [LINK HERE](https://github.com/winstongubantes/matchasync/tree/master/Sample/SampleMobile/SampleMobile/ViewModels/InfiniteScrollPageViewModel.cs)

<= Back to [Table of Contents](README.md)