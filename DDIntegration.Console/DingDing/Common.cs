using DingTalk.Api;
using DingTalk.Api.Request;
using DingTalk.Api.Response;
using System;
using System.Configuration;

namespace DDIntegration
{
    class Common
    {
        private const string GetTokenUrl = "https://oapi.dingtalk.com/gettoken";

        public static string GetAccessToken()
        {
            DefaultDingTalkClient client = new DefaultDingTalkClient(GetTokenUrl);
            OapiGettokenRequest request = new OapiGettokenRequest();
            request.Appkey = ConfigurationManager.AppSettings["Appkey"];
            request.Appsecret = ConfigurationManager.AppSettings["Appsecret"];
            request.SetHttpMethod("GET");
            OapiGettokenResponse response = client.Execute(request);
            if (response.Errcode != 0)
            {
                throw new Exception("获取AccessToken失败，错误信息: " + response.Errmsg);
            }

            string accessToken = response.AccessToken;
            return accessToken;
        }
    }
}
