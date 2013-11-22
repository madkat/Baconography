using GalaSoft.MvvmLight;
using SnooStream.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnooStream.ViewModel
{
    public class UserHubViewModel : ViewModelBase
    {
        public UserHubViewModel(InitializationBlob initializationBlob)
        {
            Login = new LoginViewModel();
            Self = new SelfViewModel(initializationBlob.SelfThings, initializationBlob.AfterSelfMessage, initializationBlob.AfterSelfSentMessage, initializationBlob.AfterSelfAction);
        }
        public SelfViewModel Self {get; private set;}
        public LoginViewModel Login { get; private set; }
    }
}
