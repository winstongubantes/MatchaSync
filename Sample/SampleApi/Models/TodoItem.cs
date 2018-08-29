using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Matcha.Sync.Model;

namespace SampleApi.Models
{
    public class TodoItem : Synchronizable
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsComplete { get; set; }
    }
}
