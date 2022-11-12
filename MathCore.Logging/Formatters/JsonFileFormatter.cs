using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;

using MathCore.Logging.Formatters.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace MathCore.Logging.Formatters
{
    internal class JsonFileFormatter : FileFormatter, IDisposable
    {
        private readonly IDisposable _OptionsReloadToken;

        public JsonFileFormatterOptions FormatterOptions { get; set; }

        public JsonFileFormatter(IOptionsMonitor<JsonFileFormatterOptions> options) : base("json")
        {
            FormatterOptions = options.CurrentValue;
            _OptionsReloadToken = options.OnChange(opt => FormatterOptions = opt);
        }

        public override void Write<TState>(in LogEntry<TState> Entry, IExternalScopeProvider Scope, TextWriter Writer)
        {
            var message = Entry.Formatter(Entry.State, Entry.Exception);
            if (Entry.Exception is null && message is null)
                return;
            var log_level = Entry.LogLevel;
            var category = Entry.Category;
            var id = Entry.EventId.Id;
            var exception = Entry.Exception;
            using (var buffer_writer = new PooledByteBufferWriter(1024))
            {
                using (var writer = new Utf8JsonWriter(buffer_writer, FormatterOptions.JsonWriterOptions))
                {
                    writer.WriteStartObject();
                    var timestamp_format = FormatterOptions.TimestampFormat;
                    if (timestamp_format is not null)
                    {
                        var date_time_offset = FormatterOptions.UseUtcTimestamp 
                            ? DateTimeOffset.UtcNow 
                            : DateTimeOffset.Now;
                        writer.WriteString("Timestamp", date_time_offset.ToString(timestamp_format));
                    }
                    writer.WriteNumber("EventId", id);
                    writer.WriteString("LogLevel", GetLogLevelString(log_level));
                    writer.WriteString("Category", category);
                    writer.WriteString("Message", message);
                    if (exception is not null)
                    {
                        var exception_str = exception.ToString();
                        if (!FormatterOptions.JsonWriterOptions.Indented)
                            exception_str = exception_str.Replace(Environment.NewLine, " ");
                        writer.WriteString("Exception", exception_str);
                    }
                    if (Entry.State is not null)
                    {
                        writer.WriteStartObject("State");
                        writer.WriteString("Message", Entry.State.ToString());
                        if (Entry.State is IReadOnlyCollection<KeyValuePair<string, object>> state)
                            foreach (var value in state)
                                WriteItem(writer, value);
                        writer.WriteEndObject();
                    }
                    WriteScopeInformation(writer, Scope);
                    writer.WriteEndObject();
                    writer.Flush();
                }
                //todo: Вернуть обратно Span вместо .ToArray()
                Writer.Write(Encoding.UTF8.GetString(buffer_writer.WrittenMemory.Span.ToArray()));
            }
            Writer.Write(Environment.NewLine);
        }

        private static string GetLogLevelString(LogLevel Level) =>
            Level switch
            {
                LogLevel.Trace => "Trace",
                LogLevel.Debug => "Debug",
                LogLevel.Information => "Information",
                LogLevel.Warning => "Warning",
                LogLevel.Error => "Error",
                LogLevel.Critical => "Critical",
                LogLevel.None => "---",
                _ => throw new ArgumentOutOfRangeException(nameof(Level))
            };

        private void WriteScopeInformation(Utf8JsonWriter Writer, IExternalScopeProvider Scope)
        {
            if (!FormatterOptions.IncludeScopes || Scope is null)
                return;
            Writer.WriteStartArray("Scopes");
            Scope.ForEachScope((scope, writer) =>
            {
                if (scope is IReadOnlyCollection<KeyValuePair<string, object>> values)
                {
                    writer.WriteStartObject();
                    writer.WriteString("Message", scope.ToString());
                    foreach (var value in values)
                        WriteItem(writer, value);
                    writer.WriteEndObject();
                }
                else
                    writer.WriteStringValue(ToInvariantString(scope));
            }, Writer);
            Writer.WriteEndArray();
        }

        private static void WriteItem(Utf8JsonWriter writer, KeyValuePair<string, object> item)
        {
            var (key, value) = item;
            switch (value)
            {
                case bool bool_value:
                    writer.WriteBoolean(key, bool_value);
                    break;
                case byte byte_value:
                    writer.WriteNumber(key, byte_value);
                    break;
                case sbyte s_byte_value:
                    writer.WriteNumber(key, s_byte_value);
                    break;
                case char char_value:
                    writer.WriteString(key, new string(char_value, 1));
                    //writer.WriteString(key, MemoryMarshal.CreateSpan(ref char_value, 1));
                    break;
                case decimal decimal_value:
                    writer.WriteNumber(key, decimal_value);
                    break;
                case double double_value:
                    writer.WriteNumber(key, double_value);
                    break;
                case float float_value:
                    writer.WriteNumber(key, float_value);
                    break;
                case int int_value:
                    writer.WriteNumber(key, int_value);
                    break;
                case uint u_int_value:
                    writer.WriteNumber(key, u_int_value);
                    break;
                case long long_value:
                    writer.WriteNumber(key, long_value);
                    break;
                case ulong u_long_value:
                    writer.WriteNumber(key, u_long_value);
                    break;
                case short short_value:
                    writer.WriteNumber(key, short_value);
                    break;
                case ushort u_short_value:
                    writer.WriteNumber(key, u_short_value);
                    break;
                case null:
                    writer.WriteNull(key);
                    break;
                default:
                    writer.WriteString(key, ToInvariantString(value));
                    break;
            }
        }

        private static string ToInvariantString(object obj) => Convert.ToString(obj, CultureInfo.InvariantCulture);

        public void Dispose() => _OptionsReloadToken?.Dispose();
    }
}
