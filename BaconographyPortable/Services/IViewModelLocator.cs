using BaconographyPortable.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaconographyPortable.Services
{
    public interface IViewModelLocator
    {
        RedditViewModel Reddit { get; }
        bool IsLoaded { get; }
    }
}
