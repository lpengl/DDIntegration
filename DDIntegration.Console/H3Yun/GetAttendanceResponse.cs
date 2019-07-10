using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDIntegration
{
    public class GetAttendanceResponse
    {
        public int DataType { get; set; }
        public string ErrorMessage { get; set; }
        public bool Logined { get; set; }
        public bool Successful { get; set; }
        public GetAttendanceResponseReturnData ReturnData { get; set; }
    }

    public class GetAttendanceResponseReturnData
    {
        public List<H3YunAttendance> BizObjectArray { get; set; }
    }
}
