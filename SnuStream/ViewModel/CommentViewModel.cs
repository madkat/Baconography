using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SnuSharp;
using SnuStream.Model;
using SnuStream.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnuStream.ViewModel
{
    public class CommentViewModel : ViewModelBase
    {
        ViewModelBase _parent;
        Comment _comment;
        VotableViewModel _votable;
        public VotableViewModel Votable
        {
            get
            {
                if (_votable == null)
                    _votable = new VotableViewModel(_comment, () => RaisePropertyChanged("Votable"));
                return _votable;
            }
        }

        public int Depth { get; set; }

        AuthorFlairKind AuthorFlair { get; set; }

        public string AuthorFlairText { get; set; }

        public bool HasAuthorFlair
        {
            get
            {
                return (!String.IsNullOrWhiteSpace(AuthorFlairText));
            }
        }

        private string _body;
        public string Body
        {
            get
            {
                return _body;
            }
            set
            {
                _body = value;
                BodyMD = SnooStreamViewModel.MarkdownProcessor.Process(value);
                RaisePropertyChanged("Body");
                RaisePropertyChanged("BodyMD");
            }
        }

        public object BodyMD { get; private set; }

        public string PosterName
        {
            get
            {
                return _comment.Author;
            }
        }

        private bool _isMinimized;
        public bool IsMinimized
        {
            get
            {
                return _isMinimized;
            }
            set
            {
                _isMinimized = value;
                RaisePropertyChanged("IsMinimized");
            }
        }

        public bool IsEditable
        {
            get
            {
                return string.Compare(SnooStreamViewModel.RedditService.CurrentUserName, PosterName, StringComparison.CurrentCultureIgnoreCase) == 0;
            }
        }

        public AuthorFlairKind PosterFlair
        {
            get
            {
                return SnooStreamViewModel.RedditService.GetUsernameModifiers(PosterName, _comment.LinkId, _comment.SubredditId);
            }
        }

        public RelayCommand MinimizeCommand { get { return new RelayCommand(() => IsMinimized = !IsMinimized); } }
        public RelayCommand GotoContext { get { return new RelayCommand(GotoContextImpl); } }
        public RelayCommand GotoFullLink { get { return new RelayCommand(GotoFullLinkImpl); } }
        public RelayCommand Report { get { return new RelayCommand(ReportImpl); } }
        public RelayCommand Save { get { return new RelayCommand(SaveImpl); } }
        public RelayCommand GotoReply { get { return new RelayCommand(GotoReplyImpl); } }
        public RelayCommand GotoEdit { get { return new RelayCommand(GotoEditImpl); } }
        public RelayCommand GotoUserDetails { get { return new RelayCommand(GotoUserDetailsImpl); } }

        private void GotoContextImpl()
        {
            SnooStreamViewModel.CommandDispatcher.GotoCommentContext(_parent, this);
        }

        private void GotoFullLinkImpl()
        {
            SnooStreamViewModel.CommandDispatcher.GotoFullComments(_parent, this);
        }

        private void GotoUserDetailsImpl()
        {
            SnooStreamViewModel.CommandDispatcher.GotoUserDetails(_comment.Author);
        }

        private void ReportImpl()
        {
            SnooStreamViewModel.RedditService.AddReportOnThing(_comment.Name);
        }

        private void SaveImpl()
        {
            SnooStreamViewModel.RedditService.AddSavedThing(_comment.Name);
        }

        private void GotoReplyImpl()
        {
            SnooStreamViewModel.CommandDispatcher.GotoReplyToComment(_parent, this);
        }

        private void GotoEditImpl()
        {
            SnooStreamViewModel.CommandDispatcher.GotoEditComment(_parent, this);
        }
    }
}
