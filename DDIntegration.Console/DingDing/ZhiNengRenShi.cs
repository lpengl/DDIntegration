using DingTalk.Api;
using DingTalk.Api.Request;
using DingTalk.Api.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDIntegration
{
    /// <summary>
    /// 只能人事
    /// </summary>
    class ZhiNengRenShi
    {
        private const string ListEmployeeUrl = "https://oapi.dingtalk.com/topapi/smartwork/hrm/employee/list";

        public static List<OapiSmartworkHrmEmployeeListResponse.EmpFieldInfoVODomain> GetEmployeesInfo(string accessToken, List<string> userIds)
        {
            List<OapiSmartworkHrmEmployeeListResponse.EmpFieldInfoVODomain> result = new List<OapiSmartworkHrmEmployeeListResponse.EmpFieldInfoVODomain>();
            if(userIds == null || userIds.Count == 0)
            {
                return result;
            }

            DefaultDingTalkClient client = new DefaultDingTalkClient(ListEmployeeUrl);
            OapiSmartworkHrmEmployeeListRequest req = new OapiSmartworkHrmEmployeeListRequest();

            int size = 50;

            for(int offset = 0; offset < userIds.Count; offset = offset + size)
            {
                int takeUserCount = size;
                if(offset + takeUserCount > userIds.Count)
                {
                    takeUserCount = userIds.Count - offset;
                }
                req.UseridList = string.Join(",", userIds.GetRange(offset, takeUserCount));
                req.FieldFilterList = "sys00-name,sys00-dept,sys00-position,sys01-regularTime";
                OapiSmartworkHrmEmployeeListResponse rsp = client.Execute(req, accessToken);
                if(rsp.Errcode != 0)
                {
                    Console.WriteLine(rsp.Errmsg);
                }

                if(rsp.Result == null)
                {
                    continue;
                }

                result.AddRange(rsp.Result);
            }

            return result;
        }
    }
}
