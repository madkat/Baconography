using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnooStream.ViewModel
{
    public class SelfContentViewModel : ContentViewModel
    {
        protected override async Task LoadContent()
        {
            //this is essentially a nop, we have everything from the link we came from
        }
    }
}
