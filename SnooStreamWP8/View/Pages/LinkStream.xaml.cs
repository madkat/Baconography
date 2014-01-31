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

        bool _loading = false;

        public async void LoadInitialLinks(ObservableCollection<LinkViewModel> links)
        {
            try
            {
                _loading = true;
                var linkStream = DataContext as LinkStreamViewModel;
                if (linkStream != null && await linkStream.MoveNext())
                {
                    await (await linkStream.Current.AsyncContent).BeginLoad(true);
                    links.Add(linkStream.Current);

                    for (int i = 0; i < 5 && await linkStream.MoveNext(); i++)
                    {
                        await (await linkStream.Current.AsyncContent).BeginLoad(true);
                        links.Add(linkStream.Current);
                    }

                    await Task.Yield();

                    var priorLinks = await linkStream.LoadPrior.Value;
                    foreach (var link in priorLinks)
                        links.Insert(0, link);
                }
            }
            finally
            {
                _loading = false;
            }
        }

        private async void radSlideViewIndexChanged(object sender, EventArgs e)
        {
            if(Links.Count > 0 && !_noMoreLoad && !_loading)
            {
                try
                {
                    _loading = true;
                    //preload distance
                    if (((IPageProvider)radSlideView).CurrentIndex > (Links.Count - 5))
                    {
                        _noMoreLoad = !(await ((LinkStreamViewModel)DataContext).MoveNext());
                        if (!_noMoreLoad)
                        {
                            await (await ((LinkStreamViewModel)DataContext).Current.AsyncContent).BeginLoad(true);
                            Links.Add(((LinkStreamViewModel)DataContext).Current);
                        }
                    }
                }
                finally
                {
                    _loading = false;
                }
            }
        }
    }
}