using Microsoft.Xna.Framework;
using static System.Convert;
using System;

namespace Hnefatafl
{
    [Serializable]
    public class HPoint
    {
        public int X { get; set; }
        public int Y { get; set; }

        public HPoint() { } //For serialisation

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

        public HPoint(string x, string y)
        {
            X = ToInt32(x);
            Y = ToInt32(y);
        }

        public HPoint(string point)
        {
            string[] pointSplit = point.Split(",");
            X = ToInt32(pointSplit[0]);
            Y = ToInt32(pointSplit[1]);
        }

        public Point AsPoint()
        {
            return new Point(X, Y);
        }

        public override string ToString()
        {
            return "" + X + "," + Y;
        }
    }
}