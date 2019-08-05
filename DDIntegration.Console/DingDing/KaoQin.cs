using DingTalk.Api;
using DingTalk.Api.Request;
using DingTalk.Api.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DDIntegration
{
    /// <summary>
    /// 考勤
    /// </summary>
    class KaoQin
    {
        private const string ListAttendanceRecordUrl = "https://oapi.dingtalk.com/attendance/listRecord";
        private const string GetLeaveStatusUrl = "https://oapi.dingtalk.com/topapi/attendance/getleavestatus";

        public static List<OapiAttendanceListResponse.RecordresultDomain> GetAttendanceRecordsInfo(
            string accessToken, 
            List<string> userIds,
            DateTime startDate,
            DateTime endDate)
        {
            List<OapiAttendanceListResponse.RecordresultDomain> results = new List<OapiAttendanceListResponse.RecordresultDomain>();

            if(userIds == null || userIds.Count == 0)
            {
                return results;
            }
            
            int takeUserCount = 50;
            for(int i = 0; i < userIds.Count; i = i + 50)
            {
                if(i + takeUserCount > userIds.Count)
                {
                    takeUserCount = userIds.Count - i;
                }
                results.AddRange(GetAttendanceRecords(accessToken, userIds.GetRange(i, takeUserCount), startDate, endDate));
            }
            
            return results;
        }

        private static List<OapiAttendanceListResponse.RecordresultDomain> GetAttendanceRecords(
            string accessToken, 
            List<string> userIds,
            DateTime startDate,
            DateTime endDate)
        {
            List<OapiAttendanceListResponse.RecordresultDomain> results = new List<OapiAttendanceListResponse.RecordresultDomain>();

            while(startDate < endDate)
            {
                DateTime tempTo = endDate;
                if(startDate.AddDays(7) < endDate)
                {
                    tempTo = startDate.AddDays(7);
                }

                results.AddRange(GetAttendanceRecordsCore(accessToken, userIds, startDate, tempTo));
                Thread.Sleep(1000);
                startDate = tempTo;
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

        public static List<LeaveStatus> GetLeaveStatus(
            string accessToken,
            List<string> userIds,
            DateTime startDate,
            DateTime endDate)
        {
            List<LeaveStatus> results = new List<LeaveStatus>();

            if (userIds == null || userIds.Count == 0)
            {
                return results;
            }

            int takeUserCount = 100;
            for (int i = 0; i < userIds.Count; i = i + 100)
            {
                if (i + takeUserCount > userIds.Count)
                {
                    takeUserCount = userIds.Count - i;
                }
                results.AddRange(GetLeaveStatusCore(accessToken, userIds.GetRange(i, takeUserCount), startDate, endDate));
            }


            return results;
        }

        private static List<LeaveStatus> GetLeaveStatusCore(
            string accessToken,
            List<string> userIds,
            DateTime startDate,
            DateTime endDate)
        {
            List<LeaveStatus> results = new List<LeaveStatus>();
            
            DefaultDingTalkClient client = new DefaultDingTalkClient(GetLeaveStatusUrl);
            OapiAttendanceGetleavestatusRequest req = new OapiAttendanceGetleavestatusRequest();
            req.UseridList = string.Join(",", userIds);
            req.StartTime = Utility.GetUnixTimeSpan(startDate);
            req.EndTime = Utility.GetUnixTimeSpan(endDate);

            long offset = 0L;
            long limit = 20L;
            while (true)
            {
                req.Offset = offset;
                req.Size = limit;
                OapiAttendanceGetleavestatusResponse rsp = client.Execute(req, accessToken);

                if (rsp.Errcode != 0)
                {
                    throw new Exception("获取钉钉请假数据失败，错误信息：" + rsp.Errmsg);
                }
                
                LeaveStatusResponse leaveResult = JsonConvert.DeserializeObject<LeaveStatusResponse>(rsp.Body);
                if(leaveResult == null || leaveResult.result == null || leaveResult.result.leave_status == null)
                {
                    break;
                }

                results.AddRange(leaveResult.result.leave_status);
                if (!leaveResult.result.has_more)
                {
                    break;
                }

                offset = offset + limit;
            }

            return results;
        }
    }
}
