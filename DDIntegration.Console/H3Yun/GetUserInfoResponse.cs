using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDIntegration
{
    public class GetUserInfoResponse : H3YunResponse
    {
        public Dictionary<string, string> ReturnData { get; set; }
    }
}
