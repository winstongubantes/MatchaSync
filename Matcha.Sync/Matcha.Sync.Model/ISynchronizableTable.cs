using System;

namespace Matcha.Sync.Model
{
    public interface ISynchronizableTable
    {
        string LocalId { get; set; } //THIS IS GUID
        string UserId { get; set; } //THIS IS GUID
        string QueryId { get; set; }
        bool IsSynced { get; set; }
        bool IsDeleted { get; set; }
        DateTime LastUpdated { get; set; }
    }
}
