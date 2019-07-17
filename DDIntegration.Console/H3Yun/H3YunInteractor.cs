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
        private const string SchemaCode_JieSuan = "D001359a415593cd1d742569b928cf71a00c590";
        private const string SchemaCode_QingJia = "D001359a5baeebc330743b09f449d32658f5f29";
        private const string SchemaCode_SingleDayAttendance = "D00135913ef89a50a824741a57df7038537a7ef";
        private static readonly string H3EngineCode = ConfigurationManager.AppSettings["H3EngineCode"];
        private static readonly string H3Secret = ConfigurationManager.AppSettings["H3Secret"];
        private static readonly string StartSyncWorkDateStr = ConfigurationManager.AppSettings["StartSyncWorkDate"];

        #region

        public static bool NeedSyncAttendanceData()
        {
            DateTime now = DateTime.Now;
            if (!string.IsNullOrEmpty(StartSyncWorkDateStr))
            {
                DateTime startSyncWorkDate = DateTime.Parse(StartSyncWorkDateStr);
                if(now < startSyncWorkDate)
                {
                    return false;
                }
            }
            
            if(now.Day < 3)
            {
                return false;
            }

            Dictionary<string, object> dicParams = new Dictionary<string, object>();
            dicParams.Add("ActionName", "OnInvoke");
            dicParams.Add("Controller", "H3InfoController");
            dicParams.Add("AppCode", AppCode);
            dicParams.Add("Command", "CheckNeedSyncAttendanceData");

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
                    H3YunCommonResponse commonResponse = JsonConvert.DeserializeObject<H3YunCommonResponse>(result);
                    if (commonResponse != null && commonResponse.ReturnData != null)
                    {
                        string needSyncAttendanceData = commonResponse.ReturnData["NeedSyncAttendanceData"];
                        if(needSyncAttendanceData == "true")
                        {
                            return true;
                        }
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("获取是否同步信息失败！" + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("获取是否同步信息失败！");
            }
            return false;
        }

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

        #endregion

        public static void RemoveAttendance(string year, string month)
        {
            Dictionary<string, object> dicParams = new Dictionary<string, object>();
            dicParams.Add("ActionName", "OnInvoke");
            dicParams.Add("Controller", "H3InfoController");
            dicParams.Add("AppCode", AppCode);
            dicParams.Add("Command", "RemoveAttendances");
            dicParams.Add("TargetYear", year);
            dicParams.Add("TargetMonth", month);

            string postData = JsonConvert.SerializeObject(dicParams);

            HttpContent httpContent = new StringContent(postData);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            httpContent.Headers.ContentType.CharSet = "utf-8";
            httpContent.Headers.Add("EngineCode", H3EngineCode);
            httpContent.Headers.Add("EngineSecret", H3Secret);

            HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = httpClient.PostAsync(H3YunOapiUrl, httpContent).Result;
        }

        #region

        public static List<H3YunAttendance> CreateAttendances(List<OapiAttendanceListResponse.RecordresultDomain> attendances)
        {
            if(attendances == null || attendances.Count == 0)
            {
                return new List<H3YunAttendance>();
            }

            attendances = attendances.OrderBy(a => a.WorkDate).ToList();
            List<H3YunAttendance> h3Attendances = new List<H3YunAttendance>();
            foreach(OapiAttendanceListResponse.RecordresultDomain record in attendances)
            {
                h3Attendances.Add(H3YunAttendance.ConvertFrom(record));
            }

            h3Attendances = h3Attendances.Distinct(new H3YunAttendanceComparer()).ToList();

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

            return h3Attendances;
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
            }
            else
            {
                throw new Exception("插入打卡数据到氚云失败！");
            }
        }

        #endregion

        public static void SyncLeaveStatus(List<LeaveStatus> leaveStatus)
        {
            if(leaveStatus == null || leaveStatus.Count == 0)
            {
                return;
            }

            List<H3YunLeaveStatus> h3LeaveStatus = new List<H3YunLeaveStatus>();
            foreach (LeaveStatus record in leaveStatus)
            {
                h3LeaveStatus.Add(H3YunLeaveStatus.ConvertFrom(record));
            }

            H3YunBulkCreateRequest request = new H3YunBulkCreateRequest();
            request.ActionName = "CreateBizObjects";
            request.SchemaCode = SchemaCode_QingJia;
            foreach (H3YunLeaveStatus item in h3LeaveStatus)
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
            }
            else
            {
                throw new Exception("插入打卡数据到氚云失败！");
            }
        }

        #region

        public static void CreateSingleDayAttendance(List<H3YunAttendance> attendances, List<LeaveStatus> leaveStatus)
        {
            if(attendances == null || attendances.Count == 0)
            {
                return;
            }

            if(leaveStatus == null)
            {
                leaveStatus = new List<LeaveStatus>();
            }

            Dictionary<string, List<string>> userIdPair = GetUserIdPair();
            var groupedAttendances = attendances.GroupBy(a => a.F0000006);
            foreach (var group in groupedAttendances)
            {
                string userId = string.Empty;
                if (userIdPair.ContainsKey(group.Key))
                {
                    userId = userIdPair[group.Key][0];
                }
                List<LeaveStatus> userLeaveStatus = leaveStatus.Where(l => l.userid == group.Key).ToList();
                CreateSingleDayAttendance(group, userLeaveStatus, userId);
            }
        }

        private static void CreateSingleDayAttendance(
            IGrouping<string, H3YunAttendance> group, 
            List<LeaveStatus> userLeaveStatus, 
            string userId)
        {
            List<H3YunSingleDayAttendance> singleDayAttendances = new List<H3YunSingleDayAttendance>();
            
            List<H3YunAttendance> attendances = group.ToList();
            var groupedAttendances = attendances.GroupBy(a => a.F0000005.ToString("yyyy-MM-dd"));
            foreach (var innerGroup in groupedAttendances)
            {
                H3YunSingleDayAttendance singleAttendance = H3YunSingleDayAttendance.ConvertFrom(innerGroup.ToList(), userLeaveStatus, userId);
                singleDayAttendances.Add(singleAttendance);
            }

            H3YunBulkCreateRequest request = new H3YunBulkCreateRequest();
            request.ActionName = "CreateBizObjects";
            request.SchemaCode = SchemaCode_SingleDayAttendance;
            foreach (H3YunSingleDayAttendance item in singleDayAttendances)
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
            }
            else
            {
                Console.WriteLine("插入人员当天考勤数据到氚云失败！");
            }
        }

        #endregion

        #region 

        public static void SyncBasicPaymentInfo(List<OapiSmartworkHrmEmployeeListResponse.EmpFieldInfoVODomain> employees)
        {
            if(employees == null || employees.Count == 0)
            {
                return;
            }

            Dictionary<string, List<string>> userIdPair = GetUserIdPair();
            List<H3YunBasicPaymentInfo> existingPayments = GetExistingBasicPaymentInfo();
            if(existingPayments == null)
            {
                existingPayments = new List<H3YunBasicPaymentInfo>();
            }

            List<H3YunBasicPaymentInfo> paymentsInfo = new List<H3YunBasicPaymentInfo>();
            H3YunBasicPaymentInfo payment;
            foreach (OapiSmartworkHrmEmployeeListResponse.EmpFieldInfoVODomain emp in employees)
            {
                payment = H3YunBasicPaymentInfo.ConvertFrom(emp);
                if (userIdPair.ContainsKey(payment.F0000002))
                {
                    payment.F0000001 = userIdPair[payment.F0000002][0];
                    payment.F0001366 = userIdPair[payment.F0000002][1];
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
                H3YunBasicPaymentInfo tempPayment = existingPayments.FirstOrDefault(p => p.F0000002 == item.F0000002);
                if(tempPayment == null || string.IsNullOrEmpty(tempPayment.ObjectId))
                {
                    paymentToCreate.Add(item);
                }
                else
                {
                    item.ObjectId = tempPayment.ObjectId;
                    paymentToUpdate.Add(item);
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
                //Console.WriteLine(result);
            }
            else
            {
                Console.WriteLine("插入基础薪资到氚云失败！");
            }
        }

        private static void UpdateBasicPaymentInfo(List<H3YunBasicPaymentInfo> payments)
        {
            if (!NeedUpdateBasicPaymentInfo())
            {
                return;
            }
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
                    updateRequest.BizObjectId = item.ObjectId;
                    updateRequest.BizObject = JsonConvert.SerializeObject(item);
                    string postData = JsonConvert.SerializeObject(updateRequest);

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
                        //Console.WriteLine(result);
                    }
                    else
                    {
                        Console.WriteLine("插入基础薪资到氚云失败！");
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static bool NeedUpdateBasicPaymentInfo()
        {
            try
            {
                DateTime now = DateTime.Now;
                string date = now.AddMonths(-1).ToString("yyyy-MM");
                H3YunRequest request = new H3YunRequest();
                request.ActionName = "LoadBizObjects";
                request.SchemaCode = SchemaCode_JieSuan;
                request.Filter = "{\"FromRowNum\":0,\"ToRowNum\":1,\"RequireCount\":true,\"SortByCollection\":\"[]\",\"ReturnItems\":[],\"Matcher\":\"{\\\"Type\\\":\\\"Item\\\",\\\"Name\\\":\\\"F0000001\\\",\\\"Value\\\":\\\"" + date + "\\\",\\\"Operator\\\":\\\"2\\\"}\"}";
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
                        GetJieSuanResponse jiesuanResponse = JsonConvert.DeserializeObject<GetJieSuanResponse>(result);
                        if(jiesuanResponse != null && jiesuanResponse.ReturnData != null && jiesuanResponse.ReturnData.BizObjectArray != null)
                        {
                            return jiesuanResponse.ReturnData.BizObjectArray.Count > 0;
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
            
            return false;
        }

        private static List<H3YunBasicPaymentInfo> GetExistingBasicPaymentInfo()
        {
            try
            {
                H3YunRequest request = new H3YunRequest();
                request.ActionName = "LoadBizObjects";
                request.SchemaCode = SchemaCode_BasicPayment;
                request.Filter = "{\"FromRowNum\":0,\"ToRowNum\":1000,\"RequireCount\":true,\"SortByCollection\":\"[]\",\"ReturnItems\":[],\"Matcher\":{\"Type\":\"And\",\"Matchers\":[]}}";
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

        #endregion

        private static Dictionary<string, List<string>> GetUserIdPair()
        {
            Dictionary<string, List<string>> userIdPair = new Dictionary<string, List<string>>();

            Dictionary<string, object> dicParams = new Dictionary<string, object>();
            dicParams.Add("ActionName", "OnInvoke");
            dicParams.Add("Controller", "H3InfoController"); 
            dicParams.Add("AppCode", AppCode);
            dicParams.Add("Command", "GetUserIdPair");

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
                    H3YunCommonResponse userInfoResponse = JsonConvert.DeserializeObject<H3YunCommonResponse>(result);
                    if(userInfoResponse != null && userInfoResponse.ReturnData  != null)
                    {
                        string pair = userInfoResponse.ReturnData["UserIdPair"];
                        if (string.IsNullOrEmpty(pair))
                        {
                            return userIdPair;
                        }

                        Dictionary<string, List<string>> returnData = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(pair);
                        if(returnData == null)
                        {
                            return userIdPair;
                        }

                        foreach(string key in returnData.Keys)
                        {
                            string innerKey = key.IndexOf('.') > 0 ? key.Split('.')[0] : key;
                            if (!userIdPair.ContainsKey(innerKey))
                            {
                                userIdPair[innerKey] = new List<string>();
                                if(returnData[key] != null)
                                {
                                    userIdPair[innerKey].AddRange(returnData[key]);
                                }
                            }
                        }
                        return userIdPair;

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

    }
}
