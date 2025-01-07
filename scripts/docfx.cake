#load "projects.cake"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "PrepareIndex");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var docfxDir = System.IO.Path.Combine(rootDir, ".docfx");
var docfxApiDir = System.IO.Path.Combine(docfxDir, "api");
var docfxJsonFile = System.IO.Path.Combine(docfxDir, "docfx.json");

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
    Information("Cleaning '{0}' directory...", docfxDir);
    CleanDirectory(docfxDir);
    Information("'{0}' directory cleaned.", docfxDir);
});

Task("PrepareJson")
    .IsDependentOn("Clean")
    .Does(() =>
{
    var docfxConfig = new
    {
        metadata = projects
            .Select(static p => new
            {
                src = new[]
                {
                    new
                    {
                        src = "../src",
                        files = new[] { $"**/{p.GetFilename()}" }
                    }
                },
                dest = $"api/{p.GetFilenameWithoutExtension()}"
            })
            .ToArray(),
        build = new
        {
            content = new[]
            {
                new
                {
                    files = new[] { "**/*.{md,yml}" },
                    exclude = new[] { "_site/**" }
                }
            },
            resource = new[]
            {
                new
                {
                    files = new[] { "images/**" }
                }
            },
            output = "_site",
            template = new[] { "default", "modern" },
            globalMetadata = new
            {
                _appName = "LfrlAnvil",
                _appTitle = "LfrlAnvil",
                _enableSearch = true,
                _disableContribution = true,
                pdf = false
            }
        }
    };

    Information("Creating '{0}' file...", docfxJsonFile);
    var docfxJson = System.Text.Json.JsonSerializer.Serialize(
        docfxConfig,
        new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

    System.IO.File.AppendAllText(docfxJsonFile, docfxJson);
    Information("'{0}' file created.", docfxJsonFile);
});

Task("PrepareToc")
    .IsDependentOn("PrepareJson")
    .Does(() =>
{
    Information("Creating '{0}' directory...", docfxApiDir);
    System.IO.Directory.CreateDirectory(docfxApiDir);
    Information("'{0}' directory created.", docfxApiDir);

    var tocYmlFile = System.IO.Path.Combine(docfxDir, "toc.yml");
    var apiTocYmlFile = System.IO.Path.Combine(docfxApiDir, "toc.yml");

    Information("Creating '{0}' file...", tocYmlFile);
    var tocYmlLines = new[]
    {
        "- name: API",
        "  href: api/"
    };

    System.IO.File.AppendAllLines(tocYmlFile, tocYmlLines);
    Information("'{0}' file created.", tocYmlFile);

    Information("Creating '{0}' file...", apiTocYmlFile);
    var apiTocYmlLines = projects
        .SelectMany(static p => new[]
        {
            $"- name: {p.GetFilenameWithoutExtension()}",
            $"  href: ./{p.GetFilenameWithoutExtension()}/toc.yml"
        })
        .ToArray();

    System.IO.File.AppendAllLines(apiTocYmlFile, apiTocYmlLines);
    Information("'{0}' file created.", apiTocYmlFile);
});

Task("PrepareIndex")
    .IsDependentOn("PrepareToc")
    .Does(() =>
{
    var sourceReadmeFile = System.IO.Path.Combine(rootDir, "readme.md");
    var destIndexFile = System.IO.Path.Combine(docfxDir, "index.md");

    Information("Reading '{0}' file...", sourceReadmeFile);
    var sourceReadme = System.IO.File.ReadAllText(sourceReadmeFile);
    Information("'{0}' file read.", sourceReadmeFile);

    Information("Creating '{0}' file...", destIndexFile);
    var indexRegex = new System.Text.RegularExpressions.Regex("\\(\\./.+\\)");
    var destIndex = indexRegex.Replace(
        sourceReadme,
        static match => match.Value.Replace("./", "https://github.com/CalionVarduk/LfrlAnvil/blob/main/"));

    System.IO.File.AppendAllText(destIndexFile, destIndex);
    Information("'{0}' file created.", destIndexFile);
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
