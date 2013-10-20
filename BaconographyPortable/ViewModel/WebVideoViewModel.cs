using BaconographyPortable.Common;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaconographyPortable.ViewModel
{
    public class WebVideoViewModel : ViewModelBase
    {
        public WebVideoViewModel(IEnumerable<Dictionary<string, string>> avalableStreams, string linkId)
        {
            _availableStreams = avalableStreams;
            _url = MakeUsableUrl(avalableStreams.First());
            _selectedStream = AvailableStreams.First();
            LinkId = linkId;
        }

        string MakeUsableUrl(Dictionary<string, string> raw)
        {
            var rawUrl = Uri.UnescapeDataString(raw["url"]);
            var rawSig = raw["sig"];
            return rawUrl + "&signature=" + rawSig;
        }

        private string _url;
        public string Url
        {
            get
            {
                return _url;
            }
        }

        public string LinkId { get; private set; }
        IEnumerable<Dictionary<string, string>> _availableStreams;
        public IEnumerable<string> AvailableStreams
        {
            get
            {
                return _availableStreams.Select(stream => CleanName(stream["type"]) + " : " + stream["quality"]);
            }
        }

        string _selectedStream;
        public string SelectedStream
        {
            get
            {
                return _selectedStream;
            }
            set
            {
                if (_selectedStream != value)
                {
                    _selectedStream = value;

                    var selectedStream = _availableStreams.FirstOrDefault(stream => (CleanName(stream["type"]) + " : " + stream["quality"]) == _selectedStream);
                    if (selectedStream != null)
                    {
                        _url = MakeUsableUrl(selectedStream);
                        RaisePropertyChanged("Url");
                    }
                }

            }
        }
        private static string CleanName(string dirtyName)
        {
            switch (dirtyName)
            {
                case "video/webm;+codecs":
                    return "webm";
                case "video/mp4;+codecs":
                    return "mp4";
                case "video/flv":
                    return "flash";
                case "video/3gpp":
                    return "mobile";
                default:
                    return "unknown";
            }
        }

        LinkViewModel _parentLink;
        public LinkViewModel ParentLink
        {
            get
            {
                if (_parentLink == null)
                {
                    if (string.IsNullOrWhiteSpace(LinkId))
                        return null;

                    _parentLink = StreamViewUtility.FindSelfFromLink(LinkId) as LinkViewModel;
                }

                return _parentLink;
            }
        }

        public string Title
        {
            get
            {
                if (ParentLink != null)
                {
                    return ParentLink.Title;
                }
                else
                    return null;
            }
        }

        public bool HasContext
        {
            get
            {
                return ParentLink != null;
            }
        }

        public int CommentCount
        {
            get
            {
                if (HasContext)
                    return ParentLink.LinkThing.Data.CommentCount;
                return 0;
            }
        }

        public VotableViewModel Votable
        {
            get
            {
                if (ParentLink != null)
                    return ParentLink.Votable;
                return null;
            }
        }

        public RelayCommand<WebVideoViewModel> NavigateToComments { get { return _navigateToComments; } }
        static RelayCommand<WebVideoViewModel> _navigateToComments = new RelayCommand<WebVideoViewModel>(NavigateToCommentsImpl);
        private static void NavigateToCommentsImpl(WebVideoViewModel vm)
        {
            vm.ParentLink.NavigateToComments.Execute(vm.ParentLink);
        }
    }
}
