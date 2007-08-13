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
                   thread.Name = "qw_" + i.ToString();
                   thread.Start();
               }

               thread = new Thread(new ThreadStart(this.DequeueSlow));
               thread.SetApartmentState(ApartmentState.STA);
               thread.Priority = ThreadPriority.BelowNormal; 
               thread.Start();
            }

            public void Enqueue(WebShot webShot)
            {
                lock (this.syncRoot)
                {
                    if (!hash.Contains(webShot.url))
                    {
                        hash.Add(webShot.url, "fetching");
                        
                        queue.Enqueue(webShot);
                        Console.WriteLine("[{0}] Enqueue: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), webShot.url);
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

                        if (webShot == null) Thread.Sleep(1000);
                    }

                    webShot.Fetch(30);

                    if (webShot.isTimeout)
                    {
                        EnqueueSlow(webShot);
                    }

                    hash.Remove(webShot.url);
                }
            }

            public void EnqueueSlow(WebShot webShot)
            {
                Console.WriteLine("[{0}] EnqueueSlow: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), webShot.url);
                slowqueue.Enqueue(webShot);
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

                        if (webShot == null) Thread.Sleep(1000);
                    }
                    
                    webShot.Fetch(240);
                }

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
                try
                {
                    WebShot webShot = new WebShot(context.Request.RawUrl);

                    if (webShot.url != null && !webShot.isReady)
                    {
                        queueworker.Enqueue(webShot);
                    }

                    webShot.Save(context.Response.OutputStream);
                   
                    context.Response.OutputStream.Close();
                    context.Response.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message + e.StackTrace);
                }

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
                        catch (System.Net.HttpListenerException)
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

                queueworker = new QueueWorker(5);

                string localprefix = "http://*:" + 8080 + "/";
                                
                listener.Prefixes.Add(localprefix);
                listener.Start();
                                
                Console.WriteLine("[{0}] T.Serv 1.0b, HttpListener: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), localprefix);
                while (true)
                {
                    HttpListenerContext context = listener.GetContext();
                    new Thread(new ThreadStart(new HttpWorker(context).Handle)).Start();
                }
            }
        }
    }
}
