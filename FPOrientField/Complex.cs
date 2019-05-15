using System;

namespace FPOrientField{
    public class Complex{
        public double Re;
        public double Im;
        
        public static readonly Complex Zero = new Complex(0.0, 0.0);
        
        public Complex(double real, double imaginary){
            this.Re = real;
            this.Im = imaginary;
        }
    }
}