var rootDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, ".."));
var solution = System.IO.Path.Combine(rootDir, "LfrlAnvil.sln");

var projects = GetFiles(System.IO.Path.Combine(rootDir, "src", "**", "*.csproj"))
    .OrderBy(static p => p.FullPath)
    .ToArray();
