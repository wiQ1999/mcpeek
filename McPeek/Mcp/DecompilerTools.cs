using McPeek.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace McPeek.Mcp;

/// <summary>
/// MCP Tools for DLL decompilation operations
/// </summary>
[McpServerToolType]
public class DecompilerTools
{
    /// <summary>
    /// Decompiles all .NET DLLs in the specified folder
    /// </summary>
    [McpServerTool]
    [Description("Decompiles all .NET DLL files in the specified folder and caches the results")]
    public static async Task<DecompileResult> DecompileFolder(
        DecompilationService decompilationService,
        [Description("The absolute path to the folder containing DLL files")] string folderPath)
    {
        try
        {
            var assemblies = await decompilationService.DecompileFolderAsync(folderPath);
            
            return new DecompileResult
            {
                Success = true,
                Message = $"Successfully decompiled {assemblies.Count} assemblies",
                AssemblyNames = assemblies.Select(a => a.AssemblyName).ToList(),
                TotalFiles = assemblies.Sum(a => a.Files.Count)
            };
        }
        catch (Exception ex)
        {
            return new DecompileResult
            {
                Success = false,
                Message = $"Failed to decompile folder: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Decompiles a specific DLL file
    /// </summary>
    [McpServerTool]
    [Description("Decompiles a specific .NET DLL file")]
    public static async Task<DecompileResult> DecompileDll(
        DecompilationService decompilationService,
        [Description("The absolute path to the DLL file")] string dllPath)
    {
        try
        {
            var assembly = await decompilationService.DecompileAssemblyAsync(dllPath);
            
            return new DecompileResult
            {
                Success = true,
                Message = $"Successfully decompiled {assembly.AssemblyName}",
                AssemblyNames = new List<string> { assembly.AssemblyName },
                TotalFiles = assembly.Files.Count
            };
        }
        catch (Exception ex)
        {
            return new DecompileResult
            {
                Success = false,
                Message = $"Failed to decompile DLL: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Searches for text across all decompiled assemblies
    /// </summary>
    [McpServerTool]
    [Description("Searches for a text pattern across all decompiled C# code")]
    public static SearchResponse SearchCode(
        DecompilationService decompilationService,
        [Description("The text to search for")] string query,
        [Description("Whether the search should be case-sensitive (default: false)")] bool caseSensitive = false,
        [Description("Maximum number of results to return (default: 50)")] int maxResults = 50)
    {
        try
        {
            var results = decompilationService.Search(query, caseSensitive);
            var limitedResults = results.Take(maxResults).ToList();

            return new SearchResponse
            {
                Success = true,
                Query = query,
                TotalMatches = results.Count,
                Results = limitedResults.Select(r => new SearchResultDto
                {
                    AssemblyName = r.AssemblyName,
                    FilePath = r.FilePath,
                    LineNumber = r.LineNumber,
                    LineContent = r.LineContent,
                    Context = r.MatchContext
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            return new SearchResponse
            {
                Success = false,
                Message = $"Search failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Lists all currently loaded assemblies
    /// </summary>
    [McpServerTool]
    [Description("Lists all assemblies that have been decompiled and are currently loaded")]
    public static LoadedAssembliesResponse ListLoadedAssemblies(DecompilationService decompilationService)
    {
        // Use fast metadata method instead of loading full content
        var metadata = decompilationService.GetLoadedAssemblyMetadata();
        
        return new LoadedAssembliesResponse
        {
            Success = true,
            TotalAssemblies = metadata.Count,
            Assemblies = metadata.Select(m => new AssemblyInfo
            {
                Name = m.AssemblyName,
                Path = m.AssemblyPath,
                FileCount = m.FileCount,
                DecompiledAt = m.DecompiledAt,
                TypeCount = m.TypeCount,
                Namespaces = m.Namespaces
            }).ToList()
        };
    }

    /// <summary>
    /// Gets cache statistics
    /// </summary>
    [McpServerTool]
    [Description("Gets statistics about the decompilation cache")]
    public static CacheStatsResponse GetCacheStatistics(DecompilationCacheManager cacheManager)
    {
        var stats = cacheManager.GetStatistics();
        
        return new CacheStatsResponse
        {
            Success = true,
            TotalCachedAssemblies = stats.TotalCachedAssemblies,
            CacheDirectory = stats.CacheDirectory,
            CacheSizeMB = stats.TotalSizeBytes / (1024.0 * 1024.0)
        };
    }

    /// <summary>
    /// Clears the decompilation cache
    /// </summary>
    [McpServerTool]
    [Description("Clears all cached decompilation results")]
    public static BaseResponse ClearCache(DecompilationCacheManager cacheManager)
    {
        try
        {
            cacheManager.ClearCache();
            return new BaseResponse
            {
                Success = true,
                Message = "Cache cleared successfully"
            };
        }
        catch (Exception ex)
        {
            return new BaseResponse
            {
                Success = false,
                Message = $"Failed to clear cache: {ex.Message}"
            };
        }
    }
}

// Response DTOs
public class BaseResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class DecompileResult : BaseResponse
{
    public List<string> AssemblyNames { get; set; } = new();
    public int TotalFiles { get; set; }
}

public class SearchResponse : BaseResponse
{
    public string Query { get; set; } = string.Empty;
    public int TotalMatches { get; set; }
    public List<SearchResultDto> Results { get; set; } = new();
}

public class SearchResultDto
{
    public string AssemblyName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string LineContent { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
}

public class LoadedAssembliesResponse : BaseResponse
{
    public int TotalAssemblies { get; set; }
    public List<AssemblyInfo> Assemblies { get; set; } = new();
}

public class AssemblyInfo
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int FileCount { get; set; }
    public DateTime DecompiledAt { get; set; }
    public int TypeCount { get; set; }
    public List<string> Namespaces { get; set; } = new();
}

public class CacheStatsResponse : BaseResponse
{
    public int TotalCachedAssemblies { get; set; }
    public string CacheDirectory { get; set; } = string.Empty;
    public double CacheSizeMB { get; set; }
}
