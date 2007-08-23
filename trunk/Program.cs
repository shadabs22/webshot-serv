using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.IO;
using System.Collections.Specialized;
using System.Collections;
using GetSiteThumbnail;
using System.Windows.Forms;
using System.ComponentModel;
using System.Web;
using System.Runtime.InteropServices;

[assembly: CLSCompliant(true)]
namespace T.Serv
{
    class WebShotServer
    {
        public class QueueWorker
        {
            private readonly object syncRoot;
            private Queue<WebShot> queue = new Queue<WebShot>();
            private Queue<WebShot> slowqueue = new Queue<WebShot>();
            public static Hashtable hash = new Hashtable();

            public QueueWorker(int count)
            {
                syncRoot = new object();
                
                Thread thread;

                for (int i = 1; i <= count; i++)
                {
                    thread = new Thread(new ThreadStart(this.Dequeue));
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Priority = ThreadPriority.Normal;
                    thread.Start();
                }

                thread = new Thread(new ThreadStart(this.DequeueSlow));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Priority = ThreadPriority.BelowNormal;
                thread.Start();
            }
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.DateTime.ToString(System.String)")]
            public void Enqueue(WebShot webShot)
            {
                lock (this.syncRoot)
                {
                    if (!hash.Contains(webShot.url))
                    {
                        hash.Add(webShot.url, "fetching");

                        queue.Enqueue(webShot);
                        Console.WriteLine("[{0}] Enqueue({2}): {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), webShot.url, queue.Count);
                    }
                }
            }
            public void Dequeue()
            {
                while (true)
                {
                    WebShot webShot = null;

                    while (webShot == null)
                    {
                        lock (this.syncRoot)
                        {
                            if (queue.Count > 0)
                            {
                                webShot = queue.Dequeue();
                            }
                        }

                        if (webShot == null) Thread.Sleep(100);
                    }

                    webShot.Fetch(30);

                    if (webShot.isTimeout)
                    {
                        EnqueueSlow(webShot);
                    }
                    else
                    {
                        hash.Remove(webShot.url);
                    }
                }
            }
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.DateTime.ToString(System.String)")]
            public void EnqueueSlow(WebShot webShot)
            {
                hash[webShot.url] = "re-fetching";

                slowqueue.Enqueue(webShot);
                Console.WriteLine("[{0}] EnqueueSlow({2}): {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), webShot.url, slowqueue.Count);
            }
            public void DequeueSlow()
            {
                while (true)
                {
                    WebShot webShot = null;

                    while (webShot == null)
                    {
                        if (slowqueue.Count > 0)
                        {
                            webShot = slowqueue.Dequeue();
                        }

                        if (webShot == null) Thread.Sleep(100);
                    }
                    webShot.Fetch(240);
                    hash.Remove(webShot.url);
                }
            }

            public class HttpWorker
            {
                private HttpListenerContext context;
                public static QueueWorker queueworker;

                public HttpWorker(HttpListenerContext context)
                {
                    this.context = context;
                }

                public void Handle()
                {
                    Console.Write(".");

                    if (context.Request.QueryString.Count == 0) //root
                    {
                        StreamWriter writer = new StreamWriter(context.Response.OutputStream);
                        writer.Write("/");
                        writer.Close();
                        return;
                    }

                    if (context.Request.QueryString["xorbitmap"] == "true")
                    {
                        WebShot.XorBitmap(context.Response.OutputStream, 512, 512);
                    }
                                        
                    WebShot webShot = new WebShot(context.Request.RawUrl);

                    if (webShot.url != null && !webShot.isReady)
                    {
                        queueworker.Enqueue(webShot);
                    }

                    webShot.Save(context.Response.OutputStream);

                    try
                    {
                        context.Response.OutputStream.Close();
                        context.Response.Close();
                    }
                    catch (ObjectDisposedException)
                    {

                    }
                }
                [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.DateTime.ToString(System.String)"), STAThread]
                public static void Main(string[] args)
                {
                    HttpListener listener = new HttpListener();
                    queueworker = new QueueWorker(5);

                    string localprefix = "http://*:" + 8080 + "/";

                    listener.Prefixes.Add(localprefix);
                    listener.Start();

                    DateTime start = DateTime.Now;
                    Console.WriteLine("[{0}] T.Serv 3.3, HttpListener: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), localprefix);
                                        
                    while (true)
                    {
                        HttpListenerContext context = listener.GetContext();
                        new Thread(new ThreadStart(new HttpWorker(context).Handle)).Start();

                        TimeSpan span = DateTime.Now - start;
                        if (span.Hours > 1) System.Environment.Exit(-1);
                    }
                }
            }
        }
    }
}

