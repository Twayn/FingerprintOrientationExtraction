using System;
using System.Collections.Generic;
using System.Linq;

namespace FPOrientField{
    public class Grid{
        private readonly bool[][] _foreground;
        private readonly int _rows;
        private readonly int _columns;
        private readonly int _border;
        private readonly int _step;
        
        private static int[,] _module;
        private static int[,] _angle;
        
        public static int[,] Layer1;
        public static int[,] Layer2;
        public static int[,] Layer3;
        public static int[,] Layer4;
        public static int[,] Layer5;
        public static int[,] Layer6;

        public Grid(bool[][] foreground, int rows, int columns, int border, int step){
            _foreground = foreground;
            _rows = rows;
            _columns = columns;
            _border = border;
            _step = step;
        }

        public static void SetGradientElements(int[,] pointModule, int[,] pointAngle){
            _module = pointModule;
            _angle = pointAngle;
        }

        public int[,] CalcQualityMeasure(IQualityMeasure measure, int range){
            var result = new int[_module.GetLength(0), _module.GetLength(1)];
            
            for (var y = 0; y < _rows; y++){
                for (var x = 0; x < _columns; x++){
                    if (!_foreground[y][x]) continue;
                    var px = _border + x * _step;
                    var py = _border + y * _step;
                    result[px, py] = measure.CalculateInArea(px, py, range);
                    FillAreaNearPixel(result, px, py, _step/2);
                }
            }

            return result;
        }
        
        public int[,] CalcQualityMeasureOriginalSize(IQualityMeasure measure, int range){
            var result = new int[_columns, _rows];
            
            for (var x = 0; x < _columns; x++){
                for (var y = 0; y < _rows; y++){
                    if (!_foreground[y][x]) continue;
                    var px = _border + x * _step;
                    var py = _border + y * _step;
                    result[x, y] = measure.CalculateInArea(px, py, range);
                }
            }

            return result;
        }


        private static void FillAreaNearPixel(int[,] toFill, int x, int y, int range){
            for (var w = x - range; w < x + range; w++) {
                for (var z = y - range; z < y + range; z++){
                    toFill[w, z] = toFill[x, y];
                }
            }
        }

        public interface IQualityMeasure{
            int CalculateInArea(int x, int y, int range);
        }

        public class Coherence : IQualityMeasure{
            public int CalculateInArea(int x, int y, int range){
                var sumX = 0.0d;
                var sumY = 0.0d;

                var modulesSum = 0.0d;
        
                for (var w = x - range; w < x + range; w++) {
                    for (var z = y - range; z < y + range; z++) {
                        sumX += _module[w, z] * FastTrigon.FastCos[2 * _angle[w, z]];
                        sumY += _module[w, z] * FastTrigon.FastSin[2 * _angle[w, z]];

                        modulesSum += _module[w, z];
                    }
                }
                
                if (Math.Abs(modulesSum) < 0.0001d) return 1; 
                

                return (int) (Math.Sqrt(sumX * sumX + sumY * sumY) / modulesSum * 1000);
            }
        }
        
        public class AverageModule : IQualityMeasure{
            public int CalculateInArea(int x, int y, int range){
                var modulesSum = 0;
                var modulesCount = 0;
                
                for (var w = x - range; w < x + range; w++) {
                    for (var z = y - range; z < y + range; z++){
                        modulesCount++;
                        modulesSum += _module[w, z];
                    }
                }
                
                return modulesSum/modulesCount;
            }
        }
        
        public int Threshold(int[,] input){
            var numbers = new List<int>();
            
            for (var y = 0; y < _rows; y++){
                for (var x = 0; x < _columns; x++){
                    if (!_foreground[y][x]) continue;
                    var px = _border + x * _step;
                    var py = _border + y * _step;
                    numbers.Add(input[px,py]);
                }
            }
            numbers.Sort();
            
            return numbers[numbers.Count/2];
        }
        
        public class ComplexQuality : IQualityMeasure{
            public int CalculateInArea(int x, int y, int range){
                int[] matrix1 = {
                    Layer1[x, y],
                    Layer2[x, y],
                    Layer3[x, y]
                };
                double range1 = matrix1.Max() - matrix1.Min();//TODO think about clever use
                
                int[] matrix2 = {
                    Layer4[x, y],
                    Layer5[x, y],
                    Layer6[x, y]
                };
                double range2 = matrix2.Max() - matrix2.Min();//TODO think about clever use
                  
//                if (range1 < 1) range1 = 1;
//              
//                if (range2 < 1) range2 = 1;
//                
//                Console.WriteLine("R: " + range1);
//                Console.WriteLine("R2: " + range2);
                
                //Console.WriteLine("RR: " + Layer6[x,y] * Trigon.Sin(Layer3[x,y]/1000.0d*90.0d) / range1 /range2);
                
                return (int) (Layer6[x,y] * Trigon.Sin(Layer3[x,y]/1000.0d*90.0d));
            }
        }
    }
}