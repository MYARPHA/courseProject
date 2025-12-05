using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;
using System.Collections.Generic;

// Minimal top-level program: find appsettings.json, read connection string, insert migration marker row
string[] candidates = new[] {
    Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "courseProject", "appsettings.json"),
    Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "courseProject", "appsettings.json"),
    Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "appsettings.json"),
    Path.Combine(Directory.GetCurrentDirectory(), "..", "courseProject", "appsettings.json"),
    Path.Combine(Directory.GetCurrentDirectory(), "courseProject", "appsettings.json")
};

string? found = candidates.FirstOrDefault(File.Exists);
IConfiguration config;

if (found == null)
{
    Console.WriteLine("Could not find appsettings.json. Set CONNECTION_STRING env var or place appsettings.json where expected.");
    var env = Environment.GetEnvironmentVariable("CONNECTION_STRING");
    if (string.IsNullOrEmpty(env)) Environment.Exit(1);
    var inMem = new Dictionary<string, string?> { { "ConnectionStrings:Default", env } };
    config = new ConfigurationBuilder().AddInMemoryCollection(inMem).Build();
}
else
{
    config = new ConfigurationBuilder()
        .SetBasePath(Path.GetDirectoryName(found)!)
        .AddJsonFile(Path.GetFileName(found), optional: false)
        .Build();
}

int code = RunWithConfig(config);
Environment.Exit(code);

static int RunWithConfig(IConfiguration config)
{
    var connStr = config.GetConnectionString("Default");
    if (string.IsNullOrEmpty(connStr))
    {
        Console.WriteLine("Connection string not found in appsettings.json");
        return 1;
    }

    var migrationId = "20251205002304_AddRequestsEntities";
    var productVersion = "8.0.21";

    using var conn = new MySqlConnection(connStr);
    conn.Open();

    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
  `MigrationId` varchar(150) NOT NULL,
  `ProductVersion` varchar(32) NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
        cmd.ExecuteNonQuery();
    }

    using (var check = conn.CreateCommand())
    {
        check.CommandText = "SELECT COUNT(1) FROM `__EFMigrationsHistory` WHERE `MigrationId` = @id";
        check.Parameters.AddWithValue("@id", migrationId);
        var exists = Convert.ToInt32(check.ExecuteScalar() ?? 0);
        if (exists > 0)
        {
            Console.WriteLine($"Migration '{migrationId}' is already recorded in __EFMigrationsHistory.");
            return 0;
        }
    }

    using (var ins = conn.CreateCommand())
    {
        ins.CommandText = "INSERT INTO `__EFMigrationsHistory` (`MigrationId`,`ProductVersion`) VALUES (@id,@pv)";
        ins.Parameters.AddWithValue("@id", migrationId);
        ins.Parameters.AddWithValue("@pv", productVersion);
        var rows = ins.ExecuteNonQuery();
        Console.WriteLine($"Inserted migration marker: {rows} row(s) affected.");
    }

    return 0;
}