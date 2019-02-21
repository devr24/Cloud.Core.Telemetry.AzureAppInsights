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
    public class AppInsightsTest
    {
        [Fact, IsUnit]
        public void Test_Telemetry_IsEnabled()
        {
            var logger = new AppInsightsLogger("test", LogLevel.Trace);
            logger.BeginScope("test");
            logger.Flush();
            Assert.True(logger.IsEnabled(LogLevel.Information));
        }

        [Fact, IsUnit]
        public void Test_Telemetry_LogWarningMessage()
        {
            AssertExtensions.DoesNotThrow(() =>
            {
                var logger = new AppInsightsLogger("test", LogLevel.Trace);
                logger.LogWarning("test");
                logger.Flush();
            });
        }

        [Fact, IsUnit]
        public void Test_Telemetry_LogWithWrongLevel()
        {
            AssertExtensions.DoesNotThrow(() =>
            {
                var logger = new AppInsightsLogger("test", LogLevel.Warning);
                logger.LogDebug("test");
                logger.Flush();
            });
        }

        [Fact, IsUnit]
        public void Test_Telemetry_LogCriticalMessage()
        {
            AssertExtensions.DoesNotThrow(() =>
            {
                var logger = new AppInsightsLogger("test", LogLevel.Trace);
                logger.LogCritical("test");
                logger.Flush();
            });
        }

        [Fact, IsUnit]
        public void Test_Telemetry_LogInformationMessage()
        {
            AssertExtensions.DoesNotThrow(() =>
            {
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

        [Fact, IsUnit]
        public void Test_Telemetry_LogVerboseMessage()
        {
            AssertExtensions.DoesNotThrow(() =>
            {
                var logger = new AppInsightsLogger("test", LogLevel.Trace);
                logger.LogVerbose("test");
                logger.Flush();
            });
        }

        [Fact, IsUnit]
        public void Test_Telemetry_LogErrorMessage()
        {
            AssertExtensions.DoesNotThrow(() =>
            {
                var logger = new AppInsightsLogger("test", LogLevel.Trace);
                logger.LogError("test");
                logger.Flush();
            });
        }

        [Fact, IsUnit]
        public void Test_Telemetry_LogWarningDebug()
        {
            AssertExtensions.DoesNotThrow(() =>
            {
                var logger = new AppInsightsLogger("test", LogLevel.Trace);
                logger.LogDebug(new Exception());
                logger.Flush();
            });
        }

        [Fact, IsUnit]
        public void Test_Telemetry_LogWarningException()
        {
            AssertExtensions.DoesNotThrow(() =>
            {
                var logger = new AppInsightsLogger("test", LogLevel.Trace);
                logger.LogWarning(new Exception());
                logger.Flush();
            });
        }

        [Fact, IsUnit]
        public void Test_Telemetry_LoCriticalException()
        {
            AssertExtensions.DoesNotThrow(() =>
            {
                var logger = new AppInsightsLogger("test", LogLevel.Trace);
                logger.LogCritical(new Exception());
                logger.Flush();
            });
        }

        [Fact, IsUnit]
        public void Test_Telemetry_LogExceptionWithMessage()
        {
            AssertExtensions.DoesNotThrow(() =>
            {
                var logger = new AppInsightsLogger("test", LogLevel.Trace);
                logger.LogError(new Exception(), "Some error");
                logger.Flush();
            });
        }

        [Fact, IsUnit]
        public void Test_Telemetry_ExceptionWithMessageObject()
        {
            AssertExtensions.DoesNotThrow(() =>
            {
                var logger = new AppInsightsLogger("test", LogLevel.Trace);
                logger.LogError(new Exception(), "test", new Dictionary<string, string> { { "a", "a" } });
                logger.Flush();
            });
        }

        [Fact, IsUnit]
        public void Test_Telemetry_ExceptionWithMessageFullParams()
        {
            AssertExtensions.DoesNotThrow(() =>
            {
                var logger = new AppInsightsLogger("test", LogLevel.Trace);
                logger.LogError(new Exception(), "test", new Dictionary<string, string> { { "a", "a" } }, "", "", 1);
                logger.Flush();
            });
        }

        [Fact, IsUnit]
        public void Test_Telemetry_ExceptionWithDictionary()
        {
            AssertExtensions.DoesNotThrow(() =>
            {
                var logger = new AppInsightsLogger("test", LogLevel.Trace);
                logger.LogError(new Exception(), new Dictionary<string, string> { { "a", "a" } });
                logger.Flush();
            });
        }

        [Fact, IsUnit]
        public void Test_Telemetry_LogLevelDebug()
        {
            AssertExtensions.DoesNotThrow(() =>
            {
                var logger = new AppInsightsLogger("test", LogLevel.Debug);
                logger.Log(LogLevel.Debug, new EventId(1, "test"), "test", new Exception("test"), null);
                logger.Flush();
            });
        }

        [Fact, IsUnit]
        public void Test_Telemetry_LogLevelError()
        {
            AssertExtensions.DoesNotThrow(() =>
            {
                var logger = new AppInsightsLogger("test", LogLevel.Error);
                logger.Log(LogLevel.Error, new EventId(1, "test"), "test", new Exception("test"), null);
                logger.Flush();
            });
        }

        [Fact, IsUnit]
        public void Test_Telemetry_LogLevelInfo()
        {
            AssertExtensions.DoesNotThrow(() =>
            {
                var logger = new AppInsightsLogger("test", LogLevel.Information);
                logger.Log(LogLevel.Information, new EventId(1, "test"), "test", null, null);
                logger.Flush();
            });
        }

        [Fact, IsUnit]
        public void Test_Telemetry_LogLevelInfoWithException()
        {
            AssertExtensions.DoesNotThrow(() =>
            {
                var logger = new AppInsightsLogger("test", LogLevel.Information);
                logger.Log(LogLevel.Error, new EventId(1, "test"), "test", new Exception("test"), null);
                logger.Flush();
            });
        }

        [Fact, IsUnit]
        public void Test_Telemetry_LogLevelInfoWithEvent()
        {
            AssertExtensions.DoesNotThrow(() =>
            {
                var logger = new AppInsightsLogger("test", LogLevel.Information);
                logger.Log(LogLevel.Information, new EventId(1, "test"), "test", null, null);
                logger.Flush();
            });
        }

        [Fact, IsUnit]
        public void Test_Telemetry_LogLevelNoneWithException()
        {
            AssertExtensions.DoesNotThrow(() =>
            {
                var logger = new AppInsightsLogger("test", LogLevel.None);
                logger.Log(LogLevel.Trace, new EventId(1, "test"), "test", new Exception(), null);
                logger.Flush();
            });
        }

        [Fact, IsUnit]
        public void Test_Telemetry_LogLevelNone()
        {
            AssertExtensions.DoesNotThrow(() =>
            {
                var logger = new AppInsightsLogger("test", LogLevel.None);
                logger.Log(LogLevel.Trace, new EventId(1, "test"), "test", null, null);
                logger.LogError(new Exception(), null, null);
                logger.LogMetric("test", Double.MinValue);
                logger.Log(LogLevel.Trace, new EventId(1, "test"), "test", null, null);
                logger.Flush();
            });
        }

        [Fact, IsUnit]
        public void Test_Telemetry_LogMetric()
        {
            AssertExtensions.DoesNotThrow(() =>
            {
                var logger = new AppInsightsLogger("test", LogLevel.Trace);
                logger.LogMetric("test", 3.1);
                logger.Flush();
            });
        }

        [Fact, IsUnit]
        public void Test_BuilderExtension_Critical()
        {
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

            var logger = prov.GetService<ITelemetryLogger>() as AppInsightsLogger;
            var logProvider = prov.GetService<ILoggerProvider>();

            logProvider.Should().NotBeNull();
            logger.Should().NotBeNull();
            logger.LogLevel.Should().Be(LogLevel.Critical);
            logger.InstrumentationKey.Should().Be("test");
        }

        [Fact, IsUnit]
        public void Test_BuilderExtension_Debug()
        {
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
            
            var logger = collection.BuildServiceProvider().GetService<ITelemetryLogger>() as AppInsightsLogger;

            logger.Should().NotBeNull();
            logger.LogLevel.Should().Be(LogLevel.Debug);
            logger.InstrumentationKey.Should().Be("test");
        }

        [Fact, IsUnit]
        public void Test_BuilderExtension_Information()
        {
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

            var logger = collection.BuildServiceProvider().GetService<ITelemetryLogger>() as AppInsightsLogger;

            logger.Should().NotBeNull();
            logger.LogLevel.Should().Be(LogLevel.Information);
            logger.InstrumentationKey.Should().Be("test");
        }
        
        [Fact, IsUnit]
        public void Test_BuilderExtension_Warning()
        {
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

            var logger = collection.BuildServiceProvider().GetService<ITelemetryLogger>() as AppInsightsLogger;

            logger.Should().NotBeNull();
            logger.LogLevel.Should().Be(LogLevel.Warning);
            logger.InstrumentationKey.Should().Be("test");
        }

        [Fact, IsUnit]
        public void Test_BuilderExtension_Error()
        {
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

            var logger = collection.BuildServiceProvider().GetService<ITelemetryLogger>() as AppInsightsLogger;

            logger.Should().NotBeNull();
            logger.LogLevel.Should().Be(LogLevel.Error);
            logger.InstrumentationKey.Should().Be("test");
        }

        [Fact, IsUnit]
        public void Test_BuilderExtension_None()
        {
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

            var logger = collection.BuildServiceProvider().GetService<ITelemetryLogger>() as AppInsightsLogger;

            logger.Should().NotBeNull();
            logger.LogLevel.Should().Be(LogLevel.None);
            logger.InstrumentationKey.Should().Be("test");
        }

        [Fact, IsUnit]
        public void Test_BuilderExtension_Trace()
        {
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

            var logger = collection.BuildServiceProvider().GetService<ITelemetryLogger>() as AppInsightsLogger;

            logger.Should().NotBeNull();
            logger.LogLevel.Should().Be(LogLevel.Trace);
            logger.InstrumentationKey.Should().Be("test");
        }
    }
}
