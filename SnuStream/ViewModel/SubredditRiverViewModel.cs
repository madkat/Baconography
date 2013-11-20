using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnuStream.ViewModel
{
    public class SubredditRiverViewModel : ViewModelBase
    {
        private string _subreddit;
        private SnooStreamViewModel _parent;

        public SubredditRiverViewModel(string subreddit, SnooStreamViewModel snooStreamViewModel)
        {
            _subreddit = subreddit;
            _parent = snooStreamViewModel;
        }
    }
}
