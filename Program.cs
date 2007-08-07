using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.IO;
using System.Collections.Specialized;
using GetSiteThumbnail;

namespace T.Serv
{
    class WebShotServer
    {
        public class HttpWorker
        {
            private HttpListenerContext context;

            public HttpWorker(HttpListenerContext context)
            {
                this.context = context;
            }

            public void Handle()
            {
                Console.WriteLine("[{0}] GET: {2}, IP: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), context.Request.RemoteEndPoint, context.Request.RawUrl);

                if (context.Request.QueryString.Count == 0)
                {
                    context.Response.Close();
                    return;
                }

                int val;
                int w = 240; //h = output height 
                int h = 180; //w = output width 
                long q = 80L; //q = quality level (30-70%) 


                if (int.TryParse(context.Request.QueryString["w"], out val)) w = val;
                if (int.TryParse(context.Request.QueryString["h"], out val)) h = val;

                string fileName = "./web-shots/" + context.Request.QueryString["url"].Replace("/", "_").Replace("\\", "_") + "_" + w.ToString() + "x" + h.ToString() + ".png";

                FileInfo file = new FileInfo(fileName);
                TimeSpan span = DateTime.Now - file.CreationTime;

                if (!file.Exists || span.TotalDays > 7 || context.Request.QueryString["force"] == "true")
                {
                    WebPageBitmap webBitmap = new WebPageBitmap(context.Request.QueryString["url"], w, h, false);

                    if (webBitmap.isReady)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                        webBitmap.SaveThumbnail(fileName, q);

                        Console.WriteLine("[{0}] Saved: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), fileName);
                    }
                    else
                    {
                        fileName = "./window.png";
                    }
                }

                if (File.Exists(fileName)) 
                {
                    FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    int read;
                    const int size = 4096;
                    byte[] bytes = new byte[4096];
                    while ((read = fs.Read(bytes, 0, size)) > 0)
                    {
                        context.Response.OutputStream.Write(bytes, 0, read);
                    }
                }

                context.Response.OutputStream.Close();
                context.Response.Close();
            }
        }

        public static void Main(string[] args)
        {
            HttpListener listener = new HttpListener();
            string localprefix = "http://*:" + 8080 + "/";
            listener.Prefixes.Add(localprefix);
            listener.Start();

            Console.WriteLine("[{0}] HttpListener: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), localprefix);
            while (true)
            {
                HttpListenerContext context = listener.GetContext();

                Thread thread = new Thread(new ThreadStart(new HttpWorker(context).Handle));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
        }
    }
}
