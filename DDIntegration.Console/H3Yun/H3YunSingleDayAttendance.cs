using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDIntegration
{
    public class H3YunSingleDayAttendance
    {
        /// <summary>
        /// 出勤日期
        /// </summary>
        public DateTime F0000003 { get; set; }

        /// <summary>
        /// 人员姓名
        /// </summary>
        public string F0000001 { get; set; }

        /// <summary>
        /// 人员ID
        /// </summary>
        public string F0000002 { get; set; }

        /// <summary>
        /// 出勤天数
        /// </summary>
        public double F0000004 { get; set; }

        /// <summary>
        /// 旷工天数
        /// </summary>
        public double F0000005 { get; set; }

        /// <summary>
        /// 早晚缺卡次数
        /// </summary>
        public int F0000006 { get; set; }

        /// <summary>
        /// 中午缺卡次数
        /// </summary>
        public int F0000007 { get; set; }

        /// <summary>
        /// 结算月份
        /// </summary>
        public string F0000009 { get; set; }

        public static H3YunSingleDayAttendance ConvertFrom(List<H3YunAttendance> attendances, List<LeaveStatus> leaveStatus, string userId)
        {
            H3YunSingleDayAttendance result = new H3YunSingleDayAttendance();
            H3YunAttendance attendance = attendances[0];

            result.F0000001 = userId;
            result.F0000002 = attendance.F0000006;
            result.F0000003 = attendance.F0000005;
            result.F0000009 = attendance.F0000005.ToString("yyyy-MM");

            if(attendances.Where(a => a.F0000008 == "正常").Count() == 4)
            {
                //result.F0000004 = 1;
            }
            else if(attendances.Where(a => a.F0000008 == "").Count() == 4)
            {

            }

            return result;
        }
    }
}
