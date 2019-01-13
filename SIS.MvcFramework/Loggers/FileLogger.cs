using System;
using System.IO;

namespace SIS.MvcFramework.Loggers
{
    public class FileLogger : ILogger
    {
        private static readonly object LockObj = new object();

        private readonly string fileName;

        public FileLogger()
            :this("log.txt")
        {
        }

        public FileLogger(string fileName)
        {
            this.fileName = fileName;
        }

        public void Log(string message)
        {
            lock (LockObj)
            {
                File.AppendAllText(this.fileName, $"[{DateTime.UtcNow}] {message}{Environment.NewLine}");
            }
        }
    }
}