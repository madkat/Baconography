using GalaSoft.MvvmLight;
using SnuSharp;
using SnuStream.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnuStream.Common
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

        internal void GotoReplyToComment(ViewModelBase currentContext, CommentViewModel source)
        {
            throw new NotImplementedException();
        }

        internal void GotoEditComment(ViewModelBase currentContext, CommentViewModel source)
        {
            throw new NotImplementedException();
        }

        internal void UpdateComment(ViewModelBase _context, Thing theComment)
        {
            throw new NotImplementedException();
        }

        internal void InsertComment(ViewModelBase _context, Thing theComment)
        {
            throw new NotImplementedException();
        }
    }
}
