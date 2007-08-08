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
            public class QueueWorker
            {
                private Queue<HttpListenerContext> queue = new Queue<HttpListenerContext>();

                public void Fetch()
                {
                    if (queue.Count > 0)
                    {
                        HttpListenerContext context = queue.Dequeue();
                        WebPageBitmap webBitmap = new WebPageBitmap(context);
                    }
                    Thread.Sleep(1000);
                }

                public void Enqueue(HttpListenerContext context)
                {
                    queue.Enqueue(context);
                }

            }

            private HttpListenerContext context;
           

            public HttpWorker(HttpListenerContext context)
            {
                this.context = context;
            }

            public void Handle()
            {
                Console.WriteLine("[{0}] HttpWorker.Handle: {2}, ip: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), context.Request.RemoteEndPoint, context.Request.UserHostName);

                if (context.Request.QueryString.Count == 0)
                {
                    //root
                    StreamWriter writer = new StreamWriter(context.Response.OutputStream);
                    writer.Write("/");
                    writer.Close();
                    return;
                }
                
                //getting image
                FileInfo file = new FileInfo(WebPageBitmap.url2file(context.Request.RawUrl));
                TimeSpan span = DateTime.Now - file.CreationTime;
                
                if (!file.Exists || span.TotalDays > 7 || context.Request.QueryString["force"] == "true")
                {
                    new QueueWorker().Enqueue(context);
                    //this.queue.Enqueue(context);
                   //WebShotQueue webQueue = new WebShotQueue(context);


                    //Thread.Sleep(5000);

                    if (!file.Exists)
                    {
                        file = new FileInfo("./time.png");
                    }
                }

                WriteToStream(context.Response.OutputStream, file.ToString());

                context.Response.OutputStream.Close();
                context.Response.Close();
                return;
            }

            static bool WriteToStream(Stream output, string fileName)
            {
                int bytesRead;
                byte[] buffer = new byte[4096];
                FileStream fs = null;

                if (!File.Exists(fileName)) return false;

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

                for (int i = 1; i <= 5; i++)
                {
                    Thread thread = new Thread(new ThreadStart(new QueueWorker().Fetch));
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                }

                
                
                Console.WriteLine("[{0}] HttpListener: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), localprefix);
                while (true)
                {
                    HttpListenerContext context = listener.GetContext();
                    Thread thread = new Thread(new ThreadStart(new HttpWorker(context).Handle));
                   // thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                }
            }
        }
    }
}
