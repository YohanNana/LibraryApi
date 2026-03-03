namespace LibraryApi.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string OpenLibraryId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public int? FirstPublished { get; set; }
        public string? CoverUrl { get; set; }
        public string? Description { get; set; }
        public string? Subjects { get; set; }
        public string? Notes { get; set; }
        public DateTime FetchedAt { get; set; }
    }

    public class UpdateBookRequest
    {
        public string? Notes { get; set; }
    }
}
