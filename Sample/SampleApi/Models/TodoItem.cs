using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Matcha.Sync.Model;

namespace SampleApi.Models
{
    public class TodoItem : Synchronizable
    {
        public string Name { get; set; }
        public bool IsComplete { get; set; }
    }
}
