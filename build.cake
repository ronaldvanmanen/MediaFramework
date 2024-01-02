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
// VARIABLES
//////////////////////////////////////////////////////////////////////

var repoRoot = Directory(".");

var solution = repoRoot + File("MultimediaFramework.sln");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean").Does(() => 
{
    DotNetClean(solution);
});

Task("Restore").Does(() =>
{
    var interactive = Argument("Interactive", false);
    var runtime = Argument<string>("Runtime", null);
    DotNetRestore(solution, new DotNetRestoreSettings
    {
        EnvironmentVariables = new Dictionary<string, string>
        {
            { "OVERRIDE_RUNTIME_IDENTIFIER", runtime }
        },
        Interactive = interactive,
        Runtime = runtime
    });
});

Task("Build").Does(() =>
{
    var configuration = Argument<string>("Configuration", null);
    var runtime = Argument<string>("Runtime", null);
    DotNetBuild(solution, new DotNetBuildSettings
    {
        Configuration = configuration,
        EnvironmentVariables = new Dictionary<string, string>
        {
            { "OVERRIDE_RUNTIME_IDENTIFIER", runtime }
        },
        NoRestore = true
    });
});

Task("Test").Does(() =>
{
    var configuration = Argument<string>("Configuration", null);
    var runtime = Argument<string>("Runtime", null);
    DotNetTest(solution, new DotNetTestSettings
    {
        Configuration = configuration,
        EnvironmentVariables = new Dictionary<string, string>
        {
            { "OVERRIDE_RUNTIME_IDENTIFIER", runtime }
        },
        NoBuild = true,
        NoRestore = true
    });
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(Argument<string>("Target"));