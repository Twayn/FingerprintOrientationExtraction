namespace FPOrientField{
    public class Coord{
        private readonly int _x;
        private readonly int _y;

        public Coord(int x, int y){
            this._x = x;
            this._y = y;
        }

        public int GetX(){
            return _x;
        }

        public int GetY(){
            return _y;
        }
    }
}