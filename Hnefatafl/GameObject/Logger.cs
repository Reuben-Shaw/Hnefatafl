using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Hnefatafl
{
    struct Logger
    {
        private string _filePath { get; set; }
        private string _log { get; set; }

        public Logger()
        {
            _filePath = AppDomain.CurrentDomain.BaseDirectory + "Log.txt";
            using (StreamWriter sw = File.CreateText(AppDomain.CurrentDomain.BaseDirectory + "Log.txt"))
            {
                sw.Write("");
                _log = "Starting Boot Up";
                sw.Write(_log);
            }
        }

        public bool Add(string text)
        {
            try
            {
                _log += "\n" + text;
                File.WriteAllText(_filePath, _log);
                return true;    
            }
            catch (System.Exception)
            {
                return false;
            }
        }
    }
}