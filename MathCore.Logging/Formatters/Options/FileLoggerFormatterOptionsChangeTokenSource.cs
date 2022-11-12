using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace MathCore.Logging.Formatters.Options
{
    internal class FileLoggerFormatterOptionsChangeTokenSource<TFormatter, TOptions> :
        ConfigurationChangeTokenSource<TOptions>
        where TFormatter : FileFormatter
        where TOptions : FileFormatterOptions
    {
        public FileLoggerFormatterOptionsChangeTokenSource(ILoggerProviderConfiguration<FileLoggerProvider> LoggerProviderConfig)
            : base(LoggerProviderConfig.Configuration.GetSection("FormatterOptions"))
        {
        }
    }
}