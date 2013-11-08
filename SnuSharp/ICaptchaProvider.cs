using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnuSharp
{
    public interface ICaptchaProvider
    {
        Task<string> GetCaptchaResponse(string captchaIden);
    }
}
