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

            List<H3YunAttendance> onDutyAttendances = attendances.Where(a => a.F0000007 == "上班").OrderBy(a => a.F0000012).ToList();
            List<H3YunAttendance> offDutyAttendances = attendances.Where(a => a.F0000007 == "下班").OrderByDescending(a => a.F0000012).ToList();

            try
            {
                DateTime amOnDutyTime = onDutyAttendances[0].F0000012;
                DateTime pmOnDutyTime = DateTime.MinValue;
                if (onDutyAttendances.Count > 1)
                {
                    pmOnDutyTime = onDutyAttendances[1].F0000012;
                }

                DateTime amOffDutyTime = DateTime.MinValue;
                DateTime pmOffDutyTime = offDutyAttendances[0].F0000012;
                if (offDutyAttendances.Count > 1)
                {
                    amOffDutyTime = offDutyAttendances[1].F0000012;
                }

                // 请假一天
                if (leaveStatus.Where(s => s.starttime <= amOnDutyTime && s.endtime >= pmOffDutyTime).Any())
                {
                    return result;
                }

                // 上午请假
                if (leaveStatus.Where(s => s.starttime <= amOnDutyTime && s.endtime <= amOnDutyTime.AddHours(4)).Any())
                {
                    H3YunAttendance onDutyAttendance = onDutyAttendances.Last();
                    H3YunAttendance offDutyAttendance = offDutyAttendances.First();
                    if (onDutyAttendance.F0000008 == "未打卡" && offDutyAttendance.F0000008 == "未打卡")
                    {
                        result.F0000005 = 0.5;
                    }

                    return result;
                }

                // 下午请假
                if (leaveStatus.Where(s => s.starttime > amOnDutyTime && s.starttime <= amOnDutyTime.AddHours(4) && s.endtime >= pmOffDutyTime).Any())
                {
                    H3YunAttendance onDutyAttendance = onDutyAttendances.First();
                    H3YunAttendance offDutyAttendance = offDutyAttendances.Last();
                    if (onDutyAttendance.F0000008 == "未打卡" && offDutyAttendance.F0000008 == "未打卡")
                    {
                        result.F0000005 = 0.5;
                    }

                    return result;
                }

                // 未请假并全部未打卡
                if (attendances.Where(a => a.F0000008 == "未打卡").Count() == attendances.Count)
                {
                    result.F0000005 = 1;
                    return result;
                }

                // 不存在未打卡
                if (attendances.Where(a => a.F0000008 == "未打卡").Count() == 0)
                {
                    result.F0000004 = 1;
                    return result;
                }

                // 上午旷工
                if (attendances.Where(a => a.F0000012 == amOnDutyTime && a.F0000008 == "未打卡").Any() &&
                    attendances.Where(a => a.F0000012 == amOffDutyTime && a.F0000008 == "未打卡").Any())
                {
                    result.F0000005 = 0.5;

                    return result;
                }

                // 下午旷工
                if (attendances.Where(a => a.F0000012 == pmOnDutyTime && a.F0000008 == "未打卡").Any() &&
                    attendances.Where(a => a.F0000012 == pmOffDutyTime && a.F0000008 == "未打卡").Any())
                {
                    result.F0000005 = 0.5;
                    return result;
                }
            }
            catch(Exception ex)
            {
            }
            
            return result;
        }
    }
}
