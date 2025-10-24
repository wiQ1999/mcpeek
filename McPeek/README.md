# McPeek - DLL Decompiler MCP Server

A Model Context Protocol (MCP) server that decompiles .NET DLL assemblies using ICSharpCode.Decompiler (ILSpy's library) and exposes the decompiled code to AI tools.

## Features

- **Decompilation**: Decompiles .NET DLLs to C# source code
- **Caching**: Persistent file-based caching using SHA256 hash for speed
- **Search**: Search across all decompiled code
- **Resources**: Exposes decompiled files as MCP resources
- **Tools**: Provides MCP tools for decompilation and search operations
- **Prompts**: Pre-built workflow prompts for common decompilation tasks

## Installation

### Prerequisites

- .NET 9.0 SDK or later
- Windows, Linux, or macOS

### Build

```bash
cd c:\development\mcpeek\McPeek
dotnet build
```

## Usage

### Running the Server

```bash
dotnet run
```

The server communicates via stdio and follows the MCP protocol.

### MCP Prompts

The server provides pre-built prompts to streamline common workflows:

#### /decompile

Decompiles a DLL file and provides an analysis summary.

**Parameters:**
- `dllPath` (string): The absolute path to the DLL file

**Example:**
```
/decompile C:\MyProject\bin\Debug\MyLibrary.dll
```

This prompt will decompile the assembly and provide a summary including assembly name, type count, namespaces, and key public APIs.

#### /view_class

Views and analyzes a specific class from decompiled assemblies.

**Parameters:**
- `className` (string): The full name of the class (e.g., `Namespace.ClassName`)

**Example:**
```
/view_class MyNamespace.MyClass
```

This prompt searches for the class, retrieves its source code, and provides a detailed analysis including purpose, methods, properties, inheritance, and design patterns.

#### /class_diagram

Generates a Mermaid class diagram for classes matching a pattern.

**Parameters:**
- `pattern` (string): Namespace or class name pattern (e.g., `MyApp.Services.*`)
- `maxClasses` (int, optional): Maximum classes to include (default: 10)

**Example:**
```
/class_diagram MyApp.Services.* 15
```

This prompt generates a visual class diagram showing relationships, inheritance, interfaces, and associations between classes.

#### /list_assemblies

Lists all decompiled assemblies currently loaded in the McPeek cache.

**Parameters:**
- None

**Example:**
```
/list_assemblies
```

This prompt displays all loaded assemblies with details including assembly names, versions, file paths, type counts, key namespaces, and cache statistics. It organizes assemblies by type (System/Framework, Third-party, Application) and provides insights on what's available for analysis.

### MCP Tools

The server exposes the following tools:

#### 1. DecompileFolder

Decompiles all .NET DLL files in a specified folder.

**Parameters:**
- `folderPath` (string): The absolute path to the folder containing DLL files

**Returns:**
```json
{
  "Success": true,
  "Message": "Successfully decompiled 5 assemblies",
  "AssemblyNames": ["Assembly1", "Assembly2", ...],
  "TotalFiles": 150
}
```

#### 2. DecompileDll

Decompiles a specific DLL file.

**Parameters:**
- `dllPath` (string): The absolute path to the DLL file

**Returns:**
```json
{
  "Success": true,
  "Message": "Successfully decompiled MyAssembly",
  "AssemblyNames": ["MyAssembly"],
  "TotalFiles": 30
}
```

#### 3. SearchCode

Searches for text patterns across all decompiled C# code.

**Parameters:**
- `query` (string): The text to search for
- `caseSensitive` (bool, optional): Whether search should be case-sensitive (default: false)
- `maxResults` (int, optional): Maximum number of results to return (default: 50)

**Returns:**
```json
{
  "Success": true,
  "Query": "MyClass",
  "TotalMatches": 10,
  "Results": [
    {
      "AssemblyName": "MyAssembly",
      "FilePath": "MyNamespace/MyClass.cs",
      "LineNumber": 42,
      "LineContent": "public class MyClass",
      "Context": "..."
    }
  ]
}
```

#### 4. ListLoadedAssemblies

Lists all assemblies that have been decompiled and are currently loaded.

**Returns:**
```json
{
  "Success": true,
  "TotalAssemblies": 5,
  "Assemblies": [
    {
      "Name": "MyAssembly",
      "Path": "C:\\path\\to\\MyAssembly.dll",
      "FileCount": 30,
      "DecompiledAt": "2025-10-24T12:00:00Z"
    }
  ]
}
```

#### 5. GetCacheStatistics

Gets statistics about the decompilation cache.

**Returns:**
```json
{
  "Success": true,
  "TotalCachedAssemblies": 10,
  "CacheDirectory": "C:\\Users\\...\\Temp\\McPeek\\cache",
  "CacheSizeMB": 15.5
}
```

#### 6. ClearCache

Clears all cached decompilation results.

**Returns:**
```json
{
  "Success": true,
  "Message": "Cache cleared successfully"
}
```

### MCP Resources

Decompiled files are exposed as resources with URIs in the format:

```
decompiled://AssemblyName/Namespace/TypeName.cs
```

For example:
```
decompiled://MyAssembly/MyNamespace/MyClass.cs
```

### Resource Operations

#### ListResources

Lists all available decompiled files.

**Returns:**
```json
[
  {
    "Uri": "decompiled://MyAssembly/MyNamespace/MyClass.cs",
    "Name": "MyAssembly/MyNamespace/MyClass.cs",
    "Description": "Decompiled source for MyNamespace.MyClass",
    "MimeType": "text/x-csharp"
  }
]
```

#### GetResource

Retrieves the decompiled C# source code for a specific file.

**Parameters:**
- `uri` (string): The resource URI

**Returns:**
```json
{
  "Uri": "decompiled://MyAssembly/MyNamespace/MyClass.cs",
  "Content": "using System;\n\nnamespace MyNamespace\n{\n  public class MyClass\n  {\n    ...\n  }\n}",
  "MimeType": "text/x-csharp"
}
```

#### GetAssemblyOverview

Gets an overview of a decompiled assembly.

**Parameters:**
- `assemblyName` (string): The name of the assembly

**Returns:**
```json
{
  "AssemblyName": "MyAssembly",
  "AssemblyPath": "C:\\path\\to\\MyAssembly.dll",
  "TotalFiles": 30,
  "DecompiledAt": "2025-10-24T12:00:00Z",
  "Namespaces": [
    {
      "Name": "MyNamespace",
      "Types": [
        {
          "Name": "MyNamespace.MyClass",
          "FilePath": "MyNamespace/MyClass.cs",
          "IsPublic": true
        }
      ]
    }
  ]
}
```

## Cache

The server uses a persistent file-based cache stored in the system's temp directory:

```
%TEMP%\McPeek\cache\
```

The cache uses SHA256 hashes to detect when DLLs have changed. If a DLL hasn't changed, the cached decompilation is returned instantly.

## Configuration

### VS Code / Cline Integration

To use this MCP server with VS Code or Cline, add it to your MCP settings:

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

## Architecture

### Components

1. **DecompilationCacheManager**: Manages persistent file-based caching with SHA256 hashing
2. **DecompilationService**: Core decompilation service using ICSharpCode.Decompiler
3. **DecompilerTools**: MCP tools for decompilation and search operations
4. **DecompilerResources**: MCP resources for accessing decompiled content
5. **DecompilerPrompts**: Pre-built prompts for common workflows
6. **Program.cs**: MCP server host setup

### Technologies

- **ICSharpCode.Decompiler**: ILSpy's decompilation library
- **Microsoft.Extensions.AI**: MCP server implementation
- **Microsoft.Extensions.Hosting**: Hosting infrastructure
- **.NET 9.0**: Runtime platform

## Example Workflows

### Quick Start with Prompts

The easiest way to use McPeek is through the built-in prompts:

**1. Decompile and analyze a DLL:**
```
/decompile C:\MyProject\bin\Debug\MyLibrary.dll
```
This will decompile the assembly and provide a comprehensive summary.

**2. Explore a specific class:**
```
/view_class MyNamespace.MyImportantClass
```
This searches for the class and provides detailed analysis including methods, properties, and design patterns.

**3. Visualize class relationships:**
```
/class_diagram MyApp.Services.* 20
```
This generates a Mermaid class diagram showing inheritance, interfaces, and relationships for up to 20 classes.

**4. List all loaded assemblies:**
```
/list_assemblies
```
This displays all decompiled assemblies currently in the cache with details like versions, paths, and namespace information.

### Manual Tool Usage

You can also use the tools directly for more control:

**1. Decompile a folder of DLLs:**
```
Tool: DecompileFolder
folderPath: "C:\\MyProject\\bin\\Debug"
```
Result: All DLLs in the folder are decompiled and cached.

**2. Search for code patterns:**
```
Tool: SearchCode
query: "IServiceProvider"
caseSensitive: false
maxResults: 50
```
Result: Finds all occurrences across decompiled assemblies with context.

**3. List all loaded assemblies:**
```
Tool: ListLoadedAssemblies
```
Result: Shows all decompiled assemblies with file counts and timestamps.

**4. Get cache statistics:**
```
Tool: GetCacheStatistics
```
Result: Cache size, location, and number of cached assemblies.

### Using Resources

Access decompiled source code directly through MCP resources:

**1. List all available decompiled files:**
```
Resource: ListResources
```

**2. Read a specific decompiled file:**
```
Resource: GetResource
uri: "decompiled://MyAssembly/MyNamespace/MyClass.cs"
```

**3. Get assembly overview with structure:**
```
Resource: GetAssemblyOverview
assemblyName: "MyAssembly"
```

### Real-World Example

**Scenario: Understanding a third-party NuGet package**

```
# Step 1: Use the /decompile prompt
/decompile C:\Users\Me\.nuget\packages\SomeLibrary\1.0.0\lib\net8.0\SomeLibrary.dll

# AI Response: Decompiled SomeLibrary with 45 types across 3 namespaces...

# Step 2: Ask for a specific class
/view_class SomeLibrary.Core.ApiClient

# AI Response: Detailed analysis of ApiClient class with methods and usage patterns...

# Step 3: Generate architecture diagram
/class_diagram SomeLibrary.Core.* 15

# AI Response: Mermaid diagram showing the architecture...
```

### Advanced Usage

**Cache Management:**
```
# Check cache size
Tool: GetCacheStatistics

# Clear cache when needed
Tool: ClearCache
```

**Targeted Search:**
```
# Case-sensitive search for exact matches
Tool: SearchCode
query: "IDisposable"
caseSensitive: true
maxResults: 100
```

**Combine with AI Analysis:**
Ask the AI to:
- "Compare the implementation of X in these two assemblies"
- "Find all classes that implement INotifyPropertyChanged"
- "Generate documentation for all public APIs in namespace Y"
- "Identify potential memory leaks in the caching logic"

## License

This project uses ICSharpCode.Decompiler which is licensed under the MIT License.

## Contributing

Contributions are welcome! Please ensure all changes follow the Object Calisthenics and Conventional Commits guidelines.
