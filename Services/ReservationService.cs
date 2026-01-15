using Microsoft.Data.Sqlite;
using SigmaLib.Interfaces;
using SigmaLib.Models;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;


namespace SigmaLib.Services
{
    public class ReservationService : IReservationManagementService
    {
        private readonly DatabaseService _db;
        private readonly WordDocumentService _wordDocumentService;

        public ReservationService()
        {
            _db = new DatabaseService();
            _wordDocumentService = new WordDocumentService();
        }
        public bool CreateReservation(int bookId, int userId, DateTime pickupDate, DateTime returnDate, decimal fine)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using (var transaction = conn.BeginTransaction())
            {
                var selectCmd = conn.CreateCommand();
                selectCmd.Transaction = transaction;
                selectCmd.CommandText = @"
                    SELECT Id FROM BookCopies
                    WHERE IdBook = $bookId AND Status = 'Available'
                    LIMIT 1;
                ";
                selectCmd.Parameters.AddWithValue("$bookId", bookId);

                var bookCopyId = selectCmd.ExecuteScalar() as long?;

                if (bookCopyId == null)
                {
                    return false;
                }

                var updateBookAvailableCopies = conn.CreateCommand();
                updateBookAvailableCopies.CommandText = @"UPDATE Books SET AvailableCopies = AvailableCopies - 1 WHERE Id = $bookId;";
                updateBookAvailableCopies.Parameters.AddWithValue("$bookId", bookId);
                updateBookAvailableCopies.ExecuteNonQuery();

                var updateCmd = conn.CreateCommand();
                updateCmd.Transaction = transaction;
                updateCmd.CommandText = @"
                    UPDATE BookCopies
                    SET Status = 'Reserved'
                    WHERE Id = $id;";

                updateCmd.Parameters.AddWithValue("$id", bookCopyId.Value);
                updateCmd.ExecuteNonQuery();

                var insertCmd = conn.CreateCommand();
                insertCmd.CommandText = @"
                    INSERT INTO Reservations (ReaderId, BookCopyId, CreatedAt, PickupDate, ReturnDate, Status, Fine)
                    VALUES ($readerId, $bookCopyId, $createdAt, $pickupDate, $returnDate, 'Pending', @Fine);";
                insertCmd.Parameters.AddWithValue("$readerId", userId);
                insertCmd.Parameters.AddWithValue("$bookCopyId", bookCopyId.Value);
                insertCmd.Parameters.AddWithValue("$createdAt", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
                insertCmd.Parameters.AddWithValue("$pickupDate", pickupDate.ToString("dd-MM-yyyy"));
                insertCmd.Parameters.AddWithValue("$returnDate", returnDate.ToString("dd-MM-yyyy"));
                insertCmd.Parameters.AddWithValue("@Fine", fine);

                insertCmd.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
        }

        // Я не знаю, оставил для чего-то
        //public bool CreateReservation(Reservation reservation)
        //{
        //    using var conn = _db.GetConnection();
        //    conn.Open();
        //    using (var transaction = conn.BeginTransaction())
        //    {
        //        var selectCmd = conn.CreateCommand();
        //        selectCmd.Transaction = transaction;
        //        selectCmd.CommandText = @"
        //            SELECT Id FROM BookCopies
        //            WHERE IdBook = $bookId AND Status = 'Available'
        //            LIMIT 1;
        //        ";
        //        selectCmd.Parameters.AddWithValue("$bookId", reservation.BookCopyId);

        //        var bookCopyId = selectCmd.ExecuteScalar() as long?;

        //        if (bookCopyId == null)
        //        {
        //            return false;
        //        }

        //        var updateBookAvailableCopies = conn.CreateCommand();
        //        updateBookAvailableCopies.CommandText = @"UPDATE Books SET AvailableCopies = AvailableCopies - 1 WHERE Id = $bookId;";
        //        updateBookAvailableCopies.Parameters.AddWithValue("$bookId", reservation.BookCopyId);
        //        updateBookAvailableCopies.ExecuteNonQuery();

        //        var updateCmd = conn.CreateCommand();
        //        updateCmd.Transaction = transaction;
        //        updateCmd.CommandText = @"
        //            UPDATE BookCopies
        //            SET Status = 'Reserved'
        //            WHERE Id = $id;";

        //        updateCmd.Parameters.AddWithValue("$id", bookCopyId.Value);
        //        updateCmd.ExecuteNonQuery();

        //        var insertCmd = conn.CreateCommand();
        //        insertCmd.CommandText = @"
        //            INSERT INTO Reservations (ReaderId, RespondedByLibrarianId,BookCopyId, CreatedAt, PickupDate, ReturnDate, Status, Fine)
        //            VALUES ($readerId, $librarianId,$bookCopyId, $createdAt, $pickupDate, $returnDate, $status, $Fine);
        //        ";
        //        insertCmd.Parameters.AddWithValue("$readerId",
        //            reservation.ReaderId.HasValue ? reservation.ReaderId.Value : DBNull.Value);
        //        insertCmd.Parameters.AddWithValue("$bookCopyId", bookCopyId.Value);
        //        insertCmd.Parameters.AddWithValue("$createdAt", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
        //        insertCmd.Parameters.AddWithValue("$pickupDate",reservation.PickUpAt.HasValue
        //            ? reservation.PickUpAt.Value.ToString("dd-MM-yyyy")
        //            : string.Empty);

        //        insertCmd.Parameters.AddWithValue(
        //            "$returnDate", reservation.ReturnDate.HasValue
        //                ? reservation.ReturnDate.Value.ToString("dd-MM-yyyy")
        //                : string.Empty
        //        );
        //        insertCmd.Parameters.AddWithValue("$Fine", 12);
        //        insertCmd.Parameters.AddWithValue("$status", reservation.Status);
        //        insertCmd.Parameters.AddWithValue("$librarianId", reservation.RespondedByLibrarianId);
        //        insertCmd.ExecuteNonQuery();
        //        transaction.Commit();
        //        return true;
        //    }
        //}
        public void AcceptReservation(Reservation reservation, int librarianId)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE Reservations SET Status = $status, RespondedByLibrarianId = $librarianId, ResponseDate = $responseDate WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", reservation.Id);
            cmd.Parameters.AddWithValue("$status", "Accepted");
            cmd.Parameters.AddWithValue("$librarianId", librarianId);
            cmd.Parameters.AddWithValue("$responseDate", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
            cmd.ExecuteNonQuery();
        }

        // Откланяем книгу
        public void RejectReservation(Reservation reservation, int librarianId)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            using var transaction = conn.BeginTransaction();

            try
            {
                // 1. Обновляем Reservation
                using var cmd = conn.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText = @"UPDATE Reservations 
                            SET Status = $status, RespondedByLibrarianId = $librarianId,
                                Reason = $reason, ResponseDate = $responseDate 
                            WHERE Id = $id";
                cmd.Parameters.AddWithValue("$id", reservation.Id);
                cmd.Parameters.AddWithValue("$status", "Rejected");
                cmd.Parameters.AddWithValue("$librarianId", librarianId);
                cmd.Parameters.AddWithValue("$reason", reservation.Reason);
                cmd.Parameters.AddWithValue("$responseDate", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
                cmd.ExecuteNonQuery();

                // 2. Освобождаем копию книги
                using var cmdCopies = conn.CreateCommand();
                cmdCopies.Transaction = transaction;
                cmdCopies.CommandText = @"UPDATE BookCopies SET Status = $status WHERE Id = $copyId";
                cmdCopies.Parameters.AddWithValue("$status", "Available");
                cmdCopies.Parameters.AddWithValue("$copyId", reservation.BookCopyId);
                cmdCopies.ExecuteNonQuery();

                // 3. Пересчитываем количество доступных копий
                int bookId = GetBookIdByCopyId(reservation.BookCopyId);
                using var updateBookAvailableCopies = conn.CreateCommand();
                updateBookAvailableCopies.Transaction = transaction;
                updateBookAvailableCopies.CommandText = @"UPDATE Books 
                                                  SET AvailableCopies = AvailableCopies + 1 
                                                  WHERE Id = $bookId;";
                updateBookAvailableCopies.Parameters.AddWithValue("$bookId", bookId);
                updateBookAvailableCopies.ExecuteNonQuery();

                // Подтверждаем транзакцию
                transaction.Commit();
            }
            catch
            {
                // В случае ошибки откатываем все изменения
                transaction.Rollback();
                throw;
            }
        }

        // Возвращаем книгу
        public OperationResult ProcessReturnBook(Reservation reservation, User reader, User librarian, Book book)
        {
            DateTime todayDate = DateTime.Now.Date;
            int overdueDays;
            decimal resultFine;

            if (reservation.ReturnDate.HasValue)
            {
                overdueDays = (todayDate - reservation.ReturnDate.Value.Date).Days;
                if (overdueDays < 0)
                    overdueDays = 0;

            }
            else
            {
                overdueDays = 0;
            }

            resultFine = overdueDays > 0 ? overdueDays * reservation.Fine : 0;
            resultFine = Math.Round(resultFine, 2);

            using var conn = _db.GetConnection();
            conn.Open();

            using var transaction = conn.BeginTransaction();

            // Проверка статуса бронирования
            using (var checkCmd = conn.CreateCommand())
            {
                checkCmd.CommandText = @"SELECT Status FROM Reservations WHERE Id = $id";
                checkCmd.Parameters.AddWithValue("$id", reservation.Id);
                checkCmd.Transaction = transaction;

                var status = checkCmd.ExecuteScalar()?.ToString();

                if (status != "Accepted")
                {
                    transaction.Rollback();
                    return new OperationResult
                    {
                        Success = false,
                        Message = $"Возврат невозможен: статус бронирования = {status}."
                    };
                }
            }

            // Обновление статуса копии книги
            using var bookCmd = conn.CreateCommand();
            bookCmd.CommandText = @"UPDATE BookCopies
                                SET Status = 'Available'
                                WHERE Id = $bookCopyId";
            bookCmd.Parameters.AddWithValue("$bookCopyId", reservation.BookCopyId);
            bookCmd.Transaction = transaction;
            bookCmd.ExecuteNonQuery();

            // Обновление статуса бронирования
            using var reservationCmd = conn.CreateCommand();
            reservationCmd.CommandText = @"UPDATE Reservations
                                       SET Status = 'Returned',
                                        ActualReturnDate = $today
                                        WHERE Id = $id";
            reservationCmd.Parameters.AddWithValue("$id", reservation.Id); // исправил на reservation.Id
            reservationCmd.Parameters.AddWithValue("$today", todayDate.ToString("dd-MM-yyyy"));
            reservationCmd.Transaction = transaction;
            reservationCmd.ExecuteNonQuery();

            // id книги
            using var getBookIdCmd = conn.CreateCommand();
            getBookIdCmd.CommandText = @"SELECT IdBook FROM BookCopies WHERE Id = $id";
            getBookIdCmd.Parameters.AddWithValue("$id", reservation.BookCopyId);
            getBookIdCmd.Transaction = transaction;
            int bookId = Convert.ToInt32(getBookIdCmd.ExecuteScalar());

            // Обновляем количество свободных копий книги
            int availableCopies = GetAvailableCopiesCountForBook(conn, transaction, bookId);
            using var updateBookAvailableCopies = conn.CreateCommand();
            updateBookAvailableCopies.CommandText = @"UPDATE Books 
                    SET AvailableCopies = $availableCopies  
                    WHERE Id = $bookId;";
            updateBookAvailableCopies.Parameters.AddWithValue("$bookId", bookId);
            updateBookAvailableCopies.Parameters.AddWithValue("$availableCopies", availableCopies);
            updateBookAvailableCopies.Transaction = transaction;
            updateBookAvailableCopies.ExecuteNonQuery();

            // Прибавляем штраф
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE Users
                            SET TotalFine = TotalFine + $fine
                            WHERE Id = $readerId";
            cmd.Parameters.AddWithValue("$fine", resultFine);
            cmd.Parameters.AddWithValue("$readerId", reservation.ReaderId);
            cmd.Transaction = transaction;
            cmd.ExecuteNonQuery();

            transaction.Commit();
            _wordDocumentService.CreateReturnContract(reservation, librarian, reader, book, overdueDays, resultFine, todayDate);
            return new OperationResult
            {
                Success = true,
                Message = $"Книга возвращена, штраф: {resultFine}."
            };
        }

        private int GetAvailableCopiesCountForBook(SqliteConnection conn, SqliteTransaction transaction, int BookId)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT COUNT(*) 
                        FROM BookCopies 
                        WHERE Status = 'Available' And IdBook = $idBook";
            cmd.Parameters.AddWithValue("$idBook", BookId);
            cmd.Transaction = transaction;
            return Convert.ToInt32(cmd.ExecuteScalar());
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
    }
}
