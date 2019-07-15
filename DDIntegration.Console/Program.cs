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
                        Console.WriteLine("开始同步打卡数据...");
                        DateTime lastMonthNow = now.AddMonths(-1);
                        DateTime startSyncWorkDate = new DateTime(lastMonthNow.Year, lastMonthNow.Month, 1);
                        List<OapiAttendanceListResponse.RecordresultDomain> attendances = DDInteractor.GetAttendanceRecords(startSyncWorkDate);
                        H3YunInteractor.CreateAttendances(attendances);
                        Console.WriteLine("同步打卡数据结束.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void SyncBasicPaymentInfo(DateTime now)
        {
            try
            {
                if (_lastSyncEmployeeInfoTime == DateTime.MinValue ||
                    _lastSyncEmployeeInfoTime.AddHours(4) < now)
                {
                    Console.WriteLine("开始同步员工数据...");
                    List<OapiSmartworkHrmEmployeeListResponse.EmpFieldInfoVODomain> employeesInfo = DDInteractor.GetEmployeesInfo();
                    H3YunInteractor.SyncBasicPaymentInfo(employeesInfo);
                    Console.WriteLine("同步员工数据结束.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
