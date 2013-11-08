using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnuSharp
{
    public interface IListingFilter
    {
        Listing FilterLinks(Listing listing, string subreddit);
        Listing FilterComments(Listing listing, string permaLink);
        Listing FilterMore(Listing listing, string name);
        Listing FilterSubreddits(Listing listing);
        Listing Filter(Listing newListing);
        Listing FilterAdditional(Listing newListing, string baseUrl);
    }
}
