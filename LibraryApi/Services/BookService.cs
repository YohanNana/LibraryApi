using System.Text.Json;
using LibraryApi.Models;
using LibraryApi.Repositories;

namespace LibraryApi.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _repository;
        private readonly HttpClient _httpClient;
        private readonly ILogger<BookService> _logger;

        public BookService(IBookRepository repository, HttpClient httpClient, ILogger<BookService> logger)
        {
            _repository = repository;
            _httpClient = httpClient;
            _logger = logger;
        }

        public Task<IEnumerable<Book>> GetAllBooksAsync() => _repository.GetAllAsync();

        // ✅ Cache-first: DB → Open Library API
        public async Task<Book?> GetBookAsync(string openLibraryId)
        {
            var cached = await _repository.GetByOpenLibraryIdAsync(openLibraryId);
            if (cached != null)
            {
                _logger.LogInformation("Cache hit for {Id}", openLibraryId);
                return cached;
            }

            _logger.LogInformation("Cache miss for {Id} — fetching from Open Library", openLibraryId);
            return await FetchAndSaveBookAsync(openLibraryId);
        }

        public async Task<Book?> FetchAndSaveBookAsync(string openLibraryId)
        {
            try
            {
                var url = $"https://openlibrary.org/works/{openLibraryId}.json";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Open Library returned {Status} for {Id}",
                        response.StatusCode, openLibraryId);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var book = new Book
                {
                    OpenLibraryId = openLibraryId,
                    Title = root.TryGetProperty("title", out var title)
                        ? title.GetString() ?? "Unknown" : "Unknown",
                    FirstPublished = root.TryGetProperty("first_publish_date", out var year)
                        ? ParseYear(year.GetString()) : null,
                    Description = root.TryGetProperty("description", out var desc)
                        ? (desc.ValueKind == JsonValueKind.String
                            ? desc.GetString()
                            : desc.TryGetProperty("value", out var v) ? v.GetString() : null)
                        : null,
                    Subjects = root.TryGetProperty("subjects", out var subjects)
                        ? string.Join(", ", subjects.EnumerateArray()
                            .Take(5)
                            .Select(s => s.GetString()))
                        : null,
                    CoverUrl = root.TryGetProperty("covers", out var covers)
                        && covers.GetArrayLength() > 0
                        ? $"https://covers.openlibrary.org/b/id/{covers[0].GetInt32()}-M.jpg"
                        : null,
                    Author = await FetchAuthorAsync(root)
                };

                // Save to DB only if it doesn't already exist
                var existing = await _repository.GetByOpenLibraryIdAsync(openLibraryId);
                if (existing != null) return existing;

                return await _repository.InsertAsync(book);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch book {Id} from Open Library", openLibraryId);
                return null;
            }
        }

        public Task<bool> UpdateNotesAsync(string openLibraryId, string? notes)
            => _repository.UpdateNotesAsync(openLibraryId, notes);

        public Task<bool> DeleteBookAsync(string openLibraryId)
            => _repository.DeleteAsync(openLibraryId);

        // Resolves /authors/OL123A.json → author name
        private async Task<string?> FetchAuthorAsync(JsonElement root)
        {
            try
            {
                if (!root.TryGetProperty("authors", out var authors)
                    || authors.GetArrayLength() == 0) return null;

                var authorRef = authors[0];
                string? authorKey = null;

                if (authorRef.TryGetProperty("author", out var authorObj)
                    && authorObj.TryGetProperty("key", out var key))
                    authorKey = key.GetString();
                else if (authorRef.TryGetProperty("key", out var directKey))
                    authorKey = directKey.GetString();

                if (authorKey == null) return null;

                var authorUrl = $"https://openlibrary.org{authorKey}.json";
                var res = await _httpClient.GetAsync(authorUrl);
                if (!res.IsSuccessStatusCode) return null;

                var authorJson = await res.Content.ReadAsStringAsync();
                var authorDoc = JsonDocument.Parse(authorJson);
                return authorDoc.RootElement.TryGetProperty("name", out var name)
                    ? name.GetString() : null;
            }
            catch
            {
                return null;
            }
        }

        private static int? ParseYear(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            var digits = new string(raw.Where(char.IsDigit).ToArray());
            return digits.Length >= 4 && int.TryParse(digits[..4], out var y) ? y : null;
        }
    }
}
