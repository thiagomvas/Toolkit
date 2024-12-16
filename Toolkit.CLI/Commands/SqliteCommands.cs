using Microsoft.Data.Sqlite;
using SharpTables;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolkit.CLI.Commands;
internal class SqliteCommands : BaseCommand
{
    public SqliteCommands() : base("sqlite", "Browse and Query SQLite Databases")
    {
    }

    public override void Setup(RootCommand root)
    {
        AddCommand(new BrowseCommand());
        AddCommand(new QueryCommand());
        AddCommand(new SchemaCommand());
        root.AddCommand(this);
    }

    internal class BrowseCommand : Command
    {
        public BrowseCommand() : base("browse", "Browse a table in an SQLite database")
        {
            var dbPathArg = new Argument<string>("dbPath", "The path to the SQLite database file");
            var tableNameArg = new Argument<string?>("tableName", "The name of the table to browse")
            {
                Arity = ArgumentArity.ZeroOrOne // Make tableName optional
            };

            AddArgument(dbPathArg);
            AddArgument(tableNameArg);

            this.SetHandler(Execute, dbPathArg, tableNameArg);
        }

        public void Execute(string dbPath, string? tableName)
        {
            // Ensure dbPath is absolute
            if (!Path.IsPathRooted(dbPath))
            {
                dbPath = Path.GetFullPath(dbPath);
            }

            if (!File.Exists(dbPath))
            {
                Logger.LogError($"Database file not found: {dbPath}");
                return;
            }

            try
            {
                using var connection = new SqliteConnection($"Data Source={dbPath}");
                connection.Open();

                if (string.IsNullOrWhiteSpace(tableName))
                {
                    // List all available tables
                    using var command = connection.CreateCommand();
                    command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name;";

                    using var reader = command.ExecuteReader();
                    var tables = new List<string>();

                    while (reader.Read())
                    {
                        tables.Add(reader.GetString(0)); // Get table name
                    }

                    if (tables.Any())
                    {
                        Logger.LogInformation("Available tables:");
                        foreach (var t in tables)
                        {
                            Logger.LogInformation($"- {t}");
                        }
                    }
                    else
                    {
                        Logger.LogWarning("No tables found in the database.");
                    }

                    return;
                }

                // Query the specified table
                var lines = new List<List<string>>();
                using var browseCommand = connection.CreateCommand();
                browseCommand.CommandText = $"SELECT * FROM \"{tableName}\"";

                using var readerBrowse = browseCommand.ExecuteReader();
                var schema = readerBrowse.GetColumnSchema();
                var columns = schema.Select(c => c.ColumnName).ToList();

                // Log metadata
                Logger.LogInformation($"Querying {dbPath}...");
                Logger.LogInformation($"Table: {tableName}");
                Logger.LogInformation($"Columns: {string.Join(", ", columns)}");
                var table = new Table();

                // Read rows
                while (readerBrowse.Read())
                {
                    var values = new List<string>();
                    for (int i = 0; i < readerBrowse.FieldCount; i++)
                    {
                        values.Add(readerBrowse.IsDBNull(i) ? "NULL" : readerBrowse[i]?.ToString() ?? string.Empty);
                    }
                    lines.Add(values);
                }

                Logger.LogSuccess($"Total rows: {lines.Count}");
                PrintToTable(lines, columns);
            }
            catch (Exception ex)
            {
                Logger.LogError($"An error occurred while querying the database: {ex.Message}");
            }
        }
    }

    internal class QueryCommand : Command
    {
        public QueryCommand() : base("query", "Run a custom SQL query on an SQLite database")
        {
            var dbPathArg = new Argument<string>("dbPath", "The path to the SQLite database file");
            var sqlQueryArg = new Argument<string>("sqlQuery", "The SQL query to execute");

            AddArgument(dbPathArg);
            AddArgument(sqlQueryArg);

            this.SetHandler(Execute, dbPathArg, sqlQueryArg);
        }

        public void Execute(string dbPath, string sqlQuery)
        {
            // Ensure dbPath is absolute
            if (!Path.IsPathRooted(dbPath))
            {
                dbPath = Path.GetFullPath(dbPath);
            }

            if (!File.Exists(dbPath))
            {
                Logger.LogError($"Database file not found: {dbPath}");
                return;
            }

            try
            {
                using var connection = new SqliteConnection($"Data Source={dbPath}");
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = sqlQuery;

                using var reader = command.ExecuteReader();
                var schema = reader.GetColumnSchema();
                var columns = schema.Select(c => c.ColumnName).ToList();

                var lines = new List<List<string>>();

                // Log metadata
                Logger.LogInformation($"Executing query on {dbPath}...");
                Logger.LogInformation($"Query: {sqlQuery}");
                Logger.LogInformation($"Columns: {string.Join(", ", columns)}");

                var table = new Table().SetHeader(new Row(columns));

                // Read rows
                while (reader.Read())
                {
                    var values = new List<string>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        values.Add(reader.IsDBNull(i) ? "NULL" : reader[i]?.ToString() ?? string.Empty);
                    }
                    lines.Add(values);
                }

                Logger.LogSuccess($"Total rows: {lines.Count}");
                PrintToTable(lines, columns);

            }
            catch (Exception ex)
            {
                Logger.LogError($"An error occurred while executing the query: {ex.Message}");
            }
        }
    }

    internal class SchemaCommand : Command
    {
        public SchemaCommand() : base("schema", "Display the schema of an SQLite database")
        {
            // Argument for the database path
            var dbPathArg = new Argument<string>("dbPath", "The path to the SQLite database file");
            AddArgument(dbPathArg);

            // Argument for the optional table name (can be null or empty)
            var tableArg = new Argument<string?>("tableName", "The name of the table to show the schema for (optional)") { Arity = ArgumentArity.ZeroOrOne };
            AddArgument(tableArg);

            this.SetHandler(Execute, dbPathArg, tableArg);
        }

        public void Execute(string dbPath, string? tableName)
        {
            // Ensure dbPath is absolute
            if (!Path.IsPathRooted(dbPath))
            {
                dbPath = Path.GetFullPath(dbPath);
            }

            if (!File.Exists(dbPath))
            {
                Logger.LogError($"Database file not found: {dbPath}");
                return;
            }

            try
            {
                using var connection = new SqliteConnection($"Data Source={dbPath}");
                connection.Open();

                // If a specific table name is provided, show the SQL schema for that table
                if (!string.IsNullOrEmpty(tableName))
                {
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT sql FROM sqlite_master WHERE type='table' AND name=@tableName;";
                    command.Parameters.AddWithValue("@tableName", tableName);
                    using var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        var tableSql = reader.GetString(0);
                        Logger.LogInformation($"SQL for table {tableName}:");
                        Logger.LogInformation(tableSql);
                    }
                    else
                    {
                        Logger.LogError($"Table '{tableName}' not found in the database.");
                    }
                }
                else
                {
                    // Otherwise, show SQL schema for all tables
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT name, sql FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name;";
                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var tableNameInDb = reader.GetString(0);
                        var tableSql = reader.GetString(1);
                        Logger.LogInformation($"SQL for table {tableNameInDb}:");
                        Logger.LogInformation(tableSql);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"An error occurred while querying the database: {ex.Message}");
            }
        }
    }



    private static void PrintToTable(List<List<string>> lines, List<string> header)
    {
        var table = new Table();
        foreach(var row in lines)
        {
            table.AddRow(new Row(row));
        }
        table.SetHeader(new Row(header))
                .UseSettings(new TableSettings()
                {
                    DisplayRowIndexes = true,
                    RowIndexColor = ConsoleColor.DarkGray,
                    CellPreset = c =>
                    {
                        if (c.IsNull || c.Text.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                        {
                            c.Color = ConsoleColor.Blue;
                        }
                        else if (c.IsNumeric && c.Position.X > 0)
                        {
                            c.Color = ConsoleColor.Yellow;
                        }
                    },
                })
                  .Write();
    }
}
