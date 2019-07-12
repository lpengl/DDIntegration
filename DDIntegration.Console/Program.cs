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

                try
                {
                    if (_lastSyncAttendanceTime == DateTime.MinValue ||
                        _lastSyncAttendanceTime.AddMinutes(30) < now)
                    {
                        bool needSyncAttendance = H3YunInteractor.NeedSyncAttendanceData();
                        if (needSyncAttendance)
                        {
                            DateTime lastMonthNow = now.AddMonths(-1);
                            DateTime startSyncWorkDate = new DateTime(lastMonthNow.Year, lastMonthNow.Month, 1);
                            List<OapiAttendanceListResponse.RecordresultDomain> attendances = DDInteractor.GetAttendanceRecords(startSyncWorkDate);
                            H3YunInteractor.CreateAttendances(attendances);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                try
                {
                    if(_lastSyncEmployeeInfoTime == DateTime.MinValue ||
                        _lastSyncEmployeeInfoTime.AddHours(4) < now)
                    {
                        List<OapiSmartworkHrmEmployeeListResponse.EmpFieldInfoVODomain> employeesInfo = DDInteractor.GetEmployeesInfo();
                        H3YunInteractor.SyncBasicPaymentInfo(employeesInfo);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                _lastSyncAttendanceTime = now;
                _lastSyncEmployeeInfoTime = now;
                Thread.Sleep(10 * 60 * 1000);
            }
        }
    }
}
