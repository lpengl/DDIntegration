using DingTalk.Api.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDIntegration
{
    public class H3YunAttendance
    {
        /// <summary>
        /// 打卡ID
        /// </summary>
        public string F0000001 { get; set; }

        /// <summary>
        /// 考勤组ID
        /// </summary>
        public string F0000002 { get; set; }

        /// <summary>
        /// 排班ID
        /// </summary>
        public string F0000003 { get; set; }

        /// <summary>
        /// 打卡记录ID
        /// </summary>
        public string F0000004 { get; set; }

        /// <summary>
        /// 工作日
        /// </summary>
        public DateTime F0000005 { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public string F0000006 { get; set; }

        /// <summary>
        /// 考勤类型
        /// </summary>
        public string F0000007 { get; set; }

        /// <summary>
        /// 时间结果
        /// </summary>
        public string F0000008 { get; set; }

        /// <summary>
        /// 打卡位置
        /// </summary>
        public string F0000009 { get; set; }

        /// <summary>
        /// 审批ID
        /// </summary>
        public string F0000010 { get; set; }

        /// <summary>
        /// 审批实例ID
        /// </summary>
        public string F0000011 { get; set; }

        /// <summary>
        /// 打卡基准时间
        /// </summary>
        public DateTime F0000012 { get; set; }

        /// <summary>
        /// 实际打卡时间
        /// </summary>
        public DateTime F0000013 { get; set; }

        /// <summary>
        /// 数据来源
        /// </summary>
        public string F0000014 { get; set; }

        /// <summary>
        /// 迟到分钟数
        /// </summary>
        public int F0000015 { get; set; }

        /// <summary>
        /// 早退分钟数
        /// </summary>
        public int F0000016 { get; set; }

        /// <summary>
        /// 结算月份
        /// </summary>
        public string F0000017 { get; set; }
        
        public static H3YunAttendance ConvertFrom(OapiAttendanceListResponse.RecordresultDomain record)
        {
            if(record == null)
            {
                return null;
            }
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            H3YunAttendance result = new H3YunAttendance();
            result.F0000001 = record.Id.ToString();
            result.F0000002 = record.GroupId.ToString();
            result.F0000003 = record.PlanId.ToString();
            result.F0000004 = record.RecordId.ToString();
            result.F0000005 = Utility.ParseTime(record.WorkDate);
            result.F0000006 = record.UserId;
            result.F0000007 = GetCheckType(record.CheckType);
            result.F0000008 = GetTimeResult(record.TimeResult);
            result.F0000009 = GetLocationResult(record.LocationResult);
            result.F0000010 = record.ApproveId.ToString();
            result.F0000011 = record.ProcInstId;
            result.F0000012 = Utility.ParseTime(record.BaseCheckTime);
            result.F0000013 = Utility.ParseTime(record.UserCheckTime);
            result.F0000014 = GetSourceType(record.SourceType);
            if(result.F0000008 == "迟到")
            {
                result.F0000015 = (result.F0000013 - result.F0000012).Minutes;
            }
            else if (result.F0000008 == "早退")
            {
                result.F0000016 = (result.F0000012 - result.F0000013).Minutes;
            }
            result.F0000017 = result.F0000005.ToString("yyyy-MM");

            return result;
        }

        private static string GetCheckType(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            switch (input.ToLower())
            {
                case "onduty":
                    return "上班";
                case "offduty":
                    return "下班";
                default:
                    return string.Empty;
            }
        }

        private static string GetTimeResult(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            switch (input.ToLower())
            {
                case "normal":
                    return "正常";
                case "early":
                    return "早退";
                case "late":
                    return "迟到";
                case "seriousLate":
                    return "严重迟到";
                case "absenteeism":
                    return "旷工迟到";
                case "notsigned":
                    return "未打卡";
                default:
                    return string.Empty;
            }
        }

        private static string GetLocationResult(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            switch (input.ToLower())
            {
                case "Normal":
                    return "范围内";
                case "Outside":
                    return "范围外";
                case "NotSigned":
                    return "未打卡";
                default:
                    return string.Empty;
            }
        }

        private static string GetSourceType(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            switch (input.ToUpper())
            {
                case "ATM":
                    return "考勤机";
                case "BEACON":
                    return "IBeacon";
                case "DING_ATM":
                    return "钉钉考勤机";
                case "USER":
                    return "用户打卡";
                case "BOSS":
                    return "老板改签";
                case "APPROVE":
                    return "审批系统";
                case "SYSTEM":
                    return "考勤系统";
                case "AUTO_CHECK":
                    return "自动打卡";
                default:
                    return string.Empty;
            }
        }
    }
}
