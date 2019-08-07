using CsQuery;
using NacollectorUtils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;
using NacollectorSpiders.Lib;
using System.Diagnostics;

namespace NacollectorSpiders.Business
{
    /// <summary>
    /// 商品详情页图片解析
    /// </summary>
    [SpiderRegister(Label = "商品详情页图片解析")]
    public class CollItemDescImg : Spider
    {
        // 参数
        [FormTextInput(Label = "详情页链接", Type = "textInput", Parms = "'', InputValidators.isUrl")]
        public string PageUrl; // 不要使用 new Uri()，因为会把 urlencode 的参数自动 decode

        [FormTextInput(Label = "链接类型", Type = "selectInput", Parms = @"{
          'Tmall': '天猫',
          'Taobao': '淘宝',
          'TaobaoMobile': '淘宝/天猫 手机网页',
          'Alibaba': '阿里巴巴',
          'Suning': '苏宁易购',
          'Gome': '国美在线'
        }")]
        public string PageType;

        [FormTextInput(Label = "图片类型", Type = "selectInput", Parms = @"{
          'Thumb': '主图',
          'Category': '分类图',
          'Desc': '详情图'
        }")]
        public string ImgType;

        [FormTextInput(Label = "采集模式", Type = "selectInput", Parms = @"{
          'collImgSrcUrl': '显示图片链接',
          'collDownloadImgSrc': '显示图片链接 并 下载打包保存'
        }")]
        public string CollType;

        // 页面内容
        private string pageContent;

        // CsQuery Dom
        private CQ pageDom;

        // 图片链接池
        private Dictionary<string, ArrayList> imgUrlPool = new Dictionary<string, ArrayList>();

        private readonly string MobileUA = "Mozilla/5.0 (iPhone; CPU iPhone OS 11_0 like Mac OS X) AppleWebKit/604.1.38 (KHTML, like Gecko) Version/11.0 Mobile/15A372 Safari/604.1";

        public override void BeginWork()
        {
            base.BeginWork();

            // 选择页面格式
            Dictionary<string, string> headers = new Dictionary<string, string> { };
            Encoding encoding = Encoding.GetEncoding("UTF-8");

            // 淘宝天猫手机端采集
            if (PageType.Equals("TaobaoMobile"))
            {
                // 直接下一步，不下载初始页
                NextStep();
                return;
            }

            // 阿里巴巴获取 Cookie 再采集
            if (PageType.Equals("Alibaba"))
            {
                // 获取 Cookie
                var cgSettings = new NacollectorUtils.Settings.CookieGetterSettings
                {
                    StartUrl = "https://login.1688.com/member/signin.htm?from=sm&Done=" + HttpUtility.UrlEncode(PageUrl),
                    EndUrlReg = @"^" + PageUrl.Substring(0, PageUrl.IndexOf("?")),
                    Caption = "登录 1688",

                };
                cgSettings.UseInputAutoComplete(@"^https://login\.1688.com/member/signin\.htm", new List<string>() { "#TPL_username_1", "#TPL_password_1" });
                // ... Show Dialog Working
                string alibabaCookieStr = CrBrowserCookieGetter(cgSettings);
                if (string.IsNullOrEmpty(alibabaCookieStr)) { throw new Exception("Cookie 获取未成功"); }

                encoding = Encoding.GetEncoding("gb2312");
                headers.Add("cookie", alibabaCookieStr);
            }

            // 手机端修改 UA
            if (PageType.EndsWith("Mobile"))
            {
                headers.Add("user-agent", MobileUA);
            }

            // 下载页面
            LogInfo("开始下载：" + PageUrl);

            var downloadPage = Utils.GetPageByUrl(PageUrl, headers, null, encoding);
            if (downloadPage.StatusCode != System.Net.HttpStatusCode.OK) { throw new Exception("下载失败 [" + downloadPage.StatusCode + "] " + downloadPage.StatusDescription); }
            pageContent = downloadPage.Html;
            LogSuccess("下载完毕");
            pageDom = CQ.CreateDocument(pageContent);

            NextStep();
        }

        /// <summary>
        /// BeginWork 后执行下一步
        /// </summary>
        private void NextStep()
        {
            // 调用指定方法
            this.GetType().GetMethod(PageType + ImgType, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(this, new object[] { });

            // 显示&采集
            AfterGetImgUrl();
        }
        
        #region 天猫
        private void TmallThumb()
        {
            pageDom["#J_UlThumb > li img"].Each((i, e) => {
                AddImgUrl("主图", e.GetAttribute("src").Replace("_60x60q90.jpg", ""));
            });
        }
        private void TmallCategory()
        {
            pageDom[".J_TSaleProp > li > a"].Each((i, e) => {
                string cssVal = e.Cq().Css("background");
                if (cssVal == null) return;
                string picSrcUrl = new Regex("(?smi)url\\((.*)\\)").Match(cssVal).Groups[1].Value;
                AddImgUrl("分类图", picSrcUrl.Replace("_40x40q90.jpg", ""));
            });
        }
        private void TmallDesc()
        {
            JObject jsonConf;
            try
            {
                string jsonStr = new Regex("(?smi)TShop.Setup\\((.*?)\\);").Match(pageContent).Groups[1].Value.Trim();
                jsonConf = JObject.Parse(jsonStr);
            }
            catch { throw new Exception("解析 JSON 失败"); }
            if (jsonConf["api"]["descUrl"] == null) { Log("详情内容请求URL无法正常获取"); return; }
            string descReqUrl = UrlSchemeFull(jsonConf["api"]["descUrl"].ToString());
            Log("\n");
            LogInfo("开始下载详情内容：" + descReqUrl);
            string descContent;
            try
            {
                descContent = Utils.GetPageByUrl(descReqUrl).Html; // 下载详情内容
                descContent = new Regex("(?smi)var desc='(.*?)';").Match(descContent).Groups[1].Value.Trim();
            }
            catch (Exception e) { throw new Exception("详情内容下载失败：" + e.Message); }
            LogSuccess("详情内容下载完毕");
            // Console.WriteLine("详情Html："+ descContent);
            var descDom = CQ.CreateDocument(descContent);
            descDom["img"].Each((i, e) => {
                AddImgUrl("详情图", e.GetAttribute("src"));
            });
        }
        #endregion


        #region 淘宝
        private void TaobaoThumb()
        {
            JArray json;
            try
            {
                string jsonStr = new Regex("(?smi)auctionImages    : \\[(.*?)\\]").Match(pageContent).Groups[1].Value.Trim();
                json = JArray.Parse("[" + jsonStr + "]");
            }
            catch { throw new Exception("解析 JSON 失败"); }
            foreach (JValue item in json)
                AddImgUrl("主图", item.Value.ToString());
        }

        private void TaobaoCategory()
        {
            pageDom[".J_TSaleProp > li > a"].Each((i, e) => {
                string cssVal = e.Cq().Css("background");
                if (cssVal == null) return;
                string picSrcUrl = new Regex("(?smi)url\\((.*)\\)").Match(cssVal).Groups[1].Value;
                AddImgUrl("分类图", picSrcUrl.Replace("_30x30.jpg", ""));
            });
        }

        private void TaobaoDesc()
        {
            string descReqUrl = new Regex("(?smi)descUrl          : location.protocol===\\'http:\\' \\? \\'.*?\\' : \'(.*?)\'").Match(pageContent).Groups[1].Value.Trim();
            if (descReqUrl == null) { Log("详情内容请求URL无法正常获取"); return; }
            descReqUrl = UrlSchemeFull(descReqUrl);
            Log("\n");
            LogInfo("开始下载详情内容：" + descReqUrl);
            string descContent;
            try
            {
                descContent = Utils.GetPageByUrl(descReqUrl).Html;
                descContent = new Regex("(?smi)var desc='(.*?)';").Match(descContent).Groups[1].Value.Trim();
            }
            catch (Exception e) { throw new Exception("详情内容下载失败：" + e.Message); }
            LogSuccess("详情内容下载完毕");
            // Console.WriteLine("详情Html："+ descContent);
            var descDom = CQ.CreateDocument(descContent);
            descDom["img"].Each((i, e) => {
                string picSrcUrl = e.GetAttribute("src");
                AddImgUrl("详情图", picSrcUrl);
            });
        }
        #endregion

        #region 淘宝手机端
        private JObject TaobaoMobileGetInfoJson(bool isDesc = false)
        {
            string itemNumId = new Regex(@"(?smi)\??id=(\d+)&?").Match(PageUrl).Groups[1].Value.Trim();
            LogInfo($"itemNumId = \"{itemNumId}\"");
            string apiUrl;
            if (!isDesc)
                apiUrl = "https://h5api.m.taobao.com/h5/mtop.taobao.detail.getdetail/6.0/"
                    + "?jsv=2.4.11&appKey=12574478&api=mtop.taobao.detail.getdetail&v=6.0&ttid=2017%40htao_h5_1.0.0&type=jsonp&dataType=jsonp&callback=mtopjsonp1"
                    + $"&data=%7B\"exParams\"%3A\" % 7B % 5C\"countryCode%5C\" % 3A % 5C\"CN%5C\" % 7D\"%2C\"itemNumId\"%3A\"{itemNumId}\"%7D";
            else
                apiUrl = "https://h5api.m.taobao.com/h5/mtop.wdetail.getitemdescx/4.9/"
                    + "?jsv=2.4.11&appKey=12574478&api=mtop.wdetail.getItemDescx&v=4.9&type=jsonp&dataType=jsonp&callback=mtopjsonp2"
                    + $"&data=%7B\"item_num_id\"%3A\"{itemNumId}\"%7D"; // 这个 API 需要 sign 和 t 才能正常请求 md5(cookie['_m_h5_tk'] + "&" + new Date().getTime() + "&" + appKey + "&" + c.data)
            Log("\n");
            LogInfo("开始下载商品数据：" + apiUrl);
            string descContent;
            JObject jsonConf;
            try
            {
                descContent = Utils.GetPageByUrl(apiUrl, new Dictionary<string, string> { { "user-agent", MobileUA }, { "Referer", PageUrl } }).Html;
                descContent = new Regex(@"(?smi)^mtopjsonp\d?\((.*?)\)$").Match(descContent.Trim()).Groups[1].Value.Trim();
                if (descContent == "")
                    throw new Exception("商品数据为空");
                jsonConf = JObject.Parse(descContent);
            }
            catch (Exception e) { throw new Exception("商品数据下载失败：" + e.Message); }
            LogSuccess("商品数据下载完毕");
            return jsonConf;
        }

        private void TaobaoMobileThumb()
        {
            JObject infoJson = TaobaoMobileGetInfoJson();
            foreach(var item in infoJson["data"]["item"]["images"])
            {
                AddImgUrl("主图", item.ToString());
            }
        }

        private void TaobaoMobileCategory()
        {
            JObject infoJson = TaobaoMobileGetInfoJson();
            JObject innerInfoJson;
            try
            {
                string innerInfoJsonStr = infoJson["data"]["apiStack"][0]["value"].ToString().Trim();
                if (innerInfoJsonStr == "") throw new Exception("数据为空");
                innerInfoJson = JObject.Parse(innerInfoJsonStr);
            }
            catch (Exception e) { throw new Exception("商品分类数据读取失败：" + e.Message); }
            var categories = innerInfoJson["skuBase"]["props"].FirstOrDefault((item) => (string)item["name"] == "颜色分类");
            foreach (var item in categories["values"])
            {
                AddImgUrl("分类图", item["image"].ToString());
            }
        }

        private void TaobaoMobileDesc()
        {
            LogWarning("详情页暂时无法采集，新版将会更新，敬请期待");
            return;
            JObject infoJson = TaobaoMobileGetInfoJson(isDesc: true);
            foreach (var item in infoJson["data"]["images"])
            {
                AddImgUrl("详情图", item.ToString());
            }
        }
        #endregion


        #region 阿里巴巴
        private void AlibabaThumb()
        {
            pageDom["#dt-tab li.tab-trigger"].Each((i, e) => {
                string picSrcUrl = e.GetAttribute("data-imgs");
                picSrcUrl = new Regex("(?smi)\"original\":\"(.*?)\"").Match(picSrcUrl).Groups[1].Value.Trim();
                AddImgUrl("主图", picSrcUrl);
            });
        }

        private void AlibabaCategory()
        {
            pageDom[".list-leading .unit-detail-spec-operator"].Each((i, e) => {
                string picSrcUrl = e.GetAttribute("data-imgs");
                picSrcUrl = new Regex("(?smi)\"original\":\"(.*?)\"").Match(picSrcUrl).Groups[1].Value.Trim();
                AddImgUrl("分类图", picSrcUrl);
            });
        }

        private void AlibabaDesc()
        {
            string descReqUrl = pageDom[".desc-lazyload-container"].Attr("data-tfs-url");
            if (descReqUrl == null) { Log("详情内容请求URL无法正常获取"); return; }
            descReqUrl = UrlSchemeFull(descReqUrl);
            Log("\n");
            LogInfo("开始下载详情内容：" + descReqUrl);
            string descContent;
            try
            {
                descContent = Utils.GetPageByUrl(descReqUrl).Html;
                descContent = new Regex("(?smi)var offer_details={(.*?)};").Match(descContent).Groups[1].Value.Trim();
                JObject descContentJson = JObject.Parse("{" + descContent + "}");
                descContent = descContentJson["content"].ToString();
            }
            catch (Exception e) { throw new Exception("详情内容下载失败：" + e.Message); }
            LogSuccess("详情内容下载完毕");
            // Console.WriteLine("详情Html："+ descContent);
            var descDom = CQ.CreateDocument(descContent);
            descDom["img"].Each((i, e) => {
                AddImgUrl("详情图", e.GetAttribute("src"));
            });
        }
        #endregion
        

        #region 苏宁
        private void SuningThumb()
        {
            pageDom[".imgzoom-thumb-main ul li img"].Each((i, e) => {
                string picSrcUrl = e.GetAttribute("src-large");
                AddImgUrl("主图", picSrcUrl.Replace("_800x800", "").Replace("_800w_800h_4e", ""));
            });
        }

        private void SuningCategory()
        {
            pageDom[".tip-infor img"].Each((i, e) => {
                string picSrcUrl = e.GetAttribute("src");
                AddImgUrl("分类图", picSrcUrl.Replace("_60x60", "").Replace("_60w_60h_4e", ""));
            });
        }

        private void SuningDesc()
        {
            pageDom["#productDetail.pro-detail-pics img"].Each((i, e) => {
                string picSrcUrl = e.GetAttribute("src2");
                AddImgUrl("详情图", picSrcUrl);
            });
        }
        #endregion


        #region 国美
        private void GomeThumb()
        {
            pageDom[".magnifier .pic-list .pic-small ul li img.cur, .magnifier .pic-list .pic-small ul li img"].Each((i, e) => {
                string picSrcUrl = e.GetAttribute("rpic");
                AddImgUrl("主图", picSrcUrl.Replace("_800_pc", ""));
            });
        }

        private void GomeCategory()
        {
            pageDom[".prd-properties .yanse .prdRight .prdcol a img"].Each((i, e) => {
                string picSrcUrl = e.GetAttribute("gome-src");
                AddImgUrl("分类图", picSrcUrl.Replace("_60", ""));
            });
        }

        private void GomeDesc()
        {
            JObject jsonConf;
            try
            {
                string jsonConfStr = new Regex("(?smi)var prdInfo = {(.*?)};").Match(pageContent).Groups[1].Value.Trim();
                jsonConf = JObject.Parse("{" + jsonConfStr + "}");
            }
            catch { throw new Exception("解析 JSON 失败"); }
            if (jsonConf["htmlHref"] == null) { throw new Exception("详情内容请求URL无法正常获取"); }
            string descReqUrl = UrlSchemeFull(jsonConf["htmlHref"].ToString());
            Log("\n");
            LogInfo("开始下载详情内容：" + descReqUrl);
            string descContent;
            try
            {
                descContent = Utils.GetPageByUrl(descReqUrl).Html;
                descContent = new Regex("(?smi)\\(\"(.*?)\"\\)").Match(descContent).Groups[1].Value.Trim();
            }
            catch (Exception e) { throw new Exception("详情内容下载失败：" + e.Message); }
            LogSuccess("详情内容下载完毕");
            // Console.WriteLine("详情Html："+ descContent);
            var descDom = CQ.CreateDocument(descContent);
            descDom["img"].Each((i, e) => {
                AddImgUrl("详情图", e.GetAttribute("src"));
            });
        }
        #endregion

        /// <summary>
        /// 获取图片URL完毕之后执行
        /// </summary>
        private void AfterGetImgUrl()
        {
            Log("\n");
            if (imgUrlPool.Count == 0)
            {
                throw new Exception(
                    "诶？！！没有采集到任何的图片...\n" +
                    "Σ(っ °Д °;)っ   可能是 " + PageTypeTranslation(PageType) + " 页面结构更新了？！\n" +
                    "快通过 QQ:1149527164 或 <a target=\"_blank\" href=\"https://github.com/qwqcode/Nacollector/issues\">GitHub issue</a> 向我反馈");
            }
            string typeTmp = "";
            foreach (string imgType in imgUrlPool.Keys)
            {
                if (imgType != typeTmp)
                    Log(imgType + ": ");

                var imgIndex = 0;
                foreach (string imgSrc in imgUrlPool[imgType])
                {
                    var number = imgIndex + 1;
                    var imgSrcUrl = imgUrlPool[imgType][imgIndex];
                    Log($"[{number}]  <a href=\"{imgSrcUrl}\" target=\"_blank\" onclick=\"AppAction.downloadUrl($(this).text());return false;\" onmouseover=\"AppWidget.floatImg(this, $(this).text())\" >{imgSrcUrl}</a>");
                    imgIndex++;
                }
            }

            if (CollType == "collDownloadImgSrc")
            {
                DownloadAllImgAndPack();
            }
        }

        /// <summary>
        /// 下载所有图片并打包
        /// </summary>
        private void DownloadAllImgAndPack()
        {
            Log("\n");
            LogInfo("准备一口气下载所有图片并打包");

            string donwloadTempDirTag = "Download";
            var downloadTempPath = GetTempDirPath(donwloadTempDirTag);
            int imgTotal = 0;
            int doneTotal = 0;
            foreach (string imgType in imgUrlPool.Keys)
            {
                var imgIndex = 0;
                foreach (string imgSrc in imgUrlPool[imgType])
                {
                    var number = imgIndex + 1;
                    var imgSrcUrl = imgUrlPool[imgType][imgIndex];
                    imgTotal++;
                    // http://www.cnblogs.com/doforfuture/p/6293926.html
                    ThreadPool.QueueUserWorkItem(m =>
                    {
                        string errorMsg = null;
                        try
                        {
                            Utils.DownloadImgByUrl(imgSrc, downloadTempPath, imgType + "_" + number.ToString());
                        } catch (Exception e) { errorMsg = e.Message; }
                        if (errorMsg == null)
                            LogSuccess($"下载完毕 {imgSrcUrl}");
                        else
                            LogWarning($"下载错误 已忽略 {imgSrcUrl} {errorMsg}");
                        doneTotal++;
                    });
                    imgIndex++;
                }
            }

            do {} while (imgTotal != doneTotal);

            string zipFilePath = Path.Combine(GetTempDirPath("ForUserSave"), $"CollItemDescImg_{DateTime.Now.ToString("yyyyMMddhhmmss")}.zip"); // 以后将ZIP统一放到一个文件夹内
            Log("\n");
            LogInfo("开始打包所有图片");
            ZipFile.CreateFromDirectory(downloadTempPath, zipFilePath);
            LogSuccess("图片打包完毕");
            Log("\n");
            DeleteTempDirPath(donwloadTempDirTag);
            LogSuccess("临时文件清理完毕");
            Log("\n");
            LogInfo($"<a href=\"{zipFilePath}\" onclick=\"saveLocalFile($(this).attr('href'));return false;\">点击保存图片打包文件</a>");
        }

        /// <summary>
        /// 添加一张图片URL
        /// </summary>
        /// <param name="imgType">图片类型</param>
        /// <param name="srcUrl">图片URL</param>
        private void AddImgUrl(string imgType, string srcUrl)
        {
            if (string.IsNullOrEmpty(imgType) || string.IsNullOrEmpty(srcUrl)) return;
            imgType = imgType.Trim();srcUrl = srcUrl.Trim();

            if (!imgUrlPool.ContainsKey(imgType))
                imgUrlPool[imgType] = new ArrayList();
            imgUrlPool[imgType].Add(UrlSchemeFull(srcUrl));
        }

        /// <summary>
        /// 翻译页面类型
        /// </summary>
        /// <param name="pageType"></param>
        /// <returns></returns>
        private string PageTypeTranslation(string pageType)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            map["Tmall"] = "天猫";
            map["Taobao"] = "淘宝";
            map["Alibaba"] = "阿里巴巴";
            map["Suning"] = "苏宁易购";
            map["Gome"] = "国美在线";
            return map.ContainsKey(pageType) ? map[pageType] : null;
        }
    }
}