using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;

namespace Demon
{
    [Guid("3050f669-98b5-11cf-bb82-00aa00bdce0b")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    [ComImport]
    interface IHTMLElementRender
    {
        void DrawToDC([In] IntPtr hDC);

        void SetDocumentPrinter([In, MarshalAs(UnmanagedType.BStr)] string bstrPrinterName, [In] IntPtr hDC);
    };
}

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

            webBrowser = new WebBrowser();
            webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(documentCompletedEventHandler);
            //webBrowser.Navigating += new WebBrowserNavigatingEventHandler(documentNavigatingEventHandler);

            webBrowser.Size = new Size(width, height);
            webBrowser.ScrollBarsEnabled = scrollBarsEnabled;

            webBrowser.Url = new Uri(url);

            DateTime start = DateTime.Now;
            while (webBrowser.ReadyState != WebBrowserReadyState.Complete)
            {
                Application.DoEvents();

                TimeSpan span = DateTime.Now - start;

                if (span.Seconds > 30)
                {
                    webBrowser.Stop();
                }
            }
        }

        ~WebPageBitmap()
        {
            webBrowser.Dispose();
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
            WebBrowser browser = (sender as WebBrowser);

            if (browser != null)
            {
                mshtml.IHTMLDocument2 document = (browser.Document.DomDocument as mshtml.IHTMLDocument2);
                if (document != null)
                {
                    if (document.url.IndexOf("shdoclc.dll") == -1)
                    {
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


        }
    }
}
