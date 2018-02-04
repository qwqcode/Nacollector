using CsQuery;
using Nacollector.Util;
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

namespace Nacollector.Spiders.Business
{
    /// <summary>
    /// 商品详情页图片解析
    /// </summary>
    public class CollItemDescImg : Spider
    {
        // 参数
        string PageUrl = "";
        string PageType = "";
        string ImgType = "";
        string CollType = "";
        // 页面内容
        string pageContent;
        // CsQuery Dom
        CQ pageDom;
        // 图片链接池
        Dictionary<string, ArrayList> imgUrlPool = new Dictionary<string, ArrayList>();

        public override void BeginWork()
        {
            base.BeginWork();
            // 参数设定
            PageUrl = GetParm("PageUrl"); // 若使用 new Uri() 会把 urlencode 的参数自动 decode
            PageType = GetParm("PageType");
            ImgType = GetParm("ImgType");
            CollType = GetParm("CollType");
            // 下载页面
            LogInfo("开始下载：" + PageUrl);
            var downloadPage = Utils.GetPageByUrl(PageUrl);
            if (downloadPage.StatusCode != System.Net.HttpStatusCode.OK) { throw new Exception("下载失败 [" + downloadPage.StatusCode + "] " + downloadPage.StatusDescription); }
            pageContent = downloadPage.Html;
            LogSuccess("下载完毕");
            pageDom = CQ.CreateDocument(pageContent);
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
                AddImgUrl("主图", picSrcUrl.Replace("_800x800", ""));
            });
        }

        private void SuningCategory()
        {
            pageDom[".tip-infor img"].Each((i, e) => {
                string picSrcUrl = e.GetAttribute("src");
                AddImgUrl("分类图", picSrcUrl.Replace("_60x60", ""));
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
                    "喔豁！没有采集到任何的图片URL...\n" +
                    "╮(╯▽╰)╭   怕是 " + PageTypeTranslation(PageType) + " 页面结构更新了？！\n" +
                    "如果你觉得是，请 +Q: 1149527164 告诉我");
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
                    Log($"[{number}]  <a href=\"{imgSrcUrl}\" target=\"_blank\" onclick=\"downloadFile($(this).text());return false;\" onmouseover=\"AppWidget.floatImg(this, $(this).text())\" >{imgSrcUrl}</a>");
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
            LogInfo($"<a href=\"{zipFilePath}\" onclick=\"downloadFile($(this).attr('href'));return false;\">点击保存图片打包文件</a>");
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