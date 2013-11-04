using BaconographyPortable.Model.Reddit;
using BaconographyPortable.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaconographyPortable.Model.KitaroDB.ListingHelpers
{
    public class SavedLinks: IListingProvider
    {
        IOfflineService _offlineService;

        public SavedLinks(IBaconProvider baconProvider)
        {
            _offlineService = baconProvider.GetService<IOfflineService>();
        }

        public Task<Listing> GetInitialListing(Dictionary<object, object> state)
        {
            return Task.FromResult(new Listing { Kind = "Listing", Data = new ListingData { Children = new List<Thing>() } });
        }

        public Task<Listing> GetAdditionalListing(string after, Dictionary<object, object> state)
        {
            throw new NotImplementedException();
        }

        public Task<Listing> GetMore(IEnumerable<string> ids, Dictionary<object, object> state)
        {
            throw new NotImplementedException();
        }
        public Task<Listing> Refresh(Dictionary<object, object> state)
        {
            return GetInitialListing(state);
        }
    }
}
