using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SnooSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnooStream.ViewModel
{
    //this class needs to take care of storing off prior comment sets so we can point the user directly to
    //the comments that have been either edited, added, or deleted
    public class CommentsViewModel : ViewModelBase
    {
        private class CommentShell
        {
            public CommentViewModel Comment { get; set; }
            public string Id { get; set; }
            public string Parent { get; set; }
            public string NextSibling { get; set; }
            public string PriorSibling { get; set; }
            public string FirstChild { get; set; }
        }

        private Dictionary<string, CommentShell> _comments = new Dictionary<string, CommentShell>();
        private Dictionary<string, List<string>> _knownUnloaded = new Dictionary<string, List<string>>();
        private string _firstChild;
        private LoadFullCommentsViewModel _loadFullSentinel;
        private ViewModelBase _context;


        public bool IsContext {get; private set;}
        public string ContextTargetID { get; private set; }
        private string _sort;
        public string Sort
        {
            get
            {
                return _sort;
            }
            set
            {
                _sort = value;
                RaisePropertyChanged("Sort");
            }
        }
        public string BaseUrl { get; private set; }
        public LinkViewModel Link { get; private set; }

        public ObservableCollection<ViewModelBase> FlatComments { get; private set; }

        private CommentShell LastChild(CommentShell shell)
        {
            if (shell.FirstChild != null)
            {
                CommentShell currentShell;
                if (_comments.TryGetValue(shell.FirstChild, out currentShell))
                {
                    return LastSibling(currentShell);
                }
            }
            return null;
        }
        private CommentShell LastSibling(CommentShell shell)
        {
            var currentShell = shell;
            while (currentShell.NextSibling != null)
            {
                //this is the end as far as we're concerned
                CommentShell tmpShell;
                if (!_comments.TryGetValue(currentShell.NextSibling, out tmpShell))
                {
                    return tmpShell;
                }
                else
                    currentShell = tmpShell;
            }
            return null;
        }

        private void MergeComments(CommentShell parent, IEnumerable<Thing> things, int depth = 0)
        {
            CommentShell priorSibling = parent != null ? LastChild(parent) : null;
            foreach (var child in things)
            {
                if (child.Data is More)
                {
                    var firstId = ((More)child.Data).Children.First();
                    if (priorSibling == null) //need to attach to parent
                    {
                        parent.FirstChild = firstId;
                    }
                    else
                    {
                        priorSibling.NextSibling = firstId;
                    }
                    if (!_knownUnloaded.ContainsKey(firstId))
                        _knownUnloaded.Add(firstId, ((More)child.Data).Children);

                    //cant continue after a 'more' element comes through
                    break;
                }
                else if (child.Data is Comment)
                {
                    var commentId = ((Comment)child.Data).Id;

                    if (priorSibling == null && parent == null)
                    {
                        if (_firstChild != null)
                        {
                            priorSibling = LastSibling(_comments[_firstChild]);
                        }
                        else
                        {
                            _firstChild = commentId;
                        }
                    }


                    if (priorSibling == null && parent != null) //need to attach to parent
                    {
                        parent.FirstChild = commentId;
                    }
                    else if (priorSibling != null)
                    {
                        priorSibling.NextSibling = commentId;
                    }

                    priorSibling = MakeCommentShell(child.Data as Comment, depth, priorSibling);
                    if (!_comments.ContainsKey(commentId))
                        _comments.Add(commentId, priorSibling);

                    var replies = ((Comment)child.Data).Replies;
                    if (replies != null)
                        MergeComments(priorSibling, replies.Data.Children, depth + 1);
                }
            }
        }

        private CommentShell MakeCommentShell(Comment comment, int depth, CommentShell priorSibling)
        {
            var result = new CommentShell
            {
                Comment = new CommentViewModel(comment, comment.LinkId, depth),
                Parent = comment.ParentId.StartsWith("t1_") ? comment.ParentId : null,
                PriorSibling = priorSibling != null ? priorSibling.Id : null
            };
            return result;
        }

        //this needs to be called on anything that comes back from a load more because reddit mostly just gives us a non structured listing of comments
        private void FixupParentage(Listing listing)
        {
            Dictionary<string, Comment> nameMap = new Dictionary<string, Comment>();
            foreach (var item in listing.Data.Children)
            {
                if (item.Data is Comment)
                    nameMap.Add(((Comment)item.Data).Name, ((Comment)item.Data));
            }

            foreach (var item in new List<Thing>(listing.Data.Children))
            {
                if (item.Data is Comment)
                {
                    if (nameMap.ContainsKey(((Comment)item.Data).ParentId))
                    {
                        var targetParent = nameMap[((Comment)item.Data).ParentId];
                        if (targetParent.Replies == null)
                            targetParent.Replies = new Listing { Data = new ListingData { Children = new List<Thing>() } };

                        targetParent.Replies.Data.Children.Add(item);
                        listing.Data.Children.Remove(item);
                    }
                }
            }
        }

        private void InsertIntoFlatList(string firstChild, List<ViewModelBase> list)
        {
            CommentShell childShell;
            var targetChild = firstChild;
            while (targetChild != null)
            {
                if (_comments.TryGetValue(targetChild, out childShell))
                {
                    list.Add(childShell.Comment);
                    if (childShell.NextSibling != null)
                    {
                        InsertIntoFlatList(childShell.NextSibling, list);
                    }
                }
                else if(_knownUnloaded.ContainsKey(targetChild)) //if its not in the list check the known unloaded list (more)
                {
                    list.Add(new MoreViewModel(this, _knownUnloaded[targetChild]));
                }
                else //we must be looking at something missing because on context so we need to put out a 'loadfull' viewmodel
                {
                    list.Add(new LoadFullCommentsViewModel(this));
                }
            }
        }

        private void MergeDisplayReplacement(IEnumerable<ViewModelBase> replacements)
        {
            //need to find the above and do insertion of everything above
            //then skip over the middle and insert the below

            var replacementsList = replacements.ToList();
            var firstExistingId = GetId(FlatComments.First());
            var lastExistingId = GetId(FlatComments.Last());


            var aboveComments = replacements.TakeWhile((vm) => GetId(vm) != firstExistingId).ToList();
            var mergableComments = replacements.SkipWhile((vm) => GetId(vm) != firstExistingId).TakeWhile((vm) => GetId(vm) != lastExistingId).ToList();
            var belowComments = replacements.SkipWhile((vm) => GetId(vm) != lastExistingId).Skip(1).ToList();

            //get rid of the load full sentinels since we're filling in the real versions
            while (FlatComments.Remove(_loadFullSentinel)) { }

            if (mergableComments.Count != FlatComments.Count) //otherwise nothing to do, just add in the above and below
            {
                for (int flatI = 0, mergableI = 0; flatI < FlatComments.Count; flatI++, mergableI++)
                {
                    if (FlatComments[flatI] != mergableComments[mergableI])
                    {
                        //need to skip ahead one on flatI since we just added another one and we dont want that to be the new comparison point
                        FlatComments.Insert(flatI++, mergableComments[mergableI]);
                    }
                }
            }

            foreach (var comment in aboveComments)
            {
                FlatComments.Insert(0, comment);
            }

            foreach (var comment in belowComments)
            {
                FlatComments.Add(comment);
            }
        }

        private string GetId(ViewModelBase viewModel)
        {
            if (viewModel is CommentViewModel)
                return ((CommentViewModel)viewModel).Id;
            else if (viewModel is MoreViewModel)
                return ((MoreViewModel)viewModel).Id;
            else
                return "";
        }

        private void MergeDisplayChildren(IEnumerable<ViewModelBase> newChildren, string replaceId)
        {
            //the children will be in a contiguous block so we just need to find the existing viewmodel that 
            //matches the afterId then we can add each one in series

            for (int i = 0; i < FlatComments.Count; i++)
            {
                var commentId = GetId(FlatComments[i]);
                if (commentId == replaceId)
                {
                    foreach(var child in newChildren)
                    {
                        if ((FlatComments.Count - 1) <= i)
                            FlatComments.Add(child);
                        else
                            FlatComments.Insert(i, child);

                        i++;
                    }
                    break;
                }
            }
        }

        public async Task LoadMore(More target)
        {
            List<ViewModelBase> flatChilden = new List<ViewModelBase>();
            string moreId = null;
            await Task.Run<Task>(async () =>
                {
                    var listing = await SnooStreamViewModel.RedditService.GetMoreOnListing(target.Children, Link.Link.Id, Link.Link.Subreddit);
                    lock(this)
                    {
                        FixupParentage(listing);
                        var firstChild = listing.Data.Children.FirstOrDefault(thing => thing.Data is Comment);
                        if(firstChild == null)
                            return;

                        var parentId = ((Comment)firstChild.Data).ParentId;
                        CommentShell parentShell;
                        if (!_comments.TryGetValue(parentId, out parentShell))
                        {
                            parentShell = null;
                        }

                        MergeComments(parentShell, listing.Data.Children, parentShell == null ? 0 : parentShell.Comment.Depth);
                        moreId = ((Comment)firstChild.Data).Id;
                        InsertIntoFlatList(moreId, flatChilden);
                    }
                });

            if (moreId != null)
                MergeDisplayChildren(flatChilden, moreId);
        }

        public async Task LoadFull()
        {
            List<ViewModelBase> flatChilden = new List<ViewModelBase>();

            await Task.Run<Task>(async () =>
            {
                var listing = await SnooStreamViewModel.RedditService.GetCommentsOnPost(Link.Link.Subreddit, BaseUrl, null);
                lock (this)
                {
                    var firstChild = listing.Data.Children.FirstOrDefault(thing => thing.Data is Comment);
                    if (firstChild == null)
                        return;

                    MergeComments(null, listing.Data.Children, 0);
                    InsertIntoFlatList(((Comment)firstChild.Data).Id, flatChilden);
                }
            });

            if (flatChilden.Count > 0)
            {
                if (FlatComments.Count == 0)
                {
                    foreach (var comment in flatChilden)
                    {
                        FlatComments.Add(comment);
                    }
                }
                else
                {
                    MergeDisplayReplacement(flatChilden);
                }
            }
        }

        public RelayCommand GotoLink
        {
            get 
            {
                return new RelayCommand(() => SnooStreamViewModel.CommandDispatcher.GotoLink(_context, Link.Link.Url));
            }
        }

        public RelayCommand GotoReply
        {
            get
            {
                return new RelayCommand(() => SnooStreamViewModel.CommandDispatcher.GotoReplyToPost(_context, this));
            }
        }

        public RelayCommand GotoEditPost
        {
            get
            {
                return new RelayCommand(() => SnooStreamViewModel.CommandDispatcher.GotoEditPost(_context, Link));
            }
        }

    }
}
