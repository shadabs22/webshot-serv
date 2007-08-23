using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace FastImage
{
    public unsafe class FastBitmap
    {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public struct PixelData
        {
            internal byte blue;
            internal byte green;
            internal byte red;
        }

        private Bitmap Subject;
        private int SubjectWidth;
        private BitmapData bitmapData;
        private Byte* pBase = null;

        public FastBitmap(Bitmap mySubjectBitmap)
        {
            this.Subject = mySubjectBitmap;
            try
            {
                LockBitmap();
            }
            catch (Exception)
            {
                throw;
            }

        }

        public void Release()
        {
            try
            {
                UnlockBitmap();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Bitmap Bitmap
        {
            get
            {
                return Subject;
            }
        }

        public void SetPixel(int x, int y, Color myColour)
        {
            try
            {
                PixelData* p = PixelAt(x, y);
                p -> red = myColour.R;
                p -> green = myColour.G;
                p -> blue = myColour.B;
            }
            catch (AccessViolationException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Color GetPixel(int x, int y)
        {
            try
            {
                PixelData* p = PixelAt(x, y);
                return Color.FromArgb((int)p->red, (int)p->green, (int)p->blue);
            }
            catch (AccessViolationException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void LockBitmap()
        {
            GraphicsUnit unit = GraphicsUnit.Pixel;
            RectangleF boundsF = Subject.GetBounds(ref unit);
            Rectangle bounds = new Rectangle((int)boundsF.X,
                (int)boundsF.Y,
                (int)boundsF.Width,
                (int)boundsF.Height);

            SubjectWidth = (int)boundsF.Width * sizeof(PixelData);
            if (SubjectWidth % 4 != 0)
            {
                SubjectWidth = 4 * (SubjectWidth / 4 + 1);
            }

            bitmapData = Subject.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            pBase = (Byte*)bitmapData.Scan0.ToPointer();
        }

        private PixelData* PixelAt(int x, int y)
        {
            return (PixelData*)(pBase + y * SubjectWidth + x * sizeof(PixelData));
        }

        private void UnlockBitmap()
        {
            Subject.UnlockBits(bitmapData);
            bitmapData = null;
            pBase = null;
        }
    }

}