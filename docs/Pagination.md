# Pagination

Implementing your pagination is now easy and straightforward with Matcha Sync.


 ```csharp
//get the client instance
var client = MobileServiceClient.Instance;

//get the synctable
var todoTable = client.GetSyncTable<TodoItem>();


var query = _crudTodotTable.CreateQuery()
                                .Skip(Page)
                                .Take((int)NumberPages);

await _crudTodotTable.PullAsync("getinfoquery", query);
var data = _crudTodotTable.ToList("getinfoquery");
var recordNumber = _crudTodotTable.RecordCount("getinfoquery");

NumberPages = (int) Math.Ceiling(recordNumber / RecordPerPage);
 ```

For a complete reference, we have this on our Mobile sample, [LINK HERE](https://github.com/winstongubantes/matchasync/tree/master/Sample/SampleMobile/SampleMobile/ViewModels/PaginationPageViewModel.cs)

<= Back to [Table of Contents](README.md)