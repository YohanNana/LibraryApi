USE BookLibraryDB;
GO

DROP TABLE IF EXISTS Books;
GO

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
GO