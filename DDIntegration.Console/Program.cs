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
            bool firstSync = true;
            while (true)
            {
                DateTime now = DateTime.Now;

                try
                {
                    if (_lastSyncAttendanceTime == DateTime.MinValue ||
                        _lastSyncAttendanceTime < now && _lastSyncAttendanceTime.Day != now.Day)
                    {
                        DateTime startSyncWorkDate = firstSync ? H3YunInteractor.GetStartSyncWorkDate() : now.AddDays(-1);

                        List<OapiAttendanceListResponse.RecordresultDomain> attendances = DDInteractor.GetAttendanceRecords(startSyncWorkDate, firstSync);

                        H3YunInteractor.CreateAttendances(attendances);

                        firstSync = false;
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
