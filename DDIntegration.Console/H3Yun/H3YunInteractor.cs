using DingTalk.Api.Response;
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

        public static void CreateAttendances(List<OapiAttendanceListResponse.RecordresultDomain> attendances)
        {
            if(attendances == null || attendances.Count == 0)
            {
                return;
            }

            string h3EngineCode = ConfigurationManager.AppSettings["H3EngineCode"];
            string h3Secret = ConfigurationManager.AppSettings["H3Secret"];

            List<H3YunAttendance> h3Attendance = new List<H3YunAttendance>();
            foreach(OapiAttendanceListResponse.RecordresultDomain record in attendances)
            {
                h3Attendance.Add(H3YunAttendance.ConvertFrom(record));
            }

            CreateAttendanceRequest request = new CreateAttendanceRequest();
            request.ActionName = "CreateBizObject";
            request.SchemaCode = "D001359Attendance";
            request.BizObjectArray = h3Attendance;
            request.IsSubmit = true;

            string postData = Newtonsoft.Json.JsonConvert.SerializeObject(request);

            HttpContent httpContent = new StringContent(postData);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            httpContent.Headers.ContentType.CharSet = "utf-8";
            httpContent.Headers.Add("EngineCode", h3EngineCode);
            httpContent.Headers.Add("EngineSecret", h3Secret);

            HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = httpClient.PostAsync(H3YunOapiUrl, httpContent).Result;

            if (response.IsSuccessStatusCode)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine("成功创建氚云打卡详情数据。");
            }
        }
    }
}
