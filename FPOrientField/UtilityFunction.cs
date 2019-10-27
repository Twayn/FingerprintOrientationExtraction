using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FPOrientField{
    internal static class UtilityFunctions{
        private const int AllowedBorder = 14;
        
        private static int[,] _pointModule;
        private static int[,] _pointAngle;

        private static int[,] _areaModule;
        private static int[,] _areaAngle;

        private static int[,] _complex;
//        private static int _threshold;
//
//        private static int[,] _qualityMeasure;

        public static void SetInitialData(string imagePath, Grid grid, System.IO.StreamWriter res){            
            var interpolated = new FastBitmap(BitmapViewer.DoubleSize(imagePath));
            var interpolatedArray = interpolated.GetIntArray();
            var interpolatedBlured = BlurInterpolated(interpolatedArray, 2);

            var source = new FastBitmap(imagePath);
            var sourceArray = source.GetIntArray();
            var sourceBlured = Blur(sourceArray, 2);

            _pointModule = new int[source.Width, source.Height];
            _pointAngle = new int[source.Width, source.Height];
            _areaAngle = new int[source.Width, source.Height];
            _areaModule = new int[source.Width, source.Height];

            CalcGradientAtPoint(interpolatedBlured);
            CalcDirectionAtArea(AllowedBorder);

            BitmapViewer.Save(sourceArray);
            BitmapViewer.Save(sourceBlured);
            BitmapViewer.Save(RidgeDensity(sourceBlured, 8, 16));
            BitmapViewer.Save(interpolatedArray);
            BitmapViewer.Save(interpolatedBlured);
            BitmapViewer.Save(_pointAngle);
            BitmapViewer.Save(_pointModule);
            BitmapViewer.Save(_areaAngle);
            
            var bluredAlongRidgeDirection = BlurAlongRidgeDirection(sourceArray, 16, 8);
            BitmapViewer.Save(bluredAlongRidgeDirection);
            var tmp = FFT(bluredAlongRidgeDirection, 8, 16, res);
            BitmapViewer.Save(tmp);

            Grid.SetGradientElements(_pointModule, _pointAngle);

            Grid.Layer1 = grid.CalcQualityMeasure(new Grid.Coherence(), 8);
            Grid.Layer2 = grid.CalcQualityMeasure(new Grid.Coherence(), 10);
            Grid.Layer3 = grid.CalcQualityMeasure(new Grid.Coherence(), 12);
            Grid.Layer4 = grid.CalcQualityMeasure(new Grid.Coherence(), 14);
            Grid.Layer5 = grid.CalcQualityMeasure(new Grid.AverageModule(), 8);
            Grid.Layer6 = grid.CalcQualityMeasure(new Grid.AverageModule(), 10);
            Grid.Layer7 = grid.CalcQualityMeasure(new Grid.AverageModule(), 12);
            Grid.Layer8 = grid.CalcQualityMeasure(new Grid.AverageModule(), 14);
                 
            BitmapViewer.Save(Grid.Layer1);
            BitmapViewer.Save(Grid.Layer2);
            BitmapViewer.Save(Grid.Layer3);
            BitmapViewer.Save(Grid.Layer4);
            BitmapViewer.Save(Grid.Layer5);
            BitmapViewer.Save(Grid.Layer6);
            BitmapViewer.Save(Grid.Layer7);
            BitmapViewer.Save(Grid.Layer8);
            
            BitmapViewer.Save(grid.CalcQualityMeasure(new Grid.CoherenceDiff(), 0));
            BitmapViewer.Save(grid.CalcQualityMeasure(new Grid.ModuleDiff(), 0));

          _complex = grid.CalcQualityMeasure(new Grid.ComplexQuality(), 0);
//          _threshold = grid.Threshold(_complex);
            
          BitmapViewer.Save(_complex);
//          _qualityMeasure = grid.CalcQualityMeasureOriginalSize(new Grid.ComplexQuality(), 0);
        }

//        public static int[,] GetQualityMeasure(){
//            return _qualityMeasure;
//        }
//        
//        public static int GetThreshold(){
//            return _threshold;
//        }

        public static byte GetDirectionAtPoint(int px, int py){
            return AverageDirectionInArea(AllowedBorder, px, py);
        }

        private static void CalcDirectionAtArea(int area){
            Parallel.For(area, _pointModule.GetLength(0) - area, x => {
                for (var y = area; y < _pointAngle.GetLength(1) - area; y++){
                    var sumX = 0.0d;
                    var sumY = 0.0d;

                    for (var w = x - area; w < x + area; w++){
                        for (var z = y - area; z < y + area; z++){
                            sumX += _pointModule[w, z] * FastTrigon.FastCos[2 * _pointAngle[w, z]];
                            sumY += _pointModule[w, z] * FastTrigon.FastSin[2 * _pointAngle[w, z]];
                        }
                    }

                    _areaModule[x, y] = (int) Math.Sqrt(sumX * sumX + sumY * sumY);
                    _areaAngle[x, y] = (int) (180.0d - Trigon.AtanDouble(sumY, sumX) / 2.0d);
                }
            });
        }

        private static int[,] BlurInterpolated(int[,] forBlur, int border){
            var result = new int[forBlur.GetLength(0), forBlur.GetLength(1)];

            Parallel.For(border, forBlur.GetLength(0) - border, x => {
                for (var y = border; y < forBlur.GetLength(1) - border; y++){
                    result[x, y] = (forBlur[x - 2, y - 2] +
                                    forBlur[x, y - 2] +
                                    forBlur[x + 2, y - 2] +
                                    forBlur[x + 2, y] +
                                    forBlur[x + 2, y + 2] +
                                    forBlur[x, y + 2] +
                                    forBlur[x - 2, y + 2] +
                                    forBlur[x - 2, y]) / 8; //4
                }
            });

            return result;
        }
        
        private static int[,] Blur(int[,] forBlur, int border){
            var result = new int[forBlur.GetLength(0), forBlur.GetLength(1)];

            Parallel.For(border, forBlur.GetLength(0) - border, x => {
                for (var y = border; y < forBlur.GetLength(1) - border; y++){
                    result[x, y] = (forBlur[x - 1, y - 1] +
                                    forBlur[x, y - 1] +
                                    forBlur[x + 1, y - 1] +
                                    forBlur[x - 1, y] +
                                    forBlur[x, y] +
                                    forBlur[x + 1, y] +
                                    forBlur[x - 1, y + 1] +
                                    forBlur[x, y + 1] +
                                    forBlur[x + 1, y + 1]) / 9;
                }
            });

            return result;
        }

        private static double[,] RidgeDensity(int[,] forBlur, int border, int lineLength) {            
            var result = new double[forBlur.GetLength(0), forBlur.GetLength(1)];

            Parallel.For(border, forBlur.GetLength(0) - border, x => {
                for (var y = border; y < forBlur.GetLength(1) - border; y++){  
                    var lineCoordinates = GetLineCoordinates(x, y, Rotate90(CorrectAngle(_areaAngle[x, y])), lineLength);
                    result[x, y] = CalcDencity(lineCoordinates);
                }
            });

            return result;
        }
        
        private static double CalcDencity(List<Coord> lineCoordinates/*, int x, int y*/){
            var maximas = new List<int>();
            var derivative = new List<double>();
            
            for (var i = 1; i < lineCoordinates.Count - 1; i++) {
                double left = _pointModule[lineCoordinates[i - 1].GetX(), lineCoordinates[i - 1].GetY()];
                double right = _pointModule[lineCoordinates[i + 1].GetX(), lineCoordinates[i + 1].GetY()];

                derivative.Add(right-left);
            }

            for (var i = 1; i < derivative.Count - 1; i++){
                var left = derivative[i - 1];
                var center = derivative[i];
                var right = derivative[i + 1];

                if (center > left && center > right && center > 60){
                    maximas.Add(i);
                }
            }

            var divider = 0;

            for (var i = 0; i < maximas.Count - 1; i++) 
                divider += Math.Abs(maximas[i] - maximas[i + 1]);

            return divider == 0 ? 0 : (double)(maximas.Count - 1) / divider;
        }
        
//        private static double CalcDencity2(List<Coord> lineCoordinates){
//            var derivative = new List<double>();
//            var amplitude = new List<double>();
//
//            for (var i = 1; i < lineCoordinates.Count - 1; i++) {
//                double left = _pointModule[lineCoordinates[i - 1].GetX(), lineCoordinates[i - 1].GetY()];
//                double right = _pointModule[lineCoordinates[i + 1].GetX(), lineCoordinates[i + 1].GetY()];
//
//                derivative.Add(right-left);
//            }
//
//            for (var i = 1; i < derivative.Count - 1; i++){
//                var left = derivative[i - 1];
//                var center = derivative[i];
//                var right = derivative[i + 1];
//
//                if (center > left && center > right && center > 60){
//                    amplitude.Add(center);
//                }
//            }
//
//            return amplitude.Count > 2 ? amplitude.Min() : 0;
//        }

        /*Experimental method to find approximate gradient values near specified point*/
        private static void CalcGradientAtPoint(int[,] blured){
            for (var x = 0; x < blured.GetLength(0) - 4; x += 2){
                for (var y = 0; y < blured.GetLength(1) - 4; y += 2){
                    var a = blured[x, y] +
                            blured[x + 1, y] +
                            blured[x, y + 1] +
                            blured[x + 1, y + 1];

                    var b = blured[x + 2, y] +
                            blured[x + 3, y] +
                            blured[x + 2, y + 1] +
                            blured[x + 3, y + 1];

                    var c = blured[x, y + 2] +
                            blured[x + 1, y + 2] +
                            blured[x, y + 3] +
                            blured[x + 1, y + 3];

                    var d = blured[x + 2, y + 2] +
                            blured[x + 3, y + 2] +
                            blured[x + 2, y + 3] +
                            blured[x + 3, y + 3];

                    var gx = b + d - a - c;
                    var gy = c + d - a - b;

                    _pointModule[x / 2, y / 2] = (int) Math.Sqrt(gx * gx + gy * gy);
                    _pointAngle[x / 2, y / 2] = (int) Trigon.AtanDouble(gy, gx);
                }
            }
        }

        private static byte AverageDirectionInArea(int area, int x, int y){
            var sumX = 0.0d;
            var sumY = 0.0d;

            for (var w = x - area; w <= x + area; w++){
                for (var z = y - area; z <= y + area; z++){
                    sumX += _areaModule[w, z] * FastTrigon.FastCos[2 * _areaAngle[w, z]];
                    sumY += _areaModule[w, z] * FastTrigon.FastSin[2 * _areaAngle[w, z]];
                }
            }

            return ToStoredAngle(Trigon.AtanDouble(sumY, sumX));
        }

        /*Convert angle to format required by FVC Ongoing*/
        private static byte ToStoredAngle(double angle){
            var storedAngle = angle / 2.0d;
            if (storedAngle < 90.0d){
                storedAngle = storedAngle + 90.0d;
            } else storedAngle = storedAngle - 90.0d;

            storedAngle = storedAngle * 1.42222d;
            if (storedAngle > 255.0d) storedAngle = storedAngle - 255.0d;

            return Convert.ToByte(storedAngle);
        }

        private static int CorrectAngle(int angle){
            if (angle >= 90) return 180 - (angle - 90);
            else return 90 - angle;
        }
        
        private static int Rotate90(int angle){
            return angle < 90 ? angle + 90 : angle - 90;
        }

        private static int[,] BlurAlongRidgeDirection(int[,] forBlur, int border, int lineLength){
            var result = new int[forBlur.GetLength(0), forBlur.GetLength(1)];

            Parallel.For(border, forBlur.GetLength(0) - border, x => {
                for (var y = border; y < forBlur.GetLength(1) - border; y++){  
                    var lineCoordinates = GetLineCoordinates(x, y, CorrectAngle(_areaAngle[x, y]), lineLength);
                    var sum = 0;

                    foreach (var c in lineCoordinates){
                        sum += forBlur[c.GetX(), c.GetY()];
                    }

                    result[x, y] = sum/lineCoordinates.Count;
                }
            });

            
            return result;
        }
        
        private static int[,] FFT(int[,] forTransform, int border, int lineLength, StreamWriter res){
            var result = new int[forTransform.GetLength(0), forTransform.GetLength(1)];
            
            for (var x = border; x < forTransform.GetLength(0) - border; x ++){
                for (var y = border; y < forTransform.GetLength(1) - border; y ++){
                    var lineCoordinates = GetLineCoordinates(x, y, Rotate90(CorrectAngle(_areaAngle[x, y])), lineLength);
                                        
                    var complexes = new Complex[lineCoordinates.Count];
                    
                    for (var i = 0; i < lineCoordinates.Count; i++){
                        var c = lineCoordinates[i];
                        if (i == 0 || i%2 == 0) complexes[i] = new Complex(forTransform[c.GetX(), c.GetY()], 0);
                        else complexes[i] = new Complex(-forTransform[c.GetX(), c.GetY()], 0);
                    }
                    
                    res.WriteLine("\nX: " + x + " Y: " + y + ". Brightness:");

                    foreach (var c in complexes){
                        res.Write(c.Re + ", ");
                    }

                    FourierTransform.FFT(complexes, FourierTransform.Direction.Forward);
                    
                    res.WriteLine("\nFFT Real:");
                    
                    foreach (var c in complexes){
                        res.Write(string.Format("{0:0.00}", c.Re) + ", ");
                    }
                    
                    res.WriteLine("\nFFT Imaginary:");
                    
                    foreach (var c in complexes){
                        res.Write(string.Format("{0:0.00}", c.Im) + ", ");
                    }
                    
                    res.WriteLine("\nFFT Energy spectre:");
                    
                    foreach (var c in complexes){
                        res.Write(string.Format("{0:0.00}", Math.Sqrt(c.Im * c.Im + c.Re * c.Re)) + ", ");
                    }
                    
                    res.WriteLine("\nFFT Phase:");
                    
                    foreach (var c in complexes){
                        res.Write(string.Format("{0:0.00}", Math.Atan2(c.Im, c.Re)) + ", ");
                    }
                    
                    res.WriteLine("\nFFT Real Max & Index:");
                    
                    int index = 0;
                    double max = 0.0d;

                    for (int i = 1; i < complexes.Length; i++){
                        if (complexes[i].Re > complexes[index].Re){
                            index = i;
                            max = complexes[i].Re;
                        }                       
                    }
                    
                    res.Write(string.Format("{0:0.00}", max) + ", ");
                    res.Write(index);
                    
                    result[x, y] = (int) (2.0d * Math.PI * 9.0d * max / 16.0d);
                    
                    res.WriteLine("\n------------------------------------------------");
                }
            }
            
            return result;
        }

//        private static int temp(Complex[] complexes){
//            var sumRe = complexes.Sum(c => c.Re * c.Re);
//            var sumIm = complexes.Sum(c => c.Im * c.Im);
//            return (int) Math.Sqrt(sumRe + sumIm);
//        }

        private static List<Coord> GetLineCoordinates(int x, int y, int angle, int lineLength) {
            var length = lineLength;

            for (;;){
                var lineCoordinates = GetSimpleLineCoordinates(x, y, angle, length);
                if (lineCoordinates.Count >= lineLength) return lineCoordinates.GetRange(0, lineLength);
                length++;
            }
        }

        private static List<Coord> GetSimpleLineCoordinates(int x, int y, int angle, int lineLength) {
            return GetLine(Convert.ToInt32(x + FastTrigon.FastCos[angle] * lineLength / 2),
                           Convert.ToInt32(y + FastTrigon.FastSin[angle] * lineLength / 2),
                           Convert.ToInt32(x + FastTrigon.FastCos[angle + 180] * lineLength / 2),
                           Convert.ToInt32(y + FastTrigon.FastSin[angle + 180] * lineLength / 2));
        }
        
        //Bresenham's line algorithm
        private static List<Coord> GetLine(int x1, int y1, int x2, int y2) {
            var coordinates = new List<Coord>();

            var deltaX = Math.Abs(x2 - x1);
            var deltaY = Math.Abs(y2 - y1);
            var signX = x1 < x2 ? 1 : -1;
            var signY = y1 < y2 ? 1 : -1;
            
            var error = deltaX - deltaY;
 
            while(x1 != x2 || y1 != y2) {
                coordinates.Add(new Coord(x1, y1));

                var error2 = error * 2;
                
                if(error2 > -deltaY) {
                    error -= deltaY;
                    x1 += signX;
                }
                if(error2 < deltaX) {
                    error += deltaX;
                    y1 += signY;
                }
            }
            coordinates.Add(new Coord(x2, y2));

            return coordinates;
        }
    }
}