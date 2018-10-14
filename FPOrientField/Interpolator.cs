using System;

namespace FPOrientField{
    public class Interpolator{
        private readonly int[,] _qualityMeasure;
        private readonly byte[][] _orientations;
        private readonly int _threshold;
        private readonly bool[][] _foreground;
        private readonly int _rows;
        private readonly int _columns;

        public Interpolator(byte[][] orientations,
                            int[,] qualityMeasure,
                            int threshold,
                            bool[][] foreground,
                            int rows,
                            int columns){
            _orientations = orientations;
            _qualityMeasure = qualityMeasure;
            _threshold = threshold;
            _foreground = foreground;
            _rows = rows;
            _columns = columns;
        }

        /*
         *TODO
         * iterate over orientations
         * if there is a good quality item near bad quality item
         * correct bad quality item.
         */
        public void Interpolate(){
            BitmapViewer.Save(_qualityMeasure);
//            Console.WriteLine(_qualityMeasure.GetLength(0));
//            Console.WriteLine(_qualityMeasure.GetLength(1));
//            Console.WriteLine(_orientations.GetLength(0));
//            Console.WriteLine(_orientations[0].GetLength(0));

            test(25, 30);
            return;
            
            for (var y = 1; y < _rows-1; y++){
                for (var x = 1; x < _columns-1; x++){
                    if (!_foreground[y][x]) continue;

                    if (_qualityMeasure[x, y] < _threshold && HasGoodNeighbor(x, y)){
                        _orientations[y][x] = 127;
                    }
                }
            }
        }

        private bool IsBad(int val){
            return val < _threshold;
        }
        
        private bool IsGood(int val){
            return val >= _threshold;
        }

        private bool HasGoodNeighbor(int x, int y){
            return
                IsGood(_qualityMeasure[x - 1, y - 1]) ||
                IsGood(_qualityMeasure[x - 1, y])     ||
                IsGood(_qualityMeasure[x - 1, y + 1]) ||
                IsGood(_qualityMeasure[x, y - 1])     ||
                IsGood(_qualityMeasure[x, y + 1])     ||
                IsGood(_qualityMeasure[x + 1, y - 1]) ||
                IsGood(_qualityMeasure[x + 1, y])     ||
                IsGood(_qualityMeasure[x + 1, y + 1]);
        }
        
        private bool HasBadNeighbor(int x, int y){
            return
                IsBad(_qualityMeasure[x - 1, y - 1]) ||
                IsBad(_qualityMeasure[x - 1, y])     ||
                IsBad(_qualityMeasure[x - 1, y + 1]) ||
                IsBad(_qualityMeasure[x, y - 1])     ||
                IsBad(_qualityMeasure[x, y + 1])     ||
                IsBad(_qualityMeasure[x + 1, y - 1]) ||
                IsBad(_qualityMeasure[x + 1, y])     ||
                IsBad(_qualityMeasure[x + 1, y + 1]);
        }
        
        private int test(int x, int y){
            var sumX = 0.0d;
            var sumY = 0.0d;
            
            sumX += SumX(_orientations[y - 1][x - 1]);
            sumY += SumY(_orientations[y - 1][x - 1]);
            
            sumX += SumX(_orientations[y][x - 1]);
            sumY += SumY(_orientations[y][x - 1]);
            
            sumX += SumX(_orientations[y + 1][x - 1]);
            sumY += SumY(_orientations[y + 1][x - 1]);
            
            sumX += SumX(_orientations[y - 1][x]);
            sumY += SumY(_orientations[y - 1][x]);
            
            sumX += SumX(_orientations[y + 1][x]);
            sumY += SumY(_orientations[y + 1][x]);
            
            sumX += SumX(_orientations[y - 1][x + 1]);
            sumY += SumY(_orientations[y - 1][x + 1]);
            
            sumX += SumX(_orientations[y][x + 1]);
            sumY += SumY(_orientations[y][x + 1]);
            
            sumX += SumX(_orientations[y + 1][x + 1]);
            sumY += SumY(_orientations[y + 1][x + 1]);

            var res = (int) (Trigon.AtanDouble(sumY, sumX) / 2.0d);
            
            Console.WriteLine("RR: " + res*1.42222d);
            return res;
        }
        
        private double SumX(byte angle){
            Console.WriteLine("R: " + angle);
            return 1 * FastTrigon.FastCos[2 * normalize(angle)];
        }
        
        private double SumY(byte angle){
            return 1 * FastTrigon.FastSin[2 * normalize(angle)];
        }

        private int normalize(byte angle){
            var buf = angle / 1.42222d;
            return (int) buf;
        }
    }
}