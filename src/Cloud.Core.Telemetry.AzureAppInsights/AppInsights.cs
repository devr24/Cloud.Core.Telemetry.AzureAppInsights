namespace Cloud.Core.Telemetry.AzureAppInsights
{
    using Diagnostics = System.Diagnostics;
    using System;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Azure specific Application Insights <see cref="ITelemetryLogger"/> implementation.
    /// </summary>
    /// <seealso cref="Cloud.Core.ITelemetryLogger" />
    public class AppInsightsLogger : ITelemetryLogger
    {
        private readonly string _instrumentationKey;
        private readonly LogLevel _level;

        private TelemetryClient _client;

        public string InstrumentationKey => _instrumentationKey;
        public LogLevel LogLevel => _level;

        internal TelemetryClient Client
        {
            get
            {
                if (_client == null)
                {
                    // Initialise client.
                    TelemetryConfiguration.Active.InstrumentationKey = _instrumentationKey;
                    _client = new TelemetryClient();

                    // Default application information - who's running the code, guid for session id and operating system being executed on.
                    Client.Context.User.Id = AppDomain.CurrentDomain.FriendlyName;
                    Client.Context.Session.Id = Guid.NewGuid().ToString();
                    Client.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
                }

                return _client;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppInsightsLogger"/> class.
        /// </summary>
        public AppInsightsLogger(string instrumentationKey, LogLevel level)
        {
            _instrumentationKey = instrumentationKey;
            _level = level;
        }

        /// <inheritdoc />
        public void Flush()
        {
            Client.Flush();
        }

        /// <summary>
        /// Checks if the given <paramref name="logLevel" /> is enabled.
        /// </summary>
        /// <param name="logLevel">level to be checked.</param>
        /// <returns>
        ///   <c>true</c> if enabled.
        /// </returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _level;
        }

        /// <summary>
        /// Begins the scope.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        public IDisposable BeginScope<T>(T state)
        {
            return null;
        }

        /// <inheritdoc />
        public void Log<T>(LogLevel logLevel, EventId eventId, T state, Exception exception,
            Func<T, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter != null ? formatter(state, exception) : state.ToString();

            if (string.IsNullOrEmpty(eventId.Name))
            {
                var frame = new Diagnostics.StackFrame(5); // Default stack level which equates to the actual calling code...

                var method = frame.GetMethod();

                if (method.IsNullOrDefault())
                {
                    frame = new Diagnostics.StackFrame(4);
                    method = frame.GetMethod();
                }

                var type = method.DeclaringType;
                var name = method.Name;

                eventId = new EventId(
                    eventId.Id == 0 ? BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0) : eventId.Id,
                    $"{type}:{name}");
            }

            var props = new Dictionary<string, string>
            {
                {"Telemetry.EventId", eventId.Id.ToString()},
                {"Telemetry.EventName", eventId.Name}
            };

            if (exception != null)
            {
                CreateTelemetryException(logLevel, exception, props, string.Empty, string.Empty, -1);
            }
            else
            {
                CreateTelemetryEvent(logLevel, message, props, string.Empty, string.Empty, -1);
            }
        }

        /// <inheritdoc />
        public void LogVerbose(string message, Dictionary<string, string> properties = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = -1)
        {
            CreateTelemetryEvent(LogLevel.Information, message, properties, callerMemberName, callerFilePath,
                callerLineNumber);
        }

        /// <inheritdoc />
        public void LogInformation(string message, Dictionary<string, string> properties = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = -1)
        {
            CreateTelemetryEvent(LogLevel.Information, message, properties, callerMemberName, callerFilePath,
                callerLineNumber);
        }

        /// <inheritdoc />
        public void LogCritical(string message, Dictionary<string, string> properties = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = -1)
        {
            CreateTelemetryEvent(LogLevel.Information, message, properties, callerMemberName, callerFilePath,
                callerLineNumber);
        }

        /// <inheritdoc />
        public void LogCritical(Exception ex, Dictionary<string, string> properties = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = -1)
        {
            CreateTelemetryException(LogLevel.Critical, ex, properties, callerMemberName, callerFilePath,
                callerLineNumber);
        }

        /// <summary>Logs the debug.</summary>
        /// <param name="message">The message.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        public void LogDebug(string message, Dictionary<string, string> properties = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = -1)
        {
            CreateTelemetryEvent(LogLevel.Debug, message, properties, callerMemberName, callerFilePath,
                callerLineNumber);
        }

        /// <summary>Logs the debug.</summary>
        /// <param name="ex">The ex.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        public void LogDebug(Exception ex, Dictionary<string, string> properties = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = -1)
        {
            CreateTelemetryException(LogLevel.Debug, ex, properties, callerMemberName, callerFilePath,
                callerLineNumber);
        }

        /// <inheritdoc />
        public void LogWarning(string message, Dictionary<string, string> properties = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = -1)
        {
            CreateTelemetryEvent(LogLevel.Warning, message, properties, callerMemberName, callerFilePath,
                callerLineNumber);
        }

        /// <inheritdoc />
        public void LogWarning(Exception ex, Dictionary<string, string> properties = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = -1)
        {
            CreateTelemetryException(LogLevel.Warning, ex, properties, callerMemberName, callerFilePath,
                callerLineNumber);
        }

        /// <inheritdoc />
        public void LogError(string message, Dictionary<string, string> properties = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = -1)
        {
            CreateTelemetryEvent(LogLevel.Error, message, properties, callerMemberName, callerFilePath,
                callerLineNumber);
        }

        /// <inheritdoc />
        public void LogError(Exception ex, Dictionary<string, string> properties = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = -1)
        {
            CreateTelemetryException(LogLevel.Error, ex, properties, callerMemberName, callerFilePath,
                callerLineNumber);
        }

        /// <inheritdoc />
        public void LogMetric(string metricName, double metricValue,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = -1)
        {
            CreateTelemetryMetric(LogLevel.Information, metricName, metricValue, null, callerMemberName, callerFilePath,
                callerLineNumber);
        }

        /// <summary>
        /// Sets the default properties.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        /// <returns></returns>
        private Dictionary<string, string> SetDefaultProperties(LogLevel logLevel,
            Dictionary<string, string> properties,
            string callerMemberName, string callerFilePath, int callerLineNumber)
        {
            // If properties were not set, initialise now and add default properties.
            var output = properties == null ? new Dictionary<string, string>() : new Dictionary<string, string>(properties);

            output.Add("Telemetry.LogLevel", logLevel.ToString());

            if (!string.IsNullOrEmpty(callerMemberName) && !output.ContainsKey("Telemetry.SummaryMessage"))
                output.Add("Telemetry.MemberName", callerMemberName);

            if (!string.IsNullOrEmpty(callerFilePath) && !output.ContainsKey("Telemetry.SummaryMessage"))
                output.Add("Telemetry.FilePath", System.IO.Path.GetFileName(callerFilePath));

            if (callerLineNumber > 0 && !output.ContainsKey("Telemetry.SummaryMessage"))
                output.Add("Telemetry.LineNumber", callerLineNumber.ToString());

            return output;
        }

        /// <summary>Creates the telemetry event.</summary>
        /// <param name="level">The level.</param>
        /// <param name="message">The message.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        private void CreateTelemetryEvent(LogLevel level, string message, Dictionary<string, string> properties,
            string callerMemberName, string callerFilePath, int callerLineNumber)
        {
            if (!IsEnabled(level))
                return;

            var telemetry = new EventTelemetry(message);

            var output = SetDefaultProperties(level, properties, callerMemberName, callerFilePath, callerLineNumber);

            if (!string.IsNullOrEmpty(message) && !telemetry.Properties.ContainsKey("Telemetry.SummaryMessage"))
                telemetry.Properties.Add("Telemetry.SummaryMessage", message);

            foreach (var property in output)
            {
                if (!telemetry.Properties.ContainsKey(property.Key))
                    telemetry.Properties.Add(property);
            }

            Client.TrackEvent(telemetry);
        }

        /// <summary>
        /// Creates the telemetry exception.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="ex">The ex.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        private void CreateTelemetryException(LogLevel level, Exception ex, Dictionary<string, string> properties,
            string callerMemberName, string callerFilePath, int callerLineNumber)
        {
            if (!IsEnabled(level))
                return;

            var telemetry = new ExceptionTelemetry(ex);

            var output = SetDefaultProperties(level, properties, callerMemberName, callerFilePath, callerLineNumber);

            if (!string.IsNullOrEmpty(ex.Message) && !telemetry.Properties.ContainsKey("Telemetry.ExceptionMessage"))
                telemetry.Properties.Add("Telemetry.ExceptionMessage", ex.Message);

            foreach (var property in output)
            {
                if (!telemetry.Properties.ContainsKey(property.Key))
                    telemetry.Properties.Add(property);
            }

            Client.TrackException(telemetry);
        }

        /// <summary>
        /// Creates the telemetry metric.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="metricName">Name of the metric.</param>
        /// <param name="metricValue">The metric value.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        private void CreateTelemetryMetric(LogLevel level, string metricName, double metricValue,
            Dictionary<string, string> properties,
            string callerMemberName, string callerFilePath, int callerLineNumber)
        {
            if (!IsEnabled(level))
                return;

            var telemetry = new MetricTelemetry(metricName, metricValue);

            var output = SetDefaultProperties(level, properties, callerMemberName, callerFilePath, callerLineNumber);

            if (!string.IsNullOrEmpty(metricName) && !telemetry.Properties.ContainsKey("Telemetry.Metric"))
                telemetry.Properties.Add("Telemetry.Metric", metricName);

            foreach (var property in output)
            {
                if (!telemetry.Properties.ContainsKey(property.Key))
                    telemetry.Properties.Add(property);
            }

            Client.TrackMetric(telemetry);
        }
    }
}