using Microsoft.Data.Sqlite;
using SigmaLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;
    private readonly SqliteConnection _connection;

    public DatabaseService()
    {
        _connectionString = $"Data Source=Assets/Database/Library.db;";
    }
    public SqliteConnection GetConnection()
    {
        return new SqliteConnection(_connectionString);
    }

    public void ExecuteNonQuery(string sql)
    {
        using var conn = GetConnection();
        conn.Open(); 
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    public object ExecuteScalar(string sql)
    {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        return cmd.ExecuteScalar();
    }
}

