using McPeek.Services;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McPeek.Mcp;

/// <summary>
/// MCP Resources for accessing decompiled DLL content
/// </summary>
[McpServerResourceType]
public class DecompilerResources
{
    private readonly DecompilationService _decompilationService;

    public DecompilerResources(DecompilationService decompilationService)
    {
        _decompilationService = decompilationService;
    }

    /// <summary>
    /// Gets a list of all available decompiled files as resources
    /// </summary>
    [McpServerResource]
    [Description("Lists all decompiled C# source files available as resources")]
    public List<ResourceInfo> ListResources()
    {
        var assemblies = _decompilationService.GetLoadedAssemblies();
        var resources = new List<ResourceInfo>();

        foreach (var assembly in assemblies)
        {
            foreach (var file in assembly.Files)
            {
                resources.Add(new ResourceInfo
                {
                    Uri = $"decompiled://{assembly.AssemblyName}/{file.Path}",
                    Name = $"{assembly.AssemblyName}/{file.Path}",
                    Description = $"Decompiled source for {file.TypeName}",
                    MimeType = "text/x-csharp"
                });
            }
        }

        return resources;
    }

    /// <summary>
    /// Gets the content of a specific decompiled file
    /// </summary>
    [McpServerResource]
    [Description("Retrieves the decompiled C# source code for a specific file")]
    public ResourceContent? GetResource(
        [Description("The URI of the resource (e.g., decompiled://AssemblyName/Namespace/TypeName.cs)")] string uri)
    {
        if (!uri.StartsWith("decompiled://"))
            return null;

        var path = uri.Substring("decompiled://".Length);
        var parts = path.Split('/', 2);
        
        if (parts.Length < 2)
            return null;

        var assemblyName = parts[0];
        var filePath = parts[1];

        var assembly = _decompilationService.GetLoadedAssemblies()
            .FirstOrDefault(a => a.AssemblyName == assemblyName);

        if (assembly == null)
            return null;

        var file = assembly.Files.FirstOrDefault(f => f.Path == filePath);
        
        if (file == null)
            return null;

        return new ResourceContent
        {
            Uri = uri,
            Content = file.Content,
            MimeType = "text/x-csharp"
        };
    }

    /// <summary>
    /// Gets assembly overview information
    /// </summary>
    [McpServerResource]
    [Description("Gets an overview of a decompiled assembly including all types and namespaces")]
    public AssemblyOverview? GetAssemblyOverview(
        [Description("The name of the assembly")] string assemblyName)
    {
        var assembly = _decompilationService.GetLoadedAssemblies()
            .FirstOrDefault(a => a.AssemblyName == assemblyName);

        if (assembly == null)
            return null;

        var namespaces = assembly.Files
            .GroupBy(f => f.Namespace)
            .Select(g => new NamespaceInfo
            {
                Name = g.Key,
                Types = g.Select(f => new TypeInfo
                {
                    Name = f.TypeName,
                    FilePath = f.Path,
                    IsPublic = f.IsPublic
                }).ToList()
            })
            .ToList();

        return new AssemblyOverview
        {
            AssemblyName = assembly.AssemblyName,
            AssemblyPath = assembly.AssemblyPath,
            TotalFiles = assembly.Files.Count,
            DecompiledAt = assembly.DecompiledAt,
            Namespaces = namespaces
        };
    }
}

// Resource DTOs
public class ResourceInfo
{
    public string Uri { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
}

public class ResourceContent
{
    public string Uri { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
}

public class AssemblyOverview
{
    public string AssemblyName { get; set; } = string.Empty;
    public string AssemblyPath { get; set; } = string.Empty;
    public int TotalFiles { get; set; }
    public DateTime DecompiledAt { get; set; }
    public List<NamespaceInfo> Namespaces { get; set; } = new();
}

public class NamespaceInfo
{
    public string Name { get; set; } = string.Empty;
    public List<TypeInfo> Types { get; set; } = new();
}

public class TypeInfo
{
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
}
