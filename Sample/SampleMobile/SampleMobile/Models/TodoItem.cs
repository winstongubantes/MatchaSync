using Matcha.Sync.Model;

namespace SampleMobile.Models
{
    public class TodoItem : Synchronizable
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsComplete { get; set; }
    }
}
