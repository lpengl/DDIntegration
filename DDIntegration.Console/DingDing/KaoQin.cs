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
            DateTime startDate)
        {
            if(userIds == null || userIds.Count == 0)
            {
                return null;
            }

            List<OapiAttendanceListResponse.RecordresultDomain> results = new List<OapiAttendanceListResponse.RecordresultDomain>();

            DateTime now = DateTime.Now;
            DateTime to = new DateTime(now.Year, now.Month, now.Day);
            to = to.AddSeconds(-1);
            DateTime from = startDate;
            if(from > to)
            {
                from = new DateTime(to.Year, to.Month, to.Day);
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
    }
}
