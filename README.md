# DLL Decompiler MCP Server

A Model Context Protocol (MCP) server that decompiles .NET DLL assemblies using ICSharpCode.Decompiler (ILSpy's library) and exposes the decompiled code to AI tools.

## Features

- **Decompilation**: Decompiles .NET DLLs to C# source code
- **Caching**: Persistent file-based caching using SHA256 hash for speed
- **Search**: Search across all decompiled code
- **Resources**: Exposes decompiled files as MCP resources
- **Tools**: Provides MCP tools for decompilation and search operations

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
    "mcpeek": {
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
5. **Program.cs**: MCP server host setup

### Technologies

- **ICSharpCode.Decompiler**: ILSpy's decompilation library
- **Microsoft.Extensions.AI**: MCP server implementation
- **Microsoft.Extensions.Hosting**: Hosting infrastructure
- **.NET 9.0**: Runtime platform

## Example Workflow

1. **Decompile a folder of DLLs**:
   ```
   Tool: DecompileFolder
   folderPath: "C:\\MyProject\\bin\\Debug"
   ```

2. **Search for a specific class**:
   ```
   Tool: SearchCode
   query: "MyImportantClass"
   ```

3. **Access decompiled source**:
   ```
   Resource: decompiled://MyAssembly/MyNamespace/MyImportantClass.cs
   ```

4. **Get assembly overview**:
   ```
   Resource Operation: GetAssemblyOverview
   assemblyName: "MyAssembly"
   ```

## License

This project uses ICSharpCode.Decompiler which is licensed under the MIT License.

## Contributing

Contributions are welcome! Please ensure all changes follow the Object Calisthenics and Conventional Commits guidelines.
