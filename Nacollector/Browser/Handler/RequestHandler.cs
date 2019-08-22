using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CefSharp;
using CefSharp.Handler;

namespace Nacollector.Browser.Handler
{
    public class RequestHandler : DefaultRequestHandler
    {
        public override bool OnBeforeBrowse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            if (frame.Url == "" || CrBrowser.CheckIsAppUrl(frame.Url))
            {
                return false;
            }

            return true;
        }

        /*public override CefReturnValue OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            Uri url;
            if (Uri.TryCreate(request.Url, UriKind.Absolute, out url) == false)
            {
                //If we're unable to parse the Uri then cancel the request
                // avoid throwing any exceptions here as we're being called by unmanaged code
                return CefReturnValue.Cancel;
            }

            //Example of how to set Referer
            // Same should work when setting any header

            // For this example only set Referer when using our custom scheme
            // request.SetReferrer("https://qwqaq.com", ReferrerPolicy.Default);

            //Example of setting User-Agent in every request.
            //var headers = request.Headers;

            //var userAgent = headers["User-Agent"];
            //headers["User-Agent"] = userAgent + " CefSharp";

            //request.Headers = headers;

            //NOTE: If you do not wish to implement this method returning false is the default behaviour
            // We also suggest you explicitly Dispose of the callback as it wraps an unmanaged resource.
            //callback.Dispose();
            //return false;

            return CefReturnValue.Continue;
        }*/
    }
}
