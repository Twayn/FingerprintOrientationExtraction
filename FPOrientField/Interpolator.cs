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

        public void Interpolate(){
            BitmapViewer.Save(_qualityMeasure);
            Console.WriteLine(_qualityMeasure.GetLength(0));
            Console.WriteLine(_qualityMeasure.GetLength(1));
            Console.WriteLine(_orientations.GetLength(0));
            Console.WriteLine(_orientations[0].GetLength(0));

            for (var y = 0; y < _rows; y++){
                for (var x = 0; x < _columns; x++){
                    if (!_foreground[y][x]) continue;
                    if (_qualityMeasure[x, y] < _threshold){
                        _orientations[y][x] = 127;
                    }
                }
            }
        }
    }
}