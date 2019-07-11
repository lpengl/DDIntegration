using DingTalk.Api;
using DingTalk.Api.Request;
using DingTalk.Api.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDIntegration
{
    /// <summary>
    /// 考勤
    /// </summary>
    class KaoQin
    {
        private const string ListAttendanceRecordUrl = "https://oapi.dingtalk.com/attendance/listRecord";

        public static List<OapiAttendanceListResponse.RecordresultDomain> GetAttendanceRecords(
            string accessToken, 
            List<string> userIds,
            DateTime startDate,
            bool firstSync)
        {
            if(userIds == null || userIds.Count == 0)
            {
                return null;
            }

            List<OapiAttendanceListResponse.RecordresultDomain> results = new List<OapiAttendanceListResponse.RecordresultDomain>();

            DateTime now = DateTime.Now;
            DateTime to = new DateTime(now.Year, now.Month, now.Day);
            to = to.AddSeconds(-1);
            DateTime from = new DateTime(startDate.Year, startDate.Month, startDate.Day);
            if(from > to)
            {
                return results;
            }

            if (!firstSync)
            {
                // 如果不是程序启动的第一次同步，获取传进来的起始时间的前三天，主要是为了拿到补卡数据
                DateTime tempFrom = startDate.AddDays(-3);
                from = new DateTime(tempFrom.Year, tempFrom.Month, tempFrom.Day);
            }

            int takeUserCount = 50;
            for(int i = 0; i < userIds.Count; i = i + 50)
            {
                if(i + takeUserCount > userIds.Count)
                {
                    takeUserCount = userIds.Count - i;
                }
                results.AddRange(GetAttendanceRecords(accessToken, userIds.GetRange(i, takeUserCount), from, to));
            }

            if (!firstSync)
            {
                results = results.Where(r => {
                    bool sourceTypeIsApprove = false;
                    if (!string.IsNullOrEmpty(r.SourceType) && r.SourceType.ToUpper() == "APPROVE")
                    {
                        sourceTypeIsApprove = true;
                    }

                    bool isCurrentDay = false;
                    DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    DateTime workDate = origin.AddMilliseconds(long.Parse(r.WorkDate)).ToLocalTime();
                    if (workDate.Day == to.Day)
                    {
                        isCurrentDay = true;
                    }

                    return sourceTypeIsApprove || isCurrentDay;
                }).ToList();
            }
            
            return results;
        }

        private static List<OapiAttendanceListResponse.RecordresultDomain> GetAttendanceRecords(
            string accessToken, 
            List<string> userIds,
            DateTime from,
            DateTime to)
        {
            List<OapiAttendanceListResponse.RecordresultDomain> results = new List<OapiAttendanceListResponse.RecordresultDomain>();

            while(from < to)
            {
                DateTime tempTo = to;
                if(from.AddDays(7) < to)
                {
                    tempTo = from.AddDays(7);
                }

                results.AddRange(GetAttendanceRecordsCore(accessToken, userIds, from, tempTo));
                from = tempTo;
            }

            return results;
        }

        private static List<OapiAttendanceListResponse.RecordresultDomain> GetAttendanceRecordsCore(
            string accessToken,
            List<string> userIds,
            DateTime from,
            DateTime to)
        {
            List<OapiAttendanceListResponse.RecordresultDomain> results = new List<OapiAttendanceListResponse.RecordresultDomain>();

            long offset = 0L;
            long limit = 50L;
            while (true)
            {
                DefaultDingTalkClient client = new DefaultDingTalkClient("https://oapi.dingtalk.com/attendance/list");
                OapiAttendanceListRequest request = new OapiAttendanceListRequest();
                request.WorkDateFrom = from.ToString("yyyy-MM-dd HH:mm:ss");
                request.WorkDateTo = to.ToString("yyyy-MM-dd HH:mm:ss");
                request.UserIdList = userIds;
                request.Offset = offset;
                request.Limit = limit;
                OapiAttendanceListResponse response = client.Execute(request, accessToken);
                if (response.Errcode != 0)
                {
                    throw new Exception("获取打卡数据失败，错误信息: " + response.Errmsg);
                }

                if (response.Recordresult == null)
                {
                    break;
                }

                results.AddRange(response.Recordresult);

                if (!response.HasMore)
                {
                    break;
                }
                offset += limit;
            }

            return results;
        }

        private static bool MatchAttendanceResult(OapiAttendanceListResponse.RecordresultDomain attendance)
        {
            bool sourceTypeIsApprove = false;
            if(!string.IsNullOrEmpty(attendance.SourceType) && attendance.SourceType.ToUpper() == "APPROVE")
            {
                sourceTypeIsApprove = true;
            }

            bool isCurrentDay = false;
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            return sourceTypeIsApprove || isCurrentDay;
        }
    }
}
