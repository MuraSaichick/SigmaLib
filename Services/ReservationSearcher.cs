using Microsoft.Data.Sqlite;
using SigmaLib.Interfaces;
using SigmaLib.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigmaLib.Services
{
    public class ReservationSearcher : IReservationQueryService
    {
        private readonly DatabaseService _db;
        public ReservationSearcher()
        {
            _db = new DatabaseService();
        }

        public List<Reservation> GetAllReservation()
        {
            List <Reservation> reservations = new List<Reservation>();
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"
            SELECT Id, CreatedAt, PickUpDate, ReturnDate, Status, Fine, Reason, BookCopyId,RespondedByLibrarianId, ReaderId, ResponseDate, ActualReturnDate
            FROM Reservations";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Reservation reservation = MapReservation(reader);
                reservation.IdBook = GetBookIdByCopyId(reservation.BookCopyId);
                reservation.NameBook = GetBookTitleById(reservation.IdBook);
                reservation.DefaultLocation = GetBookCopyLocationByCopyId(reservation.BookCopyId);
                reservations.Add(reservation);
            }
            return reservations;
        }
        private Reservation MapReservation(Microsoft.Data.Sqlite.SqliteDataReader reader)
        {
            Reservation reservation = new Reservation()
            {
                Id = reader.GetInt32(0),
                CreateAt = DateTime.ParseExact(reader.GetString(1), "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                PickUpAt = DateTime.ParseExact(reader.GetString(2), "dd-MM-yyyy", CultureInfo.InvariantCulture),
                ReturnDate = DateTime.ParseExact(reader.GetString(3), "dd-MM-yyyy", CultureInfo.InvariantCulture),

                Status = reader.GetString(4),
                Fine = reader.GetDecimal(5),
                Reason = reader.IsDBNull(6) ? null : reader.GetString(6),
                BookCopyId = reader.GetInt32(7),
                RespondedByLibrarianId = reader.IsDBNull(8) ? (int?)null : reader.GetInt32(8),
                ReaderId = reader.GetInt32(9),
                ResponseAt = reader.IsDBNull(10)
                ? (DateTime?)null
                : DateTime.ParseExact(reader.GetString(10), "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                ActualReturnDate = reader.IsDBNull(11)
                ? (DateTime?)null
                : DateTime.ParseExact(reader.GetString(11), "dd-MM-yyyy", CultureInfo.InvariantCulture),
            };
            return reservation;
        }
        public List<Reservation> GetReservationByFilter(Reservation reservationTemplate)
        {
            List<Reservation> reservations = GetAllReservation();
            // фильтрация по ReaderId
            if (reservationTemplate.ReaderId.HasValue)
            {
                reservations = reservations
                    .Where(r => r.ReaderId == reservationTemplate.ReaderId.Value)
                    .ToList();
            }
            // фильтрация по CreateAt
            if (reservationTemplate.CreateAt.HasValue)
            {
                reservations = reservations
                    .Where(r => r.CreateAt.HasValue
                                && r.CreateAt.Value.Year == reservationTemplate.CreateAt.Value.Year
                                && r.CreateAt.Value.Month == reservationTemplate.CreateAt.Value.Month
                                && r.CreateAt.Value.Day == reservationTemplate.CreateAt.Value.Day)
                    .ToList();
            }
            // фильтрация по PickUpAt
            if (reservationTemplate.PickUpAt.HasValue)
            {
                reservations = reservations
                    .Where(r => r.PickUpAt.HasValue && r.PickUpAt.Value == reservationTemplate.PickUpAt.Value)
                    .ToList();
            }
            // фильтрация по ReturnDate
            if (reservationTemplate.ReturnDate.HasValue)
            {
                reservations = reservations
                    .Where(r => r.ReturnDate.HasValue && r.ReturnDate.Value == reservationTemplate.ReturnDate.Value)
                    .ToList();
            }
            // фильтрация по части названия книги
            if (!string.IsNullOrWhiteSpace(reservationTemplate.NameBook))
            {
                reservations = reservations
                    .Where(r => GetTitleById(r.BookCopyId)
                        .Contains(reservationTemplate.NameBook, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            // фильр по Status
            if(reservationTemplate.Status == "Все")
            {
                return reservations;
            } else if (reservationTemplate.Status == "Просроченные")
            {
                reservations = reservations
                    .Where(r => r.ReturnDate.HasValue
                    && (DateTime.Now.Date - r.ReturnDate.Value.Date).TotalDays >= 1).ToList();
            }
            else
            {
                reservations = reservations
                    .Where(r => r.Status == Reservation.StatusTranslationsReverse[reservationTemplate.Status]).ToList();
            }
                return reservations;
        }
        public string GetTitleById(int bookCopyId)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT IdBook FROM BookCopies WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", bookCopyId);

            int bookId = Convert.ToInt32(cmd.ExecuteScalar());

            // 2. Получаем Title по Id книги
            using var cmd2 = conn.CreateCommand();
            cmd2.CommandText = "SELECT Title FROM Books WHERE Id = $id";
            cmd2.Parameters.AddWithValue("$id", bookId);

            return cmd2.ExecuteScalar().ToString();
        }

        public List<Reservation> GetReservationByReaderId(int ReaderId)
        {
            List<Reservation> reservations = new List<Reservation>();
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT Id, CreatedAt, PickUpDate, ReturnDate, Status, Fine, Reason, BookCopyId,RespondedByLibrarianId, ReaderId, ResponseDate, ActualReturnDate
            FROM Reservations WHERE ReaderId = $readerId";
            cmd.Parameters.AddWithValue("$readerId", ReaderId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                reservations.Add(MapReservation(reader));
            }
            return reservations;
        }
        public List<Reservation> GetReservationByLibrarianId(int librarianId)
        {
            List<Reservation> reservations = new List<Reservation>();
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT Id, CreatedAt, PickUpDate, ReturnDate, Status, Fine, Reason, BookCopyId,RespondedByLibrarianId, ReaderId, ResponseDate, ActualReturnDate
            FROM Reservations WHERE RespondedByLibrarianId = $librarianId";
            cmd.Parameters.AddWithValue("librarianId", librarianId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                reservations.Add(MapReservation(reader));
            }
            return reservations;
        }
        private int GetBookIdByCopyId(int bookCopyId)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT IdBook FROM BookCopies WHERE Id = $copyId";
            cmd.Parameters.AddWithValue("$copyId", bookCopyId);
            var result = cmd.ExecuteScalar();
            return Convert.ToInt32(result);
        }
        private string GetBookTitleById(int bookId)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Title FROM Books WHERE Id = $bookId";
            cmd.Parameters.AddWithValue("$bookId", bookId);

            var result = cmd.ExecuteScalar();
            return result?.ToString();
        }
        private string? GetBookCopyLocationByCopyId(int bookCopyId)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Location FROM BookCopies WHERE Id = $copyId";
            cmd.Parameters.AddWithValue("$copyId", bookCopyId);

            var result = cmd.ExecuteScalar();
            return result?.ToString();
        }
    }
}