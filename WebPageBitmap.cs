using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Web;


namespace GetSiteThumbnail
{
    class WebShotQueue
    {
        private Queue<string> queue = new Queue<string>();
        private HttpListenerContext context;

        public WebShotQueue(HttpListenerContext context)
        {
            this.context = context;
            for (int i = 1; i <= 5; i++)
            {
                Thread thread = new Thread(new ThreadStart(Fetch));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
        }

        public void Enqueue(string url)
        {
            queue.Enqueue(url);
        }

        public void Fetch()
        {
            if (queue.Count > 0)
            {
                string url = queue.Dequeue();
                WebPageBitmap webBitmap = new WebPageBitmap(this.context);
            }
        }
    }

    class WebPageBitmap
    {
        private WebBrowser webBrowser;
        private Bitmap docThumbnail;
        private Bitmap docImage;
        private Rectangle docRect;
        private int width;
        private int height;
        private int thumbwidth;
        private int thumbheight;
        private string url;
        private string rawurl;
        Queue<HttpListenerContext> queue = new Queue<HttpListenerContext>();
        public bool isReady = false;

        public WebPageBitmap(HttpListenerContext context)
        {
            int val;
            this.width = 1024;
            this.height = 768;
            this.url = context.Request.QueryString["url"];
            this.rawurl = context.Request.RawUrl;

            if (int.TryParse(context.Request.QueryString["w"], out val)) { this.thumbwidth = val; } else { this.thumbwidth = 240; }
            if (int.TryParse(context.Request.QueryString["h"], out val)) { this.thumbheight = val; } else { this.thumbheight = 170; }
        }

        public static string url2file(string url)
        {
            NameValueCollection QueryString;
            try
            {
                int val;

                QueryString = HttpUtility.ParseQueryString(url.Replace("/?", "").Replace("http://", ""));

                if (!int.TryParse(QueryString["w"], out val)) QueryString.Add("w", "240");
                if (!int.TryParse(QueryString["h"], out val)) QueryString.Add("h", "180");

                return "./webshots/" + QueryString["url"].Replace("http://", "").Replace("/", "_").Replace("\\", "_") + "_" + QueryString["w"] + "x" + QueryString["h"] + ".png";
            }
            catch (Exception)
            {
                return "./time.png";
            }
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

        private void Fetch(string url)
        {
            Console.WriteLine("[{0}] WebPageBitmap.Fetch: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), url);

            docImage = new Bitmap(width, height);
            docRect = new Rectangle(0, 0, width, height);
            docThumbnail = new Bitmap(thumbwidth, thumbheight);

            try
            {
                webBrowser = new WebBrowser();
                webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(documentCompletedEventHandler);
                webBrowser.NewWindow += new CancelEventHandler(documentCancelEventHandler);

                webBrowser.Size = new Size(width, height);
                webBrowser.ScrollBarsEnabled = false;
                webBrowser.ScriptErrorsSuppressed = true;

                webBrowser.Navigate(url);

                DateTime start = DateTime.Now;
                while (webBrowser.ReadyState != WebBrowserReadyState.Complete)
                {
                    Application.DoEvents();

                    TimeSpan span1 = DateTime.Now - start;

                    if (span1.Seconds >= 60)
                    {
                        //Console.WriteLine("[{0}] 60 seconds passed, triggering Stop()", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        webBrowser.Stop();
                        Thread.Sleep(1000);
                    }

                    if (span1.Seconds >= 120)
                    {
                        //Console.WriteLine("[{0}] 120 seconds passed, breaking", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        break;
                    }
                }

                SaveThumbnail(url2file(rawurl), 90L);
                //webBrowser.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message + e.StackTrace);
            }
        }


        private void SaveThumbnail(string fileName, long q)
        {
            Console.WriteLine("[{0}] HttpWorker.SaveThumbnail: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), fileName);

            ImageCodecInfo codec = GetEncoderInfo("image/png");
            EncoderParameters codecParams = new EncoderParameters(1);
            codecParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, q);

            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            docThumbnail.Save(fileName, codec, codecParams);
        }
        private void documentCancelEventHandler(object sender, CancelEventArgs e)
        {
            // Don't want pop-ups.
            e.Cancel = true;
        }
        private void documentCompletedEventHandler(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //if (webBrowser.ReadyState != WebBrowserReadyState.Complete) return;
            if (webBrowser.Document.Url.ToString().IndexOf("shdoclc.dll") != -1) return;

            webBrowser.DrawToBitmap(docImage, docRect);

            Graphics gfx = Graphics.FromImage(docThumbnail);

            gfx.CompositingQuality = CompositingQuality.HighQuality;
            gfx.SmoothingMode = SmoothingMode.HighQuality;
            gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;

            gfx.DrawImage(docImage, new Rectangle(0, 0, this.thumbwidth, this.thumbheight), docRect, GraphicsUnit.Pixel);

            isReady = true;
        }
    }
}
