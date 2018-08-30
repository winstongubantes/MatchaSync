using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Matcha.Sync.Api;
using Microsoft.AspNet.OData;
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

        protected override Task Delete(TodoItem data)
        {
            return Task.Run(() =>
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

                _db.SaveChanges();
            });
        }

        protected override Task Insert(TodoItem data)
        {
            return Task.Run(() =>
            {
                _db.TodoItems.Add(data);
                _db.SaveChanges();
            });
        }

        protected override Task Update(TodoItem data)
        {
            return Task.Run(() =>
            {
                var todo = _db.TodoItems.Find(data.Id);
                if (todo == null) return;

                //You can use AUTOMAPPER instead doing it manual
                //but in our example i just keep it simple
                todo.IsComplete = data.IsComplete;
                todo.Name = data.Name;
                todo.LastUpdated = data.LastUpdated;
                todo.IsSynced = true;

                _db.TodoItems.Update(todo);
                _db.SaveChanges();
            });
        }
    }
}
