using System;
using System.Collections.Generic;
using LoggerLibrary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LoggerApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // configure logger options

            FileLoggerOptions fileLogger = new FileLoggerOptions();
            fileLogger.FileName = "logger";
            fileLogger.Extension = ".log";
            ILoggerProvider loggerProvider = new FileLoggerProvider(fileLogger);
            ILoggerProvider customProvider = new CustomLoggerProvider();

            //

            ILoggerFactory loggerFactory = new LoggerFactory();
            
            loggerFactory.AddProvider(loggerProvider);
            loggerFactory.AddProvider(customProvider);

            ILogger logger = loggerFactory.CreateLogger<Program>();

            /*
             * Fields
                Critical	5	Logs that describe an unrecoverable application or system crash, or a catastrophic failure that requires immediate attention.
                Debug	    1	Logs that are used for interactive investigation during development. These logs should primarily contain information useful for debugging and have no long-term value.
                Error	    4	Logs that highlight when the current flow of execution is stopped due to a failure. These should indicate a failure in the current activity, not an application-wide failure.
                Information	2   Logs that track the general flow of the application. These logs should have long-term value.
                None	    6   Not used for writing log messages. Specifies that a logging category should not write any messages.
                Trace	    0   Logs that contain the most detailed messages. These messages may contain sensitive application data. These messages are disabled by default and should never be enabled in a production environment.
                Warning     3   Logs that highlight an abnormal or unexpected event in the application flow, but do not otherwise cause the application execution to stop.
             */

            logger.LogTrace("Trace");
            logger.LogDebug("Debug");
            logger.LogInformation("Information");
            logger.LogWarning("Warning");
            logger.LogError("Error");
            logger.LogCritical("Critical");

            if (logger.IsEnabled(LogLevel.Warning) == true)
            {
                // Do somthing
                string check = "TDIWECN";
                foreach (char c in check)
                {
                    switch (c)
                    {
                        case 'T':
                            {
                                logger.LogTrace("Correct");
                                break;
                            }
                    }
                }
            }
        }
    }
}
