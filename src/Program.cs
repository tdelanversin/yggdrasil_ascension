

using System;
using System.IO;
using Clearcove.Logging;
using Microsoft.VisualBasic;

class Program
{
    static void Main(string[] args)
    {
        var time = DateAndTime.Now;
        var random = new Random();
        var targetLogFile = new FileInfo(
            "./log_[" + 
            time.Year.ToString() + 
            time.Month.ToString().PadLeft(2, '0') + 
            time.Day.ToString().PadLeft(2, '0') + 
            "][" + 
            time.Hour.ToString().PadLeft(2, '0') +
            "-" +
            time.Minute.ToString().PadLeft(2, '0') +
            "-" +
            time.Second.ToString().PadLeft(2, '0') +
            "][" +
            time.Millisecond.ToString() + 
            random.Next().ToString() + 
            "].log");
        Logger.BatchInterval = 1500;
        Logger.LogToConsole = false;  // Print log entries to console (optional).
        Logger.IgnoreDebug = false;
        Logger.Start(targetLogFile); // Loggers will complain if you skip initialization

        try
        {
            var logger = new Logger("yigdrasil_logger");
            logger.Info("=================== " + time.ToLongDateString() + " | " + time.ToLongTimeString() + " ===================");
            var game = new YGR.Game1(logger);
            game.Run();
        }
        finally
        {
            Logger.ShutDown();
        }
    }
}