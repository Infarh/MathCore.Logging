using System;
using System.IO;

using MathCore.Logging.Formatters.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace MathCore.Logging.Formatters
{
    public class SimpleFileFormatter : FileFormatter, IDisposable
    {
        private const string __LogLevelPadding = ": ";
        private static readonly string __MessagePadding = new(' ', GetLogLevelString(LogLevel.Information).Length + ": ".Length);
        private static readonly string __NewLineWithMessagePadding = Environment.NewLine + __MessagePadding;
        private readonly IDisposable _OptionsReloadToken;

        public SimpleFileFormatterOptions FormatterOptions { get; set; }

        public SimpleFileFormatter(IOptionsMonitor<SimpleFileFormatterOptions> options) : base("simple")
        {
            FormatterOptions = options.CurrentValue;
            _OptionsReloadToken = options.OnChange(opt => FormatterOptions = opt);
        }

        public override void Write<TState>(in LogEntry<TState> Entry, IExternalScopeProvider Scope, TextWriter Writer)
        {
            var message = Entry.Formatter(Entry.State, Entry.Exception);
            if (Entry.Exception is null && message is null)
                return;
            var log_level_string = GetLogLevelString(Entry.LogLevel);
            string str = null;
            var timestamp_format = FormatterOptions.TimestampFormat;
            if (timestamp_format is not null)
                str = GetCurrentDateTime().ToString(timestamp_format);
            if (str is not null)
                Writer.Write(str);
            if (log_level_string is not null)
                Writer.WriteLine(log_level_string);
            CreateDefaultLogMessage(Writer, in Entry, message, Scope);
        }

        private void CreateDefaultLogMessage<TState>(TextWriter Writer, in LogEntry<TState> Entry, string Message, IExternalScopeProvider Scope)
        {
            var single_line = FormatterOptions.SingleLine;
            var id = Entry.EventId.Id;
            var exception = Entry.Exception;
            Writer.Write($": {Entry.Category}[{id}]");
            if (!single_line)
                Writer.Write(Environment.NewLine);
            WriteScopeInformation(Writer, Scope, single_line);
            WriteMessage(Writer, Message, single_line);
            if (exception != null)
                WriteMessage(Writer, exception.ToString(), single_line);
            if (!single_line)
                return;
            Writer.Write(Environment.NewLine);
        }

        private static void WriteMessage(TextWriter Writer, string Message, bool SingleLine)
        {
            if (string.IsNullOrEmpty(Message))
                return;
            if (SingleLine)
            {
                Writer.Write(' ');
                WriteReplacing(Writer, Environment.NewLine, " ", Message);
            }
            else
            {
                Writer.Write(__MessagePadding);
                WriteReplacing(Writer, Environment.NewLine, __NewLineWithMessagePadding, Message);
                Writer.Write(Environment.NewLine);
            }

            static void WriteReplacing(TextWriter writer, string OldValue, string NewValue, string Message) => writer.Write(Message.Replace(OldValue, NewValue));
        }

        private DateTimeOffset GetCurrentDateTime() => !FormatterOptions.UseUtcTimestamp ? DateTimeOffset.Now : DateTimeOffset.UtcNow;

        private static string GetLogLevelString(LogLevel logLevel) =>
            logLevel switch
            {
                LogLevel.Trace => "trce",
                LogLevel.Debug => "dbug",
                LogLevel.Information => "info",
                LogLevel.Warning => "warn",
                LogLevel.Error => "fail",
                LogLevel.Critical => "crit",
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
            };

        private void WriteScopeInformation(TextWriter Writer, IExternalScopeProvider Scope, bool SingleLine)
        {
            if (!FormatterOptions.IncludeScopes || Scope == null)
                return;
            var padding_needed = !SingleLine;
            Scope.ForEachScope((scope, state) =>
            {
                if (padding_needed)
                {
                    padding_needed = false;
                    state.Write(__MessagePadding + "=> ");
                }
                else
                    state.Write(" => ");
                state.Write(scope);
            }, Writer);
            if (padding_needed || SingleLine)
                return;
            Writer.Write(Environment.NewLine);
        }

        public void Dispose() => _OptionsReloadToken?.Dispose();
    }
}
