using System;
using System.IO;

using MathCore.Logging.Formatters;
using MathCore.Logging.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MathCore.Logging
{
    public class FileLogger : ILogger
    {
        private readonly string _Name;
        private readonly FileLoggerProcessor _Processor;
        [ThreadStatic]
        private static StringWriter _Writer;

        public FileFormatter Formatter { get; set; }

        public IExternalScopeProvider ScopeProvider { get; set; }

        public FileLoggerOptions Options { get; set; }

        public FileLogger(string Name, FileLoggerProcessor Processor)
        {
            _Name = Name ?? throw new ArgumentNullException(nameof(Name));
            _Processor = Processor;
        }

        public bool IsEnabled(LogLevel Level) => Level != LogLevel.None;

        public void Log<TState>(LogLevel Level, EventId Id, TState State, Exception Error, Func<TState, Exception, string> Formatter)
        {
            if (!IsEnabled(Level)) return;
            if (Formatter is null) throw new ArgumentNullException(nameof(Formatter));

            _Writer ??= new StringWriter();
            var entry = new LogEntry<TState>(Level, _Name, Id, State, Error, Formatter);
            this.Formatter.Write(in entry, ScopeProvider, _Writer);
            var string_builder = _Writer.GetStringBuilder();
            if (string_builder.Length == 0) return;

            var message = string_builder.ToString();
            string_builder.Clear();
            if (string_builder.Capacity > 1024)
                string_builder.Capacity = 1024;
            _Processor.EnqueueMessage(message);
        }

        public IDisposable BeginScope<TState>(TState state) => ScopeProvider?.Push(state) ?? NullScope.Instance;
    }
}
