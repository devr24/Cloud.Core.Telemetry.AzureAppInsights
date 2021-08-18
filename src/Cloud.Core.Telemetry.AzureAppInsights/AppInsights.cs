namespace Cloud.Core.Telemetry.AzureAppInsights
{
    using Diagnostics = System.Diagnostics;
    using System;
    using Microsoft.Extensions.Logging;
    using Extensions;
    using System.Collections.Generic;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights;
    using System.Runtime.CompilerServices;
    using System.Net.Http;
    using Cloud.Core.Extensions;
    using Newtonsoft.Json;

    /// <summary>
    /// Azure specific Application Insights <see cref="ITelemetryLogger" /> implementation.
    /// </summary>
    /// <seealso cref="Cloud.Core.ITelemetryLogger" />
    public class AppInsightsLogger : ITelemetryLogger
    {
        private TelemetryClient _client;
        private readonly bool _maskSensitiveData;

        /// <summary>
        /// Gets the instrumentation key.
        /// </summary>
        /// <value>The instrumentation key.</value>
        public string InstrumentationKey { get; }

        /// <summary>
        /// Gets the log level.
        /// </summary>
        /// <value>The log level.</value>
        public LogLevel LogLevel { get; }

        /// <summary>
        /// Gets the telemetry client.
        /// </summary>
        /// <value>The client.</value>
        internal TelemetryClient Client
        {
            get
            {
                if (_client == null)
                {
                    // Initialise client.
                    var config = TelemetryConfiguration.CreateDefault();
                    config.InstrumentationKey = InstrumentationKey;
                    _client = new TelemetryClient(config);

                    // Default application information - who's running the code, guid for session id and operating system being executed on.
                    Client.Context.User.Id = AppDomain.CurrentDomain.FriendlyName;
                    Client.Context.Session.Id = Guid.NewGuid().ToString();
                    Client.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
                }

                return _client;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppInsightsLogger" /> class.
        /// </summary>
        /// <param name="instrumentationKey">The instrumentation key.</param>
        /// <param name="level">The level.</param>
        /// <param name="maskSensitiveData">Data being logged should be masked if marked PersonalData or SensitiveInfo.</param>
        public AppInsightsLogger(string instrumentationKey, LogLevel level, bool maskSensitiveData = true)
        {
            InstrumentationKey = instrumentationKey;
            LogLevel = level;
            _maskSensitiveData = maskSensitiveData;
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
        /// <returns><c>true</c> if enabled.</returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= LogLevel && logLevel != LogLevel.None;
        }

        /// <summary>
        /// Begins the scope.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="state">The state.</param>
        /// <returns>IDisposable.</returns>
        public IDisposable BeginScope<T>(T state)
        {
            return null;
        }

        /// <inheritdoc />
        public void Log<T>(LogLevel logLevel, EventId eventId, T state, Exception exception, Func<T, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

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

            if (exception != null || logLevel >= LogLevel.Error)
            {
                CreateTelemetryException(logLevel, message, exception, props, string.Empty, string.Empty, -1);
            }
            else
            {
                CreateTelemetryEvent(logLevel, message, props, string.Empty, string.Empty, -1);
            }
        }

        /// <inheritdoc />
        public void LogVerbose(string message, Dictionary<string, string> properties = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
        {
            CreateTelemetryEvent(LogLevel.Information, message, properties, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <inheritdoc />
        public void LogVerbose(string message, object objectToLog, 
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
        {
            var properties = objectToLog.AsFlatStringDictionary(StringCasing.Unchanged, _maskSensitiveData, ".");
            CreateTelemetryEvent(LogLevel.Information, message, properties, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <inheritdoc />
        public void LogInformation(string message, object objectToLog, 
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
        {
            var properties = objectToLog.AsFlatStringDictionary(StringCasing.Unchanged, _maskSensitiveData, ".");
            CreateTelemetryEvent(LogLevel.Information, message, properties, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <inheritdoc />
        public void LogInformation(string message, Dictionary<string, string> properties = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
        {
            CreateTelemetryEvent(LogLevel.Information, message, properties, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <inheritdoc />
        public void LogCritical(string message, Dictionary<string, string> properties = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
        {
            CreateTelemetryEvent(LogLevel.Information, message, properties, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <inheritdoc />
        public void LogCritical(Exception ex, Dictionary<string, string> properties = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
        {
            CreateTelemetryException(LogLevel.Critical, ex?.GetBaseException().Message, ex, properties, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <inheritdoc />
        public void LogCritical(string message, object objectToLog, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
        {
            var properties = objectToLog.AsFlatStringDictionary(StringCasing.Unchanged, _maskSensitiveData, ".");
            CreateTelemetryEvent(LogLevel.Information, message, properties, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <inheritdoc />
        public void LogCritical(Exception ex, object objectToLog, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
        {
            var properties = objectToLog.AsFlatStringDictionary(StringCasing.Unchanged, _maskSensitiveData, ".");
            CreateTelemetryException(LogLevel.Critical, ex?.GetBaseException().Message, ex, properties, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs the debug.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        public void LogDebug(string message, Dictionary<string, string> properties = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
        {
            CreateTelemetryEvent(LogLevel.Debug, message, properties, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs the debug.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        public void LogDebug(Exception ex, Dictionary<string, string> properties = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
        {
            CreateTelemetryException(LogLevel.Debug, ex?.GetBaseException().Message, ex, properties, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <inheritdoc />
        public void LogWarning(string message, Dictionary<string, string> properties = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
        {
            CreateTelemetryEvent(LogLevel.Warning, message, properties, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <inheritdoc />
        public void LogWarning(Exception ex, Dictionary<string, string> properties = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
        {
            CreateTelemetryException(LogLevel.Warning, ex?.GetBaseException().Message, ex, properties, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <inheritdoc />
        public void LogWarning(string message, object objectToLog, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
        {
            var properties = objectToLog.AsFlatStringDictionary(StringCasing.Unchanged, _maskSensitiveData, ".");
            CreateTelemetryEvent(LogLevel.Warning, message, properties, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <inheritdoc />
        public void LogWarning(Exception ex, object objectToLog, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
        {
            var properties = objectToLog.AsFlatStringDictionary(StringCasing.Unchanged, _maskSensitiveData, ".");
            CreateTelemetryException(LogLevel.Warning, ex?.GetBaseException().Message, ex, properties, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <inheritdoc />
        public void LogError(string message, Dictionary<string, string> properties = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
        {
            var telemetryEx = new TelemetryException(message);

            CreateTelemetryException(LogLevel.Error, message, telemetryEx, properties, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <inheritdoc />
        public void LogError(Exception ex, Dictionary<string, string> properties = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
        {
            CreateTelemetryException(LogLevel.Error, ex?.GetBaseException().Message, ex, properties, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <inheritdoc />
        public void LogError(Exception ex, string message, Dictionary<string, string> properties = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
        {
            CreateTelemetryException(LogLevel.Error, message, ex, properties, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <inheritdoc />
        public void LogError(Exception ex, object objectToLog, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
        {
            var properties = objectToLog.AsFlatStringDictionary(StringCasing.Unchanged, _maskSensitiveData, ".");
            CreateTelemetryException(LogLevel.Error, ex?.GetBaseException().Message, ex, properties, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <inheritdoc />
        public void LogError(Exception ex, string message, object objectToLog, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
        {
            var properties = objectToLog.AsFlatStringDictionary(StringCasing.Unchanged, _maskSensitiveData, ".");
            CreateTelemetryException(LogLevel.Error, message, ex, properties, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <inheritdoc />
        public void LogMetric(string metricName, double metricValue,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
        {
            CreateTelemetryMetric(LogLevel.Information, metricName, metricValue, null, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <inheritdoc />
        public void LogMetric(string metricName, double metricValue, Dictionary<string, string> properties,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
        {
            CreateTelemetryMetric(LogLevel.Information, metricName, metricValue, properties, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <inheritdoc />
        public void LogMetric(string metricName, double metricValue, object objectToLog, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
        {
            var properties = objectToLog.AsFlatStringDictionary(StringCasing.Unchanged, _maskSensitiveData, ".");
            CreateTelemetryMetric(LogLevel.Information, metricName, metricValue, properties, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Sets the default properties.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        /// <returns>Dictionary&lt;System.String, System.String&gt;.</returns>
        private Dictionary<string, string> SetDefaultProperties(LogLevel logLevel,
            Dictionary<string, string> properties, string callerMemberName, string callerFilePath, int callerLineNumber)
        {
            // If properties were not set, initialise now and add default properties.
            var output = properties ?? new Dictionary<string, string>();

            output.Add("Telemetry.LogLevel", logLevel.ToString());

            if (!string.IsNullOrEmpty(callerMemberName) && !output.ContainsKey("Telemetry.SummaryMessage"))
            {
                output.Add("Telemetry.MemberName", callerMemberName);
            }

            if (!string.IsNullOrEmpty(callerFilePath) && !output.ContainsKey("Telemetry.SummaryMessage"))
            {
                output.Add("Telemetry.FilePath", System.IO.Path.GetFileName(callerFilePath));
            }

            if (callerLineNumber > 0 && !output.ContainsKey("Telemetry.SummaryMessage"))
            {
                output.Add("Telemetry.LineNumber", callerLineNumber.ToString());
            }

            return output;
        }

        /// <summary>
        /// Creates the telemetry event.
        /// </summary>
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
            {
                return;
            }

            var telemetry = new EventTelemetry(message);
            var output = SetDefaultProperties(level, properties, callerMemberName, callerFilePath, callerLineNumber);

            if (!string.IsNullOrEmpty(message) && !telemetry.Properties.ContainsKey("Telemetry.SummaryMessage"))
            {
                telemetry.Properties.Add("Telemetry.SummaryMessage", message);
            }

            foreach (var property in output)
            {
                if (!telemetry.Properties.ContainsKey(property.Key))
                {
                    telemetry.Properties.Add(property);
                }
            }

            Client.TrackEvent(telemetry);
        }

        /// <summary>
        /// Creates the telemetry exception.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="message">The message to log for the exception.</param>
        /// <param name="ex">The ex.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        private void CreateTelemetryException(LogLevel level, string message, Exception ex, Dictionary<string, string> properties,
            string callerMemberName, string callerFilePath, int callerLineNumber)
        {
            if (!IsEnabled(level))
                return;

            var telemetry = new ExceptionTelemetry(ex) { Message = message };
            var output = SetDefaultProperties(level, properties, callerMemberName, callerFilePath, callerLineNumber);

            if (ex != null)
            {
                if (!string.IsNullOrEmpty(ex.Message) && !telemetry.Properties.ContainsKey("Telemetry.ExceptionMessage"))
                {
                    telemetry.Properties.Add("Telemetry.ExceptionMessage", ex.Message);
                }
            }

            foreach (var property in output)
            {
                if (!telemetry.Properties.ContainsKey(property.Key))
                {
                    telemetry.Properties.Add(property);
                }
            }

            Client.TrackException(telemetry);
            Client.Flush();
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
        private void CreateTelemetryMetric(LogLevel level, string metricName, double metricValue, Dictionary<string, string> properties,
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
            Client.Flush();
        }

        /// <summary>
        /// Returns the customDimesions of the log returned from the query. This is only designed for a query that returns a single log.
        /// </summary>
        /// <typeparam name="T">The customDimensions model of the log. This could be unique for each component logging to AppInsights.</typeparam>
        /// <param name="apiKey">The App Insights API Key.</param>
        /// <param name="appInsightsId">The App Insights Id.</param>
        /// <param name="query">The App Insights query to retrieve the log.</param>
        /// <returns>T.</returns>
        public static T GetCustomDimensions<T>(string apiKey, string appInsightsId, string query) where T : class
        {
            var URL = "http://api.applicationinsights.io/v1/apps/{0}/query?query={1}";

            using var client = new HttpClient();
            
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            var req = string.Format(URL, appInsightsId, query);
            var responseMessage = client.GetAsync(req).Result;

            var result = responseMessage.Content.ReadAsStringAsync().Result;
            var queryResult = JsonConvert.DeserializeObject<BaseLogEntry>(result);

            var customDimensionIndex = queryResult.Tables[0].Columns.FindIndex(log => log.Name == "customDimensions");
            return JsonConvert.DeserializeObject<T>(queryResult.Tables[0].Rows[0][customDimensionIndex].ToString());
        }

        // The following are classes to allow us to deserialize the Json returned from the Http request for GetCustomDimensions. 
        /// <summary>
        /// Class Column.
        /// </summary>
        private class Column
        {
            /// <summary>
            /// Gets the name.
            /// </summary>
            /// <value>The name.</value>
            public string Name { get; set; }
        }

        /// <summary>
        /// Class Table.
        /// </summary>
        private class Table
        {
            /// <summary>
            /// Gets or sets the columns.
            /// </summary>
            /// <value>The columns.</value>
            public List<Column> Columns { get; set; }
            /// <summary>
            /// Gets or sets the rows.
            /// </summary>
            /// <value>The rows.</value>
            public List<List<object>> Rows { get; set; }
        }

        /// <summary>
        /// Class BaseLogEntry.
        /// </summary>
        private class BaseLogEntry
        {
            /// <summary>
            /// Gets or sets the tables.
            /// </summary>
            /// <value>The tables.</value>
            public List<Table> Tables { get; set; }
        }

        /// <summary>
        /// Class TelemetryException.
        /// Implements the <see cref="System.Exception" />
        /// </summary>
        /// <seealso cref="System.Exception" />
        private class TelemetryException : Exception
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TelemetryException"/> class.
            /// </summary>
            /// <param name="message">The message that describes the error.</param>
            public TelemetryException(string message) : base(message) { }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AppInsightsLogger" /> class.
        /// Flushes on finalize.
        /// </summary>
        ~AppInsightsLogger() // finalizer
        {
            Flush();
        }
    }
}
