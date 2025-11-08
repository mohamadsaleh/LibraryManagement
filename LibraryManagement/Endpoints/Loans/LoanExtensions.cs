using LibraryManagement.Endpoints.Loans;

namespace LibraryManagement.Endpoints;

public static class LoanExtensions
{
    public static void RegisterLoanEndpoints(this WebApplication app)
    {
        app.MapGetLoans();
        app.MapBorrowBook();
        app.MapReturnBook();
        app.MapDeleteLoan();
    }
}