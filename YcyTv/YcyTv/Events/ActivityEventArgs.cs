using System;
using System.Collections.Generic;
using System.Text;
using Prism.Events;

namespace YcyTv.Events
{
    public class ActivityEventArgs : PubSubEvent<ActivityEventArgs>
    {
        public string Name { get; }
        private bool Status { get; }
    }
}
