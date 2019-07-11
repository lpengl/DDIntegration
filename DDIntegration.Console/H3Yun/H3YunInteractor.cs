using DingTalk.Api.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DDIntegration
{
    class H3YunInteractor
    {
        private const string H3YunOapiUrl = "https://www.h3yun.com/OpenApi/Invoke";
        private const string SchemaCode_Attendance = "D001359Attendance";
        private const string SchemaCode_BasicPayment = "po9rfqcun250lovwkwjxkqvu6";
        private static readonly string H3EngineCode = ConfigurationManager.AppSettings["H3EngineCode"];
        private static readonly string H3Secret = ConfigurationManager.AppSettings["H3Secret"];
        private static readonly string StartSyncWorkDateStr = ConfigurationManager.AppSettings["StartSyncWorkDate"];

        /// <summary>
        /// 获取要读取考勤的开始时间，先从氚云获取创建时间最新的数据的工作日，
        /// 如果获取不到，则拿配置文件中的值，
        /// 如果都没有，则拿当前时间所处月份的第一天
        /// </summary>
        /// <returns></returns>
        public static DateTime GetStartSyncWorkDate()
        {
            GetAttendanceResponse attendance = GetLastestAttendance();
            if (attendance != null &&
                attendance.ReturnData != null && 
                attendance.ReturnData.BizObjectArray != null && 
                attendance.ReturnData.BizObjectArray.Count > 0)
            {
                DateTime startWorkDate = attendance.ReturnData.BizObjectArray[0].F0000005.AddDays(1);
                return new DateTime(startWorkDate.Year, startWorkDate.Month, startWorkDate.Day);
            }

            try
            {
                if (!string.IsNullOrEmpty(StartSyncWorkDateStr))
                {
                    return DateTime.Parse(StartSyncWorkDateStr);
                }
            }
            catch
            {
            }

            DateTime now = DateTime.Now;
            return new DateTime(now.Year, now.Month, 1);
        }

        private static GetAttendanceResponse GetLastestAttendance()
        {
            try
            {
                GetAttendanceRequest request = new GetAttendanceRequest();
                request.ActionName = "LoadBizObjects";
                request.SchemaCode = SchemaCode_Attendance;
                request.Filter = "{\"FromRowNum\":0,\"ToRowNum\":1,\"RequireCount\":true,\"SortByCollection\":\"[]\",\"ReturnItems\":[],\"Matcher\":{\"Type\":\"And\",\"Matchers\":[]}}";
                string postData = JsonConvert.SerializeObject(request);
                HttpContent httpContent = new StringContent(postData);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                httpContent.Headers.ContentType.CharSet = "utf-8";
                httpContent.Headers.Add("EngineCode", H3EngineCode);
                httpContent.Headers.Add("EngineSecret", H3Secret);

                HttpClient httpClient = new HttpClient();
                HttpResponseMessage response = httpClient.PostAsync(H3YunOapiUrl, httpContent).Result;
                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    try
                    {
                        return JsonConvert.DeserializeObject<GetAttendanceResponse>(result);
                    }
                    catch
                    {
                        Console.WriteLine(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public static void CreateAttendances(List<OapiAttendanceListResponse.RecordresultDomain> attendances)
        {
            if(attendances == null || attendances.Count == 0)
            {
                return;
            }

            attendances = attendances.OrderBy(a => a.WorkDate).ToList();
            List<H3YunAttendance> h3Attendances = new List<H3YunAttendance>();
            foreach(OapiAttendanceListResponse.RecordresultDomain record in attendances)
            {
                h3Attendances.Add(H3YunAttendance.ConvertFrom(record));
            }
            
            int size = 200;

            for(int offset = 0; offset < h3Attendances.Count; offset = offset + size)
            {
                int takeCount = size;
                if(offset + takeCount > h3Attendances.Count)
                {
                    takeCount = h3Attendances.Count - offset;
                }
                CreateAttendancesCore(h3Attendances.GetRange(offset, takeCount));
            }
        }

        private static void CreateAttendancesCore(List<H3YunAttendance> h3Attendances)
        {
            H3YunBulkCreateRequest request = new H3YunBulkCreateRequest();
            request.ActionName = "CreateBizObjects";
            request.SchemaCode = SchemaCode_Attendance;
            foreach (H3YunAttendance item in h3Attendances)
            {
                request.BizObjectArray.Add(JsonConvert.SerializeObject(item));
            }
            request.IsSubmit = true;

            string postData = JsonConvert.SerializeObject(request);

            HttpContent httpContent = new StringContent(postData);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            httpContent.Headers.ContentType.CharSet = "utf-8";
            httpContent.Headers.Add("EngineCode", H3EngineCode);
            httpContent.Headers.Add("EngineSecret", H3Secret);

            HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = httpClient.PostAsync(H3YunOapiUrl, httpContent).Result;

            if (response.IsSuccessStatusCode)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(result);
            }
            else
            {
                Console.WriteLine("插入打卡数据到氚云失败！");
            }
        }

        public static void SyncEmployeesInfo(List<OapiSmartworkHrmEmployeeListResponse.EmpFieldInfoVODomain> employees)
        {
            if(employees == null || employees.Count == 0)
            {
                return;
            }

            List<H3YunBasicPaymentInfo> paymentsInfo = new List<H3YunBasicPaymentInfo>();
            foreach (OapiSmartworkHrmEmployeeListResponse.EmpFieldInfoVODomain emp in employees)
            {
                paymentsInfo.Add(H3YunBasicPaymentInfo.ConvertFrom(emp));
            }

            int size = 200;

            for (int offset = 0; offset < employees.Count; offset = offset + size)
            {
                int takeCount = size;
                if (offset + takeCount > employees.Count)
                {
                    takeCount = employees.Count - offset;
                }
                SyncEmpolyeesInfoCore(paymentsInfo.GetRange(offset, takeCount));
            }
        }

        private static void SyncEmpolyeesInfoCore(List<H3YunBasicPaymentInfo> payments)
        {
            H3YunBulkCreateRequest request = new H3YunBulkCreateRequest();
            request.ActionName = "CreateBizObjects";
            request.SchemaCode = SchemaCode_BasicPayment;
            foreach (H3YunBasicPaymentInfo item in payments)
            {
                request.BizObjectArray.Add(JsonConvert.SerializeObject(item));
            }
            request.IsSubmit = true;

            string postData = JsonConvert.SerializeObject(request);

            HttpContent httpContent = new StringContent(postData);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            httpContent.Headers.ContentType.CharSet = "utf-8";
            httpContent.Headers.Add("EngineCode", H3EngineCode);
            httpContent.Headers.Add("EngineSecret", H3Secret);

            HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = httpClient.PostAsync(H3YunOapiUrl, httpContent).Result;

            if (response.IsSuccessStatusCode)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(result);
            }
            else
            {
                Console.WriteLine("插入打卡数据到氚云失败！");
            }
        }
    }
}
