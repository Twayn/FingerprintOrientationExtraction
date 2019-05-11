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

        public void Interpolate(){//При step 3 отдавать предпочтение к тем, что ближе к уже определенному значению.
            //BitmapViewer.Save(_qualityMeasure);
            //while (InterpolateStep()){ }
            //while (InterpolateStep2()){ }
            //InterpolateStep3();
            //nullify();
        }

        void nullify(){
            for (var y = 1; y < _rows-1; y++){
                for (var x = 1; x < _columns-1; x++){
                    if (!_foreground[y][x]) continue;

                    if (_qualityMeasure[x, y] < _threshold){    
                        _orientations[y][x] = 127;
                        _qualityMeasure[x, y] = _threshold + 1;
                    }
                }
            }
        }

        private bool InterpolateStep(){
            var interpolationHappens = false;
            for (var y = 1; y < _rows-1; y++){
                for (var x = 1; x < _columns-1; x++){
                    if (!_foreground[y][x]) continue;

                    if (_qualityMeasure[x, y] < _threshold && HasGoodNeighbor(x, y)){
                        var estimation = Average3X3(x, y);
                        var existing = _orientations[y][x];
                        
                        if (DifferenceLessThan15(existing, estimation)){
                            _qualityMeasure[x, y] = _threshold + 1;
                            interpolationHappens = true;
                        }
                    } 
                }
            }

            return interpolationHappens;
        }
                
        private bool InterpolateStep2(){
            var interpolationHappens = false;
            for (var y = 1; y < _rows-1; y++){
                for (var x = 1; x < _columns-1; x++){
                    if (!_foreground[y][x]) continue;

                    if (_qualityMeasure[x, y] < _threshold && HasGoodNeighbor(x, y)){
                        var existing = _orientations[y][x];
                        
                        if (HasGoodCoherentNeigbor(existing, x, y)){
                            _qualityMeasure[x, y] = _threshold + 1;
                            interpolationHappens = true;
                        }
                    } 
                }
            }

            return interpolationHappens;
        }

        private bool InterpolateStep3(){
            var interpolationHappens = false;
            for (var y = 1; y < _rows-1; y++){
                for (var x = 1; x < _columns-1; x++){
                    if (!_foreground[y][x]) continue;

                    if (_qualityMeasure[x, y] < _threshold && HasGoodNeighbor(x, y)){    
                        var estimation = Average3X3(x, y);
                        Console.WriteLine(Math.Abs(estimation));
                        _orientations[y][x] = estimation;
                        _qualityMeasure[x, y] = _threshold + 1;
                        interpolationHappens = true;
                    }
                }
            }
            
            return interpolationHappens;
        }

        private static bool DifferenceLessThan15(int existing, int estimation){
            var abs = Math.Abs(estimation - existing);
            return abs < 13 || abs > 242;
        }
                
        private int IsGoodInt(int val){
            return val >= _threshold ? 1 : 0;
        }
        
        private bool IsGood(int val){
            return val >= _threshold;
        }

        private bool HasGoodNeighbor(int x, int y){
            return
                IsGoodInt(_qualityMeasure[x - 1, y - 1]) +
                IsGoodInt(_qualityMeasure[x - 1, y]) +
                IsGoodInt(_qualityMeasure[x - 1, y + 1]) +
                IsGoodInt(_qualityMeasure[x, y - 1]) +
                IsGoodInt(_qualityMeasure[x, y + 1]) +
                IsGoodInt(_qualityMeasure[x + 1, y - 1]) +
                IsGoodInt(_qualityMeasure[x + 1, y]) +
                IsGoodInt(_qualityMeasure[x + 1, y + 1]) > 1;
        }
        
        private byte Average3X3(int x, int y){
            var sumX = 0.0d;
            var sumY = 0.0d;

            if (IsGood(_qualityMeasure[x - 1, y - 1])){
                sumX += SumX(_orientations[y - 1][x - 1]);
                sumY += SumY(_orientations[y - 1][x - 1]);
            }
            
            if (IsGood(_qualityMeasure[x - 1, y])){
                sumX += SumX(_orientations[y][x - 1]);
                sumY += SumY(_orientations[y][x - 1]);
            }
            
            if (IsGood(_qualityMeasure[x - 1, y + 1])){
                sumX += SumX(_orientations[y + 1][x - 1]);
                sumY += SumY(_orientations[y + 1][x - 1]);
            }
            
            if (IsGood(_qualityMeasure[x, y - 1])){
                sumX += SumX(_orientations[y - 1][x]);
                sumY += SumY(_orientations[y - 1][x]);
            }
            
            if (IsGood(_qualityMeasure[x, y + 1])){
                sumX += SumX(_orientations[y + 1][x]);
                sumY += SumY(_orientations[y + 1][x]);
            }
            
            if (IsGood(_qualityMeasure[x + 1, y - 1])){
                sumX += SumX(_orientations[y - 1][x + 1]);
                sumY += SumY(_orientations[y - 1][x + 1]);
            }
            
            if (IsGood(_qualityMeasure[x + 1, y])){
                sumX += SumX(_orientations[y][x + 1]);
                sumY += SumY(_orientations[y][x + 1]);
            }
            
            if (IsGood(_qualityMeasure[x + 1, y + 1])){
                sumX += SumX(_orientations[y + 1][x + 1]);
                sumY += SumY(_orientations[y + 1][x + 1]);
            }
            
            var res = (int) (Trigon.AtanDouble(sumY, sumX) / 2.0d);
            
            return ToStored(res);
        }
        
        private bool HasGoodCoherentNeigbor(byte angle, int x, int y){
            return (IsGood(_qualityMeasure[x - 1, y - 1]) &&  DifferenceLessThan15(angle, _orientations[y - 1][x - 1])) ||
                   (IsGood(_qualityMeasure[x - 1, y])     &&  DifferenceLessThan15(angle, _orientations[y][x - 1]))     ||
                   (IsGood(_qualityMeasure[x - 1, y + 1]) &&  DifferenceLessThan15(angle, _orientations[y + 1][x - 1])) ||
                   (IsGood(_qualityMeasure[x, y - 1])     &&  DifferenceLessThan15(angle, _orientations[y - 1][x]))     ||
                   (IsGood(_qualityMeasure[x, y + 1])     &&  DifferenceLessThan15(angle, _orientations[y + 1][x]))     ||
                   (IsGood(_qualityMeasure[x + 1, y - 1]) &&  DifferenceLessThan15(angle, _orientations[y - 1][x + 1])) ||
                   (IsGood(_qualityMeasure[x + 1, y])     &&  DifferenceLessThan15(angle, _orientations[y][x + 1]))     ||
                   (IsGood(_qualityMeasure[x + 1, y + 1]) &&  DifferenceLessThan15(angle, _orientations[y + 1][x + 1]));
        }

        private static byte ToStored(int res){
            return (byte)(res * 1.42222d);
        }

        private static double SumX(byte angle){
            return 1 * FastTrigon.FastCos[2 * Normalize(angle)];
        }
        
        private static double SumY(byte angle){
            return 1 * FastTrigon.FastSin[2 * Normalize(angle)];
        }

        private static int Normalize(byte angle){
            var buf = angle / 1.42222d;
            return (int) buf;
        }
    }
}