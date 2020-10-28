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
        #region Variables

        private readonly string _path;
        private readonly string _fileName;
        private readonly string _extension;
        private readonly int? _maxFileSize;
        private readonly int? _maxRetainedFiles;
        private readonly PeriodicityOptions _periodicity;

        #endregion
        #region Constructor

        /// <summary>
        /// Creates an instance of the <see cref="FileLoggerProvider" /> 
        /// </summary>
        /// <param name="options">The options object controlling the logger</param>
        public FileLoggerProvider(IOptionsMonitor<FileLoggerOptions> options) : base(options)
        {
            var loggerOptions = options.CurrentValue;
            _path = loggerOptions.LogDirectory;
            _fileName = loggerOptions.FileName;
            _extension = loggerOptions.Extension;
            _maxFileSize = loggerOptions.FileSizeLimit;
            _maxRetainedFiles = loggerOptions.RetainedFileCountLimit;
            _periodicity = loggerOptions.Periodicity;
        }

        public FileLoggerProvider(FileLoggerOptions options) : base(options)
        {
            _path = options.LogDirectory;
            _fileName = options.FileName;
            _extension = options.Extension;
            _maxFileSize = options.FileSizeLimit;
            _maxRetainedFiles = options.RetainedFileCountLimit;
            _periodicity = options.Periodicity;
        }
        #endregion
        /// <inheritdoc />
        protected override async Task WriteMessagesAsync(IEnumerable<LogMessage> messages, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(_path);

            foreach (var group in messages.GroupBy(GetGrouping))
            {
                var fullName = GetFullName(group.Key);
                var fileInfo = new FileInfo(fullName);
                if (_maxFileSize > 0 && fileInfo.Exists && fileInfo.Length > _maxFileSize)
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

        #region Private
        private string GetFullName((int Year, int Month, int Day, int Hour, int Minute) group)
        {
            switch (_periodicity)
            {
                case PeriodicityOptions.Minutely:
                    return Path.Combine(_path, $"{_fileName}{group.Year:0000}{group.Month:00}{group.Day:00}{group.Hour:00}{group.Minute:00}.{_extension}");
                case PeriodicityOptions.Hourly:
                    return Path.Combine(_path, $"{_fileName}{group.Year:0000}{group.Month:00}{group.Day:00}{group.Hour:00}.{_extension}");
                case PeriodicityOptions.Daily:
                    return Path.Combine(_path, $"{_fileName}{group.Year:0000}{group.Month:00}{group.Day:00}.{_extension}");
                case PeriodicityOptions.Monthly:
                    return Path.Combine(_path, $"{_fileName}{group.Year:0000}{group.Month:00}.{_extension}");
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
            if (_maxRetainedFiles > 0)
            {
                var files = new DirectoryInfo(_path)
                    .GetFiles(_fileName + "*")
                    .OrderByDescending(f => f.Name)
                    .Skip(_maxRetainedFiles.Value);

                foreach (var item in files)
                {
                    item.Delete();
                }
            }
        }
        #endregion
    }
}