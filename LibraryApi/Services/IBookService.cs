using LibraryApi.Models;

namespace LibraryApi.Services
{
    public interface IBookService
    {
        Task<IEnumerable<Book>> GetAllBooksAsync();
        Task<Book?> GetBookAsync(string openLibraryId);
        Task<Book?> FetchAndSaveBookAsync(string openLibraryId);
        Task<bool> UpdateNotesAsync(string openLibraryId, string? notes);
        Task<bool> DeleteBookAsync(string openLibraryId);
    }
}
