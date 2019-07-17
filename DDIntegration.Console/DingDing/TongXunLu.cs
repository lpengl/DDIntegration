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
    /// 通讯录
    /// </summary>
    class TongXunLu
    {
        private const string ListDepartmentUrl = "https://oapi.dingtalk.com/department/list";
        private const string GetDeptMemberUrl = "https://oapi.dingtalk.com/user/getDeptMember";

        private static List<OapiDepartmentListResponse.DepartmentDomain> GetDepartments(string accessToken)
        {
            DefaultDingTalkClient client = new DefaultDingTalkClient(ListDepartmentUrl);
            OapiDepartmentListRequest request = new OapiDepartmentListRequest();
            request.FetchChild = true;
            request.SetHttpMethod("GET");
            OapiDepartmentListResponse response = client.Execute(request, accessToken);
            if(response.Errcode != 0)
            {
                throw new Exception("获取部门列表失败，错误信息: " + response.Errmsg);
            }
            return response.Department;
        }

        public static List<string> GetAllUsers(string accessToken)
        {
            List<OapiDepartmentListResponse.DepartmentDomain> departments = GetDepartments(accessToken);

            if(departments == null)
            {
                return null;
            }

            List<string> userIds = new List<string>();

            foreach(var department in departments)
            {
                DefaultDingTalkClient client = new DefaultDingTalkClient(GetDeptMemberUrl);
                OapiUserGetDeptMemberRequest req = new OapiUserGetDeptMemberRequest();
                req.DeptId = department.Id.ToString();
                req.SetHttpMethod("GET");
                OapiUserGetDeptMemberResponse response = client.Execute(req, accessToken);
                if (response.Errcode != 0)
                {
                    throw new Exception("获取用户列表失败，错误信息: " + response.Errmsg);
                }
                if(response.UserIds != null)
                {
                    userIds.AddRange(response.UserIds);
                }
            }

            return userIds.Distinct().ToList();
        }
    }
}
