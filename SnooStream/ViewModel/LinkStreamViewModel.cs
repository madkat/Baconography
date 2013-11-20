using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnooStream.ViewModel
{
    //provides current link position for flip view
    //provides load more behavior
    public class LinkStreamViewModel : ViewModelBase
    {
        private SubredditRiverViewModel _context;

        public LinkStreamViewModel(SubredditRiverViewModel context, string linkId)
        {
            _context = context;
        }
    }
}
