using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Text.Json;

// COMMAND TO RUN: dotnet run
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/api/process-data", async (HttpContext context) =>
{
    try
    {
        using StreamReader reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();

        // SIMULATED VULNERABILITY 1: JSON Parsing Crash
        // If the fuzzer sends malformed JSON, .NET throws an exception.
        // A real service should handle this, but ours will crash (500 Error).
        var data = JsonSerializer.Deserialize<UserInput>(body);

        if (data == null || data.Payload == null) return Results.BadRequest();

        // SIMULATED VULNERABILITY 2: Logic Bomb (The "Variant")
        // If the payload length is exactly 42 characters, the service crashes.
        // This simulates a "Memory Corruption" or "Buffer Edge Case".
        if (data.Payload.Length == 42)
        {
            throw new InvalidOperationException("CRITICAL MEMORY FAILURE: Buffer Edge Case Hit!");
        }

        return Results.Ok($"Processed: {data.Payload}");
    }
    catch (Exception ex)
    {
        // The Fuzzer looks for this 500 Error code to confirm a "Kill"
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
});

app.Run("http://localhost:5000");

// Data Model
public class UserInput
{
    public string? Payload { get; set; }
}
