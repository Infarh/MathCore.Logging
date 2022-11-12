using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MathCore.Logging.Extensions
{
    internal static class XmlWriterEx
    {
        public static void WriteNumber(this XmlWriter writer, string ElementName, int Value) => writer.WriteString(ElementName, Value.ToString());

        public static void WriteString(this XmlWriter writer, string ElementName, string Value)
        {
            writer.WriteStartElement(ElementName);
            writer.WriteString(Value);
            writer.WriteEndElement();
        }
    }
}
