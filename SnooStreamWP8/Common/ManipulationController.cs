using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SnooStreamWP8.Common
{
    public class ManipulationController
    {
        public event EventHandler<GestureEventArgs> DoubleTap;
        public event EventHandler<ManipulationStartedEventArgs> ManipulationStarted;
        public event EventHandler<ManipulationDeltaEventArgs> ManipulationDelta;
        public event EventHandler<ManipulationCompletedEventArgs> ManipulationCompleted;

        public void FireDoubleTap(object obj, GestureEventArgs args)
        {
            if (DoubleTap != null)
                DoubleTap(obj, args);
        }

        public void FireManipulationStarted(object obj, ManipulationStartedEventArgs args)
        {
            if (ManipulationStarted != null)
                ManipulationStarted(obj, args);
        }

        public void FireManipulationDelta(object obj, ManipulationDeltaEventArgs args)
        {
            if (ManipulationDelta != null)
                ManipulationDelta(obj, args);
        }

        public void FireManipulationCompleted(object obj, ManipulationCompletedEventArgs args)
        {
            if (ManipulationCompleted != null)
                ManipulationCompleted(obj, args);
        }
    }
}
