using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Matcha.Sync.Model;
using Prism.Mvvm;

namespace SampleMobile.Models
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
