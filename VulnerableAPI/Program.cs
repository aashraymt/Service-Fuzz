using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/api/process-data", async (HttpContext context) =>
{
    try
    {
        using StreamReader reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        var data = JsonSerializer.Deserialize<UserInput>(body);

        if (data == null || data.Payload == null) return Results.BadRequest();

        // THE LOGIC BOMB
        if (data.Payload.Length == 42)
        {
            throw new InvalidOperationException("CRITICAL MEMORY FAILURE: Buffer Edge Case Hit!");
        }

        return Results.Ok($"Processed: {data.Payload}");
    }
   catch (Exception ex)
    {
        // MAKE IT SCREAM IN THE CONSOLE FOR THE SCREENSHOT
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n[!!!] CRITICAL EXCEPTION CAUGHT: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
        Console.ResetColor();

        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
});

app.Run("http://localhost:5000");

public class UserInput
{
    public string? Payload { get; set; }
}
