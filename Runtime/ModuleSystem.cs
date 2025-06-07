namespace Calculator.Runtime;

/// <summary>
/// Represents a module in the scripting language
/// </summary>
public class Module
{
    public string FilePath { get; }
    public Dictionary<string, object?> Exports { get; }
    public bool IsLoaded { get; set; }

    public Module(string filePath)
    {
        FilePath = filePath;
        Exports = new Dictionary<string, object?>();
        IsLoaded = false;
    }
}

/// <summary>
/// Manages module loading and importing
/// </summary>
public class ModuleManager
{
    private readonly Dictionary<string, Module> modules = new();
    private string currentDirectory = "";

    public void SetCurrentDirectory(string directory)
    {
        currentDirectory = directory;
    }

    public string GetCurrentDirectory() => currentDirectory;

    public void RegisterModule(string path, Module module)
    {
        modules[path] = module;
    }

    public Module? GetModule(string path)
    {
        return modules.GetValueOrDefault(path);
    }

    public bool HasModule(string path)
    {
        return modules.ContainsKey(path);
    }

    /// <summary>
    /// Resolves a relative or absolute path to a full path
    /// </summary>
    public string ResolvePath(string relativePath)
    {
        // Convert both UNIX and Windows style paths to the current OS format
        string normalizedPath = relativePath.Replace('/', Path.DirectorySeparatorChar)
                                           .Replace('\\', Path.DirectorySeparatorChar);
        
        // Resolve relative to current directory
        if (Path.IsPathRooted(normalizedPath))
        {
            return Path.GetFullPath(normalizedPath);
        }
        else
        {
            string basePath = string.IsNullOrEmpty(currentDirectory) ? Directory.GetCurrentDirectory() : currentDirectory;
            return Path.GetFullPath(Path.Combine(basePath, normalizedPath));
        }
    }
}