using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Matcha.Sync.Api;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using SampleApi.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SampleApi.Controllers
{
    public class TodoItemsController : BaseController<TodoItem>
    {
        private TodoItemContext _db;
        private bool _isSoftDelete;

        public TodoItemsController(TodoItemContext db)
        {
            _db = db;
        }

        public override IActionResult Get() => Ok(_db.TodoItems);

        protected override async Task Delete(TodoItem data)
        {
            var todo = _db.TodoItems.Find(data.Id);
            if (todo == null) return;

            //if you prefer soft delete then set the property is delete to true

            if (_isSoftDelete) //This might be in your applications config
            {
                todo.IsDeleted = true;
                _db.TodoItems.Update(todo);
            }
            else
            {
                _db.TodoItems.Remove(todo);
            }

            await _db.SaveChangesAsync();
        }

        protected override async Task Insert(TodoItem data)
        {
            _db.TodoItems.Add(data);
            await _db.SaveChangesAsync();
        }

        protected override async Task Update(TodoItem data)
        {
            var todo = _db.TodoItems.Find(data.Id);
            if (todo == null)
            {
                await Insert(data);
                return;
            }

            //You can use AUTOMAPPER instead doing it manual
            //but in our example i just keep it simple
            todo.IsComplete = data.IsComplete;
            todo.Name = data.Name;
            todo.LastUpdated = data.LastUpdated;
            todo.IsSynced = true;

            _db.TodoItems.Update(todo);
            await _db.SaveChangesAsync();
        }

        [HttpPost]
        [ODataRoute("GetSalesTaxRate(PostalCode={postalCode})")]
        public IActionResult GetSalesTaxRate([FromODataUri] int postalCode)
        {
            double rate = 5.6;  // Use a fake number for the sample.
            return Ok(rate);
        }
    }
}
