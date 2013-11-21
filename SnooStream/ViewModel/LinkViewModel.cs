using GalaSoft.MvvmLight;
using SnooSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnooStream.ViewModel
{
    public class LinkViewModel : ViewModelBase
    {
        public LinkViewModel(Link link)
        {
            Link = link;


        }

        public ContentViewModel Content { get; private set; }
        public CommentsViewModel Comments { get; private set; }
        public Link Link { get; private set; }
        public int CommentsLastViewed { get; private set; }
        //need to add load helpers here for kicking off preview loads when we get near things

        internal void MergeLink(Link link)
        {
            throw new NotImplementedException();
        }
    }
}
