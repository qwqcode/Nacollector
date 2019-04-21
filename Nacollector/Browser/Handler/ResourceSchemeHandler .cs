using CefSharp;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

public class ResourceSchemeHandler : ResourceHandler
{
    public override bool ProcessRequestAsync(IRequest request, ICallback callback)
    {
        var names = this.GetType().Assembly.GetManifestResourceNames();

        Console.WriteLine(names);

        Uri u = new Uri(request.Url);
        String file = u.Authority + u.AbsolutePath;
        
        Assembly ass = Assembly.GetExecutingAssembly();
        String resourcePath = ass.GetName().Name + ".Resources." +  file.Replace("/", ".");

        Task.Run(() =>
        {
            using (callback)
            {
                if (ass.GetManifestResourceInfo(resourcePath) != null)
                {
                    Stream stream = ass.GetManifestResourceStream(resourcePath);
                    string mimeType = "application/octet-stream";
                    switch (Path.GetExtension(file))
                    {
                        case ".html":
                            mimeType = "text/html";
                            break;
                        case ".js":
                            mimeType = "text/javascript";
                            break;
                        case ".css":
                            mimeType = "text/css";
                            break;
                        case ".png":
                            mimeType = "image/png";
                            break;
                        case ".appcache":
                            break;
                        case ".manifest":
                            mimeType = "text/cache-manifest";
                            break;
                    }

                    // Reset the stream position to 0 so the stream can be copied into the underlying unmanaged buffer
                    stream.Position = 0;
                    // Populate the response values - No longer need to implement GetResponseHeaders (unless you need to perform a redirect)
                    ResponseLength = stream.Length;
                    MimeType = mimeType;
                    StatusCode = (int)HttpStatusCode.OK;
                    Stream = stream;

                    callback.Continue();
                }
                else
                {
                    callback.Cancel();
                }
            }
        });

        return true;
    }
}