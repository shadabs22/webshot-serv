using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;

namespace GetSiteThumbnail
{
    class WebPageBitmap
    {
        public WebBrowser webBrowser;
        private Bitmap docThumbnail;
        private Bitmap docImage;
        private Rectangle docRect;
        private string url;
        private int width;
        private int height;
        private int thumbwidth;
        private int thumbheight;
        public bool isReady = false;

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

        public WebPageBitmap(string url, int thumbwidth, int thumbheight, bool scrollBarsEnabled)
        {
            this.url = url;
            this.width = 1027;
            this.height = 768;

            this.thumbwidth = thumbwidth;
            this.thumbheight = thumbheight;
          
            docImage = new Bitmap(width, height);
            docRect = new Rectangle(0, 0, width, height);
            docThumbnail = new Bitmap(thumbwidth, thumbheight);

            try 
            {
                webBrowser = new WebBrowser();
                webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(documentCompletedEventHandler);
                //webBrowser.Navigating += new WebBrowserNavigatingEventHandler(documentNavigatingEventHandler);

                webBrowser.Size = new Size(width, height);
                webBrowser.ScrollBarsEnabled = scrollBarsEnabled;

                UriBuilder uri = new UriBuilder("http", url);

                webBrowser.Url = uri.Uri;
                
                DateTime start = DateTime.Now;
                while (webBrowser.ReadyState != WebBrowserReadyState.Complete)
                {
                    Application.DoEvents();

                    //Console.WriteLine(webBrowser.DocumentText.Length);
                    
                    TimeSpan span = DateTime.Now - start;
                    if (span.Seconds == 20)
                    {
                        Console.WriteLine("[{0}] 20 seconds passed, triggering Stop()", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        webBrowser.Stop();
                    }

                    if (span.Seconds >= 30) break;
                }

                webBrowser.Dispose();
            }
		    catch (Exception e) 
		    {
			    Console.WriteLine("Error: "+e.Message+e.StackTrace);
		    }
        }

        public void SaveThumbnail(string fileName, long q)
        {
            ImageCodecInfo codec = GetEncoderInfo("image/png");
            EncoderParameters codecParams = new EncoderParameters(1);
            codecParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, q);

            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            docThumbnail.Save(fileName, codec, codecParams);
        }

        //private void documentNavigatingEventHandler(object sender, WebBrowserNavigatingEventArgs e){}
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
