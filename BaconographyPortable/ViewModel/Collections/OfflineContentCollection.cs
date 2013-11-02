using BaconographyPortable.Common;
using BaconographyPortable.Services;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaconographyPortable.ViewModel.Collections
{
    public class OfflineContentCollection : ThingViewModelCollection
    {
        public OfflineContentCollection(IBaconProvider baconProvider) :
            base(baconProvider,
                null,
                new BaconographyPortable.Model.KitaroDB.ListingHelpers.OfflineContent(baconProvider)) { }
    }
}
