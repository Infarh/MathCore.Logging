using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace MathCore.Logging.Formatters.Options
{
    internal class FileLoggerFormatterConfigureOptions<TFormatter, TOptions> :
        ConfigureFromConfigurationOptions<TOptions>
        where TFormatter : FileFormatter
        where TOptions : FileFormatterOptions
    {
        public FileLoggerFormatterConfigureOptions(ILoggerProviderConfiguration<FileLoggerProvider> LoggerProviderConfig)
            : base(LoggerProviderConfig.Configuration.GetSection("FormatterOptions"))
        {
        }
    }
}
