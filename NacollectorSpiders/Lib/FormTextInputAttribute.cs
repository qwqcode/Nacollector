using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NacollectorSpiders.Lib
{
    [AttributeUsage(AttributeTargets.Field)]
    public class FormTextInputAttribute : Attribute
    {
        /// <summary>
        /// 字段标签
        /// </summary>
        public string Label { get; set; }

        public bool Required { get; set; } = true;

        public string Type { get; set; } = "textInput";

        public string Parms { get; set; } = "";
    }
}
