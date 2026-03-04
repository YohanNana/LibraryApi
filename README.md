# Book Library Manager API

A RESTful ASP.NET Core Web API that manages a personal book library by integrating 
with the [Open Library API](https://openlibrary.org/developers/api).

Books are fetched from Open Library on demand and cached locally in MS SQL Server.
Subsequent requests for the same book are served directly from the database.

---

## Tech Stack

| Layer | Technology | Reason |
|---|---|---|
| Framework | ASP.NET Core (.NET 8) | Mature, fast, cross-platform Web API framework |
| Database | MS SQL Server LocalDB | Lightweight local SQL Server, no separate install needed |
| DB Access | Microsoft.Data.SqlClient | Raw SQL queries |
| API Docs | Swashbuckle (Swagger) | Auto-generates interactive API documentation |
| HTTP Client | System.Net.Http.HttpClient | Built-in .NET HTTP client for calling Open Library |
| Serialization | System.Text.Json | Built-in, fast JSON parsing — no extra packages needed |

---

## Project Structure
```
BookLibrary/
├── LibraryApi/
│   ├── Controllers/
│   │   └── BooksController.cs      # API endpoints
│   ├── Services/
│   │   ├── IBookService.cs         # Service interface
│   │   └── BookService.cs          # Business logic + cache-first strategy
│   ├── Repositories/
│   │   ├── IBookRepository.cs      # Repository interface
│   │   └── BookRepository.cs       # Raw SQL queries
│   ├── Models/
│   │   └── Book.cs                 # Data model + request DTOs
│   ├── Data/
│   │   └── DbConnectionFactory.cs  # SQL connection helper
│   ├── appsettings.json            # Connection string config
│   └── Program.cs                  # App setup + DI registration
├── Database/
│   └── schema.sql                  # Database + table creation script
└── README.md
```

---

## Prerequisites

- [Visual Studio 2022] (with ASP.NET workload)
- [.NET 8 SDK]
- SQL Server LocalDB 

---

## Setup & Run Instructions

### 1. Clone the repository
```bash
git clone [https://github.com/YohanNana/LibraryApi.git]
cd LibraryApi
```

### 2. Set up the database

Open **SQL Server Object Explorer** in Visual Studio:
> View → SQL Server Object Explorer → expand `(localdb)\MSSQLLocalDB` → right-click → **New Query**

Paste and run the contents of `Database/schema.sql`:
```bash
# Or run via sqlcmd in terminal:
sqlcmd -S "(localdb)\MSSQLLocalDB" -i Database/schema.sql
```

### 3. Configure the connection string

Open `BookLibrary.Api/appsettings.json` — the default is already set for LocalDB:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=BookLibraryDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

> No API key needed — Open Library is completely free and open.

### 4. Restore NuGet packages
```bash
dotnet restore
```

### 5. Build the project
```bash
dotnet build
```

### 6. Run the API
```bash
cd LibraryApi
dotnet run
```

Swagger UI will open at:
```
https://localhost:{port}/swagger
```

---

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/books` | Get all saved books from DB |
| `GET` | `/api/books/{openLibraryId}` | Get one book — DB first, API fallback |
| `POST` | `/api/books/fetch/{openLibraryId}` | Force fetch from Open Library & save |
| `PUT` | `/api/books/{openLibraryId}` | Update personal notes on a book |
| `DELETE` | `/api/books/{openLibraryId}` | Remove a book from the database |

---

## Example Usage

### Fetch a book by Open Library ID
```bash
curl -X GET "https://localhost:{port}/api/books/OL27516W"
```

### Update notes on a book
```bash
curl -X PUT "https://localhost:{port}/api/books/OL27516W" \
  -H "Content-Type: application/json" \
  -d '{"notes": "A classic fantasy epic"}'
```

### Delete a book
```bash
curl -X DELETE "https://localhost:{port}/api/books/OL27516W"
```

---

## Sample Open Library IDs to Try

| Book | ID |
|------|----|
| The Lord of the Rings | `OL27516W` |
| Harry Potter & Sorcerer's Stone | `OL82586W` |
| The Great Gatsby | `OL468431W` |
| 1984 — George Orwell | `OL1168007W` |
| To Kill a Mockingbird | `OL2665882W` |

---

## Database Schema
```sql
CREATE TABLE Books (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    OpenLibraryId NVARCHAR(50) UNIQUE NOT NULL,
    Title         NVARCHAR(300) NOT NULL,
    Author        NVARCHAR(200),
    FirstPublished INT,
    CoverUrl      NVARCHAR(500),
    Description   NVARCHAR(MAX),
    Subjects      NVARCHAR(MAX),
    Notes         NVARCHAR(500),
    FetchedAt     DATETIME DEFAULT GETDATE()
);
```
