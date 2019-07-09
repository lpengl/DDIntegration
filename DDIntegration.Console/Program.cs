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
                    string accessToken = Common.GetAccessToken();
                    List<string> userIds = TongXunLu.GetAllUsers(accessToken);
                    KaoQin.GetAttendanceRecords(accessToken, userIds);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                Thread.Sleep(60 * 1000);
            }
        }
    }
}
