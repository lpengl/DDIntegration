using DingTalk.Api.Response;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DDIntegration
{
    class Program
    {
        private static DateTime _lastSyncAttendanceTime = DateTime.MinValue;
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

                        List<OapiAttendanceListResponse.RecordresultDomain> attendances = DDInteractor.GetAttendanceRecords(startSyncWorkDate);
                        H3YunInteractor.CreateAttendances(attendances);

                        _firstSync = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                _lastSyncAttendanceTime = now;
                Thread.Sleep(60 * 1000);
            }
        }
    }
}
