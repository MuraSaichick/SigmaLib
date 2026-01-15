using Microsoft.Data.Sqlite;
using SigmaLib.Models;
using SigmaLib.Interfaces;
using System;

namespace SigmaLib.Services
{
    public class AuthService
    {
        private readonly IDatabaseService _db;

        public AuthService()
        {
            _db = new DatabaseService();
        }
        public (User? user, string? error) LoginUser(string login, string password)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
        SELECT Id, Email, Password, FirstName, LastName, SurName, Phone, Address, Role, IsBlocked, TotalFine
        FROM Users
        WHERE Email = $login OR Phone = $login";

            cmd.Parameters.AddWithValue("$login", login);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string storedHash = reader.GetString(reader.GetOrdinal("Password"));

                if (!BCrypt.Net.BCrypt.Verify(password, storedHash))
                {
                    return (null, "Неверный логин или пароль");
                }

                var user = new User
                {
                    Id = reader.GetInt32(0),
                    Email = reader.GetString(1),
                    Password = storedHash,
                    FirstName = reader.GetString(3),
                    LastName = reader.GetString(4),
                    SurName = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Phone = reader.IsDBNull(6) ? null : reader.GetString(6),
                    Address = reader.IsDBNull(7) ? null : reader.GetString(7),
                    Role = Enum.Parse<UserRole>(reader.GetString(8)),
                    IsBlocked = reader.GetBoolean(9),
                    Dept = reader.GetInt32(10)
                };

                if (user.IsBlocked)
                {
                    return (null, "Аккаунт заблокирован");
                }

                return (user, null);
            }

            return (null, "Пользователь не найден");
        }


        public string RegistrationUser(string email, string password,
    string firstName,
    string lastName,
    string surName,
    string phone,
    string address)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            using (var checkEmailCmd = conn.CreateCommand())
            {
                checkEmailCmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
                checkEmailCmd.Parameters.AddWithValue("@Email", email);

                var emailExists = (long)checkEmailCmd.ExecuteScalar();
                if (emailExists > 0)
                {
                    return "Пользователь с таким email уже существует.";
                }
            }

            using (var checkPhoneCmd = conn.CreateCommand())
            {
                checkPhoneCmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Phone = @Phone";
                checkPhoneCmd.Parameters.AddWithValue("@Phone", phone);

                var phoneExists = (long)checkPhoneCmd.ExecuteScalar();
                if (phoneExists > 0)
                {
                    return "Пользователь с таким телефоном уже существует.";
                }
            }

            // Добавление нового пользователя
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            using var insertCmd = conn.CreateCommand();
            insertCmd.CommandText = @"
        INSERT INTO Users (Email, Password, FirstName, LastName, SurName, Phone, Address, Role, IsBlocked)
        VALUES (@Email, @Password, @FirstName, @LastName, @SurName, @Phone, @Address, @Role, @IsBlocked)";

            insertCmd.Parameters.AddWithValue("@Email", email);
            insertCmd.Parameters.AddWithValue("@Password", hashedPassword);
            insertCmd.Parameters.AddWithValue("@FirstName", firstName);
            insertCmd.Parameters.AddWithValue("@LastName", lastName);
            insertCmd.Parameters.AddWithValue("@SurName", surName);
            insertCmd.Parameters.AddWithValue("@Phone", phone);
            insertCmd.Parameters.AddWithValue("@Address", address);
            insertCmd.Parameters.AddWithValue("@Role", "Reader");
            insertCmd.Parameters.AddWithValue("@IsBlocked", false);

            insertCmd.ExecuteNonQuery();

            return "Вы зарегистрировались"; // пустая строка = ошибок нет
        }
    }
}
