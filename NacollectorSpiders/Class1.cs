using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NacollectorSpiders
{
    public class Class1
    {
        public void HelloWorld ()
        {
            Form f1 = new Form();
            f1.ShowDialog();

            Debug.WriteLine(Utils.SpiderHelper.GetTempPath());

            JArray array = new JArray();
            array.Add("Manual text");
            array.Add(new DateTime(2000, 5, 23));

            JObject o = new JObject();
            o["MyArray"] = array;

            string json = o.ToString();
            MessageBox.Show(json);
        }
    }
}
