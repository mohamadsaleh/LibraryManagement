using LibraryManagement.Endpoints.Books;

namespace LibraryManagement.Endpoints;

public static class BookExtensions
{
    public static void RegisterBookEndpoints(this WebApplication app)
    {
        app.MapGetBooks();
        app.MapGetBook();
        app.MapCreateBook();
        app.MapUpdateBook();
        app.MapDeleteBook();
    }
}