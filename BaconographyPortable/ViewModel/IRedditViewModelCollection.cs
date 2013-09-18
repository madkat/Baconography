using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaconographyPortable.ViewModel
{
    public interface IRedditViewModelCollection
    {
        IEnumerable<RedditViewModel> RedditViewModels { get; }
    }
}
