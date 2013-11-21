using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnooStream.ViewModel
{
    public class LinkRiverViewModel : ViewModelBase
    {
        //need to come up with an init blob setup for this, meaining a per river blob
        public string Subreddit { get; private set; }
        public string Sort { get; private set; }
        public string Title { get; private set; }
        public bool Loading { get { return _loadingTask != null; } }
        private string LastLinkId { get; set; }
        public LinkRiverViewModel(string subreddit, string sort)
        {
        }

        public ObservableCollection<LinkViewModel> Content { get; set; }
        private Task _loadingTask;
        public Task LoadMore()
        {
            if (_loadingTask == null)
            {
                lock (this)
                {
                    if (_loadingTask == null)
                    {
                        _loadingTask = LoadMoreImpl();
                    }
                }
            }
            return _loadingTask;
        }

        public async Task LoadMoreImpl()
        {
            //clear the loading task when we're done
            _loadingTask = null;
        }
    }
}
