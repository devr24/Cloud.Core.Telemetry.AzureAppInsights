using System;
using System.Collections.Generic;
using Cloud.Core.Testing;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Cloud.Core.Telemetry.AzureAppInsights.Tests.Unit
{
    [IsUnit]
    public class AppInsightsTests
    {
        /// <summary>Verify logging is enabled.</summary>
        [Fact]
        public void Test_Telemetry_IsEnabled()
        {
            // Arrange
            var logger = new AppInsightsLogger("test", LogLevel.Trace);
            logger.BeginScope("test");
            logger.Flush();

            // Act/Assert
            Assert.True(logger.IsEnabled(LogLevel.Information));
        }

        /// <summary>Ensure logging warning works as expected.</summary>
        [Fact]
        public void Test_Telemetry_LogWarningMessage()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Trace);

                // Act
                logger.LogWarning("test");
                logger.Flush();
            });
        }

        /// <summary>Ensure logging debug works as expected.</summary>
        [Fact]
        public void Test_Telemetry_LogWithWrongLevel()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Warning);

                // Act
                logger.LogDebug("test");
                logger.Flush();
            });
        }

        /// <summary>Ensure logging critical works as expected.</summary>
        [Fact]
        public void Test_Telemetry_LogCriticalMessage()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Trace);

                // Act
                logger.LogCritical("test");
                logger.Flush();
            });
        }

        /// <summary>Ensure logging information works as expected.</summary>
        [Fact]
        public void Test_Telemetry_LogInformationMessage()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Trace);
                Dictionary<string, string> statistics = new Dictionary<string, string>()
                {
                    {"SomeProp1", "sample1"},
                    {"SomeProp2", "sample2"},
                    {"SomeProp3", "sample3" }
                };

                logger.LogInformation("test title", statistics);
                logger.Log(LogLevel.Information, new EventId(1, null), "A", null, (a, b) =>
                {
                    return string.Format("{0}:{1}", a, b);
                });
                logger.Flush();
            });
        }

        /// <summary>Ensure logging verbose works as expected.</summary>
        [Fact]
        public void Test_Telemetry_LogVerboseMessage()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Trace);

                // Act
                logger.LogVerbose("test");
                logger.Flush();
            });
        }

        /// <summary>Ensure logging error works as expected.</summary>
        [Fact]
        public void Test_Telemetry_LogErrorMessage()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Trace);

                // Act
                logger.LogError("test");
                logger.Flush();
            });
        }

        /// <summary>Ensure logging debug when warning level works as expected.</summary>
        [Fact]
        public void Test_Telemetry_LogWarningDebug()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Trace);

                // Act
                logger.LogDebug(new Exception());
                logger.Flush();
            });
        }

        /// <summary>Ensure logging exception when warning level works as expected.</summary>
        [Fact]
        public void Test_Telemetry_LogWarningException()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Trace);

                // Act
                logger.LogWarning(new Exception());
                logger.Flush();
            });
        }

        /// <summary>Ensure logging critical when warning level works as expected.</summary>
        [Fact]
        public void Test_Telemetry_LoCriticalException()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Trace);

                // Act
                logger.LogCritical(new Exception());
                logger.Flush();
            });
        }

        /// <summary>Ensure logging error works as expected.</summary>
        [Fact]
        public void Test_Telemetry_LogExceptionWithMessage()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Trace);

                // Act
                logger.LogError(new Exception(), "Some error");
                logger.Flush();
            });
        }

        /// <summary>Ensure logging error with custom dimensions.</summary>
        [Fact]
        public void Test_Telemetry_LogExceptionWithMessageAndProperties()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Trace);

                // Act
                logger.LogError(new Exception(), "Some error", new Dictionary<string, string>() { { "test", "test" } });
                logger.Flush();
            });
        }

        /// <summary>Ensure logging error with custom dimensions and format string.</summary>
        [Fact]
        public void Test_Telemetry_ExceptionWithMessageFullParams()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Trace);

                // Act
                logger.LogError(new Exception(), "test", new Dictionary<string, string> { { "a", "a" } }, "", "", 1);
                logger.Flush();
            });
        }

        /// <summary>Ensure logging error with custom dimensions.</summary>
        [Fact]
        public void Test_Telemetry_ExceptionWithDictionary()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Trace);

                // Act
                logger.LogError(new Exception(), new Dictionary<string, string> { { "a", "a" } });
                logger.Flush();
            });
        }

        /// <summary>Ensure logging debug with custom dimensions and format string.</summary>
        [Fact]
        public void Test_Telemetry_LogLevelDebug()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Debug);

                // Act
                logger.Log(LogLevel.Debug, new EventId(1, "test"), "test", new Exception("test"), null);
                logger.Flush();
            });
        }

        /// <summary>Ensure logging error with custom dimensions and format string.</summary>
        [Fact]
        public void Test_Telemetry_LogLevelError()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Error);

                // Act
                logger.Log(LogLevel.Error, new EventId(1, "test"), "test", new Exception("test"), null);
                logger.Flush();
            });
        }

        /// <summary>Ensure logging info with custom dimensions and format string.</summary>
        [Fact]
        public void Test_Telemetry_LogLevelInfo()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Information);

                // Act
                logger.Log(LogLevel.Information, new EventId(1, "test"), "test", null, null);
                logger.Flush();
            });
        }

        /// <summary>Ensure logging error with custom dimensions and format string.</summary>
        [Fact]
        public void Test_Telemetry_LogLevelInfoWithException()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Information);

                // Act
                logger.Log(LogLevel.Error, new EventId(1, "test"), "test", new Exception("test"), null);
                logger.Flush();
            });
        }

        /// <summary>Ensure logging info with custom dimensions no format string.</summary>
        [Fact]
        public void Test_Telemetry_LogLevelInfoWithEvent()
        {
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Information);

                // Act
                logger.Log(LogLevel.Information, new EventId(1, "test"), "test", null, null);
                logger.Flush();
            });
        }

        /// <summary>Ensure logging trace with custom dimensions no format string.</summary>
        [Fact]
        public void Test_Telemetry_LogLevelNoneWithException()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.None);

                // Act
                logger.Log(LogLevel.Trace, new EventId(1, "test"), "test", new Exception(), null);
                logger.Flush();
            });
        }

        /// <summary>Ensure logging none with custom dimensions no format string.</summary>
        [Fact]
        public void Test_Telemetry_LogLevelNone()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.None);

                // Act
                logger.Log(LogLevel.Trace, new EventId(1, "test"), "test", null, null);
                logger.LogError(new Exception(), null, null, null, null);
                logger.LogMetric("test", Double.MinValue);
                logger.Log(LogLevel.Trace, new EventId(1, "test"), "test", null, null);
                logger.Flush();
            });
        }

        /// <summary>Ensure log metrics logs as expected.</summary>
        [Fact]
        public void Test_Telemetry_LogMetric()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Trace);

                // Act
                logger.LogMetric("test", 3.1);
                logger.Flush();
            });
        }

        /// <summary>Ensure log metrics with custom dimensions, logs as expected.</summary>
        [Fact]
        public void Test_Telemetry_LogMetricWithProperties()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Trace);

                // Act
                logger.LogMetric("test", 3.1, new Dictionary<string, string> { { "a", "a" }, { "b", "b" } });
                logger.Flush();
            });
        }

        /// <summary>Ensure logger added with builder extensions logs with expected level of critical.</summary>
        [Fact]
        public void Test_BuilderExtension_Critical()
        {
            // Arrange
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Logging:Instrumentationkey", "test"),
                new KeyValuePair<string, string>("Logging:LogLevel:Default", "Critical")

            }).Build();

            IServiceCollection collection = new ServiceCollection()
                .AddSingleton(config)
                .AddLogging(builder =>
                {
                    builder.AddConfiguration(config);
                    builder.AddAppInsightsLogger();
                });

            var prov = collection.BuildServiceProvider();

            // Act
            var logger = prov.GetService<ITelemetryLogger>() as AppInsightsLogger;
            var logProvider = prov.GetService<ILoggerProvider>();

            // Assert
            logProvider.Should().NotBeNull();
            logger.Should().NotBeNull();
            logger.LogLevel.Should().Be(LogLevel.Critical);
            logger.InstrumentationKey.Should().Be("test");
        }

        /// <summary>Ensure logger added with builder extensions logs with expected level of info and debug.</summary>
        [Fact]
        public void Test_BuilderExtension_Debug()
        {
            // Arrange
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Logging:Instrumentationkey", "test"),
                new KeyValuePair<string, string>("Logging:LogLevel:Default", "Information"),
                new KeyValuePair<string, string>("Logging:LogLevel:Telemetry", "Debug"),

            }).Build();

            IServiceCollection collection = new ServiceCollection()
                .AddSingleton(config)
                .AddLogging(builder =>
                {
                    builder.AddConfiguration(config);
                    builder.AddAppInsightsLogger();
                });

            // Act
            var logger = collection.BuildServiceProvider().GetService<ITelemetryLogger>() as AppInsightsLogger;

            // Assert
            logger.Should().NotBeNull();
            logger.LogLevel.Should().Be(LogLevel.Debug);
            logger.InstrumentationKey.Should().Be("test");
        }

        /// <summary>Ensure logger added with builder extensions logs with expected level of information.</summary>
        [Fact]
        public void Test_BuilderExtension_Information()
        {
            // Arrange
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Logging:Instrumentationkey", "test"),
                new KeyValuePair<string, string>("Logging:LogLevel:Default", "Information"),

            }).Build();

            IServiceCollection collection = new ServiceCollection()
                .AddSingleton(config)
                .AddLogging(builder =>
                {
                    builder.AddConfiguration(config);
                    builder.AddAppInsightsLogger();
                });

            // Act
            var logger = collection.BuildServiceProvider().GetService<ITelemetryLogger>() as AppInsightsLogger;

            // Assert
            logger.Should().NotBeNull();
            logger.LogLevel.Should().Be(LogLevel.Information);
            logger.InstrumentationKey.Should().Be("test");
        }

        /// <summary>Ensure logger added with builder extensions logs with expected level of warning.</summary>
        [Fact]
        public void Test_BuilderExtension_Warning()
        {
            // Arrange
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Logging:Instrumentationkey", "test"),
                new KeyValuePair<string, string>("Logging:LogLevel:Default", "Warning"),

            }).Build();

            IServiceCollection collection = new ServiceCollection()
                .AddSingleton(config)
                .AddLogging(builder =>
                {
                    builder.AddConfiguration(config);
                    builder.AddAppInsightsLogger();
                });

            // Act
            var logger = collection.BuildServiceProvider().GetService<ITelemetryLogger>() as AppInsightsLogger;

            // Assert
            logger.Should().NotBeNull();
            logger.LogLevel.Should().Be(LogLevel.Warning);
            logger.InstrumentationKey.Should().Be("test");
        }

        /// <summary>Ensure logger added with builder extensions logs with expected level of error.</summary>
        [Fact]
        public void Test_BuilderExtension_Error()
        {
            // Arrange
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Logging:Instrumentationkey", "test"),
                new KeyValuePair<string, string>("Logging:LogLevel:Default", "Error"),

            }).Build();

            IServiceCollection collection = new ServiceCollection()
                .AddSingleton(config)
                .AddLogging(builder =>
                {
                    builder.AddConfiguration(config);
                    builder.AddAppInsightsLogger();
                });

            // Act
            var logger = collection.BuildServiceProvider().GetService<ITelemetryLogger>() as AppInsightsLogger;

            // Assert
            logger.Should().NotBeNull();
            logger.LogLevel.Should().Be(LogLevel.Error);
            logger.InstrumentationKey.Should().Be("test");
        }

        /// <summary>Ensure logger added with builder extensions logs with expected level of none.</summary>
        [Fact]
        public void Test_BuilderExtension_None()
        {
            // Arrange
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Logging:Instrumentationkey", "test"),
                new KeyValuePair<string, string>("Logging:LogLevel:Default", "None"),

            }).Build();

            IServiceCollection collection = new ServiceCollection()
                .AddSingleton(config)
                .AddLogging(builder =>
                {
                    builder.AddConfiguration(config);
                    builder.AddAppInsightsLogger();
                });

            // Act
            var logger = collection.BuildServiceProvider().GetService<ITelemetryLogger>() as AppInsightsLogger;

            // Assert
            logger.Should().NotBeNull();
            logger.LogLevel.Should().Be(LogLevel.None);
            logger.InstrumentationKey.Should().Be("test");
        }

        /// <summary>Ensure logger added with builder extensions logs with expected level of trace.</summary>
        [Fact]
        public void Test_BuilderExtension_Trace()
        {
            // Arrange
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Logging:Instrumentationkey", "test"),
                new KeyValuePair<string, string>("Logging:LogLevel:Default", "Trace"),

            }).Build();

            IServiceCollection collection = new ServiceCollection()
                .AddSingleton(config)
                .AddLogging(builder =>
                {
                    builder.AddConfiguration(config);
                    builder.AddAppInsightsLogger();
                });

            // Act
            var logger = collection.BuildServiceProvider().GetService<ITelemetryLogger>() as AppInsightsLogger;

            // Assert
            logger.Should().NotBeNull();
            logger.LogLevel.Should().Be(LogLevel.Trace);
            logger.InstrumentationKey.Should().Be("test");
        }

        /// <summary>Ensure deafult critical statements are logged.</summary>
        [Fact]
        public void Test_BuilderExtensionWithInstrumentationKey_BothDefined()
        {
            // Arrange
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Logging:Instrumentationkey", "testing"),
                new KeyValuePair<string, string>("Logging:LogLevel:Default", "Critical")

            }).Build();

            var instrumentationKey = "test";

            IServiceCollection collection = new ServiceCollection()
                .AddSingleton(config)
                .AddLogging(builder =>
                {
                    builder.AddConfiguration(config);
                    builder.AddAppInsightsLogger(instrumentationKey);
                });

            var prov = collection.BuildServiceProvider();

            // Act
            var logger = prov.GetService<ITelemetryLogger>() as AppInsightsLogger;
            var logProvider = prov.GetService<ILoggerProvider>();

            // Assert
            logProvider.Should().NotBeNull();
            logger.Should().NotBeNull();
            logger.LogLevel.Should().Be(LogLevel.Critical);
            logger.InstrumentationKey.Should().Be("test");
        }

        /// <summary>Ensure critical statements are logged.</summary>
        [Fact]
        public void Test_BuilderExtensionWithInstrumentationKey_Critical()
        {
            // Arrange
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Logging:LogLevel:Default", "Critical")

            }).Build();

            var instrumentationKey = "test";

            IServiceCollection collection = new ServiceCollection()
                .AddSingleton(config)
                .AddLogging(builder =>
                {
                    builder.AddConfiguration(config);
                    builder.AddAppInsightsLogger(instrumentationKey);
                });

            var prov = collection.BuildServiceProvider();

            // Act
            var logger = prov.GetService<ITelemetryLogger>() as AppInsightsLogger;
            var logProvider = prov.GetService<ILoggerProvider>();

            // Assert
            logProvider.Should().NotBeNull();
            logger.Should().NotBeNull();
            logger.LogLevel.Should().Be(LogLevel.Critical);
            logger.InstrumentationKey.Should().Be("test");
        }

        /// <summary>Ensure debug statements are logged.</summary>
        [Fact]
        public void Test_BuilderExtensionWithInstrumentationKey_Debug()
        {
            // Arrange
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Logging:LogLevel:Default", "Information"),
                new KeyValuePair<string, string>("Logging:LogLevel:Telemetry", "Debug"),

            }).Build();

            var instrumentationKey = "test";

            IServiceCollection collection = new ServiceCollection()
                .AddSingleton(config)
                .AddLogging(builder =>
                {
                    builder.AddConfiguration(config);
                    builder.AddAppInsightsLogger(instrumentationKey);
                });

            // Act
            var logger = collection.BuildServiceProvider().GetService<ITelemetryLogger>() as AppInsightsLogger;

            // Assert
            logger.Should().NotBeNull();
            logger.LogLevel.Should().Be(LogLevel.Debug);
            logger.InstrumentationKey.Should().Be("test");
        }

        /// <summary>Ensure info statements are logged.</summary>
        [Fact]
        public void Test_BuilderExtensionWithInstrumentationKey_Information()
        {
            // Arrange
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Logging:LogLevel:Default", "Information"),

            }).Build();

            var instrumentationKey = "test";

            IServiceCollection collection = new ServiceCollection()
                .AddSingleton(config)
                .AddLogging(builder =>
                {
                    builder.AddConfiguration(config);
                    builder.AddAppInsightsLogger(instrumentationKey);
                });

            // Act
            var logger = collection.BuildServiceProvider().GetService<ITelemetryLogger>() as AppInsightsLogger;

            // Assert
            logger.Should().NotBeNull();
            logger.LogLevel.Should().Be(LogLevel.Information);
            logger.InstrumentationKey.Should().Be("test");
        }

        /// <summary>Ensure warning statements are logged.</summary>
        [Fact]
        public void Test_BuilderExtensionWithInstrumentationKey_Warning()
        {
            // Arrange
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Logging:LogLevel:Default", "Warning"),

            }).Build();

            var instrumentationKey = "test";

            IServiceCollection collection = new ServiceCollection()
                .AddSingleton(config)
                .AddLogging(builder =>
                {
                    builder.AddConfiguration(config);
                    builder.AddAppInsightsLogger(instrumentationKey);
                });

            // Act
            var logger = collection.BuildServiceProvider().GetService<ITelemetryLogger>() as AppInsightsLogger;

            // Assert
            logger.Should().NotBeNull();
            logger.LogLevel.Should().Be(LogLevel.Warning);
            logger.InstrumentationKey.Should().Be("test");
        }

        /// <summary>Ensure error statements are logged.</summary>
        [Fact]
        public void Test_BuilderExtensionWithInstrumentationKey_Error()
        {
            // Arrange
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Logging:LogLevel:Default", "Error"),

            }).Build();

            var instrumentationKey = "test";

            IServiceCollection collection = new ServiceCollection()
                .AddSingleton(config)
                .AddLogging(builder =>
                {
                    builder.AddConfiguration(config);
                    builder.AddAppInsightsLogger(instrumentationKey);
                });

            // Act
            var logger = collection.BuildServiceProvider().GetService<ITelemetryLogger>() as AppInsightsLogger;

            // Assert
            logger.Should().NotBeNull();
            logger.LogLevel.Should().Be(LogLevel.Error);
            logger.InstrumentationKey.Should().Be("test");
        }

        /// <summary>Ensure level none statements are logged.</summary>
        [Fact]
        public void Test_BuilderExtensionWithInstrumentationKey_None()
        {
            // Arrange
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Logging:LogLevel:Default", "None"),

            }).Build();

            var instrumentationKey = "test";

            IServiceCollection collection = new ServiceCollection()
                .AddSingleton(config)
                .AddLogging(builder =>
                {
                    builder.AddConfiguration(config);
                    builder.AddAppInsightsLogger(instrumentationKey);
                });

            // Act
            var logger = collection.BuildServiceProvider().GetService<ITelemetryLogger>() as AppInsightsLogger;

            // Assert
            logger.Should().NotBeNull();
            logger.LogLevel.Should().Be(LogLevel.None);
            logger.InstrumentationKey.Should().Be("test");
        }

        /// <summary>Ensure trace statements are logged.</summary>
        [Fact]
        public void Test_BuilderExtensionWithInstrumentationKey_Trace()
        {
            // Arrange
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Logging:LogLevel:Default", "Trace"),

            }).Build();

            var instrumentationKey = "test";

            IServiceCollection collection = new ServiceCollection()
                .AddSingleton(config)
                .AddLogging(builder =>
                {
                    builder.AddConfiguration(config);
                    builder.AddAppInsightsLogger(instrumentationKey);
                });

            // Act
            var logger = collection.BuildServiceProvider().GetService<ITelemetryLogger>() as AppInsightsLogger;

            // Assert
            logger.Should().NotBeNull();
            logger.LogLevel.Should().Be(LogLevel.Trace);
            logger.InstrumentationKey.Should().Be("test");
        }

        /// <summary>Ensure logging Warning works as expected with an object and message.</summary>
        [Fact]
        public void Test_Telemetry_LogWarningMessageWithObject()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Warning);

                // Act
                logger.LogWarning("test", new Test
                {
                    PropA = "propA",
                    PropB = 1,
                    PropC = true,
                    PropD = new SubItem
                    {
                        PropE = "propE",
                        PropF = new List<int> { 1, 2, 3 }
                    }
                });
                logger.Flush();
            });
        }

        /// <summary>Ensure logging Warning works as expected with an object and exception.</summary>
        [Fact]
        public void Test_Telemetry_LogWarningExceptionWithObject()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Warning);

                // Act
                logger.LogWarning(new Exception("test"), new Test
                {
                    PropA = "propA",
                    PropB = 1,
                    PropC = true,
                    PropD = new SubItem
                    {
                        PropE = "propE",
                        PropF = new List<int> { 1, 2, 3 }
                    }
                });
                logger.Flush();
            });
        }

        /// <summary>Ensure logging critical works as expected with an object and message.</summary>
        [Fact]
        public void Test_Telemetry_LogCriticalMessageWithObject()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Critical);

                // Act
                logger.LogCritical("test", new Test
                {
                    PropA = "propA",
                    PropB = 1,
                    PropC = true,
                    PropD = new SubItem
                    {
                        PropE = "propE",
                        PropF = new List<int> { 1, 2, 3 }
                    }
                });
                logger.Flush();
            });
        }

        /// <summary>Ensure logging critical works as expected with an object and exception.</summary>
        [Fact]
        public void Test_Telemetry_LogCriticalExceptionWithObject()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Critical);

                // Act
                logger.LogCritical(new Exception("test"), new Test
                {
                    PropA = "propA",
                    PropB = 1,
                    PropC = true,
                    PropD = new SubItem
                    {
                        PropE = "propE",
                        PropF = new List<int> { 1, 2, 3 }
                    }
                });
                logger.Flush();
            });
        }

        /// <summary>Ensure logging error works as expected with an object and message.</summary>
        [Fact]
        public void Test_Telemetry_LogErrorMessageWithObject()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Error);

                // Act
                logger.LogError("test", new Test
                {
                    PropA = "propA",
                    PropB = 1,
                    PropC = true,
                    PropD = new SubItem
                    {
                        PropE = "propE",
                        PropF = new List<int> { 1, 2, 3 }
                    }
                });
                logger.Flush();
            });
        }

        /// <summary>Ensure logging error works as expected with an object and exception.</summary>
        [Fact]
        public void Test_Telemetry_LogErrorExceptionWithObject()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Error);

                // Act
                logger.LogError(new Exception("test"), new Test
                {
                    PropA = "propA",
                    PropB = 1,
                    PropC = true,
                    PropD = new SubItem
                    {
                        PropE = "propE",
                        PropF = new List<int> { 1, 2, 3 }
                    }
                });
                logger.Flush();
            });
        }

        /// <summary>Ensure logging metric works as expected with an object.</summary>
        [Fact]
        public void Test_Telemetry_LogMetricWithObject()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Information);

                // Act
                logger.LogMetric("test", 5, new Test
                {
                    PropA = "propA",
                    PropB = 1,
                    PropC = true,
                    PropD = new SubItem
                    {
                        PropE = "propE",
                        PropF = new List<int> { 1, 2, 3 }
                    }
                });
                logger.Flush();
            });
        }

        /// <summary>Ensure logging verbose works as expected with an object.</summary>
        [Fact]
        public void Test_Telemetry_LogVerboseWithObject()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Debug);

                // Act
                logger.LogVerbose("test", new Test
                {
                    PropA = "propA",
                    PropB = 1,
                    PropC = true,
                    PropD = new SubItem
                    {
                        PropE = "propE",
                        PropF = new List<int> { 1, 2, 3 }
                    }
                });
                logger.Flush();
            });
        }

        /// <summary>Ensure logging info works as expected with an object.</summary>
        [Fact]
        public void Test_Telemetry_LogInfoWithObject()
        {
            // Assert
            AssertExtensions.DoesNotThrow(() =>
            {
                // Arrange
                var logger = new AppInsightsLogger("test", LogLevel.Debug);

                // Act
                logger.LogInformation("test", new Test
                {
                    PropA = "propA",
                    PropB = 1,
                    PropC = true,
                    PropD = new SubItem
                    {
                        PropE = "propE",
                        PropF = new List<int> { 1, 2, 3 }
                    }
                });
                logger.Flush();
            });
        }

        public class Test
        {
            public string PropA { get; set; }
            public int PropB { get; set; }
            public bool PropC { get; set; }
            public SubItem PropD { get; set; }
        }

        public class SubItem
        {
            public string PropE { get; set; }
            public List<int> PropF { get; set; }
        }
    }
}
