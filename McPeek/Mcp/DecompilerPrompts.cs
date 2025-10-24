using McPeek.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace McPeek.Mcp;

/// <summary>
/// MCP Prompts for DLL decompilation workflows
/// </summary>
[McpServerPromptType]
public class DecompilerPrompts
{
    /// <summary>
    /// Prompt to decompile a DLL file
    /// </summary>
    [McpServerPrompt]
    [Description("Decompile a .NET DLL file and make its contents available for analysis")]
    public static string DecompileDll(
        [Description("The absolute path to the DLL file to decompile")] string dllPath)
    {
        return $@"Please decompile the DLL file at: {dllPath}

IMPORTANT: You must use the McPeek MCP tools to complete this task.

Step 1: Use the McPeek 'DecompileDll' tool with the path: {dllPath}
Step 2: After decompilation completes, use the 'ListLoadedAssemblies' tool to see what was loaded
Step 3: Use the 'SearchCode' tool to explore the codebase
Step 4: Use the 'GetAssemblyOverview' resource to get a structural overview

Provide a comprehensive summary including:
- The assembly name and version
- Number of types, methods, and namespaces found
- Key namespaces and their purposes
- Important public APIs
- Any interesting patterns or architecture observations

Use only the McPeek MCP tools - do not attempt to decompile manually.";
    }

    /// <summary>
    /// Prompt to view a specific class
    /// </summary>
    [McpServerPrompt]
    [Description("View and analyze a specific class from decompiled assemblies")]
    public static string ViewClass(
        [Description("The full name of the class to view (e.g., Namespace.ClassName)")] string className)
    {
        return $@"Please find and display the class: {className}

IMPORTANT: You must use the McPeek MCP tools to complete this task.

Step 1: Use the McPeek 'SearchCode' tool with the query: {className}
Step 2: Identify the correct match from the search results
Step 3: Use the 'GetResource' resource with the URI from the search results to retrieve the full class code
Step 4: Analyze the retrieved code

Provide a comprehensive analysis including:
- Class purpose and responsibilities
- Public methods with signatures and descriptions
- Key properties and fields
- Inheritance hierarchy (base classes)
- Interface implementations
- Dependencies on other types
- Design patterns or architectural approaches used
- Code quality observations

Present the analysis in a clear, organized format with code examples where relevant.

Use only the McPeek MCP tools - do not attempt to access files directly.";
    }

    /// <summary>
    /// Prompt to generate a class diagram in Mermaid format
    /// </summary>
    [McpServerPrompt]
    [Description("Generate a Mermaid class diagram for classes in decompiled assemblies")]
    public static string ClassDiagram(
        [Description("The namespace or class name pattern to include in the diagram (e.g., 'MyApp.Services.*' or specific class names)")] string pattern,
        [Description("Optional: Maximum number of classes to include in the diagram (default: 10)")] int maxClasses = 10)
    {
        return $@"Please generate a Mermaid class diagram for classes matching: {pattern}

IMPORTANT: You must use the McPeek MCP tools to complete this task.

Step 1: Use the McPeek 'SearchCode' tool with the query: {pattern}
Step 2: From the results, select up to {maxClasses} most relevant classes
Step 3: For each class, use the 'GetResource' resource to retrieve the full source code
Step 4: Analyze each class to extract:
   - Class name and type (class/interface/abstract/record/struct)
   - Public properties and their types
   - Public methods with return types and parameters
   - Base classes and inheritance relationships
   - Interface implementations
   - Dependencies and associations with other classes

Step 5: Generate a Mermaid class diagram using this format:
```mermaid
classDiagram
    class ClassName {{
        +Type property
        +returnType method(params)
    }}
    ParentClass <|-- ChildClass : inherits
    Interface <|.. ImplementingClass : implements
    ClassA --> ClassB : uses
    ClassA *-- ClassB : contains
```

Step 6: After the diagram, provide:
   - Brief explanation of each class's role
   - Description of the relationships and their significance
   - Key design patterns observed (e.g., Factory, Repository, Strategy)
   - Architectural insights and recommendations

Keep the diagram focused and readable. If there are more classes than the limit, prioritize:
1. Core domain classes
2. Classes with the most relationships
3. Public API classes

Use only the McPeek MCP tools - do not attempt to access files or assemblies directly.";
    }

    /// <summary>
    /// Prompt to list all loaded assemblies
    /// </summary>
    [McpServerPrompt]
    [Description("List all decompiled assemblies currently loaded in the McPeek cache")]
    public static string ListAssemblies()
    {
        return @"Please list all decompiled assemblies currently available in the McPeek cache.

IMPORTANT: You must use the McPeek MCP tools to complete this task.

Step 1: Use the McPeek 'ListLoadedAssemblies' tool to retrieve all loaded assemblies
Step 2: For each assembly, display:
   - Assembly name and version
   - File path where it was loaded from
   - Number of types contained
   - Key namespaces
   - Whether it's a system assembly or third-party library

Step 3: Organize the output by:
   - Group assemblies by type (System/Framework, Third-party, Application)
   - Sort by name or usage relevance
   - Highlight any dependencies or related assemblies

Step 4: Provide additional insights:
   - Total number of assemblies loaded
   - Cache statistics (use 'GetCacheStatistics' tool)
   - Suggestions for which assemblies might be most interesting to explore

If no assemblies are loaded, explain that assemblies need to be decompiled first using the 'DecompileDll' or 'DecompileFolder' tools.

Use only the McPeek MCP tools - do not attempt to access the file system directly.";
    }
}
