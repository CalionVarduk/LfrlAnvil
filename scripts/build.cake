#tool "dotnet:?package=GitVersion.Tool&version=5.12.0"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Pack");
var configuration = Argument("debug-mode", false) ? "Debug" : "Release";
var runTests = Argument("run-tests", true);
var publishOutput = Argument("publish-output", "");
var nugetOutput = Argument("nuget-output", "nuget");
var version = GitVersion(new GitVersionSettings()).NuGetVersion;

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var rootDir = System.IO.Path.Combine(Environment.CurrentDirectory, "..");
var binDirs = System.IO.Path.Combine(rootDir, "**", "bin", configuration);
var objDirs = System.IO.Path.Combine(rootDir, "**", "obj", configuration);
var publishDir = publishOutput.Length > 0 ? System.IO.Path.Combine(rootDir, publishOutput) : "";
var nugetDir = System.IO.Path.Combine(rootDir, nugetOutput);
var solution = System.IO.Path.Combine(rootDir, "LfrlAnvil.sln");
var projects = GetFiles(System.IO.Path.Combine(rootDir, "src", "**", "*.csproj")).ToArray();
var testProjects = GetFiles(System.IO.Path.Combine(rootDir, "tests", "**", "*.Tests.csproj")).ToArray();

Information("Configuration: {0}", configuration);
Information("Run tests: {0}", runTests);
Information("Version: {0}", version);
Information("Solution: '{0}'", solution);
Information("Project count: {0}", projects.Length);
for (var i = 0; i < projects.Length; ++i)
    Information("{0}. '{1}'", i + 1, projects[i].FullPath);

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    Information("Cleaning '{0}' directory...", nugetDir);
    CleanDirectory(nugetDir);
    Information("'{0}' directory cleaned.", nugetDir);

    if (publishDir.Length > 0)
    {
        Information("Cleaning '{0}' directory...", publishDir);
        CleanDirectory(publishDir);
        Information("'{0}' directory cleaned.", publishDir);
    }

    Information("Cleaning '{0}' directories...", binDirs);
    CleanDirectories(binDirs);
    Information("'{0}' directories cleaned.", binDirs);

    Information("Cleaning '{0}' directories...", objDirs);
    CleanDirectories(objDirs);
    Information("'{0}' directories cleaned.", objDirs);
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    var versionArgument = $"/p:Version={version}";

    var settings = new DotNetBuildSettings
    {
        NoRestore = false,
        Configuration = configuration,
        ArgumentCustomization = args => args.Append(versionArgument)
    };
    DotNetBuild(solution, settings);
});

Task("Test")
    .IsDependentOn("Build")
    .WithCriteria(runTests)
    .Does(() =>
{
    var settings = new DotNetTestSettings
    {
        NoBuild = true,
        Configuration = configuration
    };
    foreach (var project in testProjects)
        DotNetTest(project.FullPath, settings);
});

Task("Publish")
      .IsDependentOn("Test")
      .WithCriteria(publishOutput.Length > 0)
      .Does(() =>
{
    foreach (var project in projects)
    {
        var settings = new DotNetPublishSettings
        {
            NoBuild = true,
            Configuration = configuration,
            OutputDirectory = System.IO.Path.Combine(publishDir, project.GetFilenameWithoutExtension().ToString())
        };
        DotNetPublish(project.FullPath, settings);
    }
});

Task("Pack")
      .IsDependentOn("Publish")
      .Does(() =>
{
    var settings = new DotNetPackSettings
    {
        NoBuild = true,
        IncludeSymbols = true,
        Configuration = configuration,
        SymbolPackageFormat = "snupkg",
        OutputDirectory = nugetDir,
        ArgumentCustomization = args => args.Append($"/p:PackageVersion={version}")
    };
    foreach (var project in projects)
        DotNetPack(project.FullPath, settings);
});

Task("Push")
      .IsDependentOn("Pack")
      .Does(() =>
{
    var nugetApiKey = EnvironmentVariable("NUGET_API_KEY");
    if (string.IsNullOrEmpty(nugetApiKey))
        throw new InvalidOperationException("'NUGET_API_KEY' environment variable is required by the 'Publish' task.");

    var nugetSource = "https://api.nuget.org/v3/index.json";

    var packages = GetFiles(System.IO.Path.Combine(nugetDir, "*.nupkg"));
    foreach (var package in packages)
    {
        var settings = new NuGetPushSettings
        {
            Source = nugetSource,
            ApiKey = nugetApiKey,
            SkipDuplicate = true
        };
        NuGetPush(package.FullPath, settings);
    }

    var symbolPackages = GetFiles(System.IO.Path.Combine(nugetDir, "*.snupkg"));
    foreach (var package in symbolPackages)
    {
        var settings = new NuGetPushSettings
        {
            Source = nugetSource,
            ApiKey = nugetApiKey,
            SkipDuplicate = true
        };
        NuGetPush(package.FullPath, settings);
    }
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
