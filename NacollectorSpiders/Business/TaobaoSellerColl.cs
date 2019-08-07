using CsQuery;
using NacollectorUtils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using NacollectorSpiders.Lib;

namespace NacollectorSpiders.Business
{
    /// <summary>
    /// 淘宝店铺搜索卖家ID名采集
    /// </summary>
    [SpiderRegister(Label = "淘宝店铺搜索卖家ID名采集")]
    public class TaobaoSellerColl : Spider
    {
        // 参数
        [FormTextInput(Label = "店铺搜索页链接", Type = "textInput", Parms = "'', InputValidators.isUrl")]
        public string PageUrl;

        [FormTextInput(Label = "采集开始页码", Type = "numberInput", Parms = "1, 1")]
        public int CollBeginPage;

        [FormTextInput(Label = "采集结束页码", Type = "numberInput", Parms = "undefined, 1")]
        public int CollEndPage;
        
        [FormTextInput(Label = "忽略天猫卖家", Type = "selectInput", Parms = @"{
          'on': '开启',
          'off': '关闭'
        }")]
        public string IgnoreTmall;

        private bool _IgnoreTmall;

        // 卖家名池
        private List<string> sellerNames = new List<string>();

        public override void BeginWork()
        {
            base.BeginWork();

            // 参数设定
            if (CollBeginPage <= 0 || CollEndPage <= 0 || CollBeginPage > CollEndPage) throw new Exception("老铁，你输入的参数是什么鬼？");
            _IgnoreTmall = IgnoreTmall.Trim().ToLower() == "on" ? true : false;

            for (int i = CollBeginPage; i <= CollEndPage; i++)
            {
                WorkOnPage(i); 
            }

            string txtFilePath = Path.Combine(GetTempDirPath("ForUserSave"), $"TaobaoSellerColl_{DateTime.Now.ToString("yyyyMMddhhmmss")}.txt");
            string txtFileStr = "";
            foreach (var item in sellerNames) txtFileStr += $"{item}{Environment.NewLine}";
            txtFileStr = txtFileStr.Trim();
            File.WriteAllText(txtFilePath, txtFileStr);
            Log("\n");
            LogInfo($"<a href=\"{txtFilePath}\" onclick=\"saveLocalFile($(this).attr('href'));return false;\">点击保存卖家ID名文本文件</a>");
        }
        
        /// <summary>
        /// 收集指定某一页的全部卖家名
        /// </summary>
        /// <param name="page"></param>
        private void WorkOnPage(int page)
        {
            Log("\n");
            Log($"&gt;&gt; 开始采集第 {page} 页卖家名");
            string reqUrl = GetHandledUrl(page);
            if (string.IsNullOrEmpty(reqUrl)) { throw new Exception("请求地址有误"); }
            string refererUrl = (page > 1) ? (GetHandledUrl(page - 1)) : "https://www.taobao.com";
            Log($"请求地址：{reqUrl}\n伪造来源：{refererUrl}");
            HttpResult downloadPage = Utils.GetPageByUrl(reqUrl, new Dictionary<string, string>{ { "referer", refererUrl } });
            if (downloadPage.StatusCode != System.Net.HttpStatusCode.OK) { throw new Exception("下载失败 [" + downloadPage.StatusCode + "] " + downloadPage.StatusDescription); }
            string pageContent = downloadPage.Html;
            LogSuccess("下载完毕");
            JObject jsonConf;
            try
            {
                string jsonStr = new Regex("(?smi)g_page_config = {(.*?)};").Match(pageContent).Groups[1].Value.Trim();
                jsonConf = JObject.Parse("{"+jsonStr+"}");
            }
            catch { throw new Exception("解析 JSON 失败"); }
            string itemsJsonStr = null;
            try
            {
                itemsJsonStr = jsonConf["mods"]["shoplist"]["data"]["shopItems"].ToString();
            }
            catch { throw new Exception("内容为空"); }
            JArray items = JArray.Parse(itemsJsonStr);
            int addedCount = 0;
            foreach (var item in items)
            {
                if (_IgnoreTmall && item["isTmall"].ToString().Trim().ToLower() == "true")
                    continue;

                var seller = item["nick"].ToString().Trim();
                if (!sellerNames.Contains(seller))
                    sellerNames.Add(seller);
                addedCount++;
            }
            LogSuccess($"采集了 {addedCount} 个卖家ID");
        }

        /// <summary>
        /// 根据页码生成 URL
        /// </summary>
        /// <param name="page"></param>
        private string GetHandledUrl(int page)
        {
            if (page <= 0)
                page = 0;
            else
                page = page * 20 - 20;
            
            if (string.IsNullOrEmpty(PageUrl)) return null;
            if (PageUrl.IndexOf("shopsearch.taobao.com") <= -1) return null;
            string pattern = @"(?<=&s=)\d+(?=.*?)";
            if (Regex.IsMatch(PageUrl, pattern))
                return Regex.Replace(PageUrl, pattern, page.ToString());
            else
                return PageUrl + "&s=" + page;
        }
    }
}