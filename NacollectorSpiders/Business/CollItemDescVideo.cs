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

namespace NacollectorSpiders.Business
{
    /// <summary>
    /// 详情页视频采集
    /// </summary>
    public class CollItemDescVideo : Spider
    {
        // 参数
        string PageUrl = "";

        // ts 池
        List<string> tsLinkPool = new List<string>();

        public override void BeginWork()
        {
            base.BeginWork();
            // 参数设定
            PageUrl = GetParm("PageUrl").Trim();

            LogInfo("开始获取数据：" + PageUrl);

            Encoding encoding = Encoding.GetEncoding("gb2312");
            var downloadPage = Utils.GetPageByUrl(PageUrl, null, null, encoding);
            if (downloadPage.StatusCode != System.Net.HttpStatusCode.OK) { throw new Exception("下载失败 [" + downloadPage.StatusCode + "] " + downloadPage.StatusDescription); }
            var pageContent = downloadPage.Html;
            LogSuccess("正在分析数据");
            var imgVedioID = new Regex("\"imgVedioID\":\"(.*?)\"").Match(pageContent).Groups[1].Value;
            Log("\n");
            LogInfo($"<a href=\"https://cloud.video.taobao.com/play/u/p/1/e/6/t/1/{imgVedioID}.mp4\" onclick=\"downloadFile($(this).attr('href'));return false;\">点击下载视频</a>");
        }
    }
}