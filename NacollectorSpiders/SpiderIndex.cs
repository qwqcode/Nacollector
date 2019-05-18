using NacollectorSpiders.Lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NacollectorSpiders
{
    public class SpiderIndex
    {
        public static readonly List<SpiderTypeAttribute> SpiderTypes = new List<SpiderTypeAttribute>
        {
            new SpiderTypeAttribute(){
                Name = "Business",
                Label = "电商",
                NameSpace = "NacollectorSpiders.Business"
            },
            new SpiderTypeAttribute(){
                Name = "Picture",
                Label = "图片",
                NameSpace = "NacollectorSpiders.Picture"
            }
        };

        public static string BuildAllSpiderFormJsCode()
        {
            string jsCode = "";

            // 获取当前程序集下的全部类
            var types = Assembly.GetExecutingAssembly().GetTypes();

            // 遍历爬虫类型
            foreach (SpiderTypeAttribute typeItem in SpiderTypes)
            {
                jsCode += $"TaskGen.newSpiderType(\"{typeItem.Name}\", \"{typeItem.Label}\");"; // 前端注册类型代码

                // 从所有的类中过滤
                var classes = types.Where((t) => {
                    return String.Equals(t.Namespace, typeItem.NameSpace, StringComparison.Ordinal) // 指定 namespace 下的类
                        && t.IsSubclassOf(typeof(Spider)); // 继承了 Spider 的类
                });

                // 遍历过滤后的类
                foreach (Type classItem in classes)
                {
                    string spiderFormCode = ""; // 前端 Spider 表单构建代码

                    // 获取 Spider Info
                    var attributes = classItem.GetCustomAttributes(typeof(SpiderRegisterAttribute), false);
                    if (attributes.GetLength(0) <= 0)
                        continue; // 若没有任何 SpiderRegisterAttribute，则跳过这个 class
                    var spiderInfo = (SpiderRegisterAttribute)attributes[0];
                    
                    // 遍历类中所有字段，获取 FormTextInput
                    foreach (var fields in classItem.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
                    {
                        var ftiAttributes = fields.GetCustomAttributes(typeof(FormTextInputAttribute), false);
                        if (ftiAttributes.GetLength(0) <= 0)
                            continue; // 若这个字段没有任何 FormTextInputAttribute，则跳过
                        var formTextInput = (FormTextInputAttribute)ftiAttributes[0];
                        string parmsCode = (formTextInput.Parms.Trim() != "") ? $", {formTextInput.Parms}" : "";
                        spiderFormCode += $"form.{formTextInput.Type}('{fields.Name}', '{formTextInput.Label}'{parmsCode});"; // 前端 textInput 代码
                    }

                    jsCode += $"TaskGen.newSpider(\"{typeItem.Name}\", \"{classItem.Name}\", \"{spiderInfo.Label}\", (form) => {{{spiderFormCode}}});"; // 前端创建新 Spider 代码
                }
            }

            jsCode += "TaskGen.loadSpiderList();"; // 前端装载 SpiderList 代码

            //Debug.WriteLine(jsCode);

            return jsCode;
        }
    }
}
