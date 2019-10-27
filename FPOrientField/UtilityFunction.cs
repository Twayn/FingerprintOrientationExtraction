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

//        private static int[,] _qualityMeasure;

        public static void SetInitialData(string imagePath, Grid grid, System.IO.StreamWriter res){            
            var interpolated = new FastBitmap(BitmapViewer.DoubleSize(imagePath));
            var interpolatedArray = interpolated.GetIntArray();
            var interpolatedBlured = BlurInterpolated(interpolatedArray, 2);

            var source = new FastBitmap(imagePath);
            var sourceArray = source.GetIntArray();

            _pointModule = new int[source.Width, source.Height];
            _pointAngle = new int[source.Width, source.Height];
            _areaAngle = new int[source.Width, source.Height];
            _areaModule = new int[source.Width, source.Height];

            CalcGradientAtPoint(interpolatedBlured);
            CalcDirectionAtArea(AllowedBorder);

            BitmapViewer.Save(sourceArray);
            BitmapViewer.Save(interpolatedArray);
            BitmapViewer.Save(interpolatedBlured);
            BitmapViewer.Save(_pointAngle);
            BitmapViewer.Save(_pointModule);
            BitmapViewer.Save(_areaAngle);
              
//            Grid.SetGradientElements(_pointModule, _pointAngle);

//            Grid.Layer1 = grid.CalcQualityMeasure(new Grid.Coherence(), 8);
//            Grid.Layer2 = grid.CalcQualityMeasure(new Grid.Coherence(), 10);
//            Grid.Layer3 = grid.CalcQualityMeasure(new Grid.Coherence(), 12);
//            Grid.Layer4 = grid.CalcQualityMeasure(new Grid.Coherence(), 14);
//            Grid.Layer5 = grid.CalcQualityMeasure(new Grid.AverageModule(), 8);
//            Grid.Layer6 = grid.CalcQualityMeasure(new Grid.AverageModule(), 10);
//            Grid.Layer7 = grid.CalcQualityMeasure(new Grid.AverageModule(), 12);
//            Grid.Layer8 = grid.CalcQualityMeasure(new Grid.AverageModule(), 14);
//                 
//            BitmapViewer.Save(Grid.Layer1);
//            BitmapViewer.Save(Grid.Layer2);
//            BitmapViewer.Save(Grid.Layer3);
//            BitmapViewer.Save(Grid.Layer4);
//            BitmapViewer.Save(Grid.Layer5);
//            BitmapViewer.Save(Grid.Layer6);
//            BitmapViewer.Save(Grid.Layer7);
//            BitmapViewer.Save(Grid.Layer8);
            
//            BitmapViewer.Save(grid.CalcQualityMeasure(new Grid.CoherenceDiff(), 0));
//            BitmapViewer.Save(grid.CalcQualityMeasure(new Grid.ModuleDiff(), 0));
        }

//        public static int[,] GetQualityMeasure(){
//            return _qualityMeasure;
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
    }
}