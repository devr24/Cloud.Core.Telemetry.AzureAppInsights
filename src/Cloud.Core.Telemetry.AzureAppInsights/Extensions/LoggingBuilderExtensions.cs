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
    public static class LoggingBuilderExtensions
    {
        /// <summary>
        /// Adds the telemetry logger factory method to the logging builder.
        /// </summary>
        /// <param name="builder">The builder to add to.</param>
        /// <returns>The modified builder.</returns>
        public static ILoggingBuilder AddAppInsightsLogger(this ILoggingBuilder builder)
        {
            builder.Services.AddSingleton<ITelemetryLogger>((a) =>
            {
                var config = a.GetService<IConfiguration>();

                var key = config.GetValue<string>("Logging:InstrumentationKey");
                var defaultLevel = config.GetValue<string>("Logging:LogLevel:Default");
                var telemetryLevel = config.GetValue<string>("Logging:LogLevel:Telemetry");
                
                return new AppInsightsLogger(key, GetLogLevel(defaultLevel, telemetryLevel));
            });
    
            builder.Services.AddSingleton<ILoggerProvider>(a => new TelemetryLoggerProvider(a.GetService<ITelemetryLogger>()));
            return builder;
        }

        private static LogLevel GetLogLevel(string defaultLevel, string desiredLevel)
        {
            if (!desiredLevel.IsNullOrEmpty())
                defaultLevel = desiredLevel;
            
            switch (defaultLevel)
            {
                case "Trace": return LogLevel.Trace;
                case "Information": return LogLevel.Information;
                case "Critical": return LogLevel.Critical;
                case "Debug": return LogLevel.Debug;
                case "Error": return LogLevel.Error;
                case "Warning": return LogLevel.Warning;
                default: return LogLevel.None;
            }
        }
    }
}