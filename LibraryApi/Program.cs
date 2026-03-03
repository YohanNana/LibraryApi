using LibraryApi.Data;
using LibraryApi.Repositories;
using LibraryApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register services
builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddHttpClient<BookService>();
builder.Services.AddScoped<IBookService, BookService>(sp =>
    new BookService(
        sp.GetRequiredService<IBookRepository>(),
        sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(BookService)),
        sp.GetRequiredService<ILogger<BookService>>()
    ));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
