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
        private const string AppCode = "A0fd9ef65c4344aa5aa356d54e9f54569";
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
                H3YunRequest request = new H3YunRequest();
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

        public static void SyncBasicPaymentInfo(List<OapiSmartworkHrmEmployeeListResponse.EmpFieldInfoVODomain> employees)
        {
            if(employees == null || employees.Count == 0)
            {
                return;
            }

            Dictionary<string, string> userIdPair = GetUserIdPair();
            List<H3YunBasicPaymentInfo> existingPayments = GetExistingBasicPaymentInfo();

            List<H3YunBasicPaymentInfo> paymentsInfo = new List<H3YunBasicPaymentInfo>();
            H3YunBasicPaymentInfo payment;
            foreach (OapiSmartworkHrmEmployeeListResponse.EmpFieldInfoVODomain emp in employees)
            {
                payment = H3YunBasicPaymentInfo.ConvertFrom(emp);
                if (userIdPair.ContainsKey(payment.F0000002))
                {
                    payment.F0000001 = userIdPair[payment.F0000002];
                }
                paymentsInfo.Add(payment);
            }

            int size = 200;

            for (int offset = 0; offset < employees.Count; offset = offset + size)
            {
                int takeCount = size;
                if (offset + takeCount > employees.Count)
                {
                    takeCount = employees.Count - offset;
                }
                SyncBasicPaymentInfoCore(paymentsInfo.GetRange(offset, takeCount), existingPayments);
            }
        }

        private static void SyncBasicPaymentInfoCore(
            List<H3YunBasicPaymentInfo> payments, 
            List<H3YunBasicPaymentInfo> existingPayments)
        {
            List<H3YunBasicPaymentInfo> paymentToCreate = new List<H3YunBasicPaymentInfo>();
            List<H3YunBasicPaymentInfo> paymentToUpdate = new List<H3YunBasicPaymentInfo>();

            foreach(H3YunBasicPaymentInfo item in payments)
            {
                if(existingPayments.Exists(p => p.F0000002 == item.F0000002))
                {
                    paymentToUpdate.Add(item);
                }
                else
                {
                    paymentToCreate.Add(item);
                }
            }

            CreateBasicPaymentInfo(paymentToCreate);
            UpdateBasicPaymentInfo(paymentToUpdate);
        }

        private static void CreateBasicPaymentInfo(List<H3YunBasicPaymentInfo> payments)
        {
            if(payments == null || payments.Count == 0)
            {
                return;
            }

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
                Console.WriteLine("插入基础薪资到氚云失败！");
            }
        }

        private static void UpdateBasicPaymentInfo(List<H3YunBasicPaymentInfo> payments)
        {
            if(payments == null || payments.Count == 0)
            {
                return;
            }

            UpdateBasicPaymentRequest updateRequest = new UpdateBasicPaymentRequest();
            updateRequest.ActionName = "UpdateBizObject";
            updateRequest.SchemaCode = SchemaCode_BasicPayment;

            foreach(H3YunBasicPaymentInfo item in payments)
            {
                try
                {

                }
                catch
                {

                }
            }
        }

        private static Dictionary<string, string> GetUserIdPair()
        {
            Dictionary<string, string> userIdPair = new Dictionary<string, string>();

            Dictionary<string, object> dicParams = new Dictionary<string, object>();
            dicParams.Add("ActionName", "OnInvoke");
            dicParams.Add("Controller", "H3InfoController"); 
            dicParams.Add("AppCode", AppCode);

            string postData = JsonConvert.SerializeObject(dicParams);

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
                    GetUserInfoResponse userInfoResponse = JsonConvert.DeserializeObject<GetUserInfoResponse>(result);
                    if(userInfoResponse != null && userInfoResponse.ReturnData  != null)
                    {
                        string pair = userInfoResponse.ReturnData["UserIdPair"];
                        if (!string.IsNullOrEmpty(pair))
                        {
                            userIdPair = JsonConvert.DeserializeObject<Dictionary<string, string>>(pair);
                            List<string> keys = userIdPair.Keys.ToList<string>();
                            foreach(string key in keys)
                            {
                                if (!string.IsNullOrEmpty(userIdPair[key]))
                                {
                                    if(userIdPair[key].IndexOf('.') > 0)
                                    {
                                        userIdPair[key] = userIdPair[key].Split('.')[0];
                                    }
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                Console.WriteLine("获取用户信息失败！");
            }

            return userIdPair;
        }

        private static List<H3YunBasicPaymentInfo> GetExistingBasicPaymentInfo()
        {
            try
            {
                H3YunRequest request = new H3YunRequest();
                request.ActionName = "LoadBizObjects";
                request.SchemaCode = SchemaCode_BasicPayment;
                request.Filter = "{\"FromRowNum\":0,\"ToRowNum\":500,\"RequireCount\":true,\"SortByCollection\":\"[]\",\"ReturnItems\":[],\"Matcher\":{\"Type\":\"And\",\"Matchers\":[]}}";
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
                        GetBasicPaymentInfoResponse payments = JsonConvert.DeserializeObject<GetBasicPaymentInfoResponse>(result);
                        if(payments != null && payments.ReturnData != null)
                        {
                            return payments.ReturnData.BizObjectArray;
                        }
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
    }
}
