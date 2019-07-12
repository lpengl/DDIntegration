using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDIntegration
{
    public class GetJieSuanResponse : H3YunResponse
    {
        public GetJieSuanReturnData ReturnData { get; set; }
    }

    public class GetJieSuanReturnData
    {
        public List<H3YunJieSuan> BizObjectArray { get; set; }
    }
}
