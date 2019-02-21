# **Cloud.Core.Telemetry.AzureAppInsights**

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

## Test Coverage
A threshold will be added to this package to ensure the test coverage is above 80% for branches, functions and lines.  If it's not above the required threshold 
(threshold that will be implemented on ALL of the new core repositories going forward), then the build will fail.

## Compatibility
This package has has been written in .net Standard and can be therefore be referenced from a .net Core or .net Framework application. The advantage of utilising from a .net Core application, 
is that it can be deployed and run on a number of host operating systems, such as Windows, Linux or OSX.  Unlike referencing from the a .net Framework application, which can only run on 
Windows (or Linux using Mono).
 
## Setup
This package requires the .net Core 2.1 SDK, it can be downloaded here: 
https://www.microsoft.com/net/download/dotnet-core/2.1

IDE of Visual Studio or Visual Studio Code, can be downloaded here:
https://visualstudio.microsoft.com/downloads/

## How to access this package
All of the Cloud.Core.* packages are published to our internal NuGet feed.  To consume this on your local development machine, please add the following feed to your feed sources in Visual Studio:
TBC

For help setting up, follow this article: https://docs.microsoft.com/en-us/vsts/package/nuget/consume?view=vsts
