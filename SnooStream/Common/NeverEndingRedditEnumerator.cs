using GalaSoft.MvvmLight;
using SnooStream.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnooStream.Common
{
    class NeverEndingRedditEnumerator
    {
        LinkRiverViewModel _context;
        int _currentLinkPos;
        bool _forward;
        public NeverEndingRedditEnumerator(LinkRiverViewModel context, int currentLinkPos, bool forward)
        {
            _context = context;
            _currentLinkPos = currentLinkPos;
            _forward = forward;
        }
        public async Task<ViewModelBase> Next()
        {
            if (_forward)
            {
                _currentLinkPos++;
                if (_context.Links.Count <= _currentLinkPos || (_currentLinkPos == 0 && _context.Links.Count == 0))
                {
                    await _context.LoadMore();
                }
            }
            else
                _currentLinkPos--;

            if (_context.Links.Count > _currentLinkPos && _currentLinkPos > 0)
                return _context.Links[_currentLinkPos];
            else
                return null;
        }
    }
}
