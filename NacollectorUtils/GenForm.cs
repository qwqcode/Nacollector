using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NacollectorUtils
{
    public class GenForm
    {
        private static string Code = "";

        protected static void NewType(string name, string label)
        {
            Code += $"TaskGen.newSpiderType(\"{name}\", \"{label}\");";
        }

        protected static void NewSpider(string typeName, string name, string label, string genFormFuncCode)
        {
            Code += $"TaskGen.newSpider(\"{typeName}\", \"{name}\", \"{label}\", (form) => {{{genFormFuncCode}}});";
        }

        public static string GetJsRunCode()
        {
            return $"{Code}TaskGen.loadSpiderList();";
        }
    }
}
