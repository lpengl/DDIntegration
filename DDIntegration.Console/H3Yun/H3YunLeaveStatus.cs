using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDIntegration
{
    public class H3YunLeaveStatus
    {
        /// <summary>
        /// 人员ID
        /// </summary>
        public string F0000001 { get; set; }

        /// <summary>
        /// 请假时长
        /// </summary>
        public double F0000002 { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string F0000003 { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime F0000004 { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime F0000005 { get; set; }

        public static H3YunLeaveStatus ConvertFrom(LeaveStatus leaveStatus)
        {
            H3YunLeaveStatus result = new H3YunLeaveStatus();

            result.F0000001 = leaveStatus.userid;
            result.F0000002 = leaveStatus.duration_percent / 100.0;
            if(leaveStatus.duration_unit == "percent_day")
            {
                result.F0000003 = "天";
            }
            else if(leaveStatus.duration_unit == "percent_hour")
            {
                result.F0000003 = "小时";
            }
            result.F0000004 = leaveStatus.starttime;
            result.F0000005 = leaveStatus.endtime;

            return result;
        }
    }
}
