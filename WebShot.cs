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
using Gif.Components;
using System.Resources;

namespace GetSiteThumbnail
{
    class WebShot
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

        public byte ReadyState;

        public const byte wsNotReady = 0;
        public const byte wsReady = 1;
        public const byte wsExpired = 2;
        public const byte wsTimeout = 3;
        
        public WebShot(NameValueCollection q)
        {
                //Качество
                codecParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);
                           
                //Размеры тамбнейла
                try
                {
                    thumbwidth = int.Parse(q["w"]);
                    thumbheight = int.Parse(q["h"]);
                }
                catch
                {  }

                //Уменьшенная картинка
                docThumbnail = new Bitmap(thumbwidth, thumbheight);

                gfx = Graphics.FromImage(docThumbnail);
                gfx.CompositingQuality = CompositingQuality.HighQuality;
                gfx.SmoothingMode = SmoothingMode.HighQuality;
                gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                
                gfx.Clear(Color.White);

                try
                {
                    Uri uri = new Uri("http://" + q["url"].Replace("http://", ""));

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
        public void Fetch(int timeout)
        {
            /*
            //Проверка ресурса на доступность
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
            //httpRequest.Method = "HEAD";

            HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            StreamReader readStream = new StreamReader(httpResponse.GetResponseStream());

            this.docSize = readStream.ReadToEnd().Length;

            httpResponse.Close();
            readStream.Close();
            */
           
            using(WebBrowser webBrowser = new WebBrowser())
            {
                webBrowser.Size = new Size(width, height);
                webBrowser.ScrollBarsEnabled = false;
                webBrowser.ScriptErrorsSuppressed = true;
                webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(documentCompletedEventHandler);
                webBrowser.ProgressChanged += new WebBrowserProgressChangedEventHandler(documentProgressChangedEventHandler);
                webBrowser.NewWindow += new CancelEventHandler(documentCancelEventHandler);

                if (ReadyState == wsNotReady)
                {
                    Console.Write("f");
                }
                if (ReadyState == wsExpired)
                {
                    Console.Write("u");
                }
                //Console.WriteLine("[{0}] Fetch: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), url);

                webBrowser.Navigate(url, false);

                DateTime start = DateTime.Now;
                while (webBrowser.ReadyState != WebBrowserReadyState.Complete)
                {
                    Application.DoEvents();
                    Thread.Sleep(100);

                    TimeSpan span = DateTime.Now - start;

                    if (span.Seconds >= timeout)
                    {
                        ReadyState = wsTimeout;
                        break;
                    }
                }
            }
                        
            Save();
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
        private void Save()
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
            {  }
        }
        private void documentProgressChangedEventHandler(object sender, WebBrowserProgressChangedEventArgs e)
        {
            //WebBrowser webBrowser = sender as WebBrowser;
            //Font fnt = new Font("Tahoma", 11);
            //gfx.DrawString((int)(((double)webBrowser.DocumentStream.Length / (double)this.docSize) * 100) + "%", fnt, new SolidBrush(Color.Black), 10,10);
            //Console.WriteLine((int)(((double)webBrowser.DocumentStream.Length / (double)this.docSize) * 100) + "%, " + (int)(((double)e.CurrentProgress / (double)e.MaximumProgress) * 100)+"%");
        }
        private void documentCancelEventHandler(object sender, CancelEventArgs e)
        {
            // Don't want pop-ups.
            e.Cancel = true;
        }
        private void documentCompletedEventHandler(object sender, WebBrowserDocumentCompletedEventArgs ex)
        {
            int shadowSize = 5;
            int shadowMargin = 2;

            WebBrowser webBrowser = sender as WebBrowser;

            if (webBrowser.Document.Url.ToString().IndexOf("shdoclc.dll") == -1)
            {
                //Целая картинка
                Bitmap docImage = new Bitmap(width, height);

                webBrowser.DrawToBitmap(docImage, new Rectangle(0, 0, width, height));
                gfx.DrawImage(docImage, new Rectangle(0, 0, thumbwidth - (shadowSize + 2), thumbheight - (shadowSize + 2)),new Rectangle(0, 0, width, height),GraphicsUnit.Pixel);
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
