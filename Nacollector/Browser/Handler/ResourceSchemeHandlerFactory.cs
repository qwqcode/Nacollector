using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;

class ResourceSchemeHandlerFactory : ISchemeHandlerFactory
{
    public IResourceHandler Create(IBrowser browser, IFrame frame, string schemeName, IRequest request)
    {
        return new ResourceSchemeHandler();
    }

    public static string SchemeName { get { return "nacollector"; } }
}
