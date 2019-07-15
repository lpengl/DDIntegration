using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDIntegration
{
    public class LeaveStatusResponse
    {
        public int errcode { get; set; }
        public string request_id { get; set; }
        public bool success { get; set; }
        public LeaveStatusResult result { get; set; }
    }

    public class LeaveStatusResult
    {
        public bool has_more { get; set; }
        public List<LeaveStatus> leave_status { get; set; }
    }

    public class LeaveStatus
    {
        public int duration_percent { get; set; }
        public string duration_unit { get; set; }

        public long start_time { get; set; }

        public DateTime starttime
        {
            get
            {
                return Utility.ParseTime(this.start_time);
            }
        }

        public long end_time { get; set; }

        public DateTime endtime
        {
            get
            {
                return Utility.ParseTime(this.end_time);
            }
        }
        public string userid { get; set; }
    }
}
