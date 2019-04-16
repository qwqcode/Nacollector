using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NacollectorUtils.Settings
{
    [Serializable]
    public class CookieGetterSettings
    {
        public string StartUrl { get; set; }

        public string EndUrlReg { get; set; }

        public string Caption = "";

        public Dictionary<string, object> InputAutoCompleteConfig = null;

        public void UseInputAutoComplete(string pageUrlReg, List<string> inputElemCssSelectors)
        {
            InputAutoCompleteConfig = new Dictionary<string, object>
            {
                { "pageUrlReg", pageUrlReg },
                { "inputElemCssSelectors", inputElemCssSelectors },
            };
        }
    }
}
