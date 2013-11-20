using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnooStream.ViewModel
{
    public class VideoViewModel : ContentViewModel
    {
        //this needs to load the preview image from youtube
        public override void LoadContent()
        {
            throw new NotImplementedException();
        }

        public override void LoadPreview()
        {
            throw new NotImplementedException();
        }

        ObservableCollection<Tuple<string, string>> AvailableStreams { get; private set; }


        private string _selectedStream;
        public string SelectedStream
        {
            get
            {
                return _selectedStream;
            }
            set
            {
                _selectedStream = value;
                RaisePropertyChanged("SelectedStream");
            }
        }

        
    }
}
