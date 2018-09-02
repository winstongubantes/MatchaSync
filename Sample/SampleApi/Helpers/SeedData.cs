using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SampleApi.Models;

namespace SampleApi.Helpers
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<TodoItemContext>();
            context.Database.EnsureCreated();
            if (context.TodoItems.Any()) return;

            var dateCreated = DateTime.Now;

            for (var i = 1; i <= 100; i++)
            {
                context.TodoItems.Add(new TodoItem
                {
                    Id = i,
                    IsComplete = i % 2 == 0,
                    Name = $"Task {i}",
                    LastUpdated = dateCreated,
                    LocalId = Guid.NewGuid().ToString()
                });
            }

            context.SaveChanges();
        }
    }
}
