using Matcha.Sync.Model;

namespace SampleMobile.Models
{
    public class TodoItem : SyncBindable
    {
        private bool _isComplete;
        public string Name { get; set; }

        public bool IsComplete
        {
            get => _isComplete;
            set
            {
                if (SetProperty(ref _isComplete, value)) IsSynced = false;
            }
        }
    }
}
