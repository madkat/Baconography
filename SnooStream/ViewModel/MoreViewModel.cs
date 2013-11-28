using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnooStream.ViewModel
{
    public class MoreViewModel : ViewModelBase
    {
        public MoreViewModel(CommentsViewModel context, List<string> ids)
        {
        }

        public string Id { get; set; }
    }
}
