using GalaSoft.MvvmLight;
using SnooStream.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnooStream.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        public PrimaryLiveTileViewModel PrimaryLiveTile { get; private set; }
        public SecondaryLiveTileHubViewModel SecondaryLiveTileHub { get; private set; }
        public LockScreenViewModel LockScreen { get; private set; }
        public AppearanceSettingsViewModel LayoutSettings { get; private set; }
        public ContentSettingsViewModel ContentSettings { get; private set; }
        public Settings Settings { get; private set; }
    }
}
