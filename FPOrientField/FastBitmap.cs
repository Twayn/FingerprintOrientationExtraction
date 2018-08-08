using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace FPOrientField {
    public class FastBitmap {
        private readonly Bitmap _source;
        private IntPtr _iptr = IntPtr.Zero;
        private BitmapData _bitmapData;

        private byte[] _pixels;
        private int _depth;
        public int Width;
        public int Height;

        public FastBitmap(string imagePath) {
            _source = new Bitmap(imagePath);
            LockBits();
        }
        
        public FastBitmap(Bitmap source) {
            _source = source;
            LockBits();
        }

        //~FastBitmap() { UnlockBits(); }

        private void LockBits() {
            Width = _source.Width;
            Height = _source.Height;

            var pixelCount = Width * Height;
            var rect = new Rectangle(0, 0, Width, Height);

            _depth = Image.GetPixelFormatSize(_source.PixelFormat);

            if (_depth != 8 && _depth != 24 && _depth != 32) {
                throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
            }

            _bitmapData = _source.LockBits(rect, ImageLockMode.ReadWrite, _source.PixelFormat);

            var step = _depth / 8;
            _pixels = new byte[pixelCount * step];
            _iptr = _bitmapData.Scan0;

            Marshal.Copy(_iptr, _pixels, 0, _pixels.Length);
        }

        private void UnlockBits() {
            _source.UnlockBits(_bitmapData);
        }

        private byte GetBrightness(int x, int y) {
            var cCount = _depth / 8;
            var i = (y * Width + x) * cCount;
            if (i > _pixels.Length - cCount)
                throw new IndexOutOfRangeException();

            if (_depth != 8) {
                return (byte)((_pixels[i] + _pixels[i + 1] + _pixels[i + 2]) / 3);
            }

            return _pixels[i];
        }

        public byte[,] GetByteArray(){
            var result = new byte[Width, Height];

            for (var i = 0; i < Width; i++){
                for (var j = 0; j < Height; j++) {
                    result[i, j] = GetBrightness(i, j);
                }
            }

            return result;
        }
        
        public int[,] GetIntArray(){
            var result = new int[Width, Height];

            for (var i = 0; i < Width; i++){
                for (var j = 0; j < Height; j++) {
                    result[i, j] = GetBrightness(i, j);
                }
            }

            return result;
        }
    }
}
