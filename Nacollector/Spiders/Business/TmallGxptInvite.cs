using Nacollector.Browser;
using Nacollector.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nacollector.Spiders.Business
{
    /// <summary>
    /// 天猫供销平台分销商一键邀请
    /// </summary>
    public class TmallGxptInvite : Spider
    {
        // 参数
        string SellerId = "";
        string[] SellerIdArr = null;

        string cookieStr = null;
        string _tb_token_ = null;

        public override void BeginWork()
        {
            base.BeginWork();
            // 参数设定
            SellerId = GetParm("SellerId").Trim();
            SellerIdArr = SellerId.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (SellerIdArr.Length <= 0) { throw new Exception("卖家ID不能一个都没有啊"); }
            // 获取 Cookie
            var browserCookieGetter = new CrBrowserCookieGetter("登录天猫供销平台")
            {
                StartUrl = "https://qudao.gongxiao.tmall.com/supplier/user/invitation_list.htm",
                EndUrlRegPattern = @"^(https|http)://qudao\.gongxiao\.tmall\.com/supplier/user/invitation_list\.htm"
            };
            browserCookieGetter.UseAutoCompleteInput(@"^https://login\.taobao\.com/member/login\.jhtml", new List<string>() { "#TPL_username_1", "#TPL_password_1" });
            browserCookieGetter.BeginWork();
            // ... Show Dialog Working
            cookieStr = browserCookieGetter.GetCookieStr();
            if (string.IsNullOrEmpty(cookieStr)) { throw new Exception("Cookie 获取未成功"); }
            // 获取 _tb_token_
            var homePageUrl = "https://qudao.gongxiao.tmall.com/supplier/user/invitation_list.htm";
            string homePageHtml = ReqByUrl(homePageUrl);
            LogSuccess($"{homePageUrl} 下载完毕");
            _tb_token_ = new Regex("(?smi)name=(?:\"|')_tb_token_(?:\"|') type=(?:\"|')hidden(?:\"|') value=(?:\"|')(.*?)(?:\"|')").Match(homePageHtml).Groups[1].Value;
            if (string.IsNullOrEmpty(_tb_token_)) { throw new Exception("_tb_token_ 获取失败"); }
            LogSuccess($"成功获取到一个 _tb_token_ = {_tb_token_}");
            Log("\n");
            // 执行邀请操作
            foreach (var entry in SellerIdArr)
            {
                string id = entry.ToString();
                InviteSellerOnce(id);
            }
        }

        /// <summary>
        /// 邀请卖家一次
        /// </summary>
        /// <param name="sellerId"></param>
        private void InviteSellerOnce(string sellerId)
        {
            if (string.IsNullOrEmpty(sellerId)) return;
            LogInfo($"开始邀请 {sellerId}");
            // 确认您要邀请的分销商信息 对话框数据获取
            var dialogUrl = "https://" + $"qudao.gongxiao.tmall.com/supplier/json/invite_result.htm?action=user/invitation_action&event_submit_do_search=t&_input_charset=utf-8&_tb_token_={Uri.EscapeDataString(_tb_token_)}&userNick={Uri.EscapeDataString(sellerId)}"; // vs 字符串一包含 https:// 变色遮盖了符号高亮色，That's why... # 废话一大堆
            Log($"请求对话框：{dialogUrl}");
            string dialogHtml = ReqByUrl($"{dialogUrl}", isAjax: true);
            string userIdNum = new Regex("(?smi)type=\"hidden\" value=\"(.*?)\" name=\"userIdNum\"").Match(dialogHtml).Groups[1].Value;
            if (string.IsNullOrEmpty(userIdNum)) { throw new Exception($"卖家ID为 {sellerId} 的 userIdNum 获取失败"); }
            Log($"成功获取 {sellerId} 的 userIdNum = {userIdNum}");
            // 请求邀请
            var trade_type = 1; // 邀请合作模式：1 = 代销（供应商一件代发）|| 2 = 经销（分销商囤货销售）
            var inviteUrl = "https://" + $"qudao.gongxiao.tmall.com/supplier/json/invite_result_json.htm?action=user/invitation_action&event_submit_do_invite=t&_input_charset=utf-8&_tb_token_={Uri.EscapeDataString(_tb_token_)}&userIdNum={Uri.EscapeDataString(userIdNum)}&trade_type={trade_type}";
            Log($"请求邀请：{inviteUrl}");
            string inviteResult = ReqByUrl(inviteUrl, isAjax: true);
            // 解析 Json
            JObject resultJObject;
            try
            {
                resultJObject = JObject.Parse(inviteResult.Trim());
            }
            catch { throw new Exception("解析结果 JSON 失败 \n" + inviteResult.Trim().Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "")); }
            string result_Result = resultJObject["result"].ToString();
            string result_Message = resultJObject["message"].ToString();
            if (result_Result == "success")
                LogSuccess($"邀请 {sellerId} 响应 {result_Message}");
            else
                LogError($"邀请 {sellerId} 响应 {result_Result} - {result_Message}");
            Log("\n");
        }

        public string ReqByUrl(string url, bool isAjax=false)
        {
            var headers = new Dictionary<string, string> { };
            headers.Add("cookie", cookieStr);
            if (isAjax) headers.Add("x-requested-with", "XMLHttpRequest");
            HttpResult req = Utils.GetPageByUrl(url, headers);
            if (req.StatusCode != System.Net.HttpStatusCode.OK) { throw new Exception($"{req.ResponseUri.ToString()} 请求失败 [{req.StatusCode}] {req.StatusDescription}"); }
            return req.Html;
        }
    }
}