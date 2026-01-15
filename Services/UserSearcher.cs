using DocumentFormat.OpenXml.Office2010.CustomUI;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.Data.Sqlite;
using SigmaLib.Interfaces;
using SigmaLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigmaLib.Services
{
    public class UserSearcher
    {
        private readonly IDatabaseService _db;

        public UserSearcher()
        {
            _db = new DatabaseService();
        }

        public List<User> GetAllUsers()
        {
            List<User> users = new List<User>();

            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
            SELECT Id, Email, Password, FirstName, LastName, SurName, Phone, Address, Role, IsBlocked, TotalFine
            FROM Users";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                users.Add(MapUser(reader));
            }

            return users;
        }

        public User SearchById(int? id)
        {
            User user = new User();
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = $@"
            SELECT Id, Email, Password, FirstName, LastName, SurName, Phone, Address, Role, IsBlocked, TotalFine
            FROM Users
            WHERE Id = $value";

            cmd.Parameters.AddWithValue("$value", id);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                user = (MapUser(reader));
            }
            return user;
        }

        private User MapUser(System.Data.IDataRecord reader)
        {
            return new User
            {
                Id = reader.GetInt32(0),
                Email = reader.GetString(1),
                Password = reader.GetString(2),
                FirstName = reader.GetString(3),
                LastName = reader.GetString(4),
                SurName = reader.GetString(5),
                Phone = reader.GetString(6),
                Address = reader.GetString(7),
                Role = Enum.TryParse<UserRole>(reader.GetString(8), out var role)
                ? role : UserRole.Reader,
                IsBlocked = reader.GetBoolean(9),
                Dept = reader.GetDecimal(10)
            };
        }
        public List<User> FilterUsers(User userTemplate, string role, string blockedStatus, string userId)
        {
            List<User> users = GetAllUsers();

            var filtered = users.Where(u =>
    (string.IsNullOrEmpty(userId) || u.Id == int.Parse(userId)) &&
    (string.IsNullOrEmpty(userTemplate.FirstName) || u.FirstName.Contains(userTemplate.FirstName, StringComparison.OrdinalIgnoreCase)) &&
    (string.IsNullOrEmpty(userTemplate.LastName) || u.LastName.Contains(userTemplate.LastName, StringComparison.OrdinalIgnoreCase)) &&
    (string.IsNullOrEmpty(userTemplate.SurName) || u.SurName.Contains(userTemplate.SurName, StringComparison.OrdinalIgnoreCase)) &&
    (string.IsNullOrEmpty(userTemplate.Email) || u.Email.Contains(userTemplate.Email, StringComparison.OrdinalIgnoreCase)) &&
    (string.IsNullOrEmpty(userTemplate.Phone) || u.Phone.Contains(userTemplate.Phone, StringComparison.OrdinalIgnoreCase)) &&
    (string.IsNullOrEmpty(userTemplate.Address) || u.Address.Contains(userTemplate.Address, StringComparison.OrdinalIgnoreCase)) &&
    (role == "All" || u.Role == Enum.Parse<UserRole>(role, true)) &&
    (
        blockedStatus == "All" ||
        (blockedStatus == "Active" && !u.IsBlocked) ||
        (blockedStatus != "Active" && u.IsBlocked)
    )
).ToList();
            return filtered;
        }
        public List<User> SearchReaders(User userTemplate, string userId, string isDept)
        {
            List<User> users = GetAllUsers();

            var filtered = users.Where(u =>
    (string.IsNullOrEmpty(userId) || u.Id == int.Parse(userId)) &&
    (string.IsNullOrEmpty(userTemplate.FirstName) || u.FirstName.Contains(userTemplate.FirstName, StringComparison.OrdinalIgnoreCase)) &&
    (string.IsNullOrEmpty(userTemplate.LastName) || u.LastName.Contains(userTemplate.LastName, StringComparison.OrdinalIgnoreCase)) &&
    (string.IsNullOrEmpty(userTemplate.SurName) || u.SurName.Contains(userTemplate.SurName, StringComparison.OrdinalIgnoreCase)) &&
    (string.IsNullOrEmpty(userTemplate.Email) || u.Email.Contains(userTemplate.Email, StringComparison.OrdinalIgnoreCase)) &&
    (string.IsNullOrEmpty(userTemplate.Phone) || u.Phone.Contains(userTemplate.Phone, StringComparison.OrdinalIgnoreCase)) &&
    (string.IsNullOrEmpty(userTemplate.Address) || u.Address.Contains(userTemplate.Address, StringComparison.OrdinalIgnoreCase)) &&
    (u.Role == Enum.Parse<UserRole>("Reader", true)) &&
    (isDept == "All" || (isDept == "WithDebt" && u.Dept > 0) || (isDept == "WithoutDebt" && u.Dept == 0))
    ).ToList();
            return filtered;
        }

        public void PayFineReader(User user)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"UPDATE Users SET TotalFine = 0 WHERE Id = $id";

            cmd.Parameters.AddWithValue("$id", user.Id);
            cmd.ExecuteNonQuery();
        }
    }
}