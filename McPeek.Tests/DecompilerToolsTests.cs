using McPeek.Mcp;
using McPeek.Services;
using Xunit;

namespace McPeek.Tests;

public class DecompilerToolsTests : IDisposable
{
    private readonly DecompilationCacheManager _cacheManager;
    private readonly DecompilationService _decompilationService;
    private readonly string _testDllPath;
    private bool _assemblyDecompiled;

    public DecompilerToolsTests()
    {
        _cacheManager = new DecompilationCacheManager();
        _decompilationService = new DecompilationService(_cacheManager);
        
        // Use System.Text.Json as a test assembly (it's always available)
        _testDllPath = typeof(System.Text.Json.JsonSerializer).Assembly.Location;
        _assemblyDecompiled = false;
    }

    public void Dispose()
    {
        // Clean up any test data if needed
        GC.SuppressFinalize(this);
    }

    private async Task EnsureTestAssemblyDecompiledAsync()
    {
        if (!_assemblyDecompiled)
        {
            var result = await DecompilerTools.DecompileDll(_decompilationService, _testDllPath);
            Assert.True(result.Success, $"Failed to decompile test assembly: {result.Message}");
            _assemblyDecompiled = true;
        }
    }

    [Fact]
    public async Task DecompileDll_ValidPath_ReturnsSuccess()
    {
        // Arrange & Act
        var result = await DecompilerTools.DecompileDll(_decompilationService, _testDllPath);

        // Assert
        Assert.True(result.Success);
        Assert.Contains("System.Text.Json", result.AssemblyNames);
        Assert.True(result.TotalFiles > 0);
    }

    [Fact]
    public async Task DecompileDll_InvalidPath_ReturnsError()
    {
        // Arrange
        var invalidPath = "C:\\NonExistent\\Invalid.dll";

        // Act
        var result = await DecompilerTools.DecompileDll(_decompilationService, invalidPath);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ListLoadedAssemblies_AfterDecompilation_ReturnsAssemblies()
    {
        // Arrange
        await EnsureTestAssemblyDecompiledAsync();

        // Act
        var result = DecompilerTools.ListLoadedAssemblies(_decompilationService);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.TotalAssemblies > 0);
        Assert.Contains(result.Assemblies, a => a.Name == "System.Text.Json");
    }

    [Fact]
    public async Task SearchCode_ExistingPattern_ReturnsMatches()
    {
        // Arrange
        await EnsureTestAssemblyDecompiledAsync();

        // Act
        var result = DecompilerTools.SearchCode(_decompilationService, "JsonSerializer", caseSensitive: false);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.TotalMatches > 0);
        Assert.NotEmpty(result.Results);
    }

    [Fact]
    public async Task SearchCode_NonExistentPattern_ReturnsNoMatches()
    {
        // Arrange
        await EnsureTestAssemblyDecompiledAsync();

        // Act
        var result = DecompilerTools.SearchCode(_decompilationService, "ThisClassDoesNotExist12345", caseSensitive: false);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0, result.TotalMatches);
        Assert.Empty(result.Results);
    }

    [Fact]
    public async Task GetClass_ExistingClass_ReturnsSourceCode()
    {
        // Arrange
        await EnsureTestAssemblyDecompiledAsync();

        // Act
        var result = DecompilerTools.GetClass(_decompilationService, "JsonSerializer", caseSensitive: false);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("JsonSerializer", result.ClassName);
        Assert.Equal("System.Text.Json", result.Namespace);
        Assert.Equal("System.Text.Json", result.AssemblyName);
        Assert.NotNull(result.SourceCode);
        Assert.NotEmpty(result.SourceCode);
        Assert.Contains("class JsonSerializer", result.SourceCode);
    }

    [Fact]
    public async Task GetClass_FullyQualifiedName_ReturnsSourceCode()
    {
        // Arrange
        await EnsureTestAssemblyDecompiledAsync();

        // Act
        var result = DecompilerTools.GetClass(_decompilationService, "System.Text.Json.JsonSerializer", caseSensitive: false);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("JsonSerializer", result.ClassName);
        Assert.Equal("System.Text.Json", result.Namespace);
        Assert.NotNull(result.SourceCode);
        Assert.NotEmpty(result.SourceCode);
    }

    [Fact]
    public async Task GetClass_NonExistentClass_ReturnsError()
    {
        // Arrange
        await EnsureTestAssemblyDecompiledAsync();

        // Act
        var result = DecompilerTools.GetClass(_decompilationService, "ThisClassDoesNotExist12345", caseSensitive: false);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetClass_CaseSensitive_MatchesExactCase()
    {
        // Arrange
        await EnsureTestAssemblyDecompiledAsync();

        // Act - correct case
        var resultCorrect = DecompilerTools.GetClass(_decompilationService, "JsonSerializer", caseSensitive: true);
        
        // Act - wrong case
        var resultWrong = DecompilerTools.GetClass(_decompilationService, "jsonserializer", caseSensitive: true);

        // Assert
        Assert.True(resultCorrect.Success);
        Assert.False(resultWrong.Success);
    }

    [Fact]
    public async Task GetClass_CaseInsensitive_MatchesAnyCase()
    {
        // Arrange
        await EnsureTestAssemblyDecompiledAsync();

        // Act
        var result = DecompilerTools.GetClass(_decompilationService, "jsonserializer", caseSensitive: false);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("JsonSerializer", result.ClassName);
    }

    [Fact]
    public async Task GetCacheStatistics_ReturnsValidStats()
    {
        // Arrange
        await EnsureTestAssemblyDecompiledAsync();

        // Act
        var result = DecompilerTools.GetCacheStatistics(_cacheManager);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.TotalCachedAssemblies > 0);
        Assert.NotNull(result.CacheDirectory);
        Assert.True(result.CacheSizeMB >= 0);
    }

    [Fact]
    public async Task ClearCache_RemovesAllCachedAssemblies()
    {
        // Arrange
        await EnsureTestAssemblyDecompiledAsync();
        var statsBefore = DecompilerTools.GetCacheStatistics(_cacheManager);
        Assert.True(statsBefore.TotalCachedAssemblies > 0, "Cache should have assemblies before clearing");

        // Act
        var result = DecompilerTools.ClearCache(_cacheManager);

        // Assert
        Assert.True(result.Success);
        
        var statsAfter = DecompilerTools.GetCacheStatistics(_cacheManager);
        Assert.Equal(0, statsAfter.TotalCachedAssemblies);
    }
}
