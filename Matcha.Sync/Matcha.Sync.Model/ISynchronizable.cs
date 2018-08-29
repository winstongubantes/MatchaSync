using System;

namespace Matcha.Sync.Model
{
    public interface ISynchronizable
    {
        long Id { get; set; } //THIS IS SERVER ID
        string LocalId { get; set; } //THIS IS GUID FOR MOBILE USE
        string UserId { get; set; } //THIS IS GUID
        string QueryId { get; set; }
        bool IsSynced { get; set; }
        bool IsDeleted { get; set; }
        DateTime LastUpdated { get; set; }
    }
}
