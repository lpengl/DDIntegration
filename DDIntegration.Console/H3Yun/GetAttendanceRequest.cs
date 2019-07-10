using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDIntegration
{
    public class GetAttendanceRequest
    {
        public string ActionName { get; set; }
        public string SchemaCode { get; set; }
        public string Filter { get; set; }
    }
}
