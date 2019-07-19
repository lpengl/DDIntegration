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
            // 如果只打了一次卡，记为旷工
            if(attendances.Count == 1)
            {
                result.F0000005 = 1;
                return result;
            }

            List<H3YunAttendance> weiDaKaAttends = attendances.Where(a => a.F0000008 == "未打卡").ToList();

            List<H3YunAttendance> onDutyAttendances = attendances.Where(a => a.F0000007 == "上班").OrderBy(a => a.F0000012).ToList();
            List<H3YunAttendance> offDutyAttendances = attendances.Where(a => a.F0000007 == "下班").OrderByDescending(a => a.F0000012).ToList();

            try
            {
                DateTime amOnDutyTime = DateTime.MinValue;
                DateTime pmOnDutyTime = DateTime.MinValue;
                if (onDutyAttendances.Count > 1)
                {
                    amOnDutyTime = onDutyAttendances[0].F0000012;
                    pmOnDutyTime = onDutyAttendances[1].F0000012;
                }
                else
                {
                    if(onDutyAttendances[0].F0000012.Hour < 12)
                    {
                        amOnDutyTime = onDutyAttendances[0].F0000012;
                        pmOnDutyTime = new DateTime(amOnDutyTime.Year, amOnDutyTime.Month, amOnDutyTime.Day, 13, 0, 0);
                    }
                    else
                    {
                        pmOnDutyTime = onDutyAttendances[0].F0000012;
                        amOnDutyTime = new DateTime(pmOnDutyTime.Year, pmOnDutyTime.Month, pmOnDutyTime.Day, 8, 0, 0);
                    }
                }

                DateTime amOffDutyTime = DateTime.MinValue;
                DateTime pmOffDutyTime = DateTime.MinValue;
                if (offDutyAttendances.Count > 1)
                {
                    pmOffDutyTime = offDutyAttendances[0].F0000012;
                    amOffDutyTime = offDutyAttendances[1].F0000012;
                }
                else
                {
                    if(offDutyAttendances[0].F0000012.Hour < 14)
                    {
                        amOffDutyTime = offDutyAttendances[0].F0000012;
                        pmOffDutyTime = new DateTime(amOffDutyTime.Year, amOffDutyTime.Month, amOffDutyTime.Day, 17, 0, 0);
                    }
                    else
                    {
                        pmOffDutyTime = offDutyAttendances[0].F0000012;
                        amOffDutyTime = new DateTime(pmOffDutyTime.Year, pmOffDutyTime.Month, pmOffDutyTime.Day, 12, 0, 0);
                    }
                }

                // 请假一天
                if (leaveStatus.Where(s => s.starttime <= amOnDutyTime && s.endtime >= pmOffDutyTime).Any())
                {
                    return result;
                }

                // 上午请假
                if (leaveStatus.Where(s => s.starttime <= amOnDutyTime && s.endtime <= amOnDutyTime.AddHours(4)).Any())
                {
                    // 获取下午的打卡情况
                    H3YunAttendance onDutyAttendance = onDutyAttendances.FirstOrDefault(a => a.F0000012 > amOffDutyTime);
                    H3YunAttendance offDutyAttendance = offDutyAttendances.FirstOrDefault(a => a.F0000012 > pmOnDutyTime);
                    if (onDutyAttendance == null || offDutyAttendance == null || 
                        onDutyAttendance.F0000008 == "未打卡" && offDutyAttendance.F0000008 == "未打卡")
                    {
                        result.F0000005 = 0.5;
                    }
                    return result;
                }

                // 下午请假
                if (leaveStatus.Where(s => s.starttime > amOnDutyTime && s.starttime <= amOnDutyTime.AddHours(4) && s.endtime >= pmOffDutyTime).Any())
                {
                    // 获取上午的打卡情况
                    H3YunAttendance onDutyAttendance = onDutyAttendances.FirstOrDefault(a => a.F0000012 < amOffDutyTime);
                    H3YunAttendance offDutyAttendance = offDutyAttendances.FirstOrDefault(a => a.F0000012 < pmOnDutyTime);
                    if (onDutyAttendance == null || offDutyAttendance == null || 
                        onDutyAttendance.F0000008 == "未打卡" && offDutyAttendance.F0000008 == "未打卡")
                    {
                        result.F0000005 = 0.5;
                    }
                    return result;
                }


                // 未请假并且全部未打卡或大于三次未打卡，旷工一天
                if (weiDaKaAttends.Count() == attendances.Count ||
                    weiDaKaAttends.Count() >= 3)
                {
                    result.F0000005 = 1;
                    return result;
                }

                // 上午旷工
                if (onDutyAttendances.Where(a => a.F0000012 < amOffDutyTime && a.F0000008 == "未打卡").Any() &&
                    offDutyAttendances.Where(a => a.F0000012 < pmOnDutyTime && a.F0000008 == "未打卡").Any())
                {
                    result.F0000004 = 0.5;
                    result.F0000005 = 0.5;
                    return result;
                }

                // 下午旷工
                if (onDutyAttendances.Where(a => a.F0000012 > amOffDutyTime && a.F0000008 == "未打卡").Any() &&
                    offDutyAttendances.Where(a => a.F0000012 > pmOnDutyTime && a.F0000008 == "未打卡").Any())
                {
                    result.F0000004 = 0.5;
                    result.F0000005 = 0.5;
                    return result;
                }

                // 没有未打卡
                if(weiDaKaAttends.Count() == 0)
                {
                    result.F0000004 = 1;
                    return result;
                }

                foreach(H3YunAttendance att in weiDaKaAttends)
                {
                    if(att.F0000007 == "上班" && att.F0000012 < amOffDutyTime || att.F0000007 == "下班" && att.F0000012 > pmOnDutyTime)
                    {
                        result.F0000006 += 1;
                    }
                    else
                    {
                        result.F0000007 += 1;
                    }
                    result.F0000004 = 1;
                }
            }
            catch(Exception ex)
            {
            }
            
            return result;
        }
    }
}
