using DingTalk.Api.Response;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DDIntegration
{
    class Program
    {
        private static DateTime _lastSyncAttendanceTime = DateTime.MinValue;
        private static DateTime _lastSyncEmployeeInfoTime = DateTime.MinValue;

        static void Main(string[] args)
        {
            while (true)
            {
                DateTime now = DateTime.Now;

                SyncAttendanceData(now);
                SyncBasicPaymentInfo(now);

                _lastSyncAttendanceTime = now;
                _lastSyncEmployeeInfoTime = now;
                Thread.Sleep(10 * 60 * 1000);
            }
        }

        static DateTime GetStartDate(DateTime now)
        {
            DateTime lastMonthNow = now.AddMonths(-1);
            DateTime startSyncWorkDate = new DateTime(lastMonthNow.Year, lastMonthNow.Month, 1);
            return startSyncWorkDate;
        }

        static DateTime GetEndDate(DateTime startDate)
        {
            return startDate.AddMonths(1).AddSeconds(-1);
        }

        static void SyncAttendanceData(DateTime now)
        {
            try
            {
                if (_lastSyncAttendanceTime == DateTime.MinValue ||
                    _lastSyncAttendanceTime.AddMinutes(30) < now)
                {
                    bool needSyncAttendance = H3YunInteractor.NeedSyncAttendanceData();
                    if (needSyncAttendance)
                    {
                        string accessToken = Common.GetAccessToken();
                        List<string> userIds = DDInteractor.GetAllUserIds(accessToken);

                        DateTime startDate = GetStartDate(now);
                        DateTime endDate = GetEndDate(startDate);

                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - 开始获取钉钉打卡数据...");
                        List<OapiAttendanceListResponse.RecordresultDomain> attendances = DDInteractor.GetAttendanceRecordsInfo(accessToken, userIds, startDate, endDate);
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - 获取钉钉打卡数据成功，数据总数：" + attendances.Count.ToString());
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - 开始同步打卡数据到氚云...");
                        H3YunInteractor.CreateAttendances(attendances);
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - 同步打卡数据到氚云结束！");

                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - 开始获取钉钉请假数据...");
                        List<LeaveStatus> leaveStatus = DDInteractor.GetLeaveStatus(accessToken, userIds, startDate, endDate);
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - 获取钉钉请假数据结束，数据总数：" + leaveStatus.Count.ToString());
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - 开始同步请假数据到氚云...");
                        H3YunInteractor.SyncLeaveStatus(leaveStatus);
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - 同步请假数据到氚云结束！");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("同步考勤数据失败：" + ex.Message);
            }
        }

        static void SyncBasicPaymentInfo(DateTime now)
        {
            try
            {
                if (_lastSyncEmployeeInfoTime == DateTime.MinValue ||
                    _lastSyncEmployeeInfoTime.AddHours(4) < now)
                {
                    string accessToken = Common.GetAccessToken();

                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - 开始同步员工数据...");
                    List<OapiSmartworkHrmEmployeeListResponse.EmpFieldInfoVODomain> employeesInfo = DDInteractor.GetEmployeesInfo(accessToken);
                    H3YunInteractor.SyncBasicPaymentInfo(employeesInfo);
                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - 同步员工数据结束！");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("同步员工数据失败：" + ex.Message);
            }
        }
    }
}
