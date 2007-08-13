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
using System.Text;

namespace GetSiteThumbnail
{
    class WebShot
    {
        private Bitmap docImage;
        private Bitmap docThumbnail;
        private Rectangle docRect;
        private ImageCodecInfo codec;
        private EncoderParameters codecParams;
        private int thumbwidth = 240;
        private int thumbheight = 170;

        public static int width = 1024;
        public static int height = 768;

        public string url = null;
        private string rawurl;
        public bool isReady = false;
        public bool isTimeout = false;

        public WebShot(string url)
        {
            int val;
            codec = GetEncoderInfo("image/png");
            codecParams = new EncoderParameters(1);
            codecParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);
            
            docImage = new Bitmap(width, height);
            docRect = new Rectangle(0, 0, width, height);

            NameValueCollection q = HttpUtility.ParseQueryString(url.Replace("/?", "").Replace("http://", ""));
                       
            if (int.TryParse(q["w"], out val)) thumbwidth = val;
            if (int.TryParse(q["h"], out val)) thumbheight = val;

            docThumbnail = new Bitmap(thumbwidth, thumbheight);

            try
            {
                new Uri("http://" + q["url"].Replace("http://", ""));
                rawurl = url;
                
                this.url = q["url"];
                
                //getting image
                FileInfo file = new FileInfo(url2file(url));
                TimeSpan span = DateTime.Now - file.CreationTime;

                if (file.Exists && span.TotalDays <= 7 && q["force"] != "true")
                {
                    docThumbnail = new Bitmap(file.ToString(), true);
                    isReady = true;
                }
            }   
            catch (Exception e)
            {
                return;
            }
        }

        public void Fetch(int timeout)
        {
            try
            {
                WebBrowser webBrowser = new WebBrowser();
                webBrowser.NewWindow += new CancelEventHandler(documentCancelEventHandler);

                webBrowser.Size = new Size(WebShot.width, WebShot.height);
                webBrowser.ScrollBarsEnabled = false;
                webBrowser.ScriptErrorsSuppressed = true;
                webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(documentCompletedEventHandler);

                try
                {
                    Console.WriteLine("[{0}] Fetch: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), url);
                    webBrowser.Navigate(url, false);

                    DateTime start = DateTime.Now;
                    while (webBrowser.ReadyState != WebBrowserReadyState.Complete)
                    {
                        Application.DoEvents();

                        TimeSpan span = DateTime.Now - start;

                        if (span.Seconds >= timeout)
                        {
                            isTimeout = true;
                            documentCompletedEventHandler(webBrowser, new WebBrowserDocumentCompletedEventArgs(new Uri("http://" + url)));
                            break;
                        }
                    }

                    if (isReady)
                    {
                        Save();
                    }
                }
                finally
                {
                    webBrowser.Dispose();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message + e.StackTrace);
            }
        }

        public void Save(Stream output)
        {
            MemoryStream stream = new MemoryStream();
            docThumbnail.Save(stream, codec, codecParams);
            
            byte[] buffer = stream.ToArray();

            try
            {
                 output.Write(buffer, 0, buffer.Length);
            }
            catch (System.Net.HttpListenerException ex)
            {   // client closed connection
                // 1. ErrorCode=1229. An operation was attempted on a nonexistent network connection.
                // 2. ErrorCode=64. The specified network name is no longer available.
                return;
            }
        }

        public void Save()
        {
            string fileName = url2file(this.rawurl);
            Console.WriteLine("[{0}] Save: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            docThumbnail.Save(fileName, this.codec, this.codecParams);
        }

        private void documentCancelEventHandler(object sender, CancelEventArgs e)
        {
            // Don't want pop-ups.
            e.Cancel = true;
        }

        public void documentCompletedEventHandler(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //if (webBrowser.ReadyState != WebBrowserReadyState.Complete) return;
            WebBrowser webBrowser = sender as WebBrowser;

            Graphics gfx = Graphics.FromImage(docThumbnail);

            gfx.CompositingQuality = CompositingQuality.HighQuality;
            gfx.SmoothingMode = SmoothingMode.HighQuality;
            gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
            
            if (webBrowser.Document.Url.ToString().IndexOf("shdoclc.dll") == -1)
            {
                webBrowser.DrawToBitmap(docImage, docRect);
                gfx.DrawImage(docImage, new Rectangle(0, 0, thumbwidth, thumbheight), docRect, GraphicsUnit.Pixel);
            }
            else
            {
                gfx.FillRectangle(new HatchBrush(HatchStyle.HorizontalBrick, Color.Black, Color.White), new Rectangle(0, 0, thumbwidth, thumbheight));
            }

            isReady = true;
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

        public static string url2file(string url)
        {
            NameValueCollection q;
            try
            {
                int val;

                q = HttpUtility.ParseQueryString(url.Replace("/?", "").Replace("http://", ""));

                if (!int.TryParse(q["w"], out val)) q.Add("w", "240");
                if (!int.TryParse(q["h"], out val)) q.Add("h", "180");

                return "./webshots/" + q["url"].Replace("http://", "").Replace("/", "_").Replace("\\", "_") + "_" + q["w"] + "x" + q["h"] + ".png";
            }
            catch (Exception)
            {
                return "./fixme.png";
            }
        }

        
    }
}
