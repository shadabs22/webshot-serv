﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.IO;
using System.Collections.Specialized;
using System.Collections;
using GetSiteThumbnail;
using System.Windows.Forms;

namespace T.Serv
{
    class WebShotServer
    {
        

        public class QueueWorker
        {
            private readonly object syncRoot;
            private Queue<WebShot> queue = new Queue<WebShot>();
            public static Hashtable ht = new Hashtable();

            public QueueWorker()
            {
               syncRoot = new object();
            }

            public void Enqueue(WebShot webShot)
            {
                lock (syncRoot)
                {
                  if (ht[webShot.url] == "fetching") return;

                  Console.WriteLine("[{0}] Enqueue: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), webShot.url);
                  queue.Enqueue(webShot);
                }
            }
            
            public void Dequeue()
            {
                while (true)
                {
                    //Console.WriteLine(Thread.CurrentThread.Name);
                    Thread.Sleep(1000);

                    lock (syncRoot)
                    {
                        if (queue.Count > 0)
                        {
                            Thread thread = new Thread(new ThreadStart(queue.Peek().Fetch));
                            thread.SetApartmentState(ApartmentState.STA);
                            thread.Start();

                            ht.Add(queue.Peek().url, "fetching");
  
                            queue.Dequeue();
                        }
                    }
                }
            }
        }

        public class HttpWorker
        {
            private HttpListenerContext context;
            public static QueueWorker queueworker = new QueueWorker();
                    
            public HttpWorker(HttpListenerContext context)
            {
                this.context = context;
            }

            public void Handle()
            {
                //Console.WriteLine("[{0}] Handle: {1}, ip: {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), context.Request.RawUrl, context.Request.RemoteEndPoint);

                if (context.Request.QueryString.Count == 0)
                {
                    //root
                    StreamWriter writer = new StreamWriter(context.Response.OutputStream);
                    writer.Write("/");
                    writer.Close();
                    return;
                }

                WebShot webShot = new WebShot(context.Request.RawUrl);

                if (!webShot.isReady)
                {
                    queueworker.Enqueue(webShot);
                }
                
                byte[] buffer = webShot.GetStream();
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);

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
                    Thread thread = new Thread(new ThreadStart(queueworker.Dequeue));
                    thread.Name = "qw_" + i.ToString();
                    thread.Start();
                }
                                
                Console.WriteLine("[{0}] HttpListener: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), localprefix);
                while (true)
                {
                    HttpListenerContext context = listener.GetContext();
                    new Thread(new ThreadStart(new HttpWorker(context).Handle)).Start();
                }
            }
        }
    }
}
