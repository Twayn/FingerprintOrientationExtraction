using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace FPOrientField
{
    public static class BitmapViewer
    {
        private static int _counter = 0;
        private static Bitmap ArrayToBitmap(double[,] source) {
            var result = new Bitmap(source.GetLength(0), source.GetLength(1));
            
            var max = source.Cast<double>().Max();
            var equalizer = max/255.0d;
            
            for (var x = 0; x < source.GetLength(0); x++) {
                for (var y = 0; y < source.GetLength(1); y++) {
                    result.SetPixel(x, y, ToColor(source[x, y]/equalizer));
                }
            }

            return result;
        }
        
        private static Bitmap ArrayToBitmap(int[,] source) {
            var result = new Bitmap(source.GetLength(0), source.GetLength(1));
            var equalizer = source.Cast<int>().Max()/255.0d;
            
            for (var x = 0; x < source.GetLength(0); x++) {
                for (var y = 0; y < source.GetLength(1); y++) {
                    result.SetPixel(x, y, ToColor(source[x, y]/equalizer));
                }
            }

            return result;
        }

        private static Bitmap ArrayToBitmap(byte[,] source) {
            var result = new Bitmap(source.GetLength(0), source.GetLength(1));
            
            for (var x = 0; x < source.GetLength(0); x++) {
                for (var y = 0; y < source.GetLength(1); y++) {
                    result.SetPixel(x, y, ToColor(source[x, y]));
                }
            }

            return result;
        }

        public static void Save(double[,] source) {
            _counter++;
            ArrayToBitmap(source).Save("_"+_counter+"Image.bmp");
        }
        
        public static void Save(int[,] source) {
            _counter++;
            ArrayToBitmap(source).Save("_"+_counter+"Image.bmp");
        }
        
        public static void Save(byte[,] source) {
            _counter++;
            ArrayToBitmap(source).Save("_"+_counter+"Image.bmp");
        }
        
        public static void Save(Bitmap source) {
            _counter++;
            source.Save("_"+_counter+"Image.bmp");
        }
        
        private static Color ToColor(double val) {
            return Color.FromArgb(Convert.ToInt32(val), Convert.ToInt32(val), Convert.ToInt32(val));
        }
        
        private static Color ToColor(byte val) {
            return Color.FromArgb(Convert.ToInt32(val), Convert.ToInt32(val), Convert.ToInt32(val));
        }
        
        private static Color ToColor(int val) {
            return Color.FromArgb(val, val, val);
        }
       
        public static Bitmap DoubleSize(string path){
            var source = new Bitmap(path);
            var res = new Bitmap(source.Width * 2, source.Height * 2);
            using (var gr = Graphics.FromImage(res)){
                gr.InterpolationMode = InterpolationMode.HighQualityBilinear;
                gr.DrawImage(source, 0, 0, source.Width * 2, source.Height * 2);
            }
 
            return res;
        }
    }
}