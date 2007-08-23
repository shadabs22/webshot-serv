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
using FastImage;

namespace GetSiteThumbnail
{
    class WebShot
    {
        private Bitmap docImage;
        private Bitmap docThumbnail;
        private Graphics gfx;

        private ImageCodecInfo codec = GetEncoderInfo("image/png");
        private EncoderParameters codecParams = new EncoderParameters(1);
        
        private int thumbwidth = 160;
        private int thumbheight = 96;

        private int shadowSize = 5;
        private int shadowMargin = 2;

        public static int width = 1600;
        public static int height = 960;
              
        private string fileName;
        public string url;
        public bool isReady;
        public bool isTimeout;
   
        public WebShot(string url)
        {
            try
            {
                int val;
                NameValueCollection q = HttpUtility.ParseQueryString(url.Replace("/?", "").Replace("http://", ""));

                //Качество
                codecParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);

                //Целая картинка
                docImage = new Bitmap(width, height);
                
                if (int.TryParse(q["w"], out val)) thumbwidth = val;
                if (int.TryParse(q["h"], out val)) thumbheight = val;

                //Уменьшенная картинка
                docThumbnail = new Bitmap(thumbwidth, thumbheight);

                gfx = Graphics.FromImage(docThumbnail);
                gfx.CompositingQuality = CompositingQuality.HighQuality;
                gfx.SmoothingMode = SmoothingMode.HighQuality;
                gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                
                Uri uri = new Uri("http://" + q["url"].Replace("http://", ""));

                //Имя файла в кеше
                fileName = "./webshots/" + uri.ToString().Replace("http://", "").Replace("/", "_").Replace("\\", "_") + "_" + thumbwidth + "x" + thumbheight + ".png";
                
                FileInfo file = new FileInfo(fileName);
                TimeSpan span = DateTime.Now - file.CreationTime;

                if (file.Exists && span.TotalDays <= 7 && q["force"] != "true")
                {
                    docThumbnail = new Bitmap(file.ToString(), true);
                    isReady = true;
                    return;
                }
                
                try
                {
                    //Проверка ресурса на доступность
                    //HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(uri);
                    //httpRequest.Method = "HEAD";
                    //httpRequest.GetResponse();

                    this.url = uri.ToString(); 
                }
                catch (WebException)
                {
                    gfx.FillRectangle(new HatchBrush(HatchStyle.DiagonalBrick, Color.AntiqueWhite, Color.White), new Rectangle(0, 0, thumbwidth, thumbheight));
                }
            }   
            catch (Exception)
            {
                gfx.FillRectangle(new HatchBrush(HatchStyle.HorizontalBrick, Color.AntiqueWhite, Color.White), new Rectangle(0, 0, thumbwidth, thumbheight));
            }
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.DateTime.ToString(System.String)")]
        public void Fetch(int timeout)
        {
            WebBrowser webBrowser = new WebBrowser();
            try
            {
                webBrowser.Size = new Size(WebShot.width, WebShot.height);
                webBrowser.ScrollBarsEnabled = false;
                webBrowser.ScriptErrorsSuppressed = true;
                webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(documentCompletedEventHandler);
                webBrowser.NewWindow += new CancelEventHandler(documentCancelEventHandler);

                Console.WriteLine("[{0}] Fetch: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), url);

                webBrowser.Navigate(url, false);

                DateTime start = DateTime.Now;
                while (webBrowser.ReadyState != WebBrowserReadyState.Complete)
                {
                    Application.DoEvents();
                    Thread.Sleep(100);

                    TimeSpan span = DateTime.Now - start;

                    if (span.Seconds >= timeout)
                    {
                        isTimeout = true;
                        break;
                    }
                }
            }
            finally
            {
                webBrowser.Dispose();
                GC.Collect();
            }

            Save();
        }
        public void Save(Stream output)
        {
            if (docThumbnail == null) return;

            MemoryStream stream = new MemoryStream();
            docThumbnail.Save(stream, codec, codecParams);
            
            byte[] buffer = stream.ToArray();

            try
            {
                 output.Write(buffer, 0, buffer.Length);
            }
            catch (System.Net.HttpListenerException)
            {   
                // client closed connection
                // 1. ErrorCode=1229. An operation was attempted on a nonexistent network connection.
                // 2. ErrorCode=64. The specified network name is no longer available.
            }

            stream.Dispose();
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.DateTime.ToString(System.String)")]
        private void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(this.fileName));
                docThumbnail.Save(this.fileName, this.codec, this.codecParams);
                Console.WriteLine("[{0}] Save: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), this.fileName);
            }
            catch (ExternalException)
            {

            }
        }
        private void documentCancelEventHandler(object sender, CancelEventArgs e)
        {
            // Don't want pop-ups.
            e.Cancel = true;
        }
        private void documentCompletedEventHandler(object sender, WebBrowserDocumentCompletedEventArgs ex)
        {
            WebBrowser webBrowser = sender as WebBrowser;

            webBrowser.DrawToBitmap(docImage, new Rectangle(0, 0, width, height));
            gfx.Clear(Color.White);

            //Shadow
            gfx.DrawImage(docImage,
                          new Rectangle(0, 0, thumbwidth - (shadowSize + 2), thumbheight - (shadowSize + 2)),
                          new Rectangle(0, 0, width, height),
                          GraphicsUnit.Pixel
                         );

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

            isReady = true;
        }
        public static void XorBitmap(Stream output, int w, int h)
        {
            Bitmap XorBitmap = new Bitmap(w, h);
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

            try
            {
                output.Write(buffer, 0, buffer.Length);
            }
            catch (System.Net.HttpListenerException)
            {   
                // client closed connection
                // 1. ErrorCode=1229. An operation was attempted on a nonexistent network connection.
                // 2. ErrorCode=64. The specified network name is no longer available.
            }

            stream.Dispose();
            codecParams.Dispose();
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
    }
}
