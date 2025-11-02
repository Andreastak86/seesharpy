using Microsoft.Data.Sqlite;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
var app = builder.Build();
app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

Database.EnsureCreated();

app.MapGet("/api/messages", () =>
{
    using var db = new SqliteConnection("Data Source=data.db");
    db.Open();

    var cmd = db.CreateCommand();
    cmd.CommandText = "SELECT Id, Text, CreatedAt FROM Messages ORDER BY CreatedAt DESC";

    var messages = new List<Message>();
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        messages.Add(new Message(
            Guid.Parse(reader.GetString(0)),
            reader.GetString(1),
            DateTime.Parse(reader.GetString(2))
        ));
    }

    return Results.Ok(messages);
});

app.MapPost("/api/messages", (MessageInput input) =>
{
    using var db = new SqliteConnection("Data Source=data.db");
    db.Open();

    var msg = new Message(Guid.NewGuid(), input.Text, DateTime.UtcNow);

    var cmd = db.CreateCommand();
    cmd.CommandText = "INSERT INTO Messages (Id, Text, CreatedAt) VALUES ($id, $text, $createdAt)";
    cmd.Parameters.AddWithValue("$id", msg.Id.ToString());
    cmd.Parameters.AddWithValue("$text", msg.Text);
    cmd.Parameters.AddWithValue("$createdAt", msg.CreatedAt.ToString("o"));
    cmd.ExecuteNonQuery();

    return Results.Created($"/api/messages/{msg.Id}", msg);
});


app.MapGet("/api/debug/sql", () =>
{
    using var db = new SqliteConnection("Data Source=data.db");
    db.Open();

    var countCmd = db.CreateCommand();
    countCmd.CommandText = "SELECT COUNT(*) FROM Messages;";
    var count = Convert.ToInt32(countCmd.ExecuteScalar());

    var listCmd = db.CreateCommand();
    listCmd.CommandText = "SELECT Id, Text, CreatedAt FROM Messages ORDER BY CreatedAt DESC LIMIT 5;";
    var items = new List<object>();
    using (var r = listCmd.ExecuteReader())
    {
        while (r.Read())
        {
            items.Add(new {
                Id = r.GetString(0),
                Text = r.GetString(1),
                CreatedAt = r.GetString(2)
            });
        }
    }

    return Results.Ok(new { count, recent = items });
});


app.Run();

record Message(Guid Id, string Text, DateTime CreatedAt);
record MessageInput(string Text);

