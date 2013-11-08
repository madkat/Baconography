using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnuSharp
{
    public interface ICreated : IThingData
    {
        DateTime Created { get; set; }
        DateTime CreatedUTC { get; set; }
    }
}
