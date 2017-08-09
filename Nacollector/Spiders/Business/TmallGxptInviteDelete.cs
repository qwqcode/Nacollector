using CsQuery;
using Nacollector.Browser;
using Nacollector.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nacollector.Spiders.Business
{
    /// <summary>
    /// 天猫供销平台分销商一键撤回
    /// </summary>
    class TmallGxptInviteDelete : Spider
    {
        // 参数
        int DeleteBeginPage = 0;
        int DeleteEndPage = 0;

        string cookieStr = null;

        List<string> errorSeller = new List<string>(); // 未撤回成功的卖家
        int maxErrorThreshold = 5; // 最多错误阈值

        public override void BeginWork()
        {
            base.BeginWork();
            // 参数设定
            bool DeleteBeginPageIsInt = Int32.TryParse(GetParm("DeleteBeginPage"), out DeleteBeginPage);
            if (!DeleteBeginPageIsInt) throw new Exception("参数 DeleteBeginPage 不是数字");
            bool DeleteEndPageIsInt = Int32.TryParse(GetParm("DeleteEndPage"), out DeleteEndPage);
            if (!DeleteEndPageIsInt) throw new Exception("参数 DeleteEndPage 不是数字");
            if (DeleteBeginPage <= 0 || DeleteEndPage <= 0 || DeleteBeginPage > DeleteEndPage) throw new Exception("老铁，你输入的参数是什么鬼？");
            // 获取 Cookie
            var browserCookieGetter = new CrBrowserCookieGetter(startUrl: "https://qudao.gongxiao.tmall.com/supplier/user/invitation_list.htm", endUrlReg: @"^(https|http)://qudao\.gongxiao\.tmall\.com/supplier/user/invitation_list\.htm", caption: "登录天猫供销平台");
            browserCookieGetter.UseInputAutoComplete(@"^https://login\.taobao\.com/member/login\.jhtml", new List<string>() { "#TPL_username_1", "#TPL_password_1" });
            browserCookieGetter.BeginWork();
            // ... Show Dialog Working
            cookieStr = browserCookieGetter.GetCookieStr();
            if (string.IsNullOrEmpty(cookieStr)) { throw new Exception("Cookie 获取未成功"); }
            Log("\n");
            // 执行撤回操作
            
            int totalPage = DeleteEndPage - DeleteBeginPage + 1;
            for (int i = DeleteBeginPage; i <= DeleteEndPage; i++)
            {
                Log($"&gt;&gt; 准备撤回第 {i} 页所有卖家，共 {totalPage} 页，还剩 {totalPage - i} 页");
                try
                {
                    WorkOnPage(i);
                }
                catch (Exception e)
                {
                    if (e.Message == "_END_TASK_")
                        throw new Exception("撤回失败过多，任务中止执行");
                    else
                        LogError(e.Message);
                }
                Log("\n");
                
            }
        }

        /// <summary>
        /// 撤回指定某一页的所有卖家
        /// </summary>
        /// <param name="page"></param>
        public void WorkOnPage(int page)
        {
            // 下载列表页
            var listPageUrl = "https://qudao.gongxiao.tmall.com/supplier/user/invitation_list.htm";
            string listPageHtml = ReqByUrl(listPageUrl, false, new Dictionary<string, string> { { "pageNo", page.ToString() } });
            LogSuccess($"{listPageUrl} 下载完毕");
            // 获取 _tb_token_
            string _tb_token_ = new Regex("(?smi)name=(?:\"|')_tb_token_(?:\"|') type=(?:\"|')hidden(?:\"|') value=(?:\"|')(.*?)(?:\"|')").Match(listPageHtml).Groups[1].Value;
            if (string.IsNullOrEmpty(_tb_token_)) { throw new Exception("_tb_token_ 获取失败"); }
            LogSuccess($"已获取 _tb_token_ = {_tb_token_}");
            // 获取列表数据
            JObject listJson;
            try
            {
                string listJsonStr = new Regex("(?smi)(?:oRate = {)(.*?)(?:};)").Match(listPageHtml).Groups[1].Value;
                listJson = JObject.Parse("{" + listJsonStr.Trim() + "}");
            }
            catch { throw new Exception("解析 列表数据 JSON 失败"); }
            if (listJson.Count <= 0) { throw new Exception("页面邀请记录为空"); }
            Log($"已获取 {listJson.Count} 个 卖家 userIdNum");
            var pageDom = CQ.CreateDocument(listPageHtml);
            // 开始执行撤回
            var index = 0;
            foreach (var item in listJson)
            {
                if (maxErrorThreshold != 0 && errorSeller.Count >= maxErrorThreshold)
                {
                    if (MessageBox.Show($"撤回失败次数已满 {maxErrorThreshold} 次" + Environment.NewLine + "是否中止任务？", this.GetType().ToString(), MessageBoxButtons.YesNo) == DialogResult.Yes)
                        throw new Exception("_END_TASK_");
                    else
                        maxErrorThreshold = 0;
                }

                var sellerIdNum = item.Key;
                var sellerId = pageDom["[data-uid=\"" + sellerIdNum + "\"] > td:first-child > a"].Text().Trim(); // 卖家名
                if (pageDom["[data-uid=\""+ sellerIdNum + "\"] .J_Status"].Text().Trim() != "已撤回")
                {
                    try
                    {
                        DeleteSellerOnce(sellerId, sellerIdNum);
                    }
                    catch (Exception e)
                    {
                        LogError(e.Message);
                        errorSeller.Add(sellerId);
                    }
                }
                else
                {
                    LogInfo($"撤回 {sellerId} 已自动忽略已撤回的卖家");
                }
                index++;
            }
        }

        /// <summary>
        /// 删除卖家一次
        /// </summary>
        /// <param name="sellerId"></param>
        /// <param name="sellerIdNum"></param>
        public void DeleteSellerOnce(string sellerId, string sellerIdNum)
        {
            string deleteResult = ReqByUrl($"https://qudao.gongxiao.tmall.com/supplier/json/cancel_invitation_json.htm?action=user/invitation_action&event_submit_do_cancel=t&invitationId={sellerIdNum}&_input_charset=utf-8", true);
            // 解析 Json
            JObject resultJObject;
            try
            {
                resultJObject = JObject.Parse(deleteResult.Trim());
            }
            catch { throw new Exception("解析结果 JSON 失败 \n" + deleteResult.Trim().Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "")); }
            string result_Result = resultJObject["result"].ToString();
            string result_Message = resultJObject["message"].ToString();
            if (result_Result == "success")
            {
                LogSuccess($"撤回 {sellerId} 响应 {result_Message}");
            }
            else
            {
                throw new Exception($"撤回 {sellerId} 响应 {result_Result} - {result_Message}");
            }
        }

        public string ReqByUrl(string url, bool isAjax = false, Dictionary<string, string>postData = null)
        {
            var headers = new Dictionary<string, string> { };
            headers.Add("cookie", cookieStr);
            if (isAjax) headers.Add("x-requested-with", "XMLHttpRequest");
            HttpResult req = Utils.GetPageByUrl(url, headers, postData, Encoding.GetEncoding("gb2312"));
            if (req.StatusCode != System.Net.HttpStatusCode.OK) { throw new Exception($"{req.ResponseUri.ToString()} 请求失败 [{req.StatusCode}] {req.StatusDescription}"); }
            return req.Html;
        }
    }
}
