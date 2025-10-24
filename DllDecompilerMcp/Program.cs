using DllDecompilerMcp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging to stderr (MCP requirement)
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Register services
builder.Services.AddSingleton<DecompilationCacheManager>();
builder.Services.AddSingleton<DecompilationService>();

// Configure MCP server with stdio transport
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

Console.Error.WriteLine("DLL Decompiler MCP Server starting...");
Console.Error.WriteLine("Ready to accept MCP requests via stdio");

await app.RunAsync();
