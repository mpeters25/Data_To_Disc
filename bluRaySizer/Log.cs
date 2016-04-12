using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace bluRaySizer
{
    public static class Log
    {
        private static string _logFilePath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + 
                "\\Data_To_Disc.log";

        private static string _logEntryDate;

        public static void LogWrite(string message)
        {
            _logEntryDate = DateTime.Now.ToString();

            using (StreamWriter streamWriter = new StreamWriter(File.Open(_logFilePath, FileMode.Append)))
            {
                    streamWriter.WriteLine(_logEntryDate + message);
            }
        }
    }
}
