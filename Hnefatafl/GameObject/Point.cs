using Microsoft.Xna.Framework;
using static System.Convert;

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

        public HPoint(string point)
        {
            string[] pointSplit = point.Split(",");
            X = ToInt32(pointSplit[0]);
            Y = ToInt32(pointSplit[1]);
        }

        public override string ToString()
        {
            return "" + X + "," + Y;
        }
    }
}