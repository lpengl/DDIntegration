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
        /// <summary>
        /// 获取所有用户Id
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public static List<string> GetAllUserIds(string accessToken)
        {
            return TongXunLu.GetAllUsers(accessToken);
        }

        /// <summary>
        /// 获取指定时间段内的打卡记录
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="userIds"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static List<OapiAttendanceListResponse.RecordresultDomain> GetAttendanceRecordsInfo(
            string accessToken, 
            List<string> userIds,
            DateTime startDate,
            DateTime endDate)
        {
            return KaoQin.GetAttendanceRecordsInfo(accessToken, userIds, startDate, endDate);
        }

        /// <summary>
        /// 获取花名册数据
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public static List<OapiSmartworkHrmEmployeeListResponse.EmpFieldInfoVODomain> GetEmployeesInfo(string accessToken)
        {
            List<string> userIds = TongXunLu.GetAllUsers(accessToken);
            return ZhiNengRenShi.GetEmployeesInfo(accessToken, userIds);
        }

        /// <summary>
        /// 获取请假数据
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="userIds"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static List<LeaveStatus> GetLeaveStatus(string accessToken, List<string> userIds, DateTime startDate, DateTime endDate)
        {
            return KaoQin.GetLeaveStatus(accessToken, userIds, startDate, endDate);
        }
    }
}
