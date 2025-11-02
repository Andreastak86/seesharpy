using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// CORS – la frontenden (samme host) snakke med APIet
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseCors();

// Server statiske filer fra wwwroot (index.html)
app.UseDefaultFiles();
app.UseStaticFiles();

// "Database": enkel JSON-fil på disk
const string DataDir = "data";
const string DataFile = $"{DataDir}/messages.json";
Directory.CreateDirectory(DataDir);

var options = new JsonSerializerOptions { WriteIndented = true };
var messages = File.Exists(DataFile)
    ? JsonSerializer.Deserialize<List<Message>>(File.ReadAllText(DataFile)) ?? new()
    : new List<Message>();

// Endepunkter
app.MapGet("/api/messages", () => Results.Ok(messages));

app.MapGet("/api/messages/{id:guid}", (Guid id) =>
{
    var m = messages.FirstOrDefault(x => x.Id == id);
    return m is null ? Results.NotFound() : Results.Ok(m);
});

app.MapPost("/api/messages", async (MessageInput input) =>
{
    if (string.IsNullOrWhiteSpace(input.Text))
        return Results.BadRequest(new { error = "Text can’t be empty." });

    var msg = new Message(Guid.NewGuid(), input.Text.Trim(), DateTime.UtcNow);
    messages.Add(msg);

    await File.WriteAllTextAsync(DataFile, JsonSerializer.Serialize(messages, options));
    return Results.Created($"/api/messages/{msg.Id}", msg);
});

app.Run();

// Små rekorder for dataflyt
record Message(Guid Id, string Text, DateTime CreatedAt);
record MessageInput(string Text);
