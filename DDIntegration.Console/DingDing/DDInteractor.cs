using DingTalk.Api.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDIntegration
{
    class DDInteractor
    {
        public static List<string> GetAllUserIds(string accessToken)
        {
            return TongXunLu.GetAllUsers(accessToken);
        }

        public static List<OapiAttendanceListResponse.RecordresultDomain> GetAttendanceRecordsInfo(
            string accessToken, 
            List<string> userIds,
            DateTime startDate,
            DateTime endDate)
        {
            return KaoQin.GetAttendanceRecordsInfo(accessToken, userIds, startDate, endDate);
        }

        public static List<OapiSmartworkHrmEmployeeListResponse.EmpFieldInfoVODomain> GetEmployeesInfo(string accessToken)
        {
            List<string> userIds = TongXunLu.GetAllUsers(accessToken);
            return ZhiNengRenShi.GetEmployeesInfo(accessToken, userIds);
        }

        public static List<LeaveStatus> GetLeaveStatus(string accessToken, List<string> userIds, DateTime startDate, DateTime endDate)
        {
            return KaoQin.GetLeaveStatus(accessToken, userIds, startDate, endDate);
        }
    }
}
