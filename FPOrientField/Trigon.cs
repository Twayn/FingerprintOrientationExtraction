using System;

namespace FPOrientField {
    internal abstract class Trigon {
        private const double Rad2Deg = 180.0d / Math.PI;

        /*Trigonometric functions working with degrees*/
        public static double Cos(double angle){
            return Math.Cos(angle / Rad2Deg);
        }

        public static double Sin(double angle) {
            return Math.Sin(angle / Rad2Deg);
        }

        public static double AtanDouble(double y, double x) {
            var angle = Math.Atan2(y, x) * Rad2Deg;
            if (angle < 0) angle += 360.0d;
            return angle;
        }
        
        /*EXPERIMENTAL SECTION*/
        private static readonly int[] Values = {
                0,  0,  0,  1,  1,  1,  1,  1,  1,  2,  2,  2,  2,  2,  2,  2,
                3,  3,  3,  3,  3,  3,  4,  4,  4,  4,  4,  4,  5,  5,  5,  5,
                5,  5,  5,  6,  6,  6,  6,  6,  6,  7,  7,  7,  7,  7,  7,  7,
                8,  8,  8,  8,  8,  8,  9,  9,  9,  9,  9,  9,  9, 10, 10, 10,
                10, 10, 10, 11, 11, 11, 11, 11, 11, 11, 12, 12, 12, 12, 12, 12,
                12, 13, 13, 13, 13, 13, 13, 13, 14, 14, 14, 14, 14, 14, 14, 15,
                15, 15, 15, 15, 15, 15, 16, 16, 16, 16, 16, 16, 16, 16, 17, 17,
                17, 17, 17, 17, 17, 18, 18, 18, 18, 18, 18, 18, 18, 19, 19, 19,
                19, 19, 19, 19, 19, 20, 20, 20, 20, 20, 20, 20, 20, 21, 21, 21,
                21, 21, 21, 21, 21, 22, 22, 22, 22, 22, 22, 22, 22, 22, 23, 23,
                23, 23, 23, 23, 23, 23, 23, 24, 24, 24, 24, 24, 24, 24, 24, 24,
                25, 25, 25, 25, 25, 25, 25, 25, 25, 26, 26, 26, 26, 26, 26, 26,
                26, 26, 26, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 28, 28, 28,
                28, 28, 28, 28, 28, 28, 28, 29, 29, 29, 29, 29, 29, 29, 29, 29,
                29, 29, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 31, 31,
                31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 32, 32, 32, 32, 32, 32,
                32,      
           };

        /*Integer arctangent (0-255)*/
        public static int AtanInt(int y, int x){
            var abx = Math.Abs(x);
            var aby = Math.Abs(y);

            if (abx + aby == 0){
                return 0; //-1
            }

            var isNegX = (x < 0) ? 1 : 0;
            var isNegY = (y < 0) ? 1 : 0;
            var more = (aby > abx) ? 1 : 0;

            //switch(((y < 0) << 2) + ((x < 0) << 1) + (aby > abx))
            switch ((isNegY << 2) + (isNegX << 1) + (more)){
                default: return (Values[At(aby, abx)]);
                case 1: return (64 - Values[At(abx, aby)]);
                case 2: return (128 - Values[At(aby, abx)]);
                case 3: return (64 + Values[At(abx, aby)]);
                case 4: return (256 - Values[At(aby, abx)]) & 255;
                case 5: return (192 + Values[At(abx, aby)]);
                case 6: return (128 + Values[At(aby, abx)]);
                case 7: return (192 - Values[At(abx, aby)]);
            }
        }

        private static int At(int left, int right) {
            return (left << 8) / right;
        }

//        public static double atanDouble(double y, double x) {
//            /*
//                if (gx == 0){
//                    if (gy == 0) angle = 0;
//                    else if (gy > 0) angle = 90;
//                    else angle = 270;
//                }
//                else{
//                    radians = Math.Atan(gy / gx);
//                    angle = radians * (RAD2DEG);
//
//                    if ((gy > 0) && (gx < 0)) //second quadrant
//                        angle = angle + 180;
//                    if ((gy < 0) && (gx < 0)) //third quadrant
//                        angle = angle + 180;
//                    if ((gy < 0) && (gx > 0)) //fourth quadrant
//                        angle = angle + 360;
//             }*/
//
//            var angle = Math.Atan2(y, x) * Rad2Deg;
//            if (angle < 0) angle += 360.0;
//            return angle;
//        }
    }
}
