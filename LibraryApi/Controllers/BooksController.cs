using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _service;
    private readonly ILogger<BooksController> _logger;

    public BooksController(IBookService service, ILogger<BooksController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // GET /api/books
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var books = await _service.GetAllBooksAsync();
            return Ok(books);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all books");
            return StatusCode(500, "An error occurred while retrieving books.");
        }
    }

    // GET /api/books/OL45804W
    [HttpGet("{openLibraryId}")]
    public async Task<IActionResult> GetById(string openLibraryId)
    {
        try
        {
            var book = await _service.GetBookAsync(openLibraryId);
            return book is null
                ? NotFound($"Book '{openLibraryId}' not found.")
                : Ok(book);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving book {Id}", openLibraryId);
            return StatusCode(500, "An error occurred while retrieving the book.");
        }
    }

    // POST /api/books/fetch/OL45804W
    [HttpPost("fetch/{openLibraryId}")]
    public async Task<IActionResult> FetchFromApi(string openLibraryId)
    {
        try
        {
            var book = await _service.FetchAndSaveBookAsync(openLibraryId);
            return book is null
                ? NotFound($"Could not fetch '{openLibraryId}' from Open Library.")
                : CreatedAtAction(nameof(GetById), new { openLibraryId }, book);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching book {Id}", openLibraryId);
            return StatusCode(500, "An error occurred while fetching the book.");
        }
    }

    // PUT /api/books/OL45804W
    [HttpPut("{openLibraryId}")]
    public async Task<IActionResult> UpdateNotes(string openLibraryId, [FromBody] UpdateBookRequest request)
    {
        try
        {
            var updated = await _service.UpdateNotesAsync(openLibraryId, request.Notes);
            return updated ? NoContent() : NotFound($"Book '{openLibraryId}' not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating book {Id}", openLibraryId);
            return StatusCode(500, "An error occurred while updating the book.");
        }
    }

    // DELETE /api/books/OL45804W
    [HttpDelete("{openLibraryId}")]
    public async Task<IActionResult> Delete(string openLibraryId)
    {
        try
        {
            var deleted = await _service.DeleteBookAsync(openLibraryId);
            return deleted ? NoContent() : NotFound($"Book '{openLibraryId}' not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting book {Id}", openLibraryId);
            return StatusCode(500, "An error occurred while deleting the book.");
        }
    }
}