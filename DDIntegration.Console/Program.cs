using DingTalk.Api.Response;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DDIntegration
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    List<OapiAttendanceListResponse.RecordresultDomain> attendances = DDInteractor.GetAttendanceRecords();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Thread.Sleep(60 * 60 * 1000);
                    continue;
                }

                Thread.Sleep(60 * 1000);
            }
        }
    }
}
