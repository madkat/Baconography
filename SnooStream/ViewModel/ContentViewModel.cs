using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnooStream.ViewModel
{
    public abstract class ContentViewModel : ViewModelBase
    {
        public abstract void LoadContent();
        public abstract void LoadPreview();
        public bool Loaded { get; set; }
        public int PreviewLoadPercent { get; set; }
    }
}
