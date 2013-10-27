using BaconographyPortable.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaconographyPortable.Messages
{
    public class SubredditSubscriptionChangeMessage : MessageBase
    {
        public string ChangedUrl { get; set; }
        public AboutSubredditViewModel ViewModel { get; set; } 
        public bool Added { get; set; }
    }
}
