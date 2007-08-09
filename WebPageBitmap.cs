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
    class WebShot
    {
        private WebBrowser webBrowser;
        private Bitmap docThumbnail;
        private Bitmap docImage;
        private Rectangle docRect;
        private ImageCodecInfo codec;
        private EncoderParameters codecParams;

        private int width;
        private int height;
        private int thumbwidth;
        private int thumbheight;
        private string rawurl;
        public string url;
        public bool isReady = false;
        
        public WebShot(string url)
        {
            int val;
            this.width = 1024;
            this.height = 768;
            this.rawurl = url;

            NameValueCollection QueryString = HttpUtility.ParseQueryString(url.Replace("/?", "").Replace("http://", ""));

            this.url = QueryString["url"];
            
            if (int.TryParse(QueryString["w"], out val)) { this.thumbwidth = val; } else { this.thumbwidth = 240; }
            if (int.TryParse(QueryString["h"], out val)) { this.thumbheight = val; } else { this.thumbheight = 170; }

            this.codec = GetEncoderInfo("image/png");
            this.codecParams = new EncoderParameters(1);
            this.codecParams.Param[0] = new EncoderParameter(Encoder.Quality, 90L);

            this.docImage = new Bitmap(width, height);
            this.docRect = new Rectangle(0, 0, width, height);
            this.docThumbnail = new Bitmap(thumbwidth, thumbheight);

            //getting image
            FileInfo file = new FileInfo(url2file(url));
            TimeSpan span = DateTime.Now - file.CreationTime;
            
            if (!file.Exists || span.TotalDays > 7 || QueryString["force"] == "true")
            {
                this.isReady = false;
            }
            else
            {
                this.docThumbnail = new Bitmap(file.ToString(), true);
                this.isReady = true;
            }

        }

        public void Fetch()
        {
            Console.WriteLine("[{0}] Fetch: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), url);
      
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

                    TimeSpan span = DateTime.Now - start;

                    if (span.Seconds >= 30 * 2)
                    {
                        webBrowser.Stop();
                        Thread.Sleep(1000);
                    }

                    if (span.Seconds >= 60 * 2)
                    {
                        break;
                    }
                }

                if (this.isReady)
                {
                    Save(url2file(rawurl));
                }
                else
                {
                    this.isReady = true;
                }

                //webBrowser.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message + e.StackTrace);
            }
        }

        private void Save(string fileName)
        {
            Console.WriteLine("[{0}] Save: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), fileName);
 
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            docThumbnail.Save(fileName, this.codec, this.codecParams);
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

        public byte[] GetStream()
        {
            MemoryStream stream = new MemoryStream();
            docThumbnail.Save(stream, this.codec, this.codecParams);
            return stream.ToArray();
        }

        private string url2file(string url)
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

        public static ImageCodecInfo GetEncoderInfo(String mimeType)
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
    }
}
