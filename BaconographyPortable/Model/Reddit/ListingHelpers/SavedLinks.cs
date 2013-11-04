using BaconographyPortable.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaconographyPortable.Model.Reddit.ListingHelpers
{
    public class SavedLinks: IListingProvider
    {
        IRedditService _redditService;
        IUserService _userService;

        public SavedLinks(IBaconProvider baconProvider)
        {
            _redditService = baconProvider.GetService<IRedditService>();
            _userService = baconProvider.GetService<IUserService>();
        }

        public Task<Listing> GetInitialListing(Dictionary<object, object> state)
        {
            return _redditService.GetSaved(null);
        }

        public async Task<Listing> GetAdditionalListing(string after, Dictionary<object, object> state)
        {
            var user = await _userService.GetUser();
            return await _redditService.GetAdditionalFromListing("http://reddit.com/user/" + user.Username + "/saved", after, null);
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
