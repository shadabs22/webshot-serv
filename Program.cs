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
                    context.Response.OutputStream.Close();
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
                   WriteToStream(context.Response.OutputStream, fileName);
                }

                context.Response.OutputStream.Close();
                context.Response.Close();
            }
        }


        static bool WriteToStream(Stream output, string fileName)
        {
            int bytesRead;
            byte[] buffer = new byte[4096];
            FileStream fs = null;

            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                long count = fs.Length;

                while ((bytesRead = fs.Read(buffer, 0, (int)(count > 4096 ? 4096 : count))) > 0)
                {
                    try
                    {
                        output.Write(buffer, 0, bytesRead);
                    }
                    catch (System.Net.HttpListenerException ex)
                    {   // client closed connection
                        // 1. ErrorCode=1229. An operation was attempted on a nonexistent network connection.
                        // 2. ErrorCode=64. The specified network name is no longer available.
                        return true;
                    }
                    count -= bytesRead;
                }
            }
            finally
            {
                if (fs != null) fs.Close();
            }
            return true;
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
