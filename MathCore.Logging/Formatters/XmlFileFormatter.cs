using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

using MathCore.Logging.Extensions;
using MathCore.Logging.Formatters.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace MathCore.Logging.Formatters
{
    public class XmlFileFormatter : FileFormatter, IDisposable
    {
        private readonly IDisposable _OptionsReloadToken;

        public XmlFileFormatterOptions FormatterOptions { get; set; }

        public XmlFileFormatter(IOptionsMonitor<XmlFileFormatterOptions> options) : base("xml")
        {
            FormatterOptions = options.CurrentValue;
            _OptionsReloadToken = options.OnChange(opt => FormatterOptions = opt);
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

        private void WriteScopeInformation(XmlWriter Writer, IExternalScopeProvider Scope)
        {
            if (!FormatterOptions.IncludeScopes || Scope is null)
                return;
            Writer.WriteStartElement("Scopes");
            Scope.ForEachScope(
                (scope, writer) =>
                {
                    if (scope is IReadOnlyCollection<KeyValuePair<string, object>> values)
                    {
                        writer.WriteStartElement("Message");
                        writer.WriteAttributeString("Scope", scope.ToString());
                        foreach (var value in values)
                            WriteItem(writer, value);
                        writer.WriteEndElement();
                    }
                    else
                        writer.WriteString(ToInvariantString(scope));
                }, Writer);
            Writer.WriteEndElement();
        }

        private static void WriteItem(XmlWriter writer, KeyValuePair<string, object> item)
        {
            var (key, value) = item;
            switch (value)
            {
                case bool bool_value:
                    writer.WriteElementString(key, bool_value.ToString());
                    break;
                case byte byte_value:
                    writer.WriteElementString(key, byte_value.ToString());
                    break;
                case sbyte s_byte_value:
                    writer.WriteElementString(key, s_byte_value.ToString());
                    break;
                case char char_value:
                    writer.WriteElementString(key, new string(char_value, 1));
                    //writer.WriteString(key, MemoryMarshal.CreateSpan(ref char_value, 1));
                    break;
                case decimal decimal_value:
                    writer.WriteElementString(key, decimal_value.ToString());
                    break;
                case double double_value:
                    writer.WriteElementString(key, double_value.ToString());
                    break;
                case float float_value:
                    writer.WriteElementString(key, float_value.ToString());
                    break;
                case int int_value:
                    writer.WriteElementString(key, int_value.ToString());
                    break;
                case uint u_int_value:
                    writer.WriteElementString(key, u_int_value.ToString());
                    break;
                case long long_value:
                    writer.WriteElementString(key, long_value.ToString());
                    break;
                case ulong u_long_value:
                    writer.WriteElementString(key, u_long_value.ToString());
                    break;
                case short short_value:
                    writer.WriteElementString(key, short_value.ToString());
                    break;
                case ushort u_short_value:
                    writer.WriteElementString(key, u_short_value.ToString());
                    break;
                case null:
                    writer.WriteStartElement("null");
                    break;
                default:
                    writer.WriteElementString(key, ToInvariantString(value));
                    break;
            }
        }

        private static string ToInvariantString(object obj) => Convert.ToString(obj, CultureInfo.InvariantCulture);

        public override void Write<TState>(in LogEntry<TState> Entry, IExternalScopeProvider Scope, TextWriter Writer)
        {
            var message = Entry!.Formatter!(Entry.State, Entry.Exception);
            if (Entry.Exception is null && message is null)
                return;
            var log_level = Entry.LogLevel;
            var category = Entry.Category;
            var id = Entry.EventId.Id;
            var exception = Entry.Exception;
            //using (var writer = new Utf8JsonWriter(buffer_writer, FormatterOptions.JsonWriterOptions))

            using var writer = XmlWriter.Create(Writer, FormatterOptions.XmlWritterSettings);
            writer.WriteStartElement("LogItem");
            //writer.WriteStartObject();
            var timestamp_format = FormatterOptions.TimestampFormat;
            if (timestamp_format is not null)
            {
                var date_time_offset = FormatterOptions.UseUtcTimestamp
                    ? DateTimeOffset.UtcNow
                    : DateTimeOffset.Now;
                writer.WriteString("Timestamp", date_time_offset.ToString(timestamp_format));
            }

            //writer.WriteEle();
            writer.WriteNumber("EventId", id);
            writer.WriteString("LogLevel", GetLogLevelString(log_level));
            writer.WriteString("Category", category);
            writer.WriteString("Message", message);
            if (exception is not null)
            {
                var exception_str = exception.ToString();
                if (!FormatterOptions.XmlWritterSettings.Indent)
                    exception_str = exception_str.Replace(Environment.NewLine, " ");
                writer.WriteString("Exception", exception_str);
            }

            if (Entry.State is not null)
            {
                writer.WriteStartElement("State");
                //writer.WriteStartObject("State");
                writer.WriteString("Message", Entry.State.ToString());
                if (Entry.State is IReadOnlyCollection<KeyValuePair<string, object>> state)
                    foreach (var value in state)
                        WriteItem(writer, value);
                writer.WriteEndElement();
                //writer.WriteEndObject();

                
            }

            WriteScopeInformation(writer, Scope);
            writer.WriteEndElement();
            //writer.WriteEndObject();
            writer.Flush();

            //Writer.Write(Encoding.UTF8.GetString(buffer_writer.WrittenMemory.Span));
            Writer.Write(Environment.NewLine);
        }
        public void Dispose() => _OptionsReloadToken?.Dispose();
    }
}
