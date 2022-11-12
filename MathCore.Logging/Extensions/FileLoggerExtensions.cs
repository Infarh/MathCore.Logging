using System;

using MathCore.Logging;
using MathCore.Logging.Formatters;
using MathCore.Logging.Formatters.Options;
using MathCore.Logging.Options;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging
{
    public static class FileLoggerExtensions
    {
        public static ILoggingBuilder AddFile(this ILoggingBuilder builder)
        {
            builder.AddConfiguration();
            builder.AddFileFormatter<JsonFileFormatter, JsonFileFormatterOptions>();
            builder.AddFileFormatter<SimpleFileFormatter, SimpleFileFormatterOptions>();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>());
            LoggerProviderOptions.RegisterProviderOptions<FileLoggerOptions, FileLoggerProvider>(builder.Services);
            return builder;
        }

        public static ILoggingBuilder AddFile(this ILoggingBuilder builder, Action<FileLoggerOptions> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            builder.AddFile();
            builder.Services.Configure(configure);
            return builder;
        }

        public static ILoggingBuilder AddSimpleFile(this ILoggingBuilder builder) => builder.AddFormatterWithName("simple");

        public static ILoggingBuilder AddSimpleFile(
            this ILoggingBuilder builder,
            Action<SimpleFileFormatterOptions> configure)
            => builder.AddFileWithFormatter(FileFormatterNames.Simple, configure);

        public static ILoggingBuilder AddJsonFile(this ILoggingBuilder builder) => builder.AddFormatterWithName("json");

        public static ILoggingBuilder AddJsonFile(
            this ILoggingBuilder builder,
            Action<JsonFileFormatterOptions> configure)
            => builder.AddFileWithFormatter(FileFormatterNames.Json, configure);

        internal static ILoggingBuilder AddFileWithFormatter<TOptions>(
          this ILoggingBuilder builder,
          string name,
          Action<TOptions> configure)
          where TOptions : FileFormatterOptions
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            builder.AddFormatterWithName(name);
            builder.Services.Configure(configure);
            return builder;
        }

        private static ILoggingBuilder AddFormatterWithName(
            this ILoggingBuilder builder,
            string name) 
            => builder.AddFile(options => options.FormatterName = name);

        public static ILoggingBuilder AddFileFormatter<TFormatter, TOptions>(this ILoggingBuilder builder)
          where TFormatter : FileFormatter
          where TOptions : FileFormatterOptions
        {
            builder.AddConfiguration();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<FileFormatter, TFormatter>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<TOptions>, FileLoggerFormatterConfigureOptions<TFormatter, TOptions>>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IOptionsChangeTokenSource<TOptions>, FileLoggerFormatterOptionsChangeTokenSource<TFormatter, TOptions>>());
            return builder;
        }

        public static ILoggingBuilder AddFileFormatter<TFormatter, TOptions>(
          this ILoggingBuilder builder,
          Action<TOptions> configure)
          where TFormatter : FileFormatter
          where TOptions : FileFormatterOptions
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            builder.AddFileFormatter<TFormatter, TOptions>();
            builder.Services.Configure(configure);
            return builder;
        }
    }
}
