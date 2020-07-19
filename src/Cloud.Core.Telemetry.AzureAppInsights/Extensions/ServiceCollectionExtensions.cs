namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Cloud.Core;
    using Cloud.Core.Telemetry.AzureAppInsights;
    using Configuration;
    using Logging;

    /// <summary>
    /// Class Service Collection extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the application insights telemetry singlton (ITelemetry).
        /// </summary>
        /// <param name="services">The service collection to extend.</param>
        /// <param name="maskSensitiveData">Data being logged should be masked if marked PersonalData or SensitiveInfo.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddAppInsightsTelemetry(this IServiceCollection services, bool maskSensitiveData = true)
        {
            services.AddSingleton<ITelemetryLogger>((a) =>
            {
                var config = a.GetService<IConfiguration>();

                var key = LoggingBuilderExtension.GetInstrumentationKeyFromConfig(config);

                if (key.IsNullOrEmpty())
                {
                    throw new InvalidOperationException("Could not find \"InstrumentationKey\" in configuration");
                }

                var defaultLevel = config.GetValue<string>("Logging:LogLevel:Default");
                var telemetryLevel = config.GetValue<string>("Logging:LogLevel:Telemetry");

                return new AppInsightsLogger(key, LoggingBuilderExtension.GetLogLevel(defaultLevel, telemetryLevel), maskSensitiveData);
            });

            return services;
        }

        /// <summary>
        /// Adds the application insights telemetry singleton (ITelemetry).
        /// </summary>
        /// <param name="services">The service collection to extend.</param>
        /// <param name="insturmentationKey">The instrumentation key.</param>
        /// <param name="maskSensitiveData">Data being logged should be masked if marked PersonalData or SensitiveInfo.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddAppInsightsTelemetry(this IServiceCollection services, string insturmentationKey, bool maskSensitiveData = true)
        {
            services.AddSingleton<ITelemetryLogger>((a) =>
            {
                var config = a.GetService<IConfiguration>();

                var defaultLevel = config.GetValue<string>("Logging:LogLevel:Default");
                var telemetryLevel = config.GetValue<string>("Logging:LogLevel:Telemetry");

                return new AppInsightsLogger(insturmentationKey, LoggingBuilderExtension.GetLogLevel(defaultLevel, telemetryLevel), maskSensitiveData);
            });

            return services;
        }
    }
}
