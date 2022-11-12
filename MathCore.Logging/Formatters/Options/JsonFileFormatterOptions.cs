using System.Text.Json;

namespace MathCore.Logging.Formatters.Options
{
    public class JsonFileFormatterOptions : FileFormatterOptions
    {
        public JsonWriterOptions JsonWriterOptions { get; set; }
    }
}
