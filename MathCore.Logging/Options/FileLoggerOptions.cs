using Microsoft.Extensions.Logging;

namespace MathCore.Logging.Options
{
    public class FileLoggerOptions
    {
        public LogLevel LogToStandardErrorThreshold { get; set; } = LogLevel.None;

        public string FormatterName { get; set; } = FileFormatterNames.Simple;

        public string FilePath { get; set; } = "events.log";

        public int MaxFileLength { get; set; }
    }
}
