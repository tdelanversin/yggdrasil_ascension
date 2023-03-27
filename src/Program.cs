using Microsoft.VisualBasic;
using System;
using System.IO;

/// <summary>
/// This class instanciates a Logger and makes it available everywhere
/// <param>Usage: just type Logger.Info(...) or Logger.Debug(...) anywhere in the program</param>
/// </summary>
public static class Logger
{
    private static Clearcove.Logging.Logger _logger = new Clearcove.Logging.Logger("ygdrasil");

    public static void Info(string message)
    {
        _logger.Info(message);
    }

    public static void Debug(string message)
    {
        _logger.Debug(message);
    }

    public static void Error(string message)
    {
        _logger.Error(message);
        throw new Exception();
    }

    public static void Warn(string message)
    {
        _logger.Warn(message);
    }
}

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
        Clearcove.Logging.Logger.BatchInterval = 1500;
        Clearcove.Logging.Logger.LogToConsole = true;  // Print log entries to console (optional).
        Clearcove.Logging.Logger.IgnoreDebug = true;
        Clearcove.Logging.Logger.Start(targetLogFile); // Loggers will complain if you skip initialization

        try
        {
            Logger.Info("=================== " + time.ToLongDateString() + " | " + time.ToLongTimeString() + " ===================");
            var game = new YGR.Game1();
            game.Run();
        }
        finally
        {
            Clearcove.Logging.Logger.ShutDown();
        }
    }
}