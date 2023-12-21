//////////////////////////////////////////////////////////////////////
// ADD-INS
//////////////////////////////////////////////////////////////////////

#addin nuget:?package=Cake.FileHelpers&version=6.1.3

//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////

#tool dotnet:?package=GitVersion.Tool&version=5.10.3

//////////////////////////////////////////////////////////////////////
// USINGS
//////////////////////////////////////////////////////////////////////

using Cake.Common.IO.Paths;
using Cake.Common.Net;

//////////////////////////////////////////////////////////////////////
// METHODS
//////////////////////////////////////////////////////////////////////

int InstallDotNetOnWindows(string architecture, string channel, string installDir, string version)
{
    const string dotnetInstallScript = "dotnet-install.ps1";
        
    DownloadFile($"https://dot.net/v1/{dotnetInstallScript}", $"{dotnetInstallScript}");

    var processArguments = new ProcessArgumentBuilder()
        .Append($"-ExecutionPolicy ByPass")
        .Append($"-File {dotnetInstallScript}");

    if (!string.IsNullOrWhiteSpace(architecture))
    {
        processArguments.Append($"-Architecture {architecture}");
    }
    
    if (!string.IsNullOrWhiteSpace(channel))
    {
        processArguments.Append($"-Channel {channel}");
    }

    if (!string.IsNullOrWhiteSpace(installDir))
    {
        processArguments.Append($"-InstallDir {installDir}");
    }

    if (!string.IsNullOrWhiteSpace(version))
    {
        processArguments.Append($"-Version {version}");
    }

    var processEnvironmentVariables = new Dictionary<string, string>
    {
        { "DOTNET_CLI_TELEMETRY_OPTOUT", "1" },
        { "DOTNET_MULTILEVEL_LOOKUP", "1" },
        { "DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "1" }
    };

    var processSettings = new ProcessSettings
    {
        Arguments = processArguments,
        EnvironmentVariables = processEnvironmentVariables
    };

    return StartProcess("powershell.exe", processSettings);
}

int InstallDotNetOnLinuxOrMacOS(string architecture, string channel, string installDir, string version)
{
    const string dotnetInstallScript = "dotnet-install.sh";
        
    DownloadFile($"https://dot.net/v1/{dotnetInstallScript}", $"{dotnetInstallScript}");

    var processArguments = new ProcessArgumentBuilder().Append($"{dotnetInstallScript}");

    if (!string.IsNullOrWhiteSpace(architecture))
    {
        processArguments.Append($"--architecture {architecture}");
    }
    
    if (!string.IsNullOrWhiteSpace(channel))
    {
        processArguments.Append($"--channel {channel}");
    }

    if (!string.IsNullOrWhiteSpace(installDir))
    {
        processArguments.Append($"--install-dir {installDir}");
    }

    if (!string.IsNullOrWhiteSpace(version))
    {
        processArguments.Append($"--version {version}");
    }

    var processEnvironmentVariables = new Dictionary<string, string>()
    {
        { "DOTNET_CLI_TELEMETRY_OPTOUT", "1" },
        { "DOTNET_MULTILEVEL_LOOKUP", "1" },
        { "DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "1" }
    };

    var processSettings = new ProcessSettings
    {
        Arguments = processArguments,
        EnvironmentVariables = processEnvironmentVariables
    };

    return StartProcess("bash", processSettings);
}

//////////////////////////////////////////////////////////////////////
// VARIABLES
//////////////////////////////////////////////////////////////////////

var repoRoot = Directory(".");

var solution = repoRoot + File("MultimediaFramework.sln");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Install-DotNet").Does(() =>
{
    var architecture = Argument<string>("Architecture", null);
    var channel = Argument<string>("Channel", null);
    var installDir = Argument<string>("InstallDir", null);
    var version = Argument<string>("Version", null);

    var exitCode = 0;
    if (IsRunningOnWindows())
    {
        exitCode = InstallDotNetOnWindows(architecture, channel, installDir, version);
    }
    else
    {
        exitCode = InstallDotNetOnLinuxOrMacOS(architecture, channel, installDir, version);
    }

    if (exitCode != 0)
    {
        throw new Exception($"The DotNet install script returned an error code: {exitCode}");
    }
});

Task("Clean").Does(() => 
{
    DotNetClean(solution);
});

Task("Restore").Does(() =>
{
    DotNetRestore(solution, new DotNetRestoreSettings
    {
        Interactive = Argument("Interactive", false),
        Runtime = Argument<string>("Runtime", null),
        ToolPath = Argument<string>("DotNetRoot", null)
    });
});

Task("Build").Does(() =>
{
    DotNetBuild(solution, new DotNetBuildSettings
    {
        Configuration = Argument<string>("Configuration", null),
        NoRestore = true,
        ToolPath = Argument<string>("DotNetRoot", null)
    });
});

Task("Test").Does(() =>
{
    DotNetTest(solution, new DotNetTestSettings
    {
        Configuration = Argument<string>("Configuration", null),
        Runtime = Argument<string>("Runtime", null),
        NoBuild = true,
        NoRestore = true,
        ToolPath = Argument<string>("DotNetRoot", null)
    });
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(Argument<string>("Target"));