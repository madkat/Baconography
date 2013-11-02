using BaconographyPortable.Model.Reddit;
using BaconographyPortable.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaconographyPortable.Model.KitaroDB.ListingHelpers
{
    class OfflineContent : IListingProvider
    {
        IOfflineService _offlineService;
        ISettingsService _settingsService;

        public OfflineContent(IBaconProvider baconProvider)
        {
            _offlineService = baconProvider.GetService<IOfflineService>();
            _settingsService = baconProvider.GetService<ISettingsService>();
        }

        public async Task<Listing> GetInitialListing(Dictionary<object, object> state)
        {
            //get initial chunk of comments
            var commentBlock = await _offlineService.GetCommentsByInsertion(null, 20);
            if (commentBlock != null)
            {
                return new Listing 
                { 
                    Data = new ListingData 
                    { 
                        After = commentBlock.Item2.ToString(),
                        Children = commentBlock.Item1.Select(listing => 
                            {
                                var resultLink = listing.Data.Children.FirstOrDefault(thing => thing.Data is Link);
                                if (resultLink != null)
                                {
                                    ((Link)resultLink.Data).Offlined = true;
                                }
                                return resultLink;
                            }).Where(link => link != null)
                            .ToList()
                    }
                };
            }
            else
            {
                return new Listing { Data = new ListingData { Children = new List<Thing>() } };
            }
        }

        public async Task<Listing> GetAdditionalListing(string after, Dictionary<object, object> state)
        {
            long afterLong;
            if (long.TryParse(after, out afterLong))
            {
                var commentBlock = await _offlineService.GetCommentsByInsertion(afterLong, 20);
                if (commentBlock != null)
                {
                    return new Listing
                    {
                        Data = new ListingData
                        {
                            After = commentBlock.Item2.ToString(),
                            Children = commentBlock.Item1.Select(listing =>
                            {
                                var resultLink = listing.Data.Children.FirstOrDefault(thing => thing.Data is Link);
                                if (resultLink != null)
                                {
                                    ((Link)resultLink.Data).Offlined = true;
                                }
                                return resultLink;
                            }).Where(link => link != null)
                            .ToList()
                        }
                    };
                }
            }

            return new Listing { Data = new ListingData { Children = new List<Thing>() } };
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
