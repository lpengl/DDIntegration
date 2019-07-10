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

        public static H3YunAttendance ConvertFrom(OapiAttendanceListResponse.RecordresultDomain record)
        {
            if(record == null)
            {
                return null;
            }

            H3YunAttendance result = new H3YunAttendance();
            result.F0000001 = record.Id.ToString();
            result.F0000002 = record.GroupId.ToString();
            result.F0000003 = record.PlanId.ToString();
            result.F0000004 = record.ProcInstId.ToString();
            result.F0000005 = new DateTime(long.Parse(record.WorkDate));
            result.F0000006 = record.UserId;
            result.F0000007 = record.CheckType;
            result.F0000008 = record.TimeResult;
            result.F0000009 = record.LocationResult;
            result.F0000010 = record.ApproveId.ToString();
            result.F0000011 = record.ProcInstId;
            result.F0000012 = new DateTime(long.Parse(record.BaseCheckTime));
            result.F0000013 = new DateTime(long.Parse(record.UserCheckTime));
            result.F0000014 = record.SourceType;

            return result;
        }
    }
}
