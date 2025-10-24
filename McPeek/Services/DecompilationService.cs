using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using System.Reflection.Metadata;
using System.Text;

namespace McPeek.Services;

/// <summary>
/// Service for decompiling .NET assemblies using ICSharpCode.Decompiler
/// </summary>
public class DecompilationService
{
    private readonly DecompilationCacheManager _cacheManager;
    private readonly Dictionary<string, DecompiledAssembly> _loadedAssemblies;

    public DecompilationService(DecompilationCacheManager cacheManager)
    {
        _cacheManager = cacheManager;
        _loadedAssemblies = new Dictionary<string, DecompiledAssembly>();
    }

    /// <summary>
    /// Decompiles all DLLs in the specified folder
    /// </summary>
    public async Task<List<DecompiledAssembly>> DecompileFolderAsync(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException($"Folder not found: {folderPath}");

        var dllFiles = Directory.GetFiles(folderPath, "*.dll", SearchOption.AllDirectories);
        var results = new List<DecompiledAssembly>();

        foreach (var dllPath in dllFiles)
        {
            try
            {
                var result = await DecompileAssemblyAsync(dllPath);
                results.Add(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to decompile {dllPath}: {ex.Message}");
            }
        }

        return results;
    }

    /// <summary>
    /// Decompiles a single assembly
    /// </summary>
    public async Task<DecompiledAssembly> DecompileAssemblyAsync(string dllPath)
    {
        if (!File.Exists(dllPath))
            throw new FileNotFoundException($"DLL not found: {dllPath}");

        // Check cache first
        if (_cacheManager.IsCached(dllPath, out var hash))
        {
            var cached = _cacheManager.GetCached(hash);
            if (cached != null)
            {
                _loadedAssemblies[dllPath] = cached;
                return cached;
            }
        }

        // Decompile the assembly
        var decompiledAssembly = await Task.Run(() => DecompileAssemblyInternal(dllPath, hash));
        
        // Cache the result
        _cacheManager.SaveToCache(hash, dllPath, decompiledAssembly);
        _loadedAssemblies[dllPath] = decompiledAssembly;

        return decompiledAssembly;
    }

    /// <summary>
    /// Gets all currently loaded assemblies
    /// </summary>
    public List<DecompiledAssembly> GetLoadedAssemblies()
    {
        return _loadedAssemblies.Values.ToList();
    }

    /// <summary>
    /// Searches for text across all decompiled files
    /// </summary>
    public List<SearchResult> Search(string query, bool caseSensitive = false)
    {
        var results = new List<SearchResult>();
        var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        foreach (var assembly in _loadedAssemblies.Values)
        {
            foreach (var file in assembly.Files)
            {
                var lines = file.Content.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(query, comparison))
                    {
                        results.Add(new SearchResult
                        {
                            AssemblyName = assembly.AssemblyName,
                            FilePath = file.Path,
                            LineNumber = i + 1,
                            LineContent = lines[i].Trim(),
                            MatchContext = GetContext(lines, i, 2)
                        });
                    }
                }
            }
        }

        return results;
    }

    private DecompiledAssembly DecompileAssemblyInternal(string dllPath, string hash)
    {
        var decompiler = new CSharpDecompiler(dllPath, new DecompilerSettings
        {
            ThrowOnAssemblyResolveErrors = false,
            ShowXmlDocumentation = true,
            UseDebugSymbols = true
        });

        var module = decompiler.TypeSystem.MainModule;
        var decompiledFiles = new List<DecompiledFile>();

        // Decompile each type
        foreach (var type in module.TopLevelTypeDefinitions)
        {
            try
            {
                // Skip compiler-generated types
                if (type.Name.StartsWith("<"))
                    continue;

                var code = decompiler.DecompileTypeAsString(type.FullTypeName);
                var relativePath = GetRelativePathForType(type);

                decompiledFiles.Add(new DecompiledFile
                {
                    Path = relativePath,
                    Content = code,
                    TypeName = type.FullName,
                    Namespace = type.Namespace,
                    IsPublic = type.Accessibility == Accessibility.Public
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to decompile type {type.FullName}: {ex.Message}");
            }
        }

        return new DecompiledAssembly
        {
            AssemblyName = module.AssemblyName,
            AssemblyPath = dllPath,
            Hash = hash,
            Files = decompiledFiles,
            DecompiledAt = DateTime.UtcNow
        };
    }

    private string GetRelativePathForType(ITypeDefinition type)
    {
        var namespacePath = type.Namespace.Replace('.', Path.DirectorySeparatorChar);
        var fileName = $"{type.Name}.cs";
        
        if (string.IsNullOrEmpty(namespacePath))
            return fileName;

        return Path.Combine(namespacePath, fileName);
    }

    private string GetContext(string[] lines, int index, int contextLines)
    {
        var start = Math.Max(0, index - contextLines);
        var end = Math.Min(lines.Length - 1, index + contextLines);
        
        var context = new StringBuilder();
        for (int i = start; i <= end; i++)
        {
            if (i == index)
                context.AppendLine($">>> {lines[i].Trim()}");
            else
                context.AppendLine($"    {lines[i].Trim()}");
        }

        return context.ToString();
    }
}

public class DecompiledAssembly
{
    public string AssemblyName { get; set; } = string.Empty;
    public string AssemblyPath { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public List<DecompiledFile> Files { get; set; } = new();
    public DateTime DecompiledAt { get; set; }
}

public class DecompiledFile
{
    public string Path { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
}

public class SearchResult
{
    public string AssemblyName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string LineContent { get; set; } = string.Empty;
    public string MatchContext { get; set; } = string.Empty;
}
