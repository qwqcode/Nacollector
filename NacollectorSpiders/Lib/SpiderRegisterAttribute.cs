using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NacollectorSpiders.Lib
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SpiderRegisterAttribute : Attribute
    {
        public string Label { get; set; }
    }
}
