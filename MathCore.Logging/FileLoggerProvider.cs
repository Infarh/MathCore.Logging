using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using MathCore.Logging.Formatters;
using MathCore.Logging.Formatters.Options;
using MathCore.Logging.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MathCore.Logging
{
    [ProviderAlias("File")]
    public class FileLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly IOptionsMonitor<FileLoggerOptions> _Options;
        private readonly ConcurrentDictionary<string, FileLogger> _Loggers = new();
        private ConcurrentDictionary<string, FileFormatter> _Formatters;
        private readonly IDisposable _OptionsReloadToken;
        private readonly FileLoggerProcessor _MessageQueue;
        private IExternalScopeProvider _ScopeProvider = NullExternalScopeProvider.Instance;

        public FileLoggerProvider(IOptionsMonitor<FileLoggerOptions> Options, IEnumerable<FileFormatter> Formatters)
        {
            _Options = Options;
            SetFormatters(Formatters);

            _OptionsReloadToken = _Options.OnChange(ReloadLoggerOptions);
            _MessageQueue = new FileLoggerProcessor(Options.CurrentValue.FilePath);
            ReloadLoggerOptions(Options.CurrentValue);
        }

        private void ReloadLoggerOptions(FileLoggerOptions options)
        {
            if (options.FormatterName == null || !_Formatters.TryGetValue(options.FormatterName, out var formatter))
                formatter = _Formatters[FileFormatterNames.Simple];

            _MessageQueue.FilePath = options.FilePath;

            foreach (var (_, logger) in _Loggers)
            {
                logger.Options = options;
                logger.Formatter = formatter;
            }
        }

        private void SetFormatters(IEnumerable<FileFormatter> Formatters = null)
        {
            _Formatters = new(StringComparer.OrdinalIgnoreCase);
            if (Formatters is null || !Formatters.Any())
            {
                var default_monitor = new FormatterOptionsMonitor<SimpleFileFormatterOptions>(new SimpleFileFormatterOptions());
                var json_monitor = new FormatterOptionsMonitor<JsonFileFormatterOptions>(new JsonFileFormatterOptions());
                _Formatters.GetOrAdd(FileFormatterNames.Simple, _ => new SimpleFileFormatter(default_monitor));
                _Formatters.GetOrAdd(FileFormatterNames.Json, _ => new JsonFileFormatter(json_monitor));
            }
            else
            {
                foreach (var formatter in Formatters)
                    _Formatters.GetOrAdd(formatter.Name, _ => formatter);
            }
        }

        public ILogger CreateLogger(string Name)
        {
            if (_Options.CurrentValue.FormatterName is null || !_Formatters.TryGetValue(_Options.CurrentValue.FormatterName, out var log_formatter))
                log_formatter = _Formatters[FileFormatterNames.Simple];

            return _Loggers.GetOrAdd(Name, name => new FileLogger(name, _MessageQueue)
            {
                Options = _Options.CurrentValue,
                ScopeProvider = _ScopeProvider,
                Formatter = log_formatter,
            });
        }

        public void SetScopeProvider(IExternalScopeProvider Scope)
        {
            _ScopeProvider = Scope;
            foreach (var logger in _Loggers)
                logger.Value.ScopeProvider = _ScopeProvider;
        }

        public void Dispose()
        {
            _OptionsReloadToken?.Dispose();
            _MessageQueue.Dispose();
        }
    }
}
