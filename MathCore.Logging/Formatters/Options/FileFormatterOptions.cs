namespace MathCore.Logging.Formatters.Options
{
    public class FileFormatterOptions
    {
        public bool IncludeScopes { get; set; }

        public string TimestampFormat { get; set; }

        public bool UseUtcTimestamp { get; set; }
    }
}
