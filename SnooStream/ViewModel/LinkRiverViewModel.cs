using GalaSoft.MvvmLight;
using SnooSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnooStream.ViewModel
{
    public class LinkRiverViewModel : ViewModelBase, ICollection<LinkViewModel>, INotifyCollectionChanged
    {
        //need to come up with an init blob setup for this, meaining a per river blob
        public string Subreddit { get; private set; }
        public string Sort { get; private set; }
        public string Title { get; private set; }
        public bool Loading { get { return _loadingTask != null; } }
        private string LastLinkId { get; set; }
        public LinkRiverViewModel(string subreddit, string sort, IEnumerable<Link> initialLinks)
        {
            Subreddit = subreddit;
            Sort = sort;
        }

        public ObservableCollection<LinkViewModel> Content { get; set; }
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
            //clear the loading task when we're done
            _loadingTask = null;
        }

        public void Add(LinkViewModel item)
        {
            Content.Add(item);
        }

        public void Clear()
        {
            Content.Clear();
        }

        public bool Contains(LinkViewModel item)
        {
            return Content.Contains(item);
        }

        public void CopyTo(LinkViewModel[] array, int arrayIndex)
        {
            Content.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return Content.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(LinkViewModel item)
        {
            return Content.Remove(item);
        }

        public IEnumerator<LinkViewModel> GetEnumerator()
        {
            return Content.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Content.GetEnumerator();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                Content.CollectionChanged += value;
            }
            remove
            {
                Content.CollectionChanged -= value;
            }
        }
    }
}
