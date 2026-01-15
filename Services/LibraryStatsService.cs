using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SigmaLib.Models;

namespace SigmaLib.Services
{
    public class LibraryStatsService
    {
        private readonly DatabaseService _db;

        public LibraryStatsService()
        {
            _db = new DatabaseService();
        }

        public int LoadReservationsCount()
        {
            using var conn = _db.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
        SELECT COUNT(*) 
        FROM Reservations
        WHERE Status NOT IN ('Returned', 'Rejected')";

            object? result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public int LoadBooksCount()
        {
            using var conn = _db.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT COUNT(*) FROM Books";

            object? result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public int LoadActiveUsersCount()
        {
            using var conn = _db.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT COUNT(*) FROM Users
                WHERE IsBlocked <> 1";

            object? result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }
        public decimal LoadTotalFinesSum()
        {
            using var conn = _db.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT SUM(TotalFine) FROM Users";

            object? result = cmd.ExecuteScalar();
            decimal sum = result != null ? Convert.ToDecimal(result) : 0m;

            return Math.Round(sum, 2);
        }
    }
}
