#load "projects.cake"

var target = Argument("target", "UpdateThirdPartyLicenses");

Information("Project count: {0}", projects.Length);
for (var i = 0; i < projects.Length; ++i)
    Information("{0}. '{1}'", i + 1, projects[i].FullPath);

Task("UpdateThirdPartyLicenses")
    .Does(() =>
{
    var thirdPartyLicensesSubpath = System.IO.Path.Combine(".docs", "THIRD-PARTY-LICENSES.txt");
    foreach (var p in projects)
    {
        var dir = System.IO.Path.GetDirectoryName(p.FullPath);
        StartProcess(
            "thirdlicense",
            new ProcessSettings
            {
                WorkingDirectory = dir,
                Arguments = $"--project \"{System.IO.Path.GetFileName(p.FullPath)}\" --output \"{thirdPartyLicensesSubpath}\""
            });

        var thirdPartyLicensesPath = System.IO.Path.Combine(dir, thirdPartyLicensesSubpath);
        if (string.IsNullOrWhiteSpace(System.IO.File.ReadAllText(thirdPartyLicensesPath)))
            System.IO.File.Delete(thirdPartyLicensesPath);
    }
});

RunTarget(target);
