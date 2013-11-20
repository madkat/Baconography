using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnooStream.ViewModel
{
    public class UserHubViewModel : ViewModelBase
    {
        public SelfViewModel Self {get; private set;}
        public LoginViewModel Login { get; private set; }
    }
}
