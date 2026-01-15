using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Data.Sqlite;
using SigmaLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;

namespace SigmaLib.Services
{
    public class UserService
    {
        private readonly DatabaseService _db;

        public UserService()
        {
            _db = new DatabaseService();
        }

        public OperationResult BlockUser(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = "UPDATE Users SET IsBlocked = 1 WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);

            int affected = cmd.ExecuteNonQuery();
            return new OperationResult
            {
                Success = affected > 0,
                Message = affected > 0 ? "Пользователь заблокирован." : "Пользователь не найден."
            };
        }

        public OperationResult UnblockUser(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = "UPDATE Users SET IsBlocked = 0 WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);

            int affected = cmd.ExecuteNonQuery();
            return new OperationResult
            {
                Success = affected > 0,
                Message = affected > 0 ? "Пользователь разблокирован." : "Пользователь не найден."
            };
        }

        public OperationResult DeleteUser(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            using var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = @"SELECT COUNT(*) FROM Reservations
            WHERE Status = 'Accepted' AND (ReaderId = @UserId OR RespondedByLibrarianId = @UserId);";
            checkCmd.Parameters.AddWithValue("@UserId", id);

            var count = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (count > 0)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "Нельзя удалить пользователя с активными бронированиями"
                };
            }
            using var cmd = conn.CreateCommand();

            cmd.CommandText = "DELETE FROM Users WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);

            int affected = cmd.ExecuteNonQuery();
            return new OperationResult
            {
                Success = affected > 0,
                Message = affected > 0 ? "Пользователь удалён." : "Пользователь не найден."
            };
        }

        public OperationResult CreateUser(User user)
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();
                using var cmd = conn.CreateCommand();
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
                cmd.CommandText = @"
            INSERT INTO Users (Email, Password, FirstName, LastName, SurName, Phone, Address, Role, IsBlocked)
            VALUES ($email, $password, $firstName, $lastName, $surName, $phone, $address, $role, $isBlocked)";

                cmd.Parameters.AddWithValue("$email", user.Email);
                cmd.Parameters.AddWithValue("$password", hashedPassword);
                cmd.Parameters.AddWithValue("$firstName", user.FirstName);
                cmd.Parameters.AddWithValue("$lastName", user.LastName);
                cmd.Parameters.AddWithValue("$surName", user.SurName);
                cmd.Parameters.AddWithValue("$phone", user.Phone);
                cmd.Parameters.AddWithValue("$address", user.Address);
                cmd.Parameters.AddWithValue("$role", user.Role.ToString());
                cmd.Parameters.AddWithValue("$isBlocked", user.IsBlocked);

                int affected = cmd.ExecuteNonQuery();

                return new OperationResult
                {
                    Success = affected > 0,
                    Message = affected > 0 ? "Пользователь создан." : "Не удалось создать пользователя."
                };
            }
            catch (SqliteException ex)
            {
                if (ex.SqliteErrorCode == 19)
                {
                    if (ex.Message.Contains("Email"))
                        return new OperationResult { Success = false, Message = "Указанный email уже используется." };
                    if (ex.Message.Contains("Phone"))
                        return new OperationResult { Success = false, Message = "Указанный телефон уже используется." };
                }

                return new OperationResult { Success = false, Message = "Ошибка базы данных: " + ex.Message };
            }
            catch (Exception ex)
            {
                return new OperationResult { Success = false, Message = "Неожиданная ошибка: " + ex.Message };
            }
        }

        public OperationResult EditUser(User currentUser, User changedUser)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            // Проверка: изменился ли Email
            if (!string.Equals(currentUser.Email, changedUser.Email, StringComparison.OrdinalIgnoreCase))
            {
                using var checkEmailCmd = conn.CreateCommand();
                checkEmailCmd.CommandText = "SELECT COUNT(1) FROM Users WHERE Email = $email AND Id <> $id";
                checkEmailCmd.Parameters.AddWithValue("$email", changedUser.Email);
                checkEmailCmd.Parameters.AddWithValue("$id", changedUser.Id);

                long emailExists = (long)checkEmailCmd.ExecuteScalar();
                if (emailExists > 0)
                {
                    return new OperationResult
                    {
                        Success = false,
                        Message = "Указанный email уже используется другим пользователем."
                    };
                }
            }

            // Проверка: изменился ли Phone
            if (!string.Equals(currentUser.Phone, changedUser.Phone, StringComparison.OrdinalIgnoreCase))
            {
                using var checkPhoneCmd = conn.CreateCommand();
                checkPhoneCmd.CommandText = "SELECT COUNT(1) FROM Users WHERE Phone = $phone AND Id <> $id";
                checkPhoneCmd.Parameters.AddWithValue("$phone", changedUser.Phone);
                checkPhoneCmd.Parameters.AddWithValue("$id", changedUser.Id);

                long phoneExists = (long)checkPhoneCmd.ExecuteScalar();
                if (phoneExists > 0)
                {
                    return new OperationResult
                    {
                        Success = false,
                        Message = "Указанный телефон уже используется другим пользователем."
                    };
                }
            }

            using var cmd = conn.CreateCommand();

            // Базовый SQL без пароля
            cmd.CommandText = @"UPDATE Users SET 
            Email = $email,
            FirstName = $firstName,
            LastName = $lastName,
            SurName = $surName,
            Phone = $phone,
            Address = $address,
            Role = $role,
            IsBlocked = $isBlocked
            WHERE Id = $id";

            // Общие параметры
            cmd.Parameters.AddWithValue("$email", changedUser.Email);
            cmd.Parameters.AddWithValue("$firstName", changedUser.FirstName);
            cmd.Parameters.AddWithValue("$lastName", changedUser.LastName);
            cmd.Parameters.AddWithValue("$surName", changedUser.SurName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$phone", changedUser.Phone ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$address", changedUser.Address ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$role", changedUser.Role.ToString());
            cmd.Parameters.AddWithValue("$isBlocked", changedUser.IsBlocked);
            cmd.Parameters.AddWithValue("$id", changedUser.Id);

            // Если пароль указан
            if (!string.IsNullOrWhiteSpace(changedUser.Password))
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(changedUser.Password);

                cmd.CommandText = @"UPDATE Users SET 
                Email = $email,
                Password = $password,
                FirstName = $firstName,
                LastName = $lastName,
                SurName = $surName,
                Phone = $phone,
                Address = $address,
                Role = $role,
                IsBlocked = $isBlocked
                WHERE Id = $id";

                cmd.Parameters.AddWithValue("$password", hashedPassword);
            }

            int affected = cmd.ExecuteNonQuery();

            return new OperationResult
            {
                Success = affected > 0,
                Message = affected > 0 ? "Пользователь обновлён." : "Ошибка при обновлении пользователя."
            };
        }
    }
}
