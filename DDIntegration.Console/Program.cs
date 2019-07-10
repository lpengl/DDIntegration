using DingTalk.Api.Response;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DDIntegration
{
    class Program
    {
        private static DateTime _lastSyncAttendanceTime = DateTime.MinValue;

        static void Main(string[] args)
        {
            while (true)
            {
                DateTime now = DateTime.Now;

                if(_lastSyncAttendanceTime == DateTime.MinValue || 
                    now > _lastSyncAttendanceTime && now.Day != _lastSyncAttendanceTime.Day)
                {
                    try
                    {
                        List<OapiAttendanceListResponse.RecordresultDomain> attendances = DDInteractor.GetAttendanceRecords();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Thread.Sleep(60 * 60 * 1000);
                        continue;
                    }
                }

                _lastSyncAttendanceTime = now;
                Thread.Sleep(60 * 1000);
            }
        }
    }
}
