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
        public ContentViewModel(ViewModelBase context)
        {
            Context = context;
        }

        public Task BeginLoad()
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
                                if (tsk.IsCompleted)
                                {
                                    Loaded = true;
                                    Loading = false;
                                }
                            });
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
