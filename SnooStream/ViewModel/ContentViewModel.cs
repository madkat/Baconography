using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SnooStream.Common;

namespace SnooStream.ViewModel
{
    public abstract class ContentViewModel : ViewModelBase
    {
        public ContentViewModel(ViewModelBase context)
        {
            Context = context;
        }

        public Task BeginLoad(bool highPriority)
        {
            if (ContentLoadTask == null)
            {
                lock (this)
                {
                    if (ContentLoadTask == null)
                    {
                        Loading = true;
                        ContentLoadTask = LoadContent().ContinueWith((tsk) => 
                            {
                                var tskResult = tsk.WasSuccessfull();
                                if (tskResult)
                                {
                                    Loaded = true;
                                    Loading = false;

                                    RaisePropertyChanged("Loaded");
                                    RaisePropertyChanged("Loading");
                                }
                            }, SnooStreamViewModel.UIScheduler);
                    }
                }
            }
            return ContentLoadTask;
        }
        Task ContentLoadTask { get; set; }
        protected abstract Task LoadContent();
        public ViewModelBase Context { get; private set; }
        public bool Loaded { get; set; }
        public bool Loading { get; set; }
        public int PreviewLoadPercent { get; set; }
    }
}
