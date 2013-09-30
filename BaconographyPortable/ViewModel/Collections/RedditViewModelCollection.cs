using BaconographyPortable.Messages;
using BaconographyPortable.Model.Reddit;
using BaconographyPortable.Services;
using BaconographyPortable.ViewModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaconographyPortable.ViewModel.Collections
{
    public class RedditViewModelCollection : ObservableCollection<ViewModelBase>
    {
        public RedditViewModelCollection(IEnumerable<ViewModelBase> initial) : base(initial) { }
        public RedditViewModelCollection() : base() { }
    }
}
