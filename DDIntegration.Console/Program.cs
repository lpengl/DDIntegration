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
                DateTime startSyncWorkDate;
                if (_firstSync)
                {
                    startSyncWorkDate = H3YunInteractor.GetStartSyncWorkDate();
                }
                else
                {
                    startSyncWorkDate = now.AddDays(-1);
                }

                try
                {
                    if (_lastSyncAttendanceTime == DateTime.MinValue ||
                        now > _lastSyncAttendanceTime && now.Day != _lastSyncAttendanceTime.Day)
                    {
                        List<OapiAttendanceListResponse.RecordresultDomain> attendances = DDInteractor.GetAttendanceRecords(startSyncWorkDate);
                        H3YunInteractor.CreateAttendances(attendances);
                        _firstSync = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Thread.Sleep(60 * 60 * 1000);
                    continue;
                }

                _lastSyncAttendanceTime = now;
                Thread.Sleep(60 * 1000);
            }
        }
    }
}
