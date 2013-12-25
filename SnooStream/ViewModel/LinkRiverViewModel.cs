using GalaSoft.MvvmLight;
using SnooSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SnooStream.ViewModel
{
    public class LinkRiverViewModel : ViewModelBase, ICollection<LinkViewModel>, INotifyCollectionChanged
    {
        //need to come up with an init blob setup for this, meaining a per river blob
        public Subreddit Thing { get; private set; }
        public string Sort { get; private set; }
        public bool Loading { get { return _loadingTask != null; } }
        private string LastLinkId { get; set; }
        public bool IsLocal { get; private set; }
        public LinkRiverViewModel(bool isLocal, Subreddit thing, string sort, IEnumerable<Link> initialLinks)
        {
            IsLocal = isLocal;
            Thing = thing;
            Sort = sort ?? "hot";
            Links = new ObservableCollection<LinkViewModel>();
            if(initialLinks != null)
            {
                ProcessLinkThings(initialLinks);
            }
        }

        public async Task<IEnumerable<ContentViewModel>> PreloadContent(Func<LinkViewModel, bool> predicate, int count, CancellationToken cancelToken)
        {
            List<ContentViewModel> results = new List<ContentViewModel>();
            var linkStream = new LinkStreamViewModel(this, null);
            while (results.Count < count && await linkStream.MoveNext() && !cancelToken.IsCancellationRequested)
            {
                if (predicate(linkStream.Current))
                {
                    await linkStream.Current.Content.BeginLoad();
                    results.Add(linkStream.Current.Content);
                }
            }
            return results;
        }

        private void ProcessLinkThings(IEnumerable<Link> links)
        {
            foreach (var link in links)
            {
                Links.Add(new LinkViewModel(this, link));
            }
        }

        public ObservableCollection<LinkViewModel> Links { get; set; }
        private Task _loadingTask;
        public Task LoadMore()
        {
            if (_loadingTask == null)
            {
                lock (this)
                {
                    if (_loadingTask == null)
                    {
                        _loadingTask = LoadMoreImpl();
                    }
                }
            }
            return _loadingTask;
        }

        public async Task LoadMoreImpl()
        {
            await SnooStreamViewModel.NotificationService.Report("loading posts", async () =>
                {
                    var postListing = LastLinkId != null ? 
                        await SnooStreamViewModel.RedditService.GetAdditionalFromListing(Thing.Url, LastLinkId, null) :
                        await SnooStreamViewModel.RedditService.GetPostsBySubreddit(Thing.Url, Sort);

                    if (postListing != null)
                    {
                        foreach (var thing in postListing.Data.Children)
                        {
                            if (thing.Data is Link)
                                Links.Add(new LinkViewModel(this, thing.Data as Link));
                        }

                        LastLinkId = postListing.Data.After;
                    }
                });
            
            //clear the loading task when we're done
            _loadingTask = null;
        }

        public void Add(LinkViewModel item)
        {
            Links.Add(item);
        }

        public void Clear()
        {
            Links.Clear();
        }

        public bool Contains(LinkViewModel item)
        {
            return Links.Contains(item);
        }

        public void CopyTo(LinkViewModel[] array, int arrayIndex)
        {
            Links.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return Links.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(LinkViewModel item)
        {
            return Links.Remove(item);
        }

        public IEnumerator<LinkViewModel> GetEnumerator()
        {
            return Links.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Links.GetEnumerator();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                Links.CollectionChanged += value;
            }
            remove
            {
                Links.CollectionChanged -= value;
            }
        }
    }
}
