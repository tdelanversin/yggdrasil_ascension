

using System.IO;
using Clearcove.Logging;

class Program
{
    static void Main(string[] args)
    {
        var targetLogFile = new FileInfo("./log.log");
        Logger.LogToConsole = true;  // Print log entries to console (optional).
        Logger.IgnoreDebug = false;
        Logger.Start(targetLogFile); // Loggers will complain if you skip initialization

        try
        {
            var logger = new Logger("yigdrasil_logger");
            var game = new src.Game1(logger);
            game.Run();
        }
        finally
        {
            Logger.ShutDown();
        }
    }
}