using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDIntegration
{
    public class ShouHuoXinXi : ShiChuangResultBase
    {
        public List<TransitInfo> transitInfo { get; set; }
    }

    public class TransitInfo
    {
        public string re_receive_code { get; set; }//收货编号
        public string re_goods_code { get; set; }//货物编号
        public string re_old_code { get; set; }//流水号           
        public string re_datetime { get; set; }//收货时间
        public string re_goods_name { get; set; }//货物名称
        public string re_goods_count { get; set; }//件数
        public string re_receive_operator { get; set; }//操作人
        public string re_receive_man { get; set; }//收货人
        public string re_receive_tel { get; set; }//收货人电话
        public string re_send_man { get; set; }//发货人
        public string re_send_tel { get; set; }//发货人电话   
        public string re_bankno { get; set; }//银行卡号
        public string re_weight { get; set; }//重量
        public string re_volume { get; set; }//体积
        public string re_arrive_pay { get; set; }//提付
        public string re_payed_money { get; set; }//已付现付
        public string re_owe_pay { get; set; }//回单付
        public string re_month_pay { get; set; }//扣付 
        public string re_payed_qian { get; set; }//欠付
        public string re_mat_traffic { get; set; }//垫付
        public string re_ismat { get; set; }//暗垫
        public string re_songhuofei { get; set; }//参送费
        public string re_shf_settle { get; set; }//参送费已结
        public string re_transfer_money { get; set; }//参转费
        public string re_gets_goods { get; set; }//代收货款
        public string re_insure_account { get; set; }//保价金额
        public string re_insure_money { get; set; }//保价费              
        public string re_get_type { get; set; }//提货方式
        public string re_remark { get; set; }//备注
        public string re_salesman { get; set; }//业务员       
        public string re_ruku { get; set; }//入库员
        public string re_arrive_name { get; set; }//目的地
        public string re_targetgroup_code { get; set; }//到货机构code
        public string re_target_name { get; set; }//到货机构名称
        public string re_group_code { get; set; }//收货机构code
        public string re_group_name { get; set; }//收货机构名称 					 re_ms_sender_code//民生会员号
        public string re_ms_type { get; set; }//所属银行（默认值P072民生银行）
        public string re_picker { get; set; }//接货员
        public string re_idcard { get; set; }//身份证号
        public string re_wx_money { get; set; }//微信已付

        public H3YunShouHuoXinxi ConvertToShouHuoXinXi()
        {
            H3YunShouHuoXinxi result = new H3YunShouHuoXinxi();

            DateTime tempDate;
            int tempNum;
            double tempDouble;

            result.receivecode = this.re_receive_code;
            result.goodscode = this.re_goods_code;
            result.oldcode = this.re_old_code;
            result.receiveday = DateTime.TryParse(this.re_datetime, out tempDate) ? tempDate : new DateTime(1900, 1, 1);
            result.goodsname = this.re_goods_name;
            result.goodscount = int.TryParse(this.re_goods_count, out tempNum) ? tempNum : 0;
            result.receiveoperator = this.re_receive_operator;
            result.receiveman = this.re_receive_man;
            result.receivetel = this.re_receive_tel;
            result.sendman = this.re_send_man;
            result.sendtel = this.re_send_tel;
            result.bankno = this.re_bankno;
            result.weight = double.TryParse(this.re_weight, out tempDouble) ? tempDouble : 0;
            result.volume = double.TryParse(this.re_volume, out tempDouble) ? tempDouble : 0;

            return result;
        }
    }
}
