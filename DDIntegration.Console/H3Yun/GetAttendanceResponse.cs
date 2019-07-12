using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDIntegration
{
    public class GetAttendanceResponse : H3YunResponse
    {
        public GetAttendanceResponseReturnData ReturnData { get; set; }
    }

    public class GetAttendanceResponseReturnData
    {
        public List<H3YunAttendance> BizObjectArray { get; set; }
    }
}
