using Microsoft.Xna.Framework;
namespace Hnefatafl
{
    public class HPoint
    {
        public int X { get; set; }
        public int Y { get; set; }

        public HPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public HPoint(Point point)
        {
            X = point.X;
            Y = point.Y;
        }

        public override string ToString()
        {
            return "" + X + Y;
        }
    }
}