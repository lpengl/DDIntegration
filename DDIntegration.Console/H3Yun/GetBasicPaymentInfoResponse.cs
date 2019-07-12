using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDIntegration
{
    public class GetBasicPaymentInfoResponse : H3YunResponse
    {
        public GetBasicPaymentInfoResponseReturnData ReturnData { get; set; }
    }

    public class GetBasicPaymentInfoResponseReturnData
    {
        public List<H3YunBasicPaymentInfo> BizObjectArray { get; set; }
    }
}
