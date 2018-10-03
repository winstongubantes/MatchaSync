using System;
using System.Collections.Generic;
using System.Text;
using Matcha.Sync.Model;
using SampleMobile2.ViewModels.Base;

namespace SampleMobile2.Models
{
    public class SyncBindable : BindableBase, ISynchronizable
    {
        public long Id { get; set; }
        public string LocalId { get; set; }
        public bool IsSynced { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
