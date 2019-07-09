﻿using DingTalk.Api;
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
            Console.WriteLine("正在获取AccessToken...");
            DefaultDingTalkClient client = new DefaultDingTalkClient(GetTokenUrl);
            OapiGettokenRequest request = new OapiGettokenRequest();
            request.Appkey = ConfigurationManager.AppSettings["Appkey"];
            request.Appsecret = ConfigurationManager.AppSettings["Appsecret"];
            request.SetHttpMethod("GET");
            OapiGettokenResponse response = client.Execute(request);
            string accessToken = response.AccessToken;
            Console.WriteLine(string.Format("获取AccessToken成功，Token:{0}。", accessToken));
            return accessToken;
        }
    }
}
