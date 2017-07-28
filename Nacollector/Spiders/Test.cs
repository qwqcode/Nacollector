using Nacollector.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nacollector.Spiders
{
    /// <summary>
    /// 测试
    /// </summary>
    class Test : Spider
    {
        public override void BeginWork()
        {
            base.BeginWork();

            DownloadImgsTest();
        }

        private void DownloadImgsTest()
        {
            string[] urls = new string[] {
                "https://img.alicdn.com/imgextra/i4/1990431537/TB2UJMEaTAlyKJjSZFhXXc8XFXa_!!1990431537.jpg",
                "https://img.alicdn.com/imgextra/i4/1990431537/TB2JwwCaP3nyKJjSZFHXXaTCpXa_!!1990431537.jpg",
                "https://img.alicdn.com/imgextra/i4/1990431537/TB2eKZzaTMlyKJjSZFFXXalVFXa_!!1990431537.jpg",
                "https://img.alicdn.com/imgextra/i4/67536774/TB2qG2TqpXXXXbzXpXXXXXXXXXX_!!67536774.jpg",
                "https://img.alicdn.com/imgextra/i2/67536774/TB2C6QhqpXXXXb6XXXXXXXXXXXX_!!67536774.jpg",
                "https://img.alicdn.com/imgextra/i1/67536774/TB28EAnqpXXXXblXXXXXXXXXXXX_!!67536774.jpg"
            };

            string workPath = Path.Combine(Utils.GetTempPath(), "downloads");
            Directory.CreateDirectory(workPath);

            int num = 1;
            foreach (var item in urls)
            {
                LogInfo("开始下载 " + item);
                Utils.DownloadImgByUrl(item, workPath, num.ToString());
                LogSuccess("下载成功");
                num++;
            }

            LogInfo("正在打包");
            ZipFile.CreateFromDirectory(workPath, Utils.GetTempPath("temp.zip"));
            LogSuccess("打包完毕");
        }
    }
}
