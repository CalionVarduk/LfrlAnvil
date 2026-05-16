#tool "dotnet:?package=GitVersion.Tool&version=5.12.0"

#load "projects.cake"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Pack");
var configuration = Argument("debug-mode", false) ? "Debug" : "Release";
var runTests = Argument("run-tests", true);
var coverTests = runTests && Argument("cover-tests", false);
var publishOutput = Argument("publish-output", "");
var nugetOutput = Argument("nuget-output", "nuget");
var version = GitVersion(new GitVersionSettings()).NuGetVersion;

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var binDirs = System.IO.Path.Combine(rootDir, "**", "bin", configuration);
var objDirs = System.IO.Path.Combine(rootDir, "**", "obj", configuration);
var publishDir = publishOutput.Length > 0 ? System.IO.Path.Combine(rootDir, publishOutput) : "";
var nugetDir = System.IO.Path.Combine(rootDir, nugetOutput);

var testProjects = GetFiles(System.IO.Path.Combine(rootDir, "tests", "**", "*.Tests.csproj"))
    .OrderBy(static p => p.FullPath)
    .ToArray();

var coverletOutput = System.IO.Path.Combine(rootDir, ".coverlet", "coverage.json");
var coverletDir = coverletOutput.Substring(0, coverletOutput.Length - "coverage.json".Length);
if (coverletDir.EndsWith("\\") && !coverletDir.EndsWith("\\\\"))
    coverletDir = coverletDir + "\\";

Information("Configuration: {0}", configuration);
Information("Run tests: {0}", runTests);
Information("Cover tests: {0}", coverTests);
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

    if (coverTests)
    {
        var dir = System.IO.Path.GetDirectoryName(coverletOutput);
        Information("Cleaning '{0}' directory...", dir);
        CleanDirectory(dir);
        Information("'{0}' directory cleaned.", dir);
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
    for (var i = 0; i < testProjects.Length; ++i)
    {
        var project = testProjects[i];
        var isFirst = i == 0;
        var isLast = i == testProjects.Length - 1;

        var settings = new DotNetTestSettings
        {
            NoBuild = true,
            Configuration = configuration,
            ArgumentCustomization = args =>
            {
                if (!coverTests)
                    return args;

                args
                    .Append("/p:CollectCoverage=true")
                    .Append($"/p:CoverletOutput=\"{coverletDir}\"")
                    .Append("/p:Exclude=\"[*.TestExtensions*]*\"");

                if (!isFirst)
                    args.Append($"/p:MergeWith=\"{coverletOutput}\"");
                if (isLast)
                    args.Append("/p:CoverletOutputFormat=\"lcov\"");

                return args;
            }
        };
        DotNetTest(project.FullPath, settings);
    }
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
        DotNetPackWithReadmeLogoFix(project.FullPath, settings);
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
        var settings = new DotNetNuGetPushSettings
        {
            Source = nugetSource,
            ApiKey = nugetApiKey,
            SkipDuplicate = true
        };
        DotNetNuGetPush(package.FullPath, settings);
    }
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

void DotNetPackWithReadmeLogoFix(string projectPath, DotNetPackSettings settings)
{
    Information(projectPath);
    var readmePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(projectPath), ".docs", "readme.md");
    if (!System.IO.File.Exists(readmePath))
        throw new Exception($"Readme {readmePath} does not exist!");

    var projectName = System.IO.Path.GetFileNameWithoutExtension(projectPath);
    var readmeEncoding = GetFileEncoding(readmePath);
    var originalReadmeContents = System.IO.File.ReadAllText(readmePath);
    var nugetLink = @"https://www.nuget.org/packages/" + projectName + "/";

    var logoRegex = new System.Text.RegularExpressions.Regex(
        @"(?:\.\.\/)*assets\/logo\.png",
        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    var logoMatches = logoRegex.Matches(originalReadmeContents);
    if (logoMatches.Count < 2)
        throw new Exception($"Readme {readmePath} has invalid logo setup!");

    var readmeContents = new StringBuilder(originalReadmeContents.Length);
    var lastIndex = 0;
    for (var i = 0; i < logoMatches.Count; ++i)
    {
        var match = logoMatches[i];
        readmeContents.Append(originalReadmeContents.Substring(lastIndex, match.Index - lastIndex));
        if (i == 0)
            readmeContents.Append("https://raw.githubusercontent.com/CalionVarduk/LfrlAnvil/main/assets/logo-small.png");
        else if (i == 1)
            readmeContents.Append(nugetLink);
        else
            readmeContents.Append(match.Value);

        lastIndex = match.Index + match.Length;
    }

    readmeContents.Append(originalReadmeContents.Substring(lastIndex));
    System.IO.File.WriteAllText(readmePath, readmeContents.ToString(), readmeEncoding);

    DotNetPack(projectPath, settings);

    System.IO.File.WriteAllText(readmePath, originalReadmeContents, readmeEncoding);
}

Encoding GetFileEncoding(string file)
{
    using var reader = new System.IO.StreamReader(file, detectEncodingFromByteOrderMarks: true);
    reader.Peek();
    return reader.CurrentEncoding ?? System.Text.Encoding.UTF8;
}
