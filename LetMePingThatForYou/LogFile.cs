using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LetMePingThatForYou
{
    class LogFile
    {

        public string logPath;

        public LogFile() { }

        public void writeLog(string Message)
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(logPath)))
                    Directory.CreateDirectory(logPath);
                string logFileName = Path.Combine(logPath, "LMPTFY_" + DateTime.Now.ToString("yyyyMMdd") + ".log");
                File.AppendAllText(logFileName, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss      ") + Message + Environment.NewLine);
            }
            catch (Exception)
            {
            }
        }
    }
}
