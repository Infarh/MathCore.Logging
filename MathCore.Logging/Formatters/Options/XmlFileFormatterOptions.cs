using System.Xml;

namespace MathCore.Logging.Formatters.Options
{
    public class XmlFileFormatterOptions : FileFormatterOptions
    {
        public XmlWriterSettings XmlWritterSettings { get; set; }
    }
}