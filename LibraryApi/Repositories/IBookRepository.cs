using LibraryApi.Models;

namespace LibraryApi.Repositories
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllAsync();
        Task<Book?> GetByOpenLibraryIdAsync(string openLibraryId);
        Task<Book> InsertAsync(Book book);
        Task<bool> UpdateNotesAsync(string openLibraryId, string? notes);
        Task<bool> DeleteAsync(string openLibraryId);
    }
}
