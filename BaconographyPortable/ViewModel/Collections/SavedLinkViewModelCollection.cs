using BaconographyPortable.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaconographyPortable.ViewModel.Collections
{
    public class SavedLinkViewModelCollection : ThingViewModelCollection
    {
        public SavedLinkViewModelCollection(IBaconProvider baconProvider)
            : base(baconProvider,
                new BaconographyPortable.Model.Reddit.ListingHelpers.SavedLinks(baconProvider),
                new BaconographyPortable.Model.KitaroDB.ListingHelpers.SavedLinks(baconProvider)) { } 
    }
}
