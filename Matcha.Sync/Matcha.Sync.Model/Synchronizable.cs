using System;

namespace Matcha.Sync.Model
{
    public class Synchronizable : ISynchronizable
    {
        public long Id { get; set; } //THIS IS SERVER ID
        public string LocalId { get; set; } //THIS IS GUID FOR MOBILE USE
        public bool IsSynced { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
