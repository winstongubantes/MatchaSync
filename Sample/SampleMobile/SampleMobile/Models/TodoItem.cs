using Matcha.Sync.Model;

namespace SampleMobile.Models
{
    public class TodoItem : Synchronizable
    {
        public string Name { get; set; }
        public bool IsComplete { get; set; }
    }
}
