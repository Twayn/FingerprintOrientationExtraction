using System;

namespace FPOrientField
{
    public class Interpolator
    {
        private readonly int[,] _qualityMeasure;
        private readonly byte[][] _orientations;
        private readonly byte[][] _foreground;
        public Interpolator(byte[][] orientations, int[,] qualityMeasure, byte[][] foreground)
        {
            _orientations = orientations;
            _qualityMeasure = qualityMeasure;
            _foreground = foreground;
        }

        public void Interpolate()
        {
            BitmapViewer.Save(_qualityMeasure);
            Console.WriteLine(_qualityMeasure.GetLength(0));
            Console.WriteLine(_qualityMeasure.GetLength(1));
            Console.WriteLine(_orientations.GetLength(0));
            Console.WriteLine(_orientations[0].GetLength(0));
        }
    }
}