var target = Argument("target", "UpdateLicenseHeaders");
var commitHash = Argument("commit", string.Empty);

var rootDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, ".."));
var currentYear = DateTime.UtcNow.Year.ToString(System.Globalization.CultureInfo.InvariantCulture);
var copyrightLines = new[]
{
    "// ",
    "// Licensed under the Apache License, Version 2.0 (the \"License\");",
    "// you may not use this file except in compliance with the License.",
    "// You may obtain a copy of the License at",
    "// ",
    "//     http://www.apache.org/licenses/LICENSE-2.0",
    "// ",
    "// Unless required by applicable law or agreed to in writing, software",
    "// distributed under the License is distributed on an \"AS IS\" BASIS,",
    "// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.",
    "// See the License for the specific language governing permissions and",
    "// limitations under the License.",
    ""
};

var foundFiles = new List<string>();

Task("FindEligibleFiles")
    .Does(() =>
{
    if (commitHash.Length == 0)
    {
        foundFiles.AddRange(FindGitFiles("diff --name-only --cached"));
        foundFiles.AddRange(FindGitFiles("diff --name-only"));
    }
    else
    {
        foundFiles.AddRange(FindGitFiles($"diff --name-only --diff-filter=d {commitHash}^ {commitHash}"));
    }

    Information("Found {0} file(s):", foundFiles.Count);
    foreach (var (file, pos) in foundFiles.Select(static (f, i) => (File: f, Position: i + 1)))
        Information("{0}. '{1}'", pos, file);
});

Task("UpdateLicenseHeaders")
    .IsDependentOn("FindEligibleFiles")
    .Does(() =>
{
    foreach (var file in foundFiles)
    {
        if (!System.IO.File.Exists(file))
            continue;

        var encoding = GetFileEncoding(file);
        var lines = System.IO.File.ReadAllLines(file);
        if (lines.Length == 0)
        {
            Information("'{0}': Skipping.", file);
            continue;
        }

        var containsLicenseHeader = lines[0].StartsWith("// Copyright ");
        if (containsLicenseHeader)
        {
            var containsYearRange = lines[0].Contains('-');
            if (containsYearRange)
            {
                var lastYear = lines[0].Substring("// Copyright ".Length + 5, 4);
                if (lastYear == currentYear)
                {
                    Information("'{0}': No changes.", file);
                    continue;
                }

                Information("'{0}': Updating last year {1}.", file, lastYear);
                lines[0] = lines[0].Substring(0, "// Copyright ".Length + 5) + currentYear + " Łukasz Furlepa";
            }
            else
            {
                var lastYear = lines[0].Substring("// Copyright ".Length, 4);
                if (lastYear == currentYear)
                {
                    Information("'{0}': No changes.", file);
                    continue;
                }

                Information("'{0}': Updating last year {1} to range.", file, lastYear);
                lines[0] = lines[0].Substring(0, "// Copyright ".Length) + lastYear + "-" + currentYear + " Łukasz Furlepa";
            }
        }
        else
        {
            Information("'{0}': Adding missing header.", file);
            var newLines = new string[lines.Length + copyrightLines.Length + 1];
            newLines[0] = $"// Copyright {currentYear} Łukasz Furlepa";
            copyrightLines.AsSpan().CopyTo(newLines.AsSpan(1));
            lines.AsSpan().CopyTo(newLines.AsSpan(copyrightLines.Length + 1));
            lines = newLines;
        }

        System.IO.File.WriteAllLines(file, lines, encoding);
    }
});

RunTarget(target);

IEnumerable<string> FindGitFiles(string args)
{
    var exitCode = StartProcess(
        "git",
        new ProcessSettings
        {
            Arguments = args,
            RedirectStandardOutput = true
        },
        out var output);

    if (exitCode != 0)
        throw new InvalidOperationException($"Git process with args '{args}' exited with code {exitCode}.");

    return output
        .Where(static f =>
            (f.StartsWith("./src/") || f.StartsWith("src/") || f.StartsWith(".\\src\\") || f.StartsWith("src\\"))
            && System.IO.Path.GetExtension(f) == ".cs")
        .Select(f => System.IO.Path.Combine(rootDir, f));
}

Encoding GetFileEncoding(string file)
{
    using var reader = new System.IO.StreamReader(file, detectEncodingFromByteOrderMarks: true);
    reader.Peek();
    return reader.CurrentEncoding ?? System.Text.Encoding.UTF8;
}
