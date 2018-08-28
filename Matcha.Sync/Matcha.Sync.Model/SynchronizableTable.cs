using System;

namespace Matcha.Sync.Model
{
    public class SynchronizableTable : ISynchronizableTable
    {
        public string LocalId { get; set; }
        public string UserId { get; set; }
        public string QueryId { get; set; }
        public bool IsSynced { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
