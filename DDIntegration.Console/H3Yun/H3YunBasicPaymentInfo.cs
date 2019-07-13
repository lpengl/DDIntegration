using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DingTalk.Api.Response.OapiSmartworkHrmEmployeeListResponse;

namespace DDIntegration
{
    public class H3YunBasicPaymentInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public string ObjectId { get; set; }

        /// <summary>
        /// 氚云UserId
        /// </summary>
        public string F0000001 { get; set; }

        /// <summary>
        /// 钉钉UserId
        /// </summary>
        public string F0000002 { get; set; }

        /// <summary>
        /// 职位
        /// </summary>
        public string F0000004 { get; set; }

        /// <summary>
        /// 转正日期
        /// </summary>
        public DateTime F0000013 { get; set; }

        /// <summary>
        /// 所在部门
        /// </summary>
        public string F0001366 { get; set; }

        public static H3YunBasicPaymentInfo ConvertFrom(EmpFieldInfoVODomain empInfo)
        {
            H3YunBasicPaymentInfo result = new H3YunBasicPaymentInfo();
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            result.F0000002 = empInfo.Userid;
            foreach(var field in empInfo.FieldList)
            {
                switch (field.FieldCode)
                {
                    case "sys00-dept":
                        break;
                    case "sys00-position":
                        result.F0000004 = field.Value;
                        break;
                    case "sys01-regularTime":
                        if(field.Value != null)
                        {
                            result.F0000013 = DateTime.Parse(field.Value);
                        }
                        else
                        {
                            result.F0000013 = origin;
                        }
                        break;
                }
            }

            return result;
        }
    }
}
