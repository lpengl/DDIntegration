using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDIntegration
{
    public class UpdateBasicPaymentRequest : H3YunRequest
    {
        public string BizObjectId { get; set; }
        public string BizObject { get; set; }
    }
}
