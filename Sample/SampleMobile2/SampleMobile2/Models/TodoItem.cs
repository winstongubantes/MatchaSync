using System;
using System.Collections.Generic;
using System.Text;

namespace SampleMobile2.Models
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
                _isComplete = value;
                OnPropertyChanged();
            }
        }
    }
}
