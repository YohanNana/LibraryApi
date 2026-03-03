using LibraryApi.Data;
using LibraryApi.Models;
using Microsoft.Data.SqlClient;

namespace LibraryApi.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly DbConnectionFactory _connectionFactory;

        public BookRepository(DbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            var books = new List<Book>();
            using var conn = _connectionFactory.Create();
            var cmd = new SqlCommand("SELECT * FROM Books ORDER BY FetchedAt DESC", conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                books.Add(MapBook(reader));

            return books;
        }

        public async Task<Book?> GetByOpenLibraryIdAsync(string openLibraryId)
        {
            using var conn = _connectionFactory.Create();
            var cmd = new SqlCommand(
                "SELECT * FROM Books WHERE OpenLibraryId = @OpenLibraryId", conn);
            cmd.Parameters.AddWithValue("@OpenLibraryId", openLibraryId);
            using var reader = await cmd.ExecuteReaderAsync();

            return await reader.ReadAsync() ? MapBook(reader) : null;
        }

        public async Task<Book> InsertAsync(Book book)
        {
            using var conn = _connectionFactory.Create();
            var cmd = new SqlCommand(@"
            INSERT INTO Books 
                (OpenLibraryId, Title, Author, FirstPublished, CoverUrl, Description, Subjects, Notes)
            OUTPUT INSERTED.Id, INSERTED.FetchedAt
            VALUES 
                (@OpenLibraryId, @Title, @Author, @FirstPublished, @CoverUrl, @Description, @Subjects, @Notes)",
                conn);

            cmd.Parameters.AddWithValue("@OpenLibraryId", book.OpenLibraryId);
            cmd.Parameters.AddWithValue("@Title", book.Title);
            cmd.Parameters.AddWithValue("@Author", (object?)book.Author ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FirstPublished", (object?)book.FirstPublished ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CoverUrl", (object?)book.CoverUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Description", (object?)book.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Subjects", (object?)book.Subjects ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Notes", (object?)book.Notes ?? DBNull.Value);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                book.Id = reader.GetInt32(0);
                book.FetchedAt = reader.GetDateTime(1);
            }

            return book;
        }

        public async Task<bool> UpdateNotesAsync(string openLibraryId, string? notes)
        {
            using var conn = _connectionFactory.Create();
            var cmd = new SqlCommand(
                "UPDATE Books SET Notes = @Notes WHERE OpenLibraryId = @OpenLibraryId", conn);
            cmd.Parameters.AddWithValue("@Notes", (object?)notes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@OpenLibraryId", openLibraryId);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteAsync(string openLibraryId)
        {
            using var conn = _connectionFactory.Create();
            var cmd = new SqlCommand(
                "DELETE FROM Books WHERE OpenLibraryId = @OpenLibraryId", conn);
            cmd.Parameters.AddWithValue("@OpenLibraryId", openLibraryId);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        private static Book MapBook(SqlDataReader r) => new()
        {
            Id = r.GetInt32(r.GetOrdinal("Id")),
            OpenLibraryId = r.GetString(r.GetOrdinal("OpenLibraryId")),
            Title = r.GetString(r.GetOrdinal("Title")),
            Author = r.IsDBNull(r.GetOrdinal("Author")) ? null : r.GetString(r.GetOrdinal("Author")),
            FirstPublished = r.IsDBNull(r.GetOrdinal("FirstPublished")) ? null : r.GetInt32(r.GetOrdinal("FirstPublished")),
            CoverUrl = r.IsDBNull(r.GetOrdinal("CoverUrl")) ? null : r.GetString(r.GetOrdinal("CoverUrl")),
            Description = r.IsDBNull(r.GetOrdinal("Description")) ? null : r.GetString(r.GetOrdinal("Description")),
            Subjects = r.IsDBNull(r.GetOrdinal("Subjects")) ? null : r.GetString(r.GetOrdinal("Subjects")),
            Notes = r.IsDBNull(r.GetOrdinal("Notes")) ? null : r.GetString(r.GetOrdinal("Notes")),
            FetchedAt = r.GetDateTime(r.GetOrdinal("FetchedAt"))
        };
    }
}
