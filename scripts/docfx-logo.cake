//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "FixHtml");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var rootDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, ".."));
var siteDir = System.IO.Path.Combine(rootDir, ".docfx", "_site");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("DeletePlaceholders")
    .Does(() =>
{
    var phFaviconPath = System.IO.Path.Combine(siteDir, "favicon.ico");
    var phLogoPath = System.IO.Path.Combine(siteDir, "logo.svg");
    DeleteFile(phFaviconPath);
    DeleteFile(phLogoPath);
});

Task("CopyLogo")
    .IsDependentOn("DeletePlaceholders")
    .Does(() =>
{
    var sourceLogoPath = System.IO.Path.Combine(rootDir, "assets", "logo-small.png");
    var logoPath = System.IO.Path.Combine(siteDir, "logo.png");
    CopyFile(sourceLogoPath, logoPath);
});

Task("FixHtml")
    .IsDependentOn("CopyLogo")
    .Does(() =>
{
    var htmlFiles = GetFiles(System.IO.Path.Combine(siteDir, "**", "*.html"));
    foreach (var file in htmlFiles)
    {
        var lines = System.IO.File.ReadAllLines(file.FullPath);

        var i = 0;
        for (; i < lines.Length; ++i)
        {
            var line = lines[i];
            if (!line.Contains("<link rel=\"icon\""))
                continue;

            lines[i] = line.Replace("favicon.ico", "logo.png");
            ++i;
            break;
        }

        for (; i < lines.Length; ++i)
        {
            var line = lines[i];
            if (!line.Contains("<img id=\"logo\""))
                continue;

            lines[i] = line
                .Replace(" class=\"svg\"", string.Empty)
                .Replace("logo.svg", "logo.png")
                .Replace(">", " style=\"height:51px;padding-right:1rem\">");

            break;
        }

        System.IO.File.WriteAllLines(file.FullPath, lines);
        Information("File '{0}' fixed.", file.FullPath);
    }
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
