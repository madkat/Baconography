using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaconographyPortable.Messages
{
    public class InvalidateSubredditManagmentStateMessage : MessageBase
    {
        public bool InvalidateSubscribed { get; set; }
        public bool InvalidatePivot { get; set; }
    }
}
