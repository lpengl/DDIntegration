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
        public static List<OapiAttendanceListResponse.RecordresultDomain> GetAttendanceRecords(DateTime startDate, bool firstSync)
        {
            string accessToken = Common.GetAccessToken();
            List<string> userIds = TongXunLu.GetAllUsers(accessToken);
            return KaoQin.GetAttendanceRecords(accessToken, userIds, startDate, firstSync);
        }

        public static List<OapiSmartworkHrmEmployeeListResponse.EmpFieldInfoVODomain> GetEmployeesInfo()
        {
            string accessToken = Common.GetAccessToken();
            List<string> userIds = TongXunLu.GetAllUsers(accessToken);
            return ZhiNengRenShi.GetEmployeesInfo(accessToken, userIds);
        }
    }
}
