//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "FixIndex");

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

Task("FixIndex")
    .IsDependentOn("CopyLogo")
    .Does(() =>
{
    var indexPath = System.IO.Path.Combine(siteDir, "index.html");

    var i = 0;
    var lines = System.IO.File.ReadAllLines(indexPath);

    for (; i < lines.Length; ++i)
    {
        var line = lines[i];
        if (!line.Contains("<link rel=\"icon\" href=\"favicon.ico\">"))
            continue;

        lines[i] = line.Replace("<link rel=\"icon\" href=\"favicon.ico\">", "<link rel=\"icon\" href=\"logo.png\">");
        ++i;
        break;
    }

    for (; i < lines.Length; ++i)
    {
        var line = lines[i];
        if (!line.Contains("<img id=\"logo\" class=\"svg\" src=\"logo.svg\" alt=\"LfrlAnvil\">"))
            continue;

        lines[i] = line.Replace(
            "<img id=\"logo\" class=\"svg\" src=\"logo.svg\" alt=\"LfrlAnvil\">",
            "<img id=\"logo\" src=\"logo.png\" alt=\"LfrlAnvil\" style=\"height:51px;padding-right:1rem\">");

        break;
    }

    System.IO.File.WriteAllLines(indexPath, lines);
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
