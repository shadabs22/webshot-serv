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
using System.Xml;
using System.Diagnostics;

[assembly: CLSCompliant(true)]
namespace T.Serv
{
    class WebShotServer
    {
        public static XmlInfo xi = new XmlInfo();

        public class XmlInfo
        {
            private ArrayList processortimes = new ArrayList();
            private ArrayList requests = new ArrayList();

            public XmlInfo()
            {
                new Thread(new ThreadStart(this.Handle)).Start();
            }

            public void AddRequest(string req)
            {
                requests.Add(req);
                if (requests.Count > 30) requests.RemoveAt(0);
            }

            public void Fetch(XmlDocument doc)
            {
                 XmlElement main, newnode, child;
                 
                 main = doc.CreateElement("SystemInfo");

                 newnode = doc.CreateElement("Processor");
                 foreach(float i in processortimes)
                 {
                     child = doc.CreateElement("Value");
                     child.InnerText = i.ToString();
                     newnode.AppendChild(child);
                 }
                 main.AppendChild(newnode);

                 newnode = doc.CreateElement("Memory");

                 PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");
                 child = doc.CreateElement("Value");
                 child.InnerText = ramCounter.NextValue().ToString();
                 newnode.AppendChild(child);
                 ramCounter.Close();
                 
                 main.AppendChild(newnode);
                
                 newnode = doc.CreateElement("Requests");
                 foreach (string i in requests)
                 {
                     child = doc.CreateElement("Value");
                     child.InnerText = i.ToString();
                     newnode.AppendChild(child);
                 }
                 main.AppendChild(newnode);

                 doc.DocumentElement.AppendChild(main);
            }
            
            private void Handle()
            {
                PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

                while(true)
                {
                    processortimes.Add(cpuCounter.NextValue());
                    if (processortimes.Count > 30) processortimes.RemoveAt(0);
                    Thread.Sleep(1000);
                }
            }
        }

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
                    thread.Name = "qw_" + i;
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
                        //Console.WriteLine("[{0}] Enqueue({2}): {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), webShot.url, queue.Count);
                        Console.Write("e[{0}]", queue.Count);
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

                        //Console.WriteLine(Thread.CurrentThread.Name + " alive?");
                    }

                    webShot.Fetch(30);

                    if (webShot.ReadyState == WebShot.wsTimeout)
                    {
                        EnqueueSlow(webShot);
                    }
                    else
                    {
                        hash.Remove(webShot.url);
                    }
                }
            }
            public void EnqueueSlow(WebShot webShot)
            {
                hash[webShot.url] = "re-fetching";

                slowqueue.Enqueue(webShot);
                //Console.WriteLine("[{0}] EnqueueSlow({2}): {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), webShot.url, slowqueue.Count);

                Console.Write("es[{0}]", slowqueue.Count);
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
                        xi.AddRequest(context.Request.RawUrl);

                        WebShot webShot = new WebShot(context.Request.QueryString);
                                            
                        if (webShot.url != null)
                        {
                            if (webShot.ReadyState != WebShot.wsReady) queueworker.Enqueue(webShot);

                            if (queueworker.queue.Count <= 1)
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

                            XmlDocument doc = new XmlDocument();

                            //doc.Load(new XmlTextReader("./index.xml"));

                            doc.Load(new StringReader("<?xml version=\"1.0\" encoding=\"utf-8\"?><?xml-stylesheet type=\"text/xsl\" href=\"index.xsl\"?><root><version>1.0</version><author>Telrik</author></root>"));

                            xi.Fetch(doc);
                            ResponseWrite(doc);
                        }
                    }
                    
                    try
                    {
                        context.Response.OutputStream.Close();
                        context.Response.Close();
                    }
                    catch (ObjectDisposedException)
                    {  }
                }
                
                [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "args"), STAThread]
                public static void Main(string[] args)
                {
                    xi = new XmlInfo();
                    HttpListener listener = new HttpListener();
                    queueworker = new QueueWorker(5);

                    string localprefix = "http://*:" + 8080 + "/";

                    listener.Prefixes.Add(localprefix);
                    listener.Start();

                    DateTime start = DateTime.Now;
                    Console.WriteLine("[{0}] T.Serv 6.1r, HttpListener: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), localprefix);
                                        
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
}

