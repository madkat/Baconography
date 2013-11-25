using GalaSoft.MvvmLight;
using SnooSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnooStream.ViewModel
{
    public class ActivityViewModel : ViewModelBase
    {
        public class ActivityAgeComparitor : IComparer<ActivityViewModel>
        {
            public int Compare(ActivityViewModel x, ActivityViewModel y)
            {
                return x.Created.CompareTo(y.Created);
            }
        }

        public DateTime Created { get; protected set; }
        public string Test
        {
            get
            {
                return "Test";
            }
        }

        public static string GetActivityGroupName(Thing thing)
        {
            if (thing == null)
                throw new ArgumentNullException();

            if (thing.Data is Link)
                return ((Link)thing.Data).Id;
            else if (thing.Data is Comment)
                return ((Comment)thing.Data).LinkId;
            else if (thing.Data is Message)
            {
                var messageThing = thing.Data as Message;
                if (messageThing.WasComment)
                {
                    // "/r/{subreddit}/comments/{linkname}/{linktitleish}/{thingname}?context=3"

                    var splitContext = messageThing.Context.Split('/');
                    return "t3_" + splitContext[4];
                }
                else
                {
                    return messageThing.Subject;
                }
            }
            else if (thing.Data is ModAction)
            {
                return ((ModAction)thing.Data).TargetFullname;
            }
            else
                throw new ArgumentOutOfRangeException();
        }

        public static ActivityViewModel CreateActivity(Thing thing)
        {
            if (thing.Data is Link)
                return new PostedLinkActivityViewModel(thing.Data as Link);
            else if (thing.Data is Comment)
                return new PostedCommentActivityViewModel(thing.Data as Comment);
            else if (thing.Data is Message)
            {
                var messageThing = thing.Data as Message;
                if (messageThing.WasComment)
                {
                    return new RecivedCommentReplyActivityViewModel(messageThing);
                }
                //check if its actually mod mail
                else if (messageThing.Author == "reddit")
                {
                    return new ModeratorMessageActivityViewModel(messageThing);
                }
                else
                {
                    return new MessageActivityViewModel(messageThing);
                }
            }
            else if (thing.Data is ModAction)
            {
                return new ModeratorActivityViewModel(thing.Data as ModAction);
            }
            else
                throw new ArgumentOutOfRangeException();
        }
    }

    public class PostedLinkActivityViewModel : ActivityViewModel
    {
        public Link Link { get; private set; }

        public PostedLinkActivityViewModel(Link link)
        {
            Link = link;
            Created = link.CreatedUTC.ToLocalTime();
        }
    }

    public class PostedCommentActivityViewModel : ActivityViewModel
    {
        private Comment Comment { get; set; }
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
        public string Subject { get; private set; }
        public string ParentId { get; private set; }
        public string LinkId { get; private set; }

        public PostedCommentActivityViewModel(Comment comment)
        {
            Comment = comment;
            Created = comment.CreatedUTC.ToLocalTime();
        }
    }

    public class RecivedCommentReplyActivityViewModel : ActivityViewModel
    {
        private Message Message { get; set; }
        private string _body;
        private SnooSharp.Message messageThing;

        public RecivedCommentReplyActivityViewModel(Message messageThing)
        {
            Message = messageThing;
            Created = messageThing.CreatedUTC.ToLocalTime();
        }
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
        public string Subject { get; private set; }
        public string ParentId { get; private set; }
        public string LinkId { get; private set; }
    }

    public class MentionActivityViewModel : ActivityViewModel
    {
        private Message Message { get; set; }
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
        public string Subject { get; private set; }
        public string ParentId { get; private set; }
        public string LinkId { get; private set; }
    }

    public class MessageActivityViewModel : ActivityViewModel
    {
        private string _body;
        private Message MessageThing;

        public MessageActivityViewModel(Message messageThing)
        {
            MessageThing = messageThing;
            Created = messageThing.CreatedUTC.ToLocalTime();
        }
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
        public string Subject { get; private set; }
        public string ParentId { get; private set; }
        public string LinkId { get; private set; }
    }

    public class ModeratorActivityViewModel :ActivityViewModel
    {
        private ModAction ModAction;

        public ModeratorActivityViewModel(ModAction modAction)
        {
            ModAction = modAction;
            Created = modAction.CreatedUTC.ToLocalTime();
        }
    }

    public class ModeratorMessageActivityViewModel :ActivityViewModel
    {
        private Message messageThing;

        public ModeratorMessageActivityViewModel(Message messageThing)
        {
            Created = messageThing.CreatedUTC.ToLocalTime();
            messageThing = messageThing;
        }
    }
}
