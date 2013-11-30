using GalaSoft.MvvmLight;
using SnooStream.Common;
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
        private LinkRiverViewModel _context;
        private NeverEndingRedditEnumerator _enumerator;
        public LinkStreamViewModel(LinkRiverViewModel context, string linkId)
        {
            _context = context;
            int linkIndex = -1;
            if(linkId != null)
            {
                for (int i = 0; i < _context.Links.Count; i++)
                {
                    if (_context.Links[i].Link.Id == linkId)
                    {
                        linkIndex = i;
                        break;
                    }
                }
            }
            _enumerator = new NeverEndingRedditEnumerator(_context, linkIndex, true);
        }

        public LinkViewModel Current { get; private set; }

        public async Task<bool> MoveNext()
        {
            try
            {
                var result = await _enumerator.Next();
                if (result is LinkViewModel)
                {
                    Current = result as LinkViewModel;
                    return true;
                }
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
