    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

namespace LoggerLibrary
{
    /// <summary>
    /// An <see cref="ILoggerProvider" /> that writes logs to a file
    /// </summary>
    [ProviderAlias("File")]
    public class FileLoggerProvider : BatchingLoggerProvider
    {
        #region Fields

        private readonly PeriodicityOptions _periodicity;
        private FileLoggerOptions _options;

        #endregion
        #region Constructor

        /// <summary>
        /// Creates an instance of the <see cref="FileLoggerProvider" /> 
        /// </summary>
        /// <param name="options">The options object controlling the logger</param>
        public FileLoggerProvider(IOptionsMonitor<FileLoggerOptions> options) : base(options)
        {
            var loggerOptions = options.CurrentValue;
            _options = loggerOptions;
            _periodicity = loggerOptions.Periodicity;
        }

        public FileLoggerProvider(FileLoggerOptions options) : base(options)
        {
            _options = options;
            _periodicity = options.Periodicity;
        }

        #endregion
        #region Properties

        #endregion
        #region Methods

        /// <inheritdoc />
        protected override async Task WriteMessagesAsync(IEnumerable<LogMessage> messages, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(_options.Path);

            foreach (var group in messages.GroupBy(GetGrouping))
            {
                var fullName = GetFullName(group.Key);
                var fileInfo = new FileInfo(fullName);
                if (_options.FileSizeLimit > 0 && fileInfo.Exists && fileInfo.Length > _options.FileSizeLimit)
                {
                    return;
                }

                using var streamWriter = File.AppendText(fullName);
                foreach (var item in group)
                {
                    await streamWriter.WriteAsync(item.Message);
                }
            }

            RollFiles();
        }

        #endregion
        #region Private

        private string GetFullName((int Year, int Month, int Day, int Hour, int Minute) group)
        {
            switch (_periodicity)
            {
                case PeriodicityOptions.Minutely:
                    return Path.Combine(_options.Path, $"{_options.FileName}{group.Year:0000}{group.Month:00}{group.Day:00}{group.Hour:00}{group.Minute:00}.{_options.Extension}");
                case PeriodicityOptions.Hourly:
                    return Path.Combine(_options.Path, $"{_options.FileName}{group.Year:0000}{group.Month:00}{group.Day:00}{group.Hour:00}.{_options.Extension}");
                case PeriodicityOptions.Daily:
                    return Path.Combine(_options.Path, $"{_options.FileName}{group.Year:0000}{group.Month:00}{group.Day:00}.{_options.Extension}");
                case PeriodicityOptions.Monthly:
                    return Path.Combine(_options.Path, $"{_options.FileName}{group.Year:0000}{group.Month:00}.{_options.Extension}");
                default:
                    break;
            }
            throw new InvalidDataException("Invalid periodicity");
        }

        private (int Year, int Month, int Day, int Hour, int Minute) GetGrouping(LogMessage message)
        {
            return (message.Timestamp.Year, message.Timestamp.Month, message.Timestamp.Day, message.Timestamp.Hour, message.Timestamp.Minute);
        }

        /// <summary>
        /// Deletes old log files, keeping a number of files defined by <see cref="FileLoggerOptions.RetainedFileCountLimit" />
        /// </summary>
        protected void RollFiles()
        {
            if (_options.RetainedFileCountLimit > 0)
            {
                var files = new DirectoryInfo(_options.Path)
                    .GetFiles(_options.FileName + "*")
                    .OrderByDescending(f => f.Name)
                    .Skip(_options.RetainedFileCountLimit.Value);

                foreach (var item in files)
                {
                    item.Delete();
                }
            }
        }
        #endregion
    }
}