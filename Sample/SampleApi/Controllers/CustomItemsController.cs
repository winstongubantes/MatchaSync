using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SampleApi.Models;

namespace SampleApi.Controllers
{
    [Produces("application/json")]
    [Route("api/CustomItems")]
    public class CustomItemsController : Controller
    {
        private TodoItemContext _db;
        private bool _isSoftDelete;

        public CustomItemsController(TodoItemContext db)
        {
            _db = db;
        }

        public IActionResult Get()
        {
            var list = _db.TodoItems.ToList();

            var result = new WebApiResult<TodoItem>
            {
                Count = list.Count,
                DataList = list
            };

            return Ok(result);
        }

        [HttpGet]
        [Route("GetComplete")]
        public IActionResult GetComplete()
        {
            var list = _db.TodoItems.Where(e => e.IsComplete).ToList();

            var result = new WebApiResult<TodoItem>
            {
                Count = list.Count,
                DataList = list
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] object obj)
        {
            var dataList = JsonConvert.DeserializeObject<IList<TodoItem>>(obj.ToString());
            var result = dataList;

            foreach (var data in dataList)
            {
                if (data.IsDeleted)
                    await Delete(data);
                else if (data.Id == 0)
                    await Insert(data);
                else
                    await Update(data);
            }

            return Ok("Success");
        }

        [HttpPost]
        [Route("GetSalesTaxRate")]
        public IActionResult GetSalesTaxRate([FromBody] int postalCode)
        {
            double rate = 5.6;  // Use a fake number for the sample.
            return Ok(rate);
        }

        private async Task Delete(TodoItem data)
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

        private async Task Insert(TodoItem data)
        {
            _db.TodoItems.Add(data);
            await _db.SaveChangesAsync();
        }

        private async Task Update(TodoItem data)
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
    }
}