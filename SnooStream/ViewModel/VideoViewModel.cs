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

        public ObservableCollection<Tuple<string, string>> AvailableStreams { get; private set; }

        public byte[] Preview { get; private set; }

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

        protected override async Task LoadContent()
        {
            
        }
    }
}
