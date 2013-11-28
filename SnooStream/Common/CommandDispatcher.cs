using GalaSoft.MvvmLight;
using SnooSharp;
using SnooStream.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnooStream.Common
{
    public class CommandDispatcher
    {
        public void GotoCommentContext(ViewModelBase currentContext, CommentViewModel source)
        {
        }

        public void GotoFullComments(ViewModelBase currentContext, CommentViewModel source)
        {

        }

        public void GotoUserDetails(string username)
        {
            throw new NotImplementedException();
        }

        public void GotoReplyToComment(ViewModelBase currentContext, CommentViewModel source)
        {
            throw new NotImplementedException();
        }

        public void GotoEditComment(ViewModelBase currentContext, CommentViewModel source)
        {
            throw new NotImplementedException();
        }

        public void UpdateComment(ViewModelBase _context, Thing theComment)
        {
            throw new NotImplementedException();
        }

        public void InsertComment(ViewModelBase _context, Thing theComment)
        {
            throw new NotImplementedException();
        }

        public void GotoLink(ViewModelBase context, string url)
        {
            throw new NotImplementedException();
        }
    }
}
