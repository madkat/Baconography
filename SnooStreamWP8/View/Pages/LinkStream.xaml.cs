using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SnooStream.ViewModel;
using System.Collections.ObjectModel;
using Telerik.Windows.Controls;
using SnooStreamWP8.Common;
using System.Threading.Tasks;

namespace SnooStreamWP8.View.Pages
{
    public partial class LinkStream : SnooApplicationPage
    {
        public LinkStream()
        {
            InitializeComponent();
        }

        bool _noMoreLoad = false;
        ObservableCollection<LinkViewModel> _links;
        public ObservableCollection<LinkViewModel> Links
        {
            get
            {
                if (_links == null)
                {
                    ((IPageProvider)radSlideView).CurrentIndexChanged += radSlideViewIndexChanged;
                    _links = new ObservableCollection<LinkViewModel>();
                    LoadInitialLinks(_links);
                }
                return _links;
            }
        }

        public async void LoadInitialLinks(ObservableCollection<LinkViewModel> links)
        {
            var linkStream = DataContext as LinkStreamViewModel;
            if (linkStream != null && await linkStream.MoveNext())
            {
                links.Add(linkStream.Current);

                for (int i = 0; i < 5 && await linkStream.MoveNext(); i++)
                {
                    links.Add(linkStream.Current);
                }

                await Task.Yield();

                var priorLinks = await linkStream.LoadPrior.Value;
                foreach (var link in priorLinks)
                    links.Insert(0, link);

                ((IPageProvider)radSlideView).CurrentIndex = priorLinks.Count;
            }
        }

        private async void radSlideViewIndexChanged(object sender, EventArgs e)
        {
            if(Links.Count > 0 && !_noMoreLoad)
            {
                //preload distance
                if (((IPageProvider)radSlideView).CurrentIndex > (Links.Count - 5))
                {
                    _noMoreLoad = !(await ((LinkStreamViewModel)DataContext).MoveNext());
                    if (!_noMoreLoad)
                    {
                        Links.Add(((LinkStreamViewModel)DataContext).Current);
                    }
                }
            }
        }
    }
}