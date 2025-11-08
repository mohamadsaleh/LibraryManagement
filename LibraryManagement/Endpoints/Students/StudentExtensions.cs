using LibraryManagement.Endpoints.Students;

namespace LibraryManagement.Endpoints;

public static class StudentExtensions
{
    public static void RegisterStudentEndpoints(this WebApplication app)
    {
        app.MapGetStudents();
        app.MapGetStudent();
        app.MapCreateStudent();
        app.MapUpdateStudent();
        app.MapDeleteStudent();
    }
}