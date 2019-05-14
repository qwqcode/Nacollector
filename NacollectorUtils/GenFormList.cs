using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NacollectorUtils
{
    public class GenFormList : GenForm
    {
        public static string GetCode()
        {
            NewType("Business", "电商");
            NewSpider("Business", "CollItemDescImg", "商品详情页图片解析", @"
        let pageUrlEl = form.textInput('PageUrl', '详情页链接', '', InputValidators.isUrl)
        let PageTypeEl = form.selectInput('PageType', '链接类型', {
          'Tmall': '天猫',
          'Taobao': '淘宝',
          'Alibaba': '阿里巴巴',
          'Suning': '苏宁易购',
          'Gome': '国美在线'
        })
        pageUrlEl.on('input propertychange', () => {
          let urlVal = $.trim(pageUrlEl.val().toString())
          let urlMap = {
            'Tmall': ['https://detail.tmall.com'],
            'Taobao': ['https://item.taobao.com'],
            'Alibaba': ['https://detail.1688.com'],
            'Suning': ['https://product.suning.com'],
            'Gome': ['https://item.gome.com.cn']
          }
          for (let key in urlMap) {
            for (let i in urlMap[key]) {
              if (urlVal.indexOf(urlMap[key][i]) === 0) {
                PageTypeEl.val(key)
                break;
              }
            }
          }
        })
        form.selectInput('ImgType', '图片类型', {
          'Thumb': '主图',
          'Category': '分类图',
          'Desc': '详情图'
        })
        form.selectInput('CollType', '采集模式', {
          'collImgSrcUrl': '显示图片链接',
          'collDownloadImgSrc': '显示图片链接 并 下载打包保存'
        })
");
            NewSpider("Business", "CollItemDescVideo", "商品详情页视频抓取", @"
        let pageUrlEl = form.textInput('PageUrl', '详情页链接', '', InputValidators.isUrl)
");
            NewSpider("Business", "TaobaoSellerColl", "淘宝店铺搜索卖家ID名采集", @"
        form.textInput('PageUrl', '店铺搜索页链接', '', InputValidators.isUrl)
        form.numberInput('CollBeginPage', '采集开始页码', 1, 1)
        form.numberInput('CollEndPage', '采集结束页码', undefined, 1)
        form.selectInput('IgnoreTmall', '忽略天猫卖家', {
          'on': '开启',
          'off': '关闭'
        })
");
            NewSpider("Business", "TmallGxptInvite", "天猫供销平台分销商一键邀请", @"
        form.textareaInput('SellerId', '分销商ID名（一行一个）', undefined, 250)
");
            NewSpider("Business", "TmallGxptInviteDelete", "天猫供销平台分销商一键撤回", @"
        form.numberInput('DeleteBeginPage', '撤回开始页码', 1, 1)
        form.numberInput('DeleteEndPage', '撤回结束页码', undefined, 1)
");
            NewType("Picture", "图片");

            return GetJsRunCode();
        }
    }
}
