# **Cloud.Core.Telemetry.AzureAppInsights** 
[![Build status](https://dev.azure.com/cloudcoreproject/CloudCore/_apis/build/status/Cloud.Core%20Packages/Cloud.Core.Telemetry.AzureAppInsights_Package)](https://dev.azure.com/cloudcoreproject/CloudCore/_build/latest?definitionId=10) ![Code Coverage](https://cloud1core.blob.core.windows.net/codecoveragebadges/Cloud.Core.Telemetry.AzureAppInsights-LineCoverage.png) [![Cloud.Core.Telemetry.AzureAppInsights package in Cloud.Core feed in Azure Artifacts](https://feeds.dev.azure.com/cloudcoreproject/dfc5e3d0-a562-46fe-8070-7901ac8e64a0/_apis/public/Packaging/Feeds/8949198b-5c74-42af-9d30-e8c462acada6/Packages/77080b8d-f547-4193-b2a1-6cdfd7eb6719/Badge)](https://dev.azure.com/cloudcoreproject/CloudCore/_packaging?_a=package&feed=8949198b-5c74-42af-9d30-e8c462acada6&package=77080b8d-f547-4193-b2a1-6cdfd7eb6719&preferRelease=true)



<div id="description">
	
Azure specific implementation of telemetry logging.  Uses the ITelemeteryLogger interface from _Cloud.Core_ and can be used in conjunction with the Telemetry Logging
Provider classes found in the _Cloud.Core.Telemetry.Logging_ package.

</div>

## Usage

As the logger follows the base `ILogger` implementation, it's very flexible in how it can be used.  To create an instance, do the following:

```csharp
var logger = new AppInsightsLogger("insightsKey");

logger.LogInformation("Sample information message");
logger.LogWarning("Sample warning message");
logger.LogDebug("Sample debug message");
logger.LogException("Sample exception message");
```

Any of the logging methods can also handle exception, such as:

```csharp
logger.LogWarning(new Exception("Something's gone wrong!"));
```

## Config

There are a number of ways to create an instance of the AppInsights logger, the simpliest and most direct way is the following:

```csharp
var logger = new AppInsightsLogger("instrumentationKey", LogLevel.Debug);
```

However, typical use case is as an instance of the `ITelemetryLogger` interface and with an IServiceCollection (in a Startup.cs file).  Therefore, leveraging the extensions methods that have been built for convenience as follows during the Logger configuration is probably easier:

```csharp
public class Startup
{
    public void ConfigureAppConfiguration(IConfigurationBuilder builder)
    {
        // Read config from config sources...
        builder.UseKubernetesContainerConfig();
    }

    public void ConfigureLogging(IConfiguration config, ILoggingBuilder builder)
    {
        builder.AddConfiguration(config.GetSection("Logging"));

        // Add some default loggers.
        builder.AddConsole();
        builder.AddDebug();

        // Add your app insights telemetry logger here!
        
        // 1. Implicitly using instrumentation key.
        builder.AddAppInsightsLogger();

        // 2. Explicitly specifying the instrumentation key.
        var instrumentationKey = config.GetValue<string>("InstrumentationKey");
        builder.AddAppInsightsLogger(instrumentationKey);
    }

    public void ConfigureServices(IConfiguration config, ILogger logger, IServiceCollection services)
    {
        // Configure services...
    }
}
```

## Logging Metrics and Custom Dimensions

```csharp
ITelemeteryLogger logger = new AppInsightsLogger("instrumentationKey");

// Log metrics.
logger.LogMetric("Metric Name", 100);

// Log custom dimensions.
logger.LogInformation("Some log message", new Dictionary<string, string> { 
	{ "dimension1", "someVal" },
	{ "dimension2", "someOtherVal" }
});
```

If you use the first example (implicitly AddingAppInsightsLogger) without specifying config, the code will look for the instrumentation key in one of the following config settings:

1. _InstrumentationKey_
2. _APPINSIGHTS_INSTRUMENTATIONKEY_
3. _Logging:InstrumentationKey_
4. _AppInsights:InstrumentationKey_

If It cannot find it here, it will throw an invalid argument exception.

The second example (explicit setting of instrumentation key), you pass it an instruementation key you have loaded yourself.

Both examples will look for log level from config specified in the "Logging:LogLevel:Telemetry" setting.  If it cannot find that, it will default to _Logging:LogLevel:Default_ for its setting.  Failing that, it falls back to info.

## Further Reading

Read more about logging providers and filters here:
https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-3.1

## Test Coverage
A threshold will be added to this package to ensure the test coverage is above 80% for branches, functions and lines.  If it's not above the required threshold 
(threshold that will be implemented on ALL of the core repositories to gurantee a satisfactory level of testing), then the build will fail.

## Compatibility
This package has has been written in .net Standard and can be therefore be referenced from a .net Core or .net Framework application. The advantage of utilising from a .net Core application, 
is that it can be deployed and run on a number of host operating systems, such as Windows, Linux or OSX.  Unlike referencing from the a .net Framework application, which can only run on 
Windows (or Linux using Mono).
 
## Setup
This package is built using .net Standard 2.1 and requires the .net Core 3.1 SDK, it can be downloaded here: 
https://www.microsoft.com/net/download/dotnet-core/

IDE of Visual Studio or Visual Studio Code, can be downloaded here:
https://visualstudio.microsoft.com/downloads/

## How to access this package
All of the Cloud.Core.* packages are published to a internal NuGet feed.  To consume this on your local development machine, please add the following feed to your feed sources in Visual Studio:
https://pkgs.dev.azure.com/cloudcoreproject/CloudCore/_packaging/Cloud.Core/nuget/v3/index.json
 
For help setting up, follow this article: https://docs.microsoft.com/en-us/vsts/package/nuget/consume?view=vsts


<img src="https://cloud1core.blob.core.windows.net/icons/cloud_core_small.PNG" />
