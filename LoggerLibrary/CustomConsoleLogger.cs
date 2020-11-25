﻿using System;
using LoggerLibrary;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace LoggerLibrary
{
    public class CustomConsoleLogger : ILogger
    {
        private readonly string _categoryName;

        public CustomConsoleLogger(string categoryName)
        {
            _categoryName = categoryName;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            DateTime dateTime = DateTime.Now;
            //Console.WriteLine($"{logLevel}: {_categoryName}[{eventId.Id}]: {formatter(state, exception)}");
            Console.WriteLine($"{dateTime} {formatter(state, exception)}");
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}
