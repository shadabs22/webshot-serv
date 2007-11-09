using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
//using System.Web;
using FastImage;
using Gif.Components;
using System.Resources;
using System.Xml;
using System.Diagnostics;
//using SHDocVw;
using T.Serv;

namespace GetSiteThumbnail
{
    public class WebShot : IDisposable
    {
        private Bitmap docThumbnail;
        private Graphics gfx;

        private ImageCodecInfo codec = GetEncoderInfo("image/png");
        private EncoderParameters codecParams = new EncoderParameters(1);

        private int thumbwidth = 160;
        private int thumbheight = 96;

        public static int width = 1600;
        public static int height = 960;

        private string fileName;
        public string url;

        private byte readyState;

        public const byte wsNotReady = 0;
        public const byte wsReady = 1;
        public const byte wsExpired = 2;
        public const byte wsTimeout = 3;

        public WebShot(NameValueCollection query)
        {
            //Качество
            codecParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);

            //Размеры тамбнейла
            try
            {
                thumbwidth = int.Parse(query["w"]);
                thumbheight = int.Parse(query["h"]);
            }
            catch
            { }

            //Уменьшенная картинка
            docThumbnail = new Bitmap(thumbwidth, thumbheight);

            gfx = Graphics.FromImage(docThumbnail);
            gfx.CompositingQuality = CompositingQuality.HighQuality;
            gfx.SmoothingMode = SmoothingMode.HighQuality;
            gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;

            gfx.Clear(Color.White);

            try
            {
                Uri uri = new Uri("http://" + query["url"].Replace("http://", ""));

                //Имя файла в кеше
                fileName = "./webshots/" + uri.ToString().Replace("http://", "").Replace("/", "_").Replace("\\", "_").Replace("?", "_") + "_" + thumbwidth + "x" + thumbheight + ".png";

                FileInfo file = new FileInfo(fileName);
                TimeSpan span = DateTime.Now - file.LastWriteTime;

                if (file.Exists)
                {
                    BinaryReader binReader = new BinaryReader(File.Open(file.ToString(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                    docThumbnail = new Bitmap(binReader.BaseStream);
                    binReader.Close();

                    if (span.TotalDays > 3)
                    {
                        ReadyState = wsExpired;
                    }
                    else
                    {
                        ReadyState = wsReady;
                    }
                }

                this.url = uri.ToString();
            }
            catch
            {
                gfx.FillRectangle(new HatchBrush(HatchStyle.HorizontalBrick, Color.AntiqueWhite, Color.White), new Rectangle(0, 0, thumbwidth, thumbheight));
            }
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                docThumbnail.Dispose();
                codecParams.Dispose();
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(this.fileName));
                FileStream fs = new FileStream(this.fileName, FileMode.Create, FileAccess.Write, FileShare.Read);
                docThumbnail.Save(fs, this.codec, this.codecParams);
                fs.Close();

                //Console.WriteLine("[{0}] Save: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), this.fileName);
                Console.Write("s");
            }
            catch (ExternalException)
            { }
        }
        public byte[] GetWaitShot()
        {
            GifDecoder gifDecoder = new GifDecoder();

            AnimatedGifEncoder gifEncoder = new AnimatedGifEncoder();
            MemoryStream outStream = new MemoryStream();

            gifEncoder.SetFrameRate(10);
            gifEncoder.SetDelay(100);
            gifEncoder.SetRepeat(0);
            gifEncoder.SetTransparent(Color.Black);

            gifEncoder.Start(outStream);

            ResourceManager rm = new ResourceManager("webshot.serv.Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());
            gifDecoder.Read(rm.GetStream("ajax_loader"));

            for (int i = 0, count = gifDecoder.GetFrameCount(); i < count; i++)
            {
                gifEncoder.AddFrame(gifDecoder.GetFrame(i), thumbwidth, thumbheight);
            }

            gifEncoder.Finish();

            byte[] buffer = outStream.ToArray();

            outStream.Close();

            return buffer;
        }
        public byte[] GetWebShot()
        {
            MemoryStream stream = new MemoryStream();
            docThumbnail.Save(stream, codec, codecParams);
            byte[] buffer = stream.ToArray();
            stream.Close();
            return buffer;
        }
        public static byte[] GetXorBitmap()
        {
            Bitmap XorBitmap = new Bitmap(512, 512);
            FastBitmap FBitmap = new FastBitmap(XorBitmap);

            for (int x = 0; x < XorBitmap.Width; x++)
            {
                for (int y = 0; y < XorBitmap.Height; y++)
                {
                    Color color = ColorTranslator.FromOle((x ^ y));
                    color = Color.FromArgb(color.R, color.R, color.R);
                    FBitmap.SetPixel(x, y, color);
                }
            }

            FBitmap.Release();

            ImageCodecInfo codec = WebShot.GetEncoderInfo("image/png");
            EncoderParameters codecParams = new EncoderParameters(1);
            codecParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);

            MemoryStream stream = new MemoryStream();
            XorBitmap.Save(stream, codec, codecParams);

            byte[] buffer = stream.ToArray();

            stream.Close();
            codecParams.Dispose();

            return buffer;
        }
        public void HookEvents(WebBrowser webBrowser)
        {
            webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(DocumentCompletedEventHandler);
            webBrowser.NewWindow += new CancelEventHandler(DocumentCancelEventHandler);
        }
        private void DocumentCancelEventHandler(object sender, CancelEventArgs e)
        {
            // Don't want pop-ups.
            e.Cancel = true;
        }

        private void DocumentCompletedEventHandler(object sender, WebBrowserDocumentCompletedEventArgs ex)
        {
            int shadowSize = 5;
            int shadowMargin = 2;

            WebBrowser webBrowser = sender as WebBrowser;

            if (webBrowser.Document.Url.ToString().IndexOf("shdoclc.dll") == -1)
            {
                //Целая картинка
                Bitmap docImage = new Bitmap(width, height);

                webBrowser.DrawToBitmap(docImage, new Rectangle(0, 0, width, height));
                gfx.DrawImage(docImage, new Rectangle(0, 0, thumbwidth - (shadowSize + 2), thumbheight - (shadowSize + 2)), new Rectangle(0, 0, width, height), GraphicsUnit.Pixel);
            }
            else
            {
                gfx.FillRectangle(new HatchBrush(HatchStyle.DiagonalBrick, Color.AntiqueWhite, Color.White), new Rectangle(0, 0, thumbwidth - (shadowSize + 2), thumbheight - (shadowSize + 2)));
            }

            if (ReadyState != wsReady)
            {
                // Create tiled brushes for the shadow on the right and at the bottom.
                TextureBrush shadowRightBrush = new TextureBrush(webshot.serv.Properties.Resources.tshadowright, WrapMode.Tile);
                TextureBrush shadowDownBrush = new TextureBrush(webshot.serv.Properties.Resources.tshadowdown, WrapMode.Tile);

                // Translate (move) the brushes so the top or left of the image matches the top or left of the
                // area where it's drawed. If you don't understand why this is necessary, comment it out. 
                // Hint: The tiling would start at 0,0 of the control, so the shadows will be offset a little.
                shadowDownBrush.TranslateTransform(0, thumbheight - shadowSize);
                shadowRightBrush.TranslateTransform(thumbwidth - shadowSize, 0);

                // Define the rectangles that will be filled with the brush.
                // (where the shadow is drawn)
                Rectangle shadowDownRectangle =
                    new Rectangle(shadowSize + shadowMargin, thumbheight - shadowSize, thumbwidth - (shadowSize * 2 + shadowMargin), shadowSize);
                Rectangle shadowRightRectangle =
                    new Rectangle(thumbwidth - shadowSize, shadowSize + shadowMargin, shadowSize, thumbheight - (shadowSize * 2 + shadowMargin));

                // And draw the shadow on the right and at the bottom.
                gfx.FillRectangle(shadowDownBrush, shadowDownRectangle);
                gfx.FillRectangle(shadowRightBrush, shadowRightRectangle);

                // Now for the corners, draw the 3 5x5 pixel images.
                gfx.DrawImage(webshot.serv.Properties.Resources.tshadowtopright,
                    new Rectangle(thumbwidth - shadowSize, shadowMargin, shadowSize, shadowSize));

                gfx.DrawImage(webshot.serv.Properties.Resources.tshadowdownright,
                    new Rectangle(thumbwidth - shadowSize, thumbheight - shadowSize, shadowSize, shadowSize));

                gfx.DrawImage(webshot.serv.Properties.Resources.tshadowdownleft,
                    new Rectangle(shadowMargin, thumbheight - shadowSize, shadowSize, shadowSize));

                shadowRightBrush.Dispose();
                shadowDownBrush.Dispose();
            }

            ReadyState = wsReady;
        }
        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
        public byte ReadyState
        {
            get { return readyState; }
            set { readyState = value; }
        }
    }

    public class WebShotQueueWorker
    {
        private const int QS_ALLINPUT = 255;
        [DllImport("user32")]
        private static extern int GetQueueStatus(int fuFlags);

        public int count;
        private readonly object syncRoot;
        public Queue<WebShot> queue = new Queue<WebShot>();
        public Hashtable hash = new Hashtable();
        
        private IDictionary<string, ArrayList> data = new Dictionary<string, ArrayList>();

        public WebShotQueueWorker(int count)
        {
            this.count = count;
            syncRoot = new object();
            new Thread(new ThreadStart(this.Gather)).Start();

            for (int i = 1; i <= count; i++)
            {
                Thread thread = new Thread(new ThreadStart(this.Fetch));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Priority = ThreadPriority.Normal;
                thread.Start();
            }
        }
        public void Enqueue(WebShot webShot)
        {
            lock (this.syncRoot)
            {
                if (!hash.Contains(webShot.url))
                {
                    hash.Add(webShot.url, "enqueue");
                    queue.Enqueue(webShot);
                }
            }
        }
        private void Fetch()
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
                        Console.Title = "queue.Count: " + queue.Count + ", hash.Count: " + hash.Count;
                    }

                    if (webShot == null) Thread.Sleep(100);
                }

                using (WebBrowser webBrowser = new WebBrowser())
                {
                    webBrowser.Size = new Size(WebShot.width, WebShot.height);
                    webBrowser.ScrollBarsEnabled = false;
                    webBrowser.ScriptErrorsSuppressed = true;
                   // webBrowser.Silent
                    (SHDocVw.IWebBrowser2)webBrowser.Silent = true;
                    hash[webShot.url] = "fetching";

                    switch (webShot.ReadyState)
                    {
                        case WebShot.wsNotReady:
                            Console.Write("f");
                            break;
                        case WebShot.wsExpired:
                            Console.Write("u");
                            break;
                        case WebShot.wsTimeout:
                            Console.Write("r");
                            break;
                    }

                    webShot.HookEvents(webBrowser);
                    webBrowser.Navigate(webShot.url, false);

                    DateTime start = DateTime.Now;
                    while (webBrowser.ReadyState != WebBrowserReadyState.Complete)
                    {
                        if (GetQueueStatus(QS_ALLINPUT) != 0)
                        {
                            Application.DoEvents();
                        }
                        Thread.Sleep(500);

                        TimeSpan span = DateTime.Now - start;

                        if (span.Seconds >= 30)
                        {
                            webShot.ReadyState = WebShot.wsTimeout;
                            break;
                        }
                    }
                    webShot.Save();
                }

                hash.Remove(webShot.url);
            }
        }
        public XmlDocument GetXml()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement newnode, child;

            doc.Load(new StringReader(webshot.serv.Properties.Resources.index));
            //doc.Load(new XmlTextReader("./index.xml"));

            newnode = doc.CreateElement("version");
            newnode.InnerText = webshot.serv.Properties.Resources.Version;
            doc.DocumentElement.AppendChild(newnode);

            foreach (KeyValuePair<string, ArrayList> kvp in data)
            {
                newnode = doc.CreateElement(kvp.Key);
                foreach (string s in kvp.Value)
                {
                    child = doc.CreateElement("value");
                    child.InnerText = s;
                    newnode.AppendChild(child);
                }
                doc.DocumentElement.AppendChild(newnode);
            }

            newnode = doc.CreateElement("hash");
            foreach (string url in hash.Keys)
            {
                child = doc.CreateElement("value");
                child.SetAttribute("status", hash[url].ToString());
                child.InnerText = url;
                newnode.AppendChild(child);
            }

            doc.DocumentElement.AppendChild(newnode);

            return doc;
        }
        public void AddNode(string name, string value)
        {
            lock (data)
            {
                if (!data.ContainsKey(name)) data.Add(name, new ArrayList());

                data[name].Add(value);
                if (data[name].Count > 30) data[name].RemoveAt(0);
            }
        }
        private void Gather()
        {
            PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");

            while (true)
            {
                AddNode("processor", cpuCounter.NextValue().ToString());
                AddNode("memory", ramCounter.NextValue().ToString());
                Thread.Sleep(1000);
            }
        }
    }
}
