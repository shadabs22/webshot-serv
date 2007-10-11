using System;
using System.Text;
using System.Net;
using System.Threading;
using System.IO;
using System.Collections;
using System.Windows.Forms;
using System.Xml;
using GetSiteThumbnail;

[assembly: CLSCompliant(true)]
namespace T.Serv
{
    class WebShotServer
    {
        const int WORKER_COUNT = 4;

        public class HttpWorker
        {
            private HttpListenerContext context;
            public HttpWorker(HttpListenerContext context)
            {
                this.context = context;
            }
            public void ResponseWrite(XmlDocument doc)
            {
                try
                {
                    doc.Save(context.Response.OutputStream);
                }
                catch (System.Net.HttpListenerException)
                {
                    // client closed connection
                    // 1. ErrorCode=1229. An operation was attempted on a nonexistent network connection.
                    // 2. ErrorCode=64. The specified network name is no longer available.
                }
            }
            public void ResponseWrite(byte[] buffer)
            {
                try
                {
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                }
                catch (System.Net.HttpListenerException)
                {
                    // client closed connection
                    // 1. ErrorCode=1229. An operation was attempted on a nonexistent network connection.
                    // 2. ErrorCode=64. The specified network name is no longer available.
                }
            }
            public void ResponseWrite(string fileName)
            {
                BinaryReader binReader = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                ResponseWrite(binReader.ReadBytes((int)binReader.BaseStream.Length));
                binReader.Close();
            }

            public void Handle()
            {
                Console.Write(".");

                if (!String.IsNullOrEmpty(context.Request.QueryString["url"]))
                {
                    queueworker.AddNode("request", context.Request.RawUrl);

                    WebShot webShot = new WebShot(context.Request.QueryString);

                    if (webShot.url != null)
                    {
                        if (webShot.ReadyState != WebShot.wsReady) queueworker.Enqueue(webShot);

                        if (queueworker.hash.Count == WORKER_COUNT - 1)
                        {
                            DateTime start = DateTime.Now;
                            while (webShot.ReadyState == WebShot.wsNotReady)
                            {
                                TimeSpan span = DateTime.Now - start;
                                if (span.Seconds > 3) break;
                                Thread.Sleep(100);
                            }
                        }
                    }

                    if (webShot.ReadyState != WebShot.wsNotReady)
                    {
                        ResponseWrite(webShot.GetWebShot());
                    }
                    else
                    {
                        ResponseWrite(webShot.GetWaitShot());
                    }
                }
                else if (context.Request.QueryString["xorbitmap"] == "true")
                {
                    ResponseWrite(WebShot.GetXorBitmap());
                }
                else
                {
                    if (File.Exists("." + context.Request.RawUrl))
                    {
                        ResponseWrite("." + context.Request.RawUrl);
                    }
                    else
                    {
                        context.Response.ContentType = "text/xml";

                        ResponseWrite(queueworker.GetXml());
                    }
                }
                try
                {
                    context.Response.OutputStream.Close();
                    context.Response.Close();
                }
                catch
                { }
            }
  
            public static WebShotQueueWorker queueworker = new WebShotQueueWorker(WORKER_COUNT);
                        
            public static void Main(string[] args)
            {

                HttpListener listener = new HttpListener();

                string localprefix = "http://*:" + 8080 + "/";

                listener.Prefixes.Add(localprefix);
                listener.Start();

                DateTime start = DateTime.Now;
                Console.WriteLine("\n[{0}] T.Serv "+webshot.serv.Properties.Resources.Version+", HttpListener: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), localprefix);

                while (true)
                {
                    HttpListenerContext context = listener.GetContext();
                    new Thread(new ThreadStart(new HttpWorker(context).Handle)).Start();

                    TimeSpan span = DateTime.Now - start;
                    if (span.Hours > 3) System.Environment.Exit(-1);
                }
            }
        }
    }

}

