using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NacollectorSpiders.Lib
{
    public class SpiderTypeAttribute : Attribute
    {
        public string NameSpace { get; set; }

        public string Name { set; get; }

        public string Label { set; get; }
    }
}
