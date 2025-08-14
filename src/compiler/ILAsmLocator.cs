using System.Runtime.InteropServices;

namespace compiler;

/// <summary>
/// Utility for locating the IL assembler (ilasm) executable
/// </summary>
public static class ILAsmLocator
{
    /// <summary>
    /// Find the IL assembler executable path
    /// </summary>
    /// <returns>The path to ilasm, or null if not found</returns>
    public static string? FindILAsm()
    {
        // 1. Check ILASM_PATH environment variable
        var ilasmPath = Environment.GetEnvironmentVariable("ILASM_PATH");
        if (!string.IsNullOrWhiteSpace(ilasmPath) && File.Exists(ilasmPath))
        {
            return ilasmPath;
        }

        // 2. Look in the same directory as the .NET runtime
        var runtimeIlasm = FindILAsmInRuntimeDirectory();
        if (!string.IsNullOrWhiteSpace(runtimeIlasm))
        {
            return runtimeIlasm;
        }

        // 3. Look in DOTNET_ROOT or use 'dotnet' command location
        var dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
        if (string.IsNullOrWhiteSpace(dotnetRoot))
        {
            // Try to find dotnet location
            dotnetRoot = FindDotNetRoot();
        }

        if (!string.IsNullOrWhiteSpace(dotnetRoot) && Directory.Exists(dotnetRoot))
        {
            var ilasm = FindILAsmInDotNetRoot(dotnetRoot);
            if (!string.IsNullOrWhiteSpace(ilasm))
            {
                return ilasm;
            }
        }

        // 4. Try to find ilasm in PATH
        var pathIlasm = FindILAsmInPath();
        if (!string.IsNullOrWhiteSpace(pathIlasm))
        {
            return pathIlasm;
        }

        return null;
    }

    private static string? FindILAsmInRuntimeDirectory()
    {
        try
        {
            // Get the directory where the .NET runtime is located
            var runtimeLocation = Path.GetDirectoryName(typeof(object).Assembly.Location);
            if (string.IsNullOrWhiteSpace(runtimeLocation))
            {
                return null;
            }

            // Check for ilasm in the runtime directory
            var ilasmExecutable = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ilasm.exe" : "ilasm";
            var ilasmPath = Path.Combine(runtimeLocation, ilasmExecutable);
            
            if (File.Exists(ilasmPath))
            {
                return ilasmPath;
            }

            // On Linux/Unix, also check parent directories and common locations
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Check parent directory (sometimes ilasm is one level up)
                var parentDir = Path.GetDirectoryName(runtimeLocation);
                if (!string.IsNullOrWhiteSpace(parentDir))
                {
                    var parentIlasmPath = Path.Combine(parentDir, ilasmExecutable);
                    if (File.Exists(parentIlasmPath))
                    {
                        return parentIlasmPath;
                    }
                }

                // Check if we can find it through the package manager paths
                var commonPaths = new[]
                {
                    "/usr/bin/ilasm",
                    "/usr/local/bin/ilasm",
                    "/opt/dotnet/ilasm"
                };

                foreach (var path in commonPaths)
                {
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }
            }
        }
        catch
        {
            // Ignore errors in finding ilasm in runtime directory
        }

        return null;
    }

    private static string? FindDotNetRoot()
    {
        try
        {
            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "where" : "which",
                Arguments = "dotnet",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(processStartInfo);
            if (process == null) return null;

            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
            {
                var dotnetPath = output.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(dotnetPath))
                {
                    // dotnet is typically in <root>/dotnet or <root>/bin/dotnet
                    var directory = Path.GetDirectoryName(dotnetPath);
                    if (directory != null)
                    {
                        // Go up to find the root if we're in a bin directory
                        if (Path.GetFileName(directory)?.ToLowerInvariant() == "bin")
                        {
                            directory = Path.GetDirectoryName(directory);
                        }
                        return directory;
                    }
                }
            }
        }
        catch
        {
            // Ignore errors in finding dotnet
        }

        return null;
    }

    private static string? FindILAsmInDotNetRoot(string dotnetRoot)
    {
        try
        {
            var sdkPath = Path.Combine(dotnetRoot, "sdk");
            if (!Directory.Exists(sdkPath))
            {
                return null;
            }

            // Find the highest version SDK that contains ilasm
            var sdkVersions = Directory.GetDirectories(sdkPath)
                .Select(d => new { Path = d, Version = Path.GetFileName(d) })
                .Where(v => Version.TryParse(v.Version, out _))
                .OrderByDescending(v => Version.Parse(v.Version))
                .ToList();

            foreach (var sdk in sdkVersions)
            {
                var ilasmExecutable = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ilasm.exe" : "ilasm";
                var ilasmPath = Path.Combine(sdk.Path, ilasmExecutable);
                
                if (File.Exists(ilasmPath))
                {
                    return ilasmPath;
                }
            }
        }
        catch
        {
            // Ignore errors in finding ilasm
        }

        return null;
    }

    private static string? FindILAsmInPath()
    {
        try
        {
            var ilasmExecutable = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ilasm.exe" : "ilasm";
            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "where" : "which",
                Arguments = ilasmExecutable,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(processStartInfo);
            if (process == null) return null;

            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
            {
                var firstPath = output.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(firstPath) && File.Exists(firstPath))
                {
                    return firstPath;
                }
            }
        }
        catch
        {
            // Ignore errors in finding ilasm in PATH
        }

        return null;
    }

    /// <summary>
    /// Get a user-friendly error message when ilasm is not found
    /// </summary>
    public static string GetILAsmNotFoundMessage()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "IL Assembler (ilasm.exe) not found. Please ensure one of the following:\n" +
                   "1. Set ILASM_PATH environment variable to the ilasm.exe path\n" +
                   "2. Set DOTNET_ROOT environment variable to your .NET installation\n" +
                   "3. Ensure 'dotnet' is in your PATH and SDK includes ilasm.exe\n" +
                   "4. Install .NET SDK which includes IL assembler tools";
        }
        else
        {
            return "IL Assembler (ilasm) not found. Please ensure one of the following:\n" +
                   "1. Set ILASM_PATH environment variable to the ilasm path\n" +
                   "2. Install mono-devel package: sudo apt install mono-devel (Ubuntu/Debian)\n" +
                   "3. Install .NET SDK with IL tools\n" +
                   "4. Set DOTNET_ROOT environment variable to your .NET installation\n" +
                   "Note: On Linux, IL assembly might require mono-devel or equivalent packages";
        }
    }
}