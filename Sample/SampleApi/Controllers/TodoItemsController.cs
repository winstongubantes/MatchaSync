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

        public TodoItemsController(TodoItemContext db)
        {
            _db = db;
        }

        public override IActionResult Get() => Ok(_db.TodoItems);

        protected override Task Delete(TodoItem data)
        {
            return Task.FromResult(0);
        }

        protected override Task Insert(TodoItem data)
        {
            return Task.FromResult(0);
        }

        protected override Task Update(TodoItem data)
        {
            return Task.FromResult(0);
        }
    }
}
