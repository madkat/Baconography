using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnooStream.ViewModel
{
    public class RiverViewModel : ViewModelBase
    {
        //need to come up with an init blob setup for this, meaining a per river blob
        public string Subreddit { get; private set; }
        public string Sort { get; private set; }
        public string Title { get; private set; }
        public bool Loading { get; private set; }
        public RiverViewModel(string subreddit, string sort)
        {
        }

        public ObservableCollection<LinkViewModel> Content { get; set; }
        public async Task LoadMore()
        {

        }
    }
}
