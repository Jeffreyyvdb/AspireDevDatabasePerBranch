using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AspireDevDatabasePerBranch;

public static partial class PostgresServerResourceExtension
{
    private const int MaxDatabaseNameLength = 63;

    /// <summary>
    /// Adds a PostgreSQL database to the application model with the database name modified to include the current git branch name.
    /// </summary>
    /// <param name="builder">The PostgreSQL server resource builder.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="databaseName">The name of the database. If not provided, this defaults to the same value as <paramref name="name"/>.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    /// <remarks>
    /// This extension method modifies the database name to include the current git branch name in the format: {databaseName}-{branchName}.
    /// This helps avoid database name conflicts when working on different branches.
    /// </remarks>
    public static IResourceBuilder<PostgresDatabaseResource> AddDatabaseWithBranchNameSuffix(this IResourceBuilder<PostgresServerResource> builder, [ResourceName] string name, string? databaseName = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(name);

        // Use the resource name as the database name if it's not provided
        databaseName ??= name;

        // Get the current git branch name
        var branchName = GetCurrentGitBranch();
        if (string.IsNullOrEmpty(branchName))
        {
            return builder.AddDatabase(name, databaseName);
        }

        branchName = SanitizeBranchName(branchName);
        var remainingLength = MaxDatabaseNameLength - databaseName.Length - 1; // -1 for the hyphen
        if (remainingLength > 0)
        {
            var truncatedBranchName = branchName.Length > remainingLength
                ? branchName[^remainingLength..] // take last `remainingLength` characters
                : branchName;

            databaseName = $"{databaseName}-{truncatedBranchName}";
        }
        else
        {
            // Handle edge case: not enough room for even a hyphen and part of the branch name
            databaseName = databaseName[..MaxDatabaseNameLength];
        }

        return builder.AddDatabase(name, databaseName);
    }

    private static string GetCurrentGitBranch()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "rev-parse --abbrev-ref HEAD",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output.Trim();
            }
        }
        catch
        {
            // If git command fails, return empty string
        }

        return string.Empty;
    }

    private static string SanitizeBranchName(string branchName)
    {
        // Replace invalid characters with hyphens
        var sanitized = DatabaseNameSanitizerRegex().Replace(branchName, "-");
        // Remove consecutive hyphens
        sanitized = ConsecutiveHyphensRegex().Replace(sanitized, "-");
        return sanitized.ToLowerInvariant();
    }

    [GeneratedRegex("[^a-zA-Z0-9]")]
    private static partial Regex DatabaseNameSanitizerRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex ConsecutiveHyphensRegex();
}