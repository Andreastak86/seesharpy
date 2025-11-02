using Microsoft.Data.Sqlite;

public static class Database
{
    public static void EnsureCreated()
    {
        using var connection = new SqliteConnection("Data Source=data.db");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        CREATE TABLE IF NOT EXISTS Messages (
            Id TEXT PRIMARY KEY,
            Text TEXT NOT NULL,
            CreatedAt TEXT NOT NULL
        );";
        command.ExecuteNonQuery();
    }
}
