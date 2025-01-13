using Microsoft.Extensions.Configuration;
using Microsoft.Data.Sqlite;
using Dapper;
using SQLitePCL;

namespace EmojiHelper;

public class SqliteConfigurationSource(string connectionString) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new SqliteConfigurationProvider(connectionString);

    public class SqliteConfigurationProvider(string connectionString) : ConfigurationProvider
    {
        static SqliteConfigurationProvider()
        {
            // Initialize SQLite provider
            Batteries.Init();
        }

        public override void Load()
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            // Ensure the Configuration table exists
            EnsureTableExists(connection);

            var data = connection.Query<KeyValuePair<string, string>>("SELECT Key, Value FROM Configuration");

            foreach (var kvp in data)
            {
                Data[kvp.Key] = kvp.Value;
            }
        }

        private static void EnsureTableExists(SqliteConnection connection)
        {
            var tableExists = connection.ExecuteScalar<bool>(
                "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Configuration';");

            if (!tableExists)
            {
                connection.Execute(
                    "CREATE TABLE Configuration (Key TEXT PRIMARY KEY, Value TEXT);");
            }
        }

        public override void Set(string key, string? value)
        {
            base.Set(key, value);

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var existing = connection.ExecuteScalar<bool>(
                "SELECT COUNT(*) FROM Configuration WHERE Key = @Key;", new { Key = key });

            if (existing)
            {
                connection.Execute(
                    "UPDATE Configuration SET Value = @Value WHERE Key = @Key;", new { Key = key, Value = value });
            }
            else
            {
                connection.Execute(
                    "INSERT INTO Configuration (Key, Value) VALUES (@Key, @Value);", new { Key = key, Value = value });
            }

            OnReload();
        }
    }
}

public static class SqliteConfigurationExtensions
{
    public static IConfigurationBuilder AddSqliteConfiguration(this IConfigurationBuilder builder, string connectionString)
    {
        return builder.Add(new SqliteConfigurationSource(connectionString));
    }
}
