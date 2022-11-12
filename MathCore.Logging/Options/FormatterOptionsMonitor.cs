using System;

using MathCore.Logging.Formatters.Options;

using Microsoft.Extensions.Options;

namespace MathCore.Logging.Options
{
    internal class FormatterOptionsMonitor<TOptions> : IOptionsMonitor<TOptions> 
        where TOptions : FileFormatterOptions
    {
        private readonly TOptions _Options;
        public TOptions CurrentValue => _Options;

        public FormatterOptionsMonitor(TOptions Options) => _Options = Options;

        public TOptions Get(string Name) => _Options;

        public IDisposable OnChange(Action<TOptions, string> Listener) => null;
    }
}
