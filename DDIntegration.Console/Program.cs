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
            string opKey = string.Empty;
            Console.Write("输入操作类型（1：开始同步数据，2：清除氚云考勤数据）：");
            opKey = Console.ReadLine();
            if(opKey == "1")
            {
                while (true)
                {
                    DateTime now = DateTime.Now;

                    SyncAttendanceData(now);
                    SyncBasicPaymentInfo(now);
                    Console.WriteLine("同步数据完成！\n");

                    _lastSyncAttendanceTime = now;
                    _lastSyncEmployeeInfoTime = now;
                    Thread.Sleep(10 * 60 * 1000);
                }
            }
            else if(opKey == "2")
            {
                Console.Write("请输入需要清除的年月（如 2019-6）：");
                string dateStr = Console.ReadLine();
                if(dateStr.IndexOf("-") == -1)
                {
                    Console.WriteLine("格式错误！");
                }
                string[] date = dateStr.Split('-');
                Console.WriteLine("开始清除氚云考勤数据...");
                H3YunInteractor.RemoveAttendance(date[0], date[1]);
                Console.WriteLine("清除氚云结束，按回车键退出...");
            }

            Console.ReadLine();
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
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - 获取钉钉打卡数据成功!");

                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - 开始同步打卡数据到氚云...");
                        List<H3YunAttendance> h3yunAttendances = H3YunInteractor.CreateAttendances(attendances);
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - 同步打卡数据到氚云结束，数据总数：" + h3yunAttendances.Count.ToString());
                        Console.WriteLine();

                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - 开始获取钉钉请假数据...");
                        List<LeaveStatus> leaveStatus = DDInteractor.GetLeaveStatus(accessToken, userIds, startDate, endDate);
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - 获取钉钉请假数据结束，数据总数：" + leaveStatus.Count.ToString());
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - 开始同步请假数据到氚云...");
                        H3YunInteractor.SyncLeaveStatus(leaveStatus);
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - 同步请假数据到氚云结束！");
                        Console.WriteLine();

                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - 开始创建氚云人员当天考勤数据...");
                        H3YunInteractor.CreateSingleDayAttendance(h3yunAttendances, leaveStatus);
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - 创建氚云人员当天考勤数据结束！");
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "暂无需同步考勤数据！\n");
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
