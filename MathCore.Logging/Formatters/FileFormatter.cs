using System;
using System.IO;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MathCore.Logging.Formatters
{
    public abstract class FileFormatter
    {
        public string Name { get; }
        protected FileFormatter(string name) => Name = name ?? throw new ArgumentNullException(nameof(name));
        public abstract void Write<TState>(in LogEntry<TState> Entry, IExternalScopeProvider Scope, TextWriter Writer);
    }
}
