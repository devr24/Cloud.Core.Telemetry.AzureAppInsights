namespace Microsoft.Extensions.Logging
{
    using System;
    using Cloud.Core;
    using Cloud.Core.Telemetry.AzureAppInsights;
    using Cloud.Core.Telemetry.Logging;
    using Microsoft.Extensions.Configuration;
    using DependencyInjection;

    /// <summary>
    /// Telemetry logger factory adds the AddTelemetryLogger method to the ILoggingBuilder.
    /// </summary>
    public static class LoggingBuilderExtension
    {
        /// <summary>
        /// Adds the telemetry logger factory method to the logging builder.
        /// Will attempt to pull "InstrumentationKey" from config values "Logging:InstrumentationKey" or directly from "InstrumentationKey"
        /// </summary>
        /// <param name="builder">The builder to add to.</param>
        /// <param name="maskSensitiveData">Data being logged should be masked if marked PersonalData or SensitiveInfo.</param>
        /// <returns>The modified builder.</returns>
        public static ILoggingBuilder AddAppInsightsLogger(this ILoggingBuilder builder, bool maskSensitiveData = true)
        {
            builder.Services.AddSingleton<ITelemetryLogger>((a) =>
            {
                var config = a.GetService<IConfiguration>();

                var key = GetInstrumentationKeyFromConfig(config);

                if (key.IsNullOrEmpty())
                    throw new InvalidOperationException("Could not find \"InstrumentationKey\" in configuration");

                var defaultLevel = config.GetValue<string>("Logging:LogLevel:Default");
                var telemetryLevel = config.GetValue<string>("Logging:LogLevel:Telemetry");
                
                return new AppInsightsLogger(key, GetLogLevel(defaultLevel, telemetryLevel), maskSensitiveData);
            });
    
            builder.Services.AddSingleton<ILoggerProvider>(a => new TelemetryLoggerProvider(a.GetService<ITelemetryLogger>()));
            return builder;
        }

        /// <summary>
        /// Adds the telemetry logger factory method to the logging builder.
        /// </summary>
        /// <param name="builder">The builder to add to.</param>
        /// <param name="instrumentationKey">Instrumentation key for the Application Insights</param>
        /// <param name="maskSensitiveData">Data being logged should be masked if marked PersonalData or SensitiveInfo.</param>
        /// <returns></returns>
        public static ILoggingBuilder AddAppInsightsLogger(this ILoggingBuilder builder, string instrumentationKey, bool maskSensitiveData = true)
        {
            if (!instrumentationKey.IsNullOrWhiteSpace())
            {
                builder.Services.AddSingleton<ITelemetryLogger>((a) =>
                {
                    var config = a.GetService<IConfiguration>();

                    var defaultLevel = config.GetValue<string>("Logging:LogLevel:Default");
                    var telemetryLevel = config.GetValue<string>("Logging:LogLevel:Telemetry");

                    return new AppInsightsLogger(instrumentationKey, GetLogLevel(defaultLevel, telemetryLevel), maskSensitiveData);
                });

                builder.Services.AddSingleton<ILoggerProvider>(a => new TelemetryLoggerProvider(a.GetService<ITelemetryLogger>()));
            }
            return builder;
        }

        internal static LogLevel GetLogLevel(string defaultLevel, string desiredLevel)
        {
            if (!desiredLevel.IsNullOrEmpty())
                defaultLevel = desiredLevel;

            return defaultLevel switch
            {
                "Trace" => LogLevel.Trace,
                "Information" => LogLevel.Information,
                "Critical" => LogLevel.Critical,
                "Debug" => LogLevel.Debug,
                "Error" => LogLevel.Error,
                "Warning" => LogLevel.Warning,
                _ => LogLevel.None,
            };
        }

        internal static string GetInstrumentationKeyFromConfig(IConfiguration config)
        {
            var key = config.GetValue<string>("AppInsightsInstrumentationKey");

            if (key.IsNullOrEmpty())
                key = config.GetValue<string>("InstrumentationKey");

            if (key.IsNullOrEmpty())
                key = config.GetValue<string>("Logging:InstrumentationKey");

            if (key.IsNullOrEmpty())
                key = config.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY");

            if (key.IsNullOrEmpty())
                key = config.GetValue<string>("AppInsights:InstrumentationKey");
            
            return key;
        }
    }
}
