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
        private static bool _firstSync = true;

        static void Main(string[] args)
        {
            while (true)
            {
                DateTime now = DateTime.Now;

                try
                {
                    if (_lastSyncAttendanceTime == DateTime.MinValue ||
                        _lastSyncAttendanceTime < now && _lastSyncAttendanceTime.Day != now.Day)
                    {
                        DateTime startSyncWorkDate = _firstSync ? H3YunInteractor.GetStartSyncWorkDate() : now.AddDays(-1);

                        Console.WriteLine("开始获取钉钉打卡数据...");
                        List<OapiAttendanceListResponse.RecordresultDomain> attendances = DDInteractor.GetAttendanceRecords(startSyncWorkDate, _firstSync);
                        Console.WriteLine("获取钉钉打卡数据结束！");

                        Console.WriteLine("同步打卡数据到氚云...");
                        H3YunInteractor.CreateAttendances(attendances);
                        Console.WriteLine("同步打卡数据结束！");

                        _firstSync = false;
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
                        Console.WriteLine("开始获取钉钉花名册数据...");
                        List<OapiSmartworkHrmEmployeeListResponse.EmpFieldInfoVODomain> employeesInfo = DDInteractor.GetEmployeesInfo();
                        Console.WriteLine("获取钉钉花名册数据结束！");

                        Console.WriteLine("同步花名册数据到氚云...");
                        //H3YunInteractor.SyncEmployeesInfo(employeesInfo);
                        Console.WriteLine("同步花名册数据结束！");
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
