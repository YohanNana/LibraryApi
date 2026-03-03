CREATE DATABASE BookLibraryDB;
GO

USE BookLibraryDB;
GO

CREATE TABLE Books (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    OpenLibraryId NVARCHAR(50) UNIQUE NOT NULL,  -- e.g. "OL45804W"
    Title         NVARCHAR(300) NOT NULL,
    Author        NVARCHAR(200),
    FirstPublished INT,
    CoverUrl      NVARCHAR(500),
    Description   NVARCHAR(MAX),
    Subjects      NVARCHAR(MAX),                 -- comma-separated
    Notes         NVARCHAR(500),                 -- user-editable
    FetchedAt     DATETIME DEFAULT GETDATE()
);
GO