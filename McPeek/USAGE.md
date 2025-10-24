# McPeek - DLL Decompiler MCP Server - Usage Examples

## Quick Start

### 1. Start the Server

```bash
cd c:\development\mcpeek\McPeek
dotnet run
```

The server will start and listen for MCP requests via stdio.

### 2. Configure in VS Code/Cline

Add to your MCP settings file (usually in VS Code settings or Cline configuration):

```json
{
  "mcpServers": {
    "dll-decompiler": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "c:\\development\\mcpeek\\McPeek\\McPeek.csproj"
      ]
    }
  }
}
```

## Example Workflows

### Example 1: Decompile a Project's Dependencies

```
AI: Use the DecompileFolder tool to decompile all DLLs in C:\MyProject\bin\Debug\net8.0
```

**Expected Response:**
```json
{
  "Success": true,
  "Message": "Successfully decompiled 15 assemblies",
  "AssemblyNames": [
    "MyProject",
    "Microsoft.Extensions.DependencyInjection",
    ...
  ],
  "TotalFiles": 450
}
```

### Example 2: Find Implementation of an Interface

```
AI: Search for "IServiceProvider" in the decompiled code
```

**Expected Response:**
```json
{
  "Success": true,
  "Query": "IServiceProvider",
  "TotalMatches": 23,
  "Results": [
    {
      "AssemblyName": "Microsoft.Extensions.DependencyInjection",
      "FilePath": "ServiceProvider.cs",
      "LineNumber": 15,
      "LineContent": "public class ServiceProvider : IServiceProvider",
      "Context": "...(surrounding code)..."
    },
    ...
  ]
}
```

### Example 3: Analyze a Specific Class

```
AI: First, decompile C:\MyProject\bin\Debug\MyLibrary.dll
Then search for "MyImportantClass"
```

This workflow:
1. Decompiles the specific DLL
2. Searches for the class
3. AI can then analyze the implementation details

### Example 4: Understanding Dependencies

```
AI: List all loaded assemblies
```

**Expected Response:**
```json
{
  "Success": true,
  "TotalAssemblies": 15,
  "Assemblies": [
    {
      "Name": "MyProject",
      "Path": "C:\\MyProject\\bin\\Debug\\MyProject.dll",
      "FileCount": 30,
      "DecompiledAt": "2025-10-24T12:00:00Z"
    },
    ...
  ]
}
```

### Example 5: Cache Management

```
AI: Get cache statistics
```

**Expected Response:**
```json
{
  "Success": true,
  "TotalCachedAssemblies": 15,
  "CacheDirectory": "C:\\Users\\User\\AppData\\Local\\Temp\\McPeek\\cache",
  "CacheSizeMB": 12.5
}
```

To clear the cache:
```
AI: Clear the decompilation cache
```

## Real-World Scenarios

### Scenario 1: Understanding Third-Party Libraries

**Objective:** Understand how a third-party NuGet package implements a specific feature.

**Steps:**
1. Locate the DLL in your NuGet packages folder (usually `%USERPROFILE%\.nuget\packages`)
2. Decompile: `DecompileDll C:\Users\You\.nuget\packages\SomeLibrary\1.0.0\lib\net8.0\SomeLibrary.dll`
3. Search for the feature: `SearchCode "FeatureName"`
4. Ask AI to explain the implementation

### Scenario 2: Debugging Production Issues

**Objective:** Analyze decompiled code from a production DLL to understand a bug.

**Steps:**
1. Copy production DLLs to a local folder
2. Decompile the folder: `DecompileFolder C:\ProductionDlls`
3. Search for the problematic method: `SearchCode "ProblematicMethod"`
4. Ask AI to identify potential issues in the code

### Scenario 3: API Documentation Generation

**Objective:** Generate documentation for an undocumented internal library.

**Steps:**
1. Decompile the library: `DecompileDll C:\InternalLibs\MyLib.dll`
2. List all types: Ask AI to summarize all public classes
3. For each class, ask AI to generate documentation based on the decompiled code

### Scenario 4: Migration Analysis

**Objective:** Understand what changes are needed to migrate from one framework version to another.

**Steps:**
1. Decompile old framework DLLs: `DecompileFolder C:\OldFramework\bin`
2. Decompile new framework DLLs: `DecompileFolder C:\NewFramework\bin`
3. Search for specific APIs: `SearchCode "ObsoleteAPI"`
4. Ask AI to identify migration paths

## Tips

1. **Cache is Your Friend**: The cache uses SHA256 hashes, so if you decompile the same DLL multiple times, it's instant after the first time.

2. **Search Smartly**: Use specific search terms to reduce the number of results. The default limit is 50 results per search.

3. **Case Sensitivity**: By default, searches are case-insensitive. Use the `caseSensitive` parameter if you need exact matches.

4. **Large Projects**: For large projects with many DLLs, decompile one folder at a time to keep things manageable.

5. **XML Documentation**: The decompiler includes XML documentation comments when available, making the AI's understanding more accurate.

## Troubleshooting

### Issue: "Failed to decompile DLL"

**Possible causes:**
- The file is not a .NET assembly
- The assembly is obfuscated
- File permissions issue

**Solution:**
- Verify the file is a valid .NET DLL using `dotnet --info` or ILSpy
- Check file permissions

### Issue: "Cache growing too large"

**Solution:**
```
AI: Clear the decompilation cache
```

Then selectively decompile only what you need.

### Issue: "Search returns too many results"

**Solution:**
- Use more specific search terms
- Add the namespace or class name
- Use the `maxResults` parameter to limit results
- Use `caseSensitive` search for exact matches

## Performance Notes

- **First decompilation**: Can take a few seconds per DLL depending on size
- **Cached decompilation**: Instant (< 100ms)
- **Search**: Very fast across all loaded assemblies
- **Cache location**: `%TEMP%\McPeek\cache`

## Security Considerations

- The server only decompiles local DLLs that you explicitly specify
- No network access required
- Cache is stored in your local temp folder
- All operations are read-only (doesn't modify source DLLs)

## Integration with AI Workflows

The MCP server is designed to work seamlessly with AI assistants like:
- GitHub Copilot (VS Code Agent Mode)
- Claude with MCP support
- Custom AI agents built with Semantic Kernel

The AI can:
- Understand complex library implementations
- Explain how specific features work
- Identify patterns and best practices
- Suggest improvements based on decompiled code
- Help with debugging by analyzing actual compiled code
