using System.Security.Cryptography;
using System.Text.Json;

namespace McPeek.Services;

/// <summary>
/// Manages persistent file-based caching of decompiled DLL content using SHA256 hashes
/// </summary>
public class DecompilationCacheManager
{
    private readonly string _cacheDirectory;
    private readonly string _indexFile;
    private Dictionary<string, CacheEntry> _cacheIndex = new();

    public DecompilationCacheManager()
    {
        _cacheDirectory = Path.Combine(Path.GetTempPath(), "McPeek", "cache");
        _indexFile = Path.Combine(_cacheDirectory, "cache-index.json");
        
        Directory.CreateDirectory(_cacheDirectory);
        LoadCacheIndex();
    }

    /// <summary>
    /// Computes SHA256 hash of a file
    /// </summary>
    public string ComputeFileHash(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// Checks if a DLL is cached based on its hash
    /// </summary>
    public bool IsCached(string dllPath, out string hash)
    {
        hash = ComputeFileHash(dllPath);
        return _cacheIndex.ContainsKey(hash);
    }

    /// <summary>
    /// Gets cached decompilation result
    /// </summary>
    public DecompiledAssembly? GetCached(string hash)
    {
        if (!_cacheIndex.TryGetValue(hash, out var entry))
            return null;

        var cacheFilePath = GetCacheFilePath(hash);
        if (!File.Exists(cacheFilePath))
        {
            // Cache file missing, remove from index
            _cacheIndex.Remove(hash);
            SaveCacheIndex();
            return null;
        }

        var json = File.ReadAllText(cacheFilePath);
        return JsonSerializer.Deserialize<DecompiledAssembly>(json);
    }

    /// <summary>
    /// Saves decompilation result to cache
    /// </summary>
    public void SaveToCache(string hash, string originalPath, DecompiledAssembly decompiledAssembly)
    {
        var cacheFilePath = GetCacheFilePath(hash);
        var json = JsonSerializer.Serialize(decompiledAssembly, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        
        File.WriteAllText(cacheFilePath, json);

        _cacheIndex[hash] = new CacheEntry
        {
            Hash = hash,
            OriginalPath = originalPath,
            CachedAt = DateTime.UtcNow,
            AssemblyName = decompiledAssembly.AssemblyName
        };

        SaveCacheIndex();
    }

    /// <summary>
    /// Clears all cached data
    /// </summary>
    public void ClearCache()
    {
        if (Directory.Exists(_cacheDirectory))
        {
            Directory.Delete(_cacheDirectory, recursive: true);
            Directory.CreateDirectory(_cacheDirectory);
        }

        _cacheIndex.Clear();
        SaveCacheIndex();
    }

    /// <summary>
    /// Gets statistics about the cache
    /// </summary>
    public CacheStatistics GetStatistics()
    {
        return new CacheStatistics
        {
            TotalCachedAssemblies = _cacheIndex.Count,
            CacheDirectory = _cacheDirectory,
            TotalSizeBytes = GetCacheSize()
        };
    }

    private string GetCacheFilePath(string hash)
    {
        return Path.Combine(_cacheDirectory, $"{hash}.json");
    }

    private void LoadCacheIndex()
    {
        if (File.Exists(_indexFile))
        {
            try
            {
                var json = File.ReadAllText(_indexFile);
                _cacheIndex = JsonSerializer.Deserialize<Dictionary<string, CacheEntry>>(json) 
                    ?? new Dictionary<string, CacheEntry>();
            }
            catch
            {
                _cacheIndex = new Dictionary<string, CacheEntry>();
            }
        }
        else
        {
            _cacheIndex = new Dictionary<string, CacheEntry>();
        }
    }

    private void SaveCacheIndex()
    {
        var json = JsonSerializer.Serialize(_cacheIndex, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(_indexFile, json);
    }

    private long GetCacheSize()
    {
        var directory = new DirectoryInfo(_cacheDirectory);
        return directory.GetFiles("*.json", SearchOption.AllDirectories)
            .Sum(file => file.Length);
    }
}

public class CacheEntry
{
    public string Hash { get; set; } = string.Empty;
    public string OriginalPath { get; set; } = string.Empty;
    public DateTime CachedAt { get; set; }
    public string AssemblyName { get; set; } = string.Empty;
}

public class CacheStatistics
{
    public int TotalCachedAssemblies { get; set; }
    public string CacheDirectory { get; set; } = string.Empty;
    public long TotalSizeBytes { get; set; }
}
