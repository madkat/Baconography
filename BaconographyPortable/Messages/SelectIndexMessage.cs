using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaconographyPortable.Messages
{
    public class SelectIndexMessage : MessageBase
    {
        public int Index { get; set; }
        public Type TypeContext { get; set; }
    }
}
