﻿using CsQuery;
using Nacollector.Browser;
using Nacollector.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        List<string> errorSeller = new List<string>(); // 未邀请成功的卖家
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

            int totalPage = DeleteEndPage - DeleteBeginPage + 1;
            for (int i = DeleteBeginPage; i <= DeleteEndPage; i++)
            {
                Log($"&gt;&gt; 准备撤回第 {i} 页所有卖家，共 {totalPage} 页，还剩 {totalPage - i} 页");
                WorkOnPage(i);
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
            string listPageHtml = ReqByUrl(listPageUrl, false, new Dictionary<string, string> { { "pageNo", page.ToString() } }, Encoding.GetEncoding("gb2312"));
            LogSuccess($"{listPageUrl} 下载完毕");
            // 获取 _tb_token_
            string _tb_token_ = new Regex("(?smi)name=(?:\"|')_tb_token_(?:\"|') type=(?:\"|')hidden(?:\"|') value=(?:\"|')(.*?)(?:\"|')").Match(listPageHtml).Groups[1].Value;
            if (string.IsNullOrEmpty(_tb_token_)) { throw new Exception("_tb_token_ 获取失败"); }
            LogSuccess($"成功获取到 _tb_token_ = {_tb_token_}");
            // 获取列表数据
            JObject listJson;
            try
            {
                string listJsonStr = new Regex("(?smi)(?:oRate = {)(.*?)(?:};)").Match(listPageHtml).Groups[1].Value;
                listJson = JObject.Parse("{" + listJsonStr.Trim() + "}");
            }
            catch { throw new Exception("解析 列表数据 JSON 失败"); }
            LogSuccess($"成功获取 {listJson.Count} 个 卖家 userIdNum");
            var pageDom = CQ.CreateDocument(listPageHtml);
            // 开始执行撤回
            foreach (var item in listJson)
            {
                if (pageDom["[data-uid=\""+ item.Key + "\"] .J_Status"].Text().Trim() != "已撤回")
                {
                    // 明天继续
                }
                else
                {
                    Log($"自动忽略已撤回卖家 {item.Key}");
                }
            }
        }

        public string ReqByUrl(string url, bool isAjax = false, Dictionary<string, string>postData = null, Encoding encoding = null)
        {
            var headers = new Dictionary<string, string> { };
            headers.Add("cookie", cookieStr);
            if (isAjax) headers.Add("x-requested-with", "XMLHttpRequest");
            HttpResult req = Utils.GetPageByUrl(url, headers, postData, encoding);
            if (req.StatusCode != System.Net.HttpStatusCode.OK) { throw new Exception($"{req.ResponseUri.ToString()} 请求失败 [{req.StatusCode}] {req.StatusDescription}"); }
            return req.Html;
        }
    }
}