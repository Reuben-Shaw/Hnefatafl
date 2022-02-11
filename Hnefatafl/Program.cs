using System;

namespace Hnefatafl
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new Hnefatafl())
                game.Run();
        }
    }
}
